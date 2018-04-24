using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Instamojo.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Sp.Agent.Core.Execution;

namespace Nop.Plugin.Payments.Instamojo
{
    /// <summary>
    /// Payu payment processor
    /// </summary>
    public class InstamojoProcessor : BasePlugin, IPaymentMethod
    {
        private readonly InstamojoSettings _instamojoSettings;

        private readonly ISettingService _settingService;

        private readonly ICurrencyService _currencyService;

        private readonly CurrencySettings _currencySettings;

        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;

        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }

        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.Instamojo.PaymentMethodDescription"); }
        }

        public InstamojoProcessor(InstamojoSettings instamojoSettings, ISettingService settingService, ICurrencyService currencyService, ILocalizationService localizationService, CurrencySettings currencySettings, IWebHelper webHelper)
        {
            this._instamojoSettings = instamojoSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._localizationService = localizationService;
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            CancelRecurringPaymentResult result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        public bool CanRePostProcessPayment(Order order)
        {
            bool V_2;
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }
            if (order.PaymentStatus == PaymentStatus.Pending)
            {
                V_2 = ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes >= 1 ? true : false);
            }
            else
            {
                V_2 = false;
            }
            return V_2;
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            CapturePaymentResult result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return decimal.Zero;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentInstamojo/Configure";
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "Instamojo";
            routeValues = new RouteValueDictionary()
            {
                { "Namespaces", "Nop.Plugin.Payments.Instamojo.Controllers" },
                { "area", null }
            };
        }

        //public Type GetControllerType()
        //{
        //    return typeof(InstamojoController);
        //}

        //public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        //{
        //    actionName = "PaymentInfo";
        //    controllerName = "Instamojo";
        //    routeValues = new RouteValueDictionary()
        //    {
        //        { "Namespaces", "Antargyan.Plugin.Payments.Instamojo.Controllers" },
        //        { "area", null }
        //    };
        //}

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        public override void Install()
        {
            InstamojoSettings instamojoSetting = new InstamojoSettings()
            {
                ClientId = "",
                ClientSecret = "",
                EndPoint = "",
                AuthEndPoint = "",
                WebHookUrl = ""
            };
            this._settingService.SaveSetting<InstamojoSettings>(instamojoSetting, 0);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.RedirectionTip", "You will be redirected to Instamojo site to complete the order.", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.ClientId", "Client ID", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.ClientId.Hint", "Enter Client ID.", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.ClientSecret", "Client Secret", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.ClientSecret.Hint", "Enter client secret.", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.EndPoint", "End Point", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.EndPoint.Hint", "End Point.", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.AuthEndPoint", "Auth End Point", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.AuthEndPoint.Hint", "Auth End Point", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.PrivateApiKey", "PrivateApiKey", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.PrivateApiKey.Hint", "Enter PrivateApiKey.", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.PrivateAuthToken", "PrivateAuthToken", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.PrivateAuthToken.Hint", "Enter PrivateAuthToken.", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.PrivateSalt", "PrivateSalt", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.PrivateSalt.Hint", "Enter PrivateSalt", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.WebHookUrl", "WebHookUrl", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.WebHookUrl.Hint", "Enter WebHookUrl", null);
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.PaymentMethodDescription", "Pay by \"Instamojo\"");
            base.Install();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            object[] objArray = new object[] { postProcessPaymentRequest };
            PermutedExecutionServices2.ExecuteMethod(this, "31877a8cc032424ba9869314152add1b", "84515A2F2188489FA43F609922C17C0C", objArray, null, null);
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            ProcessPaymentResult result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            ProcessPaymentResult result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            RefundPaymentResult result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        public override void Uninstall()
        {
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.ClientId");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.ClientId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.ClientSecret");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.ClientSecret.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.EndPoint");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.EndPoint.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.AuthEndPoint");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.AuthEndPoint.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.PrivateApiKey");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.PrivateApiKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.PrivateAuthToken");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.PrivateAuthToken.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.PrivateSalt");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.PrivateSalt.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.WebHookUrl");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.WebHookUrl.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.PaymentMethodDescription");
            base.Uninstall();
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            VoidPaymentResult result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentInstamojo";
        }
    }
}
