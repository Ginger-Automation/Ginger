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
    public partial class ActReadTextFileEditPage 
    {
        private ActReadTextFile mAct;

        public ActReadTextFileEditPage(ActReadTextFile act)
        {
            InitializeComponent();
            mAct = act;
            TextFileNameTextBox.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActReadTextFile.Fields.TextFilePath));
            TextToWrite.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActReadTextFile.Fields.TextToWrite));
            LineNumber.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActReadTextFile.Fields.AppendLineNumber));

            mAct.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();

            GingerCore.General.FillComboFromEnumObj(FileActionMode, mAct.FileActionMode);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(FileActionMode, ComboBox.SelectedValueProperty, mAct,"FileActionMode");

            GingerCore.General.FillComboFromEnumObj(TextFileEncoding, mAct.TextFileEncoding);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TextFileEncoding, ComboBox.SelectedValueProperty, mAct, "TextFileEncoding");

            GingerCore.General.FillComboFromEnumObj(TextFileAppendType, mAct.AppendAt);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TextFileAppendType, ComboBox.SelectedValueProperty, mAct, "AppendAt");
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
            if ((ActReadTextFile.eTextFileActionMode)FileActionMode.SelectedValue == ActReadTextFile.eTextFileActionMode.Append || (ActReadTextFile.eTextFileActionMode)FileActionMode.SelectedValue == ActReadTextFile.eTextFileActionMode.Write)
            {
                PanelToWrite.Visibility = Visibility.Visible;
            }
            else
            {
                PanelToWrite.Visibility = Visibility.Collapsed;
            }
            if ((ActReadTextFile.eTextFileActionMode)FileActionMode.SelectedValue == ActReadTextFile.eTextFileActionMode.Append)
            {
                PanelAppendAt.Visibility = Visibility.Visible;                
            }
            else
            {
                PanelAppendAt.Visibility = Visibility.Collapsed;                
            }
        }

        private void TextFileAppendType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((ActReadTextFile.eAppendAt)TextFileAppendType.SelectedValue == ActReadTextFile.eAppendAt.SpecificLine)
            {                
                PanelAppendLine.Visibility = Visibility.Visible;
            }
            else
            {                
                PanelAppendLine.Visibility = Visibility.Collapsed;
            }
        }
    }
}
