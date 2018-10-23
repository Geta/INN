using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using INN.Core;
using INN.Core.Api;
using INN.Core.Infrastructure;
using INN.Core.Models.DTO;
using INN.Core.Services.Auth;
using Refit;
using Xunit;

namespace INN.Test.Integration.Services.Auth
{
    public class InnTokenServiceTests : InnServiceTestBase
    {
        private readonly ApplicationToken _realApplicationTokenId;
        private readonly string _realUserTokenId;

        public InnTokenServiceTests()
        {
            _realApplicationTokenId = TestHelper.GetApplicationToken().GetAwaiter().GetResult();
            _realUserTokenId = TestHelper.GetUserTokenId(PhoneNumber, Password).GetAwaiter().GetResult();
        }

        [Fact]
        public void can_get_application_token_id()
        {
            Assert.False(_realApplicationTokenId.Equals(ApplicationToken.EmptyApplicationToken));
            Assert.False(string.IsNullOrWhiteSpace(_realApplicationTokenId.ApplicationTokenId));
        }

        [Fact]
        public async Task can_validate_retries_on_wrong_applicationToken()
        {
            var app = A.Fake<IApplicationTokenCache>();
            var api = A.Fake<ITokenApi>();
            var service = new InnTokenService(app, api);
            var fakeToken = new ApplicationToken
            {
                ApplicationTokenId = "fake"
            };
            var fakeToken2 = new ApplicationToken
            {
                ApplicationTokenId = "fake2"
            };
            var exception = await GetException();

            A.CallTo(() => api.ValidateUserTokenId(A<string>._, A<string>._)).Throws(exception);
            A.CallTo(() => app.Get())
                .Returns(fakeToken)
                .Once()
                .Then
                .Returns(fakeToken2);

            var validateUserTokenResponse =
                await service.ValidateUsertokenId("ThisWillNotWork");

            A.CallTo(() => api.ValidateUserTokenId(fakeToken.ApplicationTokenId, A<string>._))
                .MustHaveHappened(Repeated.Exactly.Once)
                .Then(A.CallTo(() => api.ValidateUserTokenId(fakeToken2.ApplicationTokenId, A<string>._))
                    .MustHaveHappened(Repeated.Exactly.Once));

            Assert.False(validateUserTokenResponse);
        }

        [Fact]
        public async Task can_validate_wrong_usertoken()
        {
            var applicationTokenCache = A.Fake<IApplicationTokenCache>();
            var tokenService = new InnTokenService(applicationTokenCache,
                TestHelper.CreateApi<ITokenApi>(InnSettings.TokenServiceUri));

            A.CallTo(() => applicationTokenCache.Get()).Returns(_realApplicationTokenId);

            var validateUserTokenResponse =
                await tokenService.ValidateUsertokenId("ThisWillNotWork");

            Assert.False(validateUserTokenResponse);
        }

        [Fact]
        public async Task can_validate_correct_usertoken()
        {
            var applicationTokenCache = A.Fake<IApplicationTokenCache>();
            var tokenService = new InnTokenService(applicationTokenCache,
                TestHelper.CreateApi<ITokenApi>(InnSettings.TokenServiceUri));

            A.CallTo(() => applicationTokenCache.Get()).Returns(_realApplicationTokenId);

            var validateUserTokenResponse =
                await tokenService.ValidateUsertokenId(_realUserTokenId);

            Assert.True(validateUserTokenResponse);
        }

        private static async Task<ApiException> GetException()
        {
            return await ApiException.Create(A.Fake<Uri>(), HttpMethod.Get,
                new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    Content = new StringContent(
                        "{\"status\":403,\"code\":7000,\"message\":\"Illegal Application.\",\"link\":\"\",\"developerMessage\":\"Application is invalid\"}")
                }, TestHelper.GetRefitSettings());
        }
    }
}
