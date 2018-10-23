using System.Net;

namespace INN.Test.Integration.Response
{
    public class SendVerificationPinResponse
    {
        public bool IsSuccess { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
