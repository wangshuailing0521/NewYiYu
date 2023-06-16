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
    [Description("销售出库初始化插件")]
    public  class SaleOutBatch: AbstractOperationServicePlugIn
    {
        string url = "https://zoho.onetrum.com/public/index.php/api/index/insert_shipments";
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FCarrierID");
            e.FieldKeys.Add("FCarriageNO");
            e.FieldKeys.Add("FSalesManID");
            e.FieldKeys.Add("FReceiverID");
            e.FieldKeys.Add("FReceiverContactID");
            e.FieldKeys.Add("FCreatorId");
            e.FieldKeys.Add("FDate");
            e.FieldKeys.Add("FSaleOrgId");
            e.FieldKeys.Add("F_aaaa_Text6");

            e.FieldKeys.Add("FMaterialID");
            e.FieldKeys.Add("FMateriaModel");
            e.FieldKeys.Add("FAuxPropId");
            e.FieldKeys.Add("FAuxUnitQty");
            e.FieldKeys.Add("FAuxUnitID");
            e.FieldKeys.Add("FExtAuxUnitQty");
            e.FieldKeys.Add("FSoorDerno");
            e.FieldKeys.Add("FLot");
            e.FieldKeys.Add("FRealQty");
            e.FieldKeys.Add("FExpiryDate");
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FPriceUnitQty");

            e.FieldKeys.Add("FSettleCurrID");

            e.FieldKeys.Add("FSerialNo");

            e.FieldKeys.Add("FDealId");
            e.FieldKeys.Add("FSaleOwnerId");
            e.FieldKeys.Add("FProductId");
            e.FieldKeys.Add("FZohoContact");
            e.FieldKeys.Add("FZohoBillNo");
            e.FieldKeys.Add("FZohoShipmentNo");
        }

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
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
                sb.AppendLine($@"接口方向：Kingdee --> CRM");
                sb.AppendLine($@"接口名称：销售出库API");
                try
                {
                    Shipment shipment = new Shipment();

                    #region 单据头
                    //Erp编号
                    shipment.erp_shipment_no = billObj["BillNo"].ToString();
                    //创建人
                    DynamicObject creator = billObj["FCreatorId"] as DynamicObject;
                    shipment.CreatedBy = creator["Name"].ToString();
                    shipment.CreatedByID = creator["Id"].ToString();
                    //销售组织
                    DynamicObject org = billObj["SaleOrgId"] as DynamicObject;
                    if (org != null)
                    {
                        shipment.Medtrum_Organization = org["Name"].ToString();
                        if (org["Number"].ToString()== "1002")
                        {
                            continue;
                        }
                    }

                    //处理人
                    shipment.DealID = billObj["FDealId"].ToString();

                    //销售员
                    DynamicObject saler = billObj["SalesManID"] as DynamicObject;
                    shipment.shipment_owner_name = saler["Name"].ToString();
                    shipment.shipment_owner_id = billObj["FSaleOwnerId"].ToString();
                    //收货方联系人.
                    //DynamicObject contact = billObj["FZohoContact"] as DynamicObject;
                    DynamicObject contact = billObj["ReceiverContactID"] as DynamicObject;

                    if (contact != null)
                    {
                        shipment.ship_to_contact_name = contact["Name"].ToString();
                        shipment.contact_erp_id = contact["Number"].ToString();
                        shipment.ship_to_contact_id = contact["FZohoId"].ToString();
                    }
                    //收货方
                    DynamicObject receive = billObj["ReceiverID"] as DynamicObject;
                    shipment.ship_to_account_name = receive["Name"].ToString();
                    shipment.account_erp_id = receive["Number"].ToString();
                    if (receive["FZohoId"] != null)
                    {
                        shipment.ship_to_account_id = receive["FZohoId"].ToString();
                    }

                    //日期
                    shipment.delivery_date = billObj["Date"].ToString();
                    //承运商
                    DynamicObject carrier = billObj["CarrierID"] as DynamicObject;
                    if (carrier != null)
                    {
                        shipment.Carrier = carrier["Name"].ToString();
                    }
                    //运输单号
                    shipment.TackingNo = billObj["CarriageNO"].ToString();
                    //运输公司
                    shipment.TackingCompany = billObj["F_aaaa_Text6"].ToString();
                    //类型
                    shipment.Type = "Order";
                    #endregion

                    #region 明细信息
                    DynamicObjectCollection entrys = billObj["SAL_OUTSTOCKENTRY"] as DynamicObjectCollection;
                    List<ShipmentEntry> shipmentEntrys = new List<ShipmentEntry>();
                    foreach (var entry in entrys)
                    {
                        ShipmentEntry shipmentEntry = new ShipmentEntry();

                        //明细ID
                        shipmentEntry.EntryId = entry["Id"].ToString();

                        //结算币别
                        DynamicObjectCollection finEntrys = billObj["SAL_OUTSTOCKFIN"] as DynamicObjectCollection;
                        DynamicObject currency = finEntrys[0]["SettleCurrID"] as DynamicObject;
                        shipmentEntry.Currency
                            = SqlHelper.GetCurrencyEngName(this.Context, currency["Id"].ToString());

                        //物料
                        DynamicObject material = entry["MaterialID"] as DynamicObject;
                        shipmentEntry.product_name = material["Name"].ToString();
                        shipmentEntry.MaterialCode = material["Number"].ToString();
                        shipmentEntry.product_id = entry["FProductId"].ToString();

                        DynamicObject auxPropId = entry["AuxPropId"] as DynamicObject;
                        if (auxPropId != null)
                        {
                            //Color
                            DynamicObject F100003
                                = auxPropId["F100003"] as DynamicObject;
                            if (F100003 != null)
                            {
                                shipmentEntry.AttrRegion = F100003["FDataValue"].ToString();
                            }
                            //Region
                            DynamicObject F100004
                                = auxPropId["F100004"] as DynamicObject;
                            if (F100004 != null)
                            {
                                shipmentEntry.AttrColor = F100004["FDataValue"].ToString();
                            }
                        }

                        //Lot
                        DynamicObject lot = entry["Lot"] as DynamicObject;
                        if (lot != null)
                        {
                            shipmentEntry.LotNo = lot["Number"].ToString();
                        }

                        //规格型号
                        shipmentEntry.model = material["Specification"].ToString();
                        //实发数量
                        shipmentEntry.Qty_pcs = Convert.ToDecimal(entry["RealQty"]);
                        //库存辅单位
                        DynamicObject auxUnitID = entry["AuxUnitID"] as DynamicObject;
                        if (auxUnitID != null)
                        {
                            shipmentEntry.Unit = auxUnitID["Name"].ToString();
                        }

                        //有效期至
                        if (entry["ExpiryDate"] != null)
                        {
                            shipmentEntry.ExpiryDate = entry["ExpiryDate"].ToString();
                        }

                        //实发数量(辅单位)
                        shipmentEntry.Qty_box = Convert.ToDecimal(entry["ExtAuxUnitQty"]);

                        //计价数量
                        shipmentEntry.Qty_price = Convert.ToDecimal(entry["PriceUnitQty"]);

                        shipmentEntry.SalesOrderID = entry["FZohoBillNo"].ToString();

                        #region 序列号
                        List<SNTracking> snTrackings = new List<SNTracking>();
                        DynamicObjectCollection snTrackingEntrys = entry["SAL_OutStockSerial"] as DynamicObjectCollection;
                        if (snTrackingEntrys != null)
                        {
                            foreach (var snTrackingEntry in snTrackingEntrys)
                            {
                                if (snTrackingEntry["SerialNo"] == null)
                                {
                                    continue;
                                }

                                SNTracking snTracking = new SNTracking();
                                snTracking.EntryId = snTrackingEntry["Id"].ToString();
                                string serialNo = snTrackingEntry["SerialNo"].ToString();
                                string[] serialNos = null;
                                if (serialNo.Contains("#3D"))
                                {
                                    serialNos = serialNo.Split(new string[] { "#3D" }, StringSplitOptions.None);
                                    snTracking.transmitter_sn = serialNos[1].Split('$')[0];
                                    serialNo = serialNos[0];
                                }
                                if (serialNo.Contains("#2D"))
                                {
                                    serialNos = serialNo.Split(new string[] { "#2D" }, StringSplitOptions.None);
                                    snTracking.pump_base_sn = serialNos[1].Split('$')[0];
                                    serialNo = serialNos[0];
                                }
                                if (serialNo.Contains("#1D"))
                                {
                                    serialNos = serialNo.Split(new string[] { "#1D" }, StringSplitOptions.None);
                                    snTracking.pdm_sn = serialNos[1].Split('$')[0];
                                    serialNo = serialNos[0];
                                }

                                if (auxPropId != null)
                                {
                                    //Ver1
                                    DynamicObject F100006
                                        = auxPropId["F100006"] as DynamicObject;
                                    if (F100006 != null)
                                    {
                                        snTracking.pdm_ver = F100006["FDataValue"].ToString();
                                    }
                                    //Ver2
                                    DynamicObject F100007
                                        = auxPropId["F100007"] as DynamicObject;
                                    if (F100007 != null)
                                    {
                                        snTracking.pump_base_ver = F100007["FDataValue"].ToString();
                                    }
                                    //Ver3
                                    DynamicObject F100008
                                        = auxPropId["F100008"] as DynamicObject;
                                    if (F100008 != null)
                                    {
                                        snTracking.transmitter_ver = F100008["FDataValue"].ToString();
                                    }
                                }
                                snTrackings.Add(snTracking);
                            }
                        }

                        shipmentEntry.SNTrackings = snTrackings;
                        #endregion
                        shipmentEntrys.Add(shipmentEntry);
                    }
                    shipment.ShipmentEntrys = shipmentEntrys;
                    #endregion

                    //string zohoShipmentNo = billObj["FZohoShipmentNo"].ToString();
                    //if (!string.IsNullOrWhiteSpace(zohoShipmentNo))
                    //{
                    //    shipment.shipment_zoho_id = zohoShipmentNo;
                    //    url = "https://zoho.onetrum.com/public/index.php/api/index/insert_shipment";
                    //}

                    string json = JsonHelper.ToJSON(shipment);
                    sb.AppendLine($@"请求信息：{json}");
                    string response = ApiHelper.HttpPost(url, json);
                    sb.AppendLine($@"返回信息：{response}");

                    #region 解析返回信息
                    JObject model = JObject.Parse(response);
                    if (model["code"] != null)
                    {
                        if (model["code"].ToString() == "200")
                        {
                            string shipment_info = "";
                            if (model["shipment_info"] != null)
                            {
                                shipment_info = model["shipment_info"].ToString();
                                SqlHelper.UpdateOutStock(this.Context, billObj["Id"].ToString(), shipment_info);
                                billObj["FZohoShipmentNo"] = shipment_info;
                            }
                        }
                        else
                        {
                            throw new KDException("错误", response);
                        }
                    }
                    else
                    {
                        throw new KDException("错误", response);
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
