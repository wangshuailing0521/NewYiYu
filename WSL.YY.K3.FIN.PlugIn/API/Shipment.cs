using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Core;
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

namespace WSL.YY.K3.FIN.PlugIn.API
{
    [Description("样品申请单WEBAPI")]
    public class Shipment : AbstractWebApiBusinessService
    {
        Context Context = null;
        string formId = "BD_Cust";
        StringBuilder sb = new StringBuilder();

        public Shipment(KDServiceContext context)
           : base(context)
        { }

        public JObject ExecuteService(string DataJson)
        {
            sb.AppendLine($@"请求信息：{DataJson}");
            JObject objRetutrn = new JObject();
            try
            {
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
        }

        private JObject Create(string json)
        {
            JObject model = JObject.Parse(json);
            IOperationResult result = CreateBill(model, formId);
            JObject objRetutrn = new JObject();
            objRetutrn.Add("IsSuccess", result.IsSuccess);
            string message = "";
            string number = "";
            if (result.IsSuccess)
            {
                foreach (DynamicObject billObj in result.SuccessDataEnity)
                {
                    number = billObj["BillNo"].ToString();
                }
                foreach (var item in result.OperateResult)
                {

                    message = message + item.Message.ToString() + "\r\n";
                }
            }
            else
            {
                foreach (var item in result.ValidationErrors)
                {
                    message = message + item.Message.ToString() + "\r\n";
                }
            }
            objRetutrn.Add("Number", number);
            objRetutrn.Add("Message", message);
            return objRetutrn;
        }

        /// <summary>
        /// 生成单据
        /// </summary>
        /// <param name="drawCashBill"></param>
        private IOperationResult CreateBill(JObject model, string FormId)
        {
            IOperationResult result = new OperationResult();

            // 构建一个IBillView实例，通过此实例，可以方便的填写单据各属性
            IBillView billView = this.CreateBankBillView(FormId);
            // 新建一个空白单据
            // billView.CreateNewModelData();
            ((IBillViewService)billView).LoadData();

            // 触发插件的OnLoad事件：
            // 组织控制基类插件，在OnLoad事件中，对主业务组织改变是否提示选项进行初始化。
            // 如果不触发OnLoad事件，会导致主业务组织赋值不成功
            DynamicFormViewPlugInProxy eventProxy = billView.GetService<DynamicFormViewPlugInProxy>();
            eventProxy.FireOnLoad();


            this.FillBankBillPropertys(billView, model);

            // 保存单据
            OperateOption saveOption = OperateOption.Create();
            result = this.SaveBill(billView, saveOption);

            return result;
        }

        /// <summary>
        /// 把单据的各属性，填写到IBillView当前所管理的单据中
        /// </summary>
        /// <param name="billView"></param>
        private void FillBankBillPropertys(IBillView billView, JObject model)
        {
            // 把billView转换为IDynamicFormViewService接口：
            // 调用IDynamicFormViewService.UpdateValue: 会执行字段的值更新事件
            // 调用 dynamicFormView.SetItemValueByNumber ：不会执行值更新事件，需要继续调用：
            // ((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService(key, rowIndex);
            IDynamicFormViewService dynamicFormView = billView as IDynamicFormViewService;

            /********************物料页签上的字段******************/
            // 创建组织、使用组织 : 
            // 基础资料字段，用编码录入 (SetItemValueByNumber)
            // 特别说明：基础资料字段，也可以使用SetValue函数赋值，填写基础资料内码
            // 本示例，模拟引入数据，并不清楚这些组织的内码是多少，只知道编码，所以选用SetItemValueByNumber
            // 函数参数 : 基础资料字段Key，组织编码，行号
            // 文本(简单值类型)，直接使用SetValue赋值

            //单据类型
            //JObject FBillType = JObject.Parse(model["FBillType"].ToString());
            //dynamicFormView.SetItemValueByNumber("FBillType",
            //   FBillType["FNumber"].ToString(), 0);

            //业务日期
            dynamicFormView.UpdateValue("FPeriodEnddate", 0, model["FPeriodEnddate"].ToString());


            //资产组织
            JObject FAssetOrgID = JObject.Parse(model["FAssetOrgID"].ToString());
            dynamicFormView.SetItemValueByNumber("FAssetOrgID",
               FAssetOrgID["FNumber"].ToString(), 0);

            //货主组织
            JObject FOwnerOrgID = JObject.Parse(model["FOwnerOrgID"].ToString());
            dynamicFormView.SetItemValueByNumber("FOwnerOrgID",
               FOwnerOrgID["FNumber"].ToString(), 0);

            //是否车辆
            //货主组织
            JObject FASSETTYPEID = JObject.Parse(model["FASSETTYPEID"].ToString());
            dynamicFormView.SetItemValueByNumber("FASSETTYPEID",
               FASSETTYPEID["FNumber"].ToString(), 0);
            //dynamicFormView.UpdateValue("FASSETTYPEID", 0, model["FASSETTYPEID"].ToString());

            //处置方式
            JObject FDisPoseMethod = JObject.Parse(model["FDisPoseMethod"].ToString());
            dynamicFormView.SetItemValueByNumber("FDisPoseMethod",
               FDisPoseMethod["FNumber"].ToString(), 0);

            //处置原因
            dynamicFormView.UpdateValue("FDisposeReason", 0, model["FDisposeReason"].ToString());



            //资产
            string details = model["FDISPOSALENTRY"].ToString();
            JArray details0 = JArray.Parse(details);
            var detail = details0[0];

            billView.Model.CreateNewEntryRow("FDISPOSALENTRY");
            //卡片编码
            JObject FAlterId = JObject.Parse(detail["FAlterId"].ToString());
            dynamicFormView.SetItemValueByNumber("FAlterId",
               FAlterId["FNumber"].ToString(), 0);

            //处置币别
            JObject FDisposalCurrency = JObject.Parse(detail["FDisposalCurrency"].ToString());
            dynamicFormView.SetItemValueByNumber("FDisposalCurrency",
               FDisposalCurrency["FNumber"].ToString(), 0);

            ////货主组织
            //JObject FUnitId = JObject.Parse(detail["FUnitId"].ToString());
            //dynamicFormView.SetItemValueByNumber("FUnitId",
            //   FUnitId["FNumber"].ToString(), 0);

            //FQuantity
            dynamicFormView.UpdateValue("FQuantity", 0, Convert.ToDecimal(detail["FQuantity"].ToString()));

            //FDISPOSEQTY
            dynamicFormView.UpdateValue("FDISPOSEQTY", 0, Convert.ToDecimal(detail["FDISPOSEQTY"].ToString()));

            //FDISPOSEQTY
            // dynamicFormView.UpdateValue("FDISPOSEQTY", 0, Convert.ToDecimal(detail["FDISPOSEQTY"].ToString()));
            //未税成本
            //dynamicFormView.UpdateValue("FOriginalCost", 0, Convert.ToDecimal(fince["FOriginalCost"].ToString()));

            //未税成本
            //dynamicFormView.UpdateValue("FOriginalCost", 0, Convert.ToDecimal(fince["FOriginalCost"].ToString()));

            //实物
            //数量
            //string FSubEntitys = detail["FSubEntity"].ToString();
            //JArray FSubEntity0 = JArray.Parse(FSubEntitys);
            //var FSubEntity = FSubEntity0[0];
            //dynamicFormView.UpdateValue("FQty", 0, Convert.ToDecimal(FSubEntity["FQty"].ToString()));

            //财务
            string FDisposalFinances = detail["FDisposalFinance"].ToString();
            JArray FDisposalFinance0 = JArray.Parse(FDisposalFinances);
            var FDisposalFinance = FDisposalFinance0[0];
        }

        /// <summary>
        /// 保存单据，并显示保存结果
        /// </summary>
        /// <param name="billView"></param>
        /// <returns></returns>
        private IOperationResult SaveBill(IBillView billView, OperateOption saveOption)
        {
            IOperationResult result = new OperationResult();

            // 设置FormId
            Form form = billView.BillBusinessInfo.GetForm();
            if (form.FormIdDynamicProperty != null)
            {
                form.FormIdDynamicProperty.SetValue(billView.Model.DataObject, form.Id);
            }

            // 调用保存操作
            IOperationResult saveResult = BusinessDataServiceHelper.Save(
                        this.Context,
                        billView.BillBusinessInfo,
                        billView.Model.DataObject,
                        saveOption,
                        "Save");

            result.MergeResult(saveResult);

            if (saveResult.IsSuccess)
            {
                result.SuccessDataEnity = saveResult.SuccessDataEnity;
            }

            //提交目标单据
            DynamicObject[] destObjs = new DynamicObject[] { billView.Model.DataObject };
            if (saveResult.IsSuccess == true)
            {
                object[] pkArray = (from p in destObjs
                                    select p[0]).ToArray();

                IOperationResult submitResult = ServiceHelper.GetService<ISubmitService>().Submit(
                    this.Context, billView.BillBusinessInfo, pkArray, "Submit");
                result.MergeResult(submitResult);

                if (submitResult.IsSuccess == true)
                {
                    List<KeyValuePair<object, object>> pkIds = new List<KeyValuePair<object, object>>();
                    foreach (var o in pkArray)
                    {
                        pkIds.Add(new KeyValuePair<object, object>(o, ""));
                    }
                    List<object> paraAudit = new List<object>();
                    //1审核通过
                    paraAudit.Add("1");
                    //审核意见
                    paraAudit.Add("");
                    IOperationResult auditResult = ServiceHelper.GetService<SetStatusService>().SetBillStatus(
                        this.Context, billView.BillBusinessInfo, pkIds, paraAudit, "Audit", OperateOption.Create());
                    result.MergeResult(auditResult);

                }
            }

            //根据操作结果构造返回结果
            if ((result.ValidationErrors != null && result.ValidationErrors.Count > 0))
            {
                result.IsSuccess = false;
            }

            return result;

            #region MyRegion
            // 显示处理结果
            //if (saveResult == null)
            //{
            //    //this.View.ShowErrMessage("未知原因导致保存单据失败！");
            //    return;
            //}
            //else if (saveResult.IsSuccess == true)
            //{// 保存成功，直接显示
            //    //this.View.ShowOperateResult(saveResult.OperateResult);
            //    return;
            //}
            //else if (saveResult.InteractionContext != null
            //        && saveResult.InteractionContext.Option.GetInteractionFlag().Count > 0)
            //{// 保存失败，需要用户确认问题
            //    //InteractionUtil.DoInteraction(this.View, saveResult, saveOption,
            //    //    new Action<FormResult, IInteractionResult, OperateOption>((
            //    //        formResult, opResult, option) =>
            //    //    {
            //    //        // 用户确认完毕，重新调用保存处理
            //    //        this.SaveMaterial(billView, option);
            //    //    }));
            //}
            //// 保存失败，显示错误信息
            //if (saveResult.IsShowMessage)
            //{
            //    //saveResult.MergeValidateErrors();
            //    //this.View.ShowOperateResult(saveResult.OperateResult);
            //    //return;
            //}
            #endregion
        }

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
    }
}
