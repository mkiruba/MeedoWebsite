using Nop.Web.Framework.Mvc.Models;

namespace Nop.Web.Models.Directory
{
    public partial class PincodeModel : BaseNopModel
    {
        public string Office { get; set; }
        public string Pincode { get; set; }
        public string Division { get; set; }
        public string Region { get; set; }
        public string Circle { get; set; }
        public string District { get; set; }
        public string State { get; set; }
    }
}