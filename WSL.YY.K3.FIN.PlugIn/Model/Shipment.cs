using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSL.YY.K3.FIN.PlugIn.Model
{
    public class Shipment
    {
        public string token = "680b00734a2c39515548afc32766b43d";
        public string erp_shipment_no { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedByID { get; set; }

        public string shipment_owner_name { get; set; }

        public string shipment_owner_id { get; set; }

        public string Tag { get; set; }

        public string account_erp_id { get; set; }
        public string contact_erp_id { get; set; }
        public string ship_to_contact_id { get; set; }

        public string ship_to_contact_name { get; set; }

        public string ship_to_account_id { get; set; }

        public string ship_to_account_name { get; set; }

        public string delivery_date { get; set; }

        public string Carrier { get; set; }

        public string TackingNo { get; set; }

        public string TackingCompany { get; set; }

        public string DealID { get; set; }

        public string Type { get; set; }

        public string shipment_zoho_id { get; set; }

        public string Medtrum_Organization { get; set; }

        public string Description { get; set; }

        public List<ShipmentEntry> ShipmentEntrys { get; set; }
    }

    public class ShipmentEntry
    {
        public string EntryId { get; set; }

        public string Currency { get; set; }

        public string MaterialCode { get; set; }

        public string product_id { get; set; }

        public string product_name { get; set; }

        public string AttrRegion { get; set; }

        public string AttrColor { get; set; }

        public string LotNo { get; set; }

        public string model { get; set; }

        public decimal Qty_pcs { get; set; }

        public string Unit { get; set; }

        public string ExpiryDate { get; set; }

        public decimal Qty_box { get; set; }

        public decimal Qty_price { get; set; }

        public string SalesOrderID { get; set; }

        public List<SNTracking> SNTrackings { get; set; }
    }

    public class SNTracking
    {
        public string EntryId { get; set; }
        public string pdm_sn { get; set; }

        public string pump_base_sn { get; set; }

        public string transmitter_sn { get; set; }

        public string pdm_ver { get; set; }

        public string pump_base_ver { get; set; }

        public string transmitter_ver { get; set; }
    }
}
