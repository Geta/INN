using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using INN.Core.Api;
using INN.Core.Infrastructure;
using INN.Core.Models.DTO;
using INN.Core.Models.Request;
using Polly;
using Polly.Retry;
using Refit;

namespace INN.Core.Services.Auth
{
    [ServiceConfiguration(typeof(IInnTokenService))]
    public class InnTokenService : IInnTokenService
    {
        private readonly ILogger _log = LogManager.GetLogger(typeof(InnTokenService));
        private readonly IApplicationTokenCache _applicationTokenCache;
        private readonly ITokenApi _tokenApi;
        private readonly RetryPolicy _retryPolicy;

        public InnTokenService(
            IApplicationTokenCache applicationTokenCache,
            ITokenApi tokenApi)
        {
            _applicationTokenCache = applicationTokenCache;
            _tokenApi = tokenApi;

            // Try to refresh application token
            _retryPolicy = Policy
                .Handle<ApiException>(apiException => apiException.StatusCode == HttpStatusCode.Forbidden &&
                                                      apiException.GetContentAs<InnStsException>()?.Code ==
                                                      InnStsErrorCode.InvalidApplication)
                .RetryAsync(1, (e, i) => RefreshAuthorization());
        }

        public async Task<string> GetApplicationTokenId()
        {
            return (await GetApplicationToken().ConfigureAwait(false)).ApplicationTokenId;
        }

        public async Task<ApplicationToken> GetApplicationToken()
        {
            var cachedToken = _applicationTokenCache.Get();
            if(cachedToken != null && !cachedToken.Equals(ApplicationToken.EmptyApplicationToken))
            {
                return cachedToken;
            }

            return await RefreshApplicationToken();
        }

        public async Task<ApplicationToken> RefreshApplicationToken()
        {
            var newToken = await GetApplicationTokenResponse().ConfigureAwait(false);
            _applicationTokenCache.Set(newToken);
            return newToken;
        }

        public async Task<bool> ValidateUsertokenId(string userTokenId)
        {
            if(string.IsNullOrWhiteSpace(userTokenId))
            {
                return false;
            }

            try
            {
                var response = await Execute(async () =>
                {
                    var applicationTokenId = await GetApplicationTokenId().ConfigureAwait(false);
                    return await _tokenApi.ValidateUserTokenId(applicationTokenId, userTokenId);
                });

                return response.Result;
            }
            catch(ApiException ex)
            {
                _log.Information("ValidateUsertokenId failed", ex);
                return false;
            }
        }

        public async Task<UserToken> GetUserToken(string userTicket)
        {
            var client = InnHttpClient.Instance;
            var applicationTokenId = await GetApplicationTokenId();
            var uri = new Uri($"{InnSettings.TokenServiceUri}/user/{applicationTokenId}/get_usertoken_by_userticket");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("apptoken", await ApplicationTokenXmlAsync()),
                new KeyValuePair<string, string>("userticket", userTicket)
            });
            var response = await client.PostAsync(uri, content);
            var textResult = await response.Content.ReadAsStringAsync();
            _log.Information("Response: " + textResult);

            if(string.IsNullOrWhiteSpace(textResult) || response.StatusCode != HttpStatusCode.OK)
            {
                return UserToken.EmptyToken;
            }
            else
            {
                var result = new UserToken();

                var doc = new XmlDocument();
                doc.LoadXml(textResult);
                var userTokenNode = doc.SelectSingleNode("/usertoken");
                if(userTokenNode != null)
                {
                    result.UserTokenId = userTokenNode.Attributes?["id"]?.Value;
                    result.Username = userTokenNode.SelectSingleNode("username")?.InnerText;
                    result.Firstname = userTokenNode.SelectSingleNode("firstname")?.InnerText;
                    result.Lastname = userTokenNode.SelectSingleNode("lastname")?.InnerText;
                    result.Cellphone = userTokenNode.SelectSingleNode("cellphone")?.InnerText;
                    result.Email = userTokenNode.SelectSingleNode("email")?.InnerText;
                    if(result.Lastname?.StartsWith("Demographics:") ?? true)
                        result.Anonymous = true;
                    if(result.Username?.Equals("anonymous") ?? true)
                        result.Anonymous = true;
                }

                // We will override INN return codes if userTokenId is invalid
                if(string.IsNullOrWhiteSpace(result.UserTokenId))
                {
                    return UserToken.EmptyToken;
                }

                return result;
            }
        }

        /// <summary>
        /// We can registrer a ticket, that can later be used as a ticket to get a token. 
        /// The ticket is only valid for one call
        /// </summary>
        /// <param name="userTokenId"></param>
        /// <returns></returns>
        public async Task<string> GetUserTicketByUserTokenId(string userTokenId)
        {
            if(string.IsNullOrWhiteSpace(userTokenId))
            {
                throw new ArgumentNullException(nameof(userTokenId));
            }

            var client = InnHttpClient.Instance;
            var applicationTokenId = await GetApplicationTokenId();
            var uri = new Uri($"{InnSettings.TokenServiceUri}/user/{applicationTokenId}/create_userticket_by_usertokenid");

            var userticket = Guid.NewGuid().ToString().Replace("-", "");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("apptoken", await ApplicationTokenXmlAsync()),
                new KeyValuePair<string, string>("userticket", userticket),
                new KeyValuePair<string, string>("usertokenid", userTokenId)
            });
            var response = await client.PostAsync(uri, content);
            var textResult = await response.Content.ReadAsStringAsync();

            return string.IsNullOrWhiteSpace(textResult) ? string.Empty : userticket;
        }

        protected virtual async Task<ApplicationToken> GetApplicationTokenResponse()
        {
            var request = new ApplicationCredentialRequest(InnSettings.ApplicationId,
                InnSettings.ApplicationSecret,
                InnSettings.ApplicationName);
            var resultString = await _tokenApi.LoginApp(request).ConfigureAwait(false);
            if(string.IsNullOrWhiteSpace(resultString))
            {
                return ApplicationToken.EmptyApplicationToken;
            }
            var serializer = new XmlSerializer(typeof(applicationtoken));
            applicationtoken applicationtoken;
            using(TextReader reader = new StringReader(resultString))
            {
                applicationtoken = (applicationtoken)serializer.Deserialize(reader);
            }

            return new ApplicationToken
            {
                ApplicationTokenId = applicationtoken.@params.applicationtokenID,
                ApplicationName = applicationtoken.@params.applicationname,
                ApplicationId = applicationtoken.@params.applicationid,
                Expires = applicationtoken.@params.expires
            };
        }

        protected async Task<T> Execute<T>(Func<Task<T>> func)
        {
            return await _retryPolicy
                .ExecuteAsync(func)
                .ConfigureAwait(false);
        }

        private async Task<string> ApplicationTokenXmlAsync()
        {
            var applicationToken = await GetApplicationToken();
            var applicationId = InnSettings.ApplicationId;
            return $"<applicationtoken><params><applicationtokenID>{applicationToken.ApplicationTokenId}</applicationtokenID><applicationid>{applicationId}</applicationid><applicationname>{InnSettings.ApplicationName}</applicationname><expires>{applicationToken.Expires}</expires></params><Url type=\"application/xml\" method=\"POST\" template=\"https://inn-prod-sts.opplysningen.no/tokenservice/user/{applicationToken.ApplicationTokenId}/get_usertoken_by_usertokenid\" /></applicationtoken>";
        }

        private async Task<ApplicationToken> RefreshAuthorization()
        {
            return await RefreshApplicationToken()
                .ConfigureAwait(false);
        }

        // ReSharper disable InconsistentNaming
        // ReSharper disable ClassNeverInstantiated.Local
        [Serializable]
        [XmlType(AnonymousType = true),
         XmlRoot(Namespace = "", IsNullable = false)]
        public class applicationtoken
        {
            public applicationtokenParams @params { get; set; }
            public applicationtokenUrl Url { get; set; }
        }

        [XmlType(AnonymousType = true)]
        public class applicationtokenParams
        {
            public string applicationtokenID { get; set; }
            public string applicationid { get; set; }
            public string applicationname { get; set; }
            public ulong expires { get; set; }
        }
        [XmlType(AnonymousType = true)]
        public class applicationtokenUrl
        {
            [XmlAttribute(AttributeName = "type")]
            public string Type { get; set; }
            [XmlAttribute(AttributeName = "method")]
            public string Method { get; set; }
            [XmlAttribute(AttributeName = "template")]
            public string Template { get; set; }
        }
        // ReSharper restore InconsistentNaming
        // ReSharper restore ClassNeverInstantiated.Local
    }
}
