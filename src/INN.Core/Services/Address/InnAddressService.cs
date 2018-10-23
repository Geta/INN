using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using INN.Core.Api;
using INN.Core.Infrastructure;
using INN.Core.Models.DTO;
using INN.Core.Models.Request;
using INN.Core.Services.Auth;
using Mediachase.Commerce.Customers;
using Polly;
using Polly.Retry;
using Refit;

namespace INN.Core.Services.Address
{
    [ServiceConfiguration(typeof(IInnAddressService))]
    public class InnAddressService : IInnAddressService
    {
        private readonly IInnTokenService _innTokenService;
        private readonly ILogger _log = LogManager.GetLogger(typeof(InnAddressService));
        private readonly IAddressApi _addressApi;
        private readonly ISyncInnAddresses _syncInnAddresses;
        private readonly RetryPolicy _retryPolicy;

        public InnAddressService(IInnTokenService innTokenService,
            IAddressApi addressApi,
            ISyncInnAddresses syncInnAddresses)
        {
            _innTokenService = innTokenService;
            _addressApi = addressApi;
            _syncInnAddresses = syncInnAddresses;

            // Try to refresh application token
            _retryPolicy = Policy
                .Handle<ApiException>(apiException => apiException.StatusCode == HttpStatusCode.BadRequest &&
                                                      apiException.GetContentAs<InnException>()?.Code ==
                                                      InnErrorCode.InvalidApplication)
                .RetryAsync(1, (e, i) => RefreshAuthorization());
        }

        public async Task<Profile> GetUserProfile(string userTokenId)
        {
            var applicationTokenId = await _innTokenService.GetApplicationTokenId();
            return await Execute(() => _addressApi.GetProfile(applicationTokenId, userTokenId))
                .ConfigureAwait(false);
        }

        public async Task<List<DeliveryAddress>> GetUserAddresses(string userTokenId)
        {
            return (await Execute(async () =>
            {
                var applicationTokenId = await _innTokenService.GetApplicationTokenId();
                return await _addressApi.GetAddresses(applicationTokenId, userTokenId);
            }).ConfigureAwait(false))
                .ToList();
        }

        public async Task<DeliveryAddress> GetDefaultDeliveryAddress(string userTokenId)
        {
            var deliveryAddressResponse = await Execute(async () =>
            {
                var applicationTokenId = await _innTokenService.GetApplicationTokenId().ConfigureAwait(false);
                return await _addressApi.GetDefaultDeliveryAddress(applicationTokenId, userTokenId);
            })
                .ConfigureAwait(false);

            return deliveryAddressResponse.DeliveryAddress;
        }

        public async Task<DeliveryAddress> AddUserAddress(string userTokenId, Models.DTO.Address address)
        {
            var addResult = await Execute(async () =>
            {
                var applicationTokenId = await _innTokenService.GetApplicationTokenId().ConfigureAwait(false);
                return await _addressApi.AddAddress(applicationTokenId, userTokenId,
                    new AddDeliveryAddressRequest(address));
            })
                .ConfigureAwait(false);

            return addResult.DeliveryAddress;
        }

        public async Task<bool> UpdateUserAddress(string userTokenId, Models.DTO.Address address)
        {
            var updateResult = await Execute(async () =>
                {
                    var applicationTokenId = await _innTokenService.GetApplicationTokenId().ConfigureAwait(false);
                    return await _addressApi.UpdateDeliveryAddress(applicationTokenId, userTokenId,
                        new UpdateDeliveryAddressRequest(address));
                })
                .ConfigureAwait(false);

            return updateResult.Result;

        }

        public async Task<bool> DeleteUserAddress(string userTokenId, string tag)
        {
            try
            {
                var deleteResult = await Execute(async () =>
                {
                    var applicationTokenId = await _innTokenService.GetApplicationTokenId().ConfigureAwait(false);
                    return await _addressApi.DeleteDeliveryAddress(applicationTokenId, userTokenId, tag);
                })
                    .ConfigureAwait(false);

                return deleteResult.Result;
            }
            catch(Exception apiException)
            {
                _log.Error("Something went wrong", apiException);
                throw GetInnException(apiException);
            }
        }

        public async Task<bool> SyncAddresses(CustomerContact customerContact, string userTokenId)
        {
            var userAddresses = await GetUserAddresses(userTokenId)
                .ConfigureAwait(false);
            return userAddresses.Any() && _syncInnAddresses.SyncToEpi(customerContact, userAddresses);
        }

        protected InnException GetInnException(Exception exception)
        {
            var innException = new InnException
            {
                DeveloperMessage = "Unknown exception"
            };

            if(exception is ApiException apiException)
            {
                innException = apiException.GetContentAs<InnException>() ?? innException;
            }

            innException.BaseException = exception;
            return innException;
        }

        private async Task<ApplicationToken> RefreshAuthorization()
        {
            return await _innTokenService.RefreshApplicationToken()
                .ConfigureAwait(false);
        }

        protected async Task<T> Execute<T>(Func<Task<T>> func)
        {
            try
            {
                return await _retryPolicy
                    .ExecuteAsync(func)
                    .ConfigureAwait(false);
            }
            catch(Exception apiException)
            {
                _log.Error("Something went wrong", apiException);
                throw GetInnException(apiException);
            }
        }
    }
}
