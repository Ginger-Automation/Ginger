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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.Repository;
using Ginger.Actions.UserControls;
using Ginger.UserControls;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCoreNET.Application_Models;
using GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static GingerCore.General;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.ApplicationModelsLib.POMModels
{
    /// <summary>
    /// Interaction logic for PomDeltaViewPage.xaml
    /// </summary>
    public partial class PomDeltaViewPage : Page
    {
        ObservableList<DeltaElementInfo> mDeltaElements;
        ComboEnumItem mCurrentGroupByFilter;
        ComboEnumItem mCurrentDeltaStatusFilter;

        bool IsFirstSelection = true;
        bool isEnableFriendlyLocator = false;
        GridColView mGridCompareColumn;

        private Agent mAgent;
        IWindowExplorer mWinExplorer
        {
            get
            {
                if (mAgent != null && ((AgentOperations)mAgent.AgentOperations).Status == Agent.eStatus.Running)
                {
                    return ((AgentOperations)mAgent.AgentOperations).Driver as IWindowExplorer;
                }
                else
                {
                    if (mAgent != null)
                    {
                        mAgent.AgentOperations.Close();
                    }
                    return null;
                }
            }
        }

       internal DeltaElementInfo mSelectedElement
        {
            get
            {
                if (xMainElementsGrid.Grid.SelectedItem != null)
                {
                        return (DeltaElementInfo)xMainElementsGrid.Grid.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        DeltaElementLocator mSelectedLocator
        {
            get
            {
                if (xLocatorsGrid.Grid.SelectedItem != null)
                {
                    return (DeltaElementLocator)xLocatorsGrid.Grid.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        public PomDeltaViewPage(ObservableList<DeltaElementInfo> deltaElements = null, GridColView gridCompareColumn = null,Agent mAgent=null)
        {
            InitializeComponent();

            mDeltaElements = deltaElements;
            
            if (gridCompareColumn != null)
            {
                mGridCompareColumn = gridCompareColumn;
            }
            if (mAgent.Platform == ePlatformType.Web)
            {
                isEnableFriendlyLocator = true;
                xFriendlyLocatorTab.Visibility = Visibility.Visible;
            }
            else
            {
                xFriendlyLocatorTab.Visibility = Visibility.Collapsed;
            }

            SetDeltaElementsGridView();
            SetDeltaLocatorsGridView();
            
            if (isEnableFriendlyLocator)
            {
                SetDeltaFriendlyLocatorsGridView();
            }
            SetDeltaControlPropertiesGridView();

            xMainElementsGrid.DataSourceList = mDeltaElements;

            if (mDeltaElements.Count > 0)
            {
                xMainElementsGrid.Grid.SelectedItem = mDeltaElements[0];
            }
            else
            {
                DisableDetailsExpander();
            }
        }

        private void Properties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdatePropertiesHeader();
        }
        private void UpdatePropertiesHeader()
        {
            Dispatcher.Invoke(() =>
            {
                xPropertiesTextBlock.Text = string.Format("Properties ({0})", mSelectedElement.Properties.Count);
            });
        }

        private void Locators_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateLocatorsHeader();
            
        }

        private void FriendlyLocators_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateFriendlyLocatorsHeader();
        }
        private void UpdateLocatorsHeader()
        {
            Dispatcher.Invoke(() =>
            {
                xLocatorsTextBlock.Text = string.Format("Locators ({0})", mSelectedElement.Locators.Count);
            });
        }

        private void UpdateFriendlyLocatorsHeader()
        {
            Dispatcher.Invoke(() =>
            {
                xFriendlyLocatorsTextBlock.Text = string.Format("Friendly Locators ({0})", mSelectedElement.FriendlyLocators.Count);
            });
        }

        internal void SetAgent(Agent agent)
        {
            mAgent = agent;
        }

        private void SetDeltaElementsGridView()
        {
            xMainElementsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.ElementTypeImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.IsSelected), Header = "Update", WidthWeight = 60, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox, AllowSorting = true});            
            List<ComboEnumItem> GroupTypeList = GetEnumValuesForCombo(typeof(ApplicationPOMModel.eElementGroup));
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.SelectedElementGroup), Header = "Group", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(GroupTypeList, nameof(DeltaElementInfo.SelectedElementGroup), true), WidthWeight = 200 });           
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.ElementName), Header = "Name", WidthWeight = 200, AllowSorting = true, ReadOnly = true, BindingMode=BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.Description), Header = "Description", WidthWeight = 200, AllowSorting = true, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.OptionalValuesObjectsListAsString), Header = "Possible Values", WidthWeight = 200, AllowSorting = true, ReadOnly = true, BindingMode = BindingMode.OneWay });
            List<ComboEnumItem> ElementTypeList = GetEnumValuesForCombo(typeof(eElementType));
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.ElementTypeEnum), Header = "Type", WidthWeight = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = ElementTypeList, ReadOnly = true, BindingMode = BindingMode.OneWay });            
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.IsAutoLearned), Header = "Auto Learned", WidthWeight = 250, MaxWidth = 100, AllowSorting = true, ReadOnly = true, BindingMode = BindingMode.OneWay });
            //view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.StatusIcon), Header = "Identification Status", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.DeltaStatusIcon), Header = "Comparison Status", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xDeltaStatusIconTemplate"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementInfo.DeltaExtraDetails), WidthWeight = 300, Header = "Comparison Details", AllowSorting = true, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = "", Header = "Highlight", WidthWeight = 80, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xHighlightButtonTemplate"] });
            
            if (mGridCompareColumn !=null)
            {
                view.GridColsView.Add(mGridCompareColumn);
            }

            xMainElementsGrid.SetAllColumnsDefaultView(view);
            xMainElementsGrid.InitViewItems();

            xMainElementsGrid.ShowPaste = Visibility.Collapsed;
            xMainElementsGrid.ShowAdd = Visibility.Collapsed;
            xMainElementsGrid.ShowDelete = Visibility.Collapsed;
            xMainElementsGrid.ShowCopy = Visibility.Collapsed;
            xMainElementsGrid.btnMarkAll.Visibility = Visibility.Visible;
            xMainElementsGrid.MarkUnMarkAllActive += MarkUnMarkAllDeltaUpdates;
            xMainElementsGrid.AddComboBoxToolbarTool("Group Filter:", typeof(ApplicationPOMModel.eElementGroup), GroupFilter_SelectionChanged, "All");
            xMainElementsGrid.AddComboBoxToolbarTool("Status Filter:", typeof(eDeltaStatus), ComperisonStatusFilter_SelectionChanged, "All");

            xMainElementsGrid.SelectedItemChanged += XMainElementsGrid_SelectedItemChanged;
            xMainElementsGrid.Grid.SelectionChanged += Grid_SelectionChanged;
        }

        private void ComperisonStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mCurrentDeltaStatusFilter = (ComboEnumItem)e.AddedItems[0];
            FilterListByFiltersCreteria();
        }

        private void GroupFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mCurrentGroupByFilter = (ComboEnumItem)e.AddedItems[0];// ((ComboBox)sender).SelectedItem;
            FilterListByFiltersCreteria();
        }

        private void FilterListByFiltersCreteria()
        {
            if ((mCurrentGroupByFilter == null || mCurrentGroupByFilter.text == "All") && (mCurrentDeltaStatusFilter == null || mCurrentDeltaStatusFilter.text == "All"))
            {
                xMainElementsGrid.DataSourceList = mDeltaElements;
            }
            else if (mCurrentGroupByFilter == null || mCurrentGroupByFilter.text == "All")
            {
                xMainElementsGrid.DataSourceList = GingerCore.General.ConvertListToObservableList(mDeltaElements.Where(x => ((DeltaElementInfo)x).DeltaStatus == (eDeltaStatus)mCurrentDeltaStatusFilter.Value).ToList());
            }
            else if (mCurrentDeltaStatusFilter == null || mCurrentDeltaStatusFilter.text == "All")
            {
                xMainElementsGrid.DataSourceList = GingerCore.General.ConvertListToObservableList(mDeltaElements.Where(x => x.SelectedElementGroup.ToString() == mCurrentGroupByFilter.Value.ToString()).ToList());
            }
            else
            {
                xMainElementsGrid.DataSourceList = GingerCore.General.ConvertListToObservableList(mDeltaElements.Where(x => x.SelectedElementGroup.ToString() == mCurrentGroupByFilter.Value.ToString() && ((DeltaElementInfo)x).DeltaStatus == (eDeltaStatus)mCurrentDeltaStatusFilter.Value).ToList());
            }
        }

        private void MarkUnMarkAllDeltaUpdates(bool Status)
        {           
            foreach (DeltaElementInfo EI in xMainElementsGrid.DataSourceList)
            {                
                    EI.IsSelected = Status;                
            }
        }

        private void XMainElementsGrid_SelectedItemChanged(object selectedItem)
        {
            HandelElementSelectionChange();
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandelElementSelectionChange();
        }

        private void SetDeltaLocatorsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.Active), WidthWeight = 50, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox, ReadOnly = true, BindingMode = BindingMode.OneWay });
            List<ComboEnumItem> locateByList = GingerCore.General.GetEnumValuesForCombo(typeof(eLocateBy));
            ComboEnumItem comboItem = locateByList.Where(x => ((eLocateBy)x.Value) == eLocateBy.POMElement).FirstOrDefault();
            if (comboItem != null)
            {
                locateByList.Remove(comboItem);
            }
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.LocateBy), Header = "Locate By", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList, ReadOnly = true, BindingMode = BindingMode.OneWay });
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.LocateValue), Header = "Locate Value", WidthWeight = 200, ReadOnly = true, BindingMode = BindingMode.OneWay });
            defView.GridColsView.Add(new GridColView() { Field = "", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xCopyLocatorButtonTemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.EnableFriendlyLocator),Visible = isEnableFriendlyLocator, Header = "Friendly Locator", WidthWeight = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox, BindingMode = BindingMode.OneWay });
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 100, ReadOnly = true, BindingMode = BindingMode.OneWay });           
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.DeltaStatusIcon), Header = "Comparison Status", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xDeltaStatusIconTemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.DeltaExtraDetails), Header = "Comparison Details", WidthWeight = 300, AllowSorting = true, ReadOnly = true, BindingMode = BindingMode.OneWay });
            defView.GridColsView.Add(new GridColView() { Field = "Test", WidthWeight = 50, MaxWidth = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestElementButtonTemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.StatusIcon), Header = "Identification Status", WidthWeight = 100, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });
            xLocatorsGrid.SetAllColumnsDefaultView(defView);
            xLocatorsGrid.InitViewItems();

            xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            xLocatorsGrid.ShowCopy = Visibility.Collapsed;
            xLocatorsGrid.ShowPaste = Visibility.Collapsed;
            xLocatorsGrid.ShowAdd = Visibility.Collapsed;
            xLocatorsGrid.ShowDelete = Visibility.Collapsed;
            xLocatorsGrid.ShowUpDown = Visibility.Collapsed;
            xLocatorsGrid.AddToolbarTool(eImageType.Run, "Test All Elements Locators", new RoutedEventHandler(TestAllElementsLocators));
        }

        private void SetDeltaControlPropertiesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaControlProperty.Name), WidthWeight = 150, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaControlProperty.Value), WidthWeight = 250, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaControlProperty.DeltaStatusIcon), Header = "Comparison Status", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xDeltaStatusIconTemplate"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaControlProperty.DeltaExtraDetails), Header = "Comparison Details", WidthWeight = 300, AllowSorting = true, ReadOnly = true });
            xPropertiesGrid.SetAllColumnsDefaultView(view);
            xPropertiesGrid.InitViewItems();
            xPropertiesGrid.SetTitleLightStyle = true;
        }

        private void SetDeltaFriendlyLocatorsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            List<ComboEnumItem> positionList = GingerCore.General.GetEnumValuesForCombo(typeof(ePosition));
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.Active), WidthWeight = 50, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox, ReadOnly = true, BindingMode = BindingMode.OneWay });
            List<ComboEnumItem> locateByList = GingerCore.General.GetEnumValuesForCombo(typeof(eLocateBy));
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.Position), Header = "Position", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = positionList,ReadOnly= true,BindingMode= BindingMode.OneWay });
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.LocateBy), Header = "Locate By", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList, ReadOnly = true, BindingMode = BindingMode.OneWay });
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.ReferanceElement), Header = "Locate Value", WidthWeight = 65 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 100, ReadOnly = true, BindingMode = BindingMode.OneWay });
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.DeltaStatusIcon), Header = "Comparison Status", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xDeltaStatusIconTemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(DeltaElementLocator.DeltaExtraDetails), Header = "Comparison Details", WidthWeight = 300, AllowSorting = true, ReadOnly = true, BindingMode = BindingMode.OneWay });
            xFriendlyLocatorsGrid.SetAllColumnsDefaultView(defView);
            xFriendlyLocatorsGrid.InitViewItems();

            xFriendlyLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            xFriendlyLocatorsGrid.ShowCopy = Visibility.Collapsed;
            xFriendlyLocatorsGrid.ShowPaste = Visibility.Collapsed;
            xFriendlyLocatorsGrid.ShowAdd = Visibility.Collapsed;
            xFriendlyLocatorsGrid.ShowDelete = Visibility.Collapsed;
            xFriendlyLocatorsGrid.ShowUpDown = Visibility.Collapsed;
        }

        private void HandelElementSelectionChange()
        {
            if (mSelectedElement != null)
            {
                xDetailsExpander.IsEnabled = true;
                if (IsFirstSelection)
                {
                    xDetailsExpander.IsExpanded = true;
                    IsFirstSelection = false;
                }

                if (mSelectedElement.ElementName != null)
                {
                    string title;
                    if (mSelectedElement.ElementName.Length > 100)
                    {
                        title = string.Format("'{0}...' Comparison Details", mSelectedElement.ElementName.Substring(0, 25));
                    }
                    else
                    {
                        title = string.Format("'{0}' Comparison Details", mSelectedElement.ElementName);
                    }
                    xDetailsExpanderLabel.Content = title;
                }
                else
                {
                    xDetailsExpanderLabel.Content = "Element Comparison Details";
                }

                mSelectedElement.Locators.CollectionChanged -= Locators_CollectionChanged;
                mSelectedElement.Locators.CollectionChanged += Locators_CollectionChanged;
                xLocatorsGrid.DataSourceList = mSelectedElement.Locators;
                UpdateLocatorsHeader();
                if (isEnableFriendlyLocator)
                {
                    mSelectedElement.FriendlyLocators.CollectionChanged -= FriendlyLocators_CollectionChanged;
                    mSelectedElement.FriendlyLocators.CollectionChanged += FriendlyLocators_CollectionChanged;
                    xFriendlyLocatorsGrid.DataSourceList = mSelectedElement.FriendlyLocators;
                    xFriendlyLocatorsGrid.ShowAdd = Visibility.Collapsed;
                    xFriendlyLocatorsGrid.ShowDelete = Visibility.Collapsed;
                    UpdateFriendlyLocatorsHeader();
                }
                else
                {
                    xFriendlyLocatorTab.Visibility = Visibility.Collapsed;
                }
                mSelectedElement.Properties.CollectionChanged -= Properties_CollectionChanged;
                mSelectedElement.Properties.CollectionChanged += Properties_CollectionChanged;
                xPropertiesGrid.DataSourceList = mSelectedElement.Properties;
                UpdatePropertiesHeader();

                //update screenshot
                BitmapSource source = null;
                if (mSelectedElement.ElementInfo.ScreenShotImage != null)
                {
                    source = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(mSelectedElement.ElementInfo.ScreenShotImage.ToString()));
                }
                xElementScreenShotFrame.Content = new ScreenShotViewPage(mSelectedElement.ElementInfo?.ElementName, source, false);

            }
            else
            {
                DisableDetailsExpander();
            }
        }

        private void DisableDetailsExpander()
        {
            xDetailsExpanderLabel.Content = "Element Comparison Details";
            xDetailsExpander.IsEnabled = false;
            xDetailsExpander.IsExpanded = false;
        }

        private void HighlightElementClicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (mSelectedElement != null)
            {
                mWinExplorer.HighLightElement(mSelectedElement.ElementInfo, true);
            }
        }

        private void DetailsGrid_Expanded(object sender, RoutedEventArgs e)
        {
            Row2.Height = new GridLength(300);
        }

        private void DetailsGrid_Collapsed(object sender, RoutedEventArgs e)
        {
            Row2.Height = new GridLength(35);
        }

        private void TestElementButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            DeltaElementInfo CurrentEI = (DeltaElementInfo)xMainElementsGrid.CurrentItem;

            if (mSelectedLocator != null)
            {
                bool originalActiveVal = mSelectedLocator.ElementLocator.Active;
                mSelectedLocator.ElementLocator.Active = true;//so it will be tested even if disabeled 
                mWinExplorer.TestElementLocators(new ElementInfo() { Path = CurrentEI.ElementInfo.Path, Locators = new ObservableList<ElementLocator>() { mSelectedLocator.ElementLocator } });
                mSelectedLocator.ElementLocator.Active = originalActiveVal;
            }
        }

        private void TestAllElementsLocators(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (mSelectedElement != null)
            {
                mWinExplorer.TestElementLocators(mSelectedElement.ElementInfo);
            }
        }

        private void xCopyLocatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateDriverAvalability())
            {
                return;
            }

            if (mSelectedLocator != null)
            {
                Clipboard.SetText(mSelectedLocator.LocateValue);
            }
        }

        private bool ValidateDriverAvalability()
        {
            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKey.POMAgentIsNotRunning);
                return false;
            }

            if (IsDriverBusy())
            {
                Reporter.ToUser(eUserMsgKey.POMDriverIsBusy);
                return false;
            }

            return true;
        }

        private bool IsDriverBusy()
        {
            if (mAgent != null && ((AgentOperations)mAgent.AgentOperations).Driver.IsDriverBusy)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void FinishEditInGrids()
        {
            xMainElementsGrid.Grid.CommitEdit();
            xLocatorsGrid.Grid.CommitEdit();
        }


        private void xElementDetailsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set the selected tab text style
            try
            {
                if (xElementDetailsTabs.SelectedItem != null)
                {
                    foreach (TabItem tab in xElementDetailsTabs.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (xElementDetailsTabs.SelectedItem == tab)
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                else
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$Color_DarkBlue");

                                ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in POM Delta View Page tabs style", ex);
            }
        }

    }
}
