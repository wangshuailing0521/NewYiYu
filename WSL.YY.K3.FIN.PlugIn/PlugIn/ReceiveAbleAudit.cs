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
    [Description("应收单审核插件")]
    public class ReceiveAbleAudit: AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FSETTLEORGID");
            e.FieldKeys.Add("FPAYORGID");
            e.FieldKeys.Add("FCURRENCYID");
            e.FieldKeys.Add("FCUSTOMERID");
            e.FieldKeys.Add("FALLAMOUNT");
            e.FieldKeys.Add("FCreatorId");
            e.FieldKeys.Add("FCreateDate");
            e.FieldKeys.Add("FModifierId");
            e.FieldKeys.Add("FModifyDate");
            e.FieldKeys.Add("FEntityPlan");
            e.FieldKeys.Add("FWRITTENOFFAMOUNTFOR");
            e.FieldKeys.Add("FENDDATE_H");
            e.FieldKeys.Add("FPayConditon");
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
                sb.AppendLine($@"接口方向：Kingdee --> CRM");
                sb.AppendLine($@"接口名称：应收单API");

                try
                {
                    ReceivableCollection receivable = new ReceivableCollection();

                    receivable.type = "Receivable";

                    receivable.receive_name = billObj["BillNo"].ToString();

                    DynamicObject settleOrg = billObj["SETTLEORGID"] as DynamicObject;

                    if (settleOrg["Number"].ToString()== "1001"|| settleOrg["Number"].ToString() == "1002")
                    {
                        continue;
                    }
                      

                    //组织
                    DynamicObject org = billObj["FPAYORGID"] as DynamicObject;
                    receivable.medtrum_orgnization = org["Name"].ToString();

                    //币别
                    DynamicObject currency = billObj["CURRENCYID"] as DynamicObject;
                    receivable.currency
                        = SqlHelper.GetCurrencyEngName(this.Context, currency["Id"].ToString());

                    //客户
                    DynamicObject cust = billObj["CUSTOMERID"] as DynamicObject;
                    receivable.account_name = cust["Name"].ToString();
                    receivable.account_id = SqlHelper.GetCustZohoId(this.Context, cust["Id"].ToString());

                    DynamicObjectCollection finObj 
                        = billObj["AP_PAYABLEFIN"] as DynamicObjectCollection;
                    foreach (var item in finObj)
                    {
                        //应收金额
                        receivable.total_amount = Convert.ToDecimal(item["FALLAMOUNT"]);
                    }

                    //收款条件
                    DynamicObject payConditon 
                        = billObj["PayConditon"] as DynamicObject;
                    if (payConditon != null)
                    {
                        receivable.payment_terms = payConditon["Name"].ToString();
                    }

                    //创建人
                    DynamicObject creator = billObj["CreatorId"] as DynamicObject;
                    receivable.CreatedBy = creator["Name"].ToString();

                    //创建日期
                    receivable.CreatedDate = billObj["CreateDate"].ToString();

                    //修改人
                    DynamicObject modified = billObj["ModifierId"] as DynamicObject;
                    receivable.ModifiedBy = modified["Name"].ToString();

                    //修改日期
                    receivable.ModifiedDate = billObj["ModifyDate"].ToString();

                    //到期日
                    receivable.due_date = billObj["FENDDATE_H"].ToString();

                    //应收单收款计划
                    DynamicObjectCollection entrys
                        = SqlHelper.GetAmountByAbleId(this.Context, billObj["Id"].ToString());
                    if (entrys .Count > 0)
                    {
                        //收款金额
                        receivable.paid_amount
                            = Convert.ToDecimal(entrys[0]["FWRITTENOFFAMOUNTFOR"]);
                    }

                    if (receivable.total_amount > receivable.paid_amount && DateTime.Now <= Convert.ToDateTime(receivable.due_date))
                    {
                        receivable.status = "Open";
                    }
                    if (receivable.total_amount > receivable.paid_amount && DateTime.Now > Convert.ToDateTime(receivable.due_date))
                    {
                        receivable.status = "Open-Overdue";
                    }

                    List<string> saleOrderList = new List<string>();
                    List<string> shipmentList = new List<string>();
                    List<SaleEntry> saleEntrys = new List<SaleEntry>();

                    string fid = billObj["Id"].ToString();
                    List<string> saleBillNos
                        = SqlHelper.GetSaleNoByAble(this.Context, fid);
                    foreach (var saleBillNo in saleBillNos)
                    {
                        if (!saleOrderList.Contains(saleBillNo))
                        {
                            saleOrderList.Add(saleBillNo);
                        }

                        List<ShipmentBill> shipments
                            = SqlHelper.GetOutStockNumber(Context, saleBillNo);
                        foreach (var shipment in shipments)
                        {
                            if (!shipmentList.Contains(shipment.Shipment))
                            {
                                shipmentList.Add(shipment.Shipment);
                            }
                        }
                    }

                    receivable.sales_order_id = string.Join(",", saleOrderList);
                    receivable.shipment_id = string.Join(",", shipmentList);

                    #region 根据销售单号获取销售订单上的联系人
                    if (!string.IsNullOrWhiteSpace(receivable.sales_order_id))
                    {
                        receivable.contact_id
                        = SqlHelper.GetContactZohoIdBySaleNo(this.Context, receivable.sales_order_id);

                        receivable.contact_name
                            = SqlHelper.GetContactZohoNameBySaleNo(this.Context, receivable.sales_order_id);

                        DynamicObjectCollection data
                            = SqlHelper.GetSaleZohoInfo(this.Context, receivable.sales_order_id);

                        foreach (var item in data)
                        {
                            receivable.sales_order_id = item["FZohoBillNo"].ToString();
                            receivable.receive_owner_id = item["FSaleOwnerId"].ToString();
                            receivable.deal_id = item["FDealId"].ToString();
                        }
                    }

                    #endregion


                    string json = JsonHelper.ToJSON(receivable);

                    string url = "https://zoho.onetrum.com/public/index.php/api/index/insert_receive";
                    sb.AppendLine($@"请求信息：{json}");

                    if (saleOrderList != null && saleOrderList.Count > 0)
                    {
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
