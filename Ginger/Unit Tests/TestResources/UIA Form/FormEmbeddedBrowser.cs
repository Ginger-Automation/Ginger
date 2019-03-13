#region License
/*
Copyright © 2014-2019 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GingerUnitTests.UIA_Form
{
    public partial class FormEmbeddedBrowser : Form
    {
        public FormEmbeddedBrowser()
        {
            InitializeComponent();
            string filepath = @"C:\Users\chitt_000\Documents\Visual Studio 2013\Projects\NextGenWPF\Devs\GingerNextVer_Dev\GingerUnitTester\UIA Form\Browser.html";
            //Uri uri = new Uri("..\\test.html", UriKind.Relative);
            Uri uri = new Uri(filepath);
            webBrowser1.Navigate(uri);
        }

        private void menuItem1ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
