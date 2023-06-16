using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WSL.YY.K3.FIN.PlugIn.Helper;
using WSL.YY.K3.FIN.PlugIn.Model;

namespace WSL.YY.K3.FIN.PlugIn.PlugIn.WMS.Base
{
    [Description("物料辅助属性传递插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class AssistantAudit : AbstractOperationServicePlugIn
    {
        string url = "http://8.209.75.207:20032/Inbound/Api/SaveLotTemplate";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FEntityAuxPty");
            e.FieldKeys.Add("FAuxPropertyId");
            e.FieldKeys.Add("FIsEnable1");
            e.FieldKeys.Add("FIsBatchManage");
            e.FieldKeys.Add("FIsKFPeriod");
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
                sb.AppendLine($@"接口名称：辅助资料API");

                try
                {
                    string apiKey = "16123E89FEE245D5A64609890809814A";
                    List<AssistantWMS> itemWmss = new List<AssistantWMS>();

                    AssistantWMS itemWms = new AssistantWMS();
                    LotTemplate lotTemplate = new LotTemplate();
                    List<Detail> details = new List<Detail>();

                    lotTemplate.LotTemplateID = billObj["Number"].ToString();
                    lotTemplate.Description = billObj["Name"].ToString();

                    bool lot = false;
                    bool period = false;

                    DynamicObjectCollection auxs 
                        = billObj["MaterialAuxPty"] as DynamicObjectCollection;
                   
                    foreach (var item in auxs)
                    {
                        bool isEnable = Convert.ToBoolean(item["IsEnable1"]);
                        if (isEnable)
                        {
                            Detail detail = new Detail();
                            DynamicObject FAuxPropertyId = item["AuxPropertyId"] as DynamicObject;
                            string id = FAuxPropertyId["Id"].ToString();
                            string no = FAuxPropertyId["Number"].ToString();
                            string name = FAuxPropertyId["Name"].ToString();
                            detail.LotLable = name;
                            detail.AttrValue = no;
                            if (no == "1001")
                            {
                                detail.LotField = "LOT_ATTR01";
                            }
                            if (no == "1002")
                            {
                                detail.LotField = "LOT_ATTR02";
                            }
                            if (no == "1003")
                            {
                                detail.LotField = "LOT_ATTR03";
                            }
                            if (no == "1004")
                            {
                                detail.LotField = "LOT_ATTR04";
                            }
                            if (no == "1005")
                            {
                                detail.LotField = "LOT_ATTR05";
                            }
                            if (no == "1006")
                            {
                                detail.LotField = "LOT_ATTR06";
                            }
                            if (no == "1007")
                            {
                                detail.LotField = "LOT_ATTR07";
                            }

                            details.Add(detail);
                        }
                    }

                    DynamicObjectCollection stocks
                       = billObj["MaterialStock"] as DynamicObjectCollection;
                    foreach (var item in stocks)
                    {
                        lot = Convert.ToBoolean(item["IsBatchManage"]);
                        period = Convert.ToBoolean(item["IsKFPeriod"]);
                    }
                    if (lot)
                    {
                        Detail detail = new Detail();
                        detail.LotField = "EXTERNAL_LOT";
                        detail.LotLable = "外部批号";
                        detail.AttrValue = "";
                        details.Add(detail);
                    }
                    if (period)
                    {
                        Detail detail = new Detail();
                        detail.LotField = "EXPIRE_DATE";
                        detail.LotLable = "失效日期";
                        detail.AttrValue = "";
                        details.Add(detail);

                        Detail detail1 = new Detail();
                        detail1.LotField = "PRODUCE_DATE";
                        detail1.LotLable = "生产日期";
                        detail1.AttrValue = "";
                        details.Add(detail1);
                    }

                    Detail detail2 = new Detail();
                    detail2.LotField = "LOT_ATTR08";
                    detail2.LotLable = "是否样品";
                    detail2.AttrValue = "IsSample";
                    details.Add(detail2);

                    lotTemplate.Details = details;
                    itemWms.LotTemplate = lotTemplate;
                    itemWmss.Add(itemWms);
                    string json = JsonHelper.ToJSON(itemWmss);
                    sb.AppendLine($@"请求信息：{json}");
                    string response = ApiHelper.HttpPostAuth(url, apiKey, json);
                    sb.AppendLine($@"返回信息：{response}");

                    #region 解析返回信息
                    WMSResponse result = JsonHelper.FromJSON<WMSResponse>(response);
                    if (!result.success)
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
