#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Amdocs.Ginger.Common.UIElement;
using Ginger.UserControls;
using GingerCoreNET.Application_Models;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    /// <summary>
    /// Interaction logic for PomDeltaUpdateElementsComparePage.xaml
    /// </summary>
    public partial class PomDeltaMappingElementsComparePage : Page
    {
        GenericWindow genericWindow = null;

        public PomDeltaMappingElementsComparePage(DeltaElementInfo deletedElement, DeltaElementInfo newAddedElement)
        {
            InitializeComponent();
            
            // set delement gridview
            SetElementLocatorsGridView(new GridViewDef(GridViewDef.DefaultViewName));
            xDeletedElementDetails.xLocatorsGrid.DataSourceList = deletedElement.ElementInfo.Locators;
            xDeletedElementDetails.xSelectedElementTextBlock.Text = "Deleted Element";
            xDeletedElementDetails.xLocatorsGrid.AllowHorizentalScroll = true;
            SetElementPropertiesGridView(new GridViewDef(GridViewDef.DefaultViewName));
            xDeletedElementDetails.xPropertiesGrid.DataSourceList = deletedElement.ElementInfo.Properties;
            xDeletedElementDetails.xPropertiesGrid.AllowHorizentalScroll = true;

            //set new added element grdiview
            SetElementLocatorsGridView(new GridViewDef(GridViewDef.DefaultViewName), false);
            SetElementPropertiesGridView(new GridViewDef(GridViewDef.DefaultViewName), false);
            xAddedElementDetails.xSelectedElementTextBlock.Text = "New Added Element";
            xAddedElementDetails.xLocatorsGrid.AllowHorizentalScroll = true;
            xAddedElementDetails.xPropertiesGrid.AllowHorizentalScroll = true;
            if (newAddedElement != null)
            {
                xAddedElementDetails.xLocatorsGrid.DataSourceList = newAddedElement.ElementInfo.Locators;
                xAddedElementDetails.xPropertiesGrid.DataSourceList = newAddedElement.ElementInfo.Properties;
            }

        }

        private void SetElementLocatorsGridView(GridViewDef gridViewDef,bool isDeletedElement=true)
        {
            gridViewDef.GridColsView = new ObservableList<GridColView>();
            gridViewDef.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25,ReadOnly=true});
            gridViewDef.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.LocateValue), Header = "Locate Value", WidthWeight = 65,ReadOnly=true });
            gridViewDef.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 20, ReadOnly=true });

            if (isDeletedElement)
            {
                xDeletedElementDetails.xLocatorsGrid.toolbar.Visibility = Visibility.Collapsed;
                xDeletedElementDetails.xLocatorsGrid.SetAllColumnsDefaultView(gridViewDef);
                xDeletedElementDetails.xLocatorsGrid.InitViewItems();
                xDeletedElementDetails.xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            }
           else
            {
                xAddedElementDetails.xLocatorsGrid.toolbar.Visibility = Visibility.Collapsed;
                xAddedElementDetails.xLocatorsGrid.SetAllColumnsDefaultView(gridViewDef);
                xAddedElementDetails.xLocatorsGrid.InitViewItems();
                xAddedElementDetails.xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            }

        }

        private void SetElementPropertiesGridView(GridViewDef gridViewDef,bool isDeletedElement=true)
        {
            gridViewDef.GridColsView = new ObservableList<GridColView>();

            gridViewDef.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Name), WidthWeight = 25 ,ReadOnly=true});
            gridViewDef.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Value), WidthWeight = 75 ,ReadOnly=true});

            if (isDeletedElement)
            {
                xDeletedElementDetails.xPropertiesGrid.SetAllColumnsDefaultView(gridViewDef);
                xDeletedElementDetails.xPropertiesGrid.InitViewItems();
                xDeletedElementDetails.xPropertiesGrid.SetTitleLightStyle = true;
            }
            else
            {
                xAddedElementDetails.xPropertiesGrid.SetAllColumnsDefaultView(gridViewDef);
                xAddedElementDetails.xPropertiesGrid.InitViewItems();
                xAddedElementDetails.xPropertiesGrid.SetTitleLightStyle = true;
            }

        }

        internal void ShowAsWindow(string windowTitle)
        {         
            this.Height = 600;
            this.Width = 950;
            GenericWindow.LoadGenericWindow(ref genericWindow, null, eWindowShowStyle.Dialog, windowTitle, this, null, true, "Close", CloseBtn_Click);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (genericWindow != null)
            {
                genericWindow.Close();
            }
        }
    }
}
