using System;
using System.Threading.Tasks;
using System.Web;
using EPiServer.ServiceLocation;
using INN.Core.Infrastructure;
using INN.Core.Models;
using INN.Core.Models.DTO;

namespace INN.Core.Services.Auth
{
    [ServiceConfiguration(typeof(IInnLoginService))]
    public class InnLoginService : IInnLoginService
    {
        private const string NoTicket = "no_ticket";
        private const string NoConsent = "no_consent";

        private readonly IInnTokenService _innTokenService;

        public InnLoginService(IInnTokenService innTokenService)
        {
            _innTokenService = innTokenService;
        }

        public async Task HandleRedirectResult(IInnUserTokenCache innUserTokenCache, string userticket)
        {
            //TODO let users decide where to store usertokens (customer, session etc)
            if(string.IsNullOrWhiteSpace(userticket) ||
                userticket.Equals(NoTicket))
            {
                innUserTokenCache.Set(UserToken.EmptyToken);
            }
            else
            {
                var userToken = await _innTokenService.GetUserToken(userticket);
                innUserTokenCache.Set(userToken);
            }
        }

        public async Task<InnStatusResult> GetLoginStatus(IInnUserTokenCache innUserTokenCache, string redirectResultUrl, string returnUrl)
        {
            if (!(innUserTokenCache.Get() is UserToken userToken))
            {
                // Redirect to INN to do a session check
                var redirectUrl = GetRedirectUrl(redirectResultUrl, returnUrl, true, false);
                return new InnStatusResult
                {
                    LoginStatus = InnLoginStatus.Unknown,
                    Url = redirectUrl
                };
            }

            if (!await _innTokenService.ValidateUsertokenId(userToken.UserTokenId))
            {
                userToken = UserToken.EmptyToken;
            }

            if (userToken.Equals(UserToken.EmptyToken))
            {
                // Show log in
                var redirectUrl = GetRedirectUrl(redirectResultUrl, returnUrl, false, true);
                return new InnStatusResult
                {
                    UserToken = userToken,
                    LoginStatus = InnLoginStatus.NotLoggedIn,
                    Url = redirectUrl
                };
            }
            if (userToken.Anonymous)
            {
                // Show consent
                var redirectUrl = await GetConsentUrl(redirectResultUrl, returnUrl, userToken).ConfigureAwait(false);
                return new InnStatusResult
                {
                    UserToken = userToken,
                    LoginStatus = InnLoginStatus.NeedConsent,
                    Url = redirectUrl
                };
            }

            return new InnStatusResult
            {
                UserToken = userToken,
                LoginStatus = InnLoginStatus.LoggedIn
            };
        }

        private string BuildReturnUrl(string redirectResultUrl, string returnToUrl, string userTicketDefault)
        {
            var uriBuilder = new UriBuilder(redirectResultUrl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["returnToUrl"] = returnToUrl;
            query["userticket"] = userTicketDefault;
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        public string GetRedirectUrl(string redirectResultUrl, string returnToUrl, bool sessionCheck, bool userCheckout)
        {
            var redirectUri = BuildReturnUrl(redirectResultUrl, returnToUrl, NoTicket);

            var uriBuilder = new UriBuilder($"{InnSettings.SingleSignOnServiceUri}/login");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            if (sessionCheck)
            {
                query["SessionCheck"] = "true";
            }
            if (userCheckout)
            {
                query["UserCheckout"] = "true";
            }
            query["redirectURI"] = redirectUri;
            uriBuilder.Query = query.ToString();

            return uriBuilder.ToString();
        }

        private async Task<string> GetConsentUrl(string redirectResultUrl, string returnToUrl)
        {
            var applicationTokenResponse = await _innTokenService.GetApplicationToken();
            var applicationToken = applicationTokenResponse.ApplicationTokenId.Replace("\"", string.Empty);

            var redirectUri = BuildReturnUrl(redirectResultUrl, returnToUrl, NoConsent);
            return $"{InnSettings.SingleSignOnServiceUri}/getAdressSharingConcent/{applicationToken}?redirectURI={redirectUri}";
        }

        private async Task<string> GetConsentUrl(string redirectResultUrl, string returnToUrl, UserToken userToken)
        {
            var applicationTokenResponse = await _innTokenService.GetApplicationToken();
            var userticket = await _innTokenService.GetUserTicketByUserTokenId(userToken.UserTokenId);
            var applicationToken = (applicationTokenResponse.ApplicationTokenId).Replace("\"", string.Empty);

            var redirectUri = BuildReturnUrl(redirectResultUrl, returnToUrl, NoConsent);
            return $"{InnSettings.SingleSignOnServiceUri}/getAddressSharingConsent/{applicationToken}?userticket={userticket}&redirectURI={redirectUri}";
        }
    }
}
