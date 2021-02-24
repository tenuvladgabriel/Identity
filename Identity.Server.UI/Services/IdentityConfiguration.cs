using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Identity.Server.UI.Services
{
    public static class IdentityConfiguration
    {
        private static readonly IConfiguration Configuration = Startup.StaticConfiguration;
        private static readonly string MinatoKey = Configuration["SecretKeys:MinatoSecret"];
        private static readonly string IdentityKey = Configuration["SecretKeys:IdentitySecret"];
        private static readonly string MinatoUrl = Configuration["ApplicationUrls:MinatoUri"];

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile(),
                new IdentityResources.Phone(),
                new IdentityResources.Address()
            };
        }
        
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope
                {
                    Name = "minato-api",
                    Enabled = true
                }
            };
        }
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource
                {
                    Name = "minato",
                    ApiSecrets = {new Secret(MinatoKey.Sha256())},
                    Scopes = {"minato-api"}
                }
            };
        }
        
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "user@mailinator.com",
                    Password = "user",
                    Claims = new[]
                    {
                        new Claim("roleType", "User")
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "admin@mailinator.com",
                    Password = "admin",
                    Claims = new[]
                    {
                        new Claim("roleType", "Admin")
                    }
                }
            };
        }

        public static List<IdentityRole> GetRoles()
        {
            return new List<IdentityRole>
            {
                new IdentityRole
                {
                    Name = "Admin",
                    Id = Guid.NewGuid().ToString()
                },
                new IdentityRole
                {
                    Name = "User",
                    Id = Guid.NewGuid().ToString()
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "minato-ui",
                    AllowedGrantTypes = GrantTypes.Code,
                    ClientSecrets = {new Secret(MinatoKey.Sha256())},
                    RequireClientSecret = true,
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = {MinatoUrl + "/index.html"},
                    PostLogoutRedirectUris = {MinatoUrl + "/index.html"},
                    AccessTokenLifetime = 60 * 60 * 24,
                    AllowedCorsOrigins = {MinatoUrl},
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "minato-api"
                    }
                },
                new Client
                {
                    ClientId = "swagger-minato",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    ClientSecrets = {new Secret(MinatoKey.Sha256())},
                    AllowedScopes = {"minato-api"},
                },
                new Client
                {
                    ClientId = "identity-ui",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    AllowOfflineAccess = true,
                    ClientSecrets = { new Secret(IdentityKey.Sha256()) },
                    RedirectUris = { "https://localhost:5001/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:5001/" },
                    FrontChannelLogoutUri = "https://localhost:5001/signout-oidc",
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,

                        "minato-api"
                    }
                }
            };
        }
    }
}