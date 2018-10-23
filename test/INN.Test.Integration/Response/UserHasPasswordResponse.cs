using System.Net;

namespace INN.Test.Integration.Response
{
    public class UserHasPasswordResponse
    {
        public bool HasPassword { get; set; }

        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
    }
}
