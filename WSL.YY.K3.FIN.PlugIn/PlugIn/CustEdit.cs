using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSL.YY.K3.FIN.PlugIn.Helper;
using WSL.YY.K3.FIN.PlugIn.Model;

namespace WSL.YY.K3.FIN.PlugIn.PlugIn
{
    [Description("客户修改")]
    public class CustEdit
    {
        private Context context;

        public void Edit(long custId,long contactId, Context Context)
        {
            context = Context;

            FormMetadata formMetadata =
                Kingdee.BOS.App.ServiceHelper
                .GetService<IMetaDataService>()
                .Load(context, "BD_Customer") as FormMetadata;

            List<KeyValuePair<object, object>> pkIds
                   = new List<KeyValuePair<object, object>>();

            pkIds.Add(new KeyValuePair<object, object>(custId, ""));

            //反审核单据
            UnAuditBill(formMetadata.BusinessInfo, pkIds);

            List<DynamicObject> billObjList = new List<DynamicObject>();
 
                DynamicObject billObj
                    = Kingdee.BOS.App.ServiceHelper.GetService<IViewService>().
                        LoadSingle(context, custId, formMetadata.BusinessInfo.GetDynamicObjectType());
                //修改单据
                EditCust(formMetadata.BusinessInfo, billObj, contactId);
                billObjList.Add(billObj);
            

            //保存单据
            OperateOption saveOption = OperateOption.Create();
            IOperationResult saveResult
                = BusinessDataServiceHelper.Save(
                       context,
                       formMetadata.BusinessInfo,
                       billObjList.ToArray(),
                       saveOption,
                       "Save");

            if (saveResult.IsSuccess)
            {
                //审核单据
                AuditBill(formMetadata.BusinessInfo, pkIds);
            }

            if ((saveResult.ValidationErrors != null && saveResult.ValidationErrors.Count > 0))
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in saveResult.ValidationErrors)
                {
                    sb.AppendLine(item.Message);
                }
                throw new Exception(sb.ToString());
            }
        }

        void EditCust(BusinessInfo businessInfo, DynamicObject billObj,long contactId)
        {
            K3Contact k3Contact 
                = SqlHelper.GetContactById(context, contactId);

            #region 添加地址单据体
            DynamicObjectCollection addressEntrys
               = billObj["BD_CUSTCONTACT"] as DynamicObjectCollection;

            DynamicObject address
                = new DynamicObject(addressEntrys.DynamicCollectionItemPropertyType);

            address["NUMBER"] = k3Contact.BillAddressNumber;
            address["NAME"] = k3Contact.BillAddressName;
            address["ADDRESS"] = k3Contact.BillAddressDetail;
            address["TContact_Id"] = k3Contact.Id;
            address["TTel"] = k3Contact.Tel;
            address["MOBILE"] = k3Contact.Mobile;
            address["EMail"] = k3Contact.Email;

            addressEntrys.Add(address);
            #endregion


            #region 添加联系人单据体
            DynamicObjectCollection contactEntrys
               = billObj["BD_CUSTLOCATION"] as DynamicObjectCollection;

            DynamicObject contact
                = new DynamicObject(contactEntrys.DynamicCollectionItemPropertyType);

            contact["ContactId_Id"] = k3Contact.Id;

            contactEntrys.Add(contact);
            #endregion

        }

        /// <summary>
        /// 反审核单据
        /// </summary>
        /// <param name="billView"></param>
        /// <param name="pkIds"></param>
        private void UnAuditBill(BusinessInfo businessInfo,
            List<KeyValuePair<object, object>> pkIds)
        {
            List<object> paraUnAudit = new List<object>();
            //2反审核
            paraUnAudit.Add("2");
            //审核意见
            paraUnAudit.Add("");
            Kingdee.BOS.App.ServiceHelper.GetService<ISetStatusService>().SetBillStatus(
                            context,
                            businessInfo,
                            pkIds,
                            paraUnAudit,
                            "UnAudit",
                            null
                            );
        }

        /// <summary>
        /// 审核单据
        /// </summary>
        /// <param name="billView"></param>
        /// <param name="pkIds"></param>
        private void AuditBill(BusinessInfo businessInfo,
            List<KeyValuePair<object, object>> pkIds)
        {
            List<object> idList = new List<object>();
            foreach (var item in pkIds)
            {
                idList.Add(Convert.ToInt64(item.Key));
            }
            //提交单据
            IOperationResult submitResult =
                Kingdee.BOS.App.
                ServiceHelper.GetService<ISubmitService>().Submit(
                    context, businessInfo, idList.ToArray(), "Submit");

            //审核单据
            List<object> paraAudit = new List<object>();
            //1审核通过
            paraAudit.Add("1");
            //审核意见
            paraAudit.Add("");
            IOperationResult auditResult
                = Kingdee.BOS.App.ServiceHelper.GetService<ISetStatusService>().SetBillStatus(
                context, businessInfo, pkIds, paraAudit, "Audit", OperateOption.Create());
        }
    }
}
