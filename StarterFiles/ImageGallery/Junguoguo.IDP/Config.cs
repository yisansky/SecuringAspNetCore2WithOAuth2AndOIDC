using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace Junguoguo.IDP
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                // 必须， 提供用户名密码等验证
                new IdentityResources.OpenId(),
                // 添加以支持返回 User 上的自定义属性 Claims， 如 CurrentAddr 等
                new IdentityResources.Profile()
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>()
            {
                new Client()
                {
                    ClientName = "Image Gallery",
                    ClientId = "junguoguoimagegalleryclient",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RedirectUris = new List<string>()
                    {
                        "https://localhost:44314/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>()
                    {
                        "https://localhost:44314/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    },
                    ClientSecrets =
                    {
                        new Secret("junguoguosecret".Sha256())
                    }
                }
            };
        }

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                // 对应到 Images 表初始化数据时用到的两个用户 Claire 和 Frank， SubjectId 取表中的 OwnerId 字段
                // Claims 里面的信息可以随自己爱好添加，相当于给 User 增加自定义的属性和值
                new TestUser
                {
                    SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                    Username = "Frank",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim("name", "Frank Hawk"),
                        new Claim("family_name", "Hawk"),
                        new Claim("given_name", "Frank"),
                        new Claim("website", "https://frank.com"),
                        new Claim("CurrentAddr","USA. LA")
                    }
                },
                new TestUser
                {
                    SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                    Username = "Claire",
                    Password = "pwd123",
                    Claims = new List<Claim>
                    {
                        new Claim("name", "Claire Underwood"),
                        new Claim("website", "https://claire.com"),
                        new Claim("CurrentAddr","USA. Tex")
                    }
                },
                // 自己的测试用户，SubjectId 用 guid 生成一个
                new TestUser
                {
                    SubjectId = "4bc6cfb7-6606-dab2-8bd3-07f779b3fd8c",
                    Username = "Junguoguo",
                    Password = "pwd123",
                    Claims = new List<Claim>
                    {
                        new Claim("name", "Alex Mercer"),
                        new Claim("website", "https://junguoguo.com"),
                        new Claim("CurrentAddr","CHN. Heaven")
                    }
                }
            };
        }
    }
}
