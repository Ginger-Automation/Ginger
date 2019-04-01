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
using Amdocs.Ginger.Common;
using GingerCore.Actions;
namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActReadTextFile.xaml
    /// </summary>
    public partial class ActFileOperationEditPage 
    {
        private ActFileOperations mAct;

        public ActFileOperationEditPage(ActFileOperations act)
        {
            InitializeComponent();
            mAct = act;
            TextFileNameTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActFileOperations.Fields.SourceFilePath), true, true, UCValueExpression.eBrowserType.File);
            DestinationFolderTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActFileOperations.Fields.DestinationFolder),true,true,UCValueExpression.eBrowserType.File);
            xRunArgumentsTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActFileOperations.Arguments)), true, false);

            mAct.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();

            GingerCore.General.FillComboFromEnumObj(FileActionMode, mAct.FileOperationMode);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(FileActionMode, ComboBox.SelectedValueProperty, mAct, "FileOperationMode");
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            
            string SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string FileName = dlg.FileName.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }
                TextFileNameTextBox.ValueTextBox.Text = FileName;
            }
        }
        
        private void FileActionMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileActionMode.SelectedValue != null)
            {
                if ((ActFileOperations.eFileoperations)FileActionMode.SelectedValue == ActFileOperations.eFileoperations.Copy
                    ||(ActFileOperations.eFileoperations)FileActionMode.SelectedValue == ActFileOperations.eFileoperations.ForceCopy
                   || (ActFileOperations.eFileoperations)FileActionMode.SelectedValue == ActFileOperations.eFileoperations.Move 
                   || (ActFileOperations.eFileoperations)FileActionMode.SelectedValue == ActFileOperations.eFileoperations.UnZip )
                {
                    PanelToWrite.Visibility = Visibility.Visible;
                }
                else
                {
                    PanelToWrite.Visibility = Visibility.Collapsed;

                }

                if ((ActFileOperations.eFileoperations)FileActionMode.SelectedValue == ActFileOperations.eFileoperations.RunCommand
                    || (ActFileOperations.eFileoperations)FileActionMode.SelectedValue == ActFileOperations.eFileoperations.Execute)
                    xPanelRunArguments.Visibility = Visibility.Visible;
                else
                    xPanelRunArguments.Visibility = Visibility.Collapsed;
            }


        }
    }
}