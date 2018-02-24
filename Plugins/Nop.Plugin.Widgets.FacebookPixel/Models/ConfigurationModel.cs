using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Widgets.FacebookPixel.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }
       
        public string DefaultFacebookPixelScriptFirstPart { get; set; }

        public string DefaultFacebookPixelScriptLastPart { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.FacebookPixel.FacebookPixelScriptFirstPart")]
        public string FacebookPixelScriptFirstPart { get; set; }

        public bool FacebookPixelScriptFirstPart_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.FacebookPixel.FacebookPixelScriptLastPart")]
        public string FacebookPixelScriptLastPart { get; set; }

        public bool FacebookPixelScriptLastPart_OverrideForStore { get; set; }

        public ConfigurationModel()
        {
        }
    }
}
