using System.Web;
using INN.Core.Models.DTO;
using Newtonsoft.Json;

namespace INN.Core.Infrastructure
{
    public class CookieTokenCache : IInnUserTokenCache
    {
        public const string CookieName = "INN:Usertoken";
        private HttpContextBase _httpContext;

        public CookieTokenCache(
            HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }

        public UserToken Get()
        {
            var cookie = _httpContext.Request.Cookies.Get(CookieName);
            if(cookie != null)
            {
                var cookieValue = cookie.Value;
                try
                {
                    return JsonConvert.DeserializeObject<UserToken>(cookieValue);
                }
                catch { }
            }
            return null;
        }

        public void Set(UserToken userToken)
        {
            _httpContext.Response.Cookies.Set(new HttpCookie(CookieName, JsonConvert.SerializeObject(userToken)));
        }
    }
}
