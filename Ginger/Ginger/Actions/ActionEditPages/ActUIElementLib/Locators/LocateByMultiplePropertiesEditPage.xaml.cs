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
using Ginger.UserControls;
using GingerCore.Actions.Common;
using GingerCore.GeneralLib;
using GingerCore.Platforms.PlatformsInfo;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for LocateByMultiplePropertiesEditPage.xaml
    /// </summary>
    public partial class LocateByMultiplePropertiesEditPage : Page
    {
        ActUIElement mAction;
        PlatformInfoBase mPlatform;

        public LocateByMultiplePropertiesEditPage(ActUIElement Action, PlatformInfoBase Platform)
        {
            InitializeComponent();
            mAction = Action;
            mPlatform = Platform;
            SetMultiplePropertiesGridView();
        }

        public void SetMultiplePropertiesGridView()
        {
            ObservableList<UIElementPropertyValueLocator> list = new ObservableList<UIElementPropertyValueLocator>();

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            List<string> lstLocateBy = mPlatform.GetPlatformUIElementPropertiesList(mAction.ElementType);

            //TODO: get same list for platform from  cboLocateBy.Items - except ByMultiple which is not valid

            view.GridColsView.Add(new GridColView() { Field = "Property", WidthWeight = 50, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = lstLocateBy });

            List<ComboEnumItem> lstOps = GingerCore.General.GetEnumValuesForCombo(typeof(UIElementPropertyValueLocator.eLocatorOpertor));
            view.GridColsView.Add(new GridColView() { Field = "Operator", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = lstOps });


            view.GridColsView.Add(new GridColView() { Field = "Value", WidthWeight = 100 });

            MultiplePropertiesGrid.SetAllColumnsDefaultView(view);
            MultiplePropertiesGrid.InitViewItems();

            // some dummy data for test
            //TODO: get and save the data in AIVs? or locavalue
            // Value need to be VE - editable
            list.Add(new UIElementPropertyValueLocator() { Property = "ID", Opertor = UIElementPropertyValueLocator.eLocatorOpertor.EQ, Value = "ABC" });
            list.Add(new UIElementPropertyValueLocator() { Property = "Text", Opertor = UIElementPropertyValueLocator.eLocatorOpertor.Contains, Value = "123" });

            MultiplePropertiesGrid.DataSourceList = list;
        }
    }
}
