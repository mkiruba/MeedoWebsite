using InstamojoAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Instamojo.Models;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Sp.Agent.Core.Execution;
using System;
using System.Collections.Generic;
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

        public PaymentInstamojoController(ISettingService settingService, IPaymentService paymentService, IOrderService orderService, IOrderProcessingService orderProcessingService, InstamojoSettings instamojoSettings, PaymentSettings paymentSettings, ILogger logger)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._instamojoSettings = instamojoSettings;
            this._paymentSettings = paymentSettings;
            this._logger = logger;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public ActionResult Configure()
        {
            ConfigurationModel model = new ConfigurationModel()
            {
                ClientId = this._instamojoSettings.ClientId,
                ClientSecret = this._instamojoSettings.ClientSecret,
                EndPoint = this._instamojoSettings.EndPoint,
                AuthEndPoint = this._instamojoSettings.AuthEndPoint,
                WebHookUrl = this._instamojoSettings.WebHookUrl
            };
            return base.View("~/Plugins/Payments.Instamojo/Views/Instamojo/Configure.cshtml", model);
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
                this._instamojoSettings.ClientId = model.ClientId;
                this._instamojoSettings.ClientSecret = model.ClientSecret;
                this._instamojoSettings.EndPoint = model.EndPoint;
                this._instamojoSettings.AuthEndPoint = model.AuthEndPoint;
                this._instamojoSettings.WebHookUrl = model.WebHookUrl;
                this._settingService.SaveSetting<InstamojoSettings>(this._instamojoSettings, 0);
                V_1 = base.View("~/Plugins/Payments.Instamojo/Views/Instamojo/Configure.cshtml", model);
            }
            else
            {
                V_1 = this.Configure();
            }
            return V_1;
        }

        public static List<PropertyInfo> GetDifferences(PaymentOrderDetailsResponse r1, PaymentOrderDetailsResponse r2)
        {
            List<PropertyInfo> differences = new List<PropertyInfo>();
            PropertyInfo[] V_1 = r1.GetType().GetProperties();
            for (int V_2 = 0; V_2 < (int)V_1.Length; V_2++)
            {
                PropertyInfo property = V_1[V_2];
                if (!property.GetValue(r1, null).Equals(property.GetValue(r2, null)))
                {
                    differences.Add(property);
                }
            }
            return differences;
        }

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
        public ActionResult Return(string id, string transaction_id, string payment_id)
        {
            object[] objArray = new object[] { id, transaction_id, payment_id };
            return (ActionResult)PermutedExecutionServices2.ExecuteMethod(this, "3ae71556bd254127b580d2eedc51fcc7", "84515A2F2188489FA43F609922C17C0C", objArray, null, null);
        }

        //[NonAction]
        //public override IList<string> ValidatePaymentForm(FormCollection form)
        //{
        //    return new List<string>();
        //}
    }
}
