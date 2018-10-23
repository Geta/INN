using System;
using INN.Core.Models.DTO;

namespace INN.Core.Infrastructure
{
    public abstract class ApplicationTokenCacheBase : IApplicationTokenCache
    {
        protected const string CacheKey = "inn:applicationTokenId";

        public void Set(ApplicationToken applicationToken)
        {
            if (IsValidToken(applicationToken))
            {
                SetCachedToken(applicationToken);
            }
        }

        public abstract void SetCachedToken(ApplicationToken applicationToken);

        public ApplicationToken Get()
        {
            var token = GetFromCache();
            return IsValidToken(token) ? token : ApplicationToken.EmptyApplicationToken;
        }

        public abstract ApplicationToken GetFromCache();

        protected virtual bool IsValidToken(ApplicationToken token)
        {
            return token != null &&
                   !token.Equals(ApplicationToken.EmptyApplicationToken) &&
                   JavaTimeStampToDateTime(token.Expires) > DateTime.Now.AddSeconds(-10);
        }

        /// <summary>
        /// Converts java timestamp to datetime
        /// </summary>
        /// <param name="javaTimeStamp"></param>
        /// <returns></returns>
        protected virtual DateTime JavaTimeStampToDateTime(double javaTimeStamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(Math.Round(javaTimeStamp / 1000)).ToLocalTime();
        }
    }
}
