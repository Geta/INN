using System.Web;
using EPiServer.ServiceLocation;
using INN.Core.Models.DTO;

namespace INN.Core.Infrastructure
{
    [ServiceConfiguration(typeof(IApplicationTokenCache))]
    public class ApplicationTokenCache : ApplicationTokenCacheBase
    {
        public override void SetCachedToken(ApplicationToken token)
        {
            if (HttpContext.Current == null) return;
            HttpContext.Current.Cache[CacheKey] = token;
        }

        public override ApplicationToken GetFromCache()
        {
            if(HttpContext.Current == null) return null;
            return (ApplicationToken)HttpContext.Current.Cache[CacheKey];
        }
    }
}
