using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Payu.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Payu.Controllers
{
    public class PaymentPayuController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly PayuPaymentSettings _PayuPaymentSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IEmailSender _emailSender;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;

        public PaymentPayuController(ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService,
             ILocalizationService localizationService,
            PayuPaymentSettings PayuPaymentSettings,
            PaymentSettings paymentSettings,
            IWorkContext workContext,
            IStoreContext storeContext, IEmailSender emailSender,
            IEmailAccountService emailAccountService, EmailAccountSettings emailAccountSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._PayuPaymentSettings = PayuPaymentSettings;
            this._localizationService = localizationService;
            this._paymentSettings = paymentSettings;
            this._emailSender = emailSender;
            this._emailAccountService = emailAccountService;
            this._emailAccountSettings = emailAccountSettings;
        }


        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.MerchantId = _PayuPaymentSettings.MerchantId;
            model.Key = _PayuPaymentSettings.Key;
            model.MerchantParam = _PayuPaymentSettings.MerchantParam;
            model.PayUri = _PayuPaymentSettings.PayUri;
            model.AdditionalFee = _PayuPaymentSettings.AdditionalFee;

            // return View("Nop.Plugin.Payments.Payu.Views.PaymentPayu.Configure", model);

            return View("~/Plugins/Payments.Payu/Views/Configure.cshtml", model);

            //return View("~/Plugins/Payments.PayPalStandard/Views/PaymentPayPalStandard/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _PayuPaymentSettings.MerchantId = model.MerchantId;
            _PayuPaymentSettings.Key = model.Key;
            _PayuPaymentSettings.MerchantParam = model.MerchantParam;
            _PayuPaymentSettings.PayUri = model.PayUri;
            _PayuPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_PayuPaymentSettings);

            //return View("Nop.Plugin.Payments.Payu.Views.PaymentPayu.Configure", model);
            //return View("~/Plugins/Payments.Payu/Views/PaymentPayu/Configure.cshtml", model);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        //[ChildActionOnly]
        //public ActionResult PaymentInfo()
        //{
        //    var model = new PaymentInfoModel();
        //    //return View("Nop.Plugin.Payments.Payu.Views.PaymentPayu.PaymentInfo", model);
        //    return View("~/Plugins/Payments.Payu/Views/PaymentPayu/PaymentInfo.cshtml", model);

        //}

        //[NonAction]
        //public override IList<string> ValidatePaymentForm(FormCollection form)
        //{
        //    var warnings = new List<string>();
        //    return warnings;
        //}

        //[NonAction]
        //public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        //{
        //    var paymentInfo = new ProcessPaymentRequest();
        //    return paymentInfo;
        //}
                
        public ActionResult Return(IFormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Payu") as PayuPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("Payu module cannot be loaded");

            var myUtility = new PayuHelper();
            string orderId, merchantId, amount, productinfo, firstname, email, hash, status, checksum;

            //Assign following values to send it to verifychecksum function.
            if (String.IsNullOrWhiteSpace(_PayuPaymentSettings.Key))
                throw new NopException("Payu key is not set");

            merchantId = _PayuPaymentSettings.MerchantId.ToString();
            orderId = form["txnid"];
            amount = form["amount"];
            productinfo = form["productinfo"];
            firstname = form["firstname"];
            email = form["email"];
            hash = form["hash"];
            status = form["status"];

            checksum = myUtility.Verifychecksum(merchantId, orderId, amount, productinfo, firstname, email, status, _PayuPaymentSettings.Key);

            if (checksum == hash)
            {
                if (status == "success")
                {
                    /* 
                        Here you need to put in the routines for a successful 
                         transaction such as sending an email to customer,
                         setting database status, informing logistics etc etc
                    */

                    var order = _orderService.GetOrderById(Convert.ToInt32(orderId));
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
                    SendEmail(orderId, firstname, email);
                    ErrorNotification(_localizationService.GetResource("Plugins.Payments.Instamojo.PaymentFailed"), true);
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
            }
            else
            {
                /*
                    Here you need to simply ignore this and dont need
                    to perform any operation in this condition
                */
                return Content("Security Error. Illegal access detected");
            }
        }
        public void SendEmail(string orderId, string firstName, string email)
        {
            var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount != null)
            {
                var subject = "Meedo - Important Payment failed in Instamojo";
                var body = $"Hi, \n Payment been failed for the genuine payment made by the customer. Please be in touch with customer as soon as possible.\n" +
                    $"OrderId - {orderId}\n CustomerName - {firstName}\n " +
                    $"CustomerEmailAddress - {email}\n";
                _emailSender.SendEmail(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, "meedoindia@gmail.com", null);
            }
        }
    }
}
