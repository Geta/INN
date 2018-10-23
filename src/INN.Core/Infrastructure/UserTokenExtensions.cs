using INN.Core.Models.DTO;

namespace INN.Core.Infrastructure
{
    public static class UserTokenExtensions
    {
        public static bool IsNullOrEmpty(this UserToken userToken)
        {
            return userToken == null || UserToken.EmptyToken.Equals(userToken);
        }
    }
}
