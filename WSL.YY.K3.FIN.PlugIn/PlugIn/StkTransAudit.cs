using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WSL.YY.K3.FIN.PlugIn.Helper;

namespace WSL.YY.K3.FIN.PlugIn.PlugIn
{
    [Description("直接调拨审核插件")]
    public class StkTransAudit : AbstractOperationServicePlugIn
    {
        string url = "https://zoho.onetrum.com/public/index.php/api/index/insert_shipment";
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
                sb.AppendLine($@"接口名称：直接调拨订单关闭API");
                try
                {
                    #region 调用关闭接口
                    string zohoBillNo = "";
                    bool close = SqlHelper.TransApplyIsClose(
                        this.Context,
                        billObj["Id"].ToString(),
                        out zohoBillNo);
                    if (close)
                    {
                        url = "https://zoho.onetrum.com/public/index.php/api/index/close_order_from_kd";
                        sb.AppendLine($@"请求地址：{url}");

                        var order_id = new { order_id = zohoBillNo };
                        string json = JsonHelper.ToJSON(order_id);
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