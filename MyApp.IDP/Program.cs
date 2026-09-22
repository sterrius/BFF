
using Duende.IdentityServer.Test;

namespace MyApp.IDP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "IdentityProvider";
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddRazorPages();

            var isBuilder = builder.Services.AddIdentityServer(configuration =>
            {
                //configuration.EmitStaticAudienceClaim = true;
            })
            .AddTestUsers(IdentityServerHost.Pages.TestUsers.Users);

            // in-memory, code config
            isBuilder.AddInMemoryIdentityResources(Config.IdentityResources);
            isBuilder.AddInMemoryApiResources(Config.ApiResources);
            isBuilder.AddInMemoryApiScopes(Config.ApiScopes);
            isBuilder.AddInMemoryClients(Config.Clients);

            // AddIdentityServer() calls these 2 methods internally, but we need to call them here to be explicit
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.MapRazorPages()
                .RequireAuthorization();

            app.Run();
        }
    }
}
