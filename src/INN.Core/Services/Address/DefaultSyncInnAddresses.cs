using System.Collections.Generic;
using EPiServer.ServiceLocation;
using INN.Core.Models.DTO;
using Mediachase.Commerce.Customers;

namespace INN.Core.Services.Address
{
    [ServiceConfiguration(typeof(ISyncInnAddresses))]
    public class DefaultSyncInnAddresses : ISyncInnAddresses
    {
        public virtual bool SyncToEpi(CustomerContact customerContact, List<DeliveryAddress> deliveryAddresses)
        {
            foreach (var deliveryAddress in deliveryAddresses)
            {
                var isNewAddress = false;
                var customerAddress = customerContact.ContactAddresses.GetInnAddress(deliveryAddress.Tags);
                if (customerAddress == null)
                {
                    // Create
                    isNewAddress = true;
                    customerAddress = CustomerAddress.CreateInstance();
                    customerAddress[Constants.AddressIdentifierFieldName] = deliveryAddress.Tags;
                }

                customerAddress = MapModel(customerAddress, deliveryAddress);
                CreateOrUpdateAddress(customerContact, isNewAddress, customerAddress);
            }
            customerContact.SaveChanges();

            return true;
        }

        public virtual void CreateOrUpdateAddress(CustomerContact customerContact, bool isNewAddress, CustomerAddress customerAddress)
        {
            if (isNewAddress)
            {
                customerContact.AddContactAddress(customerAddress);
            }
            else
            {
                customerContact.UpdateContactAddress(customerAddress);
            }
        }

        public virtual CustomerAddress MapModel(CustomerAddress customerAddress, DeliveryAddress deliveryAddress)
        {
            return deliveryAddress.MapModel(customerAddress);
        }
    }
}
