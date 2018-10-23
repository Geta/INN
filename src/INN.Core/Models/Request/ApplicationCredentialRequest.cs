using Refit;

namespace INN.Core.Models.Request
{
    public class ApplicationCredentialRequest
    {
        public ApplicationCredentialRequest(string applicationId, string applicationSecret, string applicationName)
        {
            ApplicationCredential = ApplicationTokenRequestXml(applicationId, applicationSecret, applicationName);
        }

        private string ApplicationTokenRequestXml(string applicationId, string applicationSecret, string applicationName)
        {
            return $"<applicationcredential><params><applicationID>{applicationId}</applicationID><applicationSecret>{applicationSecret}</applicationSecret><applicationName>{applicationName}</applicationName></params></applicationcredential>";
        }

        [AliasAs("applicationcredential")]
        public string ApplicationCredential { get; set; }
    }
}
