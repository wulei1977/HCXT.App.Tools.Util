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
    /// AES����/������
    /// </summary>
    public class MyAes
    {
        /// <summary>
        /// �¼����ӳ���־
        /// </summary>
        public event DelegateOnLog OnLog;
        private void CallOnLog(string logtype, string logmessage)
        {
            DelegateOnLog handler = OnLog;
            if (handler != null) handler(logtype, logmessage);
        }

        private Encoding _encoding = Encoding.UTF8;
        /// <summary>
        /// �ַ����뼯����
        /// </summary>
        public string EncodingName
        {
            get { return _encoding.HeaderName; }
            set { _encoding = Encoding.GetEncoding(value); }
        }

        private byte[] _iv;
        /// <summary>
        /// ����
        /// </summary>
        public string Iv
        {
            get { return _encoding.GetString(_iv); }
            set
            {
                if (_encoding.GetByteCount(value) == 16)
                    _iv = _encoding.GetBytes(value);
                else
                    throw new Exception("��������Ϊ16�ֽڣ�");
            }
        }
        private byte[] _key;
        /// <summary>
        /// ��Կ
        /// </summary>
        public string Key
        {
            get { return _encoding.GetString(_key); }
            set
            {
                if (_encoding.GetByteCount(value) == 16 || _encoding.GetByteCount(value) == 32)
                    _key = _encoding.GetBytes(value);
                else
                    throw new Exception("��Կ����Ϊ16��32�ֽڣ�");
            }
        }

        private string _cipherModeName;
        /// <summary>
        /// ָ�����ڼ��ܵĿ�����ģʽ(CBC/ECB/OFB/CFB/CTS)
        /// </summary>
        public string CipherModeName
        {
            get { return _cipherModeName; }
            set { _cipherModeName = value; }
        }
        private string _paddingModeName;
        /// <summary>
        /// ���ģʽ(None/PKCS7/Zeros/ANSIX923/ISO10126)
        /// </summary>
        public string PaddingModeName
        {
            get { return _paddingModeName; }
            set { _paddingModeName = value; }
        }

        /// <summary>
        /// ���췽��
        /// </summary>
        public MyAes()
        {
            _iv = _encoding.GetBytes("1234567890123456");
            _key = _encoding.GetBytes("1234567890123456");
        }

        /// <summary>
        /// ���췽��
        /// </summary>
        /// <param name="key">��Կ</param>
        /// <param name="iv">����</param>
        /// <param name="encodingName">�ַ����뼯����</param>
        public MyAes(string key, string iv, string encodingName)
        {
            EncodingName = encodingName;
            Key = key;
            Iv = iv;
        }

        /// <summary>
        /// AES�����㷨
        /// </summary>
        /// <param name="plainText">�����ַ���</param>
        /// <returns>���ؼ��ܺ������BASE64��</returns>
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
                CallOnLog("Error", string.Format("[Encrypter.Encrypt] AES�����㷨���������쳣���쳣��Ϣ��{0}\r\n��ջ��{1}", err.Message, err.StackTrace));
                return null;
            }
        }

        /// <summary>
        /// AES�����㷨
        /// </summary>
        /// <param name="cipherText">����BASE64��</param>
        /// <returns>���ؽ��ܺ�������ַ���</returns>
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
                CallOnLog("Error", string.Format("[Encrypter.Decrypt] AES�����㷨���������쳣���쳣��Ϣ��{0}\r\n��ջ��{1}", err.Message, err.StackTrace));
                return null;
            }
        }

        /// <summary>
        /// AES�����㷨
        /// </summary>
        /// <param name="plainText">�����ֽ�����</param>
        /// <returns>���ؼ��ܺ�������ֽ�����</returns>
        public byte[] Encrypt(byte[] plainText)
        {
            SymmetricAlgorithm aes = Rijndael.Create();
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
                result = ms.ToArray(); // �õ����ܺ���ֽ�����
            }
            catch (Exception err)
            {
                CallOnLog("Error", string.Format("[Encrypter.AesEncrypt] AES�����㷨���������쳣���쳣��Ϣ��{0}\r\n��ջ��{1}", err.Message, err.StackTrace));
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
        /// AES����
        /// </summary>
        /// <param name="cipherText">�����ֽ�����</param>
        /// <returns>���ؽ��ܺ���ַ���</returns>
        public byte[] Decrypt(byte[] cipherText)
        {
            SymmetricAlgorithm aes = Rijndael.Create();
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
                CallOnLog("Error", string.Format("[Encrypter.AesDecrypt] AES�����㷨���������쳣���쳣��Ϣ��{0}\r\n��ջ��{1}", err.Message, err.StackTrace));
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