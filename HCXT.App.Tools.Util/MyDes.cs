using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HCXT.App.Tools.Util
{
    /// <summary>
    /// 
    /// </summary>
    public class MyDes
    {
        /// <summary>
        /// ���뼯���ơ�Ĭ��Ϊgb2312
        /// </summary>
        public static string EncodingName = "gb2312";

        /// <summary>
        /// ����DES���ܡ�
        /// </summary>
        /// <param name="pToEncrypt">Ҫ���ܵ��ַ�����</param>
        /// <param name="sKey">��Կ���ұ���Ϊ8λ��</param>
        /// <param name="sIv"> </param>
        /// <param name="isBase64">true:��Base64��ʽ���صļ����ַ����� false:��HEX��ʽ���ؼ����ַ���</param>
        /// <returns></returns>
        public static string Encrypt(string pToEncrypt, string sKey, string sIv, bool isBase64)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                byte[] inputByteArray = Encoding.GetEncoding(EncodingName).GetBytes(pToEncrypt);
                byte[] desKey = new byte[8];
                byte[] desIv = new byte[8];
                byte[] bKey = Encoding.Default.GetBytes(sKey);
                byte[] bIv = Encoding.Default.GetBytes(sIv);
                Array.Copy(bKey, 0, desKey, 0, bKey.Length > 8 ? 8 : bKey.Length);
                Array.Copy(bIv, 0, desIv, 0, bIv.Length > 8 ? 8 : bIv.Length);
                des.Key = desKey;
                des.IV = desIv;

                MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                byte[] be = ms.ToArray();
                ms.Close();
                string str = isBase64 ? Convert.ToBase64String(be) : ToHexString(be);
                return str;
            }
        }

        /// <summary>
        /// ����DES���ܡ�
        /// </summary>
        /// <param name="pToDecrypt">Ҫ���ܵ���Base64</param>
        /// <param name="sKey">��Կ���ұ���Ϊ8λ��</param>
        /// <param name="isBase64"> </param>
        /// <returns>�ѽ��ܵ��ַ�����</returns>
        public static string Decrypt(string pToDecrypt, string sKey, string sIv, bool isBase64)
        {
            byte[] inputByteArray;
            if (isBase64)
                inputByteArray = Convert.FromBase64String(pToDecrypt);
            else
            {
                MemoryStream ms = new MemoryStream();
                char[] c = pToDecrypt.ToCharArray();
                for (int i = 0; i < c.Length; i += 2)
                {
                    string s = string.Format("0x{0}{1}", c[i], c[i + 1]);
                    byte b = Convert.ToByte(Convert.ToInt16(s, 16));
                    ms.WriteByte(b);
                }
                ms.Seek(0, SeekOrigin.Begin);
                inputByteArray = ms.ToArray();
            }
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                byte[] desKey = new byte[8];
                byte[] desIv = new byte[8];
                byte[] bKey = Encoding.Default.GetBytes(sKey);
                byte[] bIv = Encoding.Default.GetBytes(sIv);
                Array.Copy(bKey, 0, desKey, 0, bKey.Length > 8 ? 8 : bKey.Length);
                Array.Copy(bIv, 0, desIv, 0, bIv.Length > 8 ? 8 : bIv.Length);
                des.Key = desKey;
                des.IV = desIv;
                MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                string str = Encoding.GetEncoding(EncodingName).GetString(ms.ToArray());
                ms.Close();
                return str;
            }
        }

        /// <summary>
        /// ������Կ
        /// </summary>
        /// <returns></returns>
        public static string GenerateKey()
        {
            DESCryptoServiceProvider desCrypto = (DESCryptoServiceProvider)DES.Create();
            return Encoding.ASCII.GetString(desCrypto.Key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                int i = Convert.ToInt32(b);
                int j = i >> 4;
                sb.Append(Convert.ToString(j, 16));
                j = ((i << 4) & 0x00ff) >> 4;
                sb.Append(Convert.ToString(j, 16));
            }
            return sb.ToString();
        }
    }
}