using System.Threading.Tasks;
using INN.Core;
using INN.Core.Api;
using INN.Core.Services.Auth;
using Xunit;

namespace INN.Test.Integration.Services
{
    public class InnAccountServiceTests : InnServiceTestBase
    {
        public IInnTokenService InnTokenService;
        public IInnAccountService InnAccountService;

        public InnAccountServiceTests()
        {
            InnTokenService = new InnTokenService(new ApplicationTokenMemoryCache(), TestHelper.CreateApi<ITokenApi>(InnSettings.TokenServiceUri));
            InnAccountService = new InnAccountService(InnTokenService);
        }

        [Fact]
        public async Task log_in_with_wrong_password_fails()
        {
            var loginResponse = await InnAccountService.LoginOwnPassword(PhoneNumber, "ThisWillNotWork");

            Assert.False(loginResponse.IsSuccess);
        }

        [Fact]
        public async Task log_in_with_correct_password_succeeds()
        {
            Assert.False(string.IsNullOrWhiteSpace(await TestHelper.GetUserTokenId(PhoneNumber, Password)));
        }
    }
}
