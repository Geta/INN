using INN.Core.Models.DTO;

namespace INN.Core.Infrastructure
{
    public interface IInnUserTokenCache
    {
        void Set(UserToken userToken);
        UserToken Get();
    }
}
