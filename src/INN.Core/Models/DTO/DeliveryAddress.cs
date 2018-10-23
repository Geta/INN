namespace INN.Core.Models.DTO
{
    public class DeliveryAddress
    {
        public DeliveryAddress()
        {
            Contact = new Contact();
            DeliveryInformation = new DeliveryInformation();
        }

        public string Name { get; set; }
        public string Company { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Postalcode { get; set; }
        public string Postalcity { get; set; }
        public string CountryCode { get; set; }
        public string Reference { get; set; }
        public string Tags { get; set; }
        public Contact Contact { get; set; }
        public DeliveryInformation DeliveryInformation { get; set; }
    }
}
