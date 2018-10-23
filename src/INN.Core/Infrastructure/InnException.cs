using System;
using System.Net;
using Newtonsoft.Json;

namespace INN.Core.Infrastructure
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InnException : Exception
    {
        [JsonProperty("status")]
        public HttpStatusCode Status { get; set; }
        [JsonProperty("code")]
        public InnErrorCode Code { get; set; }
        [JsonProperty("message")]
        public string InnErrorMessage { get; set; }
        [JsonProperty("link")]
        public string Link { get; set; }
        [JsonProperty("developerMessage")]
        public string DeveloperMessage { get; set; }

        public Exception BaseException { get; set; }

        [JsonConstructor]
        public InnException() { }

        public InnException(Exception ex)
        {
            BaseException = ex;
        }
    }
}
