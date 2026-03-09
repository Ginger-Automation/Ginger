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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCore.Actions;
using System.Windows;
using System.Windows.Controls;
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

            DestinationFolderTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActFileOperations.Fields.DestinationFolder), true, true, UCValueExpression.eBrowserType.Folder);

            xRunArgumentsTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(nameof(ActFileOperations.Arguments)), true, false);

            mAct.SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();

            GingerCore.General.FillComboFromEnumObj(FileActionMode, mAct.FileOperationMode);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(FileActionMode, ComboBox.SelectedValueProperty, mAct, "FileOperationMode");
            UpdateBrowserTypes();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()) is string fileName)
            {
                TextFileNameTextBox.ValueTextBox.Text = fileName;
            }
            else if (General.SetupBrowseFolder(new System.Windows.Forms.FolderBrowserDialog()) is string folderName)
            {
                TextFileNameTextBox.ValueTextBox.Text = folderName;
            }
        }

        private void FileActionMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBrowserTypes();
        }

        /// <summary>
        /// Update browser type according to selected file action. Some needs file browser and some needs folder browser
        /// </summary>
        private void UpdateBrowserTypes()
        {
            if (FileActionMode.SelectedValue != null)
            {
                if ((ActFileOperations.eFileoperations)FileActionMode.SelectedValue is ActFileOperations.eFileoperations.Copy
                    or ActFileOperations.eFileoperations.ForceCopy
                   or ActFileOperations.eFileoperations.Move
                   or ActFileOperations.eFileoperations.UnZip)
                {
                    PanelToWrite.Visibility = Visibility.Visible;
                    DestinationFolderTextBox.BrowserType = UCValueExpression.eBrowserType.Folder;
                }
                else
                {
                    PanelToWrite.Visibility = Visibility.Collapsed;
                    DestinationFolderTextBox.BrowserType = UCValueExpression.eBrowserType.File;
                }

                if ((ActFileOperations.eFileoperations)FileActionMode.SelectedValue is ActFileOperations.eFileoperations.CheckFolderExists
                  or ActFileOperations.eFileoperations.DeleteDirectoryFiles
                  or ActFileOperations.eFileoperations.DeleteDirectory)
                {
                    TextFileNameTextBox.BrowserType = UCValueExpression.eBrowserType.Folder;
                }
                else
                {
                    TextFileNameTextBox.BrowserType = UCValueExpression.eBrowserType.File;
                }

                if ((ActFileOperations.eFileoperations)FileActionMode.SelectedValue is ActFileOperations.eFileoperations.RunCommand
                    or ActFileOperations.eFileoperations.Execute)
                {
                    xPanelRunArguments.Visibility = Visibility.Visible;
                }
                else
                {
                    xPanelRunArguments.Visibility = Visibility.Collapsed;
                }

            }
        }
    }
}