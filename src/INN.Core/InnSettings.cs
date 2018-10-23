using System.Configuration;

namespace INN.Core
{
    public static class InnSettings
    {
        public static string TokenServiceUri => ConfigurationManager.AppSettings["INN:TokenServiceUri"];
        public static string SingleSignOnServiceUri => ConfigurationManager.AppSettings["INN:SingleSignOnServiceUri"];
        public static string ApplicationId => ConfigurationManager.AppSettings["INN:ApplicationId"];
        public static string ApplicationSecret => ConfigurationManager.AppSettings["INN:ApplicationSecret"];
        public static string ApplicationName => ConfigurationManager.AppSettings["INN:ApplicationName"];
    }
}
