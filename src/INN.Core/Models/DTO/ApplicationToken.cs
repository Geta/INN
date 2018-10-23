namespace INN.Core.Models.DTO
{
    public class ApplicationToken
    {
        public static ApplicationToken EmptyApplicationToken = new ApplicationToken();

        public string ApplicationTokenId { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public double Expires { get; set; }
    }
}
