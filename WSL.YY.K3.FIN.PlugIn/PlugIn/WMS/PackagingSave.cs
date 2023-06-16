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
    [Description("条码拆装箱插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class PackagingSave : AbstractOperationServicePlugIn
    {
        string url = "https://wms-test.medtrum.com/SCM.WMS7.WebApi/WMS/SaveShippingOrder";

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
                sb.AppendLine($@"接口名称：条码拆装箱API");

                try
                {
                    List<SerialReceipt> serialReceipts = new List<SerialReceipt>();
                    SerialReceipt serialReceipt = new SerialReceipt();
                    serialReceipt.Whgid = "medtrum.wh1";

                    List<Serial> serials = new List<Serial>();
                    DynamicObjectCollection entrys
                        = billObj["UN_PackagingEntry"] as DynamicObjectCollection;
                    foreach (var entry in entrys)
                    {
                        Serial serial = new Serial();
                        serial.WHID = "";
                        serials.Add(serial);
                    }
                    serialReceipt.Serials = serials;
                    serialReceipts.Add(serialReceipt);

                    string json = JsonHelper.ToJSON(serialReceipts);
                    sb.AppendLine($@"请求信息：{json}");
                    string response = ApiHelper.HttpPost(url, json);
                    sb.AppendLine($@"返回信息：{response}");

                    #region 解析返回信息
                    WMSResult result = JsonHelper.FromJSON<WMSResult>(response);
                    if (!result.flag)
                    {
                        throw new Exception(JsonHelper.ToJSON(result.data));
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
