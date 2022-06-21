using Duende.IdentityServer.Models;

namespace MisOAuth.IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        { 
            new IdentityResources.OpenId()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
            {
            new ApiScope
            {
                Enabled=true,
                Name="misUserManagementApi",
                DisplayName= "MIS User Management API",
                Required=true,
                UserClaims =
                {
                    "role",
                    "name"
                }
            },
                new ApiScope(name: "api1", displayName: "MyAPI")
            };

    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource
            {
                Name="misUserManagementApi",
                DisplayName= "MIS User Management API",
                Enabled= true,
                Scopes =
                {
                    "misUserManagementApi"
                }
            }
        };

    public static IEnumerable<Client> Clients =>
         new List<Client>
    {
        new Client
        {
                Enabled=true,
                ClientId = "misUserManagementSwagger",
                ClientName = "MIS Swagger UI for User Management API",
                ClientSecrets = {new Secret("secret".Sha256())}, // change me!

                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,

                RedirectUris = {"https://localhost:7145/swagger/oauth2-redirect.html"},
                AllowedCorsOrigins = {"https://localhost:7145"},
                AllowedScopes = { "misUserManagementApi"}

        },


            new Client
            {
                Enabled=true,
                ClientId = "misUserManagementUI",
                ClientName = "misUserManagement UI",
                ClientUri = "https://localhost:3000",
                RequireConsent = false,
                RedirectUris =
                {
                    "https://localhost:3000/SignInCallBack",
                    "https://localhost:3000/SilentCallBack"
                },
                FrontChannelLogoutSessionRequired=true,
                FrontChannelLogoutUri = "https://localhost:3000/SignOutCallBack",
                PostLogoutRedirectUris =
                {
                    "https://localhost:3000/SignOutCallBack"
                },
                AllowedCorsOrigins =
                {
                    "https://localhost:3000"
                },


                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.Code,

                // secret for authentication
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                // scopes that client has access to
                AllowedScopes = { "openid", "misUserManagementApi"}
            },
    };
}