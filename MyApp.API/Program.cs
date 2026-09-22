
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MyApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddAuthentication(options =>
             {
                 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
             })
            .AddJwtBearer(options =>
            {
                // The issuer in your token: "iss": "https://localhost:5001"
                options.Authority = "https://localhost:5001";

                // The audience/client_id in your token: "client_id": "MyApp"
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "https://localhost:5001",

                    ValidateAudience = true,
                    ValidAudience = "MyApp",

                    //ValidateLifetime = true,
                    //ClockSkew = TimeSpan.Zero, // optional, for strict expiry

                    //ValidateIssuerSigningKey = true
                    // If you’re not using Authority/metadata, you can set IssuerSigningKey(s) manually:
                    // IssuerSigningKey = new RsaSecurityKey(yourRsaPublicKey)
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClaim("scope", "myapp.api") // Ensure the token has the required scope
                    .Build();
            });

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
