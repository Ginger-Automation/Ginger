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

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ScriptFileNameTextBox, TextBox.TextProperty, mRunSetActionScript, RunSetActionScript.Fields.ScriptFileName);
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();

            dlg.DefaultExt = "*.VBS";
            dlg.Filter = "Script File (*.VBS)|*.VBS";
            string SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
            
            if(dlg.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                // replace Absolute file name with relative to solution
                string FileName = dlg.FileName.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }

                ScriptFileNameTextBox.Text = FileName;
                //TODO: show file for view only... maybe edit later
            }
        }
    }
}
