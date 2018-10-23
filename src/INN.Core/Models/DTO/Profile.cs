using System;
using Newtonsoft.Json;

namespace INN.Core.Models.DTO
{
    public class Profile
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("firstname")]
        public string Firstname { get; set; }
        [JsonProperty("middlename")]
        public string Middlename { get; set; }
        [JsonProperty("lastname")]
        public string Lastname { get; set; }
        [JsonProperty("sex")]
        public string Sex { get; set; }
        [JsonProperty("birthdate")]
        public string Birthdate { get; set; }
        [JsonProperty("defaultEmailLabel")]
        public string DefaultEmailLabel { get; set; }
        [JsonProperty("defaultPhoneLabel")]
        public string DefaultPhoneLabel { get; set; }
        [JsonProperty("defaultAddressLabel")]
        public string DefaultAddressLabel { get; set; }
        [JsonProperty("defaultEmail")]
        public string DefaultEmail { get; set; }
    }
}
