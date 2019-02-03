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
        SingleItemTreeViewSelectionPage mApplicationPOMSelectionPage = null;
        ApplicationPOMModel mSelectedPOM = null;
        RepositoryFolder<ApplicationPOMModel> mPOMModelFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>();       
        string mLocateValue;

        Object mObjectElementType;
        string mElementTypeFieldName;
        Object mObjectLocateValue;
        string mLocateValueFieldName;
        bool mOnlyPOMRequest;
        public delegate void ElementChangedEventHandler();

        public event ElementChangedEventHandler ElementChangedPageEvent;


        public void ElementChangedEvent()
        {
            if (ElementChangedPageEvent != null)
            {
                ElementChangedPageEvent();
            }
        }

        public LocateByPOMElementPage(Object objectElementType, string elementTypeFieldName, Object objectLocateValue, string locateValueFieldName, bool onlyPOMRequest = false)
        {
            InitializeComponent();

            mObjectElementType = objectElementType;
            mElementTypeFieldName = elementTypeFieldName;
            mObjectLocateValue = objectLocateValue;
            mLocateValueFieldName = locateValueFieldName;
            mOnlyPOMRequest = onlyPOMRequest;

            DataContext = this;

            SetControlsGridView();
            if(mOnlyPOMRequest)
            {
                HideElementSelection();
            }
            mLocateValue = (string)mObjectLocateValue.GetType().GetProperty(mLocateValueFieldName).GetValue(mObjectLocateValue);
            if (!string.IsNullOrWhiteSpace(mLocateValue))
            {
                try
                {
                    string[] pOMandElementGUIDs = mLocateValue.Split('_');
                    Guid selectedPOMGUID = new Guid(pOMandElementGUIDs[0]);
                    mSelectedPOM = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>(selectedPOMGUID);
                    if (mSelectedPOM == null)
                    {
                        Reporter.ToUser(eUserMsgKey.POMSearchByGUIDFailed);
                        mLocateValue = string.Empty;
                        SelectPOM_Click(null, null);
                    }
                    else
                    {
                        SetPOMPathToShow();
                        if (!mOnlyPOMRequest)
                        {
                            Guid selectedPOMElementGUID = new Guid(pOMandElementGUIDs[1]);
                            ElementInfo selectedPOMElement = (ElementInfo)mSelectedPOM.MappedUIElements.Where(z => z.Guid == selectedPOMElementGUID).FirstOrDefault();
                            if (selectedPOMElement == null)
                            {
                                Reporter.ToUser(eUserMsgKey.POMElementSearchByGUIDFailed);
                            }
                            else
                            {
                                xPOMElementsGrid.DataSourceList = GenerateElementsDataSourseList();

                                xPOMElementsGrid.Grid.SelectedItem = selectedPOMElement;
                                SetElementTypeProperty(selectedPOMElement.ElementTypeEnum);

                                xPOMElementTextBox.Text = selectedPOMElement.ElementName;
                                HighlightButton.IsEnabled = true;
                            }
                        }
                    }
                }
                catch
                {
                    Reporter.ToUser(eUserMsgKey.POMSearchByGUIDFailed);
                    mLocateValue = string.Empty;
                    SelectPOM_Click(null, null);
                }
            }
        }

        private ObservableList<ElementInfo> GenerateElementsDataSourseList()
        {
            ObservableList<ElementInfo> tempList = new ObservableList<ElementInfo>();
            foreach (ElementInfo EI in mSelectedPOM.MappedUIElements)
            {
                tempList.Add(EI);
            }
            return tempList;
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

                if (mOnlyPOMRequest)
                {
                    mObjectLocateValue.GetType().GetProperty(mLocateValueFieldName).SetValue(mObjectLocateValue, mSelectedPOM.Guid.ToString());
                }
                else
                {
                    xPOMElementsGrid.DataSourceList = GenerateElementsDataSourseList();
                    xPOMElementTextBox.Text = string.Empty;
                    mObjectLocateValue.GetType().GetProperty(mLocateValueFieldName).SetValue(mObjectLocateValue, string.Empty);
                    SetElementTypeProperty(eElementType.Unknown);
                }
                AllowElementSelection();
            }
        }

        private void SetElementTypeProperty(eElementType elementType)
        {
            if (mObjectElementType != null)
            {
                Type elementTypePropertyType = mObjectElementType.GetType().GetProperty(mElementTypeFieldName).PropertyType;

                if (elementTypePropertyType == typeof(eElementType))
                {
                    mObjectElementType.GetType().GetProperty(mElementTypeFieldName).SetValue(mObjectElementType, elementType);
                }
                else if (elementTypePropertyType == typeof(string))
                {
                    mObjectElementType.GetType().GetProperty(mElementTypeFieldName).SetValue(mObjectElementType, elementType.ToString());
                }
            }
        }

        private void HideElementSelection()
        {
            xPOMElementsLbl.Visibility = Visibility.Collapsed;
            ArrowDownButton.Visibility = Visibility.Collapsed;
            HighlightButton.Visibility = Visibility.Collapsed;
            xPOMElementTextBox.Visibility = Visibility.Collapsed;
            xPOMTitleLbl.Visibility = Visibility.Collapsed;
            xPOMGrid.ColumnDefinitions[0].Width = new GridLength(0);
        }

        private void AllowElementSelection()
        {
            if (!mOnlyPOMRequest)
            {
                xPOMElementsLbl.Visibility = Visibility.Visible;
                ArrowDownButton.Visibility = Visibility.Visible;
                HighlightButton.Visibility = Visibility.Visible;
                xPOMElementTextBox.Visibility = Visibility.Collapsed;
                xPOMTitleLbl.Visibility = Visibility.Visible;
                xPOMElementsGrid.Visibility = Visibility.Visible;
                xSelectElement.Visibility = Visibility.Visible;
                xPOMElementsGrid.Refresh();
                ArrowExpended = true;
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

        private void SelectElement_Click(object sender, RoutedEventArgs e)
        {
            AllowElementSelection();
        }

        private void EndSelectingElement()
        {
            xPOMElementTextBox.Visibility = Visibility.Visible;
            xPOMElementsGrid.Visibility = Visibility.Collapsed;
            xSelectElement.Visibility = Visibility.Collapsed;
            ArrowExpended = false;
            if ((ElementInfo)xPOMElementsGrid.Grid.SelectedItem != null)
            {
                xPOMElementTextBox.Text = ((ElementInfo)xPOMElementsGrid.Grid.SelectedItem).ElementName;
                string POMAndElementGuids = mSelectedPOM.Guid.ToString() + "_" + ((ElementInfo)xPOMElementsGrid.Grid.SelectedItem).Guid.ToString();
                mObjectLocateValue.GetType().GetProperty(mLocateValueFieldName).SetValue(mObjectLocateValue, POMAndElementGuids);
                SetElementTypeProperty(((ElementInfo)xPOMElementsGrid.Grid.SelectedItem).ElementTypeEnum);
            }


            HighlightButton.IsEnabled = true;
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

        private void HighlightElementClicked(object sender, RoutedEventArgs e)
        {
            ApplicationAgent currentAgent = (ApplicationAgent)App.AutomateTabGingerRunner.ApplicationAgents.Where(z => z.AppName == App.BusinessFlow.CurrentActivity.TargetApplication).FirstOrDefault();
            if ((currentAgent == null) || !(((Agent)currentAgent.Agent).Driver is IWindowExplorer) || (((Agent)currentAgent.Agent).Status != Agent.eStatus.Running))
            {
                Reporter.ToUser(eUserMsgKey.NoRelevantAgentInRunningStatus);
            }
            else
            {
                ((IWindowExplorer)((Agent)currentAgent.Agent).Driver).HighLightElement((ElementInfo)xPOMElementsGrid.Grid.SelectedItem, true);
            }
        }


        bool ArrowExpended = false;
        private void ArrowDownClicked(object sender, RoutedEventArgs e)
        {
            if (ArrowExpended)
            {
                EndSelectingElement();
            }
            else
            {
                AllowElementSelection();
            }
        }

        private void SelectElementsClicked(object sender, RoutedEventArgs e)
        {
            SetSelectedElement();
        }
    
        private void XPOMElementsGrid_RowDoubleClick(object sender, EventArgs e)
        {
            SetSelectedElement();
        }

        private void SetSelectedElement()
        {
            EndSelectingElement();
            ElementChangedEvent();
        }

        private void XPOMElementTextBox_MouseClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AllowElementSelection();
        }
    }
}
