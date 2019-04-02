using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.WinForms;

namespace EasyVideoWin.WinForms
{
    public partial class WhiteBoardForm : Form
    {
        public  ChromiumWebBrowser browser { get; }
        public WhiteBoardForm()
        {
            InitializeComponent();
            this.TopLevel = false;
            browser = new ChromiumWebBrowser("")
            {
                Dock = DockStyle.Fill,
            };
            toolStripContainer1.ContentPanel.Controls.Add(browser);
        }

        //public void LoadUrl(String url)
        //{
        //    browser.Load(url);
        //}

        //public void execJsCmd( String cmd)
        //{
        //    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(cmd); 
        //}

        //public bool canExecJsCmd()
        //{
        //    return browser.CanExecuteJavascriptInMainFrame;
        //}


    }
}
