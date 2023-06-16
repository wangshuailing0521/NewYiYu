using Kingdee.BOS.Log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using WSL.YY.K3.FIN.PlugIn.Helper;

namespace WSL.YY.K3.FIN.PlugIn.API
{
    public class CustAssign
    {
        public string Assign(string custId, List<string> orgIds)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.Url = "http://47.254.177.237/k3cloud/Kingdee.BOS.WebApi.ServicesStub.AuthService.ValidateUser.common.kdsvc";
            //httpClient.Url = "http://8.211.1.246/k3cloud/Kingdee.BOS.WebApi.ServicesStub.AuthService.ValidateUser.common.kdsvc";
            //httpClient.Url = "http://desktop-ru7vte7/k3cloud/Kingdee.BOS.WebApi.ServicesStub.AuthService.ValidateUser.common.kdsvc";
            List<object> Parameters = new List<object>();
            Parameters.Add("60026403dd9180");//正式服务器正式账套
            //Parameters.Add("5f03d33ca31cd3");//测试服务器测试账套
            Parameters.Add("沈蓉");//用户名
            Parameters.Add("804420");//密码
            //Parameters.Add("5ea0f099c8bbcf");//帐套Id
            //Parameters.Add("administrator");//用户名
            //Parameters.Add("888888");//密码
            Parameters.Add(2052);
            httpClient.Content = JsonConvert.SerializeObject(Parameters);
            var iResult = JObject.Parse(httpClient.AsyncRequest())["LoginResultType"].Value<int>();
            if (iResult == 1)
            {
                httpClient.Url =
                    string.Concat("http://47.254.177.237/k3cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Allocate.common.kdsvc");
                //httpClient.Url =
                //    string.Concat("http://8.211.1.246/k3cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Allocate.common.kdsvc");
                //httpClient.Url =
                //    string.Concat("http://desktop-ru7vte7/k3cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Allocate.common.kdsvc");

                Parameters = new List<object>();

                string formId = "BD_Customer";
                Parameters.Add(formId);
                JObject dataObj = new JObject();
                dataObj.Add("PkIds", custId);
                dataObj.Add("TOrgIds", string.Join(",", orgIds));
                dataObj.Add("IsAutoSubmitAndAudit", "true");
                string data = dataObj.ToString();
                Parameters.Add(data);
                httpClient.Content = JsonConvert.SerializeObject(Parameters);
                string responseOut = httpClient.AsyncRequest();
                Logger.Info("", responseOut);
                return $@"客户分配结果：{responseOut}";            
            }

            return "分配失败";
        }
    }
}
