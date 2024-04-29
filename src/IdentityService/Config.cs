using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[] { new IdentityResources.OpenId(), new IdentityResources.Profile(), };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[] { new ApiScope("auctionApp", "Auction app full access"), };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // temporary client with not ideal security, used until UI is written
            new Client
            {
                ClientId = "postman",
                ClientName = "Postman",
                AllowedScopes = { "openid", "profile", "auctionApp" },
                RedirectUris = { "https://www.getpostman.com/oauth2/callback" }, // we won't use this anyway, for Postman
                ClientSecrets = new[] { new Secret("NotASecret".Sha256()) }, // we will use this in Postman
                AllowedGrantTypes = { GrantType.ResourceOwnerPassword },
            },
            new Client
            {
                ClientId = "nextApp",
                ClientName = "nextApp",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                RequirePkce = false,
                RedirectUris = { "http://localhost:3000/api/auth/callback/id-server" },
                AllowOfflineAccess = true, // allow refresh token
                AllowedScopes = { "openid", "profile", "auctionApp" },
                AccessTokenLifetime = 3600 * 24 * 30 // lasts for 30 days - for dev purposes only
            }
        };
}
