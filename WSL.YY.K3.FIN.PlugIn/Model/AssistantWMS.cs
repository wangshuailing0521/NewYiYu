using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSL.YY.K3.FIN.PlugIn.Model
{
    public class AssistantWMS
    {
       public LotTemplate LotTemplate { get; set; }
    }

    public class LotTemplate
    {
        public string LotTemplateID { get; set; }
        public string Description { get; set; }
        public string IsActive = "Y";

        public List<Detail> Details { get; set; }
    }

    public class Detail
    {
        public string LotField { get; set; }
        public string LotLable { get; set; }
        public string AttrValue = "1001";
    }
}
