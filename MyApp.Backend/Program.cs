using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace MyApp.Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // TO THINK ABOUT NOW
        // HOW LONG DOES THE COOKIE REMAIN VALID? HOW LONG DOES THE TOKEN REMAIN VALID? CAN I SET THEM TO BE THE SAME?
        // REFRESH TOKEN SO THE USER ISN'T LOGGED OUT
        // ADD A LOGOUT BUTTON TO THE ANGULAR APP
        // MAKE THE ANGULAR APP FETCH SOME DATA FROM THE API AND DISPLAY IT ON THE PAGE. THIS WILL DEMONSTRATE THAT THE TOKEN IS BEING PASSED TO THE API AND THE USER IS AUTHENTICATED.

        //builder.Services.AddAntiforgery(options =>
        //{
        //    options.HeaderName = "X-CSRF";
        //});

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
            .AddTransforms(transformBuilderContext =>
            {
                //transformBuilderContext.AddRequestTransform(async transformContext =>
                //{
                //var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
                //if (!string.IsNullOrEmpty(accessToken))
                //{
                //    transformContext.ProxyRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                //}
                //else
                //{                         // If the access token is null or empty, return a 401 Unauthorized response
                //    transformContext.HttpContext.Response.Redirect("/login");
                //    //transformContext.HttpContext.Response.StatusCode = 401;
                //    //await transformContext.HttpContext.Response.WriteAsync("Unauthorized: Access token is missing or expired.");
                //    //return;
                //}

                // --------------------------------------------------------------------------------------------------------------------------------------
                //});

                // Only apply this transform to routes that require auth
                //if (string.Equals("Default", transformBuilderContext.Route.AuthorizationPolicy))
                //{
                //    transformBuilderContext.AddRequestTransform(async transformContext =>
                //    {
                //        //// Authentication has already run before this point
                //        //var ticket = await transformContext.HttpContext.AuthenticateAsync(
                //        //    CookieAuthenticationDefaults.AuthenticationScheme);

                //        //var tokenService = transformContext.HttpContext.RequestServices
                //        //    .GetRequiredService<TokenService>();

                //        //// Get the access token for the downstream API
                //        //var token = await tokenService.GetAuthTokenAsync(ticket.Principal);

                //        var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");

                //        if (string.IsNullOrEmpty(accessToken))
                //        {
                //            transformContext.HttpContext.Response.StatusCode = 401;
                //            return;
                //        }

                //        transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                //    });
                //}
            });

        //});
        //});

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "__Host-MyAppCookie";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.Path = "/";
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = "https://localhost:5001";
            options.ClientId = "MyApp";
            options.ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0";
            options.ResponseType = "code";
            options.Scope.Add("myapp.api");
            options.SaveTokens = true;
            //options.GetClaimsFromUserInfoEndpoint = true;

            options.SignedOutCallbackPath = "/signout-callback-oidc";
            options.SignedOutRedirectUri = "/goodbye";
        });

        var app = builder.Build();

        // Use the permissive CORS policy so requests from any origin are allowed
        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapReverseProxy();

        app.MapGet("/protected", [Authorize] () => "Hello Authorised World!"); // PROTECTED WITH COOKIE AUTHENTICATION - CAN ACCESS IF TOKEN IS NULL BUT COOKIE PRESENT
                                                                               // NEED TO CHECK FOR THE SCENARIO WHERE THE COOKIE IS PRESENT BUT THE TOKEN IS NULL AND GET THE USER TO LOGIN AGAIN TO GET A NEW TOKEN. THIS IS THE CASE WHEN THE COOKIE IS STILL VALID BUT THE TOKEN HAS EXPIRED.


        app.MapGet("/login", async (HttpContext context) =>
        {
            await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = "/"
            });
        });

        app.MapGet("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        });

        app.MapGet("/signout-callback-oidc", () => Results.Ok());
        app.MapGet("/goodbye", () => Results.Content("<html><body><p>Goodbye!</p></body></html>", "text/html"));

        app.Run();
    }
}
