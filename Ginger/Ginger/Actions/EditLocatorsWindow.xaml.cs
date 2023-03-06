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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using Ginger.Actions;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for EditPlatformsWindow.xaml
    /// </summary>
    //TODO: not use? delete or recycle - might be good idea...
    public partial class EditLocatorsWindow : Window
    {
        ObservableList<Act> Locators = new ObservableList<Act>();
        UIAutomation UIA = new UIAutomation();

        public static string sMultiLocatorVals { get; set; }
        public EditLocatorsWindow(BusinessFlow BizFlow)
        {
            InitializeComponent();
            SetGridView();
            UIA.CreateLocatorList(LocatorsGrid, Locators);
        }

        public void SetGridView()
        {
            List<string> lstLocateBy = new List<string>();
            Type Etype = eLocateBy.ByID.GetType();
            foreach (object item in Enum.GetValues(Etype))
            {
                lstLocateBy.Add(item.ToString());
            }
            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            LocatorsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddLocator));
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.LocateBy, WidthWeight = 50, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = lstLocateBy.ToList() });
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.LocateValue, WidthWeight = 50 });
            LocatorsGrid.SetAllColumnsDefaultView(view);
            LocatorsGrid.InitViewItems();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            sMultiLocatorVals = UIA.getSelectedLocators(Locators); 
            this.Close();
        }

        public void AddLocator(object sender, RoutedEventArgs e)
        {
            Locators.Add(new ActDummy() { LocateBy = eLocateBy.NA, LocateValue = " " });
        }

        private void RecButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainWindow.WindowState = System.Windows.WindowState.Minimized;
            this.WindowState = System.Windows.WindowState.Minimized;
        }
    }
}
