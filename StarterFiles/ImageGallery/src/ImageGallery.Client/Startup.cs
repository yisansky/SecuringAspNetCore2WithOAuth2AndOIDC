using System;
using System.IdentityModel.Tokens.Jwt;
using IdentityModel;
using ImageGallery.Client.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Profiling.Storage;

namespace ImageGallery.Client
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            // 清除默认的 Claim Maper
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }
 
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.AddPolicy(
                    "CanOrderFrame",
                    policyBuilder =>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.RequireClaim("country", "Heaven", "CHN");
                        policyBuilder.RequireClaim("subscriptionlevel", "supervip");
                    });
            });
            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IImageGalleryHttpClient
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();

            var connectionString = Configuration["ConnectionStrings:imageGalleryDBConnectionString"];
            services.AddDbContext<GalleryContext>(o => o.UseSqlServer(connectionString));

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                }).AddCookie("Cookies",
                    (options) =>
                    {
                        // 用户是否有权限访问页面，信息来源是Cookie，所以在这里加
                        options.AccessDeniedPath = "/Authorization/AccessDenied";
                    })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";
                    // IDP server 的url
                    options.Authority = "https://localhost:44319/";
                    options.ClientId = "junguoguoimagegalleryclient";
                    // specify use hybrid mode
                    options.ResponseType = "code id_token";
                    options.CallbackPath = new PathString("/signin-oidc");
                    options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("address");
                    options.Scope.Add("roles");
                    options.Scope.Add("guoguoextrainfo");
                    options.Scope.Add("junguoguoAPI");
                    options.Scope.Add("country");
                    options.Scope.Add("subscriptionlevel");
                    options.Scope.Add("offline_access");
                    options.SaveTokens = true;
                    options.ClientSecret = "junguoguosecret";
                    options.GetClaimsFromUserInfoEndpoint = true;
                    // 从 filter 中移除 amr， 代表需要 get 【amr】 的值
                    options.ClaimActions.Remove("amr");
                    // 下面两个无效，这种写法拿不出来
                    options.ClaimActions.Remove("website");
                    options.ClaimActions.Remove("CurrentAddr");
                    // 不需要 sid 和 idp 的值
                    options.ClaimActions.DeleteClaim("sid");
                    options.ClaimActions.DeleteClaim("idp");
                    options.ClaimActions.MapUniqueJsonKey("address", "address");
                    options.ClaimActions.MapUniqueJsonKey("role", "role");
                    options.ClaimActions.MapUniqueJsonKey("extra", "extra");
                    options.ClaimActions.MapUniqueJsonKey("subscriptionlevel", "subscriptionlevel");
                    options.ClaimActions.MapUniqueJsonKey("country", "country");

                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        NameClaimType = JwtClaimTypes.GivenName,
                        RoleClaimType = "role" 
                    };
                });

            // Note .AddMiniProfiler() returns a IMiniProfilerBuilder for easy intellisense
            services.AddMiniProfiler(options =>
            {
                // All of this is optional. You can simply call .AddMiniProfiler() for all defaults

                // (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
                options.RouteBasePath = "/profiler";

                // (Optional) Control storage
                // (default is 30 minutes in MemoryCacheStorage)
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();

                // (Optional) To control authorization, you can use the Func<HttpRequest, bool> options:
                // (default is everyone can access profilers)
                //options.ResultsAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
                //options.ResultsListAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;

                // (Optional)  To control which requests are profiled, use the Func<HttpRequest, bool> option:
                // (default is everything should be profiled)
                //options.ShouldProfile = request => MyShouldThisBeProfiledFunction(request);

                // (Optional) Profiles are stored under a user ID, function to get it:
                // (default is null, since above methods don't use it by default)
                //options.UserIdProvider = request => MyGetUserIdFunction(request);

                // (Optional) Swap out the entire profiler provider, if you want
                // (default handles async and works fine for almost all appliations)
                // options.ProfilerProvider = new MyProfilerProvider();

                // (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
                // (defaults to true, and connection opening/closing is tracked)
                options.TrackConnectionOpenClose = true;
            });

            services.AddMiniProfiler()
                .AddEntityFramework();

            services.AddScoped<GalleryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            // 需要放在 UseMvc 前
            app.UseAuthentication();

            app.UseStaticFiles();

            // ...existing configuration...
            app.UseMiniProfiler();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
