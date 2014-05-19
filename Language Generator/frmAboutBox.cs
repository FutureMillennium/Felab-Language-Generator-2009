using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Language_Generator
{
    partial class frmAboutBox : Form
    {
        clsAssemblyInfo assInfo = new clsAssemblyInfo();

        public frmAboutBox()
        {
            InitializeComponent();

            string[] strComments = assInfo.AssemblyDescription.Split(';');
            this.Text += assInfo.AssemblyProduct;
            labelProductName.Text = assInfo.AssemblyTitle;
            labelVersion.Text = "Version " + assInfo.AssemblyVersion + strComments[3];
            labelCopyright.Text = assInfo.AssemblyCopyright;
            labelCompanyName.Text = assInfo.AssemblyCompany;
            labelComments.Text = strComments[0].Trim();
            labelSlogan.Text = strComments[1].Trim();
            linkLabel1.Text = strComments[2].Trim();
            linkLabel1.Links.Add(0, this.linkLabel1.Text.Length, strComments[2].Trim());
        }

        private void frmAboutBox_Load(object sender, EventArgs e)
        {
            this.Location = new Point(this.Owner.Location.X + ((this.Owner.Width - this.Width) / 2), this.Owner.Location.Y + ((this.Owner.Height - this.Height) / 2));
        }

        private void okButton_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.Links[linkLabel1.Links.IndexOf(e.Link)].Visited = true;
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }
    }
}
