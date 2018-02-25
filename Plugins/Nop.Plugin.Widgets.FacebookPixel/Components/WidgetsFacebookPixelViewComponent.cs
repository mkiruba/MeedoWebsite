using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Widgets.FacebookPixel.Extension;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.FacebookPixel.Components
{
    [ViewComponent(Name = "WidgetsFacebookPixel")]
    public class WidgetsFacebookPixelViewComponent : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ILogger _logger;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly CultureInfo _usCulture;

        public WidgetsFacebookPixelViewComponent(
            IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            IOrderService orderService,
            IProductService productService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceCalculationService priceCalculationService, 
            IPriceFormatter priceFormatter, 
            IPermissionService permissionService, 
            ILocalizationService localizationService, 
            ITaxService taxService, 
            ICurrencyService currencyService,
            ILogger logger)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._settingService = settingService;
            this._orderService = orderService;
            this._productService = productService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._permissionService = permissionService;
            this._localizationService = localizationService;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._logger = logger;
            this._usCulture = new CultureInfo("en-US");
        }
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var globalScript = GetEcommerceScript();
            return View("~/Plugins/Widgets.FacebookPixel/Views/PublicInfo.cshtml", globalScript);
        }

        private string GetEcommerceScript()
        {
            string globalScript = "";
            RouteData routeData = this.RouteData;// ((Page)base.HttpContext.CurrentHandler).RouteData;
            string controller = routeData.Values["controller"].ToString().ToLowerInvariant();
            string action = routeData.Values["action"].ToString().ToLowerInvariant();
            try
            {
                string str = controller;
                if (str == "home")
                {
                    if (action.Equals("index", StringComparison.InvariantCultureIgnoreCase))
                    {
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("home", null, null, null));
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("home", null, null, null));
                    }
                }
                else if (str == "catalog")
                {
                    if (action.Equals("Category", StringComparison.InvariantCultureIgnoreCase))
                    {
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Category", null, null, null));
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Category", null, null, null));
                    }
                    else if (action.Equals("Manufacturer", StringComparison.InvariantCultureIgnoreCase))
                    {
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Manufacturer", null, null, null));
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Manufacturer", null, null, null));
                    }
                    else if (action.Equals("Vendor", StringComparison.InvariantCultureIgnoreCase))
                    {
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Vendor", null, null, null));
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Vendor", null, null, null));
                    }
                    else if (action.Equals("search", StringComparison.InvariantCultureIgnoreCase))
                    {
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("search", null, null, null));
                        globalScript = string.Concat(globalScript, "fbq('track', 'Search');");
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("search", null, null, null));
                    }
                }
                else if (str == "product")
                {
                    if (action.Equals("ProductDetails", StringComparison.InvariantCultureIgnoreCase))
                    {
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("ProductDetails", null, null, null));
                        globalScript = string.Concat(globalScript, "fbq('track', 'ViewContent'); \r\n");
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("ProductDetails", null, null, null));
                    }
                }
                else if (str == "shoppingcart")
                {
                    if (action.Equals("Cart", StringComparison.InvariantCultureIgnoreCase))
                    {
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Cart", null, null, null));
                        globalScript = string.Concat(globalScript, "fbq('track', 'AddToCart');");
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Cart", null, null, null));
                    }
                    else if (action.Equals("Wishlist", StringComparison.InvariantCultureIgnoreCase))
                    {
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Wishlist", null, null, null));
                        globalScript = string.Concat(globalScript, "fbq('track', 'AddToWishlist');");
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Wishlist", null, null, null));
                    }
                }
                else if (str != "checkout")
                {
                    if (str != "customer")
                    {
                        globalScript = globalScript ?? "";
                    }
                    else if (action.Equals("Login", StringComparison.InvariantCultureIgnoreCase))
                    {
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Login", null, null, null));
                        globalScript = string.Concat(globalScript, "fbq('track', 'Lead');");
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Login", null, null, null));
                    }
                    else if (action.Equals("RegisterResult", StringComparison.InvariantCultureIgnoreCase))
                    {
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Register", null, null, null));
                        globalScript = string.Concat(globalScript, "fbq('track', 'CompleteRegistration');");
                        globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Register", null, null, null));
                    }
                }
                else if (action.Equals("OpcSavePaymentInfo", StringComparison.InvariantCultureIgnoreCase))
                {
                    globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("OpcSavePaymentInfo", null, null, null));
                    globalScript = string.Concat(globalScript, "fbq('track', 'AddPaymentInfo');");
                    globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("OpcSavePaymentInfo", null, null, null));
                }
                else if (action.Equals("OnePageCheckout", StringComparison.InvariantCultureIgnoreCase))
                {
                    globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("OnePageCheckout", null, null, null));
                    globalScript = string.Concat(globalScript, "fbq('track', 'InitiateCheckout');");
                    globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("OnePageCheckout", null, null, null));
                }
                else if (action.Equals("Completed", StringComparison.InvariantCultureIgnoreCase))
                {
                    globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptFirstPart("Completed", null, null, null));
                    globalScript = string.Concat(globalScript, "fbq('track', 'Purchase', {value: '1.00', currency: 'USD'});");
                    globalScript = string.Concat(globalScript, this.GetFacebookPixelScriptLastPart("Completed", null, null, null));
                }                
            }
            catch (Exception exception)
            {
                Exception ex = exception;
                this._logger.InsertLog(LogLevel.Error, "Error creating scripts for google adwords convertion tracking", ex.ToString(), null);
            }
            return globalScript;
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

        private Order GetLastOrder()
        {
            DateTime? nullable = null;
            DateTime? nullable1 = nullable;
            nullable = null;
            Order order = this._orderService.SearchOrders(this._storeContext.CurrentStore.Id, 0, this._workContext.CurrentCustomer.Id).FirstOrDefault<Order>();
            return order;
        }

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
