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
    [Description("供应商审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class SupplierAudit: AbstractOperationServicePlugIn
    {
        string url = "https://wms-test.medtrum.com/SCM.TMS7.WebApi/BaseInfo/SaveVendor";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FCountry");
            e.FieldKeys.Add("FProvincial");
            e.FieldKeys.Add("FAddress");
            e.FieldKeys.Add("FZip");
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
                sb.AppendLine($@"接口名称：供应商API");

                try
                {
                    DynamicObjectCollection SupplierBase = billObj["SupplierBase"] as DynamicObjectCollection;
                    SupplierWms supplierWms = new SupplierWms();
                    supplierWms.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");
                    supplierWms.apiKey = "C4B60A0726CA408FAEEE55817A4B0991";

                    Vendor vendor = new Vendor();
                    vendor.VendorID = billObj["Number"].ToString();
                    vendor.VendorName = billObj["Name"].ToString();

                    DynamicObject COUNTRY = SupplierBase[0]["Country"] as DynamicObject;
                    string country = "";
                    if (COUNTRY != null)
                    {
                        country
                         = SqlHelper.GetCountryWMSName(this.Context, COUNTRY["FDataValue"].ToString());
                        vendor.Country = country;
                    }

                    DynamicObject PROVINCIAL = SupplierBase[0]["Provincial"] as DynamicObject;
                    if (PROVINCIAL != null)
                    {
                        //vendor.VendorUdf4 = PROVINCIAL["FDataValue"].ToString();
                    }

                    vendor.ZIP = SupplierBase[0]["Zip"].ToString();
                    vendor.Address = SupplierBase[0]["Address"].ToString();

                    supplierWms.vendor = vendor;

                    string json = JsonHelper.ToJSON(supplierWms);
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

                    url = "https://wms-test.medtrum.com/SCM.TMS7.WebApi/BaseInfo/SaveCustomer";
                    CustomerWMS customerWms = new CustomerWMS();
                    customerWms.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");
                    customerWms.apiKey = "C4B60A0726CA408FAEEE55817A4B0991";

                    Customer customer = new Customer();
                    customer.CustomerID = billObj["Number"].ToString();
                    customer.CustomerName = billObj["Name"].ToString();

                    if (COUNTRY != null)
                    {
                        customer.Country = country;
                    }

                    if (PROVINCIAL != null)
                    {
                        //customer.CustomerUdf4 = PROVINCIAL["FDataValue"].ToString();
                    }

                    customer.ZIP = SupplierBase[0]["Zip"].ToString();
                    customer.Address = SupplierBase[0]["Address"].ToString();

                    customerWms.customer = customer;

                    json = JsonHelper.ToJSON(customerWms);
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
