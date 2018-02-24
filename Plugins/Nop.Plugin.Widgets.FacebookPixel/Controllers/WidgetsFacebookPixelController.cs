using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Widgets.FacebookPixel.Extension;
using Nop.Plugin.Widgets.FacebookPixel.Models;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Nop.Plugin.Widgets.FacebookPixel.Controllers
{
    [Area(AreaNames.Admin)]
    public class WidgetsFacebookPixelController : BasePluginController
    {
        private readonly IWorkContext _workContext;        
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;

        public WidgetsFacebookPixelController(IWorkContext workContext, IStoreService storeService, ISettingService settingService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
        }

        [AuthorizeAdmin]
        public IActionResult Configure()
        {
            int storeScope = this.GetActiveStoreScopeConfiguration(this._storeService, this._workContext);
            FacebookPixelSettings facebookPixelSettings = this._settingService.LoadSetting<FacebookPixelSettings>(storeScope);
            ConfigurationModel model = new ConfigurationModel()
            {
                FacebookPixelScriptFirstPart = facebookPixelSettings.FacebookPixelScriptFirstPart,
                FacebookPixelScriptLastPart = facebookPixelSettings.FacebookPixelScriptLastPart,
                DefaultFacebookPixelScriptLastPart = "fbq('track', \"PageView\");</script>\r\n<noscript>\r\n    <img height=\"1\" width=\"1\" style=\"display:none\"\r\n         src=\"https://www.facebook.com/tr?id=1769980523222843&ev=PageView&noscript=1\" />\r\n</noscript>\r\n<!-- End Facebook Pixel Code -->",
                DefaultFacebookPixelScriptFirstPart = "<!-- Facebook Pixel Code -->\r\n<script>\r\n    !function (f, b, e, v, n, t, s) {\r\n        if (f.fbq) return; n = f.fbq = function () {\r\n            n.callMethod ?\r\n            n.callMethod.apply(n, arguments) : n.queue.push(arguments)\r\n        }; if (!f._fbq) f._fbq = n;\r\n        n.push = n; n.loaded = !0; n.version = '2.0'; n.queue = []; t = b.createElement(e); t.async = !0;\r\n        t.src = v; s = b.getElementsByTagName(e)[0]; s.parentNode.insertBefore(t, s)\r\n    }(window,\r\n    document, 'script', '//connect.facebook.net/en_US/fbevents.js');\r\n\r\n    fbq('init', '1769980523222843')",
                ActiveStoreScopeConfiguration = storeScope
            };
            if (storeScope > 0)
            {
                model.FacebookPixelScriptFirstPart_OverrideForStore = this._settingService.SettingExists<FacebookPixelSettings, string>(facebookPixelSettings, 
                    (FacebookPixelSettings x) => x.FacebookPixelScriptFirstPart, storeScope);
                model.FacebookPixelScriptLastPart_OverrideForStore = this._settingService.SettingExists<FacebookPixelSettings, string>(facebookPixelSettings, 
                    (FacebookPixelSettings x) => x.FacebookPixelScriptLastPart, storeScope);
            }
            return base.View("~/Plugins/Widgets.FacebookPixel/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public IActionResult Configure(ConfigurationModel model)
        {
            ActionResult actionResult;
            if (base.ModelState.IsValid)
            {
                int storeScope = this.GetActiveStoreScopeConfiguration(this._storeService, this._workContext);
                FacebookPixelSettings facebookPixelSettings = this._settingService.LoadSetting<FacebookPixelSettings>(storeScope);
                facebookPixelSettings.FacebookPixelScriptFirstPart = model.FacebookPixelScriptFirstPart;
                facebookPixelSettings.FacebookPixelScriptLastPart = model.FacebookPixelScriptLastPart;
                if ((model.FacebookPixelScriptFirstPart_OverrideForStore ? true : storeScope == 0))
                {
                    this._settingService.SaveSetting<FacebookPixelSettings, string>(facebookPixelSettings, (FacebookPixelSettings x) => x.FacebookPixelScriptFirstPart,
                        storeScope, false);
                }
                else if (storeScope > 0)
                {
                    this._settingService.DeleteSetting<FacebookPixelSettings, string>(facebookPixelSettings, (FacebookPixelSettings x) => x.FacebookPixelScriptFirstPart,
                        storeScope);
                }
                if ((model.FacebookPixelScriptLastPart_OverrideForStore ? true : storeScope == 0))
                {
                    this._settingService.SaveSetting<FacebookPixelSettings, string>(facebookPixelSettings, (FacebookPixelSettings x) => x.FacebookPixelScriptLastPart,
                        storeScope, false);
                }
                else if (storeScope > 0)
                {
                    this._settingService.DeleteSetting<FacebookPixelSettings, string>(facebookPixelSettings, (FacebookPixelSettings x) => x.FacebookPixelScriptLastPart, 
                        storeScope);
                }
                this._settingService.ClearCache();
                return Configure();
            }
            else
            {
                return Configure();
            }            
        }              
    }
}
