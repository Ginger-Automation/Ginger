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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
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
            
            GingerCore.General.FillComboFromEnumObj(FileTransferActionComboBox, mAct.FileTransferAction);            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(FileTransferActionComboBox, ComboBox.TextProperty, mAct, "FileTransferAction");
            
             GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(PCPath, TextBox.TextProperty, mAct, ActFileTransfer.Fields.PCPath);
             PCPath.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActFileTransfer.Fields.PCPath),nameof( ActInputValue.Value));
            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(UnixPath, TextBox.TextProperty, mAct, ActFileTransfer.Fields.UnixPath);
            UnixPath.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActFileTransfer.Fields.UnixPath), nameof (ActInputValue.Value));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(UserName, TextBox.TextProperty, mAct, ActFileTransfer.Fields.UserName);
            UserName.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActFileTransfer.Fields.UserName), nameof(ActInputValue.Value));
            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(Password, TextBox.TextProperty, mAct, ActFileTransfer.Fields.Password);
            Password.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActFileTransfer.Fields.Password),nameof( ActInputValue.Value));
            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(PrivateKey, TextBox.TextProperty, mAct, ActFileTransfer.Fields.PrivateKey);
            PrivateKey.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActFileTransfer.Fields.PrivateKey), nameof (ActInputValue.Value));
            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(KeyPassPhrase, TextBox.TextProperty, mAct, ActFileTransfer.Fields.PrivateKeyPassPhrase);
            KeyPassPhrase.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActFileTransfer.Fields.PrivateKeyPassPhrase), nameof(ActInputValue.Value));
            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(Port, TextBox.TextProperty, mAct, ActFileTransfer.Fields.Port);
            Port.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActFileTransfer.Fields.Port), nameof(ActInputValue.Value));
            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(Host, TextBox.TextProperty, mAct, ActFileTransfer.Fields.Host);
            Host.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActFileTransfer.Fields.Host), nameof(ActInputValue.Value));
        }

        private void BrowsePCPathButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.DefaultExt = "*.*";
            dlg.Filter = "Any Data Files (*.*)|*.*";
            string SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();

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
