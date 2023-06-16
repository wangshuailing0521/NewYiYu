using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSL.YY.K3.FIN.PlugIn.Helper;

namespace WSL.YY.K3.FIN.PlugIn.Model
{
    public class WmsInStock
    {
        /// <summary>
        /// 是否自动收货  默认N
        /// </summary>
        public string isautoReceiving { get; set; }

        /// <summary>
        /// 是否自动调度  默认N
        /// </summary>
        public string isautoDispatch { get; set; }

        /// <summary>
        /// 是否码盘  默认N
        /// </summary>
        public string isPalletized { get; set; }

        public bool AllowDuplicate { get; set; }

        public string whgid { get; set; }

        public string defPointId { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        public string transactionId { get; set; }

        public string lg { get; set; }

        /// <summary>
        /// 登录标识
        /// </summary>
        public string token { get; set; }

        public receiptEditDTO receiptEditDTO { get; set; }

        
    }

    public class receiptEditDTO {
        /// <summary>
        /// WMS仓库代码
        /// </summary>
        public string WH_ID { get; set; }

        //public string whgid { get; set; }

        /// <summary>
        /// 货主代码
        /// </summary>
        public string OWNER_ID { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public string RECEIPT_TYPE_SC { get; set; }

        /// <summary>
        /// 来源系统
        /// </summary>
        public string RECEIPT_FROM_SC = "接口创建";

        /// <summary>
        /// 来源系统
        /// </summary>
        public string RECEIPT_ORDER_SOURCE_SC = "ERP";

        /// <summary>
        /// 退货原因
        /// </summary>
        public string RETURN_REASON_SC { get; set; }

        /// <summary>
        /// 收料通知单号
        /// </summary>
        public string EXTERNAL_RECEIPT_ID { get; set; }

        /// <summary>
        /// 通知单ID
        /// </summary>
        public string EXTERNAL_RECEIPT_ID2 { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CREATED_BY = "demo";


        /// <summary>
        /// 审核时间
        /// </summary>
        [JsonConverter(typeof(LongDateTimeConvert))]
        public DateTime CREATED_DATE { get; set; }


        /// <summary>
        /// 收料组织ID
        /// </summary>
        public string R_UDF1 { get; set; }

        /// <summary>
        /// 采购部门ID
        /// </summary>
        public string R_UDF2 { get; set; }

        /// <summary>
        /// 采购员ID
        /// </summary>
        public string R_UDF3 { get; set; }

        /// <summary>
        /// 申请人ID
        /// </summary>
        public string R_UDF4 { get; set; }

        /// <summary>
        /// 币别
        /// </summary>
        public string R_UDF5 { get; set; }

        /// <summary>
        /// 汇率
        /// </summary>
        public string R_UDF6 { get; set; }

        /// <summary>
        /// 汇率类型
        /// </summary>
        public string R_UDF7 { get; set; }

        /// <summary>
        /// 供应商ID
        /// </summary>
        public string VENDOR_ID { get; set; }

        /// <summary>
        /// 预计到货日期
        /// </summary>
        [JsonConverter(typeof(LongDateTimeConvert))]
        public DateTime EXPECTED_ARRIVAL_DATE { get; set; }

        public List<ReceiptDetailList> ReceiptDetailList { get; set; }
    }

    public class ReceiptDetailList
    {
        /// <summary>
        /// 明细行ID
        /// </summary>
        public string EXTERNAL_LINE_ID { get; set; }

        /// <summary>
        /// 货品代码
        /// </summary>
        public string SKU_ID { get; set; }

        /// <summary>
        /// 包装代码
        /// </summary>
        public string PACK_ID { get; set; }

        /// <summary>
        /// 单位代码
        /// </summary>
        public string UOM_ID { get; set; }

        /// <summary>
        /// 预期量
        /// </summary>
        public decimal EXPECTED_QTY { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string EXTERNAL_LOT { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string RD_REMARK { get; set; }

        /// <summary>
        /// 明细采购订单号
        /// </summary>
        public string EXTERNAL_RECEIPT_ID { get; set; }

        /// <summary>
        /// 国别
        /// </summary>
        public string LOT_ATTR01 { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        public string LOT_ATTR02 { get; set; }

        /// <summary>
        /// 原材料版本号
        /// </summary>
        public string LOT_ATTR03 { get; set; }

        /// <summary>
        /// 产成品v1
        /// </summary>
        public string LOT_ATTR04 { get; set; }

        /// <summary>
        /// 产成品v2
        /// </summary>
        public string LOT_ATTR05 { get; set; }

        /// <summary>
        /// 产成品v3
        /// </summary>
        public string LOT_ATTR06 { get; set; }

        /// <summary>
        /// box no
        /// </summary>
        public string LOT_ATTR07 { get; set; }

        /// <summary>
        /// 是否样品
        /// </summary>
        public string LOT_ATTR08 { get; set; }

        /// <summary>
        /// 良品率
        /// </summary>
        public string LOT_ATTR09 { get; set; }

        /// <summary>
        /// 货品属性
        /// </summary>
        public string SKU_PROPERTY { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public string PRODUCE_DATE { get; set; }

        /// <summary>
        /// 失效日期
        /// </summary>
        public string EXPIRE_DATE { get; set; }

        /// <summary>
        /// 税率
        /// </summary>
        public string RD_UDF1 { get; set; }

        /// <summary>
        /// 含税单价
        /// </summary>
        public string RD_UDF2 { get; set; }

        /// <summary>
        /// 计价单位
        /// </summary>
        public string RD_UDF3 { get; set; }

    }

}
