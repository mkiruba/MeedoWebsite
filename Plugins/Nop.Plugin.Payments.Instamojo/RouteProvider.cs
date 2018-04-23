using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Payu
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.Payments.Instamojo.Configure",
                 "Plugins/PaymentInstamojo/Configure",
                 new { controller = "PaymentPayu", action = "Configure" });

            routeBuilder.MapRoute("Plugin.Payments.Instamojo.PaymentInfo",
                 "Plugins/PaymentInstamojo/PaymentInfo",
                 new { controller = "PaymentInstamojo", action = "PaymentInfo" });

            //Return
            routeBuilder.MapRoute("Plugin.Payments.Instamojo.Return",
                 "Plugins/PaymentInstamojo/Return",
                 new { controller = "PaymentInstamojo", action = "Return" });
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
