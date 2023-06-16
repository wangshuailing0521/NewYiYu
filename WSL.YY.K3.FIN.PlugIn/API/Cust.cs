using Kingdee.BOS;
using Kingdee.BOS.App;
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
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.ServicesStub;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WSL.YY.K3.FIN.PlugIn.Helper;
using WSL.YY.K3.FIN.PlugIn.Model;
using WSL.YY.K3.FIN.PlugIn.PlugIn;

namespace WSL.YY.K3.FIN.PlugIn.API
{
    [Description("客户API")]
    public class Cust: AbstractWebApiBusinessService
    {
        Context Context = null;
        string formId = "BD_Customer";
        DBLog dBLog = new DBLog();
        StringBuilder sb = new StringBuilder();
        K3Contact k3Contact = new K3Contact();
        List<string> k3Ids = new List<string>();
        List<string> orgIds = new List<string>();
        string k3Id = "";
        string billingAddress = "";
        string shippingAddress = "";

        public Cust(KDServiceContext context)
           : base(context)
        {
            dBLog.FInvocation = "zoho";
            dBLog.FInterfaceType = "Customer";
            dBLog.FBeginTime = DateTime.Now.ToString();
            dBLog.Context = KDContext.Session.AppContext;
        }

        public JObject ExecuteService(string DataJson)
        {
            sb.AppendLine($@"接口方向：CRM --> Kingdee");
            sb.AppendLine($@"接口名称：客户API");
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

            if (model["Account ERP ID"] != null)
            {
                string custNumber = model["Account ERP ID"].ToString();
                dBLog.FBillNo = custNumber;

                string createCustId 
                    = SqlHelper.GetCustCreateIdByNumber(this.Context, custNumber);

                #region 获取组织Id列表
                string orgNames = model["Medtrum Organization"].ToString();
                if (string.IsNullOrWhiteSpace(orgNames))
                {
                    orgIds.Add("1");
                }
                else
                {
                    List<string> orgNameList = orgNames.Split(',').ToList();

                    foreach (var orgName in orgNameList)
                    {
                        string orgId = SqlHelper.GetOrgId(Context, orgName);
                        orgIds.Add(orgId);
                    }
                }
               
                #endregion

                if (string.IsNullOrWhiteSpace(createCustId))
                {
                    result = CreateBill(model, formId);
                }
                else
                {
                    result = EditBill(model, formId);
                }

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

            //保存单据,忽略交互提示
            OperateOption saveOption = OperateOption.Create();
            saveOption.SetIgnoreWarning(true);
            saveOption.SetIgnoreScopeValidateFlag(true);
            result = this.SaveBill(billView, saveOption, model);

            #region 分配客户
            if (model["Medtrum Organization"] != null)
            {
                if (!string.IsNullOrWhiteSpace(model["Medtrum Organization"].ToString()))
                {
                    CustAssign assign = new CustAssign();
                    string custId
                        = SqlHelper.GetCustCreateIdByNumber(this.Context, model["Account ERP ID"].ToString());
                    List<string> allocateOrgIds
                        = SqlHelper.GetOrgIdByCustNumber(this.Context, model["Account ERP ID"].ToString(), orgIds);

                    if (allocateOrgIds.Any())
                    {
                        string aggignResult
                        = assign.Assign(custId, allocateOrgIds);

                        OperateResult op = new OperateResult();
                        op.Message = aggignResult;
                        result.OperateResult.Add(op);
                    }
                }
            }            
            #endregion


            return result;
        }

        private IOperationResult EditBill(JObject model, string FormId)
        {
            IOperationResult result = new OperationResult();

            IBillView billView = this.CreateBankBillView(FormId);

            string custNumber = model["Account ERP ID"].ToString();

            k3Ids
                = SqlHelper.GetAllCustIdByNumber(this.Context, custNumber, string.Join(",", orgIds));

            foreach (var k3id in k3Ids)
            {
                //反审核
                IOperationResult unAuditResult
                     = UnAudit(billView, k3id);

                //加载
                ModifyBill(billView, k3id);

                this.FillBankBillPropertys(billView, model,"1");

                //保存单据
                OperateOption saveOption = OperateOption.Create();
                result = this.SaveBill(billView, saveOption, model);

                foreach (var item in unAuditResult.OperateResult)
                {
                    result.OperateResult.Add(item);
                }             
            }

            #region 分配客户
            CustAssign assign = new CustAssign();
            string custId 
                = SqlHelper.GetCustCreateIdByNumber(this.Context, model["Account ERP ID"].ToString());
            List<string> allocateOrgIds 
                = SqlHelper.GetOrgIdByCustNumber(this.Context, model["Account ERP ID"].ToString(), orgIds);

            if (allocateOrgIds.Any())
            {
                string aggignResult
                = assign.Assign(custId, allocateOrgIds);

                OperateResult op = new OperateResult();
                op.Message = aggignResult;
                result.OperateResult.Add(op);
            }
           
            #endregion

            return result;
        }

        /// <summary>
        /// 把单据的各属性，填写到IBillView当前所管理的单据中
        /// </summary>
        /// <param name="billView"></param>
        private void FillBankBillPropertys(IBillView billView, JObject model,string type = "0")
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


            //string orgNumber
            //    = SqlHelper.GetOrgNumber(Context, model["Medtrum Organization"].ToString());
            if (type == "0")
            {
                //创建组织
                dynamicFormView.SetItemValueByNumber("FCreateOrgId", "0000", 0);

                //使用组织
                dynamicFormView.SetItemValueByNumber("FUseOrgId", "0000", 0);
            }
           

            //ZohoId
            dynamicFormView.UpdateValue("FZohoId", 0, model["Account ID"].ToString());

            //ZohoNumber
            if (model["Account ERP ID"] != null)
            {
                //if (string.IsNullOrWhiteSpace(model["Account ERP ID"].ToString()))
                //{
                //    throw new Exception("Account ERP ID不能为空！");
                //}            
                dynamicFormView.UpdateValue("FNumber", 0, model["Account ERP ID"].ToString());
            }

            //客户名称
            string name = model["Account Name"].ToString();
            DynamicObject billObj = billView.Model.DataObject;           
            LocaleValue localValue = new LocaleValue(name, 2052);
            LocaleValue localEValue = new LocaleValue(name, 1033);
            localValue.Merger(localEValue, ";");
            billObj["Name"] = localValue;

            dynamicFormView.UpdateValue("FINVOICETITLE", 0, name);

            if (model["AccountRef"] != null)
            {
                dynamicFormView.UpdateValue("F_AAAA_TEXT3", 0, model["AccountRef"].ToString());
            }

            //客户分组
            string erpId = model["Account ERP ID"].ToString();
            if (erpId.Contains("MED"))
            {
                dynamicFormView.SetItemValueByNumber("FGroup", "1001", 0);
            }
            else
            {
                dynamicFormView.SetItemValueByNumber("FGroup", "1002", 0);
            }
            

            string country = "";
            string state = "";
            string city = "";
            string street = "";
            string zip = "";
            string title = "";
            string address = "";

            if (model["Billing Street"] != null)
            {
                //区县
                street = model["Billing Street"].ToString();
                address = street;
            }
            if (model["Billing City"] != null)
            {
                //城市
                title = string.IsNullOrWhiteSpace(address) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Billing City"].ToString()))
                {
                    city = title + model["Billing City"].ToString();
                }
                address = address + city;
            }
            if (model["Billing State"] != null)
            {
                //省份
                title = string.IsNullOrWhiteSpace(address) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Billing State"].ToString()))
                {
                    state = title + model["Billing State"].ToString();
                }
               
                address = address + state;
            }
            if (model["Billing Code"] != null)
            {
                title = string.IsNullOrWhiteSpace(address) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Billing Code"].ToString()))
                {
                    zip = title + model["Billing Code"].ToString();
                }
                
                address = address + zip;
            }
            if (model["Account Country"] != null)
            {
                //国家
                country = model["Account Country"].ToString();
                string countryNumber
                    = SqlHelper.GetCountryNumber(Context, model["Account Country"].ToString());
                dynamicFormView.SetItemValueByNumber("FCOUNTRY", countryNumber, 0);

                title = string.IsNullOrWhiteSpace(address) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Account Country"].ToString()))
                {
                    country = title + model["Account Country"].ToString();
                }
               
                address = address + country;
            }

            billingAddress = address;
            dynamicFormView.UpdateValue("FADDRESS", 0, address);
            dynamicFormView.UpdateValue("FINVOICEADDRESS", 0, address);

            //Shipping地址
            string shippcountry = "";
            string shippstate = "";
            string shippcity = "";
            string shippstreet = "";
            string shippzip = "";
            string shippaddress = "";
            if (model["Shipping Street"] != null)
            {
                //区县
                shippstreet = model["Shipping Street"].ToString();
                shippaddress = shippstreet;
            }
            if (model["Shipping City"] != null)
            {
                //城市
                title = string.IsNullOrWhiteSpace(shippaddress) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Shipping City"].ToString()))
                {
                    shippcity = title + model["Shipping City"].ToString();
                }
                
                shippaddress = shippaddress + shippcity;
            }
            if (model["Shipping State"] != null)
            {
                //省份
                title = string.IsNullOrWhiteSpace(shippaddress) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Shipping State"].ToString()))
                {
                    shippstate = title + model["Shipping State"].ToString();
                }
                
                shippaddress = shippaddress + shippstate;
            }
            if (model["Shipping Zip"] != null)
            {
                title = string.IsNullOrWhiteSpace(shippaddress) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Shipping Zip"].ToString()))
                {
                    shippzip = title + model["Shipping Zip"].ToString();
                }                
                shippaddress = shippaddress + shippzip;
            }
            if (model["Shipping Country"] != null)
            {
                //国家
                title = string.IsNullOrWhiteSpace(shippaddress) ? "" : ", ";
                if (!string.IsNullOrWhiteSpace(model["Shipping Country"].ToString()))
                {
                    shippcountry = title + model["Shipping Country"].ToString();
                }
                
                shippaddress = shippaddress + shippcountry;
            }

            shippingAddress = shippaddress;
            dynamicFormView.UpdateValue("FShippingAddress", 0, shippaddress);

            billView.Model.DeleteEntryData("FT_BD_CUSTCONTACT");
            if (!string.IsNullOrWhiteSpace(address))
            {
                billView.Model.CreateNewEntryRow("FT_BD_CUSTCONTACT");
                dynamicFormView.UpdateValue("FADDRESS1", 0, address);
            }
               
            if (!string.IsNullOrWhiteSpace(shippaddress))
            {
                billView.Model.CreateNewEntryRow("FT_BD_CUSTCONTACT");
                dynamicFormView.UpdateValue("FADDRESS1", 1, shippaddress);
            }

            //结算方式
            if (model["Settlement Method"] != null)
            {
                string settleTypeNumber
                    = SqlHelper.GetSettleTypeNumber(Context, model["Settlement Method"].ToString());
                dynamicFormView.SetItemValueByNumber("FSETTLETYPEID", settleTypeNumber, 0);
            }

            //收款条件
            if (model["Payment Terms"] != null)
            {
                string recConditionNumber
                    = SqlHelper.GetRecConditionNumber(Context, model["Payment Terms"].ToString());
                dynamicFormView.SetItemValueByNumber("FRECCONDITIONID", recConditionNumber, 0);
            }

            //币别
            if (model["Currency"] != null)
            {
                string currNumber
                    = SqlHelper.GetCurrencyNumber(Context, model["Currency"].ToString());
                dynamicFormView.SetItemValueByNumber("FTRADINGCURRID", currNumber, 0);
            }

            //税率
            if (model["Rate"] != null)
            {
                string rateNumber
                    = SqlHelper.GetTaxRateId(Context, Convert.ToDecimal(model["Rate"].ToString()), model["Account Country"].ToString());
                if (!string.IsNullOrWhiteSpace(rateNumber))
                {
                    dynamicFormView.SetItemValueByNumber("FTaxRate", rateNumber, 0);
                }
            }

            if (erpId.Contains("MED"))
            {
                dynamicFormView.SetItemValueByNumber("FTaxRate", "SL04_SYS", 0);
            }

            //Vat
            if (model["Vat"] != null)
            {
                dynamicFormView.UpdateValue("F_aaaa_Text", 0, model["Vat"].ToString());
            }
        }

        /// <summary>
        /// 保存单据，并显示保存结果
        /// </summary>
        /// <param name="billView"></param>
        /// <returns></returns>
        private IOperationResult SaveBill(IBillView billView, OperateOption saveOption, JObject model)
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

            IOperationResult saveResult = ServiceHelper.GetService<ISaveService>().Save(
                   this.Context, billView.BillBusinessInfo, objs, saveOption);

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

            #region 添加联系人

            //AddContact(billView, model, Convert.ToInt64(pkArray[0]));

            //IOperationResult contactSave = ServiceHelper.GetService<ISaveService>().Save(
            //    this.Context, billView.BillBusinessInfo, objs, saveOption);

            //if (!contactSave.IsSuccess)
            //{
            //    result.MergeResult(contactSave);
            //    foreach (var item in contactSave.ValidationErrors)
            //    {
            //        result.ValidationErrors.Add(item);
            //    }
            //    return result;
            //}

            #endregion

            #region 添加地址信息

            //AddLocation(billView);

            //IOperationResult locationSave = ServiceHelper.GetService<ISaveService>().Save(
            //   this.Context, billView.BillBusinessInfo, objs, saveOption);

            //if (!locationSave.IsSuccess)
            //{
            //    result.MergeResult(locationSave);
            //    foreach (var item in locationSave.ValidationErrors)
            //    {
            //        result.ValidationErrors.Add(item);
            //    }
            //    return result;
            //}

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

        private IOperationResult UnAudit(IBillView billView,string id)
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
                   this.Context, billView.BillBusinessInfo, pkIds, paraUnAudit,"UnAudit");

            return result;
        }

        /// <summary>
        /// 添加联系人单据体
        /// </summary>
        /// <param name="billView"></param>
        /// <param name="model"></param>
        /// <param name="custId"></param>
        private void AddContact(IBillView billView, JObject model, long custId)
        {
            IDynamicFormViewService dynamicFormView = billView as IDynamicFormViewService;

            //创建联系人
            ContactEdit contactEdit = new ContactEdit();
            IOperationResult contactResult
                        = contactEdit.CreateBill(Context, model, custId);
            
            if (contactResult.SuccessDataEnity != null)
            {
                if (contactResult.SuccessDataEnity.Count() > 0)
                {
                    foreach (DynamicObject item in contactResult.SuccessDataEnity)
                    {
                        k3Contact
                            = SqlHelper.GetContactById(Context, Convert.ToInt64(item["Id"]));

                        string country = "";
                        string state = "";
                        string city = "";
                        string street = "";

                        if (model["Shipping Country"] != null)
                        {
                            country = model["Shipping Country"].ToString();
                        }
                        if (model["Shipping State"] != null)
                        {
                            state = model["Shipping State"].ToString();
                        }
                        if (model["Shipping City"] != null)
                        {
                            city = model["Shipping City"].ToString();
                        }
                        if (model["Shipping Street"] != null)
                        {
                            street = model["Shipping Street"].ToString();
                        }

                        string address = country + state + city + street;
                        k3Contact.ShipAddressDetail = address;

                        //联系人页签
                        billView.Model.CreateNewEntryRow("FT_BD_CUSTLOCATION");
                        dynamicFormView.SetItemValueByNumber("FContactId", k3Contact.Number, 0);
                        ((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FContactId", 0);

                        //地址页签
                        billView.Model.CreateNewEntryRow("FT_BD_CUSTCONTACT");
                        dynamicFormView.UpdateValue("FNUMBER1", 0, k3Contact.BillAddressNumber);
                        dynamicFormView.UpdateValue("FNAME1", 0, k3Contact.BillAddressName);
                        dynamicFormView.UpdateValue("FADDRESS1", 0, k3Contact.BillAddressDetail);
                        dynamicFormView.UpdateValue("FTTel", 0, k3Contact.Tel);
                        dynamicFormView.UpdateValue("FMOBILE", 0, k3Contact.Mobile);
                        dynamicFormView.UpdateValue("FEMail", 0, k3Contact.Email);
                        dynamicFormView.SetItemValueByID("FTContact", k3Contact.Id, 0);
                        ((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FTContact", 0);
                        if (!string.IsNullOrWhiteSpace(k3Contact.ShipAddressDetail))
                        {
                            billView.Model.CreateNewEntryRow("FT_BD_CUSTCONTACT");
                            dynamicFormView.UpdateValue("FADDRESS1", 1, k3Contact.ShipAddressDetail);
                            dynamicFormView.UpdateValue("FTTel", 1, k3Contact.Tel);
                            dynamicFormView.UpdateValue("FMOBILE", 1, k3Contact.Mobile);
                            dynamicFormView.UpdateValue("FEMail", 1, k3Contact.Email);
                            dynamicFormView.SetItemValueByID("FTContact", k3Contact.Id, 1);
                            ((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FTContact", 1);
                        }
                    }
                }
            }           
        }

        /// <summary>
        /// 添加地址信息单据体
        /// </summary>
        /// <param name="billView"></param>
        private void AddLocation(IBillView billView)
        {
            IDynamicFormViewService dynamicFormView = billView as IDynamicFormViewService;
            //地址页签
            billView.Model.CreateNewEntryRow("FT_BD_CUSTCONTACT");
            dynamicFormView.UpdateValue("FNUMBER1", 0, k3Contact.BillAddressNumber);
            dynamicFormView.UpdateValue("FNAME1", 0, k3Contact.BillAddressName);
            dynamicFormView.UpdateValue("FADDRESS1", 0, k3Contact.BillAddressDetail);            
            dynamicFormView.UpdateValue("FTTel", 0, k3Contact.Tel);
            dynamicFormView.UpdateValue("FMOBILE", 0, k3Contact.Mobile);
            dynamicFormView.UpdateValue("FEMail", 0, k3Contact.Email);
            dynamicFormView.SetItemValueByID("FTContact", k3Contact.Id, 0);
            ((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FTContact", 0);
            if (!string.IsNullOrWhiteSpace(k3Contact.ShipAddressDetail))
            {
                billView.Model.CreateNewEntryRow("FT_BD_CUSTCONTACT");
                dynamicFormView.UpdateValue("FADDRESS1", 1, k3Contact.ShipAddressDetail);
                dynamicFormView.UpdateValue("FTTel", 1, k3Contact.Tel);
                dynamicFormView.UpdateValue("FMOBILE", 1, k3Contact.Mobile);
                dynamicFormView.UpdateValue("FEMail", 1, k3Contact.Email);
                dynamicFormView.SetItemValueByID("FTContact", k3Contact.Id, 1);
                ((IDynamicFormView)dynamicFormView).InvokeFieldUpdateService("FTContact", 1);
            }
           
        }

        /// <summary>
        /// 初始化billView
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
    }
}
