using Kingdee.BOS;
using Kingdee.BOS.Log;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.WebApi.ServicesStub;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WSL.YY.K3.FIN.PlugIn.Helper;

namespace WSL.YY.K3.FIN.PlugIn.API
{
    public class GetOrgId: AbstractWebApiBusinessService
    {
        Context Context = null;
        StringBuilder sb = new StringBuilder();

        public GetOrgId(KDServiceContext context)
           : base(context)
        { }

        public JObject ExecuteService(string DataJson)
        {
            sb.AppendLine($@"接口方向：CRM --> Kingdee");
            sb.AppendLine($@"接口名称：组织内码获取API");
            sb.AppendLine($@"请求信息：{DataJson}");
            JObject objRetutrn = new JObject();
            try
            {
                Context = KDContext.Session.AppContext;
                objRetutrn = Get(DataJson);
                sb.AppendLine($@"返回信息：{objRetutrn.ToString()}");
                Logger.Info("", sb.ToString());
                return objRetutrn;
            }
            catch (Exception ex)
            {
                objRetutrn.Add("IsSuccess", "false");
                objRetutrn.Add("Number", "");
                objRetutrn.Add("Message", ex.Message);
                sb.AppendLine($@"返回信息：{objRetutrn.ToString()}");
                Logger.Error("", sb.ToString(), ex);
                return objRetutrn;
            }
        }

        public JObject Get(string json)
        {
           // JObject model = JObject.Parse(json);
            //string orgNameJson = model["orgNames"].ToString();
            List<string> orgNames
                = json.Split(',').ToList();

            List<string> orgIds = new List<string>();
            foreach (var orgName in orgNames)
            {
                string orgId = SqlHelper.GetOrgId(Context, orgName);
                orgIds.Add(orgId);
            }   


            JObject objRetutrn = new JObject();
            objRetutrn.Add("IsSuccess", true);
            string message = "";
            objRetutrn.Add("Number", string.Join(",",orgIds));
            objRetutrn.Add("Message", message);
            return objRetutrn;
        }
    }
}
