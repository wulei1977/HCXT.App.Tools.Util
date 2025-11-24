using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;


namespace HCXT.App.Tools.Util
{
    /// <summary>
    /// 主窗体
    /// </summary>
    public partial class FrmMain : Form
    {
        private HttpListener httpListener;
        private Thread httpServerThread;
        private bool isHttpServerRunning = false;
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
            SelJsonFormat.SelectedIndex = 0;
            SelXmlFormat.SelectedIndex = 0;

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
            CbHttpTestReqEnc.Items.AddRange(arr);
            CbHttpTestResEnc.Items.AddRange(arr);
            CbHttpTestReqEnc.SelectedIndex = CbHttpTestReqEnc.Items.IndexOf("utf-8");
            CbHttpTestResEnc.SelectedIndex = CbHttpTestResEnc.Items.IndexOf("utf-8");
            CbGuidFormat.SelectedIndex = 0;
        }

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
                    //butFtClick(butFtLoad, e);
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
                case "ButFileSplitOutDir":
                    {
                        var fbd = new FolderBrowserDialog() { ShowNewFolderButton = true };
                        if (fbd.ShowDialog(this) != DialogResult.OK)
                            break;
                        TxtFileSplitOutDir.Text = fbd.SelectedPath;
                        fbd.Dispose();
                    }
                    break;
                case "ButFileSplit":
                    ButFileSplit.Enabled = false;
                    try
                    {
                        if (!File.Exists(txtFtFileName.Text))
                            throw new Exception("要切割的文件不存在！");
                        if (!Directory.Exists(TxtFileSplitOutDir.Text))
                            throw new Exception("输出文件夹不存在！");
                        bool bEnd = false;
                        var pcOutFile = 0;
                        long pcReadBytes = 0;
                        using (var fs = File.OpenRead(txtFtFileName.Text))
                        {
                            // MessageBox.Show("Size=" + fs.Length);
                            ProFileSplit.Value = 0;
                            while (!bEnd)
                            {
                                var sFo = string.Format("{0}\\{2:D5}-{1}", TxtFileSplitOutDir.Text, new FileInfo(txtFtFileName.Text).Name, ++pcOutFile);
                                using (var fo = File.OpenWrite(sFo))
                                {
                                    var pcWrtSize = 0;
                                    while (pcWrtSize < NudFileSplitBlockSize.Value)
                                    {
                                        var b = fs.ReadByte();
                                        if (b == -1)
                                        {
                                            bEnd = true;
                                            return;
                                        }
                                        fo.WriteByte(BitConverter.GetBytes(b)[0]);
                                        pcReadBytes++;
                                        if (++pcWrtSize % 10485760 == 0)
                                        {
                                            ProFileSplit.Value = Convert.ToInt32(pcReadBytes * 100 / fs.Length);
                                            ProFileSplit.Refresh();
                                            fo.Flush();
                                            Application.DoEvents();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.Message);
                    }
                    finally
                    {
                        ButFileSplit.Enabled = true;
                    }
                    break;
            }
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
                string fmt = CbGuidFormat.SelectedItem.ToString();
                sb.AppendLine(radGUIDUpper.Checked ? Guid.NewGuid().ToString(fmt).ToUpper() : Guid.NewGuid().ToString(fmt).ToLower());
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
                //sb.Append(Convert.ToString(b, 16));
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
        private void ButBase64Browse_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = false;
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                TxtBase64File.Text = openFileDialog1.FileName;
                RadBase64File.Checked = true;
            }
        }

        private void ButBase64EncodeClick(object sender, EventArgs e)
        {
            if (RadBase64File.Checked)
            {
                if (!File.Exists(TxtBase64File.Text))
                {
                    MessageBox.Show(string.Format("文件[{0}]不存在！", TxtBase64File.Text));
                    return;  
                }
                int buffLen = 1024*1024;
                FileInfo f = new FileInfo(TxtBase64File.Text);
                if (f.Length > buffLen)
                {
                    MessageBox.Show(string.Format("文件[{0}]长度大于1M字节，本程序不予转换。", TxtBase64File.Text));
                    return;  
                }
                byte[] buff = new byte[buffLen];
                FileStream fs = f.OpenRead();
                try
                {
                    int len = fs.Read(buff, 0, buffLen);
                    txtBase64Obj.Text = Convert.ToBase64String(buff, 0, len);
                }
                catch (Exception err)
                {
                    txtBase64Obj.Text = string.Format("发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
                }
                finally
                {
                    fs.Close();
                }
                return;
            }
            byte[] arr = Encoding.GetEncoding(cbBase64EncodingName.Text).GetBytes(txtBase64Src.Text);
            txtBase64Obj.Text = Convert.ToBase64String(arr);
        }

        private void ButBase64DecodeClick(object sender, EventArgs e)
        {
            if (RadBase64File.Checked)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                try
                {
                    byte[] arr = Convert.FromBase64String(txtBase64Obj.Text);
                    sfd.FileName = TxtBase64File.Text;
                    if (sfd.ShowDialog() != DialogResult.OK)
                        return;
                    File.WriteAllBytes(sfd.FileName, arr);
                }
                catch (Exception err)
                {
                    txtBase64Src.Text = string.Format("发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
                }
                finally
                {
                    sfd.Dispose();
                }
                return;
            }
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
                            if (dataToEncrypt.Length > 117)
                            {
                                MessageBox.Show("加密源数据长度不得超过117字节。");
                                return;
                            }
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

        #region 文件批量替换相关
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
            if (string.IsNullOrEmpty(txtReplaceFolder.Text) || !Directory.Exists(txtReplaceFolder.Text))
            {
                MessageBox.Show("请选择有效的源目录！");
                return;
            }
            if (string.IsNullOrEmpty(txtReplaceFilter.Text))
            {
                MessageBox.Show("请输入文件类型匹配规则！");
                return;
            }
            if (string.IsNullOrEmpty(cbReplaceEncoding.Text))
            {
                MessageBox.Show("请选择文件编码！");
                return;
            }

            butReplace.Enabled = false;
            txtReplaceHistory.Clear();
            int totalFiles = 0;
            int replacedFiles = 0;

            try
            {
                var encoding = Encoding.GetEncoding(cbReplaceEncoding.Text);
                var filters = txtReplaceFilter.Text.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                var searchOption = chkReplaceRecurse.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                LogReplace(string.Format("开始批量替换操作..."));
                LogReplace(string.Format("源目录：{0}", txtReplaceFolder.Text));
                LogReplace(string.Format("文件类型：{0}", txtReplaceFilter.Text));
                LogReplace(string.Format("文件编码：{0}", cbReplaceEncoding.Text));
                LogReplace(string.Format("包含子目录：{0}", chkReplaceRecurse.Checked ? "是" : "否"));
                LogReplace(string.Format("关键字：{0}", txtReplaceFrom.Text));
                LogReplace(string.Format("替换为：{0}", txtReplaceTo.Text));
                LogReplace("-----------------------------------");

                foreach (var filter in filters)
                {
                    var files = Directory.GetFiles(txtReplaceFolder.Text, filter.Trim(), searchOption);
                    foreach (var file in files)
                    {
                        totalFiles++;
                        try
                        {
                            var content = File.ReadAllText(file, encoding);
                            var searchText = txtReplaceFrom.Text;
                            var replaceText = txtReplaceTo.Text;
                            int index = 0;
                            int replaceCount = 0;
                            
                            while ((index = content.IndexOf(searchText, index)) != -1)
                            {
                                int line = 1;
                                int charPos = 0;
                                for (int i = 0; i < index; i++)
                                {
                                    if (content[i] == '\n')
                                    {
                                        line++;
                                        charPos = 0;
                                    }
                                    else
                                    {
                                        charPos++;
                                    }
                                }
                                
                                LogReplace(string.Format("[替换] {0} - 第{1}行第{2}字符", file, line, charPos + 1));
                                replaceCount++;
                                index += searchText.Length;
                            }
                            
                            if (replaceCount > 0)
                            {
                                var newContent = content.Replace(searchText, replaceText);
                                File.WriteAllText(file, newContent, encoding);
                                replacedFiles++;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogReplace(string.Format("[失败] {0} - {1}", file, ex.Message));
                        }
                    }
                }

                LogReplace("-----------------------------------");
                LogReplace(string.Format("操作完成！共处理 {0} 个文件，成功替换 {1} 个文件。", totalFiles, replacedFiles));
                MessageBox.Show(string.Format("批量替换完成！\n共处理 {0} 个文件\n成功替换 {1} 个文件", totalFiles, replacedFiles));
            }
            catch (Exception ex)
            {
                LogReplace(string.Format("发生异常：{0}", ex.Message));
                MessageBox.Show(string.Format("操作失败！\n{0}", ex.Message));
            }
            finally
            {
                butReplace.Enabled = true;
            }
        }

        private void LogReplace(string message)
        {
            txtReplaceHistory.AppendText(string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("HH:mm:ss"), message));
            txtReplaceHistory.Refresh();
            Application.DoEvents();
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

        #region HTTP测试相关
        private void ButHttpTest_Click(object sender, EventArgs e)
        {
            Hashtable hash = new Hashtable();
            foreach (ListViewItem lvi in LstHttpTestReqHead.Items)
                hash.Add(lvi.SubItems[0].Text, lvi.SubItems[1].Text);
            if (ChkHttpTestHttps.Checked)
            {
                TxtHttpTestResponse.Text = HttpsPost(TxtHttpTestUrl.Text, TxtHttpTestPostData.Text, SelHttpTestContentType.Text, hash);
                return;
            }
            if (!ChkHttpTestDownload.Checked)
            {
                TxtHttpTestResponse.Text = RadHttpTestGet.Checked
                    ? HttpGet(TxtHttpTestUrl.Text, SelHttpTestContentType.Text, hash)
                    : HttpPost(TxtHttpTestUrl.Text, TxtHttpTestPostData.Text, SelHttpTestContentType.Text, hash);
                return;
            }
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "请选择下载文件保存路径：";
            try
            {
                if (!fbd.ShowDialog().Equals(DialogResult.OK))
                    return;
                TxtHttpTestResponse.Text = RadHttpTestGet.Checked
                    ? HttpGetDownload(TxtHttpTestUrl.Text, SelHttpTestContentType.Text, hash, fbd.SelectedPath)
                    : HttpPostDownload(TxtHttpTestUrl.Text, TxtHttpTestPostData.Text, SelHttpTestContentType.Text, hash, fbd.SelectedPath);
            }
            catch (Exception err)
            {
                TxtHttpTestResponse.Text = string.Format("发生异常。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
            }
            finally
            {
                fbd.Dispose();
            }
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
                Encoding encRes = Encoding.GetEncoding(CbHttpTestResEnc.SelectedItem.ToString());
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
                        sb.Append(encRes.GetString(buff, 0, readLen));
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
                Encoding encReq = Encoding.GetEncoding(CbHttpTestReqEnc.SelectedItem.ToString());
                Encoding encRes = Encoding.GetEncoding(CbHttpTestResEnc.SelectedItem.ToString());
                myWriter = new StreamWriter(sw, encReq);
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
                        sb.Append(encRes.GetString(buff, 0, readLen));
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
        /// <summary>
        /// 向服务端发起GET请求，取得返回信息
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <returns></returns>
        private string HttpGetDownload(string url, string contentType, Hashtable httpHeads, string localFolder)
        {
            Stream res = null;
            HttpWebResponse rsp = null;
            FileStream fs = null;
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
                string fName = rsp.GetResponseHeader("Content-Disposition").Replace("filename=", "");
                if (fName.Trim().Equals(""))
                    fName = "NoName";
                fName = string.Format("{0}\\{1}", localFolder, fName).Replace(@"/", @"\").Replace(@"\\", @"\");
                byte[] buff = new byte[4096];
                if (res != null)
                {
                    int fileSize = 0;
                    fs = File.Create(fName);
                    int readLen = res.Read(buff, 0, buff.Length);
                    while (readLen > 0)
                    {
                        fs.Write(buff, 0, readLen);
                        fileSize += readLen;
                        readLen = res.Read(buff, 0, buff.Length);
                    }
                    return string.Format("文件[{0}]下载完成。(共计[{1}]字节)", fName, fileSize);
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
                if (fs != null) fs.Dispose();
            }
            return "";
        }
        /// <summary>
        /// 向服务端发起Post请求，取得返回信息
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="data">Post的数据</param>
        /// <returns></returns>
        private string HttpPostDownload(string url, string data, string contentType, Hashtable httpHeads, string localFolder)
        {
            Stream res = null;
            HttpWebResponse rsp = null;
            Stream sw = null;
            StreamWriter myWriter = null;
            FileStream fs = null;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = contentType; //"application/x-www-form-urlencoded";
                SortedList list = new SortedList(httpHeads);
                foreach (DictionaryEntry item in list)
                    req.Headers.Add(item.Key.ToString(), item.Value.ToString());
                sw = req.GetRequestStream();
                Encoding encReq = Encoding.GetEncoding(CbHttpTestReqEnc.SelectedItem.ToString());
                myWriter = new StreamWriter(sw, encReq);
                myWriter.Write(data);
                myWriter.Close();
                myWriter.Dispose();
                myWriter = null;
                rsp = (HttpWebResponse)req.GetResponse();
                res = rsp.GetResponseStream();
                string fName = rsp.GetResponseHeader("Content-Disposition").Replace("filename=", "");
                if (fName.Trim().Equals(""))
                    fName = "NoName";
                fName = string.Format("{0}\\{1}", localFolder, fName).Replace(@"/", @"\").Replace(@"\\", @"\");
                //MessageBox.Show("FileName=" + fName);
                byte[] buff = new byte[4096];
                if (res != null)
                {
                    int fileSize = 0;
                    fs = File.Create(fName);
                    int readLen = res.Read(buff, 0, buff.Length);
                    while (readLen > 0)
                    {
                        fs.Write(buff, 0, readLen);
                        fileSize += readLen;
                        readLen = res.Read(buff, 0, buff.Length);
                    }
                    return string.Format("文件[{0}]下载完成。(共计[{1}]字节)", fName, fileSize);
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
                if (fs != null) fs.Dispose();
            }
            return "";
        }

        /// <summary>
        /// HTTPS方式提交请求，并得到响应
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="data">Post的数据包</param>
        /// <returns></returns>
        private string HttpsPost(string url, string data, string contentType, Hashtable httpHeads)
        {
            const string logHead = "[HttpsPost]";
            Stream streamRequest = null;
            StreamWriter streamWriterReq = null;
            Stream streamResponse = null;
            StreamReader streamReaderRes = null;
            try
            {
                // 证书文件
                //var cert = @"C:\Users\Administrator\Documents\Tencent Files\18386685\FileRecv\server (1).crt"; // 配置文件中，[NotifyUrl]节点用来保存证书文件路径
                var cert = TxtHttpTestHttpsFile.Text;
                // 证书密码
                var password = TxtHttpTestHttpsPass.Text; // 配置文件中，[TelChargeUrl]节点用来保存证书密码
                var cer = password.Equals("") ? new X509Certificate2(cert) : new X509Certificate2(cert, password);
                var webrequest = (HttpWebRequest)WebRequest.Create(url);
                webrequest.ClientCertificates.Add(cer);
                webrequest.Method = "post";
                webrequest.ContentType = contentType; //"application/x-www-form-urlencoded";
                SortedList list = new SortedList(httpHeads);
                foreach (DictionaryEntry item in list)
                    webrequest.Headers.Add(item.Key.ToString(), item.Value.ToString());
                streamRequest = webrequest.GetRequestStream();
                Encoding encReq = Encoding.GetEncoding(CbHttpTestReqEnc.SelectedItem.ToString());
                Encoding encRes = Encoding.GetEncoding(CbHttpTestResEnc.SelectedItem.ToString());
                streamWriterReq = new StreamWriter(streamRequest, encReq);
                streamWriterReq.Write(data);
                streamWriterReq.Close();
                streamWriterReq.Dispose();
                streamWriterReq = null;

                var webreponse = (HttpWebResponse)webrequest.GetResponse();
                streamResponse = webreponse.GetResponseStream();
                if (streamResponse == null)
                    throw new Exception("获取ResponseStream失败");
                streamReaderRes = new StreamReader(streamResponse, encRes);
                var result = streamReaderRes.ReadToEnd();
                streamReaderRes.Close();
                streamReaderRes.Dispose();
                streamReaderRes = null;
                return result;
            }
            catch (Exception err)
            {
                return string.Format("{0}HTTPS提交数据时发生异常。异常信息：{1}\r\n堆栈：{2}", logHead, err.Message, err.StackTrace);
                // MessageBox.Show(err.ToString());
            }
            finally
            {
                if (streamRequest != null) streamRequest.Dispose();
                if (streamWriterReq != null) streamWriterReq.Dispose();
                if (streamResponse != null) streamResponse.Dispose();
                if (streamReaderRes != null) streamReaderRes.Dispose();
            }
            return "";
        }
        #endregion

        #region 格式化 JSON 相关
        private void ButFormatJson_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists("Newtonsoft.Json.Net20.dll"))
                {
                    MessageBox.Show("未找到JSON类库文件[Newtonsoft.Json.Net20.dll]，请获取该文件并复制到本工具程序所在文件夹。");
                    return;
                }
                Assembly ass = Assembly.LoadFrom("Newtonsoft.Json.Net20.dll");
                var serializerType = ass.GetType("Newtonsoft.Json.JsonSerializer");
                var serializer = Activator.CreateInstance(serializerType);
                TextReader tr = new StringReader(TxtFormatJsonSrc.Text);
                var jtr = Activator.CreateInstance(ass.GetType("Newtonsoft.Json.JsonTextReader"), tr);
                MethodInfo deserializeMethod = null;
                foreach (var m in serializerType.GetMethods())
                {
                    if (m.Name == "Deserialize" && m.GetParameters().Length == 1)
                    {
                        deserializeMethod = m;
                        break;
                    }
                }
                var obj = deserializeMethod.Invoke(serializer, new[] { jtr });
                if (obj != null)
                {
                    string format = SelJsonFormat.SelectedItem?.ToString() ?? "JSON";
                    StringWriter sw = new StringWriter();
                    
                    switch (format)
                    {
                        case "XML":
                            ConvertJsonToXml(obj, sw, ass);
                            break;
                        case "YAML":
                            ConvertJsonToYaml(obj, sw);
                            break;
                        case "TOON":
                            ConvertJsonToToon(obj, sw);
                            break;
                        default: // JSON
                            var jtw = Activator.CreateInstance(ass.GetType("Newtonsoft.Json.JsonTextWriter"), sw);
                            var jtwType = jtw.GetType();
                            jtwType.GetProperty("Formatting").SetValue(jtw, Enum.Parse(ass.GetType("Newtonsoft.Json.Formatting"), "Indented"), null);
                            jtwType.GetProperty("Indentation").SetValue(jtw, 4, null);
                            jtwType.GetProperty("IndentChar").SetValue(jtw, ' ', null);
                            MethodInfo serializeMethod = null;
                            var jsonWriterType = ass.GetType("Newtonsoft.Json.JsonWriter");
                            foreach (var m in serializerType.GetMethods())
                            {
                                if (m.Name == "Serialize" && m.GetParameters().Length == 2)
                                {
                                    var ps = m.GetParameters();
                                    if (ps[0].ParameterType == jsonWriterType)
                                    {
                                        serializeMethod = m;
                                        break;
                                    }
                                }
                            }
                            serializeMethod.Invoke(serializer, new[] { jtw, obj });
                            break;
                    }
                    TxtFormatJsonObj.Text = sw.ToString();
                }
                else
                {
                    TxtFormatJsonObj.Text = TxtFormatJsonSrc.Text;
                }
            }
            catch (Exception err)
            {
                TxtFormatJsonObj.Text = string.Format("格式化失败。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
            }
        }

        private void ConvertJsonToXml(object obj, StringWriter sw, Assembly jsonAssembly)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("root");
            doc.AppendChild(root);
            ConvertObjectToXml(obj, root, doc);
            XmlTextWriter xtw = new XmlTextWriter(sw) { Formatting = Formatting.Indented, Indentation = 4, IndentChar = ' ' };
            doc.Save(xtw);
        }

        private void ConvertObjectToXml(object obj, XmlElement parent, XmlDocument doc)
        {
            if (obj == null) return;
            var objType = obj.GetType();
            if (objType.Name.Contains("JObject"))
            {
                foreach (var prop in (IEnumerable)objType.GetMethod("Properties").Invoke(obj, null))
                {
                    var name = prop.GetType().GetProperty("Name").GetValue(prop, null).ToString();
                    var value = prop.GetType().GetProperty("Value").GetValue(prop, null);
                    XmlElement elem = doc.CreateElement(name);
                    ConvertObjectToXml(value, elem, doc);
                    parent.AppendChild(elem);
                }
            }
            else if (objType.Name.Contains("JArray"))
            {
                int index = 0;
                foreach (var item in (IEnumerable)obj)
                {
                    XmlElement elem = doc.CreateElement("item" + index++);
                    ConvertObjectToXml(item, elem, doc);
                    parent.AppendChild(elem);
                }
            }
            else if (objType.Name.Contains("JValue"))
            {
                var value = objType.GetProperty("Value").GetValue(obj, null);
                parent.InnerText = value?.ToString() ?? "";
            }
            else
            {
                parent.InnerText = obj.ToString();
            }
        }

        private void ConvertJsonToYaml(object obj, StringWriter sw)
        {
            ConvertObjectToYaml(obj, sw, 0);
        }

        private void ConvertObjectToYaml(object obj, StringWriter sw, int indent)
        {
            if (obj == null)
            {
                sw.Write("null");
                return;
            }
            var objType = obj.GetType();
            string indentStr = new string(' ', indent * 2);
            if (objType.Name.Contains("JObject"))
            {
                bool first = true;
                foreach (var prop in (IEnumerable)objType.GetMethod("Properties").Invoke(obj, null))
                {
                    if (!first) sw.WriteLine();
                    first = false;
                    var name = prop.GetType().GetProperty("Name").GetValue(prop, null).ToString();
                    var value = prop.GetType().GetProperty("Value").GetValue(prop, null);
                    sw.Write(indentStr + name + ": ");
                    if (value != null && (value.GetType().Name.Contains("JObject") || value.GetType().Name.Contains("JArray")))
                    {
                        sw.WriteLine();
                        ConvertObjectToYaml(value, sw, indent + 1);
                    }
                    else
                    {
                        ConvertObjectToYaml(value, sw, 0);
                    }
                }
            }
            else if (objType.Name.Contains("JArray"))
            {
                bool first = true;
                foreach (var item in (IEnumerable)obj)
                {
                    if (!first) sw.WriteLine();
                    first = false;
                    sw.Write(indentStr + "- ");
                    if (item != null && (item.GetType().Name.Contains("JObject") || item.GetType().Name.Contains("JArray")))
                    {
                        sw.WriteLine();
                        ConvertObjectToYaml(item, sw, indent + 1);
                    }
                    else
                    {
                        ConvertObjectToYaml(item, sw, 0);
                    }
                }
            }
            else if (objType.Name.Contains("JValue"))
            {
                var value = objType.GetProperty("Value").GetValue(obj, null);
                if (value is string)
                    sw.Write("\"" + value.ToString().Replace("\"", "\\\"") + "\"");
                else
                    sw.Write(value?.ToString() ?? "null");
            }
            else
            {
                sw.Write(obj.ToString());
            }
        }

        private void ConvertJsonToToon(object obj, StringWriter sw)
        {
            ConvertObjectToToon(obj, sw, 0);
        }

        private void ConvertObjectToToon(object obj, StringWriter sw, int indent)
        {
            if (obj == null)
            {
                sw.Write("null");
                return;
            }
            var objType = obj.GetType();
            string indentStr = new string(' ', indent * 4);
            if (objType.Name.Contains("JObject"))
            {
                sw.WriteLine("{");
                bool first = true;
                foreach (var prop in (IEnumerable)objType.GetMethod("Properties").Invoke(obj, null))
                {
                    if (!first) sw.WriteLine(",");
                    first = false;
                    var name = prop.GetType().GetProperty("Name").GetValue(prop, null).ToString();
                    var value = prop.GetType().GetProperty("Value").GetValue(prop, null);
                    sw.Write(indentStr + "    " + name + " = ");
                    ConvertObjectToToon(value, sw, indent + 1);
                }
                sw.WriteLine();
                sw.Write(indentStr + "}");
            }
            else if (objType.Name.Contains("JArray"))
            {
                sw.WriteLine("[");
                bool first = true;
                foreach (var item in (IEnumerable)obj)
                {
                    if (!first) sw.WriteLine(",");
                    first = false;
                    sw.Write(indentStr + "    ");
                    ConvertObjectToToon(item, sw, indent + 1);
                }
                sw.WriteLine();
                sw.Write(indentStr + "]");
            }
            else if (objType.Name.Contains("JValue"))
            {
                var value = objType.GetProperty("Value").GetValue(obj, null);
                if (value is string)
                    sw.Write("\"" + value.ToString().Replace("\"", "\\\"") + "\"");
                else
                    sw.Write(value?.ToString() ?? "null");
            }
            else
            {
                sw.Write(obj.ToString());
            }
        }
        #endregion

        #region 格式化 XML 相关
        private void ButFormatXml_Click(object sender, EventArgs e)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(TxtFormatXmlSrc.Text);
                string format = SelXmlFormat.SelectedItem?.ToString() ?? "XML";
                StringWriter sw = new StringWriter();
                
                switch (format)
                {
                    case "JSON":
                        ConvertXmlToJson(doc, sw);
                        break;
                    case "YAML":
                        ConvertXmlToYaml(doc, sw);
                        break;
                    case "TOON":
                        ConvertXmlToToon(doc, sw);
                        break;
                    default: // XML
                        XmlTextWriter xtw = new XmlTextWriter(sw) { Formatting = Formatting.Indented, Indentation = 4, IndentChar = ' ' };
                        doc.Save(xtw);
                        break;
                }
                TxtFormatXmlObj.Text = sw.ToString();
            }
            catch (Exception err)
            {
                TxtFormatXmlObj.Text = string.Format("格式化失败。异常信息：{0}\r\n堆栈：{1}", err.Message, err.StackTrace);
            }
        }

        private void ConvertXmlToJson(XmlDocument doc, StringWriter sw)
        {
            sw.Write("{");
            ConvertXmlNodeToJson(doc.DocumentElement, sw, 1);
            sw.WriteLine();
            sw.Write("}");
        }

        private void ConvertXmlNodeToJson(XmlNode node, StringWriter sw, int indent)
        {
            if (node == null) return;
            string indentStr = new string(' ', indent * 4);
            sw.WriteLine();
            sw.Write(indentStr + "\"" + node.Name + "\": ");
            if (node.HasChildNodes && node.FirstChild.NodeType == XmlNodeType.Element)
            {
                sw.Write("{");
                bool first = true;
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        if (!first) sw.Write(",");
                        first = false;
                        ConvertXmlNodeToJson(child, sw, indent + 1);
                    }
                }
                sw.WriteLine();
                sw.Write(indentStr + "}");
            }
            else
            {
                string value = node.InnerText.Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
                sw.Write("\"" + value + "\"");
            }
        }

        private void ConvertXmlToYaml(XmlDocument doc, StringWriter sw)
        {
            ConvertXmlNodeToYaml(doc.DocumentElement, sw, 0);
        }

        private void ConvertXmlNodeToYaml(XmlNode node, StringWriter sw, int indent)
        {
            if (node == null) return;
            string indentStr = new string(' ', indent * 2);
            sw.Write(indentStr + node.Name + ": ");
            if (node.HasChildNodes && node.FirstChild.NodeType == XmlNodeType.Element)
            {
                sw.WriteLine();
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        ConvertXmlNodeToYaml(child, sw, indent + 1);
                    }
                }
            }
            else
            {
                sw.WriteLine("\"" + node.InnerText.Replace("\"", "\\\"") + "\"");
            }
        }

        private void ConvertXmlToToon(XmlDocument doc, StringWriter sw)
        {
            ConvertXmlNodeToToon(doc.DocumentElement, sw, 0);
        }

        private void ConvertXmlNodeToToon(XmlNode node, StringWriter sw, int indent)
        {
            if (node == null) return;
            string indentStr = new string(' ', indent * 4);
            sw.Write(indentStr + node.Name + " = ");
            if (node.HasChildNodes && node.FirstChild.NodeType == XmlNodeType.Element)
            {
                sw.WriteLine("{");
                bool first = true;
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        if (!first) sw.WriteLine(",");
                        first = false;
                        ConvertXmlNodeToToon(child, sw, indent + 1);
                    }
                }
                sw.WriteLine();
                sw.Write(indentStr + "}");
            }
            else
            {
                sw.Write("\"" + node.InnerText.Replace("\"", "\\\"") + "\"");
            }
        }
        #endregion

        #region HTTP服务相关
        private void ButHttpServBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                TxtHttpServPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void ButHttpServRun_Click(object sender, EventArgs e)
        {
            if (!isHttpServerRunning)
            {
                StartHttpServer();
            }
            else
            {
                StopHttpServer();
            }
        }

        private void StartHttpServer()
        {
            try
            {
                if (string.IsNullOrEmpty(TxtHttpServIp.Text) || TxtHttpServIp.Text.Trim() == "")
                {
                    LogHttpServer("错误：请输入监听IP地址");
                    return;
                }

                if (!Directory.Exists(TxtHttpServPath.Text))
                {
                    LogHttpServer("错误：指定的目录不存在");
                    return;
                }

                if (!ChkHttpServAnonymous.Checked)
                {
                    if (string.IsNullOrEmpty(TxtHttpServUser.Text) || TxtHttpServUser.Text.Trim() == "" || string.IsNullOrEmpty(TxtHttpServPass.Text) || TxtHttpServPass.Text.Trim() == "")
                    {
                        LogHttpServer("错误：非匿名访问时必须填写用户名和密码");
                        return;
                    }
                }

                httpListener = new HttpListener();
                string ip = TxtHttpServIp.Text.Trim();
                if (ip == "localhost" || ip == "127.0.0.1")
                {
                    ip = "localhost";
                }
                else if (ip == "0.0.0.0" || ip == "*")
                {
                    ip = "+";
                }
                string prefix = string.Format("http://{0}:{1}/", ip, NudHttpServPort.Value);
                httpListener.Prefixes.Add(prefix);

                if (!ChkHttpServAnonymous.Checked)
                {
                    httpListener.AuthenticationSchemes = AuthenticationSchemes.Basic;
                }

                httpListener.Start();
                isHttpServerRunning = true;

                ButHttpServRun.Text = "已启动，点击停止";
                ButHttpServRun.ForeColor = Color.DarkGreen;

                TxtHttpServIp.Enabled = false;
                NudHttpServPort.Enabled = false;
                TxtHttpServPath.Enabled = false;
                ButHttpServBrowse.Enabled = false;
                ChkHttpServAnonymous.Enabled = false;
                TxtHttpServUser.Enabled = false;
                TxtHttpServPass.Enabled = false;

                LogHttpServer(string.Format("HTTP服务已启动，监听地址：{0}", prefix));
                LogHttpServer(string.Format("映射目录：{0}", TxtHttpServPath.Text));

                httpServerThread = new Thread(ProcessHttpRequests);
                httpServerThread.IsBackground = true;
                httpServerThread.Start();
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 5)
                {
                    LogHttpServer("启动失败：需要管理员权限。请以管理员身份运行程序，或使用localhost作为监听地址。");
                }
                else
                {
                    LogHttpServer(string.Format("启动失败：{0} (错误代码:{1})", ex.Message, ex.ErrorCode));
                }
                isHttpServerRunning = false;
            }
            catch (Exception ex)
            {
                LogHttpServer(string.Format("启动失败：{0}", ex.Message));
                isHttpServerRunning = false;
            }
        }

        private void StopHttpServer()
        {
            try
            {
                if (httpListener != null)
                {
                    httpListener.Stop();
                    httpListener.Close();
                    httpListener = null;
                }

                isHttpServerRunning = false;

                ButHttpServRun.Text = "已停止，点击启动";
                ButHttpServRun.ForeColor = Color.Black;

                TxtHttpServIp.Enabled = true;
                NudHttpServPort.Enabled = true;
                TxtHttpServPath.Enabled = true;
                ButHttpServBrowse.Enabled = true;
                ChkHttpServAnonymous.Enabled = true;
                TxtHttpServUser.Enabled = true;
                TxtHttpServPass.Enabled = true;

                LogHttpServer("HTTP服务已停止");
            }
            catch (Exception ex)
            {
                LogHttpServer(string.Format("停止失败：{0}", ex.Message));
            }
        }

        private void ProcessHttpRequests()
        {
            while (isHttpServerRunning)
            {
                try
                {
                    HttpListenerContext context = httpListener.GetContext();
                    ThreadPool.QueueUserWorkItem(state => HandleRequest(context));
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LogHttpServer(string.Format("处理请求异常：{0}", ex.Message));
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            try
            {
                if (!ChkHttpServAnonymous.Checked)
                {
                    if (!ValidateCredentials(context.User))
                    {
                        response.StatusCode = 401;
                        response.AddHeader("WWW-Authenticate", "Basic realm=\"HTTP Server\"");
                        LogHttpServer(string.Format("未授权访问：{0} {1}", request.HttpMethod, request.Url.AbsolutePath));
                        try { response.Close(); } catch { }
                        return;
                    }
                }

                string urlPath = request.Url.AbsolutePath.TrimStart('/');
                urlPath = Uri.UnescapeDataString(urlPath);
                string filePath = Path.Combine(TxtHttpServPath.Text, urlPath);

                if (Directory.Exists(filePath))
                {
                    string[] defaultFiles = { "index.html", "index.htm", "default.html", "default.htm" };
                    bool foundDefault = false;
                    foreach (string defaultFile in defaultFiles)
                    {
                        string defaultPath = Path.Combine(filePath, defaultFile);
                        if (File.Exists(defaultPath))
                        {
                            filePath = defaultPath;
                            foundDefault = true;
                            break;
                        }
                    }

                    if (!foundDefault)
                    {
                        SendDirectoryListing(response, filePath, urlPath);
                        LogHttpServer(string.Format("{0} {1} - 200 (目录列表)", request.HttpMethod, request.Url.AbsolutePath));
                        return;
                    }
                }

                if (!File.Exists(filePath))
                {
                    response.StatusCode = 404;
                    byte[] buffer = Encoding.UTF8.GetBytes("<html><body><h1>404 - File Not Found</h1></body></html>");
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    LogHttpServer(string.Format("{0} {1} - 404", request.HttpMethod, request.Url.AbsolutePath));
                }
                else
                {
                    HttpServerOptimization.SendFileWithRangeSupport(response, request, filePath, LogHttpServer);
                    /*
                    byte[] fileBytes = File.ReadAllBytes(filePath);
                    response.ContentType = GetContentType(filePath);
                    response.ContentLength64 = fileBytes.Length;
                    response.StatusCode = 200;
                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                    LogHttpServer(string.Format("{0} {1} - 200 ({2} bytes)", request.HttpMethod, request.Url.AbsolutePath, fileBytes.Length));
                    */
                }
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException || ex is HttpListenerException)
                    return;
                try
                {
                    response.StatusCode = 500;
                }
                catch { }
                LogHttpServer(string.Format("处理请求错误：{0}", ex.Message));
            }
            finally
            {
                try
                {
                    response.Close();
                }
                catch { }
            }
        }

        private bool ValidateCredentials(System.Security.Principal.IPrincipal user)
        {
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return false;

            HttpListenerBasicIdentity identity = user.Identity as HttpListenerBasicIdentity;
            if (identity == null)
                return false;

            return identity.Name == TxtHttpServUser.Text && identity.Password == TxtHttpServPass.Text;
        }

        private void SendDirectoryListing(HttpListenerResponse response, string dirPath, string urlPath)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<html><head><meta charset='utf-8'><title>目录列表</title></head><body>");
            html.AppendFormat("<h1>目录：/{0}</h1><hr/>", urlPath);
            html.Append("<ul>");

            if (!string.IsNullOrEmpty(urlPath))
            {
                string parentPath = Path.GetDirectoryName(urlPath.Replace('/', '\\'));
                if (parentPath == null) parentPath = "";
                parentPath = parentPath.Replace('\\', '/');
                html.AppendFormat("<li><a href='/{0}'>[上级目录]</a></li>", parentPath);
            }

            foreach (string dir in Directory.GetDirectories(dirPath))
            {
                string dirName = Path.GetFileName(dir);
                string dirUrl = string.IsNullOrEmpty(urlPath) ? dirName : urlPath + "/" + dirName;
                html.AppendFormat("<li><a href='/{0}/'>[{1}]</a></li>", dirUrl, dirName);
            }

            foreach (string file in Directory.GetFiles(dirPath))
            {
                string fileName = Path.GetFileName(file);
                string fileUrl = string.IsNullOrEmpty(urlPath) ? fileName : urlPath + "/" + fileName;
                FileInfo fi = new FileInfo(file);
                html.AppendFormat("<li><a href='/{0}'>{1}</a> ({2} bytes)</li>", fileUrl, fileName, fi.Length);
            }

            html.Append("</ul></body></html>");

            byte[] buffer = Encoding.UTF8.GetBytes(html.ToString());
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = 200;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        private string GetContentType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            switch (ext)
            {
                case ".html":
                case ".htm":
                    return "text/html";
                case ".css":
                    return "text/css";
                case ".js":
                    return "application/javascript";
                case ".json":
                    return "application/json";
                case ".xml":
                    return "text/xml";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".ico":
                    return "image/x-icon";
                case ".txt":
                    return "text/plain";
                case ".pdf":
                    return "application/pdf";
                case ".zip":
                    return "application/zip";
                default:
                    return "application/octet-stream";
            }
        }

        private void LogHttpServer(string message)
        {
            if (TxtHttpServLog.InvokeRequired)
            {
                TxtHttpServLog.Invoke(new Action<string>(LogHttpServer), message);
            }
            else
            {
                string logMessage = string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message);
                TxtHttpServLog.AppendText(logMessage);
            }
        }

        /// <summary>
        /// 支持 HTTP Range 请求的文件传输，用于断点续传和多线程下载
        /// </summary>
        private void SendFileWithRangeSupport(HttpListenerResponse response, HttpListenerRequest request, string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;
            const int bufferSize = 65536;

            response.ContentType = GetContentType(filePath);
            response.AddHeader("Accept-Ranges", "bytes");
            response.AddHeader("Last-Modified", fileInfo.LastWriteTime.ToString("R"));

            string rangeHeader = request.Headers["Range"];

            if (string.IsNullOrEmpty(rangeHeader))
            {
                // 不支持 Range，返回整个文件
                response.StatusCode = 200;
                response.ContentLength64 = fileSize;
                SendFileStream(response, filePath, 0, fileSize, bufferSize);
                LogHttpServer(string.Format("{0} {1} - 200 ({2} bytes)", request.HttpMethod, request.Url.AbsolutePath, fileSize));
            }
            else
            {
                // 解析 Range 头并处理分片请求
                if (ParseRangeHeader(rangeHeader, fileSize, out long rangeStart, out long rangeEnd))
                {
                    response.StatusCode = 206;
                    response.ContentLength64 = rangeEnd - rangeStart + 1;
                    response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", rangeStart, rangeEnd, fileSize));
                    SendFileStream(response, filePath, rangeStart, rangeEnd - rangeStart + 1, bufferSize);
                    LogHttpServer(string.Format("{0} {1} - 206 (Partial Content: {2}-{3})", request.HttpMethod, request.Url.AbsolutePath, rangeStart, rangeEnd));
                }
                else
                {
                    // Range 无效，返回 416 错误
                    response.StatusCode = 416;
                    response.AddHeader("Content-Range", string.Format("bytes */{0}", fileSize));
                    LogHttpServer(string.Format("{0} {1} - 416 (Range Not Satisfiable)", request.HttpMethod, request.Url.AbsolutePath));
                }
            }
        }

        /// <summary>
        /// 使用流式传输发送文件，避免全量加载到内存
        /// </summary>
        private void SendFileStream(HttpListenerResponse response, string filePath, long offset, long length, int bufferSize)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);
                if (offset > 0)
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                }

                byte[] buffer = new byte[bufferSize];
                long remainingBytes = length;
                int readBytes;

                while (remainingBytes > 0)
                {
                    int bytesToRead = (int)Math.Min(bufferSize, remainingBytes);
                    readBytes = fs.Read(buffer, 0, bytesToRead);

                    if (readBytes == 0)
                        break;

                    response.OutputStream.Write(buffer, 0, readBytes);
                    remainingBytes -= readBytes;
                }
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }
        }

        /// <summary>
        /// 解析 HTTP Range 头，支持多种格式：bytes=0-100, bytes=100-, bytes=-100
        /// </summary>
        private bool ParseRangeHeader(string rangeHeader, long fileSize, out long rangeStart, out long rangeEnd)
        {
            rangeStart = 0;
            rangeEnd = fileSize - 1;

            try
            {
                if (!rangeHeader.StartsWith("bytes="))
                    return false;

                string range = rangeHeader.Substring(6);

                if (range.Contains("-"))
                {
                    string[] parts = range.Split('-');

                    if (parts.Length == 2)
                    {
                        if (string.IsNullOrEmpty(parts[0]))
                        {
                            // bytes=-500 (最后500字节)
                            if (long.TryParse(parts[1], out long suffixLength) && suffixLength > 0)
                            {
                                rangeStart = Math.Max(0, fileSize - suffixLength);
                            }
                            else
                                return false;
                        }
                        else if (string.IsNullOrEmpty(parts[1]))
                        {
                            // bytes=100- (从100到末尾)
                            if (long.TryParse(parts[0], out rangeStart) && rangeStart < fileSize)
                            {
                                rangeEnd = fileSize - 1;
                            }
                            else
                                return false;
                        }
                        else
                        {
                            // bytes=100-200
                            if (long.TryParse(parts[0], out rangeStart) && long.TryParse(parts[1], out rangeEnd))
                            {
                                if (rangeStart > rangeEnd || rangeStart >= fileSize)
                                    return false;
                                rangeEnd = Math.Min(rangeEnd, fileSize - 1);
                            }
                            else
                                return false;
                        }

                        return rangeStart >= 0 && rangeEnd >= rangeStart && rangeEnd < fileSize;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
        #endregion

        #region Email相关
        private void butEmailSend_Click(object sender, EventArgs e)
        {
            const string jiong = "此功能还没写完，最近没时间弄。有时间再说吧。\r\n囧.....";
            MessageBox.Show(jiong);
            var frmAbout = new FrmAbout();
            frmAbout.ShowDialog(this);
            frmAbout.Dispose();
            //return;
        }
        #endregion
    }
}