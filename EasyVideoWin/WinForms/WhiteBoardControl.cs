using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.WinForms;

namespace EasyVideoWin.WinForms
{
    public partial class WhiteBoardControl : UserControl
    {
        public ChromiumWebBrowser browser { get; }
        public string name { set; get; }
        public WhiteBoardControl()
        {
            InitializeComponent();
            browser = new ChromiumWebBrowser("")
            {
                Dock = DockStyle.Fill,
            };
            toolStripContainer1.ContentPanel.Controls.Add(browser);

        }
    }
}
