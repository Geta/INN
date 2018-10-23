using System.Collections.Generic;
using INN.Core.Models.DTO;
using Mediachase.Commerce.Customers;

namespace INN.Core.Services.Address
{
    public interface ISyncInnAddresses
    {
        bool SyncToEpi(CustomerContact customerContact, List<DeliveryAddress> deliveryAddresses);
    }
}
