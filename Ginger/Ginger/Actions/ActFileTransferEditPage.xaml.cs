#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for XLSReadDataToVariablesPage.xaml
    /// </summary>
    public partial class ActFileTransferEditPage:Page
    {
         ActFileTransfer mAct{get;set;}
        
         public ActFileTransferEditPage(Act act)
        {
            InitializeComponent();

            this.mAct = (ActFileTransfer)act;
            
            App.FillComboFromEnumVal(FileTransferActionComboBox, mAct.FileTransferAction);            
            App.ObjFieldBinding(FileTransferActionComboBox, ComboBox.TextProperty, mAct, "FileTransferAction");
            
             App.ObjFieldBinding(PCPath, TextBox.TextProperty, mAct, ActFileTransfer.Fields.PCPath);
             PCPath.Init(mAct.GetOrCreateInputParam(ActFileTransfer.Fields.PCPath), ActInputValue.Fields.Value);
            
            App.ObjFieldBinding(UnixPath, TextBox.TextProperty, mAct, ActFileTransfer.Fields.UnixPath);
            UnixPath.Init(mAct.GetOrCreateInputParam(ActFileTransfer.Fields.UnixPath), ActInputValue.Fields.Value);

            App.ObjFieldBinding(UserName, TextBox.TextProperty, mAct, ActFileTransfer.Fields.UserName);
            UserName.Init(mAct.GetOrCreateInputParam(ActFileTransfer.Fields.UserName), ActInputValue.Fields.Value);
            
            App.ObjFieldBinding(Password, TextBox.TextProperty, mAct, ActFileTransfer.Fields.Password);
            Password.Init(mAct.GetOrCreateInputParam(ActFileTransfer.Fields.Password), ActInputValue.Fields.Value);
            
            App.ObjFieldBinding(PrivateKey, TextBox.TextProperty, mAct, ActFileTransfer.Fields.PrivateKey);
            PrivateKey.Init(mAct.GetOrCreateInputParam(ActFileTransfer.Fields.PrivateKey), ActInputValue.Fields.Value);
            
            App.ObjFieldBinding(KeyPassPhrase, TextBox.TextProperty, mAct, ActFileTransfer.Fields.PrivateKeyPassPhrase);
            KeyPassPhrase.Init(mAct.GetOrCreateInputParam(ActFileTransfer.Fields.PrivateKeyPassPhrase), ActInputValue.Fields.Value);
            
            App.ObjFieldBinding(Port, TextBox.TextProperty, mAct, ActFileTransfer.Fields.Port);
            Port.Init(mAct.GetOrCreateInputParam(ActFileTransfer.Fields.Port), ActInputValue.Fields.Value);
            
            App.ObjFieldBinding(Host, TextBox.TextProperty, mAct, ActFileTransfer.Fields.Host);
            Host.Init(mAct.GetOrCreateInputParam(ActFileTransfer.Fields.Host), ActInputValue.Fields.Value);
        }

        private void BrowsePCPathButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.DefaultExt = "*.*";
            dlg.Filter = "Any Data Files (*.*)|*.*";
            string SolutionFolder =  WorkSpace.UserProfile.Solution.Folder.ToUpper();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // replace Absolute file name with relative to solution
                string FileName = dlg.FileName.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }

                PCPath.ValueTextBox.Text = FileName;
                //Move code to ExcelFunction no in Act...
            }
        }

        private void FileTransferActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(FileTransferActionComboBox.SelectedValue.ToString().Equals("GetFile"))
            {
                pcPath.Content = "PC Target directory path";
                unixPath.Content = "Unix Source file path";
            }
            else
            {
                pcPath.Content = "PC Source file path";
                unixPath.Content = "Unix Target directory path";
            }
        }
    }
}
