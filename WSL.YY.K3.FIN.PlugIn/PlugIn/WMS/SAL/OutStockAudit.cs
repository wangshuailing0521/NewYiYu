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

namespace WSL.YY.K3.FIN.PlugIn.PlugIn.WMS.SAL
{
    [Description("销售出库单审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class OutStockAudit: AbstractOperationServicePlugIn
    {
        string url = "https://wms-test.medtrum.com/SCM.WMS7.WebApi/WMS/SaveShippingOrder";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FCreatorId");
            e.FieldKeys.Add("FApproveDate");
            e.FieldKeys.Add("FStockOrgId");
            e.FieldKeys.Add("FCustomerID");
            e.FieldKeys.Add("F_aaaa_Assistant");
            e.FieldKeys.Add("FZohoContact");
            e.FieldKeys.Add("FSaleDeptID");
            e.FieldKeys.Add("FSalesManID");

            e.FieldKeys.Add("FMaterialID");
            e.FieldKeys.Add("FSalUnitID");
            e.FieldKeys.Add("FSALUNITQTY");
            e.FieldKeys.Add("FPrice");
            e.FieldKeys.Add("FLot");
            e.FieldKeys.Add("FExpiryDate");
            e.FieldKeys.Add("FProduceDate");
            e.FieldKeys.Add("FBomID");
            e.FieldKeys.Add("FAuxPropId");
            e.FieldKeys.Add("FStockStatusID");
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
                sb.AppendLine($@"接口名称：销售出库（寄售）API");
                try
                {
                    DynamicObject billType = billObj["BillTypeID"] as DynamicObject;
                    if (billType["Number"].ToString() != "XSCKD02_SYS")
                    {
                        continue;
                    }
                    DynamicObjectCollection entrys
                        = billObj["SAL_OUTSTOCKENTRY"] as DynamicObjectCollection;

                    WmsOutStock wmsObject = new WmsOutStock();
                    wmsObject.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");

                    shippingorderEditDTO wmsEntry = new shippingorderEditDTO();

                  
                    wmsEntry.ORDER_TYPE_SC = billType["Name"].ToString();

                    //wmsEntry.ORDER_TYPE_SC = "出库申请";

                    wmsEntry.SHIPPING_ORDER_SOURCE_SC = "ERP";
                    wmsEntry.EXTERNAL_ORDER_ID = billObj["BillNo"].ToString();
                    wmsEntry.EXTERNAL_ORDER_ID2 = billObj["Id"].ToString();

                    DynamicObject created = billObj["FCreatorId"] as DynamicObject;
                    //wmsEntry.CREATED_BY = created["Name"].ToString();
                    wmsEntry.CREATED_DATE = Convert.ToDateTime(billObj["ApproveDate"]);

                    DynamicObject stockOrg = billObj["StockOrgId"] as DynamicObject;
                    wmsEntry.SO_UDF1 = stockOrg["Number"].ToString();

                    DynamicObject DeliveryOrgID = billObj["StockOrgId"] as DynamicObject;
                    wmsEntry.OWNER_ID = DeliveryOrgID["Number"].ToString();

                    DynamicObject CustomerID = billObj["CustomerID"] as DynamicObject;
                    if (CustomerID != null)
                    {
                        wmsEntry.CUSTOMER_ID = CustomerID["Number"].ToString();
                        wmsEntry.CUSTOMER_NAME = CustomerID["Name"].ToString();
                    }


                    //病人ID需要加字段
                    DynamicObject FPaitner = billObj["F_aaaa_Assistant"] as DynamicObject;
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


                    //销售部门
                    DynamicObject dept = billObj["SaleDeptID"] as DynamicObject;
                    if (dept != null)
                    {
                        wmsEntry.SO_UDF9 = dept["Number"].ToString();
                    }

                    //销售员
                    DynamicObject man = billObj["SalesManID"] as DynamicObject;
                    if (man != null)
                    {
                        wmsEntry.SO_UDF10 = man["Name"].ToString();
                    }


                    List<ShippingOrderDetailList> detailList = new List<ShippingOrderDetailList>();
                    foreach (var entry in entrys)
                    {
                        string orgNumber = stockOrg["Number"].ToString();
                        WMSStock wmsStock = SqlHelper.GetWmsStockNumber(this.Context, orgNumber);

                        wmsEntry.WH_ID = wmsStock.Number;
                        wmsObject.whgid = wmsStock.Number;


                        ShippingOrderDetailList detail = new ShippingOrderDetailList();

                        detail.EXTERNAL_LINE_ID = entry["Id"].ToString();

                        DynamicObject material = entry["MaterialID"] as DynamicObject;
                        detail.SKU_ID = material["Number"].ToString();

                        DynamicObject StockUnitID = entry["SalUnitId"] as DynamicObject;
                        if (StockUnitID != null)
                        {
                            detail.PACK_ID = StockUnitID["Number"].ToString();
                        }

                        DynamicObject ExtAuxUnitId = entry["SalUnitId"] as DynamicObject;
                        detail.UOM_ID = ExtAuxUnitId["Number"].ToString();

                        detail.ORDER_QTY = Convert.ToInt32(entry["SALUNITQTY"]);

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

                        //detail.LOT_ATTR8 = entry["IsFree"].ToString();

                        DynamicObject StockStatusId = entry["StockStatusID"] as DynamicObject;
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
