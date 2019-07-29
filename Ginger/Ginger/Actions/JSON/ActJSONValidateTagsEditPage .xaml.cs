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

using Amdocs.Ginger.Common;
using System.Windows;
using System.Windows.Controls;
using GingerCore.Actions;
using GingerCore.Actions.JSON;
using Ginger.UserControls;
using Amdocs.Ginger.Repository;

namespace Ginger.Actions.JSON
{
    /// <summary>
    /// Interaction logic for ActJSONValidateTagsEditPage.xaml
    /// </summary>
    public partial class ActJSONValidateTagsEditPage : Page
    {
        ActJSONTagValidation mAct { get; set; }

        public ActJSONValidateTagsEditPage(Act act)
        {
            InitializeComponent();
            this.mAct = (ActJSONTagValidation)act;
            //Binding 
            JSONPath.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActJSONTagValidation.Fields.JsonInput), true, false, UCValueExpression.eBrowserType.Folder);
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
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), Header = "Path", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), Header = "Attribute", WidthWeight = 150 });
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
    }
}