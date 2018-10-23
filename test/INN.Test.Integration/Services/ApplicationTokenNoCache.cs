using INN.Core.Infrastructure;
using INN.Core.Models.DTO;

namespace INN.Test.Integration.Services
{
    public class ApplicationTokenNoCache : ApplicationTokenCacheBase
    {
        public override void SetCachedToken(ApplicationToken applicationToken)
        {
        }

        public override ApplicationToken GetFromCache()
        {
            return null;
        }
    }
}