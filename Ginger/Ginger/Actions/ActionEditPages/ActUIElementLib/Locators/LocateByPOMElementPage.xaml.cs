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
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.Run;
using System.Windows.Media.Imaging;
using Ginger.Actions.UserControls;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for LocateValueEditPage.xaml
    /// </summary>
    public partial class LocateByPOMElementPage : Page
    {
        SingleItemTreeViewSelectionPage mApplicationPOMSelectionPage = null;
        public ApplicationPOMModel SelectedPOM = null;
        RepositoryFolder<ApplicationPOMModel> mPOMModelFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationPOMModel>();       
        string mLocateValue;
        public bool OnlyPOMSelection { get; set; }

        Object mObjectElementType;
        string mElementTypeFieldName;
        Object mObjectLocateValue;
        string mLocateValueFieldName;
        bool mOnlyPOMRequest;

        public delegate void ElementChangedEventHandler();
        public event ElementChangedEventHandler ElementChangedPageEvent;

        public delegate void POMChangedEventHandler();
        public event POMChangedEventHandler POMChangedPageEvent;

        Context mContext;

        string mTargetApplication;

        public void ElementChangedEvent()
        {
            if (ElementChangedPageEvent != null)
            {
                ElementChangedPageEvent();
            }
        }

        public void POMChangedEvent()
        {
            if (POMChangedPageEvent != null)
            {
                POMChangedPageEvent();
            }
        }

        public LocateByPOMElementPage(Context context, Object objectElementType, string elementTypeFieldName, Object objectLocateValue, string locateValueFieldName, bool onlyPOMRequest = false)
        {
            InitializeComponent();

            mObjectElementType = objectElementType;
            mElementTypeFieldName = elementTypeFieldName;
            mObjectLocateValue = objectLocateValue;
            mLocateValueFieldName = locateValueFieldName;
            mOnlyPOMRequest = onlyPOMRequest;
            mContext = context;

            if (mContext != null && mContext.BusinessFlow != null)//temp wrokaround, full is in Master 
            {
                if (mContext.BusinessFlow.CurrentActivity != null)
                {
                    mTargetApplication = mContext.BusinessFlow.CurrentActivity.TargetApplication;
                }
                else if (mContext.Activity != null)
                {
                    mTargetApplication = mContext.Activity.TargetApplication;
                }
            }
            else
            {
                mTargetApplication = WorkSpace.Instance.Solution.MainApplication;
            }
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
                    SelectedPOM = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>(selectedPOMGUID);
                    if (SelectedPOM == null)
                    {
                        Reporter.ToUser(eUserMsgKey.POMSearchByGUIDFailed);
                        mLocateValue = string.Empty;
                        SelectPOM_Click(null, null);
                    }
                    else
                    {
                        SetPOMPathToShow(onlyPOMRequest);
                        if (!mOnlyPOMRequest)
                        {
                            xPOMElementsGrid.DataSourceList = GenerateElementsDataSourseList();

                            Guid selectedPOMElementGUID = new Guid(pOMandElementGUIDs[1]);
                            ElementInfo selectedPOMElement = (ElementInfo)SelectedPOM.MappedUIElements.Where(z => z.Guid == selectedPOMElementGUID).FirstOrDefault();
                            if (selectedPOMElement == null)
                            {
                                Reporter.ToUser(eUserMsgKey.POMElementSearchByGUIDFailed);
                            }
                            else
                            {                                
                                xPOMElementsGrid.Grid.SelectedItem = selectedPOMElement;
                                //SetElementTypeProperty(selectedPOMElement.ElementTypeEnum); //we don't want it to overwrite user type selection in case it is diffrent from element type                                
                                SetElementViewText(selectedPOMElement.ElementName, selectedPOMElement.ElementTypeEnum.ToString());
                                HighlightButton.IsEnabled = true;

                                UpdatedElementScreenshot(selectedPOMElement);
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

        private void SetElementViewText(string elementName, string elementType)
        {
            xPOMElementTextBox.Text = string.Format("{0} [{1}]", elementName, elementType);
        }

        private ObservableList<ElementInfo> GenerateElementsDataSourseList()
        {
            ObservableList<ElementInfo> tempList = new ObservableList<ElementInfo>();
            foreach (ElementInfo EI in SelectedPOM.MappedUIElements)
            {
                tempList.Add(EI);
            }
            return tempList;
        }

        public void SelectPOM_Click(object sender, RoutedEventArgs e)
        {
            if (mApplicationPOMSelectionPage == null)
            {
                ApplicationPOMsTreeItem pOMsRoot = new ApplicationPOMsTreeItem(mPOMModelFolder);              
                mApplicationPOMSelectionPage = new SingleItemTreeViewSelectionPage("Page Objects Model Element", eImageType.ApplicationPOMModel, pOMsRoot,
                                                                                    SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true,
                                                                                    new Tuple<string, string>(  nameof(ApplicationPOMModel.TargetApplicationKey) + "." +
                                                                                                                nameof(ApplicationPOMModel.TargetApplicationKey.ItemName),                                                                                                                 
                                                                                                                mTargetApplication));
            }

            List<object> selectedPOMs = mApplicationPOMSelectionPage.ShowAsWindow();
            if (selectedPOMs != null && selectedPOMs.Count > 0)
            {
                SelectedPOM = (ApplicationPOMModel)selectedPOMs[0];
                POMChangedEvent();
                UpdatePomSelection();
            }
        }

        private void UpdatePomSelection()
        {
            SetPOMPathToShow();
            if (mOnlyPOMRequest)
            {
                mObjectLocateValue.GetType().GetProperty(mLocateValueFieldName).SetValue(mObjectLocateValue, SelectedPOM.Guid.ToString());
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
            xPOMElementTextBox.Visibility = Visibility.Collapsed;
            ArrowDownButton.Visibility = Visibility.Collapsed;
            HighlightButton.Visibility = Visibility.Collapsed;
            xPOMTitleLbl.Visibility = Visibility.Collapsed;
            xPOMGrid.ColumnDefinitions[0].Width = new GridLength(0);

            xElementScreenShotFrame.Visibility = Visibility.Visible;
        }

        private void AllowElementSelection()
        {
            if (!mOnlyPOMRequest)
            {
                xPOMElementsLbl.Visibility = Visibility.Visible;
                xPOMElementTextBox.Visibility = Visibility.Visible;
                ArrowDownButton.Visibility = Visibility.Visible;
                HighlightButton.Visibility = Visibility.Visible;
                xPOMTitleLbl.Visibility = Visibility.Visible;
                xPOMElementsGrid.Visibility = Visibility.Visible;
                xSelectElement.Visibility = Visibility.Visible;
                xPOMElementsGrid.Refresh();
                ArrowExpended = true;

                xElementScreenShotFrame.Visibility = Visibility.Collapsed;
            }
        }

        private void SetPOMPathToShow(bool onlyPOMRequest = false)
        {
            //string pathToShow;
            //pathToShow = mSelectedPOM.FilePath.Substring(0, mSelectedPOM.FilePath.LastIndexOf("\\")).Substring(mPOMModelFolder.FolderFullPath.Length) + @"\" + mSelectedPOM.ItemName;
            xPomPathTextBox.Text = SelectedPOM.NameWithRelativePath; 
            xViewPOMBtn.Visibility = Visibility.Visible;
            if (onlyPOMRequest)
            {
                xViewPOMElementBtn.Visibility = Visibility.Visible;
            }
            
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
            xElementScreenShotFrame.Visibility = Visibility.Collapsed;
            ArrowExpended = false;            
            if (xPOMElementsGrid.Grid.SelectedItem != null)
            {
                ElementInfo selectedElement = (ElementInfo)xPOMElementsGrid.Grid.SelectedItem;
                string pomAndElementGuids = SelectedPOM.Guid.ToString() + "_" + selectedElement.Guid.ToString();
                mObjectLocateValue.GetType().GetProperty(mLocateValueFieldName).SetValue(mObjectLocateValue, pomAndElementGuids);
                SetElementTypeProperty(selectedElement.ElementTypeEnum);
                SetElementViewText(selectedElement.ElementName, selectedElement.ElementTypeEnum.ToString());
                HighlightButton.IsEnabled = true;
                UpdatedElementScreenshot(selectedElement);
            }
        }

        private void UpdatedElementScreenshot(ElementInfo selectedElement)
        {
            //update screenshot
            BitmapSource source = null;
            if (selectedElement.ScreenShotImage != null)
            {
                source = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(selectedElement.ScreenShotImage.ToString()));
                xElementScreenShotFrame.Content = new ScreenShotViewPage(selectedElement?.ElementName, source, false);
                xElementScreenShotFrame.Visibility = Visibility.Visible;
            }
        }

        private void SetControlsGridView()
        {
            xPOMElementsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementName), Header = "Name", WidthWeight = 30, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Description), Header = "Description", WidthWeight = 30, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeEnumDescription), Header = "Type", WidthWeight = 40, AllowSorting = true, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.OptionalValuesObjectsListAsString), Header = "Possible Values", WidthWeight = 40, ReadOnly = true, BindingMode = BindingMode.OneWay});
            xPOMElementsGrid.SetAllColumnsDefaultView(view);
            xPOMElementsGrid.InitViewItems();
        }

        private void HighlightElementClicked(object sender, RoutedEventArgs e)
        {
            ApplicationAgent currentAgent = (ApplicationAgent)((GingerExecutionEngine)mContext.Runner).GingerRunner.ApplicationAgents.Where(z => z.AppName == mTargetApplication).FirstOrDefault();
            if ((currentAgent == null) || !(((AgentOperations)((Agent)currentAgent.Agent).AgentOperations).Driver is IWindowExplorer) || (((AgentOperations)((Agent)currentAgent.Agent).AgentOperations).Status != Agent.eStatus.Running))
            {
                Reporter.ToUser(eUserMsgKey.NoRelevantAgentInRunningStatus);
            }
            else
            {
                ((IWindowExplorer)((AgentOperations)((Agent)currentAgent.Agent).AgentOperations).Driver).HighLightElement((ElementInfo)xPOMElementsGrid.Grid.SelectedItem, true);
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

        private void XViewPOMBtn_Click(object sender, RoutedEventArgs e)
        {
            POMEditPage mPOMEditPage = new POMEditPage(SelectedPOM, General.eRIPageViewMode.Standalone);
            mPOMEditPage.ShowAsWindow(eWindowShowStyle.Dialog);

            //refresh Elements list
            if(SelectedPOM.DirtyStatus == eDirtyStatus.Modified || mPOMEditPage.IsPageSaved)
            {
                UpdatePomSelection();
            }
        }

        private void XViewPOMElementBtn_Click(object sender, RoutedEventArgs e)
        {
            POMEditPage mPOMEditPage = new POMEditPage(SelectedPOM, General.eRIPageViewMode.Standalone);
            mPOMEditPage.ShowAsWindow(eWindowShowStyle.Dialog);

            //refresh Elements list
            if (SelectedPOM.DirtyStatus == eDirtyStatus.Modified || mPOMEditPage.IsPageSaved)
            {
                UpdatePomSelection();
            }
        }
    }
}
