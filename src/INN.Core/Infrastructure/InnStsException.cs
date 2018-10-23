using System;
using System.Net;
using Newtonsoft.Json;

namespace INN.Core.Infrastructure
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InnStsException : Exception
    {
        [JsonProperty("status")]
        public HttpStatusCode Status { get; set; }
        [JsonProperty("code")]
        public InnStsErrorCode Code { get; set; }
        [JsonProperty("message")]
        public string InnErrorMessage { get; set; }
        [JsonProperty("link")]
        public string Link { get; set; }
        [JsonProperty("developerMessage")]
        public string DeveloperMessage { get; set; }

        public Exception BaseException { get; set; }

        [JsonConstructor]
        public InnStsException() { }

        public InnStsException(Exception ex)
        {
            BaseException = ex;
        }
    }
}
