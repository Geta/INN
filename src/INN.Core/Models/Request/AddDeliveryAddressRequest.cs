using INN.Core.Models.DTO;
using Refit;

namespace INN.Core.Models.Request
{
    public class AddDeliveryAddressRequest
    {
        public AddDeliveryAddressRequest(Address address)
        {
            Company = address.Company;
            Email = address.Email;
            Name = address.Name;
            Cellphone = address.PhoneNumber;
            CountryCode = address.CountryCode;
            Postalcode = address.Postalcode;
            Postalcity = address.Postalcity;
            AddressLine1 = address.AddressLine1;
            AddressLine2 = address.AddressLine2;
            Comment = address.AdditionalAddressInfo;
        }

        [AliasAs("company")]
        public string Company { get; set; }
        [AliasAs("email")]
        public string Email { get; set; }
        [AliasAs("name")]
        public string Name { get; set; }
        [AliasAs("cellphone")]
        public string Cellphone { get; set; }
        [AliasAs("countryCode")]
        public string CountryCode { get; set; }
        [AliasAs("postalcode")]
        public string Postalcode { get; set; }
        [AliasAs("postalcity")]
        public string Postalcity { get; set; }
        [AliasAs("addressLine1")]
        public string AddressLine1 { get; set; }
        [AliasAs("addressLine2")]
        public string AddressLine2 { get; set; }
        [AliasAs("comment")]
        public string Comment { get; set; }
    }
}
