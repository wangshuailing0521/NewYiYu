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
    public class WmsOutStock
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

        public shippingorderEditDTO shippingorderEditDTO { get; set; }


    }

    public class shippingorderEditDTO
    {
        /// <summary>
        /// WMS仓库代码
        /// </summary>
        public string WH_ID { get; set; }

        /// <summary>
        /// 上海仓        Europe
        /// </summary>
        public string DEST_WH_ID { get; set; }

        /// <summary>
        ///  "medtrum.wh1"or"medtrum.wh2"
        /// </summary>
        public string DEST_WH_GID { get; set; }

        //public string whgid { get; set; }

        /// <summary>
        /// 货主代码
        /// </summary>
        public string OWNER_ID { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public string ORDER_TYPE_SC { get; set; }

        /// <summary>
        /// 来源系统
        /// </summary>
        public string SHIPPING_ORDER_SOURCE_SC { get; set; }

        /// <summary>
        /// 【出库订单号】,字符串类型,最大长度30
        /// </summary>
        public string EXTERNAL_ORDER_ID { get; set; }

        /// <summary>
        /// 通知单ID
        /// </summary>
        public string EXTERNAL_ORDER_ID2 { get; set; }

        /// <summary>
        /// 通知单ID
        /// </summary>
        public string SO_ID { get; set; }

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
        /// 更新时间
        /// </summary>
        [JsonConverter(typeof(LongDateTimeConvert))]
        public DateTime UPDATED_DATE { get; set; }


        /// <summary>
        /// 销售组织ID
        /// </summary>
        public string SO_UDF1 { get; set; }

        /// <summary>
        /// 运单号
        /// </summary>
        public string WAYBILL_NO { get; set; }



        /// <summary>
        /// 客户ID
        /// </summary>
        public string CUSTOMER_ID { get; set; }

        /// <summary>
        /// 病人ID
        /// </summary>
        public string CUSTOMER_REF { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CUSTOMER_NAME { get; set; }

        public string CUSTOMER_PHONE { get; set; }

        /// <summary>
        /// 实际收货人
        /// </summary>
        public string DEST_CONTACT_NAME { get; set; }

        /// <summary>
        /// 所有地址信息
        /// </summary>
        public string DEST_ADDRESS { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string DEST_CONTACT_PHONE { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string CUSTOMER_EMAIL { get; set; }

        /// <summary>
        /// 结算币别ID
        /// </summary>
        public string SO_UDF2 { get; set; }

        /// <summary>
        /// 汇率类型
        /// </summary>
        public string SO_UDF3 { get; set; }

        /// <summary>
        /// 汇率类型ID
        /// </summary>
        public string SO_UDF4 { get; set; }

        /// <summary>
        /// 结算方式ID
        /// </summary>
        public string SO_UDF5 { get; set; }

        /// <summary>
        /// 收款条件ID
        /// </summary>
        public string SO_UDF6 { get; set; }

        /// <summary>
        /// 结算币别
        /// </summary>
        public string SO_UDF7 { get; set; }

        /// <summary>
        /// 销售部门ID
        /// </summary>
        public string SO_UDF9 { get; set; }

        /// <summary>
        /// 销售员ID
        /// </summary>
        public string SO_UDF10 { get; set; }

        /// <summary>
        /// FDealId
        /// </summary>
        public string SO_UDF11 { get; set; }

        /// <summary>
        /// FSaleOwnerId
        /// </summary>
        public string SO_UDF12 { get; set; }

        /// <summary>
        /// FContactName
        /// </summary>
        public string SO_UDF13 { get; set; }

        /// <summary>
        /// FContactId
        /// </summary>
        public string SO_UDF14 { get; set; }

        /// <summary>
        /// FZohoContact
        /// </summary>
        public string SO_UDF15 { get; set; }

        /// <summary>
        /// FZohoShipmentNo
        /// </summary>
        public string SO_UDF16 { get; set; }

        /// <summary>
        /// 汇率
        /// </summary>
        public string SO_UDF17 { get; set; }

        public string SO_UDF20 { get; set; }

        public string SO_UDF21 { get; set; }

        public string SO_UDF22 { get; set; }

        public string SO_UDF26 { get; set; }

        public string SO_UDF23 { get; set; }

        public string SO_UDF24 { get; set; }

        public string SO_UDF25 { get; set; }

        public string SO_UDF27 { get; set; }

       

        public string SO_UDF29 { get; set; }

        /// <summary>
        /// 运输方式ID
        /// </summary>
        public string TRANSPORT_MODE_ID { get; set; }

        public string REQUEST_SHIP_DATE { get; set; }


        public List<ShippingOrderDetailList> ShippingOrderDetailList { get; set; }
    }

    public class ShippingOrderDetailList
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
        /// 包装单位
        /// </summary>
        public string UOM_ID { get; set; }

        /// <summary>
        /// 发货数量
        /// </summary>
        public decimal ORDER_QTY { get; set; }

        /// <summary>
        /// 批号
        /// </summary>
        public string EXTERNAL_LOT { get; set; }

        /// <summary>
        /// 欧洲套装编号
        /// </summary>
        public string OL_UDF1 { get; set; }

        /// <summary>
        /// 欧洲套装数量
        /// </summary>
        public string OL_UDF2 { get; set; }

        /// <summary>
        /// 欧洲套装价格
        /// </summary>
        public string OL_UDF3 { get; set; }

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
        public string SOD_UDF1 { get; set; }

        /// <summary>
        /// 含税单价 
        /// </summary>
        public string SOD_UDF2 { get; set; }

        /// <summary>
        /// 计价单位
        /// </summary>
        public string SOD_UDF4 { get; set; }

        public string SOD_UDF5 { get; set; }

        public string SOD_UDF6 { get; set; }

        public string SOD_UDF7 { get; set; }

        public string SO_UDF28 { get; set; }
    }
}
