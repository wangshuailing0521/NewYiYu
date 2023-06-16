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
    [Description("收款单反审核插件")]
    public class ReceiveBillUnAudit: AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FRECEIVEAMOUNTFOR_H");
            e.FieldKeys.Add("FPURPOSEID");
            e.FieldKeys.Add("FRECEIVEITEMTYPE");
            e.FieldKeys.Add("FRECEIVEITEM");
            e.FieldKeys.Add("FSALEORDERNO");
            e.FieldKeys.Add("FCreatorId");
            e.FieldKeys.Add("FCreateDate");
            e.FieldKeys.Add("FModifierId");
            e.FieldKeys.Add("FModifyDate");
            e.FieldKeys.Add("FSETTLEORGID");
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
                sb.AppendLine($@"接口名称：收款反审核API");

                try
                {
                    DynamicObject settleOrg = billObj["SETTLEORGID"] as DynamicObject;
                    if (settleOrg["Number"].ToString() == "1001" || settleOrg["Number"].ToString() == "1002")
                    {
                        continue;
                    }

                    ReceivableCollection receivable = new ReceivableCollection();

                    #region 收款
                    string fid = billObj["Id"].ToString();
                    DynamicObjectCollection ableData
                        = SqlHelper.GetAbleNo(this.Context, fid);
                    foreach (var item in ableData)
                    {
                        receivable.payment_terms = item["FName"].ToString();
                        receivable.type = "Receivable";
                        //应收单号
                        receivable.receive_name = item["FBILLNO"].ToString();
                        //应收金额
                        receivable.total_amount = Convert.ToDecimal(item["FPAYAMOUNTFOR"]);
                        //实收金额
                        receivable.paid_amount = Convert.ToDecimal(item["FWRITTENOFFAMOUNTFOR"]);
                        //到期日
                        receivable.due_date = item["FENDDATE"].ToString();
                        if (receivable.total_amount > receivable.paid_amount && DateTime.Now <= Convert.ToDateTime(receivable.due_date))
                        {
                            receivable.status = "Open";
                        }
                        if (receivable.total_amount > receivable.paid_amount && DateTime.Now > Convert.ToDateTime(receivable.due_date))
                        {
                            receivable.status = "Open-Overdue";
                        }
                    }
                    #endregion

                    //组织
                    DynamicObject org = billObj["FPAYORGID"] as DynamicObject;
                    receivable.medtrum_orgnization = org["Name"].ToString();

                    //币别
                    DynamicObject currency = billObj["CURRENCYID"] as DynamicObject;
                    receivable.currency
                        = SqlHelper.GetCurrencyEngName(this.Context, currency["Id"].ToString());

                    //客户
                    DynamicObject cust = billObj["CONTACTUNIT"] as DynamicObject;
                    receivable.account_name = cust["Name"].ToString();
                    receivable.account_id = SqlHelper.GetCustZohoId(this.Context, cust["Id"].ToString());

                    //创建人
                    DynamicObject creator = billObj["FCreatorId"] as DynamicObject;
                    receivable.CreatedBy = creator["Name"].ToString();

                    //创建日期
                    receivable.CreatedDate = billObj["FCreateDate"].ToString();

                    //修改人
                    DynamicObject modified = billObj["ModifierId"] as DynamicObject;
                    receivable.ModifiedBy = modified["Name"].ToString();

                    //修改日期
                    receivable.ModifiedDate = billObj["FModifyDate"].ToString();

                    //收款单明细
                    List<string> saleOrderList = new List<string>();
                    List<string> shipmentList = new List<string>();
                    List<SaleEntry> saleEntrys = new List<SaleEntry>();
                    DynamicObjectCollection entrys
                        = billObj["RECEIVEBILLENTRY"] as DynamicObjectCollection;

                    foreach (var entry in entrys)
                    {
                        //收款用途
                        DynamicObject purpose = entry["PURPOSEID"] as DynamicObject;
                        if (purpose["Name"].ToString() == "预收款")
                        {
                            string itemType = entry["RECEIVEITEMTYPE"].ToString();
                            //预收项目类型是销售订单
                            if (itemType == "1")
                            {
                                if (!saleOrderList.Contains(entry["RECEIVEITEM"].ToString()))
                                {
                                    saleOrderList.Add(entry["RECEIVEITEM"].ToString());
                                }

                                List<ShipmentBill> shipments
                                    = SqlHelper.GetOutStockNumber(Context, entry["RECEIVEITEM"].ToString());
                                foreach (var shipment in shipments)
                                {
                                    if (!shipmentList.Contains(shipment.Shipment))
                                    {
                                        shipmentList.Add(shipment.Shipment);
                                    }
                                }

                                DynamicObjectCollection
                                    amountData = SqlHelper.GetAmountBySaleNo(
                                        this.Context,
                                        string.Join(",", saleOrderList));

                                //汇款单号
                                receivable.receive_name = string.Join(",", saleOrderList);
                                if (amountData.Count > 0)
                                {
                                    //应收金额
                                    receivable.total_amount = Convert.ToDecimal(amountData[0]["FRECADVANCEAMOUNT"]);
                                    //实收金额
                                    receivable.paid_amount = Convert.ToDecimal(amountData[0]["FRECAMOUNT"]);
                                    receivable.type = "Collection";
                                    receivable.payment_terms = amountData[0]["FNAME"].ToString();
                                    if (receivable.total_amount > receivable.paid_amount)
                                    {
                                        receivable.status = "Open";
                                    }
                                }
                            }

                            //预收项目类型是客户
                            if (itemType == "2")
                            {
                                continue;
                            }
                        }

                        if (purpose["Name"].ToString() == "销售收款")
                        {

                            List<string> saleBillNos
                                = SqlHelper.GetSaleNoByRec(this.Context, fid);
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
