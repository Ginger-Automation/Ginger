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

using Amdocs.Ginger.Common;
using GingerCore;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib.TextEditor
{
    /// <summary>
    /// Interaction logic for ImportDocumentPage.xaml
    /// </summary>
    public partial class ImportDocumentPage : System.Windows.Controls.Page
    {
        GenericWindow genWin;
        string mFolder;
        public bool Imported;
        public string mPath;
        public string FileExtensionForBrowse;
        public string PlugInEditorName;

        public ImportDocumentPage(string folder, string plugInEditorName, string fileExtensionForBrowse)
        {
            InitializeComponent();
            FileExtensionForBrowse = fileExtensionForBrowse;
            PlugInEditorName = plugInEditorName;
            mFolder = folder;
            FileName.FileExtensions.Add(fileExtensionForBrowse);
            FileName.FilePathTextBox.TextChanged += FilePathTextBox_TextChanged;
        }

        private void FilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(FileName.FilePathTextBox.Text))
                return;
            if (System.IO.Path.GetExtension(FileName.FilePathTextBox.Text).ToUpper() != FileExtensionForBrowse.ToUpper())
            {
                Reporter.ToUser(eUserMsgKey.FileExtensionNotSupported, FileExtensionForBrowse);
                FileName.FilePathTextBox.Text = string.Empty;
                return;
            }
            else
            {
                EditorFrame.Content = new  DocumentEditorPage(FileName.FilePathTextBox.Text, false, true,"Selected Content Viewer");
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            ObservableList<Button> winButtons = new ObservableList<Button>();
            Button ImportButton = new Button();
            ImportButton.Content = "Import";
            ImportButton.Click += new RoutedEventHandler(ImportButton_Click);
            winButtons.Add(ImportButton);

            genWin = null;
            this.Height = 400;
            this.Width = 400;
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Import "+ PlugInEditorName + " File", this, winButtons);
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            mPath = Import();
            Imported = true;
        }

        private string Import()
        {
            string SelectedFileName = System.IO.Path.GetFileName(FileName.FilePathTextBox.Text);
            string targetFile = System.IO.Path.Combine(mFolder, SelectedFileName);

            if (targetFile == FileName.FilePathTextBox.Text)
            {
                Reporter.ToUser(eUserMsgKey.NotifyFileSelectedFromTheSolution, targetFile);
                return String.Empty;
            }
            File.Copy(FileName.FilePathTextBox.Text, targetFile);
            Reporter.ToUser(eUserMsgKey.FileImportedSuccessfully, targetFile);
            if (genWin != null)
            {
                genWin.Close();
            }
            return targetFile;
        }
    }
}
