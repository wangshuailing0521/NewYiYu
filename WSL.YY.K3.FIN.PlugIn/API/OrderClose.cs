using Kingdee.BOS;
using Kingdee.BOS.Log;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.ServicesStub;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Text;
using WSL.YY.K3.FIN.PlugIn.Model;

namespace WSL.YY.K3.FIN.PlugIn.API
{
    [Description("订单关闭API")]
    public class OrderClose: AbstractWebApiBusinessService
    {
        private Context Context = null;
        StringBuilder sb = new StringBuilder();

        public OrderClose(KDServiceContext context)
           : base(context) { }

        public JObject ExecuteService(string DataJson)
        {
            sb.AppendLine($@"接口方向：CRM --> Kingdee");
            sb.AppendLine($@"接口名称：订单关闭API");
            sb.AppendLine($@"请求信息：{DataJson}");

             
            JObject objRetutrn = new JObject();
            try
            {
                Context = KDContext.Session.AppContext;
                objRetutrn = Close(DataJson);
                if (objRetutrn != null)
                {
                    sb.AppendLine($@"返回信息：{objRetutrn.ToString()}");
                }
                
                Logger.Info("", sb.ToString());
                return objRetutrn;
            }
            catch (Exception ex)
            {
                if (objRetutrn == null)
                {
                    objRetutrn = new JObject();
                }
                objRetutrn.Add("IsSuccess", "false");
                objRetutrn.Add("Number", "");
                objRetutrn.Add("Message", ex.Message);
                sb.AppendLine($@"返回信息：{objRetutrn.ToString()}");
                Logger.Error("", sb.ToString(), ex);
                return objRetutrn;
            }
        }

        public JObject Close(string json)
        {
            JObject model = JObject.Parse(json);
            string billType = model["BillType"].ToString();
            string operate = model["Operate"].ToString();
            string Numbers = model["Numbers"].ToString();

            JObject objRetutrn 
                = CloseBill(billType, operate, Numbers);
            return objRetutrn;
        }

        public JObject CloseBill(string billType, string operate, string Numbers)
        {
            // 使用webapi引用组件Kingdee.BOS.WebApi.Client.dll
            K3CloudApiClient client = new K3CloudApiClient("http://47.254.177.237/K3Cloud/");
            var loginResult = client.ValidateLogin("60026403dd9180", "沈蓉", "804420", 2052);
            var resultType = JObject.Parse(loginResult)["LoginResultType"].Value<int>();
            //登录结果类型等于1，代表登录成功
            if (resultType == 1)
            {
                OrderCloseModel model = new OrderCloseModel()
                {
                    Numbers = new string[] { Numbers }
                };
                string data = JsonConvert.SerializeObject(model);
                string responseOut = client.ExcuteOperation(billType, operate, data);
                Logger.Info("", responseOut);
               return JObject.Parse(responseOut);
            }
            else
            {
                throw new Exception("登录失败");
            }
            return null;
        }
    }
}
