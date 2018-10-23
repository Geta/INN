using INN.Core.Models.DTO;
using Newtonsoft.Json;

namespace INN.Core.Models.Request
{
    public class UpdateDeliveryAddressRequest
    {
        public UpdateDeliveryAddressRequest(Address address)
        {
            DeliveryAddress = new DeliveryAddress
            {
                Name = address.Name,
                Company = address.Company,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                Postalcode = address.Postalcode,
                Postalcity = address.Postalcity,
                CountryCode = address.CountryCode,
                Reference = "",
                Tags = address.Id,
                Contact =
                {
                    Name = address.Name,
                    Email = address.Email,
                    EmailConfirmed = false,
                    PhoneNumber = address.PhoneNumber,
                    PhoneNumberConfirmed = true
                },
                DeliveryInformation =
                {
                    AdditionalAddressInfo = address.AdditionalAddressInfo,
                    PickupPoint = "",
                    Deliverytime = ""
                }
            };
            ;
        }

        [JsonProperty("deliveryaddress")]
        public DeliveryAddress DeliveryAddress { get; set; }
    }
}
