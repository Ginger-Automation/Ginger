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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GingerCore.Actions;
using System.Collections.ObjectModel;
using Ginger.UserControls;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActTextBoxEditPage.xaml
    /// </summary>
    public partial class ActUIAButtonEditPage : Page
    {
        public static Act currentAct;
        public ObservableCollection<string> lstParentWindows { get; set; }
        public ObservableList<Act> olParentLocators = new ObservableList<Act>();
        public bool RootWindow { get; set; }

        public ActUIAButtonEditPage(GingerCore.Actions.ActUIAButton Act)
        {
            InitializeComponent();

            currentAct = Act;

            GingerCore.General.FillComboFromEnumObj(ActionNameComboBox, Act.ButtonAction);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ActionNameComboBox, ComboBox.TextProperty, Act, "ButtonAction"); 

            //TODO add dynamic parent windows population
           // lstParentWindows = new ObservableCollection<string>();
           // lstParentWindows.Add("Root");
           // lstParentWindows.Add("Parent Window");
           // ParentComboBox.ItemsSource = lstParentWindows;
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

            grdParentProps.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddLocator));

            view.GridColsView.Add(new GridColView() { Field = Act.Fields.LocateBy, WidthWeight = 50, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = lstLocateBy.ToList() });
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.LocateValue, WidthWeight = 50 });

            grdParentProps.SetAllColumnsDefaultView(view);
            grdParentProps.InitViewItems();
        }

        public void AddLocator(object sender, RoutedEventArgs e)
        {
            olParentLocators.Add(new ActDummy() { LocateBy = eLocateBy.NA, LocateValue = " " });
        }

        private void ActionNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ParentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParentComboBox.SelectedItem.ToString() == "Parent Window")
            { grdParentProps.Visibility = System.Windows.Visibility.Visible; RootWindow = false; }
            else { grdParentProps.Visibility = System.Windows.Visibility.Collapsed; RootWindow = true; }
        }
    }
}
