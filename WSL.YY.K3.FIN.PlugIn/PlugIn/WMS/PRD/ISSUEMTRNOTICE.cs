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

namespace WSL.YY.K3.FIN.PlugIn.PlugIn.WMS.PRD
{
    [Description("生产发料通知单审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class ISSUEMTRNOTICE: AbstractOperationServicePlugIn
    {
        string url = "http://testxa.360scm.com/SCM.WMS7.WebApi/WMS/SaveShippingOrder";

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

                    WmsOutStock wmsObject = new WmsOutStock();
                    wmsObject.token = WMSAPI.GetToken("13416AAE3BCB4F9497FA05527B9C13ED");

                    shippingorderEditDTO wmsEntry = new shippingorderEditDTO();

                    DynamicObject billType = billObj["BillTypeID"] as DynamicObject;
                    wmsEntry.ORDER_TYPE_SC = billType["Name"].ToString();
                    wmsEntry.ORDER_TYPE_SC = "销售出库";

                    wmsEntry.SHIPPING_ORDER_SOURCE_SC = "ERP";
                    wmsEntry.EXTERNAL_ORDER_ID = billObj["BillNo"].ToString();
                    wmsEntry.EXTERNAL_ORDER_ID2 = billObj["Id"].ToString();

                    DynamicObject created = billObj["CreatorId"] as DynamicObject;
                    //wmsEntry.CREATED_BY = created["Name"].ToString();
                    wmsEntry.CREATED_DATE = Convert.ToDateTime(billObj["ApproveDate"]);

                    DynamicObject stockOrg = billObj["SaleOrgId"] as DynamicObject;
                    wmsEntry.SO_UDF1 = stockOrg["Number"].ToString();

                    DynamicObject DeliveryOrgID = billObj["DeliveryOrgID"] as DynamicObject;
                    wmsEntry.OWNER_ID = DeliveryOrgID["Number"].ToString();

                    wmsEntry.WAYBILL_NO = billObj["CarriageNO"].ToString();

                    DynamicObject CustomerID = billObj["CustomerID"] as DynamicObject;
                    wmsEntry.CUSTOMER_ID = CustomerID["Number"].ToString();
                    wmsEntry.CUSTOMER_NAME = CustomerID["Name"].ToString();

                    //病人ID需要加字段
                    wmsEntry.CUSTOMER_REF = CustomerID["Number"].ToString();

                    //实际收货人
                    DynamicObject FReceiverContactID = billObj["ReceiverContactID"] as DynamicObject;
                    wmsEntry.DEST_CONTACT_NAME = FReceiverContactID["Name"].ToString();

                    //销售部门
                    DynamicObject dept = billObj["SaleDeptID"] as DynamicObject;
                    if (dept != null)
                    {
                        wmsEntry.SO_UDF9 = dept["Number"].ToString();
                    }

                    //销售员
                    DynamicObject man = billObj["SalesManId"] as DynamicObject;
                    if (man != null)
                    {
                        wmsEntry.SO_UDF10 = man["Number"].ToString();
                    }


                    List<ShippingOrderDetailList> detailList = new List<ShippingOrderDetailList>();
                    foreach (var entry in entrys)
                    {
                        DynamicObject stock = entry["StockId"] as DynamicObject;
                        wmsEntry.WH_ID = stock["Number"].ToString();
                        wmsObject.whgid = stock["Number"].ToString();

                        DynamicObject owner = entry["OwnerId"] as DynamicObject;
                        wmsEntry.OWNER_ID = owner["Number"].ToString();

                        ShippingOrderDetailList detail = new ShippingOrderDetailList();

                        detail.EXTERNAL_LINE_ID = entry["Id"].ToString();

                        DynamicObject material = entry["MaterialId"] as DynamicObject;
                        detail.SKU_ID = material["Number"].ToString();

                        DynamicObject StockUnitID = entry["StockUnitID"] as DynamicObject;
                        detail.PACK_ID = StockUnitID["Number"].ToString();

                        DynamicObject ExtAuxUnitId = entry["UnitID"] as DynamicObject;
                        detail.UOM_ID = ExtAuxUnitId["Number"].ToString();

                        detail.ORDER_QTY = Convert.ToInt32(entry["Qty"]);

                        DynamicObject lot = entry["Lot"] as DynamicObject;
                        if (lot != null)
                        {
                            detail.EXTERNAL_LOT = lot["Number"].ToString();
                        }

                        //欧洲套装编号（BOM编号）
                        DynamicObject FBomID = entry["BomID"] as DynamicObject;
                        detail.OL_UDF1 = FBomID["Number"].ToString();

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

                        DynamicObject StockStatusId = entry["StockStatusId"] as DynamicObject;
                        detail.SKU_PROPERTY = StockStatusId["Number"].ToString();

                        if (entry["PRODUCEDATE"] != null)
                        {
                            detail.PRODUCE_DATE = entry["PRODUCEDATE"].ToString();
                        }

                        if (entry["FEXPIRYDATE"] != null)
                        {
                            detail.EXPIRE_DATE = entry["FEXPIRYDATE"].ToString();
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
