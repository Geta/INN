using System.Net;
using INN.Core.Models.DTO;

namespace INN.Test.Integration.Response
{
    public class LookupResponse
    {
        public bool IsSuccess { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public PhoneNumberInfo Information { get; set; }
    }
}
