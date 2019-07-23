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
using System.Windows.Data;
using GingerCore.Actions.MainFrame;
using Ginger.UserControls;
using Amdocs.Ginger.Repository;

namespace Ginger.Actions.Mainframe
{
    /// <summary>
    /// Interaction logic for ActMainFrameSendKey.xaml
    /// </summary>
    public partial class ActMainFrameSetTextEditPage : Page
    {
        public ActMainframeSetText mAct = null;
        public ActMainFrameSetTextEditPage(ActMainframeSetText Act)
        {
            mAct = Act;
            InitializeComponent();
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SendAfterSettingText, CheckBox.IsCheckedProperty, mAct, "SendAfterSettingText");
            GingerCore.General.FillComboFromEnumObj(SetTextModeCombo, mAct.SetTextMode);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SetTextModeCombo, ComboBox.SelectedValueProperty, mAct, "SetTextMode");
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ReloadValue, CheckBox.IsCheckedProperty, mAct, "ReloadValue");
            SetCaretValueGrid ();
            CaretValueGrid.btnAdd.AddHandler (Button.ClickEvent, new RoutedEventHandler (AddCaretValueItem));
        }

        private void SetCaretValueGrid()
        {
            GridViewDef view = new GridViewDef (GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView> ();

            view.GridColsView.Add (new GridColView () { Field = nameof(ActInputValue.Param), Header = "Caret", WidthWeight = 150 });
            view.GridColsView.Add (new GridColView () { Field = nameof(ActInputValue.Value), Header = "Text", WidthWeight = 150 });
            view.GridColsView.Add (new GridColView () { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["HttpHeadersValueExpressionButton"] });
            view.GridColsView.Add (new GridColView () { Field = nameof(ActInputValue.ValueForDriver), Header = "Replace With Value For Driver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

            CaretValueGrid.SetAllColumnsDefaultView (view);
            CaretValueGrid.InitViewItems ();
            CaretValueGrid.DataSourceList = mAct.CaretValueList;
        }

        private void AddCaretValueItem(object sender, RoutedEventArgs e)
        {
            string PlaceHolderName = "{Place Holder " + (mAct.CaretValueList.Count + 1) + "}";
            mAct.CaretValueList.Add (new ActInputValue () { Param = PlaceHolderName });
        }

        private void LoadFields_Click(object sender, RoutedEventArgs e)
        {
            Context.GetAsContext(mAct.Context).Runner.PrepActionValueExpression(mAct);
            mAct.LoadCaretValueList ();
            CaretValueGrid.DataSourceList = mAct.CaretValueList;
        }

        private void SetTextModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SetTextModeCombo.SelectedValue == null)
                return;
            if (SetTextModeCombo.SelectedValue.ToString () == "SetMultipleFields")
            {
                MultiFieldPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else 
            MultiFieldPanel.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
