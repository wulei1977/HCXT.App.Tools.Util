using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace HCXT.App.Tools.Util
{
    /// <summary>
    /// 主窗体
    /// </summary>
    public partial class FrmMain : Form
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public FrmMain()
        {
            InitializeComponent();

            cbBase64EncodingName.SelectedIndex = 0;
            cbMd5EncodingName.SelectedIndex = 0;
            cbSha1EncodingName.SelectedIndex = 0;
            cbDesEncodingName.SelectedIndex = 0;
            cbAesEncodingName.SelectedIndex = 0;

            cbCipherMode.SelectedIndex = 0;
            cbPaddingMode.SelectedIndex = 1;

            //MessageBox.Show(AppDomain.CurrentDomain.FriendlyName);
            //Icon = ApiTools.GetIcon(AppDomain.CurrentDomain.FriendlyName, true);
        }

        #region 生成GUID相关
        /// <summary>
        /// 生成GUID
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButGetGuidClick(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < nudGUIDCount.Value; i++)
            {
                sb.AppendLine(radGUIDUpper.Checked ? Guid.NewGuid().ToString().ToUpper() : Guid.NewGuid().ToString().ToLower());
            }
            txtGUIDResult.Text = sb.ToString();
        }
        #endregion

        #region MD5相关
        private void ButMd5Click(object sender, EventArgs e)
        {
            txtMd5.Text = GetMd5(radMd5File.Checked ? txtMd5SrcFile.Text : txtMd5Src.Text, radMd5File.Checked,
                                 cbMd5EncodingName.Text);
        }

        private void ButMd5BrowseClick(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = false;
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                txtMd5SrcFile.Text = openFileDialog1.FileName;
                radMd5File.Checked = true;
            }
        }

        private void RadMd5CheckedChanged(object sender, EventArgs e)
        {
            cbMd5EncodingName.Visible = radMd5Txt.Checked;
        }

        private void txtMd5SrcFile_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                    txtMd5SrcFile.Text = files[0];
            }
            //string[] arr = e.Data.GetFormats();
            //MessageBox.Show(string.Join("\r\n", arr));
        }

        /// <summary>
        /// 获取指定字符串的MD5编码散列字符串
        /// </summary>
        /// <param name="inputString">指定字符串</param>
        /// <param name="isFile"> </param>
        /// <param name="encodingName"> </param>
        /// <returns></returns>
        public static string GetMd5(string inputString, bool isFile, string encodingName)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] md5Byte;
            if(isFile)
            {
                if(File.Exists(inputString))
                {
                    try
                    {
                        FileStream fs = File.OpenRead(inputString);
                        md5Byte = md5.ComputeHash(fs);
                        fs.Close();
                        fs.Dispose();
                    }
                    catch(Exception err)
                    {
                        string msg = string.Format("发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
                        return msg;
                    }
                }
                else
                {
                    return "文件不存在";
                }
            }
            else
            {
                byte[] buff = Encoding.GetEncoding(encodingName).GetBytes(inputString);
                md5Byte = md5.ComputeHash(buff);
            }
            StringBuilder sb = new StringBuilder();
            foreach (byte b in md5Byte)
            {
                int i = Convert.ToInt32(b);
                int j = i >> 4;
                sb.Append(Convert.ToString(j, 16));
                j = ((i << 4) & 0x00ff) >> 4;
                sb.Append(Convert.ToString(j, 16));
            }
            return sb.ToString();
        }
        #endregion

        #region SHA1相关
        private void butSha1Browse_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = false;
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                txtSha1SrcFile.Text = openFileDialog1.FileName;
                radSha1File.Checked = true;
            }
        }

        private void butSha1_Click(object sender, EventArgs e)
        {
            txtSha1.Text = GetSha1(radSha1File.Checked ? txtSha1SrcFile.Text : txtSha1Src.Text, radSha1File.Checked,
                     cbSha1EncodingName.Text);
        }

        private void RadSha1CheckedChanged(object sender, EventArgs e)
        {
            cbSha1EncodingName.Visible = radSha1Txt.Checked;
        }


        /// <summary>
        /// 获取指定字符串的SHA1编码散列字符串
        /// </summary>
        /// <param name="inputString">指定字符串</param>
        /// <param name="isFile"> </param>
        /// <param name="encodingName"> </param>
        /// <returns></returns>
        public static string GetSha1(string inputString, bool isFile, string encodingName)
        {
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] sha1Byte;
            if (isFile)
            {
                if (File.Exists(inputString))
                {
                    try
                    {
                        FileStream fs = File.OpenRead(inputString);
                        sha1Byte = sha1.ComputeHash(fs);
                        fs.Close();
                        fs.Dispose();
                    }
                    catch (Exception err)
                    {
                        string msg = string.Format("发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
                        return msg;
                    }
                }
                else
                {
                    return "文件不存在";
                }
            }
            else
            {
                byte[] buff = Encoding.GetEncoding(encodingName).GetBytes(inputString);
                sha1Byte = sha1.ComputeHash(buff);
            }
            StringBuilder sb = new StringBuilder();
            foreach (byte b in sha1Byte)
            {
                int i = Convert.ToInt32(b);
                int j = i >> 4;
                sb.Append(Convert.ToString(j, 16));
                j = ((i << 4) & 0x00ff) >> 4;
                sb.Append(Convert.ToString(j, 16));
            }
            return sb.ToString();
        }
        #endregion

        #region Base64相关
        private void ButBase64EncodeClick(object sender, EventArgs e)
        {
            byte[] arr = Encoding.GetEncoding(cbBase64EncodingName.Text).GetBytes(txtBase64Src.Text);
            txtBase64Obj.Text = Convert.ToBase64String(arr);
        }

        private void ButBase64DecodeClick(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(txtBase64Obj.Text.Trim()))
                return;
            try
            {
                byte[] arr = Convert.FromBase64String(txtBase64Obj.Text);
                txtBase64Src.Text = Encoding.GetEncoding(cbBase64EncodingName.Text).GetString(arr);
            }
            catch(Exception err)
            {
                txtBase64Src.Text = string.Format("发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
            }
        }
        #endregion

        #region RTF编码相关
        private void ButRtfEncodeClick(object sender, EventArgs e)
        {
            txtRTF.Text = rtRTF.Rtf;
        }

        private void ButRtfDecodeClick(object sender, EventArgs e)
        {
            try
            {
                rtRTF.SelectAll();
                rtRTF.SelectedRtf = txtRTF.Text;
            }
            catch(Exception err)
            {
                rtRTF.Text = string.Format("发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
            }
        }
        #endregion

        #region DES相关
        private void ButDesEncodeClick(object sender, EventArgs e)
        {
            try
            {
                MyDes.EncodingName = cbDesEncodingName.Text;
                txtDesObj.Text = MyDes.Encrypt(txtDesSrc.Text, txtDesKey.Text, txtDesVI.Text, radDesBase64.Checked);
            }
            catch (Exception err)
            {
                txtDesObj.Text = err.Message;
            }

        }

        private void ButDesDecodeClick(object sender, EventArgs e)
        {
            try
            {
                MyDes.EncodingName = cbDesEncodingName.Text;
                txtDesSrc.Text = MyDes.Decrypt(txtDesObj.Text, txtDesKey.Text, txtDesVI.Text, radDesBase64.Checked);
            }
            catch (Exception err)
            {
                txtDesSrc.Text = err.Message;
            }
        }
        #endregion

        #region AES相关
        private void butAesEncode_Click(object sender, EventArgs e)
        {
            try
            {
                MyAes aes = new MyAes(txtAesKey.Text, txtAesVI.Text, cbAesEncodingName.Text);
                aes.CipherModeName = cbCipherMode.SelectedItem.ToString();
                aes.PaddingModeName = cbPaddingMode.SelectedItem.ToString();
                if(radAesBase64.Checked)
                    txtAesObj.Text = aes.Encrypt(txtAesSrc.Text);
                else
                {
                    byte[] bs = Encoding.GetEncoding(cbAesEncodingName.Text).GetBytes(txtAesSrc.Text);
                    byte[] bo = aes.Encrypt(bs);
                    txtAesObj.Text = MyDes.ToHexString(bo);
                }
            }
            catch (Exception err)
            {
                txtAesObj.Text = err.Message;
            }
        }

        private void butAesDecode_Click(object sender, EventArgs e)
        {
            try
            {
                MyAes aes = new MyAes(txtAesKey.Text, txtAesVI.Text, cbAesEncodingName.Text);
                aes.CipherModeName = cbCipherMode.SelectedItem.ToString();
                aes.PaddingModeName = cbPaddingMode.SelectedItem.ToString();
                if (radAesBase64.Checked)
                    txtAesSrc.Text = aes.Decrypt(txtAesObj.Text);
                else
                {
                    MemoryStream ms = new MemoryStream();
                    char[] c = txtAesObj.Text.ToCharArray();
                    for (int i = 0; i < c.Length; i += 2)
                    {
                        string s = string.Format("0x{0}{1}", c[i], c[i + 1]);
                        byte b = Convert.ToByte(Convert.ToInt16(s, 16));
                        ms.WriteByte(b);
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] bs = ms.ToArray();
                    ms.Dispose();
                    byte[] bo = aes.Decrypt(bs);
                    txtAesSrc.Text = Encoding.GetEncoding(cbAesEncodingName.Text).GetString(bo);
                }
            }
            catch (Exception err)
            {
                txtAesSrc.Text = err.Message;
            }
        }
        #endregion

        #region 文件批量替换相关(未完成)
        private void butReplaceFolderBrowse_Click(object sender, EventArgs e)
        {
            DialogResult dr = folderBrowserDialog1.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                txtReplaceFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void butReplace_Click(object sender, EventArgs e)
        {
            MessageBox.Show("此功能还没写完，最近没时间弄。有时间再说吧。\r\n囧.....");
            return;
            byte[] repGbFrom = Encoding.GetEncoding("gb18030").GetBytes(txtReplaceFrom.Text);
            byte[] repGbTo = Encoding.GetEncoding("gb18030").GetBytes(txtReplaceTo.Text);
            byte[] repUtfFrom = Encoding.GetEncoding("utf-8").GetBytes(txtReplaceFrom.Text);
            byte[] repUtfTo = Encoding.GetEncoding("utf-8").GetBytes(txtReplaceTo.Text);
            char[] spFilter = new char[] {'|'};
            string[] arrFileFilter = txtReplaceFilter.Text.Split(spFilter, StringSplitOptions.RemoveEmptyEntries);


            Random rnd = new Random();



            if (!Directory.Exists(txtReplaceFolder.Text))
            {
                MessageBox.Show(string.Format("文件夹[{0}]不存在！", txtReplaceFolder.Text));
                return;
            }
            DirectoryInfo directory = new DirectoryInfo(txtReplaceFolder.Text);
            DirectoryInfo[] subDirs = directory.GetDirectories();
            for (int i = 0; i < arrFileFilter.Length; i++)
            {
                FileInfo[] files = directory.GetFiles(arrFileFilter[i]);
                string fTemp = string.Format("{0}.{1}", txtReplaceFolder.Text, rnd.Next());
                Stream streamRead = null;
                Stream streamWrite = null;
                try
                {
                    streamRead = File.Open(txtReplaceFolder.Text, FileMode.Open, FileAccess.Read, FileShare.Read);
                    streamWrite = File.Open(fTemp, FileMode.Append, FileAccess.Write);


                    int b;
                    do
                    {
                        b = streamRead.ReadByte();

                    } while (b != -1);



                }
                catch (Exception err)
                {

                }
                finally
                {
                    if (streamRead != null) streamRead.Close();
                    if (streamWrite != null) streamWrite.Close();
                }

            }

        }

        /// <summary>
        /// 陷阱
        /// </summary>
        /// <param name="streamRead"></param>
        /// <param name="streamWrite"></param>
        /// <param name="rep"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private int Trap(Stream streamRead, Stream streamWrite, byte[] rep, int level)
        {
            int b = streamRead.ReadByte();
            if (b == -1)
                return -1;
            if (b != rep[level])
                return 0;
            Trap(streamRead, streamWrite, rep, level + 1);

            return 0;
        }
        #endregion

        #region 文件转码相关
        private void butEftLoad_Click(object sender, EventArgs e)
        {
            txtEftContent.MaxLength = int.MaxValue;
            try
            {
                txtEftContent.Text = File.ReadAllText(txtEftInput.Text, Encoding.GetEncoding(cbEftInput.Text));
            }
            catch(Exception err)
            {
                txtEftContent.Text = string.Format("{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
            }
        }

        private void butEftSave_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText(txtEftOutput.Text, txtEftContent.Text, Encoding.GetEncoding(cbEftOutput.Text));
            }
            catch (Exception err)
            {
                txtEftContent.Text = string.Format("{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
            }
        }
        #endregion
    }
}