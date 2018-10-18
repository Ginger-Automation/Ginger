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
using GingerCore.Actions;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for LocateValueEditPage.xaml
    /// </summary>
    public partial class LocateByPOMElementPage : Page
    {
        Act mAction;
        SingleItemTreeViewSelectionPage mApplicationPOMSelectionPage = null;
        ApplicationPOMModel mSelectedPOM = null;
        RepositoryFolder<ApplicationPOMModel> mPOMModelFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>();       
        eLocateBy mLocateBy;
        string mLocateValue;

        public LocateByPOMElementPage(Act Action)
        {
            InitializeComponent();
            DataContext = this;
            mAction = Action;
            SetControlsGridView();

            if (mAction is ActUIElement)
            {
                mLocateBy = ((ActUIElement)mAction).ElementLocateBy;
                mLocateValue = ((ActUIElement)mAction).ElementLocateValue;
            }
            else
            {
                mLocateBy = mAction.LocateBy;
                mLocateValue = mAction.LocateValue;
            }

            if (mLocateBy == eLocateBy.POMElement)
            {
                if ((mLocateValue != null) && (mLocateValue != string.Empty))
                {
                    try
                    {
                        string[] pOMandElementGUIDs = mLocateValue.Split('_');
                        Guid selectedPOMGUID = new Guid(pOMandElementGUIDs[0]);
                        mSelectedPOM = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>(selectedPOMGUID);
                        if (mSelectedPOM == null)
                        {
                            Reporter.ToUser(eUserMsgKeys.POMSearchByGUIDFailed);
                            mLocateValue = string.Empty;
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
                                if (mAction is ActUIElement)
                                {
                                    ((ActUIElement)mAction).ElementType = selectedPOMElement.ElementTypeEnum;
                                }
                                xPOMElementComboBox.IsEnabled = true;
                                xHeaderTextBlock.Text = selectedPOMElement.ElementName; 
                                xHeaderTextBlock.Visibility = Visibility.Visible;
                                xPOMElementComboBox.SelectedItem = xHeaderTextBlock;
                                HighlightButton.IsEnabled = true;
                            }
                        }
                    }
                    catch
                    {
                        Reporter.ToUser(eUserMsgKeys.POMSearchByGUIDFailed);
                        mLocateValue = string.Empty;
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
                mApplicationPOMSelectionPage = new SingleItemTreeViewSelectionPage("Page Objects Model Element", eImageType.ApplicationPOMModel, pOMsRoot,
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
                xPOMElementComboBox.IsEnabled = true;
            }
        }

        private void SetPOMPathToShow()
        {
            string pathToShow;
            pathToShow = mSelectedPOM.FilePath.Substring(0, mSelectedPOM.FilePath.LastIndexOf("\\")).Substring(mPOMModelFolder.FolderFullPath.Length) + @"\" + mSelectedPOM.ItemName;
            xHTMLReportFolderTextBox.Text = pathToShow;
        }

        private void POMElementComboBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void POMElementComboBox_DropDownOpened(object sender, System.EventArgs e)
        {
            xHeaderTextBlock.Visibility = Visibility.Collapsed;
            xPOMElementsGrid.Refresh();
        }

        private void POMElementComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            xHeaderTextBlock.Visibility = Visibility.Visible;
            xHeaderTextBlock.Text = ((ElementInfo)xPOMElementsGrid.Grid.SelectedItem).ElementName;
            xPOMElementComboBox.SelectedItem = xHeaderTextBlock;
        }

        private void SetControlsGridView()
        {
            xPOMElementsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementName), Header = "Name", WidthWeight = 30, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Description), Header = "Description", WidthWeight = 30, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeEnumDescription), Header = "Type", WidthWeight = 40, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            xPOMElementsGrid.SetAllColumnsDefaultView(view);
            xPOMElementsGrid.InitViewItems();
        }

        private void xPOMElementsGrid_RowChangedEvent(object sender, System.EventArgs e)
        {
            if ((xPOMElementComboBox.IsEnabled) && ((DataGrid)sender).SelectedItem != null)
            {
                if (mAction is ActUIElement)
                {
                    ((ActUIElement)mAction).ElementType = ((ElementInfo)xPOMElementsGrid.Grid.SelectedItem).ElementTypeEnum;
                }
                mLocateValue = mSelectedPOM.Guid.ToString() + "_" + ((ElementInfo)xPOMElementsGrid.Grid.SelectedItem).Guid.ToString();

                if (mAction is ActUIElement)
                {
                    ((ActUIElement)mAction).ElementLocateValue = mSelectedPOM.Guid.ToString() + "_" + ((ElementInfo)xPOMElementsGrid.Grid.SelectedItem).Guid.ToString();
                }
                else
                {
                    mAction.LocateValue = mSelectedPOM.Guid.ToString() + "_" + ((ElementInfo)xPOMElementsGrid.Grid.SelectedItem).Guid.ToString();
                }

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
