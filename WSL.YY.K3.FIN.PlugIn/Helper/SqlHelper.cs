using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSL.YY.K3.FIN.PlugIn.Model;

namespace WSL.YY.K3.FIN.PlugIn.Helper
{
    public static class SqlHelper
    {
        /// <summary>
        /// 根据结算方式名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSettleTypeNumber(Context context,string name)
        {

            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }
            string sql = $@"
                SELECT A.FNumber
                FROM   T_BD_SETTLETYPE A
                       INNER JOIN T_BD_SETTLETYPE_L B 
                       ON A.FID=B.FID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  B.FNAME = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的结算方式不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据组织机构获取WMS仓库信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static WMSStock GetWmsStockNumber(Context context, string number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return null;
            }
            string sql = $@"
                SELECT A.F_ORA_TEXT,A. F_ORA_TEXT1
                FROM   T_ORG_ORGANIZATIONS A
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FNUMBER = '{number}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"编码为【{number}】的组织机构不存在！");
            }
            WMSStock wmsStock = new WMSStock
            {
                Number = data[0]["F_ORA_TEXT"].ToString(),
                Name = data[0]["F_ORA_TEXT1"].ToString()
            };


            return wmsStock;
        }

        /// <summary>
        /// 根据收款条件名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetRecConditionNumber(Context context, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }
            string sql = $@"
                SELECT A.FNumber
                FROM   T_BD_RecCondition A
                       INNER JOIN T_BD_RecCondition_L B 
                       ON A.FID=B.FID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  B.FNAME = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的收款条件不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据组织名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetOrgNumber(Context context, string name)
        {
            name = name.Replace("'", "''");

            string sql = $@"
                SELECT A.FNumber
                FROM   T_ORG_ORGANIZATIONS A
                       INNER JOIN T_ORG_ORGANIZATIONS_L B 
                       ON A.FORGID=B.FORGID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  B.FNAME = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的组织不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据组织名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetOrgNumberByCustName(Context context, string name)
        {
            name = name.Replace("'", "''");
            string sql = $@"
                SELECT A.FNumber
                FROM   T_ORG_ORGANIZATIONS A
                       INNER JOIN T_ORG_ORGANIZATIONS_L B 
                       ON A.FORGID=B.FORGID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  B.FNAME = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据组织名称获取内码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetOrgId(Context context, string name)
        {
            name = name.Replace("'", "''");
            string sql = $@"
                SELECT A.FORGID
                FROM   T_ORG_ORGANIZATIONS A
                       INNER JOIN T_ORG_ORGANIZATIONS_L B 
                       ON A.FORGID=B.FORGID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  B.FNAME = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的组织不存在！");
            }

            return data[0]["FORGID"].ToString();
        }

        /// <summary>
        /// 根据国家名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetCountryNumber(Context context, string name)
        {
            name = name.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FNumber
                FROM   T_BAS_ASSISTANTDATAENTRY A
                       INNER JOIN T_BAS_ASSISTANTDATAENTRY_L B 
                       ON A.FENTRYID=B.FENTRYID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FID='8a6e30f0-2c26-4639-aff5-76749daa355e'
                  AND  B.FDATAVALUE = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的国家不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据国家名称获取wmsName
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetCountryWMSName(Context context, string name)
        {
            name = name.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string sql = $@"
                SELECT A.F_ora_Text
                FROM   T_BAS_ASSISTANTDATAENTRY A
                       INNER JOIN T_BAS_ASSISTANTDATAENTRY_L B 
                       ON A.FENTRYID=B.FENTRYID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FID='8a6e30f0-2c26-4639-aff5-76749daa355e'
                  AND  B.FDATAVALUE = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的国家不存在！");
            }

            return data[0]["F_ora_Text"].ToString();
        }

        /// <summary>
        /// 根据地区名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetRegionNumber(Context context, string name)
        {
            name = name.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FNumber
                FROM   T_BAS_ASSISTANTDATAENTRY A
                       INNER JOIN T_BAS_ASSISTANTDATAENTRY_L B 
                       ON A.FENTRYID=B.FENTRYID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FID='59ace44c3f1c96'
                  AND  B.FDATAVALUE = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的地区不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据省份名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetStateNumber(Context context, string name)
        {
            name = name.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FNumber
                FROM   T_BAS_ASSISTANTDATAENTRY A
                       INNER JOIN T_BAS_ASSISTANTDATAENTRY_L B 
                       ON A.FENTRYID=B.FENTRYID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FID='59ace44c3f1c96'
                  AND  B.FDATAVALUE = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的省份不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据城市名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetCityNumber(Context context, string name)
        {
            name = name.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FNumber
                FROM   T_BAS_ASSISTANTDATAENTRY A
                       INNER JOIN T_BAS_ASSISTANTDATAENTRY_L B 
                       ON A.FENTRYID=B.FENTRYID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FID='2465ece5-86b0-4b5b-bf39-133c8b34d1c5'
                  AND  B.FDATAVALUE = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的城市不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据区县名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetStreetNumber(Context context, string name)
        {

            name = name.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FNumber
                FROM   T_BAS_ASSISTANTDATAENTRY A
                       INNER JOIN T_BAS_ASSISTANTDATAENTRY_L B 
                       ON A.FENTRYID=B.FENTRYID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FID='8a6e30f0-2c26-4639-aff5-76749daa355e'
                  AND  B.FDATAVALUE = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的区县不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据Zoho客户表示获取客户内码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static long GetCustIdByZoho(Context context, string accountId)
        {
            
            string sql = $@"
                SELECT A.FCUSTID
                FROM   T_BD_CUSTOMER A
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FZohoId = '{accountId}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"CRM标识为【{accountId}】的客户不存在！");
            }

            return Convert.ToInt64(data[0]["FCUSTID"].ToString());
        }

        /// <summary>
        /// 根据国家名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static K3Contact GetContactById(Context context, long contactId)
        {
            string sql = $@"
                SELECT A.*,B.FNAME
                FROM   T_BD_COMMONCONTACT A
                       INNER JOIN T_BD_COMMONCONTACT_L B 
                       ON A.FCONTACTID=B.FCONTACTID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FCONTACTID = '{contactId}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"内码为【{contactId}】的联系人不存在！");
            }

            K3Contact k3Contact = new K3Contact()
            {
                Id = contactId,
                Number = data[0]["FNumber"].ToString(),
                Name = data[0]["FName"].ToString(),
                BillAddressNumber = data[0]["FBizLocNumber"].ToString(),
                BillAddressName = data[0]["FBizLocation"].ToString(),
                BillAddressDetail = data[0]["FBizAddress"].ToString(),
                Mobile = data[0]["FMobile"].ToString(),
                Tel = data[0]["FTel"].ToString(),
                Email = data[0]["FEmail"].ToString(),
            };

            return k3Contact;
        }

        /// <summary>
        /// 根据物料名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetMaterialNumber(Context context, string name)
        {
            name = name.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FNumber
                FROM   T_BD_MATERIAL A
                       INNER JOIN T_BD_MATERIAL_L B 
                       ON A.FMATERIALID=B.FMATERIALID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FFORBIDSTATUS='A'
                  AND  B.FNAME = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的物料不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据币别名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetCurrencyNumber(Context context, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FNumber
                FROM   T_BD_CURRENCY A
                       INNER JOIN T_BD_CURRENCY_L B 
                       ON A.FCURRENCYID=B.FCURRENCYID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FFORBIDSTATUS='A'
                  AND B.FLOCALEID=1033
                  AND  B.FNAME = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的币别不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据币别名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetCurrencyEngName(Context context, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return "";
            }

            string sql = $@"
                SELECT B.FNAME
                FROM   T_BD_CURRENCY A
                       INNER JOIN T_BD_CURRENCY_L B 
                       ON A.FCURRENCYID=B.FCURRENCYID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FFORBIDSTATUS='A'
                  AND  B.FLOCALEID=1033
                  AND  A.FCURRENCYID = '{id}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FNAME"].ToString();
        }

        /// <summary>
        /// 根据zohoId获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetCustNumber(Context context, string zohoId)
        {
            if (string.IsNullOrWhiteSpace(zohoId))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FNumber
                FROM   T_BD_CUSTOMER A
                       INNER JOIN T_BD_CUSTOMER_L B 
                       ON A.FCUSTID=B.FCUSTID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FFORBIDSTATUS='A'
                  AND  A.FZohoId = '{zohoId}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"zohoId为【{zohoId}】的客户不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据zohoId获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetContactNumber(Context context, string zohoId)
        {
            if (string.IsNullOrWhiteSpace(zohoId))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FNumber
                FROM   T_BD_COMMONCONTACT A
                WHERE  1=1
                  AND  A.FZohoId = '{zohoId}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"zohoId为【{zohoId}】的联系人不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 判断是否存在对应客户
        /// </summary>
        /// <param name="context"></param>
        /// <param name="zohoId"></param>
        /// <returns></returns>
        public static string IsHaveCustId(Context context, string zohoId)
        {
            if (string.IsNullOrWhiteSpace(zohoId))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FCustId
                FROM   T_BD_CUSTOMER A
                       INNER JOIN T_BD_CUSTOMER_L B 
                       ON A.FCUSTID=B.FCUSTID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FFORBIDSTATUS='A'
                  AND  A.FZohoId = '{zohoId}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FCustId"].ToString();
        }

        /// <summary>
        /// 判断是否存在对应联系人
        /// </summary>
        /// <param name="context"></param>
        /// <param name="zohoId"></param>
        /// <returns></returns>
        public static string IsHaveContactId(Context context, string zohoId)
        {
            if (string.IsNullOrWhiteSpace(zohoId))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FContactId
                FROM   T_BD_COMMONCONTACT A
                WHERE  1=1
                  AND  A.FZohoId = '{zohoId}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FContactId"].ToString();
        }

        /// <summary>
        /// 根据销售员名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSalerNumber(Context context, string name,string orgName)
        {
            name = name.Replace("'", "''");
            orgName = orgName.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FNumber
                FROM   V_BD_SALESMAN A
                       INNER JOIN V_BD_SALESMAN_L B 
                       ON A.FID=B.FID
                       INNER JOIN T_ORG_ORGANIZATIONS_L C
					   ON C.FNAME = '{orgName}' AND A.FBIZORGID = C.FORGID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FFORBIDSTATUS='A'
                  AND  B.FNAME = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的销售员不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据销售员名称获取员工人刚明细
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSalerRGNumber(Context context, string name,string dept,string org)
        {
            name = name.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string sql = $@"
                SELECT  A.FSTAFFNUMBER FNumber
                  FROM  T_BD_STAFF A
		                INNER JOIN T_HR_EMPINFO B ON A.FEMPINFOID = B.FID
		                INNER JOIN T_HR_EMPINFO_L C ON C.FID = B.FID AND C.FLOCALEID = 2052
		                INNER JOIN T_BD_DEPARTMENT_L D ON D.FDEPTID = A.FDEPTID AND D.FLOCALEID = 2052
		                INNER JOIN T_ORG_ORGANIZATIONS_L E ON E.FORGID = A.FUSEORGID AND E.FLOCALEID = 2052
                 WHERE  1=1
                   AND  C.FNAME = '{name}'
                   AND  D.FNAME = '{dept}'
                   AND  E.FNAME = '{org}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的销售员不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据部门名称获取编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetDeptNumber(Context context, string name)
        {
            name = name.Replace("'", "''");
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FNumber
                FROM   T_BD_DEPARTMENT A
                       INNER JOIN T_BD_DEPARTMENT_L B 
                       ON A.FDEPTID=B.FDEPTID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FFORBIDSTATUS='A'
                  AND  B.FNAME = '{name}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                throw new Exception($@"名称为【{name}】的部门不存在！");
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据销售订单号获取出库单号
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<ShipmentBill> GetOutStockNumber(Context context, string saleNo)
        {
            List<ShipmentBill> shipments = new List<ShipmentBill>();

            if (string.IsNullOrWhiteSpace(saleNo))
            {
                return shipments;
            }

            string sql = $@"
                SELECT A.FZohoShipmentNo
                  FROM T_SAL_OUTSTOCK A
                       INNER JOIN T_SAL_OUTSTOCKENTRY B 
                       ON A.FID=B.FID
                       INNER JOIN T_SAL_OUTSTOCKENTRY_LK C
                       ON B.FENTRYID=C.FENTRYID
                       INNER JOIN T_SAL_ORDERENTRY D
                       ON C.FSID=D.FENTRYID AND C.FSBILLID=D.FID
                       INNER JOIN T_SAL_ORDER E
                       ON D.FID=E.FID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  E.FBILLNO = '{saleNo}'
                GROUP BY A.FZohoShipmentNo";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return shipments;
            }

            foreach (var item in data)
            {
                ShipmentBill shipment = new ShipmentBill();
                shipment.Shipment = item["FZohoShipmentNo"].ToString();
                shipments.Add(shipment);
            }

            return shipments;
        }

        /// <summary>
        /// 根据客户内码获取客户ZohoId
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetCustZohoId(Context context, string custId)
        {
            if (string.IsNullOrWhiteSpace(custId))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FZohoId
                FROM   T_BD_CUSTOMER A
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FCUSTID = '{custId}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FZohoId"].ToString();
        }

        /// <summary>
        /// 根据销售单号获取联系人ZohoId
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetContactZohoIdBySaleNo(Context context, string saleNo)
        {
            if (string.IsNullOrWhiteSpace(saleNo))
            {
                return "";
            }

            string sql = $@"
                SELECT B.FZohoId
                FROM   T_SAL_ORDER A
                INNER JOIN T_BD_COMMONCONTACT B ON A.FZohoContact=B.FCONTACTID
                WHERE  1=1
                  AND  A.FBILLNO = '{saleNo}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FZohoId"].ToString();
        }

        /// <summary>
        /// 根据销售单号获取联系人名称
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetContactZohoNameBySaleNo(Context context, string saleNo)
        {
            if (string.IsNullOrWhiteSpace(saleNo))
            {
                return "";
            }

            string sql = $@"
                SELECT B.FName
                FROM   T_SAL_ORDER A
                INNER JOIN T_BD_COMMONCONTACT_L B ON A.FZohoContact=B.FCONTACTID
                WHERE  1=1
                  AND  A.FBILLNO = '{saleNo}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FName"].ToString();
        }

        /// <summary>
        /// 根据销售单号获取ZohoBillNo
        /// </summary>
        /// <param name="context"></param>
        /// <param name="saleNo"></param>
        /// <returns></returns>
        public static string GetSaleZohoBillNo(Context context, string saleNo)
        {
            if (string.IsNullOrWhiteSpace(saleNo))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FZohoBillNo
                FROM   T_SAL_ORDER A
                WHERE  1=1
                  AND  A.FBILLNO = '{saleNo}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FZohoBillNo"].ToString();
        }

        /// <summary>
        /// 根据销售单号获取ZohoInfo
        /// </summary>
        /// <param name="context"></param>
        /// <param name="saleNo"></param>
        /// <returns></returns>
        public static DynamicObjectCollection GetSaleZohoInfo(Context context, string saleNo)
        {
            if (string.IsNullOrWhiteSpace(saleNo))
            {
                return null;
            }

            string sql = $@"
                SELECT A.FDealId,A.FZohoBillNo,A.FSaleOwnerId
                FROM   T_SAL_ORDER A
                WHERE  1=1
                  AND  A.FBILLNO = '{saleNo}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            return data;
        }

        /// <summary>
        /// 根据收款单内码获取销售订单号码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="receiveId"></param>
        /// <returns></returns>
        public static List<string> GetSaleNoByRec(Context context, string receiveId)
        {
            List<string> billNos = new List<string>();

            string sql = $@"
                SELECT  I.FBILLNO
                  FROM  T_AR_RECEIVEBILLSRCENTRY A WITH(NOLOCK)
                        INNER JOIN T_AR_RECEIVEBILLSRCENTRY_LK B  WITH(NOLOCK)
                        ON A.FENTRYID=B.FENTRYID 
                        INNER JOIN t_AR_receivablePlan C  WITH(NOLOCK)
                        ON B.FSID=C.FENTRYID AND B.FSBILLID=C.FID
                        INNER JOIN T_AR_RECEIVABLEENTRY D WITH(NOLOCK)
                        ON D.FID=C.FID
                        INNER JOIN T_AR_RECEIVABLEENTRY_LK E WITH(NOLOCK)
                        ON D.FENTRYID=E.FENTRYID
                        INNER JOIN T_SAL_OUTSTOCKENTRY F WITH(NOLOCK)
                        ON E.FSID=F.FENTRYID AND E.FSBILLID=F.FID
                        INNER JOIN T_SAL_OUTSTOCKENTRY_LK G WITH(NOLOCK)
                        ON F.FENTRYID=G.FENTRYID
                        INNER JOIN T_SAL_ORDERENTRY H WITH(NOLOCK)
                        ON G.FSID=H.FENTRYID AND G.FSBILLID=H.FID
                        INNER JOIN T_SAL_ORDER I WITH(NOLOCK)
                        ON H.FID=I.FID
                 WHERE  A.FID='{receiveId}'
                 GROUP  BY I.FBILLNO
                ";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);
           
            if (data.Count>0)
            {
                billNos = data.Select(x => x["FBILLNO"].ToString()).ToList();
            }

            return billNos;
        }

        /// <summary>
        /// 根据销售单号获取已核销金额
        /// </summary>
        /// <param name="context"></param>
        /// <param name="saleNo"></param>
        /// <returns></returns>
        public static DynamicObjectCollection GetAmountBySaleNo(Context context, string saleNo)
        {
            string sql = $@"
                SELECT SUM(B.FRECADVANCEAMOUNT)FRECADVANCEAMOUNT,
                       SUM(B.FRECAMOUNT)FRECAMOUNT,
                       D.FNAME
                FROM T_SAL_ORDER A
                INNER JOIN T_SAL_ORDERPLAN B 
                ON A.FID=B.FID
                INNER JOIN T_SAL_ORDERFIN C
                ON C.FID=A.FID
                INNER JOIN T_BD_RecCondition_L D
                ON D.FID=C.FRECCONDITIONID AND D.FLOCALEID=2052
                WHERE 1=1
                AND A.FBILLNO='{saleNo}'
                GROUP BY D.FNAME
                ";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            return data;
        }

        /// <summary>
        /// 根据应收单内码获取销售订单号码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="receiveId"></param>
        /// <returns></returns>
        public static List<string> GetSaleNoByAble(Context context, string ableId)
        {
            List<string> billNos = new List<string>();

            string sql = $@"
                SELECT  I.FBILLNO
                  FROM  T_AR_RECEIVABLEENTRY D WITH(NOLOCK)
                        INNER JOIN T_AR_RECEIVABLEENTRY_LK E WITH(NOLOCK)
                        ON D.FENTRYID=E.FENTRYID
                        INNER JOIN T_SAL_OUTSTOCKENTRY F WITH(NOLOCK)
                        ON E.FSID=F.FENTRYID AND E.FSBILLID=F.FID
                        INNER JOIN T_SAL_OUTSTOCKENTRY_LK G WITH(NOLOCK)
                        ON F.FENTRYID=G.FENTRYID
                        INNER JOIN T_SAL_ORDERENTRY H WITH(NOLOCK)
                        ON G.FSID=H.FENTRYID AND G.FSBILLID=H.FID
                        INNER JOIN T_SAL_ORDER I WITH(NOLOCK)
                        ON H.FID=I.FID
                 WHERE  D.FID='{ableId}'
                 GROUP  BY I.FBILLNO
                ";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count > 0)
            {
                billNos = data.Select(x => x["FBILLNO"].ToString()).ToList();
            }

            return billNos;
        }

        /// <summary>
        /// 根据应收单内码获取已核销金额
        /// </summary>
        /// <param name="context"></param>
        /// <param name="saleNo"></param>
        /// <returns></returns>
        public static DynamicObjectCollection GetAmountByAbleId(Context context, string ableId)
        {
            string sql = $@"
                SELECT SUM(B.FWRITTENOFFAMOUNTFOR)FWRITTENOFFAMOUNTFOR
                FROM T_AR_RECEIVABLE A
                INNER JOIN t_AR_receivablePlan B ON A.FID=B.FID
                WHERE 1=1
                AND A.FID='{ableId}'
                ";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            return data;
        }

        /// <summary>
        /// 根据收款单内码获取应收单编号
        /// </summary>
        /// <param name="context"></param>
        /// <param name="receiveId"></param>
        /// <returns></returns>
        public static DynamicObjectCollection GetAbleNo(Context context,string receiveId)
        {
            string sql = $@"
                SELECT  DISTINCT D.FBILLNO,D.FENDDATE,E.FName
                        ,SUM(C.FPAYAMOUNTFOR)FPAYAMOUNTFOR
                        ,SUM(C.FWRITTENOFFAMOUNTFOR)FWRITTENOFFAMOUNTFOR
                  FROM  T_AR_RECEIVEBILLSRCENTRY A WITH(NOLOCK)
                        INNER JOIN T_AR_RECEIVEBILLSRCENTRY_LK B  WITH(NOLOCK)
                        ON A.FENTRYID=B.FENTRYID 
                        INNER JOIN t_AR_receivablePlan C  WITH(NOLOCK)
                        ON B.FSID=C.FENTRYID AND B.FSBILLID=C.FID
                        INNER JOIN T_AR_RECEIVABLE D WITH(NOLOCK)
                        ON D.FID=C.FID
                        INNER JOIN T_BD_RecCondition_L E
                        ON E.FID = D.FPayConditon AND E.FLOCALEID = 2052
                 WHERE  A.FID='{receiveId}'
                 GROUP  BY D.FBILLNO,D.FENDDATE,E.FName
                ";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            return data;
        }


        public static void UpdateContact(Context context, string contactId,string zohoId,string type,string birthday)
        {
            string sql = $@"
                UPDATE T_BD_COMMONCONTACT
                SET FZOHOID='{zohoId}',
                    FType='{type}',
                    FBirthday = '{birthday}'
                WHERE FCONTACTID='{contactId}'";
            DBUtils.Execute(context, sql);
            
        }

        public static void UpdateOutStock(Context context, string outStockId, string zohoShipmentNo)
        {
            string sql = $@"
                UPDATE T_SAL_OUTSTOCK
                SET FZohoShipmentNo='{zohoShipmentNo}'
                WHERE FID='{outStockId}'";
            DBUtils.Execute(context, sql);

        }

        public static void ClearOutStock(Context context, string outStockId)
        {
            string sql = $@"
                UPDATE T_SAL_OUTSTOCK
                SET FZohoShipmentNo=''
                WHERE FID='{outStockId}'";
            DBUtils.Execute(context, sql);

        }

        //T_STK_MISDELIVERY
        public static void UpdateOtherOutStock(Context context, string outStockId, string zohoShipmentNo)
        {
            string sql = $@"
                UPDATE T_STK_MISDELIVERY
                SET FZohoShipmentNo='{zohoShipmentNo}'
                WHERE FID='{outStockId}'";
            DBUtils.Execute(context, sql);

        }

        public static void ClearOtherOutStock(Context context, string outStockId)
        {
            string sql = $@"
                UPDATE T_STK_MISDELIVERY
                SET FZohoShipmentNo=''
                WHERE FID='{outStockId}'";
            DBUtils.Execute(context, sql);

        }

        /// <summary>
        /// 判断是否存在对应联系人
        /// </summary>
        /// <param name="context"></param>
        /// <param name="zohoId"></param>
        /// <returns></returns>
        public static string IsPaitner(Context context, string zohoId)
        {
            if (string.IsNullOrWhiteSpace(zohoId))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FContactId
                FROM   T_BD_COMMONCONTACT A
                WHERE  1=1
                  AND  A.FType='Patient'
                  AND  A.FZohoId = '{zohoId}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FContactId"].ToString();
        }

        /// <summary>
        /// 获取客户内码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="zohoId"></param>
        /// <returns></returns>
        public static List<string> GetCustIdByNumber(Context context, string number,string orgIds = "1")
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return new List<string>();
            }

            string sql = $@"
                SELECT A.FCustId Id
                  FROM T_BD_CUSTOMER A
                WHERE  1=1
                  AND  A.FUseOrgId IN ({orgIds})
                  AND  A.FNumber = '{number}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return new List<string>();
            }

            return data.Select(x => x["Id"].ToString()).ToList();
        }

        /// <summary>
        /// 获取客户内码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="zohoId"></param>
        /// <returns></returns>
        public static List<string> GetAllCustIdByNumber(Context context, string number, string orgIds = "1")
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return new List<string>();
            }

            string sql = $@"
                SELECT A.FCustId Id
                  FROM T_BD_CUSTOMER A
                WHERE  1=1
                  AND  A.FDocumentStatus = 'C'
                  AND  A.FNumber = '{number}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return new List<string>();
            }

            return data.Select(x => x["Id"].ToString()).ToList();
        }

        /// <summary>
        /// 获取客户内码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="zohoId"></param>
        /// <returns></returns>
        public static string GetCustCreateIdByNumber(Context context, string number, string orgIds = "1")
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FCustId Id
                  FROM T_BD_CUSTOMER A
                WHERE  1=1
                  AND  A.FUseOrgId IN ({orgIds})
                  AND  A.FNumber = '{number}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["Id"].ToString();
        }

        /// <summary>
        /// 获取未分配组织内码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="zohoId"></param>
        /// <returns></returns>
        public static List<string> GetOrgIdByCustNumber(
            Context context, 
            string custNumber, 
            List<string> orgIdList)
        {
            if (string.IsNullOrWhiteSpace(custNumber))
            {
                return new List<string>();
            }

            string sql = $@"
                SELECT A.FUseOrgId Id
                  FROM T_BD_CUSTOMER A
                WHERE  1=1
                  AND  A.FNumber = '{custNumber}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return new List<string>();
            }

            foreach (var item in data)
            {
                if (orgIdList.Contains(item["Id"].ToString()))
                {
                    orgIdList.Remove(item["Id"].ToString());
                }
            }

            return orgIdList;
        }

        /// <summary>
        /// 获取联系人内码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="zohoId"></param>
        /// <returns></returns>
        public static string GetContactIdByNumber(Context context, string number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FContactId Id
                  FROM T_BD_COMMONCONTACT A
                WHERE  1=1
                  AND  A.FNumber = '{number}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["Id"].ToString();
        }

        /// <summary>
        /// 获取辅助属性内码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="zohoId"></param>
        /// <returns></returns>
        public static string GetAuxPropId(Context context, string region,string color)
        {
            string sql = $@"
                select fid
                from (
                SELECT  a.fid
		               ,isnull(b.fdatavalue,'')Color
		               ,isnull(c.fdatavalue,'')Region
                FROM T_BD_FLEXSITEMDETAILV a
                left join T_BAS_AssistantDataEntry_l b 
                  on  a.FF100004=b.FENTRYID and b.FLocaleId = 2052
                left join T_BAS_AssistantDataEntry_l c
                  on  a.FF100003=c.FENTRYID and c.FLocaleId = 2052
                where 1=1
                  AND ISNULL(A.FF100005,'') = ''
                  AND ISNULL(A.FF100006,'') = ''
                  AND ISNULL(A.FF100007,'') = ''
                  AND ISNULL(A.FF100008,'') = ''
                  AND ISNULL(A.FF100012,'') = '') T1     
                where 1=1
                  AND Color ='{color}'
                  AND Region = '{region}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "0";
            }

            return data[0]["fid"].ToString();
        }

        /// <summary>
        /// 获取销售订单内码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="zohoId"></param>
        /// <returns></returns>
        public static string GetOrderIdByBillNo(Context context, string billNo)
        {
            if (string.IsNullOrWhiteSpace(billNo))
            {
                return "";
            }

            string sql = $@"
                SELECT A.FID Id
                  FROM T_SAL_ORDER A
                WHERE  1=1
                  AND  A.FZohoBillNo = '{billNo}'
                UNION ALL
                SELECT A.FID Id
                  FROM T_STK_OUTSTOCKAPPLY A
                WHERE  1=1
                  AND  A.FZohoBillNo = '{billNo}'
                UNION ALL
                SELECT A.FID Id
                  FROM T_STK_STKTRANSFERAPP A
                WHERE  1=1
                  AND  A.FZohoBillNo = '{billNo}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["Id"].ToString();
        }

        /// <summary>
        /// 获取税率内码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="zohoId"></param>
        /// <returns></returns>
        public static string GetTaxRateId(Context context, decimal rate,string country)
        {
            string sql = $@"
                SELECT TOP 1 A.FNumber FNumber
                  FROM T_BD_TAXRATE A
                 WHERE 1=1
                   AND A.FTAXRATE = {rate}
                   AND FCountry = '{country}'
                   AND A.FDocumentStatus = 'C'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 根据出库单内码获取销售订单是否关闭
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool SaleOrderIsClose(Context context, string outStockId,out string zohoId)
        {
            bool result = true;
            zohoId = "";

            if (string.IsNullOrWhiteSpace(outStockId))
            {
                return false;
            }

            string sql = $@"
                SELECT DISTINCT E.FCLOSESTATUS,E.FZohoBillNo FBILLNO
                  FROM T_SAL_OUTSTOCK A
                       INNER JOIN T_SAL_OUTSTOCKENTRY B 
                       ON A.FID=B.FID
                       INNER JOIN T_SAL_OUTSTOCKENTRY_LK C
                       ON B.FENTRYID=C.FENTRYID
                       INNER JOIN T_SAL_ORDERENTRY D
                       ON C.FSID=D.FENTRYID AND C.FSBILLID=D.FID
                       INNER JOIN T_SAL_ORDER E
                       ON D.FID=E.FID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FID = {outStockId}
                  AND  ISNULL(E.FZohoBillNo,'')<>''";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return false;
            }

            foreach (var item in data)
            {
                if (item["FCLOSESTATUS"].ToString() == "B")
                {
                    zohoId = item["FBILLNO"].ToString();
                }
                
                if (item["FCLOSESTATUS"].ToString() == "A")
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 根据出库单内码获取销售订单是否关闭
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool SaleOrderNoClose(Context context, string outStockId, out string zohoId)
        {
            bool result = true;
            zohoId = "";

            if (string.IsNullOrWhiteSpace(outStockId))
            {
                return false;
            }

            string sql = $@"
                SELECT DISTINCT E.FCLOSESTATUS,E.FZohoBillNo FBILLNO
                  FROM T_SAL_OUTSTOCK A
                       INNER JOIN T_SAL_OUTSTOCKENTRY B 
                       ON A.FID=B.FID
                       INNER JOIN T_SAL_OUTSTOCKENTRY_LK C
                       ON B.FENTRYID=C.FENTRYID
                       INNER JOIN T_SAL_ORDERENTRY D
                       ON C.FSID=D.FENTRYID AND C.FSBILLID=D.FID
                       INNER JOIN T_SAL_ORDER E
                       ON D.FID=E.FID
                WHERE  1=1
                  AND  A.FDOCUMENTSTATUS='C'
                  AND  A.FID = {outStockId}
                  AND  ISNULL(E.FZohoBillNo,'')<>''";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return false;
            }

            foreach (var item in data)
            {
                if (item["FCLOSESTATUS"].ToString() == "A")
                {
                    zohoId = item["FBILLNO"].ToString();
                }

                if (item["FCLOSESTATUS"].ToString() == "B")
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 根据出库单内码获取销售订单是否关闭
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool OutRequireIsClose(Context context, string outStockId, out string zohoId)
        {
            bool result = true;
            zohoId = "";

            if (string.IsNullOrWhiteSpace(outStockId))
            {
                return false;
            }

            string sql = $@"
               SELECT  DISTINCT A.FCLOSESTATUS,A.FZohoBillNo FBILLNO
                 FROM  T_STK_OUTSTOCKAPPLY A
                       INNER JOIN T_STK_OUTSTOCKAPPLYENTRY B 
                       ON A.FID=B.FID
                       INNER JOIN T_STK_MISDELIVERYENTRY_LK C 
                       ON C.FSID=B.FENTRYID AND C.FSBILLID= B.FID
                       INNER JOIN T_STK_MISDELIVERYENTRY D 
                       ON D.FENTRYID = C.FENTRYID
                WHERE  1=1
                  AND  D.FID={outStockId}
                  AND  ISNULL(A.FZohoBillNo,'')<>''";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return false;
            }

            foreach (var item in data)
            {
                if (item["FCLOSESTATUS"].ToString() == "B")
                {
                    zohoId = item["FBILLNO"].ToString();
                }

                if (item["FCLOSESTATUS"].ToString() == "A")
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 根据出库单内码获取销售订单是否关闭
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool OutRequireNoClose(Context context, string outStockId, out string zohoId)
        {
            bool result = true;
            zohoId = "";

            if (string.IsNullOrWhiteSpace(outStockId))
            {
                return false;
            }

            string sql = $@"
               SELECT  DISTINCT A.FCLOSESTATUS,A.FZohoBillNo FBILLNO
                 FROM  T_STK_OUTSTOCKAPPLY A
                       INNER JOIN T_STK_OUTSTOCKAPPLYENTRY B 
                       ON A.FID=B.FID
                       INNER JOIN T_STK_MISDELIVERYENTRY_LK C 
                       ON C.FSID=B.FENTRYID AND C.FSBILLID= B.FID
                       INNER JOIN T_STK_MISDELIVERYENTRY D 
                       ON D.FENTRYID = C.FENTRYID
                WHERE  1=1
                  AND  D.FID={outStockId}
                  AND  ISNULL(A.FZohoBillNo,'')<>''";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return false;
            }

            foreach (var item in data)
            {
                if (item["FCLOSESTATUS"].ToString() == "A")
                {
                    zohoId = item["FBILLNO"].ToString();
                }

                if (item["FCLOSESTATUS"].ToString() == "B")
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 根据直接调拨单内码获取调拨申请是否关闭
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool TransApplyIsClose(Context context, string outStockId, out string zohoId)
        {
            bool result = true;
            zohoId = "";

            if (string.IsNullOrWhiteSpace(outStockId))
            {
                return false;
            }

            string sql = $@"
               SELECT  DISTINCT A.FCLOSESTATUS,A.FZohoBillNo FBILLNO
                 FROM  T_STK_STKTRANSFERAPP A
                       INNER JOIN T_STK_STKTRANSFERAPPENTRY B 
                       ON A.FID=B.FID
                       INNER JOIN T_STK_STKTRANSFERINENTRY_LK C 
                       ON C.FSID=B.FENTRYID AND C.FSBILLID= B.FID
                       INNER JOIN T_STK_STKTRANSFERINENTRY D 
                       ON D.FENTRYID = C.FENTRYID
                WHERE  1=1
                  AND  D.FID={outStockId}
                  AND  ISNULL(A.FZohoBillNo,'')<>''";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return false;
            }

            foreach (var item in data)
            {
                if (item["FCLOSESTATUS"].ToString() == "B")
                {
                    zohoId = item["FBILLNO"].ToString();
                }

                if (item["FCLOSESTATUS"].ToString() == "A")
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 根据直接调拨单内码获取调拨申请是否关闭
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool TransApplyNoClose(Context context, string outStockId, out string zohoId)
        {
            bool result = true;
            zohoId = "";

            if (string.IsNullOrWhiteSpace(outStockId))
            {
                return false;
            }

            string sql = $@"
               SELECT  DISTINCT A.FCLOSESTATUS,A.FZohoBillNo FBILLNO
                 FROM  T_STK_STKTRANSFERAPP A
                       INNER JOIN T_STK_STKTRANSFERAPPENTRY B 
                       ON A.FID=B.FID
                       INNER JOIN T_STK_STKTRANSFERINENTRY_LK C 
                       ON C.FSID=B.FENTRYID AND C.FSBILLID= B.FID
                       INNER JOIN T_STK_STKTRANSFERINENTRY D 
                       ON D.FENTRYID = C.FENTRYID
                WHERE  1=1
                  AND  D.FID={outStockId}
                  AND  ISNULL(A.FZohoBillNo,'')<>''";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return false;
            }

            foreach (var item in data)
            {
                if (item["FCLOSESTATUS"].ToString() == "A")
                {
                    zohoId = item["FBILLNO"].ToString();
                }

                if (item["FCLOSESTATUS"].ToString() == "B")
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取单位编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetUnit(Context context, string name)
        {
            string sql = $@"
                SELECT A.FNUMBER
                FROM T_BD_UNIT A
                INNER JOIN T_BD_UNIT_L B ON A.FUNITID=B.FUNITID
                WHERE 1=1
                AND B.FNAME = '{name}'
                AND A.FDOCUMENTSTATUS= 'C'
				AND A.FFORBIDSTATUS='A'
                AND A.FUSEORGID = 0
                ";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);
            if (data.Count<= 0 )
            {
                return "";
            }

            return data[0]["FNUMBER"].ToString();
        }

        /// <summary>
        /// 获取申请类型编码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSQType(Context context, string name)
        {
            string sql = $@"
                SELECT A.FNUMBER
                FROM T_BAS_ASSISTANTDATAENTRY A
                INNER JOIN T_BAS_ASSISTANTDATAENTRY_L B ON A.FENTRYID=B.FENTRYID
                WHERE 1=1
                AND  B.FDATAVALUE = '{name}'
                ";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);
            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FNUMBER"].ToString();
        }

        /// <summary>
        /// 判断物料是否为套件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool IsHaveBom(Context context,string number)
        {
            string sql = $@"
                SELECT  1
                  FROM  T_BD_MATERIAL A
                        INNER JOIN T_BD_MATERIALBASE B ON A.FMATERIALID=B.FMATERIALID
                 WHERE  A.FNUMBER = '{number}'
                   AND  B.FSuite = '1'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);
            if (data.Count <= 0)
            {
                return false;
            }

            return true;
        }
    }
}
