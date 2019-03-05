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

using Amdocs.Ginger.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ginger.UserControls;
using GingerCore;
using GingerCore.FlowControlLib;
using Ginger.Actions.UserControls;
using System.Collections.Generic;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for BusinessFlowRunFlowControlPage.xaml
    /// </summary>
    public partial class BusinessFlowRunFlowControlPage : Page
    {
        GingerRunner mBfParentRunner = null;
        BusinessFlow mActParentBusinessFlow = null;
        private static readonly List<GingerCore.General.ComboEnumItem> OperatorList = GingerCore.General.GetEnumValuesForCombo(typeof(eFCOperator));

        public BusinessFlowRunFlowControlPage(GingerRunner mRunner, BusinessFlow actParentBusinessFlow)
        {
            InitializeComponent();

            mBfParentRunner = mRunner;
            mActParentBusinessFlow = actParentBusinessFlow;

            SetGridView();
            FlowControlGrid.DataSourceList = mActParentBusinessFlow.BFFlowControls;

            FlowControlGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddFlowControl));

            // TODO:  open new edit page -  FlowControlGrid.btnEdit.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditAction));
        }

        private void AddFlowControl(object sender, RoutedEventArgs e)
        {
            FlowControl FC = new FlowControl();
            FC.Operator = eFCOperator.CSharp;
            FC.Active = true;
            mActParentBusinessFlow.BFFlowControls.Add(FC);
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;
            viewCols.Add(new GridColView() { Field = FlowControl.Fields.Active, WidthWeight = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = FlowControl.Fields.Condition, Header = "Condition", WidthWeight = 200, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.getDataColValueExpressionTemplate("ConditionVE") });
            viewCols.Add(new GridColView() { Field = nameof(FlowControl.Operator), Header = "Operator", WidthWeight = 50, BindingMode = BindingMode.TwoWay, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = OperatorList });
            viewCols.Add(new GridColView() { Field = FlowControl.Fields.ConditionCalculated, Header = "Condition Calculated", WidthWeight = 150, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = FlowControl.Fields.BusinessFlowControlAction, Header = "Action", WidthWeight = 200, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = GetDataColActionFlowControlTemplate("ActionForEdit") });
            viewCols.Add(new GridColView() { Field = FlowControl.Fields.Status, WidthWeight = 150 });
            FlowControlGrid.SetAllColumnsDefaultView(view);
            FlowControlGrid.InitViewItems();
        }

        private void GridVEButton_Click(object sender, RoutedEventArgs e)
        {
            FlowControl FC = (FlowControl)FlowControlGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(FC, FlowControl.Fields.Condition, new Context() { BusinessFlow = mActParentBusinessFlow });
            VEEW.ShowAsWindow();
        }

        private DataTemplate GetDataColActionFlowControlTemplate(string Path)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(UCFlowControlAction));

            factory.SetBinding(UCDataColGrid.DataContextProperty, new Binding(Path));

            factory.SetValue(UCFlowControlAction.ActParentBusinessFlowProperty, mActParentBusinessFlow);
            factory.SetValue(UCFlowControlAction.BfParentRunnerProperty, mBfParentRunner);

            template.VisualTree = factory;

            return template;
        }
    }
}
