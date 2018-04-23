using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Instamojo
{
    public class InstamojoSettings : ISettings
    {
        public string AuthEndPoint { get; set; }
        
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string EndPoint { get; set; }

        public string WebHookUrl { get; set; }
    }
}
