using System.Net;

namespace INN.Test.Integration.Response
{
    public class SignUpResponse
    {
        public string UserTokenId { get; set; }
        public bool IsSuccess { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Username { get; set; }
        public string Firstname { get; internal set; }
        public string Lastname { get; internal set; }
        public string Cellphone { get; internal set; }
        public string Email { get; internal set; }
        public string Ticket { get; set; }
    }
}
