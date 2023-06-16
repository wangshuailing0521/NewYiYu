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
namespace WSL.YY.K3.FIN.PlugIn.PlugIn.WMS.PUR
{
    [Description("退料申请单审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class MRAPPAudit: AbstractOperationServicePlugIn
    {
        string url = "https://wms-test.medtrum.com/SCM.WMS7.WebApi/WMS/SaveShippingOrder";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FCreatorId");
            e.FieldKeys.Add("FCreateDate");
            e.FieldKeys.Add("FApproveDate");
            e.FieldKeys.Add("FModifyDate");

            e.FieldKeys.Add("FPURCHASEORGID");
            e.FieldKeys.Add("FAPPDEPTID");
            e.FieldKeys.Add("FAPPORGID");

            e.FieldKeys.Add("FSUPPLIERID");

            e.FieldKeys.Add("FMaterialId");
            e.FieldKeys.Add("FPURUNITID");
            e.FieldKeys.Add("FUNITID");
            e.FieldKeys.Add("FMRAPPQTY");
            e.FieldKeys.Add("FLot");
            e.FieldKeys.Add("FAuxpropId");
            e.FieldKeys.Add("FIsFree");
            e.FieldKeys.Add("F_ora_Base");
            e.FieldKeys.Add("FPRODUCEDATE");
            e.FieldKeys.Add("FEXPIRYDATE");
            e.FieldKeys.Add("FStockId");
            e.FieldKeys.Add("FPRICEQTY_F");
            e.FieldKeys.Add("FPRICEUNITID_F");

            e.FieldKeys.Add("FLOCALCURRID");
            e.FieldKeys.Add("FExchangeTypeId");
            e.FieldKeys.Add("FEntryTaxRate");
            e.FieldKeys.Add("FAPPROVEPRICE_F");
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
                sb.AppendLine($@"接口名称：退料申请API");
                try
                {
                    DynamicObjectCollection entrys
                        = billObj["PUR_MRAPPENTRY"] as DynamicObjectCollection;

                    DynamicObjectCollection fins
                        = billObj["PUR_MRAPPFIN"] as DynamicObjectCollection;

                    WmsOutStock wmsObject = new WmsOutStock();
                    wmsObject.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");

                    

                    shippingorderEditDTO wmsEntry = new shippingorderEditDTO();

                    DynamicObject curr = fins[0]["LOCALCURRID"] as DynamicObject;
                    if (curr != null)
                    {
                        wmsEntry.SO_UDF7 = curr["Number"].ToString();
                    }

                    DynamicObject ExchangeTypeId = fins[0]["ExchangeTypeId"] as DynamicObject;
                    if (ExchangeTypeId != null)
                    {
                        wmsEntry.SO_UDF3 = ExchangeTypeId["Number"].ToString();
                    }


                    //DynamicObject billType = billObj["BillTypeID"] as DynamicObject;
                    //wmsEntry.ORDER_TYPE_SC = billType["Name"].ToString();
                    wmsEntry.ORDER_TYPE_SC = "采购退货";

                    wmsEntry.SHIPPING_ORDER_SOURCE_SC = "ERP";
                    wmsEntry.EXTERNAL_ORDER_ID = billObj["BillNo"].ToString();
                    wmsEntry.EXTERNAL_ORDER_ID2 = billObj["Id"].ToString();

                    DynamicObject created = billObj["FCreatorId"] as DynamicObject;
                    if (created != null)
                    {
                        //wmsEntry.CREATED_BY = created["Name"].ToString();
                    }
                    
                    wmsEntry.CREATED_DATE = Convert.ToDateTime(billObj["ApproveDate"]);

                    DynamicObject stockOrg = billObj["PURCHASEORGID"] as DynamicObject;
                    wmsEntry.SO_UDF1 = stockOrg["Number"].ToString();

                    DynamicObject DeliveryOrgID = billObj["APPORGID"] as DynamicObject;
                    wmsEntry.OWNER_ID = DeliveryOrgID["Number"].ToString();

                    DynamicObject SUPPLIERID = billObj["SUPPLIERID"] as DynamicObject;
                    wmsEntry.CUSTOMER_ID = SUPPLIERID["Number"].ToString();
                    wmsEntry.CUSTOMER_NAME = SUPPLIERID["Name"].ToString();

                    //申请部门
                    DynamicObject dept = billObj["APPDEPTID"] as DynamicObject;
                    if (dept != null)
                    {
                        wmsEntry.SO_UDF9 = dept["Number"].ToString();
                    }

                    //实际退料日期
                    wmsEntry.UPDATED_DATE = Convert.ToDateTime(billObj["FModifyDate"]);

                    List<ShippingOrderDetailList> detailList = new List<ShippingOrderDetailList>();
                    foreach (var entry in entrys)
                    {
                       
                            string orgNumber = stockOrg["Number"].ToString();
                            WMSStock wmsStock = SqlHelper.GetWmsStockNumber(this.Context, orgNumber);

                            wmsEntry.WH_ID = wmsStock.Number;
                            wmsObject.whgid = wmsStock.Number;
                        

                        ShippingOrderDetailList detail = new ShippingOrderDetailList();

                        detail.EXTERNAL_LINE_ID = entry["Id"].ToString();

                        DynamicObject material = entry["MaterialId"] as DynamicObject;
                        detail.SKU_ID = material["Number"].ToString();

                        DynamicObject StockUnitID = entry["UNITID"] as DynamicObject;
                        if (StockUnitID != null)
                        {
                            detail.PACK_ID = StockUnitID["Number"].ToString();
                        }

                        DynamicObject PRICEUNITID = entry["PRICEUNITID_F"] as DynamicObject;
                        if (PRICEUNITID != null)
                        {
                            detail.SOD_UDF4 = PRICEUNITID["Number"].ToString();
                        }


                        DynamicObject ExtAuxUnitId = entry["PRICEUNITID_F"] as DynamicObject;
                        detail.UOM_ID = StockUnitID["Name"].ToString();
                        if (ExtAuxUnitId != null)
                        {
                            detail.UOM_ID = ExtAuxUnitId["Name"].ToString();
                           
                        }

                        detail.ORDER_QTY = Convert.ToDecimal(entry["PRICEQTY_F"]);

                        DynamicObject lot = entry["FLot"] as DynamicObject;
                        if (lot != null)
                        {
                            detail.EXTERNAL_LOT = lot["Number"].ToString();
                        }

                        DynamicObject StockStatusId = entry["F_ora_Base"] as DynamicObject;
                        if (StockStatusId != null)
                        {
                            detail.SKU_PROPERTY = StockStatusId["Name"].ToString();
                        }

                        detail.SOD_UDF1 = entry["TAXRATE"].ToString();
                        detail.SOD_UDF2 = entry["APPROVEPRICE_F"].ToString();

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
