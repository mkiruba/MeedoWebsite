using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Instamojo
{
    public class InstamojoSettings : ISettings
    {
        public string AuthToken { get; set; }
        
        public string ApiKey { get; set; }
        
        public string EndPoint { get; set; }

        public string WebHookUrl { get; set; }
    }
}
