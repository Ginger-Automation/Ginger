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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using GingerWPF.WizardLib;
using System;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for MapUIElementsWizardPage.xaml
    /// </summary>
    public partial class MapUIElementsWizardPage : Page, IWizardPage
    {
        IWindowExplorer mWinExplorer;
        AddPOMWizard mWizard;
        public ObservableList<ElementLocator> mLocators;
        ApplicationPOMModel mPOM;

        public MapUIElementsWizardPage(ApplicationPOMModel POM)
        {
            InitializeComponent();

            mPOM = POM;
            SetControlsGridView();
            InitLocatorsGrid();

            ShowPOMInfo();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                    mWinExplorer = mWizard.WinExplorer;                    
                    break;
                case EventType.Active:
                    ShowPOMInfo();                    
                    break;
            }
        }

        private void ShowPOMInfo()
        {
        }

        private void SetControlsGridView()
        {
        }

        private void InitLocatorsGrid()
        {
            ////TODO: need to add Help text or convert to icon...
            //// LocatorsGrid.AddToolbarTool()
            ////LocatorsGrid.AddButton("Test", TestSelectedLocator);
            ////LocatorsGrid.AddButton("Test All", TestAllLocators);

            //GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            //defView.GridColsView = new ObservableList<GridColView>();
            //defView.GridColsView.Add(new GridColView() { Field = ElementLocator.Fields.LocateBy, WidthWeight = 10 });
            //defView.GridColsView.Add(new GridColView() { Field = ElementLocator.Fields.LocateValue, WidthWeight = 30 });
            //defView.GridColsView.Add(new GridColView() { Field = ElementLocator.Fields.Help, WidthWeight = 20 });
            //defView.GridColsView.Add(new GridColView() { Field = ElementLocator.Fields.Count, WidthWeight = 10 });

            //LocatorsGrid.SetAllColumnsDefaultView(defView);
            //LocatorsGrid.InitViewItems();

            //LocatorsGrid.DataSourceList = mLocators;

            //LocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        private void ElementsGrid_RowChangedEvent(object sender, EventArgs e)
        {
        }
    }
}
