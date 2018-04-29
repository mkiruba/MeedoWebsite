using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Payments.Instamojo.Models
{
    public class ConfigurationModel : BaseNopModel
    {       
        [NopResourceDisplayName("Plugins.Payments.Instamojo.AuthToken")]
        public string AuthToken { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Instamojo.ApiKey")]
        public string ApiKey { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Instamojo.EndPoint")]
        public string EndPoint { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Instamojo.WebHookUrl")]
        public string WebHookUrl { get; set; }

        public ConfigurationModel()
        {
        }
    }
}
