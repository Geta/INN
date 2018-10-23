using System.Threading.Tasks;
using INN.Core.Models.DTO;

namespace INN.Core.Services.Auth
{
    public interface IInnTokenService
    {
        Task<string> GetApplicationTokenId();
        Task<ApplicationToken> GetApplicationToken();
        Task<bool> ValidateUsertokenId(string userTokenId);
        Task<UserToken> GetUserToken(string userTicket);
        Task<string> GetUserTicketByUserTokenId(string userTokenId);
        Task<ApplicationToken> RefreshApplicationToken();
    }
}
