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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore.Actions;
using GingerCore.Actions.XML;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.XML
{
    /// <summary>
    /// Interaction logic for ActXMLEditPage.xaml
    /// </summary>
    public partial class ActXMLValidateTagsEditPage : Page
    {
        ActXMLTagValidation mAct { get; set; }

        public ActXMLValidateTagsEditPage(Act act)
        {
            InitializeComponent();
            this.mAct = (ActXMLTagValidation)act;

            //// Bind Controls
            App.ObjFieldBinding(XMLFileTextBox , TextBox.TextProperty, mAct.InputFile , ActInputValue.Fields.Value);
      
            App.ObjFieldBinding(ReqisFromFile, CheckBox.IsCheckedProperty, mAct, "ReqisFromFile");

            xDocumentTypeComboBox.Init(mAct, ActXMLTagValidation.Fields.DocumentType, typeof(ActXMLTagValidation.eDocumentType));
     
            XMLFileTextBox.Init(mAct.InputFile);
      
            SetGridView();

            DynamicParametersGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddPlaceHolder));
        }

        private void AddPlaceHolder(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = new ActInputValue();
            mAct.DynamicElements.Add(AIV);
        }

         private void SetGridView()
         {
             GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
             view.GridColsView = new ObservableList<GridColView>();

             view.GridColsView.Add(new GridColView() { Field = ActInputValue.Fields.Param, Header = "Path", WidthWeight = 150 });
             view.GridColsView.Add(new GridColView() { Field = ActInputValue.Fields.Value, Header = "Attribute", WidthWeight = 150 });

             DynamicParametersGrid.SetAllColumnsDefaultView(view);
            DynamicParametersGrid.InitViewItems();

            DynamicParametersGrid.DataSourceList = mAct.DynamicElements ;
        }

         private void InputGridVEButton_Click(object sender, RoutedEventArgs e)
         {
             ActInputValue AIV = (ActInputValue)DynamicParametersGrid.CurrentItem;
             ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, ActInputValue.Fields.Value);
             VEEW.ShowAsWindow();
         }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            if (mAct.DocumentType == ActXMLTagValidation.eDocumentType.XML)
            {
                dlg.DefaultExt = "*.xml";
                dlg.Filter = "XML Template File (*.xml)|*.xml "+"|All Files(*.*)| *.* ";
            }
            else if (mAct.DocumentType == ActXMLTagValidation.eDocumentType.JSON)
            {
                dlg.DefaultExt = "*.json";
                dlg.Filter = "JSON Template File (*.json)|*.json" + "|All Files(*.*)| *.* ";
            }

            string SolutionFolder =  WorkSpace.UserProfile.Solution.Folder.ToUpper();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // replace Absolute file name with relative to solution
                string FileName = dlg.FileName.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }

                XMLFileTextBox.ValueTextBox.Text = FileName;
            }
        }

        private void BodyType_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
