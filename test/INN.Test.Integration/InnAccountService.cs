using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using INN.Core;
using INN.Core.Infrastructure;
using INN.Core.Models.DTO;
using INN.Core.Services.Auth;
using INN.Test.Integration.Response;
using Newtonsoft.Json.Linq;

namespace INN.Test.Integration
{
    public class InnAccountService : IInnAccountService
    {
        private readonly IInnTokenService _innTokenService;

        public InnAccountService(IInnTokenService innTokenService)
        {
            _innTokenService = innTokenService;
        }

        public async Task<LookupResponse> Lookup(string phoneNumber)
        {
            //https://sso.opplysningen.no/oidsso/api/opplysningenlookup
            var result = new LookupResponse();
            var client = InnHttpClient.Instance;
            var applicationTokenId = await _innTokenService.GetApplicationTokenId();
            var uri = new Uri($"{InnSettings.SingleSignOnServiceUri}/{applicationTokenId}/api/opplysningenlookup");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("phoneNo", phoneNumber)
            });
            var response = await client.PostAsync(uri, content);

            result.IsSuccess = response.IsSuccessStatusCode;
            result.StatusCode = response.StatusCode;
            var textResult = await response.Content.ReadAsStringAsync();
            if(textResult.Contains("Results"))
            {
                dynamic json = JObject.Parse(textResult);
                if(json.Results.Count > 0)
                {
                    var info = json.Results[0];
                    var phoneNumberInfo = new PhoneNumberInfo
                    {
                        FirstName = info.FirstName,
                        LastName = info.LastName,
                        MiddleName = info.MiddleName
                    };
                    if(info.Addresses.Count > 0)
                    {
                        var addressInfo = info.Addresses[0];
                        phoneNumberInfo.Address =
                            $"{addressInfo.Street} {addressInfo.HouseNumber} {addressInfo.HouseLetter}".Trim();
                        phoneNumberInfo.ZipCode = addressInfo.Zip;
                        phoneNumberInfo.City = addressInfo.City;
                    }
                    result.Information = phoneNumberInfo;
                }
            }
            return result;
        }

        public async Task<SendVerificationPinResponse> SendVerificationPin(string phoneNumber)
        {
            //https://sso.opplysningen.no/oidsso/{applicationtokenid}/api/getPin
            
            var client = InnHttpClient.Instance;

            var applicationTokenId = await _innTokenService.GetApplicationTokenId();
            var uri = new Uri($"{InnSettings.SingleSignOnServiceUri}/{applicationTokenId}/api/getPin");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("phoneNo", phoneNumber)
            });
            var response = await client.PostAsync(uri, content);
            var textResult = await response.Content.ReadAsStringAsync();
            var result = new SendVerificationPinResponse
            {
                StatusCode = response.StatusCode,
                IsSuccess = response.StatusCode == HttpStatusCode.OK &&
                            (string)JObject.Parse(textResult)["pin_sent"] == "true"
            };

            return result;
        }

        public async Task<UserExistsResponse> UserExists(string phoneNumber)
        {
            var result = new UserExistsResponse();

            //https://sso.opplysningen.no/oidsso/api/user_exist
            var client = InnHttpClient.Instance;
            var applicationTokenId = await _innTokenService.GetApplicationTokenId();
            var uri = new Uri($"{InnSettings.SingleSignOnServiceUri}/{applicationTokenId}/api/user_exist");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", phoneNumber)
            });
            var response = await client.PostAsync(uri, content);
            var textResult = await response.Content.ReadAsStringAsync();
            result.StatusCode = response.StatusCode;
            result.IsSuccess = response.IsSuccessStatusCode;
            var json = JObject.Parse(textResult);
            result.Exists = (string)json["user_exist"] == "true";
            return result;
        }

        public async Task<UserHasPasswordResponse> UserHasPassword(string phoneNumber)
        {
            var result = new UserHasPasswordResponse
            {
                IsSuccess = true
            };

            var client = InnHttpClient.Instance;

            var applicationTokenId = await _innTokenService.GetApplicationTokenId();
            var uri = new Uri($"{InnSettings.SingleSignOnServiceUri}/{applicationTokenId}/api/user_pw_exist");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", phoneNumber)
            });
            var response = await client.PostAsync(uri, content);
            var textResult = await response.Content.ReadAsStringAsync();
            result.StatusCode = response.StatusCode;
            var json = JObject.Parse(textResult);
            result.HasPassword = (string)json["user_pw_exist"] == "true";
            return result;
        }

        public async Task<LoginResponse> Login(string username, string password)
        {
            return await Login(username, password, "pwdlogin");
        }

        public async Task<LoginResponse> VerifyPin(string username, int pin)
        {
            return await Login(username, pin.ToString(), "login");
        }
        private async Task<LoginResponse> Login(string username, string password, string url)
        {
            var result = new LoginResponse();

            //https://sso.opplysningen.no/oidsso/api/pwdlogin
            var client = InnHttpClient.Instance;
            var applicationTokenId = await _innTokenService.GetApplicationTokenId();
            var uri = new Uri($"{InnSettings.SingleSignOnServiceUri}/{applicationTokenId}/api/{url}");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user", username),
                new KeyValuePair<string, string>("password", password)
            });
            var response = await client.PostAsync(uri, content);
            result.IsSuccess = response.IsSuccessStatusCode;
            result.StatusCode = response.StatusCode;

            if(response.IsSuccessStatusCode)
            {
                var textResult = await response.Content.ReadAsStringAsync();

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
                }
            }
            return result;
        }

        public async Task<LoginResponse> LoginOwnPassword(string username, string password)
        {
            var result = new LoginResponse();

            //https://sso.opplysningen.no/oidsso/api/pwdlogin
            var client = InnHttpClient.Instance;

            var applicationTokenId = await _innTokenService.GetApplicationTokenId();
            var uri = new Uri($"{InnSettings.SingleSignOnServiceUri}/{applicationTokenId}/api/pwdlogin");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("user", username),
                new KeyValuePair<string, string>("password", password)
            });
            var response = await client.PostAsync(uri, content);
            var textResult = await response.Content.ReadAsStringAsync();
            result.IsSuccess = response.IsSuccessStatusCode;
            result.StatusCode = response.StatusCode;

            if(response.IsSuccessStatusCode)
            {
                var doc = new XmlDocument();
                doc.LoadXml(textResult);
                result.UserTokenId = doc.SelectSingleNode("/usertoken")?.Attributes?["id"].Value;
            }

            return result;
        }


        public async Task<SignUpResponse> SignUp(string pin, string username, string email, string firstname, string lastname, string address, string zipcode, string city, string phoneNumber)
        {
            var result = new SignUpResponse();
            //https://sso.opplysningen.no/oidsso/api/signup
            var client = InnHttpClient.Instance;
            var applicationTokenId = await _innTokenService.GetApplicationTokenId();
            var uri = new Uri($"{InnSettings.SingleSignOnServiceUri}/{applicationTokenId}/api/signup");
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("firstname", firstname),
                new KeyValuePair<string, string>("lastname", lastname),
                new KeyValuePair<string, string>("cellphone", phoneNumber),
                new KeyValuePair<string, string>("pin", pin),
                new KeyValuePair<string, string>("postalcode", zipcode),
                new KeyValuePair<string, string>("postalcity", city),
                new KeyValuePair<string, string>("addressLine1", address)
            });
            var response = await client.PostAsync(uri, content);
            result.IsSuccess = response.IsSuccessStatusCode;
            result.StatusCode = response.StatusCode;

            var textResult = await response.Content.ReadAsStringAsync();

            if(string.IsNullOrWhiteSpace(textResult) || response.StatusCode != HttpStatusCode.OK)
            {
                result.IsSuccess = false;
            }
            else
            {
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
                }

                // We will override INN return codes if userTokenId is invalid
                if(string.IsNullOrWhiteSpace(result.UserTokenId))
                {
                    result.IsSuccess = false;
                    result.StatusCode = HttpStatusCode.OK;
                }
            }
            return result;
        }

        /// <summary>
/*
        * Force cross-applications/SSO session logout. Use with extreme care as the user's hate the resulting user experience..
        *
        * @param applicationtokenid
        * @param userTokenID
        * @return
        @Path("/{applicationtokenid}/release_usertoken")
        @POST

 */
        ///
        /// </summary>
        /// <param name="userTokenId"></param>
        /// <returns></returns>
        public async Task Logout(string userTokenId)
        {
            //https://sso.opplysningen.no/oidsso/api/logout
            var client = InnHttpClient.Instance;
            var applicationTokenId = await _innTokenService.GetApplicationTokenId();
            var uri = new Uri($"{InnSettings.SingleSignOnServiceUri}/{applicationTokenId}/release_usertoken");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("userTokenID", userTokenId)
            });

            var response = await client.PostAsync(uri, content);
            var textResult = await response.Content.ReadAsStringAsync();
            // var result = JObject.Parse(textResult);
            // return (string)result["result"] == "true";
        }
    }
}
