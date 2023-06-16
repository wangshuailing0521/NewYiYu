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
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WSL.YY.K3.FIN.PlugIn.Helper;

namespace WSL.YY.K3.FIN.PlugIn.PlugIn
{
    [Description("联系人修改")]
    public class ContactEdit
    {
        private Context Context;
        private string formId = "BD_CommonContact";

        public IOperationResult CreateBill(Context context, JObject model, long custId =0)
        {
            Context = context;
            IOperationResult result = new OperationResult();

            //判断联系人是否已在金蝶中存在，如果存在则跳过保存
            //string contactErpId = model["Contact ERP ID"].ToString();
            //if (string.IsNullOrWhiteSpace(contactErpId))
            //{
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
                FillBankBillPropertys(billView, model,custId);

                /*保存单据*/
                OperateOption saveOption = OperateOption.Create();
                result = this.SaveBill(billView, saveOption);

                string contactErpId = billView.Model.DataObject["Id"].ToString();

                #region 创建病人
                if (model["Type"].ToString() == "Patient")
                {
                    Patient patient = new Patient();

                string crmContactId = "";
                string crmName = "";
                string number = "";
                if (model["Billing Contact"] != null)
                {
                    crmName = model["Billing Contact"].ToString();
                    crmContactId = model["Billing Contact"].ToString();
                }
                if (model["Contact ID"] != null)
                {
                    crmContactId = model["Contact ID"].ToString();
                }

                if (model["Contact ZOHO ID"] != null)
                {
                    number = model["Contact ZOHO ID"].ToString();
                }
                IOperationResult patientResult
                    = patient.CreateBill(crmContactId, crmName,number, Context);

                result.MergeResult(patientResult);
            }
            #endregion
            //}

            return result;
        }

        /// <summary>
        /// 把单据的各属性，填写到IBillView当前所管理的单据中
        /// </summary>
        /// <param name="billView"></param>
        private void FillBankBillPropertys(IBillView billView, JObject model,long custId)
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

            //使用组织
            string orgNumber = "0000";
            //if (model["Medtrum Organization"] != null)
            //{
            //    if (!string.IsNullOrWhiteSpace(model["Medtrum Organization"].ToString()))
            //    {
            //        orgNumber
            //            = SqlHelper.GetOrgNumber(Context, model["Medtrum Organization"].ToString());
            //    }
            //}
            
            dynamicFormView.SetItemValueByNumber("FUseOrgId", orgNumber, 0);

            //类型
            dynamicFormView.UpdateValue("FCompanyType", 0, "BD_Customer");

            //所属公司
            dynamicFormView.SetItemValueByID("FCompany", custId, 0);

            if (model["Billing Contact"] != null)
            {
                //姓名
                dynamicFormView.UpdateValue("FName", 0, model["Billing Contact"].ToString());
            }
            if (model["Billing Contact"] != null)
            {
                //邮箱
                dynamicFormView.UpdateValue("FEmail", 0, model["Billing Email"].ToString());
            }

            //详细地址
            string country = "";
            string state = "";
            string city = "";
            string street = "";

            if (model["Billing Country"] != null)
            {
                country = model["Billing Country"].ToString();
            }
            if (model["Billing State"] != null)
            {
                state = model["Billing State"].ToString();
            }
            if (model["Billing City"] != null)
            {
                city = model["Billing City"].ToString();
            }
            if (model["Billing Street"] != null)
            {
                street = model["Billing Street"].ToString();
            }
            string address = country + state + city + street;
            dynamicFormView.UpdateValue("FBizAddress", 0, address);
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

            //根据操作结果构造返回结果
            if ((result.ValidationErrors != null && result.ValidationErrors.Count > 0))
            {
                result.IsSuccess = false;
            }

            return result;
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
