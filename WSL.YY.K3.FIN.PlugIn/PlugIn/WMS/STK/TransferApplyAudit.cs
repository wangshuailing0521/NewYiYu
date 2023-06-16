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

namespace WSL.YY.K3.FIN.PlugIn.PlugIn.WMS.STK
{
    [Description("调拨申请单审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class TransferApplyAudit: AbstractOperationServicePlugIn
    {
        string outUrl = "https://wms-test.medtrum.com/SCM.WMS7.WebApi/WMS/SaveShippingOrder";
        string inUrl = "https://wms-test.medtrum.com/SCM.WMS7.WebApi/WMS/SaveReceipt";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FStockId");
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FCreatorId");
            e.FieldKeys.Add("FCreateDate");
            e.FieldKeys.Add("FApproveDate");
            e.FieldKeys.Add("FAPPORGID");
            e.FieldKeys.Add("FZohoContact");
            e.FieldKeys.Add("FPaitner");
            e.FieldKeys.Add("FTRANSTYPE");

            e.FieldKeys.Add("FStockOrgId");
            e.FieldKeys.Add("FMaterialId");
            e.FieldKeys.Add("FUNITID");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FLot");
            e.FieldKeys.Add("FAuxpropId");
            e.FieldKeys.Add("FStockStatusId");
            e.FieldKeys.Add("FProduceDate");
            e.FieldKeys.Add("FExpiryDate");
            e.FieldKeys.Add("FBOMID");
            e.FieldKeys.Add("FSecUnitID");
            e.FieldKeys.Add("FExtAuxUnitId");
            e.FieldKeys.Add("FExtAuxUnitQty");

            e.FieldKeys.Add("FStockOrgInId");
            e.FieldKeys.Add("FStockStatusInId");
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
                sb.AppendLine($@"接口名称：调拨申请API");
                try
                {
                    DynamicObjectCollection entrys
                        = billObj["STK_STKTRANSFERAPPENTRY"] as DynamicObjectCollection;

                    #region 传递出库
                    WmsOutStock wmsObject = new WmsOutStock();
                    wmsObject.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");

                    shippingorderEditDTO wmsEntry = new shippingorderEditDTO();

                    DynamicObject billType = billObj["FBillTypeID"] as DynamicObject;
                    
                    wmsEntry.ORDER_TYPE_SC = billType["Name"].ToString();
                    //wmsEntry.ORDER_TYPE_SC = "调拨申请";
                    wmsEntry.SHIPPING_ORDER_SOURCE_SC = "ERP";
                    wmsEntry.EXTERNAL_ORDER_ID = billObj["BillNo"].ToString();
                    wmsEntry.EXTERNAL_ORDER_ID2 = billObj["Id"].ToString();

                    

                    DynamicObject created = billObj["FCreatorId"] as DynamicObject;
                    //wmsEntry.CREATED_BY = created["Name"].ToString();
                    wmsEntry.CREATED_DATE = Convert.ToDateTime(billObj["APPROVEDATE"]);

                    //病人ID需要加字段
                    DynamicObject FPaitner = billObj["FPaitner"] as DynamicObject;
                    if (FPaitner != null)
                    {
                        wmsEntry.CUSTOMER_REF = FPaitner["FDataValue"].ToString();
                    }

                    //实际收货人
                    DynamicObject FReceiverContactID = billObj["FZohoContact"] as DynamicObject;
                    if (FReceiverContactID != null)
                    {
                        wmsEntry.DEST_CONTACT_NAME = FReceiverContactID["Name"].ToString();
                    }


                    List<ShippingOrderDetailList> detailList = new List<ShippingOrderDetailList>();
                    foreach (var entry in entrys)
                    {
                        DynamicObject stock = entry["STOCKORGID"] as DynamicObject;
                        if (stock != null)
                        {
                            string orgNumber = stock["Number"].ToString();
                            WMSStock wmsStock = SqlHelper.GetWmsStockNumber(this.Context, orgNumber);
                            wmsEntry.SO_UDF1 = stock["Number"].ToString();
                            wmsEntry.OWNER_ID = stock["Number"].ToString();
                            wmsEntry.WH_ID = wmsStock.Number;
                            wmsObject.whgid = wmsStock.Number;
                        }

                        DynamicObject stockOrg = entry["STOCKORGINID"] as DynamicObject;
                        if (stockOrg != null)
                        {
                            string orgNumber = stockOrg["Number"].ToString();
                            WMSStock wmsStock = SqlHelper.GetWmsStockNumber(this.Context, orgNumber);
                            wmsEntry.DEST_WH_GID = wmsStock.Number;
                            wmsEntry.DEST_WH_ID = wmsStock.Name;

                            //调拨类型
                            string FTransferBizType = billObj["TRANSTYPE"].ToString();
                            if (FTransferBizType != "0")//跨组织
                            {
                                wmsEntry.SO_UDF17 = orgNumber;
                            }
                        }

                        ShippingOrderDetailList detail = new ShippingOrderDetailList();

                        detail.EXTERNAL_LINE_ID = entry["Id"].ToString();

                        DynamicObject material = entry["MaterialId"] as DynamicObject;
                        detail.SKU_ID = material["Number"].ToString();

                        DynamicObject StockUnitID = entry["UNITID"] as DynamicObject;
                        if (StockUnitID != null)
                        {
                            detail.PACK_ID = StockUnitID["Number"].ToString();
                        }

                        DynamicObject ExtAuxUnitId = entry["ExtAuxUnitId"] as DynamicObject;
                        detail.UOM_ID = StockUnitID["Name"].ToString();

                        detail.ORDER_QTY = Convert.ToDecimal(entry["Qty"]);
                        if (ExtAuxUnitId != null)
                        {
                            detail.UOM_ID = ExtAuxUnitId["Name"].ToString();
                            detail.ORDER_QTY = Convert.ToDecimal(entry["ExtAuxUnitQty"]);
                        }

                        DynamicObject lot = entry["FLot"] as DynamicObject;
                        if (lot != null)
                        {
                            detail.EXTERNAL_LOT = lot["Number"].ToString();
                        }

                        //欧洲套装编号（BOM编号）
                        DynamicObject FBomID = entry["BOMID"] as DynamicObject;
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

                        DynamicObject StockStatusId = entry["StockStatusId"] as DynamicObject;
                        if (StockStatusId != null)
                        {
                            detail.SKU_PROPERTY = StockStatusId["Name"].ToString();
                        }

                        if (entry["ProduceDate"] != null)
                        {
                            detail.PRODUCE_DATE = entry["ProduceDate"].ToString();
                        }

                        if (entry["ExpiryDate"] != null)
                        {
                            detail.EXPIRE_DATE = entry["ExpiryDate"].ToString();
                        }

                        detailList.Add(detail);
                    }

                    wmsEntry.ShippingOrderDetailList = detailList;
                    wmsObject.shippingorderEditDTO = wmsEntry;

                    string json = JsonHelper.ToJSON(wmsObject);
                    sb.AppendLine($@"请求信息：{json}");
                    string response = ApiHelper.HttpPost(outUrl, json);
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
                    #endregion

                    #region 传递入库
                    WmsInStock wmsInStock = new WmsInStock();
                    wmsInStock.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");

                    receiptEditDTO wmsInEntry = new receiptEditDTO();

                    wmsInEntry.RECEIPT_TYPE_SC = billType["Name"].ToString();
                    wmsInEntry.RECEIPT_TYPE_SC = "转仓入库";
                    //wmsInEntry.RECEIPT_TYPE_SC = "采购入库";
                    wmsInEntry.EXTERNAL_RECEIPT_ID = billObj["BillNo"].ToString();
                    wmsInEntry.EXTERNAL_RECEIPT_ID2 = billObj["Id"].ToString();
                    //wmsInEntry.CREATED_BY = created["Name"].ToString();
                    wmsInEntry.CREATED_DATE = Convert.ToDateTime(billObj["APPROVEDATE"]);

                    //DynamicObject supplier = billObj["SupplierId"] as DynamicObject;
                    //wmsInEntry.VENDOR_ID = supplier["Number"].ToString();

                    List<ReceiptDetailList> detaiInlList = new List<ReceiptDetailList>();
                    foreach (var entry in entrys)
                    {
                        DynamicObject stockOrg = entry["STOCKORGINID"] as DynamicObject;
                        if (stockOrg != null)
                        {
                            string orgNumber = stockOrg["Number"].ToString();
                            WMSStock wmsStock = SqlHelper.GetWmsStockNumber(this.Context, orgNumber);
                            wmsEntry.WH_ID = wmsStock.Number;
                            wmsInStock.whgid = wmsStock.Number;

                            wmsInEntry.R_UDF1 = stockOrg["Number"].ToString();
                            wmsInEntry.OWNER_ID = stockOrg["Number"].ToString();
                        }                      

                        ReceiptDetailList detail = new ReceiptDetailList();

                        detail.EXTERNAL_LINE_ID = entry["Id"].ToString();

                        DynamicObject material = entry["MaterialID"] as DynamicObject;
                        detail.SKU_ID = material["Number"].ToString();

                        DynamicObject SecUnitID = entry["UNITID"] as DynamicObject;
                        if (SecUnitID != null)
                        {
                            detail.PACK_ID = SecUnitID["Number"].ToString();
                        }                        

                        DynamicObject ExtAuxUnitId = entry["ExtAuxUnitId"] as DynamicObject;
                        detail.UOM_ID = SecUnitID["Name"].ToString();

                        detail.EXPECTED_QTY = Convert.ToDecimal(entry["Qty"]);
                        if (ExtAuxUnitId != null)
                        {
                            detail.UOM_ID = ExtAuxUnitId["Name"].ToString();

                            detail.EXPECTED_QTY = Convert.ToDecimal(entry["ExtAuxUnitQty"]);
                        }

                        DynamicObject lot = entry["FLot"] as DynamicObject;
                        if (lot != null)
                        {
                            detail.EXTERNAL_LOT = lot["Number"].ToString();
                        }

                        DynamicObject auxPropId = entry["AuxPropId"] as DynamicObject;
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

                        DynamicObject StockStatusId = entry["StockStatusInId"] as DynamicObject;
                        if (StockStatusId != null)
                        {
                            detail.SKU_PROPERTY = StockStatusId["Number"].ToString();
                        }

                        if (entry["ProduceDate"] != null)
                        {
                            detail.PRODUCE_DATE = entry["ProduceDate"].ToString();
                        }

                        if (entry["ExpiryDate"] != null)
                        {
                            detail.EXPIRE_DATE = entry["ExpiryDate"].ToString();
                        }

                        detaiInlList.Add(detail);
                    }

                    wmsInEntry.ReceiptDetailList = detaiInlList;
                    wmsInStock.receiptEditDTO = wmsInEntry;

                    json = JsonHelper.ToJSON(wmsInStock);
                    sb.AppendLine($@"请求信息：{json}");
                    response = ApiHelper.HttpPost(inUrl, json);
                    sb.AppendLine($@"返回信息：{response}");

                    #region 解析返回信息
                    result = JsonHelper.FromJSON<WMSResult>(response);
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
