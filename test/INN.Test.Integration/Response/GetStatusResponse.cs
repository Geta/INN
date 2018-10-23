using System.Net;
using INN.Core.Models.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace INN.Test.Integration.Response
{
    public class GetStatusResponse
    {
        public bool IsSuccess { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        [JsonConverter(typeof (StringEnumConverter))]
        public AccountStatus AccountStatus { get; set; }
    }
}
