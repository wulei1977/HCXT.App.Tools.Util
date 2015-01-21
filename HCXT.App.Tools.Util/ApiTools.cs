using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace HCXT.App.Tools.Util
{
    /// <summary>
    /// 一些调用API函数的工具方法
    /// </summary>
    public class ApiTools
    {
        #region 获取exe文件的图标 GetIcon(string path)
        /// <summary>
        /// 引入"Shell32.dll"
        /// </summary>
        /// <param name="pszPath"></param>
        /// <param name="dwFileAttributes"></param>
        /// <param name="psfi"></param>
        /// <param name="cbfileInfo"></param>
        /// <param name="uFlags"></param>
        /// <returns></returns>
        [DllImport("Shell32.dll")]
        private static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbfileInfo, SHGFI uFlags);
        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct SHFILEINFO
        {
// ReSharper disable FieldCanBeMadeReadOnly.Local
            public IntPtr hIcon;
            public int iIcon;
            public uint attributes;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 80)]
            public string szTypeName;
            public SHFILEINFO(bool b)
            {
                hIcon = IntPtr.Zero;
                iIcon = 0;
                attributes = 0;
                szDisplayName = "";
                szTypeName = "";
            }
// ReSharper restore FieldCanBeMadeReadOnly.Local
        }
        /// <summary>
        /// 
        /// </summary>
        private enum SHGFI
        {
            SmallIcon = 0x00000001,
            LargeIcon = 0x00000020,
            Icon = 0x00000100,
            DisplayName = 0x00000200,
            TypeName = 0x00000400,
            SysIconIndex = 0x00004000,
            UseFileAttributes = 0x00000010
        }
        /// <summary>
        /// 获取exe文件的图标
        /// </summary>
        /// <param name="path"></param>
        /// <param name="small"></param>
        /// <returns></returns>
        public static Icon GetIcon(string path, bool small)
        {
            SHFILEINFO info = new SHFILEINFO(true);
            int cbFileInfo = Marshal.SizeOf(info);
            SHGFI flags;
            if (small)
            {
                flags = SHGFI.Icon | SHGFI.SmallIcon | SHGFI.UseFileAttributes;
            }
            else
            {
                flags = SHGFI.Icon | SHGFI.LargeIcon | SHGFI.UseFileAttributes;
            }
            SHGetFileInfo(path, 256, out info, (uint)cbFileInfo, flags);
            return Icon.FromHandle(info.hIcon);
        }
        /*
        /// <summary>
        /// 获取exe文件的图标
        /// </summary>
        /// <param name="path"></param>
        public static void GetIcon(string path)
        {
            SHFILEINFO info = new SHFILEINFO(true);
            int cbFileInfo = Marshal.SizeOf(info);
            SHGFI flags;

            flags = SHGFI.Icon | SHGFI.SmallIcon | SHGFI.UseFileAttributes;

            SHGetFileInfo(path, 256, out info, (uint)cbFileInfo, flags);
            MessageBox.Show(info.attributes.ToString());
            MessageBox.Show(info.hIcon.ToString());
            MessageBox.Show(info.iIcon.ToString());
            MessageBox.Show(info.szDisplayName);
            MessageBox.Show(info.szTypeName);
        }
         */
        #endregion
    }
}