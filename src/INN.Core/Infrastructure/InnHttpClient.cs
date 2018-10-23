using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EPiServer.Logging;
using Newtonsoft.Json;

namespace INN.Core.Infrastructure
{
    public class InnHttpClient : HttpClient
    {
        private readonly ILogger _log = LogManager.GetLogger(typeof(InnHttpClient));

        public static InnHttpClient Instance = new InnHttpClient();

        public InnHttpClient() : base(new HttpClientHandler
        {
        })
        {
            DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
        }

        public async Task<HttpResponseMessage> PostAsync(Uri uri, FormUrlEncodedContent content)
        {
            _log.Information($"Request {uri} {JsonConvert.SerializeObject(content)}");

            var response = await base.PostAsync(uri, content);

            var textResult = await response.Content.ReadAsStringAsync();

            _log.Information($"Result {uri} {textResult}");

            return response;
        }


    }
}
