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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.GeneralLib;
using GingerCore.Helpers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Ginger.GeneralLib
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
            SetApplicationInfo();
            SetCreditInfo();
        }

        public void ShowAsWindow()
        {
            GenericWindow genWin = null;
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, eWindowShowStyle.Dialog, "About " + ApplicationInfo.ApplicationName, this);
        }

        private void SetApplicationInfo()
        {
            txtBlkApplicationInfo.Text = string.Empty;
            TextBlockHelper TBH = new TextBlockHelper(txtBlkApplicationInfo);
            SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$Color_DarkBlue")).ToString());

            //Application info
            TBH.AddFormattedText("Application:", foregroundColor, true);
            TBH.AddLineBreak();
            TBH.AddFormattedText(ApplicationInfo.ApplicationName, foregroundColor);
            TBH.AddLineBreak();
            TBH.AddFormattedText("Version: " + ApplicationInfo.ApplicationVersionWithInfo, foregroundColor);
            TBH.AddLineBreak();
            TBH.AddLineBreak();

            //Assembly Info
            TBH.AddFormattedText("Assembly Details:", foregroundColor, true);
            TBH.AddLineBreak();
            TBH.AddFormattedText("Build Time: " + ApplicationInfo.ApplicationBuildTime, foregroundColor);
        }

        private void SetCreditInfo()
        {
            txtCredit.Text = string.Empty;
            TextBlockHelper TBH = new TextBlockHelper(txtCredit);
            SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$Color_DarkBlue")).ToString());            

            TBH.AddFormattedText("Inventor, Chief Architect & Developer:", foregroundColor);
            TBH.AddLineBreak();
            TBH.AddFormattedText("Yaron Weiss", foregroundColor, true);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
        }

        private void HandleLinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }
    }
}
