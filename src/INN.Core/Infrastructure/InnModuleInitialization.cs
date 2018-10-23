using System;
using System.Net;
using System.Net.Http;
using System.Web;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using INN.Core.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;

namespace INN.Core.Infrastructure
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    internal class InnModuleInitialization : IConfigurableModule
    {
        private static bool _initialized;

        public void Initialize(InitializationEngine context)
        {
            if(_initialized)
            {
                return;
            }
            _initialized = true;
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddTransient(locator => GetInstance<IAddressApi>(InnSettings.SingleSignOnServiceUri));
            context.Services.AddTransient(locator => GetInstance<ITokenApi>(InnSettings.TokenServiceUri));
            context.Services.AddHttpContextOrThreadScoped<IInnUserTokenCache>(
                locator => new CookieTokenCache(new HttpContextWrapper(HttpContext.Current)));
        }

        public void Uninitialize(InitializationEngine context)
        {

        }

        public T GetInstance<T>(string uri)
        {
            var handler = new HttpClientHandler
            {
            };
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(uri)
            };
            var api = RestService.For<T>(httpClient, new RefitSettings
            {
                JsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            });
            return api;
        }
    }
}
