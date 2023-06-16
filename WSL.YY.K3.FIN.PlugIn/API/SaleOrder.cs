using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.WebApi.ServicesStub;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using WSL.YY.K3.FIN.PlugIn.Helper;

namespace WSL.YY.K3.FIN.PlugIn.API
{
    [Description("销售订单API")]
    public class SaleOrder : AbstractWebApiBusinessService
    {
        private Context Context = null;
        private string formId = "";
        DBLog dBLog = new DBLog();
        StringBuilder sb = new StringBuilder();
        private string billId = "";

        public SaleOrder(KDServiceContext context)
           : base(context)
        {

            dBLog.FInvocation = "zoho";
            dBLog.FInterfaceType = "Order";
            dBLog.FBeginTime = DateTime.Now.ToString();
            dBLog.Context = KDContext.Session.AppContext;
        }

        public JObject ExecuteService(string DataJson)
        {
            sb.AppendLine($@"接口方向：CRM --> Kingdee");
            sb.AppendLine($@"接口名称：订单API");
            sb.AppendLine($@"请求信息：{DataJson}");
            JObject objRetutrn = new JObject();
            try
            {
                dBLog.FRequestMessage = DataJson;

                Context = KDContext.Session.AppContext;
                objRetutrn = Create(DataJson);
                sb.AppendLine($@"返回信息：{objRetutrn.ToString()}");
                Logger.Info("", sb.ToString());
                return objRetutrn;
            }
            catch (Exception ex)
            {
                objRetutrn.Add("IsSuccess", "false");
                objRetutrn.Add("Number", "");
                objRetutrn.Add("Message", ex.Message);
                sb.AppendLine($@"返回信息：{objRetutrn.ToString()}");
                Logger.Error("", sb.ToString(), ex);
                return objRetutrn;
            }
            finally
            {
                dBLog.FEndTime = DateTime.Now.ToString();
                dBLog.FResponseMessage = objRetutrn.ToString();
                dBLog.Insert();
            }
        }

        private JObject Create(string json)
        {
            JObject model = JObject.Parse(json);
            IOperationResult result = null;

            string zohoBillNo = model["SO Number"].ToString();
            dBLog.FBillNo = zohoBillNo;
            billId = SqlHelper.GetOrderIdByBillNo(this.Context, zohoBillNo);
            if (string.IsNullOrWhiteSpace(billId))
            {
                result = CreateBill(model);
            }
            else
            {
                if (HaveSalOutStock(billId) || HaveQTOutStock(billId))
                {
                    throw new Exception($@"{zohoBillNo}已生成下游单据，不允许修改！");
                }

                if (HaveNetWorkCtrlreCords(billId))
                {
                    throw new Exception($@"{zohoBillNo}正在金蝶中进行操作，不允许修改！");
                }

                result = EditBill(model);
            }

            JObject objRetutrn = new JObject();
            objRetutrn.Add("IsSuccess", result.IsSuccess);
            string message = "";
            string number = "";
            if (result.IsSuccess)
            {
                if (result.SuccessDataEnity != null)
                {
                    foreach (DynamicObject billObj in result.SuccessDataEnity)
                    {
                        number = billObj["BillNo"].ToString();
                    }
                }
                if (result.OperateResult != null)
                {
                    foreach (var item in result.OperateResult)
                    {

                        message = message + item.Message.ToString() + "\r\n";
                    }
                }

                dBLog.FStatus = "S";
                dBLog.FMessage = message;
            }
            else
            {
                foreach (var item in result.ValidationErrors)
                {
                    message = message + item.Message.ToString() + "\r\n";
                }

                dBLog.FStatus = "E";
                dBLog.FMessage = message;
            }
            objRetutrn.Add("Number", number);
            objRetutrn.Add("Message", message);
            return objRetutrn;
        }

        /// <summary>
        /// 创建单据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IOperationResult CreateBill(JObject model)
        {
            if (model["Type"].ToString() == "Order")
            {
                formId = "SAL_SaleOrder";
            }

            if (model["Type"].ToString() == "Sample"
                || model["Type"].ToString() == "Replacement"
                || model["Type"].ToString() == "Sample from SH")
            {
                formId = "STK_OutStockApply";
            }

            if (model["Type"].ToString() == "Internal Order")
            {
                formId = "STK_TRANSFERAPPLY";
            }

            IOperationResult result = new OperationResult();

            /*构建一个IBillView实例，通过此实例，可以方便的填写单据各属性*/
            IBillView billView = this.CreateBankBillView(formId);

            /*新建一个空白单据*/
            ((IBillViewService)billView).LoadData();

            /* 触发插件的OnLoad事件：
            /* 组织控制基类插件，在OnLoad事件中，对主业务组织改变是否提示选项进行初始化。
            /* 如果不触发OnLoad事件，会导致主业务组织赋值不成功*/
            DynamicFormViewPlugInProxy eventProxy = billView.GetService<DynamicFormViewPlugInProxy>();
            eventProxy.FireOnLoad();

            /*加载数据*/
            if (model["Type"].ToString() == "Order")
            {
                FillBankBillPropertysBySaleOrder(billView, model);
            }
            if (model["Type"].ToString() == "Sample"
                || model["Type"].ToString() == "Replacement"
                || model["Type"].ToString() == "Sample from SH")
            {
                FillBankBillPropertysByOtherOutStock(billView, model);
            }

            if (model["Type"].ToString() == "Internal Order")
            {
                FillTransferApplyBill(billView, model);
            }


            /*保存单据*/
            OperateOption saveOption = OperateOption.Create();
            result = this.SaveBill(billView, saveOption);

            return result;
        }

        /// <summary>
        /// 修改单据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IOperationResult EditBill(JObject model)
        {
            if (model["Type"].ToString() == "Order")
            {
                formId = "SAL_SaleOrder";
            }
            if (model["Type"].ToString() == "Sample"
                || model["Type"].ToString() == "Replacement"
                || model["Type"].ToString() == "Sample from SH")
            {
                formId = "STK_OutStockApply";
            }
            if (model["Type"].ToString() == "Internal Order")
            {
                formId = "STK_TRANSFERAPPLY";
            }

            IOperationResult result = new OperationResult();

            /*构建一个IBillView实例，通过此实例，可以方便的填写单据各属性*/
            IBillView billView = this.CreateBankBillView(formId);

            //反审核
            IOperationResult unAuditResult
                 = UnAudit(billView, billId);

            //加载
            ModifyBill(billView, billId);

            /*加载数据*/
            if (model["Type"].ToString() == "Order")
            {
                FillBankBillPropertysBySaleOrder(billView, model, "1");
            }
            if (model["Type"].ToString() == "Sample"
                || model["Type"].ToString() == "Replacement"
                || model["Type"].ToString() == "Sample from SH")
            {
                FillBankBillPropertysByOtherOutStock(billView, model, "1");
            }

            if (model["Type"].ToString() == "Internal Order")
            {
                FillTransferApplyBill(billView, model, "1");
            }

            /*保存单据*/
            OperateOption saveOption = OperateOption.Create();
            result = this.SaveBill(billView, saveOption);

            foreach (var item in unAuditResult.OperateResult)
            {
                result.OperateResult.Add(item);
            }

            return result;
        }

        /// <summary>
        /// 把单据的各属性，填写到IBillView当前所管理的单据中
        /// </summary>
        /// <param name="billView"></param>
        private void FillBankBillPropertysBySaleOrder(IBillView billView, JObject model, string type = "0")
        {
            // 把billView转换为IDynamicFormViewService接口：
            // 调用IDynamicFormViewService.UpdateValue: 会执行字段的值更新事件
            // 调用 dynamicFormView.SetItemValueByNumber ：不会执行值更新事件，需要继续调用：
            // ((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService(key, rowIndex);
            IDynamicFormViewService dynamicFormView = billView as IDynamicFormViewService;

            #region 销售组织
            if (type == "0")
            {
                if (model["Medtrum Organization"] != null)
                {
                    string orgNumber
                        = SqlHelper.GetOrgNumber(Context, model["Medtrum Organization"].ToString());
                    dynamicFormView.SetItemValueByNumber("FSaleOrgId", orgNumber, 0);
                }
            }
            #endregion

            #region 客户
            if (model["Account Id"] != null)
            {
                string custNumber
                    = SqlHelper.GetCustNumber(Context, model["Account Id"].ToString());
                dynamicFormView.SetItemValueByNumber("FCustId", custNumber, 0);
            }
            #endregion

            #region ZOHO销售订单编号
            if (model["SO Number"] != null)
            {
                dynamicFormView.UpdateValue("FZohoBillNo", 0, model["SO Number"].ToString());
                dynamicFormView.UpdateValue("FBillNo", 0, model["SO Number"].ToString());
            }
            #endregion

            #region Shipping Title
            if (model["Shipping Title"] != null)
            {
                dynamicFormView.UpdateValue("FShippingTitle", 0, model["Shipping Title"].ToString());
            }
            #endregion

            #region Shipping Address
            if (model["Shipping Address"] != null)
            {
                dynamicFormView.UpdateValue("FShippingAddress", 0, model["Shipping Address"].ToString());
            }
            #endregion

            #region Shipping Phone
            if (model["Shipping Phone"] != null)
            {
                dynamicFormView.UpdateValue("FShippingPhone", 0, model["Shipping Phone"].ToString());
            }
            #endregion

            #region Shipping Email
            if (model["Shipping Email"] != null)
            {
                dynamicFormView.UpdateValue("FShippingEmail", 0, model["Shipping Email"].ToString());
            }
            #endregion

            #region Deal Id
            if (model["Deal Id"] != null)
            {
                dynamicFormView.UpdateValue("FDealId", 0, model["Deal Id"].ToString());
            }
            #endregion

            #region 收货方联系人
            if (model["Contact Id"] != null)
            {
                string contactNumber
                    = SqlHelper.GetContactNumber(Context, model["Contact Id"].ToString());
                dynamicFormView.SetItemValueByNumber("FZohoContact", contactNumber, 0);

                dynamicFormView.SetItemValueByNumber("FPaitner", contactNumber, 0);
            }
            #endregion

            #region Sales Order Owner Id
            if (model["Sales Order Owner Id"] != null)
            {
                dynamicFormView.UpdateValue("FSaleOwnerId", 0, model["Sales Order Owner Id"].ToString());
            }
            #endregion

            #region 销售员
            if (model["Sales Order Owner"] != null)
            {
                string salerNumber
                    = SqlHelper.GetSalerNumber(Context, model["Sales Order Owner"].ToString(), model["Medtrum Organization"].ToString());
                dynamicFormView.SetItemValueByNumber("FSalerId", salerNumber, 0);
            }
            #endregion

            #region 结算币别
            if (model["Currency"] != null)
            {
                string currNumber
                    = SqlHelper.GetCurrencyNumber(Context, model["Currency"].ToString());
                dynamicFormView.SetItemValueByNumber("FSettleCurrId", currNumber, 0);
            }
            #endregion

            #region 汇率
            if (model["Exchange Rate"] != null)
            {
                dynamicFormView.UpdateValue(
                    "FExchangeRate",
                    0,
                    Convert.ToDecimal(model["Exchange Rate"].ToString()));
            }
            #endregion

            #region 备注
            string subject = "";
            string description = "";
            string remark = "";
            if (model["Subject"] != null)
            {
                subject = model["Subject"].ToString();
            }
            if (model["Description"] != null)
            {
                description = model["Description"].ToString();
            }
            remark = subject;
            if (!string.IsNullOrWhiteSpace(description))
            {
                remark = subject + "," + description;
            }
            dynamicFormView.UpdateValue("FNote", 0, remark);
            #endregion

            #region Vendor
            if (model["Vendor"] != null)
            {
                dynamicFormView.UpdateValue("FVendor", 0, model["Vendor"].ToString());
            }
            #endregion

            #region 是否含税
            dynamicFormView.UpdateValue("FIsIncludedTax", 0, "0");
            #endregion

            #region 清空明细
            if (type == "1")
            {
                DynamicObject billObj = billView.Model.DataObject;
                DynamicObjectCollection entrys = billObj["SaleOrderEntry"] as DynamicObjectCollection;
                entrys.Clear();
                billView.Model.CreateNewEntryRow("FSaleOrderEntry");
            }
            #endregion

            bool isTao = false;

            #region 明细信息
            if (model["Details"] != null)
            {
                JArray details = JArray.Parse(model["Details"].ToString());
                int seq = 0;
                foreach (var detail in details)
                {

                    //新增行
                    if (seq > 0)
                    {
                        billView.Model.CreateNewEntryRow("FSaleOrderEntry");
                    }

                    if (SqlHelper.IsHaveBom(this.Context, detail["Product Details Name"].ToString()))
                    {
                        dynamicFormView.UpdateValue("FRowType", seq, "Parent");
                        isTao = true;
                    }

                    #region Vendor
                    if (model["Vendor"] != null)
                    {
                        string orgNumber
                           = SqlHelper.GetOrgNumber(Context, model["Vendor"].ToString());
                        dynamicFormView.SetItemValueByNumber("FStockOrgId", orgNumber, seq);
                    }
                    #endregion

                    //物料Id
                    if (detail["Product Id"] != null)
                    {
                        dynamicFormView.UpdateValue("FProductId", seq, detail["Product Id"].ToString());
                    }

                    if (detail["Product Details Name"] != null)
                    {
                        //物料
                        string materialNumber = detail["Product Details Name"].ToString();
                        dynamicFormView.SetItemValueByNumber("FMaterialId", materialNumber, seq);
                    }

                    string region = "";
                    string color = "";
                    if (detail["Region"] != null)
                    {
                        region = detail["Region"].ToString();
                    }
                    if (detail["Color"] != null)
                    {
                        color = detail["Color"].ToString();
                    }

                    long auxpropId = Convert.ToInt64(SqlHelper.GetAuxPropId(this.Context, region, color));
                    dynamicFormView.SetItemValueByID("FAuxPropId", auxpropId, seq);

                    if (detail["Unit Code"] != null)
                    {
                        dynamicFormView.SetItemValueByNumber("FUnitID", detail["Unit Code"].ToString(), seq);
                    }



                    if (detail["Quantity"] != null)
                    {
                        //销售数量
                        dynamicFormView.UpdateValue(
                            "FQty",
                            seq,
                            Convert.ToDecimal(detail["Quantity"].ToString()));
                    }


                    if (detail["List Price"] != null)
                    {
                        //单价
                        dynamicFormView.UpdateValue("FPrice", seq, Convert.ToDecimal(detail["List Price"].ToString()));
                    }

                    if (detail["Tax"] != null)
                    {
                        //税率
                        dynamicFormView.UpdateValue("FEntryTaxRate", seq, Convert.ToDecimal(detail["Tax"].ToString()));
                    }


                    if (model["CRD"] != null)
                    {
                        //期望交货日期
                        dynamicFormView.UpdateValue("FMinPlanDeliveryDate", seq, model["CRD"].ToString());
                    }



                    seq++;
                }
            }
            #endregion

            #region 是否预收
            if (model["Payment Terms"] != null)
            {
                string PaymentTerms
                    = SqlHelper.GetRecConditionNumber(this.Context, model["Payment Terms"].ToString());
                dynamicFormView.SetItemValueByNumber("FRecConditionId", PaymentTerms, 0);
            }
            #endregion

            if (isTao)
            {
                billView.InvokeFormOperation("BOMExpand");

                Logger.Info("", "调用展开套装操作：BOMExpand");
            }


        }

        /// <summary>
        /// 把单据的各属性，填写到IBillView当前所管理的单据中
        /// </summary>
        /// <param name="billView"></param>
        private void FillBankBillPropertysByOtherOutStock(IBillView billView, JObject model, string type = "0")
        {
            // 把billView转换为IDynamicFormViewService接口：
            // 调用IDynamicFormViewService.UpdateValue: 会执行字段的值更新事件
            // 调用 dynamicFormView.SetItemValueByNumber ：不会执行值更新事件，需要继续调用：
            // ((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService(key, rowIndex);
            IDynamicFormViewService dynamicFormView = billView as IDynamicFormViewService;

            if (type == "0")
            {
                //组织
                if (model["Medtrum Organization"] != null)
                {
                    string orgNumber
                        = SqlHelper.GetOrgNumber(Context, model["Medtrum Organization"].ToString());
                    dynamicFormView.SetItemValueByNumber("FStockOrgId", orgNumber, 0);
                }
            }

            //单据类型
            if (model["Type"] != null)
            {
                if (model["Type"].ToString() == "Sample")
                {
                    if (model["Medtrum Organization"].ToString().Contains("上海"))
                    {
                        dynamicFormView.SetItemValueByNumber("FBillTypeID", "CKSQLX09_SYS", 0);
                    }
                    else
                    {
                        dynamicFormView.SetItemValueByNumber("FBillTypeID", "CKSQLX08_SYS", 0);
                    }

                }
                if (model["Type"].ToString() == "Replacement")
                {
                    dynamicFormView.SetItemValueByNumber("FBillTypeID", "CKSQLX07_SYS", 0);
                }

            }

            //申请类型
            if (model["Type"] != null)
            {
                string applyType
                        = SqlHelper.GetSQType(Context, model["Type"].ToString());
                dynamicFormView.SetItemValueByNumber("FApplyType", applyType, 0);
            }

            //Type
            if (model["Type"] != null)
            {
                dynamicFormView.UpdateValue("FZohoType", 0, model["Type"].ToString());
            }


            #region Shipping Title
            if (model["Shipping Title"] != null)
            {
                dynamicFormView.UpdateValue("FShippingTitle", 0, model["Shipping Title"].ToString());
            }
            #endregion

            #region Shipping Address
            if (model["Shipping Address"] != null)
            {
                dynamicFormView.UpdateValue("FShippingAddress", 0, model["Shipping Address"].ToString());
            }
            #endregion

            #region Shipping Phone
            if (model["Shipping Phone"] != null)
            {
                dynamicFormView.UpdateValue("FShippingPhone", 0, model["Shipping Phone"].ToString());
            }
            #endregion

            #region Shipping Email
            if (model["Shipping Email"] != null)
            {
                dynamicFormView.UpdateValue("FShippingEmail", 0, model["Shipping Email"].ToString());
            }
            #endregion

            if (model["Account Id"] != null)
            {
                //客户
                string custNumber
                    = SqlHelper.GetCustNumber(Context, model["Account Id"].ToString());
                dynamicFormView.SetItemValueByNumber("FCustId", custNumber, 0);
            }

            //ZOHO订单编号
            if (model["SO Number"] != null)
            {
                dynamicFormView.UpdateValue("FZohoBillNo", 0, model["SO Number"].ToString());
                dynamicFormView.UpdateValue("FBillNo", 0, model["SO Number"].ToString());
            }

            //Deal Id
            if (model["Deal Id"] != null)
            {
                dynamicFormView.UpdateValue("FDealId", 0, model["Deal Id"].ToString());
            }

            //销售员Id
            if (model["Sales Order Owner Id"] != null)
            {
                dynamicFormView.UpdateValue("FSaleOwnerId", 0, model["Sales Order Owner Id"].ToString());
            }

            #region 领料部门
            if (model["Picking Dept"] != null)
            {
                string deptNumber
                    = SqlHelper.GetDeptNumber(Context, model["Picking Dept"].ToString());
                dynamicFormView.SetItemValueByNumber("FDeptId", deptNumber, 0);
            }
            #endregion

            #region 领料人
            if (model["Sales Order Owner"] != null)
            {
                string salerNumber
                    = SqlHelper.GetSalerRGNumber(
                        Context,
                        model["Sales Order Owner"].ToString(),
                        model["Picking Dept"].ToString(),
                        model["Medtrum Organization"].ToString()
                        );
                dynamicFormView.SetItemValueByNumber("FPickerId", salerNumber, 0);
            }
            #endregion

            //联系人
            if (model["Contact Id"] != null)
            {
                string contactNumber
                    = SqlHelper.GetContactNumber(Context, model["Contact Id"].ToString());
                dynamicFormView.SetItemValueByNumber("FZohoContact", contactNumber, 0);

                dynamicFormView.SetItemValueByNumber("FPatiner", contactNumber, 0);
            }

            string subject = "";
            string description = "";
            string remark = "";
            if (model["Subject"] != null)
            {
                subject = model["Subject"].ToString();
            }
            if (model["Description"] != null)
            {
                description = model["Description"].ToString();
            }
            remark = subject;
            if (!string.IsNullOrWhiteSpace(description))
            {
                remark = subject + "," + description;
            }
            dynamicFormView.UpdateValue("FNote", 0, remark);



            #region Vendor
            if (model["Vendor"] != null)
            {
                dynamicFormView.UpdateValue("FVendor", 0, model["Vendor"].ToString());
            }
            #endregion


            if (type == "1")
            {
                DynamicObject billObj = billView.Model.DataObject;
                DynamicObjectCollection entrys = billObj["BillEntry"] as DynamicObjectCollection;
                entrys.Clear();
                billView.Model.CreateNewEntryRow("FEntity");
            }

            if (model["Details"] != null)
            {
                //明细信息
                JArray details = JArray.Parse(model["Details"].ToString());
                int seq = 0;
                foreach (var detail in details)
                {
                    //新增行
                    if (seq > 0)
                    {
                        billView.Model.CreateNewEntryRow("FEntity");
                    }

                    if (model["Vendor"] != null)
                    {
                        string orgNumber
                            = SqlHelper.GetOrgNumber(Context, model["Vendor"].ToString());
                        dynamicFormView.SetItemValueByNumber("FStockOrgIdEntry", orgNumber, seq);
                    }

                    //物料Id
                    if (detail["Product Id"] != null)
                    {
                        dynamicFormView.UpdateValue("FProductId", seq, detail["Product Id"].ToString());
                    }

                    //货主类型
                    dynamicFormView.UpdateValue("FMinPlanDeliveryDate", seq, "BD_OwnerOrg");

                    if (model["Vendor"] != null)
                    {
                        //货主
                        string orgNumber
                            = SqlHelper.GetOrgNumber(Context, model["Vendor"].ToString());
                        dynamicFormView.SetItemValueByNumber("FOwnerId", orgNumber, seq);
                    }

                    if (detail["Product Details Name"] != null)
                    {
                        //物料
                        string materialNumber = detail["Product Details Name"].ToString();
                        //= SqlHelper.GetMaterialNumber(Context, detail["Product Details Name"].ToString());
                        dynamicFormView.SetItemValueByNumber("FMaterialId", materialNumber, seq);
                    }

                    if (detail["Unit Code"] != null)
                    {
                        dynamicFormView.SetItemValueByNumber("FUnitID", detail["Unit Code"].ToString(), seq);
                    }

                    string region = "";
                    string color = "";
                    if (detail["Region"] != null)
                    {
                        region = detail["Region"].ToString();
                    }
                    if (detail["Color"] != null)
                    {
                        color = detail["Color"].ToString();
                    }

                    long auxpropId = Convert.ToInt64(SqlHelper.GetAuxPropId(this.Context, region, color));
                    dynamicFormView.SetItemValueByID("FAuxPropId", auxpropId, seq);


                    if (detail["Quantity"] != null)
                    {
                        //数量
                        dynamicFormView.UpdateValue("FQty", seq, Convert.ToDecimal(detail["Quantity"].ToString()));
                    }

                    seq++;
                }
            }

        }

        private void FillTransferApplyBill(IBillView billView, JObject model, string type = "0")
        {
            IDynamicFormViewService dynamicFormView = billView as IDynamicFormViewService;

            #region 申请组织
            if (type == "0")
            {
                if (model["Medtrum Organization"] != null)
                {
                    string orgNumber
                        = SqlHelper.GetOrgNumber(Context, model["Medtrum Organization"].ToString());
                    dynamicFormView.SetItemValueByNumber("FAPPORGID", orgNumber, 0);
                }
            }
            #endregion

            #region 调拨类型
            dynamicFormView.UpdateValue("FTRANSTYPE", 0, "OverOrgTransfer");
            #endregion

            #region 调出货主类型
            dynamicFormView.UpdateValue("FOwnerTypeIdHead", 0, "BD_OwnerOrg");
            #endregion

            #region 调入货主类型
            dynamicFormView.UpdateValue("FOwnerTypeInIdHead", 0, "BD_OwnerOrg");
            #endregion

            #region ZOHO销售订单编号
            if (model["SO Number"] != null)
            {
                dynamicFormView.UpdateValue("FZohoBillNo", 0, model["SO Number"].ToString());
                dynamicFormView.UpdateValue("FBillNo", 0, model["SO Number"].ToString());
            }
            #endregion

            #region Shipping Title
            if (model["Shipping Title"] != null)
            {
                dynamicFormView.UpdateValue("FShippingTitle", 0, model["Shipping Title"].ToString());
            }
            #endregion

            #region Shipping Address
            if (model["Shipping Address"] != null)
            {
                dynamicFormView.UpdateValue("FShippingAddress", 0, model["Shipping Address"].ToString());
            }
            #endregion

            #region Shipping Phone
            if (model["Shipping Phone"] != null)
            {
                dynamicFormView.UpdateValue("FShippingPhone", 0, model["Shipping Phone"].ToString());
            }
            #endregion

            #region Shipping Email
            if (model["Shipping Email"] != null)
            {
                dynamicFormView.UpdateValue("FShippingEmail", 0, model["Shipping Email"].ToString());
            }
            #endregion

            #region Deal Id
            if (model["Deal Id"] != null)
            {
                dynamicFormView.UpdateValue("FDealId", 0, model["Deal Id"].ToString());
            }
            #endregion

            #region Sales Order Owner Id
            if (model["Sales Order Owner Id"] != null)
            {
                dynamicFormView.UpdateValue("FSaleOwnerId", 0, model["Sales Order Owner Id"].ToString());
            }
            #endregion

            #region 备注
            string subject = "";
            string description = "";
            string remark = "";
            if (model["Subject"] != null)
            {
                subject = model["Subject"].ToString();
            }
            if (model["Description"] != null)
            {
                description = model["Description"].ToString();
            }
            remark = subject;
            if (!string.IsNullOrWhiteSpace(description))
            {
                remark = subject + "," + description;
            }
            dynamicFormView.UpdateValue("FRemarks", 0, remark);
            #endregion

            #region Vendor
            if (model["Vendor"] != null)
            {
                dynamicFormView.UpdateValue("FVendor", 0, model["Vendor"].ToString());
            }
            #endregion


            #region 清空明细
            if (type == "1")
            {
                DynamicObject billObj = billView.Model.DataObject;
                DynamicObjectCollection entrys = billObj["STK_STKTRANSFERAPPENTRY"] as DynamicObjectCollection;
                entrys.Clear();
                billView.Model.CreateNewEntryRow("FEntity");
            }
            #endregion

            #region 明细信息
            if (model["Details"] != null)
            {
                JArray details = JArray.Parse(model["Details"].ToString());
                int seq = 0;
                foreach (var detail in details)
                {
                    //新增行
                    if (seq > 0)
                    {
                        billView.Model.CreateNewEntryRow("FEntity");
                    }

                    #region 调出组织
                    if (model["Vendor"] != null)
                    {
                        string orgNumber
                            = SqlHelper.GetOrgNumber(Context, model["Vendor"].ToString());
                        dynamicFormView.SetItemValueByNumber("FStockOrgId", orgNumber, seq);
                    }
                    #endregion

                    #region 调入组织
                    if (model["Account Name"] != null)
                    {
                        string custNumber
                            = SqlHelper.GetOrgNumber(Context, model["Account Name"].ToString());
                        dynamicFormView.SetItemValueByNumber("FStockOrgInId", custNumber, seq);
                    }

                    #endregion

                    #region 调出货主类型
                    dynamicFormView.UpdateValue("FOwnerTypeId", seq, "BD_OwnerOrg");
                    #endregion

                    #region 调入货主类型
                    dynamicFormView.UpdateValue("FOwnerTypeInId", seq, "BD_OwnerOrg");
                    #endregion

                    #region 调出货主
                    if (model["Vendor"] != null)
                    {
                        string orgNumber
                            = SqlHelper.GetOrgNumber(Context, model["Vendor"].ToString());
                        dynamicFormView.SetItemValueByNumber("FOwnerId", orgNumber, seq);
                    }
                    #endregion

                    #region 调入货主
                    if (model["Account Name"] != null)
                    {
                        string custNumber
                            = SqlHelper.GetOrgNumber(Context, model["Account Name"].ToString());
                        dynamicFormView.SetItemValueByNumber("FOwnerInId", custNumber, seq);
                    }
                    #endregion

                    //物料Id
                    if (detail["Product Id"] != null)
                    {
                        dynamicFormView.UpdateValue("FProductId", seq, detail["Product Id"].ToString());
                    }

                    if (detail["Product Details Name"] != null)
                    {
                        //物料
                        string materialNumber = detail["Product Details Name"].ToString();
                        dynamicFormView.SetItemValueByNumber("FMATERIALID", materialNumber, seq);
                    }

                    string region = "";
                    string color = "";
                    if (detail["Region"] != null)
                    {
                        region = detail["Region"].ToString();
                    }
                    if (detail["Color"] != null)
                    {
                        color = detail["Color"].ToString();
                    }

                    long auxpropId = Convert.ToInt64(SqlHelper.GetAuxPropId(this.Context, region, color));
                    dynamicFormView.SetItemValueByID("FAuxPropId", auxpropId, seq);

                    if (detail["Unit Code"] != null)
                    {
                        dynamicFormView.SetItemValueByNumber("FUNITID", detail["Unit Code"].ToString(), seq);
                    }


                    if (detail["Quantity"] != null)
                    {
                        //申请数量
                        dynamicFormView.UpdateValue(
                            "FQty",
                            seq,
                            Convert.ToDecimal(detail["Quantity"].ToString()));
                    }

                    seq++;
                }
            }
            #endregion

        }

        /// <summary>
        /// 保存单据，并显示保存结果
        /// </summary>
        /// <param name="billView"></param>
        /// <returns></returns>
        private IOperationResult SaveBill(IBillView billView, OperateOption saveOption)
        {
            IOperationResult result = new OperationResult();

            #region 组合保存信息
            // 设置FormId
            Form form = billView.BillBusinessInfo.GetForm();
            if (form.FormIdDynamicProperty != null)
            {
                form.FormIdDynamicProperty.SetValue(billView.Model.DataObject, form.Id);
            }

            //获取数据包
            DynamicObject[] objs = new DynamicObject[] { billView.Model.DataObject };
            #endregion

            #region 保存

            IOperationResult saveResult =
                BusinessDataServiceHelper.Save(
                    this.Context, billView.BillBusinessInfo, billView.Model.DataObject, saveOption, "Save");

            //IOperationResult saveResult = ServiceHelper.GetService<ISaveService>().Save(
            //       this.Context, billView.BillBusinessInfo, objs, saveOption);

            result.MergeResult(saveResult);

            if (saveResult.IsSuccess)
            {
                result.SuccessDataEnity = saveResult.SuccessDataEnity;
            }

            if (!saveResult.IsSuccess)
            {
                return result;
            }

            #endregion

            #region 获取主键

            object[] pkArray = (from p in objs select p[0]).ToArray();
            #endregion

            #region 提交

            IOperationResult submitResult = ServiceHelper.GetService<ISubmitService>().Submit(
               this.Context, billView.BillBusinessInfo, pkArray, "Submit");

            result.MergeResult(submitResult);

            if (!submitResult.IsSuccess)
            {
                return result;
            }

            #endregion

            #region 审核

            IOperationResult auditResult = ServiceHelper.GetService<IAuditService>().Audit(
                    this.Context, billView.BillBusinessInfo, pkArray, OperateOption.Create());
            result.MergeResult(auditResult);

            #endregion

            #region 关闭订单
            billView.CommitNetworkCtrl();
            billView.Close();
            #endregion

            #region 返回操作信息
            //根据操作结果构造返回结果
            if ((result.ValidationErrors != null && result.ValidationErrors.Count > 0))
            {
                result.IsSuccess = false;
            }

            return result;
            #endregion
        }

        /// <summary>
        /// 创建View
        /// </summary>
        /// <param name="FormId"></param>
        /// <returns></returns>
        private IBillView CreateBankBillView(string FormId)
        {
            // 读取单据的元数据
            FormMetadata meta = MetaDataServiceHelper.Load(this.Context, FormId) as FormMetadata;
            Form form = meta.BusinessInfo.GetForm();
            // 创建用于引入数据的单据view
            Type type = Type.GetType("Kingdee.BOS.Web.Import.ImportBillView,Kingdee.BOS.Web");
            var billView = (IDynamicFormViewService)Activator.CreateInstance(type);
            // 开始初始化billView：
            // 创建视图加载参数对象，指定各种参数，如FormId, 视图(LayoutId)等
            BillOpenParameter openParam = CreateOpenParameter(meta);
            // 动态领域模型服务提供类，通过此类，构建MVC实例
            var provider = form.GetFormServiceProvider();
            billView.Initialize(openParam, provider);
            return billView as IBillView;
        }

        /// <summary>
        /// 创建视图加载参数对象，指定各种初始化视图时，需要指定的属性
        /// </summary>
        /// <param name="meta">元数据</param>
        /// <returns>视图加载参数对象</returns>
        private BillOpenParameter CreateOpenParameter(FormMetadata meta)
        {
            Form form = meta.BusinessInfo.GetForm();
            // 指定FormId, LayoutId
            BillOpenParameter openParam = new BillOpenParameter(form.Id, meta.GetLayoutInfo().Id);
            // 数据库上下文
            openParam.Context = this.Context;
            // 本单据模型使用的MVC框架
            openParam.ServiceName = form.FormServiceName;
            // 随机产生一个不重复的PageId，作为视图的标识
            openParam.PageId = Guid.NewGuid().ToString();
            // 元数据
            openParam.FormMetaData = meta;
            // 界面状态：新增 (修改、查看)
            openParam.Status = OperationStatus.ADDNEW;
            // 单据主键：本案例演示新建单据，不需要设置主键
            openParam.PkValue = null;
            // 界面创建目的：普通无特殊目的 （为工作流、为下推、为复制等）
            openParam.CreateFrom = CreateFrom.Default;
            // 基础资料分组维度：基础资料允许添加多个分组字段，每个分组字段会有一个分组维度
            // 具体分组维度Id，请参阅 form.FormGroups 属性
            openParam.GroupId = "";
            // 基础资料分组：如果需要为新建的基础资料指定所在分组，请设置此属性
            openParam.ParentId = 0;
            // 单据类型
            openParam.DefaultBillTypeId = "";
            // 业务流程
            openParam.DefaultBusinessFlowId = "";
            // 主业务组织改变时，不用弹出提示界面
            openParam.SetCustomParameter("ShowConfirmDialogWhenChangeOrg", false);
            // 插件
            List<AbstractDynamicFormPlugIn> plugs = form.CreateFormPlugIns();
            openParam.SetCustomParameter(FormConst.PlugIns, plugs);
            PreOpenFormEventArgs args = new PreOpenFormEventArgs(this.Context, openParam);
            foreach (var plug in plugs)
            {// 触发插件PreOpenForm事件，供插件确认是否允许打开界面
                plug.PreOpenForm(args);
            }
            if (args.Cancel == true)
            {// 插件不允许打开界面
                // 本案例不理会插件的诉求，继续....
            }
            // 返回
            return openParam;
        }

        /// <summary>
        /// 加载指定的单据进行修改
        /// </summary>
        /// <param name="billView"></param>
        /// <param name="pkValue"></param>
        private void ModifyBill(IBillView billView, string pkValue)
        {
            billView.OpenParameter.Status = OperationStatus.EDIT;
            billView.OpenParameter.CreateFrom = CreateFrom.Default;
            billView.OpenParameter.PkValue = pkValue;
            billView.OpenParameter.DefaultBillTypeId = string.Empty;
            ((IDynamicFormViewService)billView).LoadData();
        }

        /// <summary>
        /// 反审核
        /// </summary>
        /// <param name="billView"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private IOperationResult UnAudit(IBillView billView, string id)
        {
            object[] pkArray = new object[] { id };

            List<KeyValuePair<object, object>> pkIds = new List<KeyValuePair<object, object>>();
            pkIds.Add(new KeyValuePair<object, object>(id, ""));

            List<object> paraUnAudit = new List<object>();
            //2反审核
            paraUnAudit.Add("2");
            //审核意见
            paraUnAudit.Add("");
            IOperationResult result
                = ServiceHelper.GetService<ISetStatusService>().SetBillStatus(
                   this.Context, billView.BillBusinessInfo, pkIds, paraUnAudit, "UnAudit");

            return result;
        }

        bool HaveSalOutStock(string id)
        {
            string sql = $@"
                SELECT 1
                  FROM  T_SAL_ORDERENTRY A
		                INNER JOIN T_SAL_OUTSTOCKENTRY_LK B
		                ON A.FID = B.FSBILLID AND A.FENTRYID = B.FSID
                 WHERE  A.FID = {id}";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(this.Context, sql);
            if (data.Count > 0)
            {
                return true;
            }

            return false;
        }

        bool HaveQTOutStock(string id)
        {
            string sql = $@"
                SELECT 1
                  FROM  T_STK_OUTSTOCKAPPLYENTRY A WITH(NOLOCK)
		                INNER JOIN T_STK_MISDELIVERYENTRY_LK B WITH(NOLOCK)
		                ON A.FID = B.FSBILLID AND A.FENTRYID = B.FSID
                 WHERE  A.FID = {id}";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(this.Context, sql);
            if (data.Count > 0)
            {
                return true;
            }

            return false;
        }

        bool HaveNetWorkCtrlreCords(string id)
        {
            string sql = $@"
                SELECT  1
                  FROM  T_BAS_NETWORKCTRLRECORDS A WITH(NOLOCK)
                 WHERE  A.FInterId = {id}";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(this.Context, sql);
            if (data.Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}
