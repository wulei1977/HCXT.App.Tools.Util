using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HCXT.App.Tools.Util
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logtype"></param>
    /// <param name="logmessage"></param>
    public delegate void DelegateOnLog(string logtype, string logmessage);
    /// <summary>
    /// AES加密/解密类
    /// </summary>
    public class MyAes
    {
        /// <summary>
        /// 事件：扔出日志
        /// </summary>
        public event DelegateOnLog OnLog;
        private void CallOnLog(string logtype, string logmessage)
        {
            DelegateOnLog handler = OnLog;
            if (handler != null) handler(logtype, logmessage);
        }

        private Encoding _encoding = Encoding.UTF8;
        /// <summary>
        /// 字符编码集名称
        /// </summary>
        public string EncodingName
        {
            get { return _encoding.HeaderName; }
            set { _encoding = Encoding.GetEncoding(value); }
        }

        private byte[] _iv;
        /// <summary>
        /// 向量
        /// </summary>
        public string Iv
        {
            get { return _encoding.GetString(_iv); }
            set
            {
                if (_encoding.GetByteCount(value) == 16)
                    _iv = _encoding.GetBytes(value);
                else
                    throw new Exception("向量必须为16字节！");
            }
        }
        private byte[] _key;
        /// <summary>
        /// 密钥
        /// </summary>
        public string Key
        {
            get { return _encoding.GetString(_key); }
            set
            {
                if (_encoding.GetByteCount(value) == 16 || _encoding.GetByteCount(value) == 32)
                    _key = _encoding.GetBytes(value);
                else
                    throw new Exception("密钥必须为16或32字节！");
            }
        }

        private string _cipherModeName;
        /// <summary>
        /// 指定用于加密的块密码模式(CBC/ECB/OFB/CFB/CTS)
        /// </summary>
        public string CipherModeName
        {
            get { return _cipherModeName; }
            set { _cipherModeName = value; }
        }
        private string _paddingModeName;
        /// <summary>
        /// 填充模式(None/PKCS7/Zeros/ANSIX923/ISO10126)
        /// </summary>
        public string PaddingModeName
        {
            get { return _paddingModeName; }
            set { _paddingModeName = value; }
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        public MyAes()
        {
            _iv = _encoding.GetBytes("1234567890123456");
            _key = _encoding.GetBytes("1234567890123456");
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>
        /// <param name="encodingName">字符编码集名称</param>
        public MyAes(string key, string iv, string encodingName)
        {
            EncodingName = encodingName;
            Key = key;
            Iv = iv;
        }

        /// <summary>
        /// AES加密算法
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <returns>返回加密后的密文BASE64串</returns>
        public string Encrypt(string plainText)
        {
            try
            {
                byte[] arrInp = _encoding.GetBytes(plainText);
                byte[] arrRes = Encrypt(arrInp);
                string result = Convert.ToBase64String(arrRes);
                return result;
            }
            catch (Exception err)
            {
                CallOnLog("Error", string.Format("[Encrypter.Encrypt] AES加密算法方法发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace));
                return null;
            }
        }

        /// <summary>
        /// AES解密算法
        /// </summary>
        /// <param name="cipherText">密文BASE64串</param>
        /// <returns>返回解密后的明文字符串</returns>
        public string Decrypt(string cipherText)
        {
            try
            {
                byte[] arrInp = Convert.FromBase64String(cipherText);
                byte[] arrRes = Decrypt(arrInp);
                string result = _encoding.GetString(arrRes);
                return result;
            }
            catch (Exception err)
            {
                CallOnLog("Error", string.Format("[Encrypter.Decrypt] AES解密算法方法发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace));
                return null;
            }
        }

        private void SetMode(SymmetricAlgorithm aes)
        {
            switch (_cipherModeName)//CBC/ECB/OFB/CFB/CTS
            {
                case "CBC":
                    aes.Mode = CipherMode.CBC;
                    break;
                case "ECB":
                    aes.Mode = CipherMode.ECB;
                    break;
                case "OFB":
                    aes.Mode = CipherMode.OFB;
                    break;
                case "CFB":
                    aes.Mode = CipherMode.CFB;
                    break;
                case "CTS":
                    aes.Mode = CipherMode.CTS;
                    break;
            }
            switch (_paddingModeName)//None/PKCS7/Zeros/ANSIX923/ISO10126
            {
                case "None":
                    aes.Padding = PaddingMode.None;
                    break;
                case "PKCS7":
                    aes.Padding = PaddingMode.PKCS7;
                    break;
                case "Zeros":
                    aes.Padding = PaddingMode.Zeros;
                    break;
                case "ANSIX923":
                    aes.Padding = PaddingMode.ANSIX923;
                    break;
                case "ISO10126":
                    aes.Padding = PaddingMode.ISO10126;
                    break;
            }            
        }
        /// <summary>
        /// AES加密算法
        /// </summary>
        /// <param name="plainText">明文字节数组</param>
        /// <returns>返回加密后的密文字节数组</returns>
        public byte[] Encrypt(byte[] plainText)
        {
            SymmetricAlgorithm aes = Rijndael.Create();
            SetMode(aes);

            MemoryStream ms = null;
            CryptoStream cs = null;
            byte[] result;
            try
            {
                byte[] inputByteArray = (byte[])plainText.Clone();
                aes.Key = _key;
                aes.IV = _iv;
                ms = new MemoryStream();
                cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                result = ms.ToArray(); // 得到加密后的字节数组
            }
            catch (Exception err)
            {
                CallOnLog("Error", string.Format("[Encrypter.AesEncrypt] AES加密算法方法发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace));
                result = null;
            }
            finally
            {
                if (cs != null) cs.Close();
                if (ms != null) ms.Close();
                aes.Clear();
            }
            return result;
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="cipherText">密文字节数组</param>
        /// <returns>返回解密后的字符串</returns>
        public byte[] Decrypt(byte[] cipherText)
        {
            SymmetricAlgorithm aes = Rijndael.Create();
            SetMode(aes);

            MemoryStream ms = null;
            CryptoStream cs = null;
            byte[] result;
            try
            {
                aes.Key = _key;
                aes.IV = _iv;
                result = new byte[cipherText.Length];
                ms = new MemoryStream(cipherText);
                cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                cs.Read(result, 0, result.Length);
            }
            catch (Exception err)
            {
                CallOnLog("Error", string.Format("[Encrypter.AesDecrypt] AES解密算法方法发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace));
                result = null;
            }
            finally
            {
                if (cs != null) cs.Close();
                if (ms != null) ms.Close();
                aes.Clear();
            }
            return result;
        }
    }
}