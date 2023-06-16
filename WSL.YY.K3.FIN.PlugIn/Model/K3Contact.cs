using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSL.YY.K3.FIN.PlugIn.Model
{
    public class K3Contact
    {
        public long Id { get; set; }

        public string Number { get; set; }

        public string Name { get; set; }
       
        public string Email { get; set; }

        public string Mobile { get; set; }

        public string Tel { get; set; }

        public string BillAddressNumber { get; set; }

        public string BillAddressName { get; set; }

        public string BillAddressDetail { get; set; }

        public string ShipAddressNumber { get; set; }

        public string ShipAddressName { get; set; }

        public string ShipAddressDetail { get; set; }
    }
}
