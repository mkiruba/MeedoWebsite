using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;


namespace Nop.Plugin.Widgets.FacebookPixel
{
    public class FacebookPixelPlugin : BasePlugin, IWidgetPlugin
    {
        public const string FACEBOOKPIXEL_SCRIPT_FIRSTPART = "<!-- Facebook Pixel Code -->\r\n<script>\r\n    !function (f, b, e, v, n, t, s) {\r\n        if (f.fbq) return; n = f.fbq = function () {\r\n            n.callMethod ?\r\n            n.callMethod.apply(n, arguments) : n.queue.push(arguments)\r\n        }; if (!f._fbq) f._fbq = n;\r\n        n.push = n; n.loaded = !0; n.version = '2.0'; n.queue = []; t = b.createElement(e); t.async = !0;\r\n        t.src = v; s = b.getElementsByTagName(e)[0]; s.parentNode.insertBefore(t, s)\r\n    }(window,\r\n    document, 'script', '//connect.facebook.net/en_US/fbevents.js');\r\n\r\n    fbq('init', '1769980523222843')";

        public const string FACEBOOKPIXEL_SCRIPT_LASTPART = "fbq('track', \"PageView\");</script>\r\n<noscript>\r\n    <img height=\"1\" width=\"1\" style=\"display:none\"\r\n         src=\"https://www.facebook.com/tr?id=1769980523222843&ev=PageView&noscript=1\" />\r\n</noscript>\r\n<!-- End Facebook Pixel Code -->";

        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly FacebookPixelSettings _facebookPixelSettings;

        public FacebookPixelPlugin(IWebHelper webHelper, ISettingService settingService, FacebookPixelSettings facebookPixelSettings)
        {
            this._webHelper = webHelper;
            this._settingService = settingService;
            this._facebookPixelSettings = facebookPixelSettings;
        }

        //public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        //{
        //    actionName = "Configure";
        //    controllerName = "WidgetsFacebookPixel";
        //    routeValues = new RouteValueDictionary()
        //    {
        //        { "Namespaces", "Nop.Plugin.Widgets.FacebookPixel.Controllers" },
        //        { "area", null }
        //    };
        //}

        //public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        //{
        //    actionName = "PublicInfo";
        //    controllerName = "WidgetsFacebookPixel";
        //    routeValues = new RouteValueDictionary()
        //    {
        //        { "Namespaces", "Nop.Plugin.Widgets.FacebookPixel.Controllers" },
        //        { "area", null },
        //        { "widgetZone", widgetZone }
        //    };
        //}

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/WidgetsFacebookPixel/Configure";
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>()
            {
                "body_end_html_tag_before",
                "mobile_body_end_html_tag_before"
            };
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <param name="viewComponentName">View component name</param>
        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            viewComponentName = "WidgetsFacebookPixel";
        }

        public override void Install()
        {
            FacebookPixelSettings facebookPixelSetting = new FacebookPixelSettings()
            {
                FacebookPixelScriptFirstPart = "<!-- Facebook Pixel Code -->\r\n<script>\r\n    !function (f, b, e, v, n, t, s) {\r\n        if (f.fbq) return; n = f.fbq = function () {\r\n            n.callMethod ?\r\n            n.callMethod.apply(n, arguments) : n.queue.push(arguments)\r\n        }; if (!f._fbq) f._fbq = n;\r\n        n.push = n; n.loaded = !0; n.version = '2.0'; n.queue = []; t = b.createElement(e); t.async = !0;\r\n        t.src = v; s = b.getElementsByTagName(e)[0]; s.parentNode.insertBefore(t, s)\r\n    }(window,\r\n    document, 'script', '//connect.facebook.net/en_US/fbevents.js');\r\n\r\n    fbq('init', '1769980523222843')",
                FacebookPixelScriptLastPart = "fbq('track', \"PageView\");</script>\r\n<noscript>\r\n    <img height=\"1\" width=\"1\" style=\"display:none\"\r\n         src=\"https://www.facebook.com/tr?id=1769980523222843&ev=PageView&noscript=1\" />\r\n</noscript>\r\n<!-- End Facebook Pixel Code -->"
            };
            this._settingService.SaveSetting<FacebookPixelSettings>(facebookPixelSetting, 0);
            LocalizationExtensions.AddOrUpdatePluginLocaleResource(this, "Plugins.Widgets.FacebookPixel.Configure.Description", "\r\n\r\n<ul>\r\n    <li>The Facebook Pixel code divided into two part </li>\r\n    <li>First parts start from &#60;&#33;&#45;&#45; Facebook Pixel Code &#45;&#45;&#62; and end fbq('init', '111111111111');</li>\r\n    <li>Second parts start from fbq('track', \"PageView\");  and end &#60;&#33;&#45;&#45; End Facebook Pixel Code &#45;&#45;&#62;;</li>\r\n    <li>Two script Part put into the two different text feld</li>\r\n    <li>Save</li>\r\n</ul>\r\n\r\n", null);
            LocalizationExtensions.AddOrUpdatePluginLocaleResource(this, "Plugins.Widgets.FacebookPixel.FacebookPixelScriptFirstPart", "FacebookPixel Script_First", null);
            LocalizationExtensions.AddOrUpdatePluginLocaleResource(this, "Plugins.Widgets.FacebookPixel.FacebookPixelScriptFirstPart.Hint", "Paste the FacebookPixel Script here, and replace hard coded values by tokens. http will be automatically replaced with https if necessary.", null);
            LocalizationExtensions.AddOrUpdatePluginLocaleResource(this, "Plugins.Widgets.FacebookPixel.FacebookPixelScriptLastPart", "FacebookPixel Script_Last", null);
            LocalizationExtensions.AddOrUpdatePluginLocaleResource(this, "Plugins.Widgets.FacebookPixel.FacebookPixelScriptLastPart.Hint", "Paste the FacebookPixel Script Last  here, and replace hard coded values by tokens. http will be automatically replaced with https if necessary.", null);
            base.Install();
        }

        public override void Uninstall()
        {
            this._settingService.DeleteSetting<FacebookPixelSettings>();
            LocalizationExtensions.DeletePluginLocaleResource(this, "Plugins.Widgets.FacebookPixel.FacebookPixelScriptFirstPart");
            LocalizationExtensions.DeletePluginLocaleResource(this, "Plugins.Widgets.FacebookPixel.FacebookPixelScriptFirstPart.Hint");
            LocalizationExtensions.DeletePluginLocaleResource(this, "Plugins.Widgets.FacebookPixel.FacebookPixelScriptLastPart");
            LocalizationExtensions.DeletePluginLocaleResource(this, "Plugins.Widgets.FacebookPixel.FacebookPixelScriptLastPart.Hint");
            base.Uninstall();
        }
    }
}
