using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Instamojo.Components
{
    [ViewComponent(Name = "PaymentInstamojo")]
    public class PaymentInstamojoViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.Instamojo/Views/PaymentInfo.cshtml");
        }
    }
}
