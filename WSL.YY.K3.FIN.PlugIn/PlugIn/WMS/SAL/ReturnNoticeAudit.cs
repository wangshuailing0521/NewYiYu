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
    [Description("退货通知单审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class ReturnNoticeAudit: AbstractOperationServicePlugIn
    {
        string url = "https://wms-test.medtrum.com/SCM.WMS7.WebApi/WMS/SaveReceipt";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FStockId");
            e.FieldKeys.Add("FOwnerID");
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FCreatorId");
            e.FieldKeys.Add("FCreateDate");
            e.FieldKeys.Add("FApproveDate");
            e.FieldKeys.Add("FRetorgId");
            e.FieldKeys.Add("FRetDeptId");
            e.FieldKeys.Add("FSaledeptid");
            e.FieldKeys.Add("FPurchaserId");
            e.FieldKeys.Add("F_aaaa_Base");
            e.FieldKeys.Add("FRetcustId");
            e.FieldKeys.Add("FSalesManId");

            e.FieldKeys.Add("FDeliverydate");
            e.FieldKeys.Add("FMaterialId");
            e.FieldKeys.Add("FStockUnitID");
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FQty");
            e.FieldKeys.Add("FLot");
            e.FieldKeys.Add("FEntryDescription");
            e.FieldKeys.Add("FOrderNo");
            e.FieldKeys.Add("FAuxpropId");
            e.FieldKeys.Add("FIsFree");
            e.FieldKeys.Add("FStockStatusId");
            e.FieldKeys.Add("FPRODUCEDATE");
            e.FieldKeys.Add("FExpiryDate");
            e.FieldKeys.Add("FSrcBillNo");
            e.FieldKeys.Add("F_ora_Base");
            e.FieldKeys.Add("FPriceUnitId");
            e.FieldKeys.Add("FPriceUnitQty");

            e.FieldKeys.Add("FRmType");
            e.FieldKeys.Add("FSettleCurrId");
            e.FieldKeys.Add("FExchangeTypeId");
            e.FieldKeys.Add("FExchangeRate");
            e.FieldKeys.Add("FEntryTaxRate");
            e.FieldKeys.Add("FTaxPrice");
            
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
                sb.AppendLine($@"接口名称：退货通知API");

                try
                {
                    DynamicObjectCollection entrys
                        = billObj["SAL_RETURNNOTICEENTRY"] as DynamicObjectCollection;

                    DynamicObjectCollection fins
                       = billObj["SAL_RETURNNOTICEFIN"] as DynamicObjectCollection;

                    WmsInStock wmsInStock = new WmsInStock();
                    wmsInStock.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");

                    receiptEditDTO wmsEntry = new receiptEditDTO();

                    DynamicObject billType = billObj["BillTypeID"] as DynamicObject;
                    wmsEntry.RECEIPT_TYPE_SC = "销售退货";

                    //退货原因

                    wmsEntry.EXTERNAL_RECEIPT_ID = billObj["BillNo"].ToString();
                    wmsEntry.EXTERNAL_RECEIPT_ID2 = billObj["Id"].ToString();

                    DynamicObject created = billObj["FCreatorId"] as DynamicObject;
                    //wmsEntry.CREATED_BY = created["Name"].ToString();
                    wmsEntry.CREATED_DATE = Convert.ToDateTime(billObj["ApproveDate"]);

                    DynamicObject stockOrg = billObj["RetorgId"] as DynamicObject;
                    wmsEntry.R_UDF1 = stockOrg["Number"].ToString();
                    wmsEntry.OWNER_ID = stockOrg["Number"].ToString();

                    DynamicObject SettleCurrId = fins[0]["SettleCurrId"] as DynamicObject;
                    if (SettleCurrId != null)
                    {
                        wmsEntry.R_UDF5 = SettleCurrId["Number"].ToString();
                    }

                    DynamicObject FExchangeTypeId = fins[0]["ExchangeTypeId"] as DynamicObject;
                    if (FExchangeTypeId != null)
                    {
                        wmsEntry.R_UDF7 = FExchangeTypeId["Number"].ToString();
                    }
                    wmsEntry.R_UDF6 = fins[0]["ExchangeRate"].ToString();

                    DynamicObject dept = billObj["Sledeptid"] as DynamicObject;
                    if (dept != null)
                    {
                        wmsEntry.R_UDF2 = dept["Number"].ToString();
                    }


                    DynamicObject man = billObj["SalesManId"] as DynamicObject;
                    if (man != null)
                    {
                        wmsEntry.R_UDF3 = man["Number"].ToString();
                    }

                    //DynamicObject F_aaaa_Base = billObj["F_aaaa_Base"] as DynamicObject;
                    //if (F_aaaa_Base != null)
                    //{
                    //    wmsEntry.R_UDF4 = F_aaaa_Base["Number"].ToString();
                    //}

                    DynamicObject supplier = billObj["RetcustId"] as DynamicObject;
                    wmsEntry.VENDOR_ID = supplier["Number"].ToString();

                    List<ReceiptDetailList> detailList = new List<ReceiptDetailList>();
                    foreach (var entry in entrys)
                    {
                        DynamicObject FRmType = entry["RmType"] as DynamicObject;
                        if (FRmType != null)
                        {
                            wmsEntry.RETURN_REASON_SC = FRmType["FDataValue"].ToString();
                        }

                        string orgNumber = stockOrg["Number"].ToString();
                        WMSStock wmsStock = SqlHelper.GetWmsStockNumber(this.Context, orgNumber);

                        wmsEntry.WH_ID = wmsStock.Number;
                        wmsInStock.whgid = wmsStock.Number;
                     
                        wmsEntry.WH_ID = "medtrum.wh1";
                        wmsInStock.whgid = "medtrum.wh1";

                        wmsEntry.EXPECTED_ARRIVAL_DATE
                            = Convert.ToDateTime(entry["Deliverydate"].ToString());

                        ReceiptDetailList detail = new ReceiptDetailList();

                        detail.RD_UDF1 = entry["TaxRate"].ToString();
                        detail.RD_UDF2 = entry["TaxPrice"].ToString();

                        detail.EXTERNAL_LINE_ID = entry["Id"].ToString();

                        DynamicObject material = entry["MaterialId"] as DynamicObject;
                        detail.SKU_ID = material["Number"].ToString();

                        DynamicObject StockUnitID = entry["StockUnitID"] as DynamicObject;
                        detail.PACK_ID = StockUnitID["Number"].ToString();

                        DynamicObject ExtAuxUnitId = entry["PriceUnitId"] as DynamicObject;
                        detail.UOM_ID = ExtAuxUnitId["Name"].ToString();

                        DynamicObject PriceUnitId = entry["PriceUnitId"] as DynamicObject;
                        detail.RD_UDF3 = PriceUnitId["Number"].ToString();

                        detail.EXPECTED_QTY = Convert.ToDecimal(entry["PriceUnitQty"]);

                        DynamicObject lot = entry["Lot"] as DynamicObject;
                        if (lot != null)
                        {
                            detail.EXTERNAL_LOT = lot["Number"].ToString();
                        }

                        detail.RD_REMARK = entry["Description"].ToString();
                        detail.EXTERNAL_RECEIPT_ID = entry["SrcBillNo"].ToString();

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
