using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSL.YY.K3.FIN.PlugIn.Model
{
    public class SupplierWms
    {
        public Vendor vendor { get; set; }
        public string token { get; set; }

        public string transactionId { get; set; }

        public string apiKey { get; set; }
    }

    public class Vendor
    {
        public string DomainName = "PUBLIC";
        public string VendorID { get; set; }

        public string VendorName { get; set; }

        public string IS_ACTIVE = "是";

        public string Country { get; set; }

        public string Province { get; set; }

        public string City { get; set; }
        public string ZIP { get; set; }

        public string ContactName { get; set; }

        public string ContactTel { get; set; }

        public string Address { get; set; }

        public string VendorUdf1 { get; set; }

        public string VendorUdf2 { get; set; }

        public string VendorUdf3 { get; set; }

        public string VendorUdf4 { get; set; }

        public string VendorUdf5 { get; set; }
    }
}
