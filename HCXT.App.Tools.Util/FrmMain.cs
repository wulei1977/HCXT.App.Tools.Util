using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data.Odbc;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Microsoft.VisualBasic.Logging;

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
            cbSha1Type.SelectedIndex = 0;
            cbDesEncodingName.SelectedIndex = 0;
            cbAesEncodingName.SelectedIndex = 0;

            cbCipherMode.SelectedIndex = 0;
            cbPaddingMode.SelectedIndex = 1;

            var resources = new ResourceManager("HCXT.App.Tools.Util.Properties.Resources", Assembly.GetEntryAssembly());
            object[] arr = resources.GetString("EncodingNameList").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            cbEftInput.Items.AddRange(arr);
            cbEftOutput.Items.AddRange(arr);
            cbReplaceEncoding.Items.AddRange(arr);
            cbRsaEncoding.Items.AddRange(arr);
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
                sb.Append(Convert.ToString(b, 16));
                //int i = Convert.ToInt32(b);
                //int j = i >> 4;
                //sb.Append(Convert.ToString(j, 16));
                //j = ((i << 4) & 0x00ff) >> 4;
                //sb.Append(Convert.ToString(j, 16));
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
                     cbSha1EncodingName.Text, cbSha1Type.SelectedItem.ToString());
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
        /// <param name="shaType">SHA算法，其值可以是：SHA1、SHA256、SHA384、SHA512</param>
        /// <returns></returns>
        public static string GetSha1(string inputString, bool isFile, string encodingName, string shaType)
        {
            HashAlgorithm sha1;

            switch (shaType)
            {
                case "SHA256":
                    sha1 = new SHA256Managed();
                    break;
                case "SHA384":
                    sha1 = new SHA384Managed();
                    break;
                case "SHA512":
                    sha1 = new SHA512Managed();
                    break;
                default:
                    sha1 = new SHA1CryptoServiceProvider();
                    break;
            }
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
                var aes = new MyAes(txtAesKey.Text, txtAesVI.Text, cbAesEncodingName.Text)
                {
                    CipherModeName = cbCipherMode.SelectedItem.ToString(),
                    PaddingModeName = cbPaddingMode.SelectedItem.ToString()
                };
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
                var aes = new MyAes(txtAesKey.Text, txtAesVI.Text, cbAesEncodingName.Text)
                {
                    CipherModeName = cbCipherMode.SelectedItem.ToString(),
                    PaddingModeName = cbPaddingMode.SelectedItem.ToString()
                };
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

        #region RSA相关
        //private readonly RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider();
        private void ButRsaClick(object sender, EventArgs e)
        {
            var but = (Button)sender;
            try
            {
                var encoder = System.Text.Encoding.GetEncoding(cbRsaEncoding.Text);
                switch (but.Name)
                {
                    case "butRsaEncrypt":
                        {
                            var dataToEncrypt = encoder.GetBytes(txtRsaSrc.Text);
                            var rsaEnc = new RSACryptoServiceProvider();
                            rsaEnc.FromXmlString(txtRsaPk.Text);
                            //rsaEnc.ImportParameters(rsaCryptoServiceProvider.ExportParameters(false));
                            var encryptedData = rsaEnc.Encrypt(dataToEncrypt, false);
                            txtRsaObj.Text = Convert.ToBase64String(encryptedData);
                            //txtRsaObj.Text += string.Format("\r\n{0}", rsaEnc.ToXmlString(false));
                        }
                        break;
                    case "butRsaDecrypt":
                        {
                            var encryptedData = Convert.FromBase64String(txtRsaObj.Text);
                            var rsaDec = new RSACryptoServiceProvider();
                            rsaDec.FromXmlString(txtRsaSk.Text);
                            //rsaDec.ImportParameters(rsaCryptoServiceProvider.ExportParameters(true));
                            var decryptedData = rsaDec.Decrypt(encryptedData, false);
                            txtRsaSrc.Text = encoder.GetString(decryptedData);
                            //txtRsaSrc.Text += string.Format("\r\n{0}", rsaDec.ToXmlString(true));
                        }
                        break;
                    case "butRsaNew":
                        {
                            var rsa = new RSACryptoServiceProvider();
                            var sk = rsa.ToXmlString(true);
                            var domPk = new XmlDocument();
                            domPk.LoadXml(sk);
                            var arrSkNode = new[] { "P", "Q", "DP", "DQ", "InverseQ", "D" };
                            var root = domPk.SelectSingleNode("//RSAKeyValue");
                            foreach (var s in arrSkNode)
                                root.RemoveChild(root.SelectSingleNode(s));
                            var pk = root.OuterXml;
                            txtRsaPk.Text = pk;
                            txtRsaSk.Text = sk;
                        }
                        break;
                    case "butRsaExport":
                        {
                            const string ext = ".key";
                            const string flt = "key文件|*.key";
                            const string reg = "\\.key$";
                            const string errMsgExpFileName = "导出失败！文件名错误。";
                            var sfd = new SaveFileDialog {DefaultExt = ext, Filter = flt};
                            var dr = sfd.ShowDialog();
                            sfd.Dispose();
                            if (dr == DialogResult.OK)
                            {
                                var fName = sfd.FileName;
                                if (fName.Length < 5)
                                {
                                    MessageBox.Show(errMsgExpFileName);
                                    return;
                                }
                                var exName = fName.Substring(fName.Length - 4, 4).ToLower();
                                fName = exName != ext ? fName + ext : fName.Substring(0, fName.Length - 4) + exName;
                                var pkName = Regex.Replace(fName, reg, "_Public.key");
                                var skName = Regex.Replace(fName, reg, "_Private.key");
                                try
                                {
                                    File.WriteAllText(pkName, txtRsaPk.Text, encoder);
                                    File.WriteAllText(skName, txtRsaSk.Text, encoder);
                                }
                                catch (Exception err)
                                {
                                    MessageBox.Show(string.Format("发生异常！异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace));
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(string.Format("发生异常！异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace));
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
            const string jiong = "此功能还没写完，最近没时间弄。有时间再说吧。\r\n囧.....";
            MessageBox.Show(jiong);
            var frmAbout = new FrmAbout();
            frmAbout.ShowDialog(this);
            frmAbout.Dispose();
            //return;
            /*
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
            */
        }
        /*
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
        */
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

        private void butEftBrowse_Click(object sender, EventArgs e)
        {
            var but = (Button) sender;
            var dlg = but.Name == "butEftBrowseLoad"
                ? new OpenFileDialog {CheckFileExists = true, Multiselect = false}
                : (FileDialog) new SaveFileDialog ();
            var txt = but.Name == "butEftBrowseLoad" ? txtEftInput : txtEftOutput;
            try
            {
                var dlgRes = dlg.ShowDialog(this);
                if (dlgRes == DialogResult.OK)
                    txt.Text = dlg.FileName;
            }
            catch (Exception err)
            {
                txtEftContent.Text = string.Format("{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
            }
            finally
            {
                dlg.Dispose();
            }
        }
        #endregion

        private void butFtClick(object sender, EventArgs e)
        {
            var but = (Button) sender;
            switch (but.Name)
            {
                case "butFtBrowse":
                    var ofd = new OpenFileDialog { CheckFileExists = true, Multiselect = false };
                    if (ofd.ShowDialog(this) != DialogResult.OK)
                        break;
                    txtFtFileName.Text = ofd.FileName;
                    ofd.Dispose();
                    butFtClick(butFtLoad, e);
                    break;
                case "butFtLoad":
                {
                    if (!File.Exists(txtFtFileName.Text))
                    {
                        MessageBox.Show(string.Format("文件“{0}”不存在！", txtFtFileName.Text));
                        break;
                    }
                    var fi = new FileInfo(txtFtFileName.Text);
                    labFtLen.Text = fi.Length.ToString("##,###");
                    txtFtMd5.Text = GetMd5(fi.FullName, true, null);
                    txtFtSha1.Text = GetSha1(fi.FullName, true, null, "SHA1");
                    chkFtA.Checked = (fi.Attributes & FileAttributes.Archive) != 0;
                    chkFtS.Checked = (fi.Attributes & FileAttributes.System) != 0;
                    chkFtH.Checked = (fi.Attributes & FileAttributes.Hidden) != 0;
                    chkFtR.Checked = (fi.Attributes & FileAttributes.ReadOnly) != 0;
                    dtFtCreateTime.Value = fi.CreationTime;
                    dtFtModifyTime.Value = fi.LastWriteTime;
                    dtFtAccessTime.Value = fi.LastAccessTime;
                }
                    break;
                case "butFtCheckLen":
                {
                    if (!File.Exists(txtFtFileName.Text))
                    {
                        MessageBox.Show(string.Format("文件“{0}”不存在！", txtFtFileName.Text));
                        break;
                    }
                    FileStream fsStream = null;
                    long len = 0;
                    try
                    {
                        fsStream = File.OpenRead(txtFtFileName.Text);
                        const int buffSize= 0x1000000;
                        var buff = new byte[buffSize];
                        var readLen = 0;
                        do
                        {
                            readLen = fsStream.Read(buff, 0, buffSize);
                            len += readLen;
                        } while (readLen > 0);
                        MessageBox.Show(string.Format("文件“{0}”实际长度为：[{1}]，与标注长度[{2}]{3}", txtFtFileName.Text,
                            len.ToString("##,###"), labFtLen.Text, len.ToString("##,###") == labFtLen.Text ? "相同" : "不同"));
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(string.Format("文件“{0}”读取发生异常。异常信息：{1}", txtFtFileName.Text, err.Message));
                    }
                    finally
                    {
                        if (fsStream != null)
                            fsStream.Dispose();
                    }
                }
                    break;
                case "butFtMd5":
                {
                    if (!File.Exists(txtFtFileName.Text))
                    {
                        MessageBox.Show(string.Format("文件“{0}”不存在！", txtFtFileName.Text));
                        break;
                    }
                    txtFtMd5.Text = GetMd5(txtFtFileName.Text, true, null);
                }
                    break;
                case "butFtSha1":
                {
                    if (!File.Exists(txtFtFileName.Text))
                    {
                        MessageBox.Show(string.Format("文件“{0}”不存在！", txtFtFileName.Text));
                        break;
                    }
                    txtFtSha1.Text = GetSha1(txtFtFileName.Text, true, null, "SHA1");
                }
                    break;
                case "butFtUpdateAttrib":
                {
                    if (!File.Exists(txtFtFileName.Text))
                    {
                        MessageBox.Show(string.Format("文件“{0}”不存在！", txtFtFileName.Text));
                        break;
                    }
                    var fi = new FileInfo(txtFtFileName.Text);
                    //var vA = chkFtA.Checked ? FileAttributes.Archive : FileAttributes.Normal;
                    fi.Attributes = chkFtA.Checked ? fi.Attributes | FileAttributes.Archive : fi.Attributes & (-1 - FileAttributes.Archive);
                    fi.Attributes = chkFtS.Checked ? fi.Attributes | FileAttributes.System : fi.Attributes & (-1 - FileAttributes.System);
                    fi.Attributes = chkFtH.Checked ? fi.Attributes | FileAttributes.Hidden : fi.Attributes & (-1 - FileAttributes.Hidden);
                    fi.Attributes = chkFtR.Checked ? fi.Attributes | FileAttributes.ReadOnly : fi.Attributes & (-1 - FileAttributes.ReadOnly);
                }
                    break;
                case "butFtUpdateCreateTime":
                {
                    if (!File.Exists(txtFtFileName.Text))
                    {
                        MessageBox.Show(string.Format("文件“{0}”不存在！", txtFtFileName.Text));
                        break;
                    }
                    var fi = new FileInfo(txtFtFileName.Text);
                    fi.CreationTime = dtFtCreateTime.Value;
                }
                    break;
                case "butFtUpdateModifyTime":
                    {
                        if (!File.Exists(txtFtFileName.Text))
                        {
                            MessageBox.Show(string.Format("文件“{0}”不存在！", txtFtFileName.Text));
                            break;
                        }
                        var fi = new FileInfo(txtFtFileName.Text);
                        fi.LastWriteTime = dtFtModifyTime.Value;
                    }
                    break;
                case "butFtUpdateAccessTime":
                    {
                        if (!File.Exists(txtFtFileName.Text))
                        {
                            MessageBox.Show(string.Format("文件“{0}”不存在！", txtFtFileName.Text));
                            break;
                        }
                        var fi = new FileInfo(txtFtFileName.Text);
                        fi.LastAccessTime = dtFtAccessTime.Value;
                    }
                    break;
            }
        }

        private void ButHttpTest_Click(object sender, EventArgs e)
        {
            Hashtable hash = new Hashtable();
            foreach (ListViewItem lvi in LstHttpTestReqHead.Items)
                hash.Add(lvi.SubItems[0].Text, lvi.SubItems[1].Text);
            TxtHttpTestResponse.Text = RadHttpTestGet.Checked
                ? HttpGet(TxtHttpTestUrl.Text, SelHttpTestContentType.Text, hash)
                : HttpPost(TxtHttpTestUrl.Text, TxtHttpTestPostData.Text, SelHttpTestContentType.Text, hash);
        }

        private void ButHttpTestReqHeadAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtHttpTestReqHeadKey.Text) || string.IsNullOrEmpty(TxtHttpTestReqHeadVal.Text))
                return;
            foreach (ListViewItem lvi in LstHttpTestReqHead.Items)
                if (lvi.SubItems[0].Text.Equals(TxtHttpTestReqHeadKey.Text))
                {
                    MessageBox.Show(string.Format("Key[{0}]已存在！", TxtHttpTestReqHeadKey.Text));
                    return;
                }
            LstHttpTestReqHead.Items.Add(
                new ListViewItem(new string[] {TxtHttpTestReqHeadKey.Text, TxtHttpTestReqHeadVal.Text}));
        }

        private void ButHttpTestReqHeadRmv_Click(object sender, EventArgs e)
        {
            if (LstHttpTestReqHead.SelectedItems.Count == 0)
                return;
            LstHttpTestReqHead.SelectedItems[0].Remove();
        }


        /// <summary>
        /// 向服务端发起GET请求，取得返回信息
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <returns></returns>
        private string HttpGet(string url, string contentType, Hashtable httpHeads)
        {
            Stream res = null;
            HttpWebResponse rsp = null;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.ContentType = contentType; //"application/x-www-form-urlencoded";
                SortedList list = new SortedList(httpHeads);
                foreach (DictionaryEntry item in list)
                    req.Headers.Add(item.Key.ToString(), item.Value.ToString());
                rsp = (HttpWebResponse)req.GetResponse();
                res = rsp.GetResponseStream();
                byte[] buff = new byte[4096];

                if (res != null)
                {
                    StringBuilder sb = new StringBuilder();
                    int readLen = res.Read(buff, 0, buff.Length);
                    while (readLen > 0)
                    {
                        sb.Append(Encoding.UTF8.GetString(buff, 0, readLen));
                        readLen = res.Read(buff, 0, buff.Length);
                    }
                    //Log("Debug", string.Format("[WeixinMsgFileScaner.HttpGet] 向URL地址[{0}]发起GET请求，返回信息：{1}", url, sb));
                    return sb.ToString();
                }
            }
            catch (Exception err)
            {
                return string.Format("[HttpGet] 发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
            }
            finally
            {
                if (res != null) res.Dispose();
                if (rsp != null) rsp.Close();
            }
            return "";
        }
        /// <summary>
        /// 向服务端发起Post请求，取得返回信息
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="data">Post的数据</param>
        /// <returns></returns>
        private string HttpPost(string url, string data, string contentType, Hashtable httpHeads)
        {
            Stream res = null;
            HttpWebResponse rsp = null;
            Stream sw = null;
            StreamWriter myWriter = null;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = contentType; //"application/x-www-form-urlencoded";
                SortedList list = new SortedList(httpHeads);
                foreach (DictionaryEntry item in list)
                    req.Headers.Add(item.Key.ToString(), item.Value.ToString());
                sw = req.GetRequestStream();
                myWriter = new StreamWriter(sw);
                myWriter.Write(data);
                myWriter.Close();
                myWriter.Dispose();
                myWriter = null;

                rsp = (HttpWebResponse)req.GetResponse();
                res = rsp.GetResponseStream();
                byte[] buff = new byte[4096];

                if (res != null)
                {
                    StringBuilder sb = new StringBuilder();
                    int readLen = res.Read(buff, 0, buff.Length);
                    while (readLen > 0)
                    {
                        sb.Append(Encoding.UTF8.GetString(buff, 0, readLen));
                        readLen = res.Read(buff, 0, buff.Length);
                    }
                    //Log("Debug", string.Format("[WeixinMsgFileScaner.HttpPost] 向URL地址[{0}]发起Post请求，返回信息：{1}", url, sb));
                    return sb.ToString();
                }
            }
            catch (Exception err)
            {
                return string.Format("[HttpPost] 发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
            }
            finally
            {
                if (res != null) res.Dispose();
                if (rsp != null) rsp.Close();
                if (sw != null) sw.Dispose();
                if (myWriter != null) myWriter.Dispose();
            }
            return "";
        }

    }
}