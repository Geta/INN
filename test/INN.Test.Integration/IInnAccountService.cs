using System.Threading.Tasks;
using INN.Test.Integration.Response;

namespace INN.Test.Integration
{
    public interface IInnAccountService
    {
        Task<LookupResponse> Lookup(string phoneNumber);
        Task<UserExistsResponse> UserExists(string phoneNumber);

        Task<LoginResponse> Login(string username, string password);
        Task<LoginResponse> LoginOwnPassword(string username, string password);
        Task<SendVerificationPinResponse> SendVerificationPin(string phoneNumber);
        Task<LoginResponse> VerifyPin(string username, int pin);
        Task<UserHasPasswordResponse> UserHasPassword(string phoneNumber);
        Task Logout(string userTokenId);
        
        Task<SignUpResponse> SignUp(string pin, string username, string email, string firstname, string lastname, string address, string zipcode, string city, string phoneNumber);
    }
}
