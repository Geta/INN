using System.Threading.Tasks;
using INN.Core.Models.Request;
using INN.Core.Models.Response;
using Refit;

namespace INN.Core.Api
{
    public interface ITokenApi
    {
        [Post("/logon")]
        Task<string> LoginApp([Body(BodySerializationMethod.UrlEncoded)] ApplicationCredentialRequest applicationCredentialRequest);
        [Get("/user/{applicationTokenId}/validate_usertokenid/{userTokenId}")]
        Task<ResultResponse> ValidateUserTokenId(string applicationTokenId, string userTokenId);
    }
}
