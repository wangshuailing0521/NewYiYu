using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSL.YY.K3.FIN.PlugIn.Model
{
    public class WMSResult
    {
        public bool success { get; set; }
        public object data { get; set; }

        public string resultCode { get; set; }

        public bool flag { get; set; }

        public string msgCode { get; set; }
        public string msg { get; set; }
        public string token { get; set; }
        public string ttid { get; set; }
        public string domain { get; set; }
        public string transactionId { get; set; }
    }
}
