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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static Ginger.ExtensionMethods;

namespace Ginger.ApiModelsFolder
{
    public partial class APIModelSelectionWizardPage : Page, IWizardPage
    {
        AddApiModelActionWizardPage mAddApiModelActionWizardPage;
        SingleItemTreeViewSelectionPage apiModelTreeSelectionPage;
        Context mContext;
        public APIModelSelectionWizardPage(Context context)
        {
            InitializeComponent();
            mContext = context;
        }

        private void SetAPIModelsGrid()
        {
            xApiModelsGrid.Title = "API Models";
            xApiModelsGrid.SetTitleStyle((Style)TryFindResource("@ucGridTitleLightStyle"));

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationAPIModel.Name), Header = "API Name", ReadOnly = true, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationAPIModel.Description), Header = "Description", ReadOnly = true, AllowSorting = true });

            xApiModelsGrid.SetAllColumnsDefaultView(view);
            xApiModelsGrid.InitViewItems();

            xApiModelsGrid.DataSourceList = mAddApiModelActionWizardPage.AAMList;
            xApiModelsGrid.ValidationRules.Add(ucGrid.eUcGridValidationRules.CantBeEmpty);

            // xApiModelsGrid.btnClearAll.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteApiButtonClicked));
            xApiModelsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddApiButtonClicked));
            // xApiModelsGrid.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteApiButtonClicked));

            this.Visibility = Visibility.Hidden;

            if(mAddApiModelActionWizardPage.AAMList.Count == 0)
                OpenAPITreeSelection();

            this.Visibility = Visibility.Visible;
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mAddApiModelActionWizardPage = ((AddApiModelActionWizardPage)WizardEventArgs.Wizard);
                    SetAPIModelsGrid();
                    break;
            }
        }

        private void OpenAPITreeSelection()
        {
            if (apiModelTreeSelectionPage == null)
            {
                AppApiModelsFolderTreeItem apiRoot = new AppApiModelsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>());
                apiModelTreeSelectionPage = new SingleItemTreeViewSelectionPage("API Models", eImageType.APIModel, apiRoot, SingleItemTreeViewSelectionPage.eItemSelectionType.Multi, true,
                                                                                                    new System.Tuple<string, string>(nameof(ApplicationPOMModel.TargetApplicationKey) + "." +
                                                                                                                nameof(ApplicationPOMModel.TargetApplicationKey.ItemName),
                                                                                                                System.Convert.ToString(mContext.Activity.TargetApplication)));
            }
            List<object> selectedList = apiModelTreeSelectionPage.ShowAsWindow();

            if (selectedList != null)
            {
                //Todo: Add folder inside folder with API's adding support
                foreach (ApplicationAPIModel aamb in selectedList)
                {
                    mAddApiModelActionWizardPage.AAMList.Add(aamb);
                }                
            }
        }


        private void AddApiButtonClicked(object sender, RoutedEventArgs e)
        {
            OpenAPITreeSelection();
        }        
    }
}
