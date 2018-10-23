using INN.Core.Models.DTO;

namespace INN.Core.Models
{
    public class InnStatusResult
    {
        public InnLoginStatus LoginStatus { get; set; }

        public UserToken UserToken { get; set; }
        public string Url { get; set; }
    }
}
