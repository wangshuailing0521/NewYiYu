using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;

namespace WSL.YY.K3.FIN.PlugIn.Helper
{
    public class DBLog
    {
        public Context Context;
        /// <summary>
        /// 调用方（kingdee or db）
        /// </summary>
        public string FInvocation { get; set; }
        /// <summary>
        /// 接口类型
        /// </summary>
        public string FInterfaceType { get; set; }
        /// <summary>
        /// 操作类型（add or edit）
        /// </summary>
        public string FOperationType { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string FBeginTime = DateTime.Now.ToString();
        /// <summary>
        /// 结束时间
        /// </summary>
        public string FEndTime { get; set; }
        /// <summary>
        /// 请求报文
        /// </summary>
        public string FRequestMessage { get; set; }
        /// <summary>
        /// 响应报文
        /// </summary>
        public string FResponseMessage { get; set; }
        /// <summary>
        /// 状态 S:成功 E:失败
        /// </summary>
        public string FStatus { get; set; }
        /// <summary>
        /// 接口调用信息
        /// </summary>
        public string FMessage { get; set; }
        /// <summary>
        /// 堆栈报错信息
        /// </summary>
        public string FStackMessage { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string FBillNo { get; set; }

        public void Insert()
        {
            if (FStackMessage == null)
            {
                FStackMessage = "";
            }
            if (FMessage == null)
            {
                FMessage = "";
            }

            if (FRequestMessage == null)
            {
                FRequestMessage = "";
            }

            if (string.IsNullOrEmpty(FBillNo))
            {
                FBillNo = "";
            }

            string sql = $@"/*dialect*/
                            INSERT INTO SCS_T_InterfaceLog
                            (FInvocation,FInterfaceType,FOperationType,FBeginTime,
                            FEndTime,FRequestMessage,FResponseMessage,FStatus,
                            FMessage,FStackMessage,FBillNo)
                            SELECT 
                            '{FInvocation}','{FInterfaceType}','{FOperationType}','{FBeginTime}',
                            '{FEndTime}','{FRequestMessage.Replace("'", "''")}','{FResponseMessage.Replace("'", "''")}','{FStatus}',
                            '{FMessage.Replace("'", "''")}','{FStackMessage.Replace("'", "''")}',
                            '{FBillNo}'";
            DBUtils.Execute(Context, sql);
        }
    }
}
