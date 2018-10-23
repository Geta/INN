using INN.Core.Models.DTO;

namespace INN.Core.Infrastructure
{
    public interface IApplicationTokenCache
    {
        void Set(ApplicationToken applicationToken);
        ApplicationToken Get();
    }
}
