using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using INN.Core.Models.DTO;
using INN.Core.Models.Request;
using INN.Core.Models.Response;
using Refit;

namespace INN.Core.Api
{
    public interface IAddressApi
    {
        [Get("/{applicationTokenId}/api/{userTokenId}/deliveryaddress/list")]
        Task<Profile> GetProfile(string applicationTokenId, string userTokenId);

        [Get("/{applicationTokenId}/api/{userTokenId}/deliveryaddress")]
        Task<List<DeliveryAddress>> GetAddresses(string applicationTokenId, string userTokenId);

        [Get("/{applicationTokenId}/api/{userTokenId}/select_default_deliveryaddress")]
        Task<DeliveryAddressResponse> GetDefaultDeliveryAddress(string applicationTokenId, string userTokenId);

        [Post("/{applicationTokenId}/api/{userTokenId}/deliveryaddress/add")]
        Task<DeliveryAddressResponse> AddAddress(string applicationTokenId, string userTokenId, [Body(BodySerializationMethod.UrlEncoded)] AddDeliveryAddressRequest deliveryAddressRequest);

        [Post("/{applicationTokenId}/api/{userTokenId}/deliveryaddress/update")]
        Task<ResultResponse> UpdateDeliveryAddress(string applicationTokenId, string userTokenId, [Body] UpdateDeliveryAddressRequest deliveryAddress);

        [Post("/{applicationTokenId}/api/{userTokenId}/deliveryaddress/delete/{tag}")]
        Task<ResultResponse> DeleteDeliveryAddress(string applicationTokenId, string userTokenId, string tag);
    }
}
