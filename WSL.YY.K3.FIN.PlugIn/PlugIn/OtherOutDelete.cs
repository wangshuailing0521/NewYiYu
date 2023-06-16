using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WSL.YY.K3.FIN.PlugIn.Helper;
using WSL.YY.K3.FIN.PlugIn.Model;

namespace WSL.YY.K3.FIN.PlugIn.PlugIn
{
    [Description("其他出库删除插件")]
    public class OtherOutDelete: AbstractOperationServicePlugIn
    {
        string url = "https://zoho.onetrum.com/public/index.php/api/index/del_shipment";
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FZohoShipmentNo");
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
                sb.AppendLine($@"接口方向：Kingdee --> CRM");
                sb.AppendLine($@"接口名称：其他出库删除API");

                try
                {
                    string shipmentNo = billObj["FZohoShipmentNo"].ToString();

                    if (string.IsNullOrWhiteSpace(shipmentNo))
                    {
                        continue;
                    }

                    var shipmentIds = new { shipment_ids = shipmentNo };
                    string json = JsonHelper.ToJSON(shipmentIds);
                    sb.AppendLine($@"请求信息：{json}");
                    string response = ApiHelper.HttpPost(url, json);
                    sb.AppendLine($@"返回信息：{response}");

                    #region 解析返回信息
                    JObject model = JObject.Parse(response);
                    if (model["code"] != null)
                    {
                        if (model["code"].ToString() != "200")
                        {
                            throw new KDException("错误", response);
                        }
                    }
                    else
                    {
                        throw new KDException("错误", response);
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
