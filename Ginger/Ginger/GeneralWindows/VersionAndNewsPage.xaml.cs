#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Ginger.GeneralWindows
{
    /// <summary>
    /// Interaction logic for VersionAndNewsPage.xaml
    /// </summary>
    public partial class VersionAndNewsPage : Page
    {
        GenericWindow _pageGenericWin = null;

        public VersionAndNewsPage()
        {
            InitializeComponent();

            xMessage.Content = Amdocs.Ginger.CoreNET.TelemetryLib.Telemetry.VersionAndNewsInfo;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            this.Width = 550;
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, null, true, "Cancel");
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo() { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
            e.Handled = true;
        }
    }
}
