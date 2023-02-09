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

using System.Windows;
using System.Windows.Controls;
using amdocs.ginger.GingerCoreNET;
namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionScriptEditPage.xaml
    /// </summary>
    public partial class RunSetActionScriptEditPage : Page
    {
        RunSetActionScript mRunSetActionScript;
        public RunSetActionScriptEditPage(RunSetActionScript RunSetActionScript)
        {
            InitializeComponent();

            mRunSetActionScript = RunSetActionScript;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ScriptFileNameTextBox, TextBox.TextProperty, mRunSetActionScript, nameof(RunSetActionScript.ScriptFileName));
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = "*.VBS",
                Filter = "Script File (*.VBS)|*.VBS|Shell Script File (*.sh)|*.sh"
            }) is string fileName)
            {
                ScriptFileNameTextBox.Text = fileName;
                //TODO: show file for view only... maybe edit later
            }
        }
    }
}
