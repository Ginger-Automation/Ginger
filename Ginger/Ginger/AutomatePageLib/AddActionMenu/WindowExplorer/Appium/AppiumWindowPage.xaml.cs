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

using System.Windows;
using System.Windows.Controls;
using GingerCore.Drivers.Appium;
using Ginger.UserControlsLib.TextEditor.XML;
using System.IO;

namespace Ginger.WindowExplorer.Appium
{
    /// <summary>
    /// Interaction logic for AppiumWindowPage.xaml
    /// </summary>
    public partial class AppiumWindowPage : Page
    {
        AppiumElementInfo mAppiumElementInfo;

        public AppiumWindowPage(AppiumElementInfo AEI)
        {
            mAppiumElementInfo = AEI;
            InitializeComponent();
        }  

        private void sourceXMLRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            ShowPageSource();
        }

        private void sourceXMLRadioBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowPageSource();
        }

        private void ShowPageSource()
        {
            if (sourceXMLRadioBtn.IsChecked == true)
            {
                XMLTextEditor e = new XMLTextEditor();
                MemoryStream ms = new MemoryStream();
                string tmp = System.IO.Path.GetTempFileName();
                mAppiumElementInfo.XmlDoc.Save(tmp);
                pageSourceXMLViewer.Init(tmp, e, false);
                pageSourceXMLViewer.Visibility = System.Windows.Visibility.Visible;
                pageSourceTextViewer.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                //Show simple text view
                pageSourceTextViewer.Text = mAppiumElementInfo.XmlDoc.OuterXml;
                pageSourceTextViewer.Visibility = System.Windows.Visibility.Visible;
                pageSourceXMLViewer.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
