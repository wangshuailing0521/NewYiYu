using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSL.YY.K3.FIN.PlugIn.Model
{
    public class ItemWMS
    {
        public Item item { get; set; }
        public string token { get; set; }

        public string transactionId { get; set; }

        public string apiKey { get; set; }
    }

    public class Item
    {
        public string DomainName = "PUBLIC";

        public string ChineseMedicineApprovalNo { get; set; }
        public string OwnerID { get; set; }

        public string ItemID { get; set; }

        public string LotTemplateID { get; set; }

        public string ProductTypeID { get; set; }

        public string PRODUCT_TYPE_ID2 { get; set; }

        public string ItemName { get; set; }

        public string ItemDesc { get; set; }

        public string ITEM_SPEC { get; set; }

        public string FactoryId { get; set; }

        public string ItemShortName { get; set; }

        public string FactoryName { get; set; }

        public int MinOrderQty { get; set; }

        public string ItemUdf1 { get; set; }

        public string PackID { get; set; }

        public string IsShelfLife { get; set; }

        public string ShelfLifeType { get; set; }

        public int ShelfLifeDays { get; set; }

        public int InvWarningDays { get; set; }

        public decimal NetWGT { get; set; }

        public string NetWGTUom { get; set; }

        public decimal GrossWGT { get; set; }

        public string GrossWGTUom { get; set; }

        public string IsSerial { get; set; }

        public string is_qc { get; set; }

    }
}
