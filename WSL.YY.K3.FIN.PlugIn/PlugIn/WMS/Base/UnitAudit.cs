using Kingdee.BOS.Core.DynamicForm.PlugIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Log;
using WSL.YY.K3.FIN.PlugIn.Model;
using WSL.YY.K3.FIN.PlugIn.Helper;
using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace WSL.YY.K3.FIN.PlugIn.PlugIn.WMS
{
    [Description("单位审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class UnitAudit : AbstractOperationServicePlugIn
    {
        string url = "https://wms-test.medtrum.com/SCM.TMS7.WebApi/BaseInfo/SaveUom";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);


            if (e.DataEntitys.Count() < 1)
            {
                return;
            }

            foreach (DynamicObject billObj in e.DataEntitys)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine($@"接口方向：Kingdee --> WMS");
                sb.AppendLine($@"接口名称：单位API");

                try
                {
                    UnitWMS unitWms = new UnitWMS();
                    unitWms.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");
                    unitWms.apiKey = "C4B60A0726CA408FAEEE55817A4B0991";

                    Unit unit = new Unit();
                    unit.UOM_ID = billObj["Number"].ToString();
                    unit.UOM_NAME = billObj["Name"].ToString();

                    unitWms.Uoms = new List<Unit>() { unit };

                    string json = JsonHelper.ToJSON(unitWms);
                    sb.AppendLine($@"请求信息：{json}");
                    string response = ApiHelper.HttpPost(url, json);
                    sb.AppendLine($@"返回信息：{response}");

                    #region 解析返回信息
                    WMSResult result = JsonHelper.FromJSON<WMSResult>(response);
                    if (!result.flag)
                    {
                        throw new Exception(result.msg);
                    }
                    #endregion

                    Logger.Info("", sb.ToString());
                }
                catch (Exception ex)
                {
                    sb.AppendLine($@"错误信息：{ex.Message.ToString()}");
                    Logger.Error("", sb.ToString(), ex);

                    throw new Exception(ex.Message.ToString());
                }
            }
        }
    }
}
