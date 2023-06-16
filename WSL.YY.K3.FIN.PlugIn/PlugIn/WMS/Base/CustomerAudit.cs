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
    [Description("客户审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class CustomerAudit: AbstractOperationServicePlugIn
    {
        string url = "https://wms-test.medtrum.com/SCM.TMS7.WebApi/BaseInfo/SaveCustomer";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FCOUNTRY");
            e.FieldKeys.Add("FPROVINCE");
            e.FieldKeys.Add("FCITY");
            e.FieldKeys.Add("FZIP");
            e.FieldKeys.Add("FADDRESS");
            e.FieldKeys.Add("FPROVINCIAL");
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
                sb.AppendLine($@"接口名称：客户API");

                try
                {
                    CustomerWMS customerWms = new CustomerWMS();
                    customerWms.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");
                    customerWms.apiKey = "C4B60A0726CA408FAEEE55817A4B0991";

                    DynamicObjectCollection BD_CUSTOMEREXT
                        = billObj["BD_CUSTOMEREXT"] as DynamicObjectCollection;

                    Customer customer = new Customer();
                    customer.CustomerID = billObj["Number"].ToString();
                    customer.CustomerName = billObj["Name"].ToString();

                    DynamicObject COUNTRY = billObj["COUNTRY"] as DynamicObject;
                    string country = "";
                    if (COUNTRY != null)
                    {
                        country
                           = SqlHelper.GetCountryWMSName(this.Context, COUNTRY["FDataValue"].ToString());
                        customer.Country = country;
                    }

                    DynamicObject PROVINCIAL = billObj["PROVINCIAL"] as DynamicObject;
                    if (PROVINCIAL != null)
                    {
                        //customer.CustomerUdf4 = PROVINCIAL["FDataValue"].ToString();
                    }

                    DynamicObject PROVINCE = BD_CUSTOMEREXT[0]["PROVINCE"] as DynamicObject;
                    if (PROVINCE != null)
                    {
                        customer.Province = PROVINCE["FDataValue"].ToString();
                    }

                    DynamicObject CITY = BD_CUSTOMEREXT[0]["CITY"] as DynamicObject;
                    if (CITY != null)
                    {
                        customer.City = CITY["FDataValue"].ToString();
                    }

                    customer.ZIP = billObj["FZIP"].ToString();
                    customer.Address = billObj["ADDRESS"].ToString();

                    customerWms.customer = customer;

                    string json = JsonHelper.ToJSON(customerWms);
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

                    url = "https://wms-test.medtrum.com/SCM.TMS7.WebApi/BaseInfo/SaveVendor";
                    SupplierWms supplierWms = new SupplierWms();
                    supplierWms.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");
                    supplierWms.apiKey = "C4B60A0726CA408FAEEE55817A4B0991";

                    Vendor vendor = new Vendor();
                    vendor.VendorID = billObj["Number"].ToString();
                    vendor.VendorName = billObj["Name"].ToString();

                    if (COUNTRY != null)
                    {
                        vendor.Country = country;
                    }

                    if (PROVINCIAL != null)
                    {
                        //vendor.VendorUdf4 = PROVINCIAL["FDataValue"].ToString();
                    }

                    if (PROVINCE != null)
                    {
                        vendor.Province = PROVINCE["FDataValue"].ToString();
                    }

                    if (CITY != null)
                    {
                        vendor.City = CITY["FDataValue"].ToString();
                    }

                    vendor.ZIP = billObj["FZIP"].ToString();
                    vendor.Address = billObj["ADDRESS"].ToString();

                    supplierWms.vendor = vendor;

                    json = JsonHelper.ToJSON(supplierWms);
                    sb.AppendLine($@"请求信息：{json}");
                    response = ApiHelper.HttpPost(url, json);
                    sb.AppendLine($@"返回信息：{response}");

                    #region 解析返回信息
                    result = JsonHelper.FromJSON<WMSResult>(response);
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
