using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using InstamojoAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Instamojo.Controllers;
using Nop.Plugin.Payments.Instamojo.Models;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Web.Framework;

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
        private readonly IEmailSender _emailSender;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PaymentMethodType PaymentMethodType { get { return PaymentMethodType.Redirection; } } 
        
        public RecurringPaymentType RecurringPaymentType { get { return RecurringPaymentType.NotSupported; } } 
       
        public bool SkipPaymentInfo { get { return false; } } 
        
        public bool SupportCapture { get { return false; } } 
        
        public bool SupportPartiallyRefund { get { return false; } } 
        
        public bool SupportRefund { get { return false; } } 
        
        public bool SupportVoid { get { return false; } } 
        
        public string PaymentMethodDescription { get { return _localizationService.GetResource("Plugins.Payments.Instamojo.PaymentMethodDescription"); } } 
       
        public InstamojoProcessor(InstamojoSettings instamojoSettings, ISettingService settingService, 
            ICurrencyService currencyService, ILocalizationService localizationService, 
            CurrencySettings currencySettings, IWebHelper webHelper, IEmailSender emailSender, 
            IEmailAccountService emailAccountService, EmailAccountSettings emailAccountSettings,
            ICustomerService customerService, IHttpContextAccessor httpContextAccessor)
        {
            this._instamojoSettings = instamojoSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._localizationService = localizationService;
            this._emailSender = emailSender;
            this._emailAccountService = emailAccountService;
            this._emailAccountSettings = emailAccountSettings;
            this._customerService = customerService;
            this._httpContextAccessor = httpContextAccessor;
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            CancelRecurringPaymentResult result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        public bool CanRePostProcessPayment(Core.Domain.Orders.Order order)
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
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Instamojo.PaymentFailed", "Payment been failed. Please try again.");
            base.Install();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //object[] objArray = new object[] { postProcessPaymentRequest };
            //PermutedExecutionServices2.ExecuteMethod(this, "31877a8cc032424ba9869314152add1b", "84515A2F2188489FA43F609922C17C0C", objArray, null, null);
            string Insta_client_id = _instamojoSettings.ClientId,// "tmLkZZ0zV41nJwhayBGBOI4m4I7bH55qpUBdEXGS",
                  Insta_client_secret = _instamojoSettings.ClientSecret,// "IDejdccGqKaFlGav9bntKULvMZ0g7twVFolC9gdrh9peMS0megSFr7iDpWwWIDgFUc3W5SlX99fKnhxsoy6ipdAv9JeQwebmOU6VRvOEQnNMWwZnWglYmDGrfgKRheXs",
                  Insta_Endpoint = _instamojoSettings.EndPoint,//InstamojoConstants.INSTAMOJO_API_ENDPOINT,
                  Insta_Auth_Endpoint = _instamojoSettings.AuthEndPoint;//InstamojoConstants.INSTAMOJO_AUTH_ENDPOINT;
            
            InstamojoAPI.Instamojo objClass = InstamojoImplementation.getApi(Insta_client_id, Insta_client_secret, Insta_Endpoint, Insta_Auth_Endpoint);
            
            //  Create Payment Order
            PaymentOrder objPaymentRequest = new PaymentOrder()
            {
                name = postProcessPaymentRequest.Order.BillingAddress.FirstName,
                email = postProcessPaymentRequest.Order.BillingAddress.Email,
                phone = postProcessPaymentRequest.Order.BillingAddress.PhoneNumber,
                amount = double.Parse(postProcessPaymentRequest.Order.OrderTotal.ToString(new CultureInfo("en-US", false).NumberFormat), CultureInfo.InvariantCulture),
                currency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode,
                transaction_id = postProcessPaymentRequest.Order.Id.ToString(),
                //redirect_url = _webHelper.GetStoreLocation(false).Contains("localhost") ? 
                //_webHelper.GetStoreLocation(false).Replace("localhost", "meedo.in"):
                //_webHelper.GetStoreLocation(false) + "Plugins/PaymentInstamojo/Return",
                webhook_url = _webHelper.GetStoreLocation(false).Contains("localhost") ?
                _webHelper.GetStoreLocation(false).Replace("localhost", "meedo.in") :
                _webHelper.GetStoreLocation(false) + "Plugins/PaymentInstamojo/Return",
                redirect_url = _webHelper.GetStoreLocation(false) + "Plugins/PaymentInstamojo/Return",
                //webhook_url = _webHelper.GetStoreLocation(false) + "Plugins/PaymentInstamojo/Return",
                description = $"Name-{postProcessPaymentRequest.Order.BillingAddress.FirstName},OrderId-{postProcessPaymentRequest.Order.Id}"
            };
            //Extra POST parameters 
            try
            {
                CreatePaymentOrderResponse objPaymentResponse = objClass.createNewPaymentRequest(objPaymentRequest);

                _httpContextAccessor.HttpContext.Response.Redirect(objPaymentResponse.payment_options.payment_url);
            }
            catch (Exception ex)
            {
                //Send email SendPaymentFailedStoreOwnerNotification
                SendEmail(ex.ToString(), postProcessPaymentRequest);
                throw new Exception(ex.Message);
            }            
        }

        public void SendEmail(string errorMessage, PostProcessPaymentRequest postProcessPaymentRequest)
        {          
            var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount != null)
            {
                var subject = "Meedo - Important Payment failed in Instamojo";
                var body = $"Hi, \n Payment been failed for the genuine payment made by the customer. Please be in touch with customer as soon as possible.\n" +
                    $"OrderId - {postProcessPaymentRequest.Order.Id}\n CustomerName - {postProcessPaymentRequest.Order.BillingAddress.FirstName}\n " +
                    $"CustomerEmailAddress - {postProcessPaymentRequest.Order.BillingAddress.Email}\n CustomerPhoneNumber - {postProcessPaymentRequest.Order.BillingAddress.PhoneNumber}\n" +
                    $"Exception - {errorMessage}";
                _emailSender.SendEmail(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, "meedoindia@gmail.com", null);
            }           
        }
        public void SendEmail(string errorMessage, ProcessPaymentRequest processPaymentRequest, Customer customer)
        {
            var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount != null)
            {
                var subject = "Meedo - Important Payment failed in Instamojo";
                var body = $"Hi, \n Payment been failed for the genuine payment made by the customer. Please be in touch with customer as soon as possible.\n" +
                    $"OrderId - {processPaymentRequest.InitialOrderId}\n CustomerName - {customer?.BillingAddress.FirstName}\n " +
                    $"CustomerEmailAddress - {customer?.BillingAddress.Email}\n CustomerPhoneNumber - {customer?.BillingAddress.PhoneNumber}\n" +
                    $"Exception - {errorMessage}";
                _emailSender.SendEmail(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, "meedoindia@gmail.com", null);
            }
        }


        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            //string Insta_client_id = _instamojoSettings.ClientId,// "tmLkZZ0zV41nJwhayBGBOI4m4I7bH55qpUBdEXGS",
            //    Insta_client_secret = _instamojoSettings.ClientSecret,// "IDejdccGqKaFlGav9bntKULvMZ0g7twVFolC9gdrh9peMS0megSFr7iDpWwWIDgFUc3W5SlX99fKnhxsoy6ipdAv9JeQwebmOU6VRvOEQnNMWwZnWglYmDGrfgKRheXs",
            //    Insta_Endpoint = _instamojoSettings.EndPoint,//InstamojoConstants.INSTAMOJO_API_ENDPOINT,
            //    Insta_Auth_Endpoint = _instamojoSettings.AuthEndPoint;//InstamojoConstants.INSTAMOJO_AUTH_ENDPOINT;

            //InstamojoAPI.Instamojo objClass = InstamojoImplementation.getApi(Insta_client_id, Insta_client_secret, Insta_Endpoint, Insta_Auth_Endpoint);
            //var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            //if (customer == null)
            //    throw new Exception("Customer cannot be loaded");

            ////  Create Payment Order
            //PaymentOrder objPaymentRequest = new PaymentOrder()
            //{
            //    name = customer.BillingAddress.FirstName,
            //    email = customer.BillingAddress.Email,
            //    phone = customer.BillingAddress.PhoneNumber,
            //    amount = double.Parse(processPaymentRequest.OrderTotal.ToString(new CultureInfo("en-US", false).NumberFormat), CultureInfo.InvariantCulture),
            //    currency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode,
            //    transaction_id = processPaymentRequest.OrderGuid.ToString(),
            //    redirect_url = _webHelper.GetStoreLocation(false).Contains("localhost") ?
            //    _webHelper.GetStoreLocation(false).Replace("localhost", "meedo.in") :
            //    _webHelper.GetStoreLocation(false) + "Plugins/PaymentInstamojo/Return",
            //    webhook_url = _webHelper.GetStoreLocation(false).Contains("localhost") ?
            //    _webHelper.GetStoreLocation(false).Replace("localhost", "meedo.in") :
            //    _webHelper.GetStoreLocation(false) + "Plugins/PaymentInstamojo/Return",
            //    description = $"Name-{customer.BillingAddress.FirstName},OrderId-{processPaymentRequest.InitialOrderId}"
            //};
            ////Extra POST parameters 
            //try
            //{
            //    CreatePaymentOrderResponse objPaymentResponse = objClass.createNewPaymentRequest(objPaymentRequest);
            //    _httpContextAccessor.HttpContext.Response.Redirect(objPaymentResponse.payment_options.payment_url);
            //    return;
            //    //var model = new PaymentInfoModel
            //    //{
            //    //    InstamojoLink = objPaymentResponse.payment_options.payment_url
            //    //};
            //    //var remotePostHelper = new RemotePost
            //    //{
            //    //    FormName = "InstamojoForm",
            //    //    Url = objPaymentResponse.payment_options.payment_url,
            //    //    Method = "get"
            //    //};
            //    //remotePostHelper.Embed();
            //}
            //catch (Exception ex)
            //{
            //    //Send email SendPaymentFailedStoreOwnerNotification
            //    SendEmail(ex.ToString(), processPaymentRequest, customer);
            //    throw new Exception(ex.Message);
            //}

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
            this.DeletePluginLocaleResource("Plugins.Payments.Instamojo.PaymentFailed");
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
