using INN.Core.Models.DTO;
using Newtonsoft.Json;

namespace INN.Core.Models.Response
{
    public class DeliveryAddressResponse
    {
        [JsonProperty("deliveryaddress")]
        public DeliveryAddress DeliveryAddress { get; set; }
    }
}
