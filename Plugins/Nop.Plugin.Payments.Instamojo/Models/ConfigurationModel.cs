using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Payments.Instamojo.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.Instamojo.AuthEndPoint")]
        public string AuthEndPoint { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Instamojo.ClientId")]
        public string ClientId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Instamojo.ClientSecret")]
        public string ClientSecret { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Instamojo.EndPoint")]
        public string EndPoint { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Instamojo.WebHookUrl")]
        public string WebHookUrl { get; set; }

        public ConfigurationModel()
        {
        }
    }
}
