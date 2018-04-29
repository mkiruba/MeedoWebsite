using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Instamojo.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Nop.Plugin.Payments.Instamojo.Controllers
{
    public class PaymentInstamojoController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly InstamojoSettings _instamojoSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;

        public PaymentInstamojoController(ISettingService settingService, IPaymentService paymentService,
            IOrderService orderService, IOrderProcessingService orderProcessingService,
            InstamojoSettings instamojoSettings, PaymentSettings paymentSettings,
            ILogger logger, ILocalizationService localizationService, IEmailSender emailSender,
            IEmailAccountService emailAccountService, EmailAccountSettings emailAccountSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._instamojoSettings = instamojoSettings;
            this._paymentSettings = paymentSettings;
            this._logger = logger;
            this._localizationService = localizationService;
            this._emailSender = emailSender;
            this._emailAccountService = emailAccountService;
            this._emailAccountSettings = emailAccountSettings;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public ActionResult Configure()
        {
            ConfigurationModel model = new ConfigurationModel()
            {
                AuthToken = this._instamojoSettings.AuthToken,
                ApiKey = this._instamojoSettings.ApiKey,
                EndPoint = this._instamojoSettings.EndPoint,
                WebHookUrl = this._instamojoSettings.WebHookUrl
            };
            return View("~/Plugins/Payments.Instamojo/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public ActionResult Configure(ConfigurationModel model)
        {
            ActionResult V_1;
            if (ModelState.IsValid)
            {
                this._instamojoSettings.AuthToken = model.AuthToken;
                this._instamojoSettings.ApiKey = model.ApiKey;
                this._instamojoSettings.EndPoint = model.EndPoint;
                this._instamojoSettings.WebHookUrl = model.WebHookUrl;
                this._settingService.SaveSetting<InstamojoSettings>(this._instamojoSettings, 0);
                V_1 = View("~/Plugins/Payments.Instamojo/Views/Configure.cshtml", model);
            }
            else
            {
                V_1 = Configure();
            }
            return V_1;
        }

        //public static List<PropertyInfo> GetDifferences(PaymentOrderDetailsResponse r1, PaymentOrderDetailsResponse r2)
        //{
        //    List<PropertyInfo> differences = new List<PropertyInfo>();
        //    PropertyInfo[] V_1 = r1.GetType().GetProperties();
        //    for (int V_2 = 0; V_2 < (int)V_1.Length; V_2++)
        //    {
        //        PropertyInfo property = V_1[V_2];
        //        if (!property.GetValue(r1, null).Equals(property.GetValue(r2, null)))
        //        {
        //            differences.Add(property);
        //        }
        //    }
        //    return differences;
        //}

        //[NonAction]
        //public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        //{
        //    return new ProcessPaymentRequest();
        //}

        //[ChildActionOnly]
        //public ActionResult PaymentInfo()
        //{
        //    return base.View("~/Plugins/Payments.Instamojo/Views/Instamojo/PaymentInfo.cshtml", new PaymentInfoModel());
        //}

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //[ValidateInput(false)]
        public ActionResult Return(string id, string payment_request_id, string payment_id)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Instamojo") as InstamojoProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("Instamojo module cannot be loaded");
            try
            {
                var requestUrl = $"{_instamojoSettings.EndPoint}/{ payment_request_id}";// $"https://www.instamojo.com/api/1.1/payment-requests/{transaction_id}";
                JObject rss;
                string paymentStatus;
                string responseString;
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-Api-Key", _instamojoSettings.ApiKey);// "abd485fcdfe77d7fb1925ce4899e13fb");
                    client.DefaultRequestHeaders.Add("X-Auth-Token", _instamojoSettings.AuthToken);// "0057118ff55d97b222d7977a1a5f7ce0");
                    var task = client.GetAsync(requestUrl);
                    task.Wait();
                    responseString = task.Result.Content.ReadAsStringAsync().Result;
                    rss = JObject.Parse(responseString);
                }

                if (rss["payment_request"]["payments"] != null)
                {
                    paymentStatus = rss["payment_request"]["payments"].Select(x => (string)x["status"]).FirstOrDefault();

                    if (paymentStatus != "failed")
                    {
                        /* 
                            Here you need to put in the routines for a successful 
                             transaction such as sending an email to customer,
                             setting database status, informing logistics etc etc
                        */
                        var order = _orderService.GetOrderById(Convert.ToInt32(rss["payment_request"]["purpose"]));
                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            _orderProcessingService.MarkOrderAsPaid(order);
                        }
                        //Thank you for shopping with us. Your credit card has been charged and your transaction is successful
                        return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                    }
                    else
                    {
                        /*
                            Here you need to put in the routines for a failed
                            transaction such as sending an email to customer
                            setting database status etc etc
                        */
                        SendEmail(responseString, rss);
                        ErrorNotification(_localizationService.GetResource("Plugins.Payments.Instamojo.PaymentFailed"), true);
                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
                }
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.Error("Error in Instamojo Payment Return.", ex);
                return RedirectToAction("Index", "Home", new { area = "" });
            }          
        }
        public void SendEmail(string errorMessage, JObject rss)
        {
            var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount != null)
            {
                var subject = "Meedo - Important Payment failed in Instamojo";
                var body = $"Hi, \n Payment been failed for the genuine payment made by the customer. Please be in touch with customer as soon as possible.\n" +
                    $"OrderId - {rss["payment_request"]["id"]}\n CustomerName - {rss["payment_request"]["buyer_name"]}\n " +
                    $"CustomerEmailAddress - {rss["payment_request"]["email"]}\n CustomerPhoneNumber - {rss["payment_request"]["phone"]}\n" +
                    $"Exception - {errorMessage}";
                _emailSender.SendEmail(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, "meedoindia@gmail.com", null);
            }
        }
        //public ActionResult Return(string payment_id, string payment_request_id)
        //{
        //    var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Instamojo") as InstamojoProcessor;
        //    if (processor == null ||
        //        !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
        //        throw new NopException("Payu module cannot be loaded");

        //    string Insta_client_id = _instamojoSettings.ClientId,// "tmLkZZ0zV41nJwhayBGBOI4m4I7bH55qpUBdEXGS",
        //          Insta_client_secret = _instamojoSettings.ClientSecret,// "IDejdccGqKaFlGav9bntKULvMZ0g7twVFolC9gdrh9peMS0megSFr7iDpWwWIDgFUc3W5SlX99fKnhxsoy6ipdAv9JeQwebmOU6VRvOEQnNMWwZnWglYmDGrfgKRheXs",
        //          Insta_Endpoint = _instamojoSettings.EndPoint,//InstamojoConstants.INSTAMOJO_API_ENDPOINT,
        //          Insta_Auth_Endpoint = _instamojoSettings.AuthEndPoint;//InstamojoConstants.INSTAMOJO_AUTH_ENDPOINT;
        //    InstamojoAPI.Instamojo objClass = InstamojoImplementation.getApi(Insta_client_id, Insta_client_secret, Insta_Endpoint, Insta_Auth_Endpoint);

        //    PaymentOrderDetailsResponse objPaymentRequestDetailsResponse = objClass.getPaymentOrderDetailsByTransactionId(transaction_id);

        //    if (objPaymentRequestDetailsResponse.status == "success")
        //    {
        //        /* 
        //            Here you need to put in the routines for a successful 
        //             transaction such as sending an email to customer,
        //             setting database status, informing logistics etc etc
        //        */

        //        var order = _orderService.GetOrderById(Convert.ToInt32(objPaymentRequestDetailsResponse.id));
        //        if (_orderProcessingService.CanMarkOrderAsPaid(order))
        //        {
        //            _orderProcessingService.MarkOrderAsPaid(order);
        //        }

        //        //Thank you for shopping with us. Your credit card has been charged and your transaction is successful
        //        return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        //    }

        //    else
        //    {
        //        /*
        //            Here you need to put in the routines for a failed
        //            transaction such as sending an email to customer
        //            setting database status etc etc
        //        */

        //        return RedirectToAction("Index", "Home", new { area = "" });

        //    }
        //}


        //[HttpPost]
        //[Consumes("application/x-www-form-urlencoded")]
        //public ActionResult Return([FromForm]object data)
        //{
        //    var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Instamojo") as InstamojoProcessor;
        //    if (processor == null ||
        //        !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
        //        throw new NopException("Payu module cannot be loaded");

        //    string Insta_client_id = _instamojoSettings.ClientId,// "tmLkZZ0zV41nJwhayBGBOI4m4I7bH55qpUBdEXGS",
        //          Insta_client_secret = _instamojoSettings.ClientSecret,// "IDejdccGqKaFlGav9bntKULvMZ0g7twVFolC9gdrh9peMS0megSFr7iDpWwWIDgFUc3W5SlX99fKnhxsoy6ipdAv9JeQwebmOU6VRvOEQnNMWwZnWglYmDGrfgKRheXs",
        //          Insta_Endpoint = _instamojoSettings.EndPoint,//InstamojoConstants.INSTAMOJO_API_ENDPOINT,
        //          Insta_Auth_Endpoint = _instamojoSettings.AuthEndPoint;//InstamojoConstants.INSTAMOJO_AUTH_ENDPOINT;
        //    InstamojoAPI.Instamojo objClass = InstamojoImplementation.getApi(Insta_client_id, Insta_client_secret, Insta_Endpoint, Insta_Auth_Endpoint);

        //    PaymentOrderDetailsResponse objPaymentRequestDetailsResponse = objClass.getPaymentOrderDetailsByTransactionId("12312321");

        //    if (objPaymentRequestDetailsResponse.status == "success")
        //    {
        //        /* 
        //            Here you need to put in the routines for a successful 
        //             transaction such as sending an email to customer,
        //             setting database status, informing logistics etc etc
        //        */

        //        var order = _orderService.GetOrderById(Convert.ToInt32(objPaymentRequestDetailsResponse.id));
        //        if (_orderProcessingService.CanMarkOrderAsPaid(order))
        //        {
        //            _orderProcessingService.MarkOrderAsPaid(order);
        //        }

        //        //Thank you for shopping with us. Your credit card has been charged and your transaction is successful
        //        return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        //    }

        //    else
        //    {
        //        /*
        //            Here you need to put in the routines for a failed
        //            transaction such as sending an email to customer
        //            setting database status etc etc
        //        */

        //        return RedirectToAction("Index", "Home", new { area = "" });

        //    }
        //}

        //[NonAction]
        //public override IList<string> ValidatePaymentForm(FormCollection form)
        //{
        //    return new List<string>();
        //}
    }
}
