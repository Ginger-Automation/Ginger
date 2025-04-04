#region License
/*
Copyright © 2014-2025 European Support Limited

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

using GingerCore.Drivers;
using System.Windows;
using System.Windows.Controls;

namespace UnitTests.UITests
{
    /// <summary>
    /// Interaction logic for MiniBrowserWindow.xaml
    /// </summary>
    public partial class MiniBrowserWindow : Window
    {

        WebBrowserPage WBP;
        public WebBrowser browser { get { return WBP.GetBrowser(); } }

        public MiniBrowserWindow()
        {
            InitializeComponent();

            WBP = new WebBrowserPage();
            BrowserFrame.Content = WBP;
        }

        public string GetElementXPath(mshtml.IHTMLElement element)
        {
            return WBP.GetElementXPath(element);
        }

        public mshtml.IHTMLElement GetElementByXPath(string Xpath)
        {
            return WBP.GetElementByXPath(Xpath);
        }
    }
}
