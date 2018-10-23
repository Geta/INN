using System.Collections.Generic;
using System.Threading.Tasks;
using INN.Core.Models.DTO;
using Mediachase.Commerce.Customers;

namespace INN.Core.Services.Address
{
    public interface IInnAddressService
    {
        Task<Profile> GetUserProfile(string userTokenId);
        Task<List<DeliveryAddress>> GetUserAddresses(string userTokenId);
        Task<DeliveryAddress> GetDefaultDeliveryAddress(string userTokenId);
        Task<DeliveryAddress> AddUserAddress(string userTokenId, Models.DTO.Address address);
        Task<bool> UpdateUserAddress(string userTokenId, Models.DTO.Address updateDeliveryAddressRequest);
        Task<bool> DeleteUserAddress(string userTokenId, string tag);
        Task<bool> SyncAddresses(CustomerContact currentContactCurrentContact, string userTokenId);
    }
}
