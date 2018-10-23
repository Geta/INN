using System.Net;

namespace INN.Core.Models.DTO
{
    /// <summary>
    /// User token from Capra
    /// </summary>
    public class UserToken
    {
        public static UserToken EmptyToken = new UserToken();

        /// <summary>
        /// The user token Id. Use this to call others services
        /// </summary>
        public string UserTokenId { get; set; }
        public bool IsSuccess { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Cellphone { get; set; }
        public string Email { get; set; }
        public bool Anonymous { get; set; }
    }
}
