using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSL.YY.K3.FIN.PlugIn.Model
{
    public class WMSResponse
    {
        public bool success { get; set; }

        public int resultCode { get; set; }

        //public List<WMSResult> result { get; set; }

        public string msg { get; set; }
    }
}
