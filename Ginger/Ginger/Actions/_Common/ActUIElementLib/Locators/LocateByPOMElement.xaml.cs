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

using System.Windows;
using System.Windows.Controls;
using GingerCore.Actions.Common;
using Ginger.SolutionWindows.TreeViewItems.ApplicationModelsTreeItems;
using GingerWPF.UserControlsLib.UCTreeView;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;
using GingerCore;
using Amdocs.Ginger.Common.Enums;
using System.Collections.Generic;
using Amdocs.Ginger.Common.UIElement;
using Ginger.UserControls;
using Amdocs.Ginger.Common;
using System.Windows.Data;
using System;
using System.Linq;
using GingerCore.Platforms;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for LocateValueEditPage.xaml
    /// </summary>
    public partial class LocateByPOMElement : Page
    {
        ActUIElement mAction;
        SingleItemTreeViewSelectionPage mApplicationPOMSelectionPage = null;
        ApplicationPOMModel mSelectedPOM = null;
        RepositoryFolder<ApplicationPOMModel> mPOMModelFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>();
        //mWinExplorer = winExplorer;

        public LocateByPOMElement(ActUIElement Action)
        {
            InitializeComponent();
            DataContext = this;
            mAction = Action;
            SetControlsGridView();

            if (Action.ElementLocateBy == eLocateBy.POMElement)
            {
                if ((mAction.ElementLocateValue != null) && (mAction.ElementLocateValue != string.Empty))
                {
                    try
                    {
                        string[] pOMandElementGUIDs = mAction.ElementLocateValue.ToString().Split('_');
                        Guid selectedPOMGUID = new Guid(pOMandElementGUIDs[0]);
                        mSelectedPOM = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>(selectedPOMGUID);
                        if (mSelectedPOM == null)
                        {
                            Reporter.ToUser(eUserMsgKeys.POMSearchByGUIDFailed);
                            mAction.ElementLocateValue = string.Empty;
                            SelectPOM_Click(null, null);
                        }
                        else
                        {
                            SetPOMPathToShow();
                            Guid selectedPOMElementGUID = new Guid(pOMandElementGUIDs[1]);
                            ElementInfo selectedPOMElement = (ElementInfo)mSelectedPOM.MappedUIElements.Where(z => z.Guid == selectedPOMElementGUID).FirstOrDefault();
                            if (selectedPOMElement == null)
                            {
                                Reporter.ToUser(eUserMsgKeys.POMElementSearchByGUIDFailed);
                            }
                            else
                            {
                                xPOMElementsGrid.DataSourceList = mSelectedPOM.MappedUIElements;
                                xPOMElementsGrid.Grid.SelectedItem = selectedPOMElement;
                                mAction.ElementTypeDelegated = selectedPOMElement.ElementTypeEnum;
                                POMElementComboBox.IsEnabled = true;
                                HeaderTextBlock.Text = (selectedPOMElement.ElementName + " - " + selectedPOMElement.ElementTypeEnumDescription).ToString(); 
                                HeaderTextBlock.Visibility = Visibility.Visible;
                                POMElementComboBox.SelectedItem = HeaderTextBlock;
                                HighlightButton.IsEnabled = true;
                            }
                        }
                    }
                    catch
                    {
                        Reporter.ToUser(eUserMsgKeys.POMSearchByGUIDFailed);
                        mAction.ElementLocateValue = string.Empty;
                        SelectPOM_Click(null, null);
                    }
                }
            }
        }

        private void SelectPOM_Click(object sender, RoutedEventArgs e)
        {
            if (mApplicationPOMSelectionPage == null)
            {
                ApplicationPOMsTreeItem pOMsRoot = new ApplicationPOMsTreeItem(mPOMModelFolder);
                //mApplicationPOMSelectionPage = new SingleItemTreeViewSelectionPage( GingerDicser.GetTermResValue(eTermResKey.POM), eImageType.ApplicationPOM, pOMsRoot, 
                //                                                                    SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true);

                mApplicationPOMSelectionPage = new SingleItemTreeViewSelectionPage(GingerDicser.GetTermResValue(eTermResKey.POM), eImageType.ApplicationPOM, pOMsRoot,
                                                                                    SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true,
                                                                                    new Tuple<string, string>(  nameof(ApplicationPOMModel.TargetApplicationKey) + "." +
                                                                                                                nameof(ApplicationPOMModel.TargetApplicationKey.ItemName), 
                                                                                                                App.BusinessFlow.CurrentActivity.TargetApplication));
            }

            List<object> selectedPOMs = mApplicationPOMSelectionPage.ShowAsWindow();
            if (selectedPOMs != null && selectedPOMs.Count > 0)
            {
                mSelectedPOM = (ApplicationPOMModel)selectedPOMs[0];
                SetPOMPathToShow();

                xPOMElementsGrid.DataSourceList = mSelectedPOM.MappedUIElements;
                POMElementComboBox.IsEnabled = true;
            }
        }

        private void SetPOMPathToShow()
        {
            string pathToShow;
            pathToShow = mSelectedPOM.FilePath.Substring(0, mSelectedPOM.FilePath.LastIndexOf("\\")).Substring(mPOMModelFolder.FolderFullPath.Length) + @"\" + mSelectedPOM.ItemName;
            HTMLReportFolderTextBox.Text = pathToShow;
        }

        private void POMElementComboBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void POMElementComboBox_DropDownOpened(object sender, System.EventArgs e)
        {
            HeaderTextBlock.Visibility = Visibility.Collapsed;
            xPOMElementsGrid.Refresh();
        }

        private void POMElementComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            HeaderTextBlock.Visibility = Visibility.Visible;
            HeaderTextBlock.Text = (((ElementInfo)xPOMElementsGrid.Grid.SelectedItem).ElementName + " - " + ((ElementInfo)xPOMElementsGrid.Grid.SelectedItem).ElementTypeEnumDescription).ToString();
            POMElementComboBox.SelectedItem = HeaderTextBlock;
        }

        private void SetControlsGridView()
        {
            xPOMElementsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementName), Header = "Name", WidthWeight = 30, AllowSorting = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Description), Header = "Description", WidthWeight = 30, AllowSorting = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeEnumDescription), Header = "Type", WidthWeight = 40, AllowSorting = true, BindingMode = BindingMode.OneWay });
            xPOMElementsGrid.SetAllColumnsDefaultView(view);
            xPOMElementsGrid.InitViewItems();
        }

        private void xPOMElementsGrid_RowChangedEvent(object sender, System.EventArgs e)
        {
            if ((POMElementComboBox.IsEnabled) && ((DataGrid)sender).SelectedItem != null)
            {
                mAction.ElementTypeDelegated = ((ElementInfo)xPOMElementsGrid.Grid.SelectedItem).ElementTypeEnum;
                mAction.ElementLocateValue = mSelectedPOM.Guid.ToString() + "_" + ((ElementInfo)xPOMElementsGrid.Grid.SelectedItem).Guid.ToString();
                HighlightButton.IsEnabled = true;
            }
        }

        private void HighlightElementClicked(object sender, RoutedEventArgs e)
        {
            ApplicationAgent currentAgent = App.AutomateTabGingerRunner.ApplicationAgents.Where(z => z.AppName == App.BusinessFlow.CurrentActivity.TargetApplication).FirstOrDefault();
            if ((currentAgent == null) || !(currentAgent.Agent.Driver is IWindowExplorer) || (currentAgent.Agent.Status != Agent.eStatus.Running))
            {
                Reporter.ToUser(eUserMsgKeys.NoRelevantAgentInRunningStatus);
            }
            else
            {
                ((IWindowExplorer)currentAgent.Agent.Driver).HighLightElement((ElementInfo)xPOMElementsGrid.Grid.SelectedItem, true);
            }

        }
    }
}
