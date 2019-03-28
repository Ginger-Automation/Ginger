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
using GingerCore;
using GingerCore.Drivers.ASCF;

namespace Ginger.Actions.Locators.ASCF
{
    /// <summary>
    /// Interaction logic for ASCFControlInfoPage.xaml
    /// </summary>
    public partial class ASCFBrowserInfoPage : Page
    {
        string mPath;
        public ASCFBrowserInfoPage(string Path)
        {
            InitializeComponent();
            mPath = Path;
            ShowControlInfo(Path);
        }

        private void ShowControlInfo(string Path)
        {
            ControlNameTextBox.Text = Path;            
        }

        //TODO: use me
        private void InjectHTMLSpyButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: fix if been used
            //ASCFDriver d = (ASCFDriver)((Agent)App.AutomateTabGingerRunner.ApplicationAgents[0].Agent).Driver;
            //d.InjectHTMLSpy(mPath);           
        }
    }
}
