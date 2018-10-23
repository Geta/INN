using System.Runtime.Caching;
using INN.Core.Infrastructure;
using INN.Core.Models.DTO;

namespace INN.Test.Integration.Services
{
    public class ApplicationTokenMemoryCache : ApplicationTokenCacheBase
    {
        public override void SetCachedToken(ApplicationToken applicationToken)
        {
            MemoryCache.Default[CacheKey] = applicationToken;
        }

        public override ApplicationToken GetFromCache()
        {
            return (ApplicationToken) MemoryCache.Default[CacheKey];
        }
    }
}
