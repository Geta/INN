using System.Net;

namespace INN.Core.Models.Response
{
    public class GetUserTicketByUserTokenIdResponse
    {
        public bool IsSuccess { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Ticket { get; set; }
    }
}
