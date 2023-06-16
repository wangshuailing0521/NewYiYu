using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSL.YY.K3.FIN.PlugIn.PlugIn
{
    [Description("销售订单维护插件")]
    [HotUpdate]
    public class SaleOrderEdit:AbstractBillPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);

            this.View.GetBarItem("FSaleOrderEntry", "tbBOMEXPAND").Visible = true;
            this.View.GetControl("FRowType").Visible = true;
            this.View.GetControl("FParentMatId").Visible = true;
        }

        public override void AfterUpdateViewState(EventArgs e)
        {
            base.AfterUpdateViewState(e);

            this.View.GetBarItem("FSaleOrderEntry", "tbBOMEXPAND").Visible = true;
            this.View.GetControl("FRowType").Visible = true;
            this.View.GetControl("FParentMatId").Visible = true;
        }
    }
}
