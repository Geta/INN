using System.Net;

namespace INN.Test.Integration.Response
{
    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string UserTokenId { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Cellphone { get; set; }
        public string Email { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
