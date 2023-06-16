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
using Kingdee.BOS;

namespace WSL.YY.K3.FIN.PlugIn.PlugIn.WMS
{
    [Description("物料审核插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class MaterialAudit : AbstractOperationServicePlugIn
    {
        string url = "https://wms-test.medtrum.com/SCM.TMS7.WebApi/BaseInfo/SaveItem";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FUseOrgId");
            e.FieldKeys.Add("FCategoryID");
            e.FieldKeys.Add("FMaterialGroup");
            e.FieldKeys.Add("FSpecification");
            e.FieldKeys.Add("FDescription");
            e.FieldKeys.Add("FSafeStock");
            e.FieldKeys.Add("FBaseUnitId");
            e.FieldKeys.Add("FCategoryID");
            e.FieldKeys.Add("FISKFPERIOD");
            e.FieldKeys.Add("FExpUnit");
            e.FieldKeys.Add("FExpPeriod");
            e.FieldKeys.Add("F_aaaa_Text");
            e.FieldKeys.Add("FNETWEIGHT");
            e.FieldKeys.Add("FGROSSWEIGHT");
            e.FieldKeys.Add("FWEIGHTUNITID");
            e.FieldKeys.Add("FIsSNManage");
            e.FieldKeys.Add("F_aaaa_Text3");
            e.FieldKeys.Add("F_aaaa_Combo");
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
                sb.AppendLine($@"接口名称：物料API");

                try
                {
                    DynamicObjectCollection MaterialBase = billObj["MaterialBase"] as DynamicObjectCollection;
                    DynamicObjectCollection MaterialStock = billObj["MaterialStock"] as DynamicObjectCollection;


                    ItemWMS itemWms = new ItemWMS();

                    itemWms.token = WMSAPI.GetToken("C4B60A0726CA408FAEEE55817A4B0991");
                    itemWms.apiKey = "C4B60A0726CA408FAEEE55817A4B0991";

                    Item item = new Item();

                    item.ItemID = billObj["Number"].ToString();
                    item.LotTemplateID = billObj["Number"].ToString();

                    DynamicObject org = billObj["UseOrgId"] as DynamicObject;
                    item.OwnerID = org["Number"].ToString();

                    DynamicObject CategoryID = MaterialBase[0]["CategoryID"] as DynamicObject;
                    item.ProductTypeID = CategoryID["Number"].ToString();

                    DynamicObject MaterialGroup = billObj["MaterialGroup"] as DynamicObject;
                    item.PRODUCT_TYPE_ID2 = MaterialGroup["Number"].ToString();

                    item.ITEM_SPEC = billObj["Specification"].ToString();

                    LocaleValue localeNames = billObj["Name"] as LocaleValue;
                    foreach (var locale in localeNames)
                    {
                        if (locale.Key == 2052)
                        {
                            item.ItemName = locale.Value;
                        }

                        if (locale.Key == 1033)
                        {
                            item.ItemDesc = locale.Value;
                        }
                    }

                    LocaleValue SpecificationNames = billObj["Specification"] as LocaleValue;
                    if (SpecificationNames != null)
                    {
                        foreach (var locale in SpecificationNames)
                        {
                            if (locale.Key == 2052)
                            {
                                item.ITEM_SPEC = locale.Value;
                            }

                            if (locale.Key == 1033)
                            {
                                item.FactoryId = locale.Value;
                            }
                        }
                    }
                   

                    LocaleValue DescriptionNames = billObj["Description"] as LocaleValue;
                    if (DescriptionNames != null)
                    {
                        foreach (var locale in DescriptionNames)
                        {
                            if (locale.Key == 2052)
                            {
                                item.ItemShortName = locale.Value;
                            }

                            if (locale.Key == 1033)
                            {
                                item.FactoryName = locale.Value;
                            }
                        }
                    }

                    item.ChineseMedicineApprovalNo = billObj["F_aaaa_Text3"].ToString();

                    string safeStock = MaterialStock[0]["SafeStock"].ToString();
                    item.MinOrderQty = Convert.ToInt32(Convert.ToDecimal(safeStock));

                    DynamicObject BaseUnitId = MaterialBase[0]["BaseUnitId"] as DynamicObject;
                    item.PackID = BaseUnitId["Number"].ToString();

                    if (billObj["F_aaaa_Combo"] != null)
                    {
                        item.is_qc = billObj["F_aaaa_Combo"].ToString() == "免检"? "否":"是";

                        if (string.IsNullOrWhiteSpace(billObj["F_aaaa_Combo"].ToString()))
                        {
                            item.is_qc = "否";
                        }
                    }

                    item.IsShelfLife = Convert.ToBoolean(MaterialStock[0]["IsKFPeriod"]) ? "是" : "否";
                    if (Convert.ToBoolean(MaterialStock[0]["IsKFPeriod"]))
                    {
                        string ExpUnit = MaterialStock[0]["ExpUnit"].ToString();
                        int day = Convert.ToInt32(MaterialStock[0]["ExpPeriod"]);
                        if (ExpUnit == "M")
                        {
                            day = day * 30;
                        }
                        if (ExpUnit == "Y")
                        {
                            day = day * 360;
                        }
                        item.ShelfLifeDays = day;
                        item.ShelfLifeType = "生产日期";
                    }

                    item.ItemUdf1 = billObj["F_aaaa_Text"].ToString();
                    item.NetWGT = Convert.ToDecimal(MaterialBase[0]["NETWEIGHT"].ToString());
                    item.GrossWGT = Convert.ToDecimal(MaterialBase[0]["GROSSWEIGHT"].ToString());
                    DynamicObject WEIGHTUNITID = MaterialBase[0]["WEIGHTUNITID"] as DynamicObject;
                    if (WEIGHTUNITID !=null)
                    {
                        item.NetWGTUom = WEIGHTUNITID["Name"].ToString();
                        item.GrossWGTUom = WEIGHTUNITID["Name"].ToString();
                    }

                    item.IsSerial = Convert.ToBoolean(MaterialStock[0]["IsSNManage"]) ? "是" : "否";
                    //item.ShelfLifeType = 

                    itemWms.item = item;

                    string json = JsonHelper.ToJSON(itemWms);
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
