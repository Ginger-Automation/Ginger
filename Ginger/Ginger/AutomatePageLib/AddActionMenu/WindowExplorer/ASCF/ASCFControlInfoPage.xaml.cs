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

using System.Windows.Controls;
using GingerCore.Drivers.ASCF;

namespace Ginger.WindowExplorer.ASCF
{
    /// <summary>
    /// Interaction logic for ASCFControlInfoPage.xaml
    /// </summary>
    public partial class ASCFControlInfoPage : Page
    {
        public ASCFControlInfoPage(ASCFControlInfo CI)
        {
            InitializeComponent();
             ShowControlInfo(CI);
        }

        private void ShowControlInfo(ASCFControlInfo CI)
        {
            ControlNameTextBox.Text = CI.Name;
            PathTextBox.Text = CI.Path;

            //TODO: fix me
            // ControlTypeTextBox.Text = CI.ControlType.ToString();
        }
    }
}
