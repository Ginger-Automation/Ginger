#region License
/*
Copyright © 2014-2023 European Support Limited

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

using GingerWPF;
using GingerWPF.SolutionLib;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Ginger
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class NewStartPage : Page
    {
        public NewStartPage()
        {
            InitializeComponent();

            //TODO: load from external - so easier to update
            lblAppVersion.Content = "Version ZZZZZZZZZZZZZ";  //+ Ginger.App.AppVersion;
            
            SolutionsFrame.SetContent(new RecentSolutionsPage());
        }

        private void HandleLinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo() { FileName = navigateUri, UseShellExecute = true });
            e.Handled = true;
        }
    }
}
