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
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource("guoguoextrainfo", "Guo's extra memo", new List<string>(){"extra"}),
                new IdentityResource("roles", "Your role(s)", new List<string>(){"role"}),
                new IdentityResource("country","Country where the user lives in", new List<string>(){"country"}),
                new IdentityResource("subscriptionlevel","User's subscription level",new List<string>(){"subscriptionlevel"})
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("junguoguoAPI","Jun's Image Gallery API")
                {
                    UserClaims = new List<string>(){"role", "subscriptionlevel","country" }
                }
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
                    //以下 token 的时间单位都是秒
                    IdentityTokenLifetime = 5 * 60, // 默认就是五分钟，所以这里也可以注释掉
                    AuthorizationCodeLifetime = 5 * 60, // 默认五分钟
                    AccessTokenLifetime = 2 * 60, // 默认 一小时。因为 IDP 有自己的刷新机制（5分钟间隔），所以这个 token 实际过期的时间也许比两分钟长

                    AllowOfflineAccess = true, // 启用 'offline_access' scope
                    AbsoluteRefreshTokenLifetime = 30 * 24 * 60 * 60, // refresh token 的绝对过期时间，默认30天
                    RefreshTokenExpiration = TokenExpiration.Sliding, // refresh token 一般不需要设置绝对的过期时间，设置成 sliding 模式就好
                    SlidingRefreshTokenLifetime = 5 * 60,//  sliding 模式下，当请求新的 refresh token时，他的过期时间会被重置为这里设置的值(但不会超过 AbsoluteRefreshTokenLifetime 的设置)
                    UpdateAccessTokenClaimsOnRefresh = true, // refresh token 请求是否更新 access token 里面携带的 user claim 信息；设为 true， 即使 token 没有过期，也会更新 accesstoken 的 claim 值

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
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "guoguoextrainfo",
                        "junguoguoAPI",
                        "subscriptionlevel",
                        "country"
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
                    SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",
                    Username = "Frank",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim("name", "Frank Hawk"),
                        new Claim("family_name", "Hawk"),
                        new Claim("given_name", "Frank"),
                        new Claim("profile", "https://frank.com"),
                        new Claim("address","USA. LA"),
                        new Claim("role","FreeUser"),
                        new Claim("extra","Frank 才14岁就上大学了，厉害"),
                        new Claim("country","BK"),
                        new Claim("subscriptionlevel","vip1")
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
                        new Claim("profile", "https://claire.com"),
                        new Claim("address","USA. Tex"),
                        new Claim("role","PayingUser"),
                        new Claim("extra","Claire is almost a godess"),
                        new Claim("country","CHN"),
                        new Claim("subscriptionlevel","supervip")
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
                        new Claim("profile", "https://junguoguo.com"),
                        new Claim("address","CHN. Heaven"),
                        new Claim("role","admin"),
                        new Claim("extra","俊果果是网站管理员"),
                        new Claim("country","HEAVEN"),
                        new Claim("subscriptionlevel","supervip")
                    }
                }
            };
        }
    }
}
