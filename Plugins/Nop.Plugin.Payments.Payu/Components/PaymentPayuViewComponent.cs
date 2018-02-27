using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Payu.Components
{
    [ViewComponent(Name = "PaymentPayu")]
    public class PaymentPayuViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.Payu/Views/PaymentInfo.cshtml");
        }
    }
}
