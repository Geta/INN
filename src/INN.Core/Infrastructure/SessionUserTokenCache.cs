using System.Web;
using INN.Core.Models.DTO;

namespace INN.Core.Infrastructure
{
    public class SessionInnUserTokenCache : IInnUserTokenCache
    {
        private readonly HttpSessionStateBase _session;
        private const string InnUsertoken = "INN:UserToken";

        public SessionInnUserTokenCache(HttpSessionStateBase session)
        {
            _session = session;
        }

        public void Set(UserToken userToken)
        {
            _session[InnUsertoken] = userToken;
        }

        public UserToken Get()
        {
            return _session[InnUsertoken] as UserToken ?? UserToken.EmptyToken; 
        }
    }
}
