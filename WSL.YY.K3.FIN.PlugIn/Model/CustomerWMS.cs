using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSL.YY.K3.FIN.PlugIn.Model
{
    public class CustomerWMS
    {
        public Customer customer { get; set; }
        public string token { get; set; }

        public string transactionId { get; set; }

        public string apiKey { get; set; }
    }

    public class Customer
    {
        public string DomainName = "PUBLIC";
        public string CustomerID { get; set; }

        public string CustomerName { get; set; }

        public string IS_ACTIVE = "是";

        public string Country { get; set; }

        public string Province { get; set; }

        public string City { get; set; }
        public string ZIP { get; set; }

        public string ContactName { get; set; }

        public string ContactTel { get; set; }

        public string Address { get; set; }

        public string CustomerUdf1 { get; set; }

        public string CustomerUdf2 { get; set; }

        public string CustomerUdf3 { get; set; }

        public string CustomerUdf4 { get; set; }

        public string CustomerUdf5 { get; set; }
    }
}
