using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Encodings;
using System.Security.Cryptography;
using System.IO;
using Xyfy.RSAHelper;

namespace WSL.YY.K3.FIN.PlugIn.Helper
{
    public class RSAForJava
    {
        public RSAForJava()
        {}

        /// <summary>
        /// KEY 结构体
        /// </summary>
        public struct RSAKEY
        {
            /// <summary>
            /// 公钥
            /// </summary>
            public string PublicKey
            {
                get;
                set;
            }
            /// <summary>
            /// 私钥
            /// </summary>
            public string PrivateKey
            {
                get;
                set;
            }
        }

        public RSAKEY GetKey()
        {
            //RSA密钥对的构造器  
            RsaKeyPairGenerator keyGenerator = new RsaKeyPairGenerator();

            //RSA密钥构造器的参数  
            RsaKeyGenerationParameters param = new RsaKeyGenerationParameters(
                Org.BouncyCastle.Math.BigInteger.ValueOf(3),
                new Org.BouncyCastle.Security.SecureRandom(),
                1024,   //密钥长度  
                25);
            //用参数初始化密钥构造器  
            keyGenerator.Init(param);
            //产生密钥对  
            AsymmetricCipherKeyPair keyPair = keyGenerator.GenerateKeyPair();
            //获取公钥和密钥  
            AsymmetricKeyParameter publicKey = keyPair.Public;
            AsymmetricKeyParameter privateKey = keyPair.Private;

            SubjectPublicKeyInfo subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey);


            Asn1Object asn1ObjectPublic = subjectPublicKeyInfo.ToAsn1Object();

            byte[] publicInfoByte = asn1ObjectPublic.GetEncoded("UTF-8");
            Asn1Object asn1ObjectPrivate = privateKeyInfo.ToAsn1Object();
            byte[] privateInfoByte = asn1ObjectPrivate.GetEncoded("UTF-8");

            RSAKEY item = new RSAKEY()
            {
                PublicKey = Convert.ToBase64String(publicInfoByte),
                PrivateKey = Convert.ToBase64String(privateInfoByte)
            };
            return item;
        }

        /// <summary>
        /// 获取公钥
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static AsymmetricKeyParameter GetPublicKeyParameter(string s)
        {
            s = s.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            byte[] publicInfoByte = Convert.FromBase64String(s);
            Asn1Object pubKeyObj = Asn1Object.FromByteArray(publicInfoByte);//这里也可以从流中读取，从本地导入   
            AsymmetricKeyParameter pubKey = PublicKeyFactory.CreateKey(publicInfoByte);
            return pubKey;
        }

        /// <summary>
        /// 获取私钥
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static AsymmetricKeyParameter GetPrivateKeyParameter(string s)
        {
            s = s.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            byte[] privateInfoByte = Convert.FromBase64String(s);
            // Asn1Object priKeyObj = Asn1Object.FromByteArray(privateInfoByte);//这里也可以从流中读取，从本地导入   
            // PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey);
            AsymmetricKeyParameter priKey = PrivateKeyFactory.CreateKey(privateInfoByte);
            return priKey;
        }

        /// <summary>
        /// 加密(私钥加密)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string EncryptByPrivateKey(string s, string key)
        {
            //非对称加密算法，加解密用  
            IAsymmetricBlockCipher engine = new Pkcs1Encoding(new RsaEngine());
 
            try
            {
                engine.Init(true, GetPrivateKeyParameter(key));
                byte[] byteData = System.Text.Encoding.UTF8.GetBytes(s);
                var ResultData = engine.ProcessBlock(byteData, 0, byteData.Length);
                return Convert.ToBase64String(ResultData);
                //Console.WriteLine("密文（base64编码）:" + Convert.ToBase64String(testData) + Environment.NewLine);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 加密(公钥加密)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string EncryptByPublicKey(string s, string key)
        {
            //非对称加密算法，加解密用  
            IAsymmetricBlockCipher engine = new Pkcs1Encoding(new RsaEngine());

            try
            {
                engine.Init(true, GetPublicKeyParameter(key));
                byte[] byteData = System.Text.Encoding.UTF8.GetBytes(s);
                var ResultData = engine.ProcessBlock(byteData, 0, byteData.Length);
                return Convert.ToBase64String(ResultData);
                //Console.WriteLine("密文（base64编码）:" + Convert.ToBase64String(testData) + Environment.NewLine);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 解密（公钥解密）
        /// </summary>
        /// <param name="s"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptByPublicKey(string s, string key)
        {
            s = s.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            //非对称加密算法，加解密用  
            IAsymmetricBlockCipher engine = new Pkcs1Encoding(new RsaEngine());
 
            try
            {
                engine.Init(false, GetPublicKeyParameter(key));
                byte[] byteData = Convert.FromBase64String(s);
                var ResultData = engine.ProcessBlock(byteData, 0, byteData.Length);
                return System.Text.Encoding.UTF8.GetString(ResultData);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region 加密解密方式一：加密密钥和解密密钥相同

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
        /// 解密
        /// </summary>
        /// <param name="encryptData">密文</param>
        /// <returns>明文</returns>
        public static string Decrypt(string encryptData)
        {
            if (string.IsNullOrWhiteSpace(encryptData)) return null;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(GetCspKey()))
            {
                byte[] bytes = Convert.FromBase64String(encryptData);
                byte[] palinData = rsa.Decrypt(bytes, false);
                return Encoding.UTF8.GetString(palinData);
            }
        }
        #endregion

        #region MyRegion


        private static readonly string _publicKey = "";

        /// <summary>
        /// 公钥加密
        /// </summary>
        /// <param name="palinData">明文</param>
        /// <param name="encodingType">编码方式</param>
        /// <returns>string：密文</returns>
        public static void EncryptByPublicKey(string palinData)
        {
            RSAHelper helper = new RSAHelper(RSAType.RSA2, Encoding.UTF8, "aaaaaaaaaaaaaa", "");
        }

        /// <summary>
        /// 公钥加密
        /// </summary>
        /// <param name="strText">明文</param>
        /// <param name="strPublicKey">密文</param>
        /// <returns></returns>
        public string Encrypt(string strText, string strPublicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(strPublicKey);
            //rsa.ImportCspBlob(Convert.FromBase64String(strPublicKey));
            byte[] byteText = Encoding.UTF8.GetBytes(strText);
            byte[] byteEntry = rsa.Encrypt(byteText, false);
            return Convert.ToBase64String(byteEntry);
        }

        /// <summary>
        /// 创建公钥
        /// </summary>
        /// <param name="publicKeyString"></param>
        /// <returns></returns>
        private static RSA CreateRsaFromPublicKey(string publicKeyString)
        {
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] x509key;
            byte[] seq = new byte[15];
            int x509size;

            x509key = Convert.FromBase64String(publicKeyString);
            x509size = x509key.Length;

            using (var mem = new MemoryStream(x509key))
            {
                using (var binr = new BinaryReader(mem))
                {
                    byte bt = 0;
                    ushort twobytes = 0;
                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                        binr.ReadByte();
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();
                    else
                        return null;
                    seq = binr.ReadBytes(15);
                    if (!CompareBytearrays(seq, SeqOID))
                        return null;
                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103)
                        binr.ReadByte();
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();
                    else
                        return null;
                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        return null;
                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                        binr.ReadByte();
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();
                    else
                        return null;
                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;
                    if (twobytes == 0x8102)
                        lowbyte = binr.ReadByte();
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte();
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return null;
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    int modsize = BitConverter.ToInt32(modint, 0);
                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {
                        binr.ReadByte();
                        modsize -= 1;
                    }
                    byte[] modulus = binr.ReadBytes(modsize);
                    if (binr.ReadByte() != 0x02)
                        return null;
                    int expbytes = (int)binr.ReadByte();
                    byte[] exponent = binr.ReadBytes(expbytes);
                    var rsa = RSA.Create();
                    var rsaKeyInfo = new RSAParameters
                    {
                        Modulus = modulus,
                        Exponent = exponent
                    };
                    rsa.ImportParameters(rsaKeyInfo);
                    return rsa;
                }
            }
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        #endregion

    }
}
