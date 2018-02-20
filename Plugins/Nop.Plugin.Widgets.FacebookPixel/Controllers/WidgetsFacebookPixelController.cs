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

        private readonly IStoreContext _storeContext;

        private readonly IStoreService _storeService;

        private readonly ISettingService _settingService;

        private readonly IOrderService _orderService;

        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        private readonly ICategoryService _categoryService;

        private readonly ILogger _logger;

        private readonly IProductService _productService;

        private readonly StoreInformationSettings _storeInformationSettings;

        private readonly IPriceCalculationService _priceCalculationService;

        private readonly IPriceFormatter _priceFormatter;

        private readonly IPermissionService _permissionService;

        private readonly ILocalizationService _localizationService;

        private readonly ITaxService _taxService;

        private readonly ICurrencyService _currencyService;

        private readonly CultureInfo _usCulture;

        public WidgetsFacebookPixelController(IWorkContext workContext, IStoreContext storeContext, IStoreService storeService, ISettingService settingService, IOrderService orderService, IOrderTotalCalculationService orderTotalCalculationService, ICategoryService categoryService, ILogger logger, StoreInformationSettings storeInformationSettings, IProductService productService, IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter, IPermissionService permissionService, ILocalizationService localizationService, ITaxService taxService, ICurrencyService currencyService)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._orderService = orderService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._categoryService = categoryService;
            this._logger = logger;
            this._storeInformationSettings = storeInformationSettings;
            this._usCulture = new CultureInfo("en-US");
            this._productService = productService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._localizationService = localizationService;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._permissionService = permissionService;
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

        private string GetFacebookPixelScriptFirstPart(string pageType, string productId, string categoryName, IList<ShoppingCartItem> cart)
        {
            FacebookPixelSettings facebookPixelSettings = this._settingService.LoadSetting<FacebookPixelSettings>(this._storeContext.CurrentStore.Id);
            string str = this.InjectValuesInScript(facebookPixelSettings.FacebookPixelScriptFirstPart, "0", pageType, null, productId, categoryName, null, cart);
            return str;
        }

        private string GetFacebookPixelScriptLastPart(string pageType, string productId, string categoryName, IList<ShoppingCartItem> cart)
        {
            FacebookPixelSettings facebookPixelSettings = this._settingService.LoadSetting<FacebookPixelSettings>(this._storeContext.CurrentStore.Id);
            string str = this.InjectValuesInScript(facebookPixelSettings.FacebookPixelScriptLastPart, "0", pageType, null, productId, categoryName, null, cart);
            return str;
        }

        private Order GetLastOrder()
        {
            DateTime? nullable = null;
            DateTime? nullable1 = nullable;
            nullable = null;
            Order order = this._orderService.SearchOrders(this._storeContext.CurrentStore.Id, 0, this._workContext.CurrentCustomer.Id).FirstOrDefault<Order>();
            return order;
        }

        private string InjectValuesInScript(string script, string conversionId, string pageType, string label, string productId, string categoryName, 
            Order order, IList<ShoppingCartItem> cart)
        {
            List<DiscountForCaching> orderSubTotalAppliedDiscount = null;
            string totalFormated = this.ToScriptFormat(decimal.Zero);
            string prodIdsFormated = productId;
            if (productId != null)
            {
                try
                {
                    int id = int.Parse(productId);
                    Product product = this._productService.GetProductById(id);
                    string price = product.PreparePrice(this._workContext, this._storeContext, this._productService, this._priceCalculationService,
                        this._priceFormatter, this._permissionService, this._localizationService, this._taxService, this._currencyService);
                    decimal value = new decimal();
                    try
                    {
                        value = Convert.ToDecimal(price);
                    }
                    catch (Exception exception)
                    {
                        value = new decimal();
                    }
                    totalFormated = this.ToScriptFormat(value);
                }
                catch
                {
                    prodIdsFormated = string.Concat("'", prodIdsFormated, "'");
                }
            }
            if (order != null)
            {
                totalFormated = this.ToScriptFormat(order.OrderTotal);
                prodIdsFormated = this.ToScriptFormat((
                    from c in order.OrderItems
                    select c.ProductId.ToString()).ToArray<string>());
            }
            if (cart != null)
            {
                decimal subTotalWithoutDiscountBase = new decimal();
                if (cart.Count > 0)
                {
                    decimal orderSubTotalDiscountAmountBase = new decimal();
                    decimal subTotalWithDiscountBase = new decimal();
                    bool subTotalIncludingTax = this._workContext.TaxDisplayType == 0;
                    this._orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax, out orderSubTotalDiscountAmountBase, 
                        out orderSubTotalAppliedDiscount, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
                    totalFormated = this.ToScriptFormat(subTotalWithoutDiscountBase);
                    prodIdsFormated = this.ToScriptFormat((
                        from c in cart
                        select c.ProductId.ToString()).ToArray<string>());
                }
            }
            if (base.HttpContext.Request.Scheme == "https")
            {
                script = script.Replace("http://", "https://");
            }
            return string.Concat(script, "\n");
        }

        //[ChildActionOnly]
        //public ActionResult PublicInfo(string widgetZone)
        //{
        //    string globalScript = "";
        //    RouteData routeData = ((Page)base.HttpContext.CurrentHandler).RouteData;
        //    string controller = routeData.Values["controller"].ToString().ToLowerInvariant();
        //    string action = routeData.Values["action"].ToString().ToLowerInvariant();
        //    try
        //    {
        //        string str = controller;
        //        if (str == "home")
        //        {
        //            if (action.Equals("index", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("home", null, null, null));
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("home", null, null, null));
        //            }
        //        }
        //        else if (str == "catalog")
        //        {
        //            if (action.Equals("Category", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Category", null, null, null));
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Category", null, null, null));
        //            }
        //            else if (action.Equals("Manufacturer", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Manufacturer", null, null, null));
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Manufacturer", null, null, null));
        //            }
        //            else if (action.Equals("Vendor", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Vendor", null, null, null));
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Vendor", null, null, null));
        //            }
        //            else if (action.Equals("search", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("search", null, null, null));
        //                globalScript = string.Concat(globalScript, "fbq('track', 'Search');");
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("search", null, null, null));
        //            }
        //        }
        //        else if (str == "product")
        //        {
        //            if (action.Equals("ProductDetails", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("ProductDetails", null, null, null));
        //                globalScript = string.Concat(globalScript, "fbq('track', 'ViewContent'); \r\n");
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("ProductDetails", null, null, null));
        //            }
        //        }
        //        else if (str == "shoppingcart")
        //        {
        //            if (action.Equals("Cart", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Cart", null, null, null));
        //                globalScript = string.Concat(globalScript, "fbq('track', 'AddToCart');");
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Cart", null, null, null));
        //            }
        //            else if (action.Equals("Wishlist", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Wishlist", null, null, null));
        //                globalScript = string.Concat(globalScript, "fbq('track', 'AddToWishlist');");
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Wishlist", null, null, null));
        //            }
        //        }
        //        else if (str != "checkout")
        //        {
        //            if (str != "customer")
        //            {
        //                globalScript = globalScript ?? "";
        //            }
        //            else if (action.Equals("Login", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Login", null, null, null));
        //                globalScript = string.Concat(globalScript, "fbq('track', 'Lead');");
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Login", null, null, null));
        //            }
        //            else if (action.Equals("RegisterResult", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Register", null, null, null));
        //                globalScript = string.Concat(globalScript, "fbq('track', 'CompleteRegistration');");
        //                globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Register", null, null, null));
        //            }
        //        }
        //        else if (action.Equals("OpcSavePaymentInfo", StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("OpcSavePaymentInfo", null, null, null));
        //            globalScript = string.Concat(globalScript, "fbq('track', 'AddPaymentInfo');");
        //            globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("OpcSavePaymentInfo", null, null, null));
        //        }
        //        else if (action.Equals("OnePageCheckout", StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("OnePageCheckout", null, null, null));
        //            globalScript = string.Concat(globalScript, "fbq('track', 'InitiateCheckout');");
        //            globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("OnePageCheckout", null, null, null));
        //        }
        //        else if (action.Equals("Completed", StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Completed", null, null, null));
        //            globalScript = string.Concat(globalScript, "fbq('track', 'Purchase', {value: '1.00', currency: 'USD'});");
        //            globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Completed", null, null, null));
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Exception ex = exception;
        //        this._logger.InsertLog(40, "Error creating scripts for google adwords convertion tracking", ex.ToString(), null);
        //    }
        //    return base.Content(globalScript);
        //}

        //private string RemoveLastComma(string value)
        //{
        //    int index = 0;
        //    for (int i = 0; i < value.Length; i++)
        //    {
        //        if (value[i] == ',')
        //        {
        //            index = i;
        //        }
        //    }
        //    if (index > 0)
        //    {
        //        value = value.Remove(index, 1);
        //    }
        //    return value;
        //}

        private string ToScriptFormat(decimal value)
        {
            return value.ToString("0.00", this._usCulture);
        }

        private string ToScriptFormat(string[] strings)
        {
            string str;
            if ((strings == null ? false : strings.Length != 0))
            {
                StringBuilder strBuilder = new StringBuilder((int)strings.Length * 3 + 5);
                strBuilder.Append(strings[0]);
                if ((int)strings.Length > 1)
                {
                    strBuilder.Insert(0, "[");
                    for (int i = 1; i < (int)strings.Length; i++)
                    {
                        strBuilder.Append(",");
                        strBuilder.Append(strings[i]);
                    }
                    strBuilder.Append("]");
                }
                str = strBuilder.ToString();
            }
            else
            {
                str = "''";
            }
            return str;
        }
    }
}
