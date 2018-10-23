using System.Collections.Generic;
using System.Linq;
using INN.Core.Models.DTO;
using Mediachase.Commerce.Customers;

namespace INN.Core.Services.Address
{
    public static class AddressExtensions
    {
        public static CustomerAddress GetInnAddress(this IEnumerable<CustomerAddress> customerAddresses, string identifier)
        {
            return customerAddresses.FirstOrDefault(x => x.GetInnIdentifier().Equals(identifier));
        }

        public static string GetInnIdentifier(this CustomerAddress customerAddress)
        {
            return customerAddress[Constants.AddressIdentifierFieldName]?.ToString() ?? string.Empty;
        }

        public static CustomerAddress MapModel(this DeliveryAddress innAddress, CustomerAddress customerAddress)
        {
            customerAddress[Constants.AddressIdentifierFieldName] = innAddress.Tags;
            customerAddress.Name = innAddress.Name;
            customerAddress.Line1 = innAddress.AddressLine1 ?? string.Empty;
            customerAddress.Line2 = innAddress.AddressLine2 ?? string.Empty;
            customerAddress.PostalCode = innAddress.Postalcode ?? string.Empty;
            customerAddress.City = innAddress.Postalcity ?? string.Empty;
            customerAddress.CountryCode = innAddress.CountryCode ?? string.Empty;
            customerAddress.EveningPhoneNumber =
                customerAddress.DaytimePhoneNumber = innAddress.Contact?.PhoneNumber ?? string.Empty;
            customerAddress.Email = innAddress.Contact?.Email ?? string.Empty;

            return customerAddress;
        }
    }
}
