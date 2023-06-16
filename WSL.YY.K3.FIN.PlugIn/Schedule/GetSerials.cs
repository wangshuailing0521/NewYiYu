using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSL.YY.K3.FIN.PlugIn.Helper;
using WSL.YY.K3.FIN.PlugIn.Model;

namespace WSL.YY.K3.FIN.PlugIn.Schedule
{
    [Description("获取序列号执行计划")]
    [Kingdee.BOS.Util.HotUpdate]
    public class GetSerials : IScheduleService
    {
        Context _ctx;
        string url = $@"http://8.209.75.207:20032/Inbound/Api/SaveReceiptSerial";
        string apikey = "16123E89FEE245D5A64609890809814A";

        public void Run(Context ctx, Kingdee.BOS.Core.Schedule schedule)
        {
            _ctx = ctx;

            ToInterface();
        }

        void ToInterface()
        {
            DynamicObjectCollection data
                = GetData();

            if (data.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < data.Count; i++)
            {
                DynamicObject item = data[i];

                string seriNo = item["Fpackaging"].ToString();
                if (IsTwo(seriNo))
                {
                    continue;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine($@"接口方向：Kingdee --> WMS");
                sb.AppendLine($@"接口名称：传递序列号API");
                try
                {
                    Require(item, sb);
                    UpdateStatus(item["FID"].ToString());


                    Logger.Info("", sb.ToString());
                }
                catch (Exception ex)
                {
                    sb.AppendLine($@"错误信息：{ex.Message.ToString()}");
                    Logger.Error("", sb.ToString(), ex);
                }
            }
        }

        /// <summary>
        /// 调用接口
        /// </summary>
        /// <param name="item"></param>
        /// <param name="sb"></param>
        void Require(DynamicObject item, StringBuilder sb)
        {
            List<SerialReceipts> serialReceiptList = new List<SerialReceipts>();
            SerialReceipts serialReceipts = new SerialReceipts();
            SerialReceipt serialReceipt = new SerialReceipt();

            List<Serial> serials = new List<Serial>();
            string id = item["FID"].ToString();
            string lot = "";
            string lpnId = "";
            string skuId = "";
            string createdate = item["FCreateDate"].ToString();
            string qty = item["FSumQty"].ToString();

            DynamicObjectCollection entrys = GetTwoEntry(id);
            if (entrys.Count > 0)
            {
                foreach (var entry in entrys)
                {
                    //string SerialNo = GetSeriNo(entry["FEntryBarCode"].ToString());
                    string SerialNo = entry["FEntryBarCode"].ToString();
                    if (string.IsNullOrWhiteSpace(SerialNo))
                    {
                        continue;
                    }
                    serialReceipt.Whgid = "medtrum.wh1";

                    Serial serial = new Serial();
                    serial.WHID = "medtrum.wh1";
                    serial.OwnerID = "1001";
                    serial.SerialNo = SerialNo;
                    serial.SkuId = entry["FMaterialNo"].ToString();
                    serial.LpnID = entry["Fpackaging"].ToString();
                    serial.CreatedBy = "demo";
                    serial.CreatedDate = entry["FCreateDate"].ToString();
                    serial.PackQty = entry["FQty"].ToString();
                    serial.ExternalLot = entry["FLOT_TEXT"].ToString();
                    lot = entry["FLOT_TEXT"].ToString();
                    lpnId = entry["Fpackaging"].ToString();
                    skuId = entry["FBoxNumber"].ToString();
                    serials.Add(serial);
                }
            }
            else
            {
                entrys = GetEntry(id);

                foreach (var entry in entrys)
                {
                    //string SerialNo = GetSeriNo(entry["FEntryBarCode"].ToString());
                    string SerialNo = entry["FEntryBarCode"].ToString();
                    if (string.IsNullOrWhiteSpace(SerialNo))
                    {
                        continue;
                    }

                    serialReceipt.Whgid = "medtrum.wh1";

                    Serial serial = new Serial();
                    serial.WHID = "medtrum.wh1";
                    serial.OwnerID = "1001";
                    serial.SerialNo = SerialNo;
                    serial.SkuId = entry["FMaterialNo"].ToString();
                    serial.LpnID = entry["Fpackaging"].ToString();
                    serial.CreatedBy = "demo";
                    serial.CreatedDate = entry["FCreateDate"].ToString();
                    serial.PackQty = entry["FQty"].ToString();
                    serial.ExternalLot = entry["FLOT_TEXT"].ToString();
                    lot = entry["FLOT_TEXT"].ToString();
                    lpnId = entry["Fpackaging"].ToString();
                    skuId = entry["FBoxNumber"].ToString();
                    serials.Add(serial);
                }
            }

            if (serials.Count<= 0)
            {
                return;
            }

            Serial serial1 = new Serial();
            serial1.WHID = "medtrum.wh1";
            serial1.OwnerID = "1001";
            serial1.SerialNo = lpnId;
            serial1.SkuId = skuId;
            serial1.LpnID = lpnId;
            serial1.CreatedBy = "demo";
            serial1.CreatedDate = createdate;
            serial1.PackQty = qty;
            serial1.CaseID = "";
            serial1.ExternalLot = lot;
            serials.Add(serial1);

            //if (!string.IsNullOrWhiteSpace(item["FExpressNo"].ToString()))
            //{
            //    serialReceipt.Whgid = entrys[0]["f_ora_text"].ToString();
            //    Serial serial = new Serial();
            //    serial.WHID = entrys[0]["f_ora_text"].ToString();
            //    serial.OwnerID = entrys[0]["FOrgNo"].ToString();
            //    serial.SerialNo = item["Fpackaging"].ToString();
            //    serial.SkuId = item["FExpressNo"].ToString();
            //    serial.LpnID = item["Fpackaging"].ToString();
            //    serial.CreatedBy = "demo";
            //    serial.CreatedDate = item["FCreateDate"].ToString();
            //    serial.PackQty = item["FSumQty"].ToString();
            //    serial.ExternalLot = entrys[0]["FLOT_TEXT"].ToString();
            //    serials.Add(serial);
            //}
            //else
            //{
            //    foreach (var entry in entrys)
            //    {
            //        serialReceipt.Whgid = entry["f_ora_text"].ToString();

            //        Serial serial = new Serial();
            //        serial.WHID = entry["f_ora_text"].ToString();
            //        serial.OwnerID = entry["FOrgNo"].ToString();
            //        serial.SerialNo = GetSeriNo(entry["FEntryBarCode"].ToString());
            //        serial.SkuId = entry["FMaterialNo"].ToString();
            //        serial.LpnID = entry["Fpackaging"].ToString();
            //        serial.CreatedBy = "demo";
            //        serial.CreatedDate = entry["FCreateDate"].ToString();
            //        serial.PackQty = entry["FQty"].ToString();
            //        serial.ExternalLot = entry["FLOT_TEXT"].ToString();

            //        serials.Add(serial);
            //    }
            //}

            serialReceipt.Serials = serials;
            serialReceipts.SerialReceipt = serialReceipt;
            serialReceiptList.Add(serialReceipts);

            sb.AppendLine($@"请求Url：{url}");
            string json = JsonHelper.ToJSON(serialReceiptList);
            sb.AppendLine($@"请求信息：{json}");
            string response = ApiHelper.HttpPostAuth(url, apikey, json);
            sb.AppendLine($@"返回信息：{response}");

            #region 解析返回信息
            WMSResponse result = JsonHelper.FromJSON<WMSResponse>(response);
            if (!result.success)
            {
                throw new Exception(result.msg);
            }
            #endregion


        }

        string GetSeriNo(string serialNo)
        {
            string[] serialNos = null;
            string sn = "";
            if (serialNo.Contains("#3D"))
            {
                serialNos = serialNo.Split(new string[] { "#3D" }, StringSplitOptions.None);
                sn = sn + "#3D"+serialNos[1].Split('$')[0];
                serialNo = serialNos[0];
            }
            if (serialNo.Contains("#2D"))
            {
                serialNos = serialNo.Split(new string[] { "#2D" }, StringSplitOptions.None);
                sn = sn + "#2D"+ serialNos[1].Split('$')[0];
                serialNo = serialNos[0];
            }
            if (serialNo.Contains("#1D"))
            {
                serialNos = serialNo.Split(new string[] { "#1D" }, StringSplitOptions.None);
                sn = sn + "#1D"+ serialNos[1].Split('$')[0];
                serialNo = serialNos[0];
            }

            return sn;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        DynamicObjectCollection GetData()
        {
            string sql = $@"
                SELECT  
                        A.Fpackaging
                       ,A.FBarCodeScan
                       ,A.FCreatorId
                       ,A.FCreateDate
                       ,A.FBoxMaterialId
                       ,A.FBillCode
                       ,A.FID
                       ,A.FExpressNo
                       ,A.FSumQty
                  FROM  t_UN_Packaging A
                 WHERE  1=1
                   AND  ISNULL(FWMSID,'') = ''
                   AND  A.FCreateDate >= '2020-06-01'
                   --AND  A.Fpackaging IN ('ZXTM2009252350','ZXTM2009302603','ZXTM190603053')
";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(_ctx, sql);

            return data;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        DynamicObjectCollection GetEntry(string id)
        {
            string sql = $@"
                SELECT  A.Fpackaging
                       ,A.FBarCodeScan
                       ,A.FCreatorId
                       ,A.FCreateDate
                       ,A.FBoxMaterialId
                       ,A.FBillCode
                       ,B.FEntryBarCode
                       ,C.FNUMBER FMaterialNo
                       ,B.FQty
                       ,B.FDetailBillcode
                       ,B.FLot
                       ,B.FLOT_TEXT
                       ,T1.FNumber FBoxNumber
                       --,E.FNUMBER FOrgNo
                       --,E.f_ora_text
                       --,E.f_ora_text1
                  FROM  t_UN_Packaging A
                        INNER JOIN t_UN_PackagingEntry B ON A.FID = B.FID
                        INNER JOIN T_BD_MATERIAL C ON B.FItemID = C.FMATERIALID
                        INNER JOIN T_BD_MATERIAL T1 ON A.FBOXMATERIALID = T1.FMATERIALID
                        --INNER JOIN T_BD_LOTMASTER D ON D.FNUMBER = B.FLOT_TEXT AND D.FLOTSTATUS = '1' AND ((B.FLOT = D.FLOTID AND B.FLOT <>0) OR B.FLOT = 0)
                        --INNER JOIN T_ORG_ORGANIZATIONS E ON D.FCREATEORGID = E.FORGID
                 WHERE  1=1
                   AND  ISNULL(FWMSID,'') = ''
                   AND  A.FID = {id}";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(_ctx, sql);

            return data;
        }

        /// <summary>
        /// 获取两层数据
        /// </summary>
        /// <returns></returns>
        DynamicObjectCollection GetTwoEntry(string id)
        {
            string sql = $@"
                SELECT  A.Fpackaging
                       ,A.FBarCodeScan
                       ,A.FCreatorId
                       ,A.FCreateDate
                       ,A.FBoxMaterialId
                       ,A.FBillCode
                       ,G.FEntryBarCode
                       ,C.FNUMBER FMaterialNo
                       ,G.FQty
                       ,G.FDetailBillcode
                       ,G.FLot
                       ,G.FLOT_TEXT
                       ,T1.FNumber FBoxNumber
                       --,E.FNUMBER FOrgNo
                       --,E.f_ora_text
                       --,E.f_ora_text1
                       ,F.FID
                  FROM  t_UN_Packaging A
                        INNER JOIN t_UN_PackagingEntry B ON A.FID = B.FID
                        INNER JOIN t_UN_Packaging F ON B.FEntryBarCode = F.Fpackaging
                        INNER JOIN t_UN_PackagingEntry G ON F.FID =G.FID
                        INNER JOIN T_BD_MATERIAL C ON G.FItemID = C.FMATERIALID
                        INNER JOIN T_BD_MATERIAL T1 ON A.FBOXMATERIALID = T1.FMATERIALID

                        --INNER JOIN T_BD_LOTMASTER D ON D.FNUMBER = G.FLOT_TEXT AND D.FLOTSTATUS = '1' AND ((G.FLOT = D.FLOTID AND G.FLOT <>0) OR G.FLOT = 0)
                        --INNER JOIN T_ORG_ORGANIZATIONS E ON D.FCREATEORGID = E.FORGID
                 WHERE  1=1
                   AND  ISNULL(F.FWMSID,'') = ''
                   AND  A.FID = {id}";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(_ctx, sql);

            return data;
        }

        bool IsTwo(string seriNo)
        {
            string sql = $@"
                SELECT  1
                  FROM  t_UN_Packaging A
                        INNER JOIN t_UN_PackagingEntry B ON A.FID = B.FID
                 WHERE  1=1
                   AND  B.FEntryBarCode = '{seriNo}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(_ctx, sql);

            if (data.Count > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 更新数据接口调用状态
        /// </summary>
        /// <param name="billNo"></param>
        void UpdateStatus(string id)
        {
            string sql = $@"
                UPDATE  t_UN_Packaging
                   SET  FWMSID = '1'
                 WHERE  FID='{id}'";
            DBUtils.Execute(_ctx, sql);
        }
    }
}
