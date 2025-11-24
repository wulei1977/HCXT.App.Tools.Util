using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace HCXT.App.Tools.Util
{
    public partial class FrmAbout : Form
    {
        private string TopCaption = "About " + Application.ProductName;

        public FrmAbout()
        {
            InitializeComponent();

            productVersionLabel.Text = Application.ProductVersion;
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            TxtHis.Text = ((AssemblyDescriptionAttribute) attributes[0]).Description;
        }

        public FrmAbout(string TopCaption, string Link)
        {
            InitializeComponent();
            this.productNameLabel.Text = Application.ProductName.Length <= 0 ? "{Product Name}" : Application.ProductName;

            this.TopCaption = TopCaption;
            this.linkLabel.Text = Link;
        }

        private void topPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawIcon(Icon.ExtractAssociatedIcon(Application.ExecutablePath), 20, 8);
            e.Graphics.DrawString(TopCaption, new Font("Segoe UI", 14f), Brushes.Azure, new PointF(70, 10));
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var ll = (LinkLabel) sender;
            System.Diagnostics.Process.Start(ll.Text);
        }

    }
}
