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
using Ginger.UserControls;
using GingerCore.Actions;
using GingerCore.Actions.XML;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.Actions.XML
{
    /// <summary>
    /// Interaction logic for ActXMLEditPage.xaml
    /// </summary>
    public partial class ActXMLProcessingEditPage : Page
    {
        ActXMLProcessing mAct { get; set; }

        public ActXMLProcessingEditPage(Act act)
        {
            InitializeComponent();
            this.mAct = (ActXMLProcessing)act;            

            //// Bind Controls
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(XMLTemplateFileTextBox , TextBox.TextProperty, mAct.TemplateFileName , nameof(ActInputValue.Value));
            TargetFileNameTextBox.Init(Context.GetAsContext(mAct.Context), mAct.TargetFileName);

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

             view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), Header = "Parameter", WidthWeight = 150 });
             view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), Header = "Value", WidthWeight = 150 });
             view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.Resources["InputValueExpressionButton"] });
             view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = "Value For Driver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

             DynamicParametersGrid.SetAllColumnsDefaultView(view);
             DynamicParametersGrid.InitViewItems();

             DynamicParametersGrid.DataSourceList = mAct.DynamicElements ;
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
            dlg.Filter = "All files (*.*)|*.*";
            string SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();

            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // replace Absolute file name with relative to solution
                string FileName = dlg.FileName.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }

                XMLTemplateFileTextBox.Text = FileName;
            }
        }
    }
}
