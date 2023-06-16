using Kingdee.BOS.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSL.YY.K3.FIN.PlugIn.Model;

namespace WSL.YY.K3.FIN.PlugIn.Helper
{
    public static class WMSAPI
    {
        public static string token = "";

        public static string GetToken(string apiKey)
        {
            string url = $@"https://wms-test.medtrum.com/SCM.TMS7.WebApi/Oauth/GetToken?apikey={apiKey}";

            string response = HttpHelper.Get(url);

            //Logger.Info("", sb.ToString());

            WMSResult result = JsonHelper.FromJSON<WMSResult>(response);
            if (result.flag)
            {
                token = result.token;
            }
            else
            {
                throw new Exception(result.msg);
            }
            return token;
        }
    }
}
