using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using INN.Core;
using INN.Core.Api;
using INN.Core.Models.DTO;
using INN.Core.Services.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;
using Xunit;

namespace INN.Test.Integration.Services
{
    public static class TestHelper
    {
        public static RefitSettings GetRefitSettings()
        {
            return new RefitSettings
            {
                JsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };
        }

        public static async Task<ApplicationToken> GetApplicationToken()
        {
            var tokenService = new InnTokenService(new ApplicationTokenMemoryCache(),
                CreateApi<ITokenApi>(InnSettings.TokenServiceUri));
            return await tokenService.GetApplicationToken();
        }

        public static async Task<string> GetUserTokenId(string phoneNumber, string password)
        {
            var tokenService = new InnTokenService(new ApplicationTokenMemoryCache(), CreateApi<ITokenApi>(InnSettings.TokenServiceUri));
            var innAccountService = new InnAccountService(tokenService);

            var loginResponse = await innAccountService.LoginOwnPassword(phoneNumber, password);
            Assert.True(loginResponse.IsSuccess);

            return loginResponse.UserTokenId;
        }

        public static T CreateApi<T>(string uri)
        {
            var handler = new HttpClientHandler
            {
#if DEBUG
                UseProxy = true,
                Proxy = new WebProxy("localhost", 8888)
#endif
            };
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(uri)
            };
            var addressApi = RestService.For<T>(httpClient, GetRefitSettings());
            return addressApi;
        }
    }
}
