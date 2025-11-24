using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HCXT.App.Tools.Util
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());

            //var tel = "";
            //MessageBox.Show(string.Format("IsDig({0})={1}", tel, IsDig(tel)));
        }

        private static bool IsDig(string str)
        {
            if(string.IsNullOrEmpty(str))
                return false;
            foreach (char c in str)
                if (c < '0' || c > '9')
                    return false;
            return true;
        }
    }
}