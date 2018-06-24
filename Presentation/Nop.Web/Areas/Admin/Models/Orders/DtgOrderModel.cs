using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    public partial class DtgOrderModel : BaseNopModel
    {
        public DtgOrderModel()
        {

        }

        public int order_id { get; set; }
        public string order_date { get; set; }
        public CustomerDetail customer_details { get; set; }
        public DtgAddress shipping_address { get; set; }
        public string self_shipping { get; set; }
        public DtgAddress billing_address { get; set; }
        public DtgInvoice invoice { get; set; }
        public List<DtgProductDetails> products { get; set; }

        public class CustomerDetail
        {
            public string title { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string email { get; set; }
        }

        public class DtgAddress
        {
            public string title { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string address_line1 { get; set; }
            public string address_line2 { get; set; }
            public string country { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string pincode { get; set; }
            public string mobile { get; set; }
        }

        public class DtgInvoice
        {
            public int invoice_number { get; set; }
            public decimal invoice_value { get; set; }
            public string tax_percentage { get; set; }
            public decimal tax_value { get; set; }
            public string payment_mode { get; set; }
            public string special_instruction { get; set; }
            public string payment_method { get; set; }
            public string shipping_method { get; set; }
            public string order_comment { get; set; }
            public string voucher_text { get; set; }
            public string voucher_value { get; set; }
            public string free_shipping { get; set; }
            public decimal cod_convenience_fee { get; set; }
            public string currency { get; set; }
        }

        public class DtgProductDetails
        {
            public DtgProducts product_detail { get; set; }
        }

        public class DtgProducts
        {
            public string product_name { get; set; }
            public string product_color { get; set; }
            public string product_design_id { get; set; }
            public DtgProductSize product_size { get; set; }                        
        }
        public class DtgProductSize
        {
            public string size_name { get; set; }
            public int size_quantity { get; set; }
            public decimal unit_price { get; set; }
        }
    }
}