using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace WSL.YY.K3.FIN.PlugIn.Helper
{
    public static class ApiHelper
    {
        public static string HttpPost(string url, string body)
        {
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //加上这一句

            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";

            byte[] buffer = encoding.GetBytes(body);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static string HttpPostAuth(string url,string apiKey, string body)
        {
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //加上这一句

            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";
            request.Headers.Add("ApiKey", apiKey);

            byte[] buffer = encoding.GetBytes(body);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection", BindingFlags.Instance | BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        }


        public static string HttpPost_APIKEY(string url, string body,string APIKEY)
        {
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //加上这一句

            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";
            SetHeaderValue(request.Headers, "ApiKey", APIKEY);

            byte[] buffer = encoding.GetBytes(body);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        //带HTTP安全协议BasicAuth验证的post请求方法
        public static string Post_BasicAuthAsync(string url, string user, string secret)
        {
            try
            {
                System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();
                var postContent = new MultipartFormDataContent();


                // 创建身份认证
                AuthenticationHeaderValue authentication = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{secret}")
                    ));

                _httpClient.DefaultRequestHeaders.Authorization = authentication;

                var values = new[]
                {
                    new KeyValuePair<string, string>("grant_type","client_credentials"),
                    new KeyValuePair<string, string>("scope","write")
                 };

                postContent.Headers.Add("ContentType", $"application/json");
                postContent.Add(new StringContent("client_credentials"), "grant_type");
                postContent.Add(new StringContent("write"), "scope");

                HttpResponseMessage response
                    = _httpClient.PostAsync(url, postContent).Result;

                return response.Content.ReadAsStringAsync().Result;

            }
            catch (Exception s)
            {
                //返回错误信息
                return s.Message;
            }
        }


        /// <summary>
                /// 秘钥：加密、解密（秘钥相同,注意保存）
                /// </summary>
                /// <returns></returns>
        private static CspParameters GetCspKey()
        {
            CspParameters param = new CspParameters
            {
                KeyContainerName = "chait"//密匙容器的名称，保持加密解密一致才能解密成功
            };
            return param;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="palinData">明文</param>
        /// <param name="encodingType">编码方式</param>
        /// <returns>密文</returns>
        public static string Encrypt(string palinData)
        {
            if (string.IsNullOrWhiteSpace(palinData)) return null;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(GetCspKey()))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(palinData); //将要加密的字符串转换为指定编码的字节数组
                byte[] encryptData = rsa.Encrypt(bytes, false);//将加密后的字节数据转换为新的加密字节数组
                return Convert.ToBase64String(encryptData);//将加密后的字节数组转换为字符串
            }
        }

        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="publickey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSAEncrypt(string publickey, string content)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(publickey);
            cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(content), false);

            return Convert.ToBase64String(cipherbytes);
        }

        /// <summary>
        /// 获取新的密码盐码
        /// </summary>
        /// <returns></returns>
        public static string GetPasswordSalt()
        {
            var salt = new byte[128 / 8];
            using (var saltnum = RandomNumberGenerator.Create())
            {
                saltnum.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

    }
}
