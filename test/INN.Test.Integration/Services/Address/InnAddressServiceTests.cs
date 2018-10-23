using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using INN.Core;
using INN.Core.Api;
using INN.Core.Services.Address;
using INN.Core.Services.Auth;
using INN.Core.Infrastructure;
using Refit;
using Xunit;

namespace INN.Test.Integration.Services.Address
{
    public class InnAddressServiceTests : InnServiceTestBase
    {
        private readonly string _realUserTokenId;

        public InnAddressServiceTests()
        {
            _realUserTokenId = TestHelper.GetUserTokenId(PhoneNumber, Password).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task it_tries_to_refresh_application_token()
        {
            var tokenService = A.Fake<IInnTokenService>();
            var addressApi = A.Fake<IAddressApi>();
            var syncInnAddresses = A.Fake<ISyncInnAddresses>();
            var service = new InnAddressService(tokenService, addressApi, syncInnAddresses);
            var exception = await GetException();

            A.CallTo(() => addressApi.GetAddresses(A<string>._, A<string>._))
                .Throws(c => exception);

            var innException = Assert.Throws<InnException>(() =>
            {
                var userAddressesResponse = service.GetUserAddresses(_realUserTokenId).GetAwaiter().GetResult();
            });

            Assert.IsType<ApiException>(innException.BaseException);

            A.CallTo(() => tokenService.GetApplicationTokenId()).MustHaveHappened(Repeated.Exactly.Twice);
            A.CallTo(() => tokenService.RefreshApplicationToken()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => addressApi.GetAddresses(A<string>._, A<string>._))
                .MustHaveHappened(Repeated.Exactly.Twice);
        }

        [Fact]
        public async Task can_get_profile()
        {
            var innTokenService = new InnTokenService(new ApplicationTokenMemoryCache(),
                TestHelper.CreateApi<ITokenApi>(InnSettings.TokenServiceUri));
            var innAddressService = new InnAddressService(
                innTokenService,
                TestHelper.CreateApi<IAddressApi>(InnSettings.SingleSignOnServiceUri),
                A.Fake<ISyncInnAddresses>());

            var profile = await innAddressService.GetUserProfile(_realUserTokenId);

            Assert.NotNull(profile);
            Assert.False(Guid.Empty.Equals(profile.Id));
        }

        [Fact]
        public async Task can_get_addresses()
        {
            var innTokenService = new InnTokenService(new ApplicationTokenMemoryCache(),
                TestHelper.CreateApi<ITokenApi>(InnSettings.TokenServiceUri));
            var innAddressService = new InnAddressService(
                innTokenService,
                TestHelper.CreateApi<IAddressApi>(InnSettings.SingleSignOnServiceUri),
                A.Fake<ISyncInnAddresses>());

            var userAddressesResponse = await innAddressService.GetUserAddresses(_realUserTokenId);

            Assert.NotNull(userAddressesResponse);
            Assert.False(string.IsNullOrWhiteSpace(userAddressesResponse.First().Name));
            foreach (var deliveryAddress in userAddressesResponse)
            {
                Assert.False(string.IsNullOrWhiteSpace(deliveryAddress.Tags));
                Assert.False(string.IsNullOrWhiteSpace(deliveryAddress.Name));
            }
        }

        [Fact]
        public async Task can_get_default__addresses()
        {
            var innTokenService = new InnTokenService(new ApplicationTokenMemoryCache(),
                TestHelper.CreateApi<ITokenApi>(InnSettings.TokenServiceUri));
            var innAddressService = new InnAddressService(
                innTokenService,
                TestHelper.CreateApi<IAddressApi>(InnSettings.SingleSignOnServiceUri),
                A.Fake<ISyncInnAddresses>());

            var address = await innAddressService.GetDefaultDeliveryAddress(_realUserTokenId);

            Assert.NotNull(address);
            Assert.False(string.IsNullOrWhiteSpace(address.Tags));
            Assert.False(string.IsNullOrWhiteSpace(address.Name));
        }

        [Fact]
        public async Task can_create_update_and_delete_address()
        {
            var innTokenService = new InnTokenService(new ApplicationTokenMemoryCache(),
                TestHelper.CreateApi<ITokenApi>(InnSettings.TokenServiceUri));
            var innAddressService = new InnAddressService(
                innTokenService,
                TestHelper.CreateApi<IAddressApi>(InnSettings.SingleSignOnServiceUri),
                A.Fake<ISyncInnAddresses>());

            var testAddressName = "TestAddressName";

            while(await RemoveTestAddress(testAddressName) != null) { }

            var testAddress = new Core.Models.DTO.Address
            {
                Name = testAddressName,
                AddressLine1 = "create-firstline",
                AddressLine2 = "create-secondline",
                Company = "Geta",
                CountryCode = "no",
                Postalcity = "Oslo",
                Postalcode = "0151",
                Email = "brian@geta.no",
                PhoneNumber = ""
            };

            // Create
            var deliveryAddress =
                await innAddressService.AddUserAddress(_realUserTokenId, testAddress);
            Assert.Equal(testAddress.AddressLine1, deliveryAddress.AddressLine1);

            // Update
            var updatedAddressLine1 = $"{testAddress.AddressLine1}-updated";
            testAddress.Id = deliveryAddress.Tags;
            testAddress.AddressLine1 = updatedAddressLine1;
            var userAddressResponse = await innAddressService.UpdateUserAddress(_realUserTokenId, testAddress);
            Assert.True(userAddressResponse);

            var updatedAddress =
                (await innAddressService.GetUserAddresses(_realUserTokenId)).FirstOrDefault(
                    x => x.Tags.Equals(testAddress.Id));
            Assert.NotNull(updatedAddress);
            Assert.Equal(updatedAddressLine1, updatedAddress.AddressLine1);

            // Delete - Cleanup
            var deleteUserAddressResponse = await RemoveTestAddress(testAddressName);
            Assert.NotNull(deleteUserAddressResponse);
            Assert.True(deleteUserAddressResponse);
        }

        private async Task<bool?> RemoveTestAddress(string testAddressName)
        {
            var innTokenService = new InnTokenService(new ApplicationTokenMemoryCache(),
                TestHelper.CreateApi<ITokenApi>(InnSettings.TokenServiceUri));
            var innAddressService = new InnAddressService(
                innTokenService,
                TestHelper.CreateApi<IAddressApi>(InnSettings.SingleSignOnServiceUri),
                A.Fake<ISyncInnAddresses>());

            var userAddressesResponse = await innAddressService.GetUserAddresses(_realUserTokenId);
            var existingAddress = userAddressesResponse.FirstOrDefault(a => a.Name.Equals(testAddressName));
            if (existingAddress != null)
            {
                return await innAddressService.DeleteUserAddress(_realUserTokenId, existingAddress.Tags);
            }

            return null;
        }

        private static async Task<ApiException> GetException()
        {
            return await ApiException.Create(A.Fake<Uri>(), HttpMethod.Get,
                new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(
                        "{\"status\":400,\"code\":5000,\"message\":\"Illegal Application Session.\",\"link\":\"\",\"developerMessage\":\"\"}")
                }, TestHelper.GetRefitSettings());
        }
    }
}
