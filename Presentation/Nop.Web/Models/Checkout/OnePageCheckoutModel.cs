using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Models.Checkout
{
    public partial class OnePageCheckoutModel : BaseNopModel
    {
        public bool ShippingRequired { get; set; }
        public bool DisableBillingAddressCheckoutStep { get; set; }

        public CheckoutBillingAddressModel BillingAddress { get; set; }
        public CheckoutShippingAddressModel ShippingAddress { get; set; }
        public CheckoutShippingMethodModel ShippingMethod { get; set; }
        public CheckoutPaymentMethodModel PaymentMethod { get; set; }
        public CheckoutPaymentInfoModel PaymentInfo { get; set; }
        public CheckoutConfirmModel ConfirmOrder { get; set; }
    }
}