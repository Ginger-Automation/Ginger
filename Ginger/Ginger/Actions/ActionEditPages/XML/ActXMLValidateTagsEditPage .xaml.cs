#region License
/*
Copyright © 2014-2025 European Support Limited

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
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(XMLFileTextBox, TextBox.TextProperty, mAct.InputFile, nameof(ActInputValue.Value));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ReqisFromFile, CheckBox.IsCheckedProperty, mAct, "ReqisFromFile");
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(chkReadJustAttributeValues, CheckBox.IsCheckedProperty, mAct, nameof(ActXMLTagValidation.ReadJustXMLAttributeValues));

            xDocumentTypeComboBox.Init(mAct, ActXMLTagValidation.Fields.DocumentType, typeof(ActXMLTagValidation.eDocumentType), xDocumentTypeComboBox_SelectionChanged);

            XMLFileTextBox.Init(Context.GetAsContext(mAct.Context), mAct.InputFile);

            SetGridView();

            DynamicParametersGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddPlaceHolder));
        }

        private void AddPlaceHolder(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = new ActInputValue();
            mAct.DynamicElements.Add(AIV);
        }

        private void RequestFromFileChecked(object sender, RoutedEventArgs e)
        {
            FileContent.Content = "Template File Path";
            BrowseFileButton.Visibility = Visibility.Visible;
        }

        private void RequestFromFileUnChecked(object sender, RoutedEventArgs e)
        {

            FileContent.Content = "Template File Content";
            BrowseFileButton.Visibility = Visibility.Collapsed;

        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ActInputValue.Param), Header = "Path", WidthWeight = 150 },
                new GridColView() { Field = nameof(ActInputValue.Value), Header = "Attribute", WidthWeight = 150 },
            ]
            };

            DynamicParametersGrid.SetAllColumnsDefaultView(view);
            DynamicParametersGrid.InitViewItems();

            DynamicParametersGrid.DataSourceList = mAct.DynamicElements;
        }

        private void InputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)DynamicParametersGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            if (mAct.DocumentType == ActXMLTagValidation.eDocumentType.XML)
            {
                dlg.DefaultExt = "*.xml";
                dlg.Filter = "XML Template File (*.xml)|*.xml|All Files (*.*)|*.*";
            }
            else if (mAct.DocumentType == ActXMLTagValidation.eDocumentType.JSON)
            {
                dlg.DefaultExt = "*.json";
                dlg.Filter = "JSON Template File (*.json)|*.json|All Files (*.*)|*.*";
            }
            if (General.SetupBrowseFile(dlg) is string fileName)
            {
                XMLFileTextBox.ValueTextBox.Text = fileName;
            }
        }

        private void BodyType_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
        private void chkReadJustAttributeValuesChecked(object sender, RoutedEventArgs e)
        {
            mAct.ReadJustXMLAttributeValues = true;
            mAct.InvokPropertyChanngedForAllFields();
        }

        private void chkReadJustAttributeValuesUnChecked(object sender, RoutedEventArgs e)
        {
            mAct.ReadJustXMLAttributeValues = false;
            mAct.InvokPropertyChanngedForAllFields();
        }
        private void xDocumentTypeComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (mAct.DocumentType == ActXMLTagValidation.eDocumentType.XML)
            {
                grRowReadJustAttributeValues.Visibility = Visibility.Visible;
            }
            else
            {
                grRowReadJustAttributeValues.Visibility = Visibility.Collapsed;
            }
        }
    }
}
