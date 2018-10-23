using INN.Core.Models.DTO;
using INN.Core.Services.Address;
using Mediachase.Commerce.Customers;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook
{
    public class CustomSyncInnAddresses : DefaultSyncInnAddresses
    {
        public override CustomerAddress MapModel(CustomerAddress customerAddress, DeliveryAddress innAddress)
        {
            base.MapModel(customerAddress, innAddress);
            customerAddress.Name = $"INN-{customerAddress.Name}";
            return customerAddress;
        }
    }
}