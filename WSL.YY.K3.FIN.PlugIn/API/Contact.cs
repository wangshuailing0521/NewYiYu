using Kingdee.BOS;
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
using System.Text;
using WSL.YY.K3.FIN.PlugIn.Helper;
using WSL.YY.K3.FIN.PlugIn.PlugIn;

namespace WSL.YY.K3.FIN.PlugIn.API
{
    [Description("联系人API")]
    public class Contact : AbstractWebApiBusinessService
    {
        private Context Context = null;
        private string formId = "BD_CommonContact";
        DBLog dBLog = new DBLog();
        StringBuilder sb = new StringBuilder();
        string k3Id = "";
        string zohoErpNumber = "";

        public Contact(KDServiceContext context)
           : base(context) {

            dBLog.FInvocation = "zoho";
            dBLog.FInterfaceType = "Contact";
            dBLog.FBeginTime = DateTime.Now.ToString();
            dBLog.Context = KDContext.Session.AppContext;
        }

        public JObject ExecuteService(string DataJson)
        {
            sb.AppendLine($@"接口方向：CRM --> Kingdee");
            sb.AppendLine($@"接口名称：联系人API");
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

            string erpId = model["Contact ERP ID"].ToString();
            dBLog.FBillNo = erpId;

            k3Id
                = SqlHelper.GetContactIdByNumber(this.Context, erpId);
            if (string.IsNullOrWhiteSpace(k3Id))
            {
                result = CreateBill(model, formId);
            }
            else
            {
                result = EditBill(model, formId);
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
                        k3Id = billObj["Id"].ToString();
                        number = billObj["Number"].ToString();
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
            objRetutrn.Add("Id", k3Id);
            objRetutrn.Add("Number", number);
            objRetutrn.Add("Message", message);
            return objRetutrn;
        }

        public IOperationResult CreateBill(JObject model, string FormId)
        {
            IOperationResult result = new OperationResult();

            //判断联系人是否已在金蝶中存在，如果存在则跳过保存
            string contactErpId = "";


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
            FillBankBillPropertys(billView, model);

            /*保存单据*/
            OperateOption saveOption = OperateOption.Create();
            result = this.SaveBill(billView, saveOption);

            contactErpId = billView.Model.DataObject["Id"].ToString();

            #region 修改ZohoId
            string type = "";
            if (model["Type"] != null)
            {
                type = model["Type"].ToString();
            }
            string birthday = "";
            if (model["Birthday"] != null)
            {
                birthday = model["Birthday"].ToString();
            }
            SqlHelper.UpdateContact(this.Context, contactErpId, model["Contact ID"].ToString(), type, birthday);
            #endregion

            #region 创建病人
            if (model["Type"].ToString() == "Patient")
            {
                Patient patient = new Patient();
                string crmContactId = model["Contact ID"].ToString();
                string number = model["Contact ERP ID"].ToString();
                string firstName = "";
                string midName = "";
                string lastName = "";
                if (model["First Name"] != null)
                {
                    firstName = model["First Name"].ToString();
                }
                if (model["Mid Name"] != null)
                {
                    midName = model["Mid Name"].ToString();
                }
                if (model["Last Name"] != null)
                {
                    lastName = model["Last Name"].ToString();
                }
                //姓名
                if (!string.IsNullOrWhiteSpace(midName))
                {
                    midName = " " + midName;
                }
                if (!string.IsNullOrWhiteSpace(lastName))
                {
                    lastName = " " + lastName;
                }
                string name = firstName + midName + lastName;
                IOperationResult patientResult
                    = patient.CreateBill(crmContactId, name, number, Context);

                result.MergeResult(patientResult);
            }
            #endregion


            #region 修改客户
            string zohoCustId = model["Account ID"].ToString();
            if (!string.IsNullOrWhiteSpace(zohoCustId))
            {
                long contactId = Convert.ToInt64(contactErpId);
                long custId
                    = SqlHelper.GetCustIdByZoho(Context, model["Account ID"].ToString());
                CustEdit custEdit = new CustEdit();
                custEdit.Edit(custId, contactId, Context);
            }
            #endregion

            return result;
        }

        public IOperationResult EditBill(JObject model, string FormId)
        {
            IOperationResult result = new OperationResult();

            //判断联系人是否已在金蝶中存在，如果存在则跳过保存
            string contactErpId = "";

            if (!string.IsNullOrWhiteSpace(k3Id))
            {
                /*构建一个IBillView实例，通过此实例，可以方便的填写单据各属性*/
                IBillView billView = this.CreateBankBillView(formId);

                //加载
                ModifyBill(billView, k3Id);

                /*加载数据*/
                FillBankBillPropertys(billView, model,"1");

                /*保存单据*/
                OperateOption saveOption = OperateOption.Create();
                result = this.SaveBill(billView, saveOption);

                contactErpId = billView.Model.DataObject["Id"].ToString();
            }

            return result;
        }

        /// <summary>
        /// 把单据的各属性，填写到IBillView当前所管理的单据中
        /// </summary>
        /// <param name="billView"></param>
        private void FillBankBillPropertys(IBillView billView, JObject model, string type = "0")
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


            if (type == "0")
            {
                if (model["Medtrum Organization"] != null)
                {
                    //使用组织
                    string orgNumber
                        = SqlHelper.GetOrgNumber(Context, model["Medtrum Organization"].ToString());
                    dynamicFormView.SetItemValueByNumber("FUseOrgId", orgNumber, 0);
                }

                //类型
                dynamicFormView.UpdateValue("FCompanyType", 0, "BD_Customer");

            }

            if (model["Contact ID"] != null)
            {
                //FZohoId
                dynamicFormView.UpdateValue("FZohoId", 0, model["Contact ID"].ToString());
            }

            if (model["Contact ERP ID"] != null)
            {
                //if (string.IsNullOrWhiteSpace(model["Contact ERP ID"].ToString()))
                //{
                //    throw new Exception("Contact ERP ID不能为空！");
                //}
                dynamicFormView.UpdateValue("FNumber", 0, model["Contact ERP ID"].ToString());
            }

            string firstName = "";
            string midName = "";
            string lastName = "";
            if (model["First Name"] != null)
            {
                firstName = model["First Name"].ToString();
            }
            if (model["Mid Name"] != null)
            {
                midName = model["Mid Name"].ToString();
            }
            if (model["Last Name"] != null)
            {
                lastName = model["Last Name"].ToString();
            }
            //姓名
            if (!string.IsNullOrWhiteSpace(midName))
            {
                midName = " " + midName;
            }
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                lastName = " " + lastName;
            }
            string name = firstName + midName + lastName;
            DynamicObject billObj = billView.Model.DataObject;
            LocaleValue localValue = new LocaleValue(name, 2052);
            LocaleValue localEValue = new LocaleValue(name, 1033);
            localValue.Merger(localEValue, ";");
            billObj["Name"] = localValue;
            //dynamicFormView.UpdateValue("FName", 0, model["First Name"].ToString()+ " " + model["Last Name"].ToString());


            if (model["Email"] != null)
            {
                //邮箱
                dynamicFormView.UpdateValue("FEmail", 0, model["Email"].ToString());
            }

            if (model["Phone"] != null)
            {
                //固定电话
                dynamicFormView.UpdateValue("FTel", 0, model["Phone"].ToString());
            }
            if (model["Mobile"] != null)
            {
                //移动电话
                dynamicFormView.UpdateValue("FMobile", 0, model["Mobile"].ToString());
            }

            //dynamicFormView.UpdateValue("FTel", 0, model["Birthday"].ToString());
            //dynamicFormView.UpdateValue("F_aaaa_Text", 0, "TEST");
            if (model["Birthday"] != null)
            {
                //生日
                dynamicFormView.UpdateValue("FBirthday", 0, model["Birthday"].ToString());
            }



            string country = "";
            string state = "";
            string city = "";
            string street = "";
            string zip = "";
            string title = "";
            string address = "";
            //详细地址
            if (model["Mailing Street"] != null)
            {
                //区县
                street = model["Mailing Street"].ToString();
                address = street;
            }

            if (model["Mailing City"] != null)
            {
                //城市
                title = string.IsNullOrWhiteSpace(address) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Mailing City"].ToString()))
                {
                    city = title + model["Mailing City"].ToString();
                }

                address = address + city;
            }
            if (model["Mailing State"] != null)
            {
                //省份
                title = string.IsNullOrWhiteSpace(address) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Mailing State"].ToString()))
                {
                    state = title + model["Mailing State"].ToString();
                }

                address = address + state;
            }
            if (model["Mailing Zip"] != null)
            {
                title = string.IsNullOrWhiteSpace(address) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Mailing Zip"].ToString()))
                {
                    zip = title + model["Mailing Zip"].ToString();
                }

                address = address + zip;
            }
            if (model["Mailing Country"] != null)
            {
                //国家
                title = string.IsNullOrWhiteSpace(address) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Mailing Country"].ToString()))
                {
                    country = title + model["Mailing Country"].ToString();
                }

                address = address + country;
            }

            dynamicFormView.UpdateValue("FBizAddress", 0, address);

            //Shipping地址
            string shippcountry = "";
            string shippstate = "";
            string shippcity = "";
            string shippstreet = "";
            string shippzip = "";
            string shippaddress = "";
            if (model["Other Street"] != null)
            {
                //区县
                shippstreet = model["Other Street"].ToString();
                shippaddress = shippstreet;
            }

            if (model["Other City"] != null)
            {
                //城市
                title = string.IsNullOrWhiteSpace(shippaddress) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Other City"].ToString()))
                {
                    shippcity = title + model["Other City"].ToString();
                }

                shippaddress = shippaddress + shippcity;
            }
            if (model["Other State"] != null)
            {
                //省份
                title = string.IsNullOrWhiteSpace(shippaddress) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Other State"].ToString()))
                {
                    shippstate = title + model["Other State"].ToString();
                }

                shippaddress = shippaddress + shippstate;
            }
            if (model["Other Zip"] != null)
            {
                title = string.IsNullOrWhiteSpace(shippaddress) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Other Zip"].ToString()))
                {
                    shippzip = title + model["Other Zip"].ToString();
                }

                shippaddress = shippaddress + shippzip;
            }
            if (model["Other Country"] != null)
            {
                //国家
                title = string.IsNullOrWhiteSpace(shippaddress) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Other Country"].ToString()))
                {
                    shippcountry = title + model["Other Country"].ToString();
                }

                shippaddress = shippaddress + shippcountry;
            }

            dynamicFormView.UpdateValue("FShippingAddress", 0, shippaddress);

            
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
