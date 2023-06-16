using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSL.YY.K3.FIN.PlugIn.Model
{
    public class ReceivableCollection
    {
        public string token = "680b00734a2c39515548afc32766b43d";

        public string receive_name { get; set; }

        public string receive_owner_id { get; set; }

        public string deal_id { get; set; }

        public string receive_owner_name { get; set; }

        public string status = "Closed";

        public string invoice_no { get; set; }

        public string type { get; set; }

        public string contact_id { get; set; }

        public string contact_name { get; set; }

        public string account_id { get; set; }

        public string account_name { get; set; }

        //public List<SaleEntry> SaleOrders { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

        public string CreatedDate { get; set; }

        public string ModifiedDate { get; set; }

        public string payment_terms { get; set; }

        public decimal total_amount { get; set; }

        public decimal paid_amount { get; set; }

        public string due_date { get; set; }

        public string currency { get; set; }

        public string sales_order_id { get; set; }
        public string sales_order_name { get; set; }

        public string shipment_id { get; set; }

        public string medtrum_orgnization { get; set; }
    }

    public class SaleEntry
    {
        public string SaleOrder { get; set; }

        public List<ShipmentBill> Shipments { get; set; }
    }

    public class ShipmentBill
    {
        public string Shipment { get; set; }
    }
}
