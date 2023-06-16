using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSL.YY.K3.FIN.PlugIn.Model
{
    public class SerialReceipts
    {
        public SerialReceipt SerialReceipt { get; set; }
    }
    public class SerialReceipt
    {
        public string Whgid { get; set; }

        public List<Serial> Serials { get; set; }

    }
    public class Serial
    {
        public string WHID { get; set; }
        public string OwnerID { get; set; }
        public string SerialNo { get; set; }
        public string SkuId { get; set; }
        //public string SerialStatus { get; set; }
        //public string SerialFrom { get; set; }
        public string LpnID { get; set; }

        public string CaseID = "N/A";
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string PackQty { get; set; }

        public string ExternalLot { get; set; }
    }
}
