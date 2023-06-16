using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSL.YY.K3.FIN.PlugIn.Model
{
    public class UnitWMS
    {
        public List<Unit> Uoms { get; set; }
        public string token { get; set; }

        public string transactionId { get; set; }

        public string apiKey { get; set; }
    }

    public class Unit {

        public string UOM_ID { get; set; }

        public string UOM_NAME { get; set; }

        public string UOM_GROUP = "数量单位";

    }
}
