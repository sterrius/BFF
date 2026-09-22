using Duende.IdentityServer.Models;

namespace MyApp.IDP
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()//,
            //new IdentityResource("roles", new[] { "role" })
        ];

        public static IEnumerable<ApiScope> ApiScopes =>
            [
                new ApiScope("myapp.api")
            ];

        public static IEnumerable<ApiResource> ApiResources =>
            [
                new ApiResource("MyApp", "MyApp API")
                {
                    Scopes = { "myapp.api" }
                }
            ];

        public static IEnumerable<Client> Clients =>
            [
                new Client // interactive client using code flow + pkce
                {
                    ClientId = "MyApp",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,

                    RedirectUris = { "https://localhost:7180/signin-oidc" },
                    //FrontChannelLogoutUri = "https://localhost:7180/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:7180/signout-callback-oidc" },

                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes = { "openid", "profile", "myapp.api" },
                    RequireConsent = true,
                }
            ];
    }
}
