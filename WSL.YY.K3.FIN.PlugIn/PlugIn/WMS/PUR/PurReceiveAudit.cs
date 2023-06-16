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
    [Description("收料通知单审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class PurReceiveAudit: AbstractOperationServicePlugIn
    {
        string url = "https://wms-test.medtrum.com/SCM.WMS7.WebApi/WMS/SaveReceipt";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FStockID");
            e.FieldKeys.Add("FOwnerId");
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FCreatorId");
            e.FieldKeys.Add("FCreateDate");
            e.FieldKeys.Add("FApproveDate");
            e.FieldKeys.Add("FStockOrgId");
            e.FieldKeys.Add("FReceiveDeptId");
            e.FieldKeys.Add("FPurDeptId");
            e.FieldKeys.Add("FPurchaserId");
            e.FieldKeys.Add("F_aaaa_Base");
            e.FieldKeys.Add("FSupplierId");
            e.FieldKeys.Add("FPreDeliveryDate");
            e.FieldKeys.Add("FMaterialID");
            e.FieldKeys.Add("FStockUnitID");
            e.FieldKeys.Add("FExtAuxUnitId");
            e.FieldKeys.Add("FExtAuxUnitQty");
            e.FieldKeys.Add("FLot");
            e.FieldKeys.Add("FDescription");
            e.FieldKeys.Add("FOrderBillNo");
            e.FieldKeys.Add("FAuxPropId");
            e.FieldKeys.Add("FGiveAway");
            e.FieldKeys.Add("FStockStatusId");
            e.FieldKeys.Add("FProduceDate");
            e.FieldKeys.Add("FExpiryDate");
            e.FieldKeys.Add("FSettleCurrId");
            e.FieldKeys.Add("FExchangeTypeId");
            e.FieldKeys.Add("FExchangeRate");
            e.FieldKeys.Add("FEntryTaxRate");
            e.FieldKeys.Add("FTaxPrice");
            e.FieldKeys.Add("FPriceUnitId");
            e.FieldKeys.Add("FActReceiveQty");
            e.FieldKeys.Add("FPriceUnitQty");
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
                sb.AppendLine($@"接口名称：收料通知API");

                try
                {
                    DynamicObjectCollection entrys
                        = billObj["PUR_ReceiveEntry"] as DynamicObjectCollection;

                    DynamicObjectCollection fins
                       = billObj["Receivefinance"] as DynamicObjectCollection;

                    WmsInStock wmsInStock = new WmsInStock();
                    wmsInStock.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");

                    receiptEditDTO wmsEntry = new receiptEditDTO();

                    wmsEntry.RECEIPT_TYPE_SC = "采购入库";
                    wmsEntry.EXTERNAL_RECEIPT_ID = billObj["BillNo"].ToString();
                    wmsEntry.EXTERNAL_RECEIPT_ID2 = billObj["Id"].ToString();

                    DynamicObject created = billObj["CreatorId"] as DynamicObject;
                    //wmsEntry.CREATED_BY = created["Name"].ToString();
                    wmsEntry.CREATED_DATE = Convert.ToDateTime(billObj["ApproveDate"]);

                    DynamicObject stockOrg = billObj["StockOrgId"] as DynamicObject;
                    wmsEntry.R_UDF1 = stockOrg["Number"].ToString();
                    wmsEntry.OWNER_ID = stockOrg["Number"].ToString();

                    DynamicObject dept = billObj["ReceiveDeptId"] as DynamicObject;
                    if (dept != null)
                    {
                        wmsEntry.R_UDF2 = dept["Number"].ToString();
                    }

                    DynamicObject SettleCurrId = fins[0]["SettleCurrId"] as DynamicObject;
                    if (SettleCurrId != null)
                    {
                        wmsEntry.R_UDF5 = SettleCurrId["Number"].ToString();
                    }

                    DynamicObject ExchangeTypeId = fins[0]["ExchangeTypeId"] as DynamicObject;
                    if (ExchangeTypeId != null)
                    {
                        wmsEntry.R_UDF7 = ExchangeTypeId["Number"].ToString();
                    }
                    wmsEntry.R_UDF6 = fins[0]["ExchangeRate"].ToString();

                    DynamicObject PurchaserId = billObj["PurchaserId"] as DynamicObject;
                    if (PurchaserId != null)
                    {
                        wmsEntry.R_UDF3 = PurchaserId["Number"].ToString();
                    }
                   
                    DynamicObject F_aaaa_Base = billObj["F_aaaa_Base"] as DynamicObject;
                    if (F_aaaa_Base != null)
                    {
                        wmsEntry.R_UDF4 = F_aaaa_Base["Name"].ToString();
                    }
                    
                    DynamicObject supplier = billObj["SupplierId"] as DynamicObject;
                    wmsEntry.VENDOR_ID = supplier["Number"].ToString();

                    List<ReceiptDetailList> detailList = new List<ReceiptDetailList>();
                    foreach (var entry in entrys)
                    {
                       
                            string orgNumber = stockOrg["Number"].ToString();
                            WMSStock wmsStock = SqlHelper.GetWmsStockNumber(this.Context, orgNumber);

                            wmsEntry.WH_ID = wmsStock.Number;
                        wmsInStock.whgid = wmsStock.Number;


                        wmsEntry.EXPECTED_ARRIVAL_DATE 
                            = Convert.ToDateTime(entry["PreDeliveryDate"].ToString());

                        ReceiptDetailList detail = new ReceiptDetailList();

                        detail.RD_UDF1 = entry["TaxRate"].ToString();
                        detail.RD_UDF2 = entry["TaxPrice"].ToString();

                        detail.EXTERNAL_LINE_ID = entry["Id"].ToString();

                        DynamicObject material = entry["MaterialID"] as DynamicObject;
                        detail.SKU_ID = material["Number"].ToString();

                        string zjff = material["F_aaaa_Combo"].ToString();
                        if (zjff == "免检" || zjff =="机物料" || string.IsNullOrWhiteSpace(zjff))
                        {
                            wmsEntry.RECEIPT_TYPE_SC = "采购入库";
                        }

                        if (zjff == "全检")
                        {
                            wmsEntry.RECEIPT_TYPE_SC = "全检采购入库";
                        }

                        if (zjff == "抽检")
                        {
                            wmsEntry.RECEIPT_TYPE_SC = "抽检采购入库";
                        }

                        DynamicObject StockUnitID = entry["StockUnitID"] as DynamicObject;
                        detail.PACK_ID = StockUnitID["Number"].ToString();

                        DynamicObject PriceUnitId = entry["PriceUnitId"] as DynamicObject;
                        detail.RD_UDF3 = PriceUnitId["Number"].ToString();

                        DynamicObject ExtAuxUnitId = entry["ExtAuxUnitId"] as DynamicObject;
                        detail.UOM_ID = StockUnitID["Name"].ToString();
                        if (ExtAuxUnitId != null)
                        {
                            detail.UOM_ID = ExtAuxUnitId["Name"].ToString();
                        }

                        detail.EXPECTED_QTY = Convert.ToDecimal(entry["ActReceiveQty"]);

                        DynamicObject lot = entry["Lot"] as DynamicObject;
                        if (lot != null)
                        {
                            detail.EXTERNAL_LOT = lot["Number"].ToString();
                        }

                        detail.RD_REMARK = entry["Description"].ToString();
                        detail.EXTERNAL_RECEIPT_ID = entry["OrderBillNo"].ToString();

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
                                detail.LOT_ATTR03 = F100005["FDataValue"].ToString();
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

                        detail.LOT_ATTR08 = entry["GiveAway"].ToString();

                        DynamicObject StockStatusId = entry["StockStatusId"] as DynamicObject;
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


                       

                        detailList.Add(detail);                     
                    }

                    wmsEntry.ReceiptDetailList = detailList;
                    wmsInStock.receiptEditDTO =  wmsEntry ;

                    string json = JsonHelper.ToJSON(wmsInStock);
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
