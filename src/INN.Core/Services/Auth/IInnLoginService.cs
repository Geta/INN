using System.Threading.Tasks;
using INN.Core.Infrastructure;
using INN.Core.Models;

namespace INN.Core.Services.Auth
{
    public interface IInnLoginService
    {
        Task HandleRedirectResult(IInnUserTokenCache innUserTokenCache, string userticket);
        Task<InnStatusResult> GetLoginStatus(IInnUserTokenCache innUserTokenCache, string redirectResultUrl, string returnUrl);
        string GetRedirectUrl(string redirectResultUrl, string returnToUrl, bool sessionCheck, bool userCheckout);
    }
}
