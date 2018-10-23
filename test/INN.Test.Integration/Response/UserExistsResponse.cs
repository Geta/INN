using System.Net;

namespace INN.Test.Integration.Response
{
    public class UserExistsResponse
    {
        public bool Exists { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
    }
}
