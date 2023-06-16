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
    [Description("发货通知单审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class DeliveryNoticeAudit: AbstractOperationServicePlugIn
    {
        string url = "https://wms-test.medtrum.com/SCM.WMS7.WebApi/WMS/SaveShippingOrder";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FStockId");
            e.FieldKeys.Add("FOwnerId");
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FCreatorId");
            e.FieldKeys.Add("FCreateDate");
            e.FieldKeys.Add("FApproveDate");
            e.FieldKeys.Add("FSaleOrgId");
            e.FieldKeys.Add("FSaleDeptID");
            e.FieldKeys.Add("FDeliveryOrgID");
            e.FieldKeys.Add("FCarriageNO");
            e.FieldKeys.Add("FCustomerID");
            e.FieldKeys.Add("FReceiverContactID");
            e.FieldKeys.Add("FSalesManID");
            e.FieldKeys.Add("FSettleCurrID");
            e.FieldKeys.Add("FExchangeTypeID");
            e.FieldKeys.Add("FExchangeRate");
            e.FieldKeys.Add("FDealId");
            e.FieldKeys.Add("FSaleOwnerId");
            e.FieldKeys.Add("FShippingTitle");
            e.FieldKeys.Add("FZohoContact");
            e.FieldKeys.Add("FShippingEmail");
            e.FieldKeys.Add("FShippingAddress");
            e.FieldKeys.Add("FShippingPhone");
            e.FieldKeys.Add("FProductId");
            e.FieldKeys.Add("FZohoBillNo");
            e.FieldKeys.Add("F_ora_Text");
            e.FieldKeys.Add("FBillAllAmount");
            e.FieldKeys.Add("F_ora_Amount");
            e.FieldKeys.Add("F_ora_Amount1");
            e.FieldKeys.Add("FRECEIPTCONDITIONID");
            e.FieldKeys.Add("F_aaaa_Text2");
            e.FieldKeys.Add("F_aaaa_Text1");

            e.FieldKeys.Add("FBomID");
            e.FieldKeys.Add("FMaterialId");
            e.FieldKeys.Add("FStockUnitID");
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FLot");
            e.FieldKeys.Add("FAuxpropId");
            e.FieldKeys.Add("FIsFree");
            e.FieldKeys.Add("FStockStatusId");
            e.FieldKeys.Add("FPRODUCEDATE");
            e.FieldKeys.Add("FEXPIRYDATE");
            e.FieldKeys.Add("FEntryTaxRate");
            e.FieldKeys.Add("FTaxPrice");
            e.FieldKeys.Add("F_ora_Base");
            e.FieldKeys.Add("FPriceUnitId");
            e.FieldKeys.Add("FPriceUnitQty");
            e.FieldKeys.Add("FRowType");
            e.FieldKeys.Add("FAmount");
            e.FieldKeys.Add("FPrice");
            e.FieldKeys.Add("FDeliveryDate");
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
                sb.AppendLine($@"接口名称：发货通知API");
                try
                {
                    DynamicObjectCollection entrys
                        = billObj["SAL_DELIVERYNOTICEENTRY"] as DynamicObjectCollection;

                    DynamicObjectCollection fins
                       = billObj["SAL_DELIVERYNOTICEFIN"] as DynamicObjectCollection;

                    WmsOutStock wmsObject = new WmsOutStock();
                    wmsObject.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");

                    shippingorderEditDTO wmsEntry = new shippingorderEditDTO();

                    DynamicObject SettleCurrID = fins[0]["SettleCurrID"] as DynamicObject;
                    if (SettleCurrID != null)
                    {
                        wmsEntry.SO_UDF7 = SettleCurrID["Number"].ToString();
                    }
                    DynamicObject ExchangeTypeID = fins[0]["ExchangeTypeID"] as DynamicObject;
                    if (ExchangeTypeID != null)
                    {
                        wmsEntry.SO_UDF3 = ExchangeTypeID["Number"].ToString();
                    }

                    wmsEntry.SO_UDF20 = fins[0]["BillAllAmount"].ToString();
                    wmsEntry.SO_UDF21 = billObj["F_ora_Amount"].ToString();
                    wmsEntry.SO_UDF22 = billObj["F_ora_Amount1"].ToString();

                    wmsEntry.SO_UDF17 = fins[0]["ExchangeRate"].ToString();

                    DynamicObject billType = billObj["BillTypeID"] as DynamicObject;
                    wmsEntry.ORDER_TYPE_SC = billType["Name"].ToString();
                    
                    wmsEntry.ORDER_TYPE_SC = "销售出库";
                    if (billType["Number"].ToString() == "FHTZD02_SYS")
                    {
                        wmsEntry.ORDER_TYPE_SC = "寄售出库";
                    }

                    wmsEntry.SHIPPING_ORDER_SOURCE_SC = "ERP";
                    wmsEntry.EXTERNAL_ORDER_ID = billObj["BillNo"].ToString();
                    wmsEntry.EXTERNAL_ORDER_ID2 = billObj["Id"].ToString();
                    
                    wmsEntry.SO_UDF11 = billObj["FDealId"].ToString();
                    wmsEntry.SO_UDF12 = billObj["FSaleOwnerId"].ToString();
                    wmsEntry.DEST_CONTACT_NAME = billObj["FShippingTitle"].ToString();
                    wmsEntry.SO_UDF14 = billObj["FShippingTitle"].ToString();
                    wmsEntry.CUSTOMER_EMAIL = billObj["FShippingEmail"].ToString();
                    wmsEntry.DEST_ADDRESS = billObj["FShippingAddress"].ToString();
                    wmsEntry.SO_UDF13 = billObj["FShippingAddress"].ToString();
                    wmsEntry.CUSTOMER_PHONE = billObj["FShippingPhone"].ToString();

                    //实际收货人
                    DynamicObject FReceiverContactID = billObj["FZohoContact"] as DynamicObject;
                    if (FReceiverContactID != null)
                    {
                        wmsEntry.SO_UDF15 = FReceiverContactID["Name"].ToString();
                        //病人ID需要加字段
                        wmsEntry.CUSTOMER_REF = FReceiverContactID["Number"].ToString();
                    }
                    //wmsEntry.SO_UDF9 = billObj["Id"].ToString();

                    DynamicObject created = billObj["CreatorId"] as DynamicObject;
                    //wmsEntry.CREATED_BY = created["Name"].ToString();
                    wmsEntry.CREATED_DATE = Convert.ToDateTime(billObj["ApproveDate"]);

                    DynamicObject stockOrg = billObj["SaleOrgId"] as DynamicObject;
                    wmsEntry.SO_UDF1 = stockOrg["Number"].ToString();
                    wmsEntry.SO_UDF23 = stockOrg["F_AAAA_MULLANGTEXT6"].ToString();



                    DynamicObject DeliveryOrgID = billObj["DeliveryOrgID"] as DynamicObject;
                    wmsEntry.OWNER_ID = DeliveryOrgID["Number"].ToString();

                    wmsEntry.WAYBILL_NO = billObj["CarriageNO"].ToString();

                    DynamicObject CustomerID = billObj["CustomerID"] as DynamicObject;
                    wmsEntry.CUSTOMER_ID = CustomerID["Number"].ToString();
                    wmsEntry.CUSTOMER_NAME = CustomerID["Name"].ToString();
                    //saletoVAT
                    wmsEntry.SO_UDF27 = CustomerID["F_aaaa_Text"].ToString();

                    //销售部门
                    //DynamicObject dept = billObj["SaleDeptID"] as DynamicObject;
                    //if (dept != null)
                    //{
                    //    //wmsEntry.SO_UDF9 = dept["Number"].ToString();
                    //}

                    //销售员
                    DynamicObject man = billObj["SalesManId"] as DynamicObject;
                    if (man != null)
                    {
                        wmsEntry.SO_UDF10 = man["Number"].ToString();
                    }
                    //收款条件
                    DynamicObject FRECEIPTCONDITIONID = billObj["FRECEIPTCONDITIONID"] as DynamicObject;
                    if (FRECEIPTCONDITIONID != null)
                    {
                        wmsEntry.SO_UDF24 = FRECEIPTCONDITIONID["Number"].ToString();
                    }

                    //saleto电话
                    wmsEntry.SO_UDF25 = billObj["F_aaaa_Text2"].ToString();
                    //INVOICE Number
                    wmsEntry.SO_UDF29 = billObj["F_aaaa_Text1"].ToString();



                    List<ShippingOrderDetailList> detailList = new List<ShippingOrderDetailList>();
                    foreach (var entry in entrys)
                    {
                        
                        string orgNumber = stockOrg["Number"].ToString();
                        WMSStock wmsStock = SqlHelper.GetWmsStockNumber(this.Context, orgNumber);

                        wmsEntry.WH_ID = wmsStock.Number;
                        wmsObject.whgid = wmsStock.Number;

                        //寄售出库只穿递父物料
                        if (billType["Number"].ToString() == "FHTZD02_SYS")
                        {
                            var FRowType = entry["RowType"];
                            if (FRowType != null)
                            {
                                if (FRowType.ToString() == "Son" || FRowType.ToString() == "Service")
                                {
                                    continue;
                                }
                            }
                        }
                        //其他类型只穿递子物料
                        else
                        {
                            var FRowType = entry["RowType"];
                            if (FRowType != null)
                            {
                                if (FRowType.ToString() == "Parent" || FRowType.ToString() == "Service")
                                {
                                    continue;
                                }
                            }
                        }
                           

                        ShippingOrderDetailList detail = new ShippingOrderDetailList();

                        wmsEntry.SO_UDF9 = entry["FZohoBillNo"].ToString();
                        detail.SOD_UDF5 = entry["FProductId"].ToString();
                        wmsEntry.SO_ID = entry["F_ora_Text"].ToString();


                        detail.SOD_UDF1 = entry["TaxRate"].ToString();
                        detail.SOD_UDF2 = entry["TaxPrice"].ToString();
                        detail.SOD_UDF6 = entry["Price"].ToString();
                        detail.SOD_UDF7 = entry["Amount"].ToString();

                        detail.EXTERNAL_LINE_ID = entry["Id"].ToString();

                        DynamicObject material = entry["MaterialId"] as DynamicObject;
                        detail.SKU_ID = material["Number"].ToString();
                        detail.SO_UDF28 = material["F_aaaa_Text"].ToString();

                        DynamicObject StockUnitID = entry["StockUnitID"] as DynamicObject;
                        detail.PACK_ID = StockUnitID["Number"].ToString();

                        DynamicObject ExtAuxUnitId = entry["UnitID"] as DynamicObject;
                        detail.UOM_ID = ExtAuxUnitId["Number"].ToString();

                        DynamicObject PriceUnitId = entry["PriceUnitId"] as DynamicObject;
                        detail.SOD_UDF4 = PriceUnitId["Number"].ToString();
                        detail.UOM_ID = PriceUnitId["Name"].ToString();

                        detail.ORDER_QTY = Convert.ToDecimal(entry["Qty"]);

                        DynamicObject lot = entry["Lot"] as DynamicObject;
                        if (lot != null)
                        {
                            detail.EXTERNAL_LOT = lot["Number"].ToString();
                        }

                        //欧洲套装编号（BOM编号）
                        DynamicObject FBomID = entry["BomID"] as DynamicObject;
                        if (FBomID != null)
                        {
                            detail.OL_UDF1 = FBomID["Number"].ToString();
                        }
                        

                        DynamicObject auxPropId = entry["AuxpropId"] as DynamicObject;
                        if (auxPropId != null)
                        {
                            //国别
                            DynamicObject F100003
                                = auxPropId["F100003"] as DynamicObject;
                            if (F100003 != null)
                            {
                                detail.LOT_ATTR01 = F100003["FDataValue"].ToString();
                            }
                            //颜色
                            DynamicObject F100004
                                = auxPropId["F100004"] as DynamicObject;
                            if (F100004 != null)
                            {
                                detail.LOT_ATTR02 = F100004["FDataValue"].ToString();
                            }
                            //原材料版本号
                            DynamicObject F100005
                                = auxPropId["F100005"] as DynamicObject;
                            if (F100005 != null)
                            {
                                detail.LOT_ATTR03 = F100004["FDataValue"].ToString();
                            }
                            //产成品v1
                            DynamicObject F100006
                                = auxPropId["F100006"] as DynamicObject;
                            if (F100006 != null)
                            {
                                detail.LOT_ATTR04 = F100006["FDataValue"].ToString();
                            }
                            //产成品v2
                            DynamicObject F100007
                                = auxPropId["F100007"] as DynamicObject;
                            if (F100007 != null)
                            {
                                detail.LOT_ATTR05 = F100007["FDataValue"].ToString();
                            }
                            //产成品v3
                            DynamicObject F100008
                                = auxPropId["F100008"] as DynamicObject;
                            if (F100008 != null)
                            {
                                detail.LOT_ATTR06 = F100008["FDataValue"].ToString();
                            }
                            //box no
                            DynamicObject F100012
                                = auxPropId["F100012"] as DynamicObject;
                            if (F100012 != null)
                            {
                                detail.LOT_ATTR07 = F100012["FDataValue"].ToString();
                            }

                        }

                        detail.LOT_ATTR08 = entry["IsFree"].ToString();

                        DynamicObject StockStatusId = entry["F_ora_Base"] as DynamicObject;
                        if (StockStatusId != null)
                        {
                            detail.SKU_PROPERTY = StockStatusId["Name"].ToString();
                        }
                       

                        if (entry["PRODUCEDATE"] != null)
                        {
                            detail.PRODUCE_DATE = entry["PRODUCEDATE"].ToString();
                        }

                        if (entry["FEXPIRYDATE"] != null)
                        {
                            detail.EXPIRE_DATE = entry["FEXPIRYDATE"].ToString();
                        }

                        if (entry["DeliveryDate"] != null)
                        {
                            wmsEntry.REQUEST_SHIP_DATE = entry["DeliveryDate"].ToString();
                        }

                        detailList.Add(detail);
                    }

                    wmsEntry.ShippingOrderDetailList = detailList;
                    wmsObject.shippingorderEditDTO =  wmsEntry ;

                    string json = JsonHelper.ToJSON(wmsObject);
                    sb.AppendLine($@"请求信息：{json}");
                    string response = ApiHelper.HttpPost(url, json);
                    sb.AppendLine($@"返回信息：{response}");

                    #region 解析返回信息
                    WMSResult result = JsonHelper.FromJSON<WMSResult>(response);
                    if (!result.flag)
                    {
                        if (result.data != null)
                        {
                            throw new Exception(JsonHelper.ToJSON(result.data));
                        }
                        else
                        {
                            throw new Exception(result.msg);
                        }
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
