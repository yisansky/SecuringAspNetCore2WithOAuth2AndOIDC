using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ImageGallery.Client.Services
{
    public class ImageGalleryHttpClient : IImageGalleryHttpClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpClient _httpClient = new HttpClient();

        public ImageGalleryHttpClient(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<HttpClient> GetClient()
        {
            var accessToken = string.Empty;
            // get the current HttpContext to access the tokens
            var currentContext = _httpContextAccessor.HttpContext;
            
            var expires_at = await currentContext.GetTokenAsync("expires_at");
            if (string.IsNullOrWhiteSpace(expires_at)
                || DateTime.Parse(expires_at).AddSeconds(-60).ToUniversalTime() < DateTime.UtcNow)
            {
                // token 过期时间未设置，或者当前距离过期时间小于 60s
                accessToken = await RenewTokens();
            }
            else
            {
                // get access token from currentContext
                accessToken = await currentContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            }

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                // set as Bearer token
                _httpClient.SetBearerToken(accessToken);
            }

            _httpClient.BaseAddress = new Uri("https://localhost:44363/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return _httpClient;
        }

        private async Task<string> RenewTokens()
        {
            // get the current HttpContext to access the tokens
            var currentContext = _httpContextAccessor.HttpContext;

            // get metadata
            var discoveryClient = new DiscoveryClient("https://localhost:44319/");
            var metaDataResponse = await discoveryClient.GetAsync(); // 结果包含 token endpoint 的 uri

            // create a new token client to get new tokens
            var tokenClient = new TokenClient(metaDataResponse.TokenEndpoint, "junguoguoimagegalleryclient", "junguoguosecret");

            // get the saved refresh token
            var currentRefreshToken = await currentContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            // refresh the tokens
            var tokenResult = await tokenClient.RequestRefreshTokenAsync(currentRefreshToken);

            if (!tokenResult.IsError)
            {
                // update the tokens & expiration value
                // 将新返回的 accesstoken， idtoken， refreshtoken 存下来
                var updatedTokens = new List<AuthenticationToken>();
                updatedTokens.Add(new AuthenticationToken()
                {
                    Name = OpenIdConnectParameterNames.IdToken,
                    Value = tokenResult.IdentityToken
                });
                updatedTokens.Add(new AuthenticationToken()
                {
                    Name = OpenIdConnectParameterNames.AccessToken,
                    Value = tokenResult.AccessToken
                });
                updatedTokens.Add(new AuthenticationToken()
                {
                    Name = OpenIdConnectParameterNames.RefreshToken,
                    Value = tokenResult.RefreshToken
                });

                // 存储更新的 access token 过期时间
                var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
                updatedTokens.Add(new AuthenticationToken()
                {
                    Name = "expires_at",
                    Value = expiresAt.ToString("o",CultureInfo.InvariantCulture) // 格式类似："2018-08-26T02:42:05.8636746Z"
                });

                // get autenticate result, containing the current principal & properties
                // 获取 cookie 并更新
                var currentAuthenticationResult = await currentContext.AuthenticateAsync("Cookies");
                // store the updated tokens
                currentAuthenticationResult.Properties.StoreTokens(updatedTokens);

                // sign in
                await currentContext.SignInAsync("Cookies", currentAuthenticationResult.Principal,
                    currentAuthenticationResult.Properties);
                // return the new access token
                return tokenResult.AccessToken;
            }
            else
            {
                //throw new Exception("Problem encountered while refreshing tokens", tokenResult.Exception);
                return ""; // 不抛异常而是直接返回空值，会提示 AccessDenied 页面
            }
        }
    }
}

