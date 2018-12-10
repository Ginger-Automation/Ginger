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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.ApplicationModelsLib.POMModels
{
    public partial class PomElementsPage : Page
    {
        public PomAllElementsPage.eElementsContext mContext;
        ApplicationPOMModel mPOM;
        ObservableList<ElementInfo> mElements;
        bool IsFirstSelection = true;


        private Agent mAgent;
        IWindowExplorer mWinExplorer
        {
            get
            {
                if (mAgent != null && mAgent.Status == Agent.eStatus.Running)
                {                    
                    return mAgent.Driver as IWindowExplorer;
                }
                else
                {
                    if (mAgent != null)
                    {
                        mAgent.Close();
                    }
                    return null;
                }
            }
        }

        public ucGrid MainElementsGrid
        {
            get
            {
                return xMainElementsGrid;
            }
        }

        ElementInfo mSelectedElement
        {
            get
            {
                if (xMainElementsGrid.Grid.SelectedItem != null)
                {
                    return (ElementInfo)xMainElementsGrid.Grid.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }
        ElementLocator mSelectedLocator
        {
            get
            {
                if (xLocatorsGrid.Grid.SelectedItem != null)
                {
                    return (ElementLocator)xLocatorsGrid.Grid.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        public PomElementsPage(ApplicationPOMModel pom, PomAllElementsPage.eElementsContext context)
        {
            InitializeComponent();
            mPOM = pom;
            mContext = context;
            if (mContext == PomAllElementsPage.eElementsContext.Mapped)
            {
                mElements = mPOM.MappedUIElements;
            }
            else
            {
                mElements = mPOM.UnMappedUIElements;
            }
            
            SetControlPropertiesGridView();
            SetLocatorsGridView();
            SetElementsGridView();

            xMainElementsGrid.DataSourceList = mElements;
            if (mElements.Count > 0)
            {
                xMainElementsGrid.Grid.SelectedItem = mElements[0];
            }
            else
            {
                DisableDetailsExpander();
            }
        }

        private void PasteElementEvent(PasteItemEventArgs EventArgs)
        {
            ((ElementInfo)EventArgs.RepositoryItemBaseObject).IsAutoLearned = false;
        }

        private void PasteLocatorEvent(PasteItemEventArgs EventArgs)
        {
            ((ElementLocator)EventArgs.RepositoryItemBaseObject).IsAutoLearned = false;
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
        private void UpdateLocatorsHeader()
        {
            Dispatcher.Invoke(() =>
            {
                xLocatorsTextBlock.Text = string.Format("Locators ({0})", mSelectedElement.Locators.Count);
            });
        }

        private void AddElementsToMappedBtnClicked(object sender, RoutedEventArgs e)
        {
            List<ElementInfo> ItemsToAddList = xMainElementsGrid.Grid.SelectedItems.Cast<ElementInfo>().ToList();
            foreach (ElementInfo EI in ItemsToAddList)
            {
                mPOM.MappedUIElements.Add(EI);
                mPOM.UnMappedUIElements.Remove(EI);
            }
        }

        private void RemoveElementsToMappedBtnClicked(object sender, RoutedEventArgs e)
        {
            List<ElementInfo> ItemsToRemoveList = xMainElementsGrid.Grid.SelectedItems.Cast<ElementInfo>().ToList();
            foreach (ElementInfo EI in ItemsToRemoveList)
            {
                mPOM.MappedUIElements.Remove(EI);
                mPOM.UnMappedUIElements.Add(EI);
            }
        }

        internal void SetAgent(Agent agent)
        {
            mAgent = agent;
        }

        public enum eGridView
        {
            RegularView,
        }
        private void SetElementsGridView()
        {
            xMainElementsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementName), Header = "Name", WidthWeight = 40, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Description), WidthWeight = 35, AllowSorting = true });

            List<GingerCore.General.ComboEnumItem> ElementTypeList = GingerCore.General.GetEnumValuesForCombo(typeof(eElementType));
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeEnum), Header = "Type", WidthWeight = 15, AllowSorting = true, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = ElementTypeList });

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = "", Header = "Highlight", WidthWeight = 10, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xHighlightButtonTemplate"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.StatusIcon), Header = "Status", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });


            GridViewDef mRegularView = new GridViewDef(eGridView.RegularView.ToString());
            mRegularView.GridColsView = new ObservableList<GridColView>();
            mRegularView.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.StatusIcon), Visible = false });

            xMainElementsGrid.AddCustomView(mRegularView);
            xMainElementsGrid.SetAllColumnsDefaultView(view);
            xMainElementsGrid.InitViewItems();
            xMainElementsGrid.ChangeGridView(eGridView.RegularView.ToString());

            if (mContext == PomAllElementsPage.eElementsContext.Mapped)
            {
                xMainElementsGrid.AddToolbarTool(eImageType.MapSigns, "Remove elements from mapped list", new RoutedEventHandler(RemoveElementsToMappedBtnClicked));
                xMainElementsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddMappedElementRow));
                xMainElementsGrid.ShowDelete = Visibility.Collapsed;
            }
            else
            {
                xMainElementsGrid.AddToolbarTool(eImageType.MapSigns, "Add elements to mapped list", new RoutedEventHandler(AddElementsToMappedBtnClicked));
                xMainElementsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddUnMappedElementRow));
                xMainElementsGrid.SetbtnDeleteHandler(DeleteUnMappedElementRow);
            }

            xMainElementsGrid.grdMain.PreparingCellForEdit += MainElementsGrid_PreparingCellForEdit;
            xMainElementsGrid.PasteItemEvent += PasteElementEvent;


            xMainElementsGrid.SelectedItemChanged += XMainElementsGrid_SelectedItemChanged;
            xMainElementsGrid.Grid.SelectionChanged += Grid_SelectionChanged;
        }

        private void XMainElementsGrid_SelectedItemChanged(object selectedItem)
        {
            HandelElementSelectionChange();
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandelElementSelectionChange();
        }

        bool disabeledElementMsgShown;
        private void MainElementsGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header == "Name" || e.Column.Header == nameof(ElementInfo.Description))
            {
                return;
            }

            ElementInfo ei = (ElementInfo)xMainElementsGrid.CurrentItem;
            if (ei.IsAutoLearned)
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "You can not edit this field of an Element which was auto learned, please duplicate it and create customized Element.");
                e.EditingElement.IsEnabled = false;
            }
        }

        private void DeleteUnMappedElementRow(object sender, RoutedEventArgs e)
        {
            bool msgShowen = false;
            List<ElementInfo> elementsToDelete = xMainElementsGrid.Grid.SelectedItems.Cast<ElementInfo>().ToList();
            foreach (ElementInfo element in elementsToDelete)
            {
                if (element.IsAutoLearned)
                {
                    if (!msgShowen)
                    {
                        Reporter.ToUser(eUserMsgKeys.POMCannotDeleteAutoLearnedElement);
                        msgShowen = true;
                    }
                }
                else
                {
                    mPOM.UnMappedUIElements.Remove(element);
                }
            }
        }

        private void AddMappedElementRow(object sender, RoutedEventArgs e)
        {
            xMainElementsGrid.Grid.CommitEdit();

            ElementInfo EI = new ElementInfo();
            mPOM.MappedUIElements.Add(EI);
            
            xMainElementsGrid.Grid.SelectedItem = EI;
            xMainElementsGrid.ScrollToViewCurrentItem();
        }

        private void AddUnMappedElementRow(object sender, RoutedEventArgs e)
        {
            xMainElementsGrid.Grid.CommitEdit();

            ElementInfo EI = new ElementInfo();
            mPOM.UnMappedUIElements.Add(EI);

            xMainElementsGrid.Grid.SelectedItem = EI;
            xMainElementsGrid.ScrollToViewCurrentItem();
        }

        private void SetLocatorsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 8, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            List<GingerCore.General.ComboEnumItem> locateByList = GingerCore.General.GetEnumValuesForCombo(typeof(eLocateBy));
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 25, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList, });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), Header = "Locate Value", WidthWeight = 65 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 25, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 10, MaxWidth = 100, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = "Test", WidthWeight = 10, MaxWidth = 100, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestElementButtonTemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.StatusIcon), Header = "Status", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });
            xLocatorsGrid.SetAllColumnsDefaultView(defView);
            xLocatorsGrid.InitViewItems();

            xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            xLocatorsGrid.AddToolbarTool(eImageType.Play, "Test All Elements Locators", new RoutedEventHandler(TestAllElementsLocators));
            xLocatorsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddLocatorButtonClicked));
            xLocatorsGrid.SetbtnDeleteHandler(new RoutedEventHandler(DeleteLocatorClicked));

            xLocatorsGrid.grdMain.PreparingCellForEdit += LocatorsGrid_PreparingCellForEdit;
            xLocatorsGrid.PasteItemEvent += PasteLocatorEvent;
        }

        private void DeleteLocatorClicked(object sender, RoutedEventArgs e)
        {
            bool msgShowen = false;
            List<ElementLocator> locatorsToDelete = xLocatorsGrid.Grid.SelectedItems.Cast<ElementLocator>().ToList();
            foreach (ElementLocator locator in locatorsToDelete)
            {
                if (locator.IsAutoLearned)
                {
                    if (!msgShowen)
                    {
                        Reporter.ToUser(eUserMsgKeys.POMCannotDeleteAutoLearnedElement);
                        msgShowen = true;
                    }
                }
                else
                {
                    mSelectedElement.Locators.Remove(locator);
                }
            }
        }

        private void AddLocatorButtonClicked(object sender, RoutedEventArgs e)
        {
            xLocatorsGrid.Grid.CommitEdit();

            ElementLocator locator = new ElementLocator() { Active = true };
            mSelectedElement.Locators.Add(locator);

            xLocatorsGrid.Grid.SelectedItem = locator;
            xLocatorsGrid.ScrollToViewCurrentItem();
        }

        bool disabeledLocatorsMsgShown;
        private void LocatorsGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header.ToString() != nameof(ElementLocator.Active) && mSelectedLocator.IsAutoLearned)
            {
                if (!disabeledLocatorsMsgShown)
                {
                    Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "You can not edit Locator which was auto learned, please duplicate it and create customized Locator.");
                    disabeledLocatorsMsgShown = true;
                }
                e.EditingElement.IsEnabled = false;
            }
        }

        private void SetControlPropertiesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Name), WidthWeight = 25, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Value), WidthWeight = 75, ReadOnly = true });

            xPropertiesGrid.SetAllColumnsDefaultView(view);
            xPropertiesGrid.InitViewItems();
            xPropertiesGrid.SetTitleLightStyle = true;
            xPropertiesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddPropertyHandler));
        }

        private void AddPropertyHandler(object sender, RoutedEventArgs e)
        {
            mSelectedElement.Properties.Add(new ControlProperty());
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
                        title = string.Format("'{0}...' Details", mSelectedElement.ElementName.Substring(0, 25));
                    }
                    else
                    {
                        title = string.Format("'{0}' Details", mSelectedElement.ElementName);
                    }
                    xDetailsExpanderLabel.Content = title;
                }
                else
                {
                    xDetailsExpanderLabel.Content = "Element Details";
                }

                mSelectedElement.Locators.CollectionChanged -= Locators_CollectionChanged;
                mSelectedElement.Locators.CollectionChanged += Locators_CollectionChanged;
                xLocatorsGrid.DataSourceList = mSelectedElement.Locators;
                UpdateLocatorsHeader();

                mSelectedElement.Properties.CollectionChanged -= Properties_CollectionChanged;
                mSelectedElement.Properties.CollectionChanged += Properties_CollectionChanged;
                xPropertiesGrid.DataSourceList = mSelectedElement.Properties;
                UpdatePropertiesHeader();

            }
            else
            {
                DisableDetailsExpander();
            }
        }

        private void DisableDetailsExpander()
        {
            xDetailsExpanderLabel.Content = "Element Details";
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
                mWinExplorer.HighLightElement(mSelectedElement, true);               
            }
        }

        private void DetailsGrid_Expanded(object sender, RoutedEventArgs e)
        {
            Row2.Height = new GridLength(30, GridUnitType.Star);
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

            if (mSelectedLocator != null)
            {
                mWinExplorer.TestElementLocators(new ObservableList<ElementLocator>() { mSelectedLocator });
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
                mWinExplorer.TestElementLocators(mSelectedElement.Locators);
            }
        }

        private bool ValidateDriverAvalability()
        {
            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKeys.POMAgentIsNotRunning);
                return false;
            }

            if (IsDriverBusy())
            {
                Reporter.ToUser(eUserMsgKeys.POMDriverIsBusy);
                return false;
            }

            return true;
        }

        private bool IsDriverBusy()
        {
            if (mAgent != null && mAgent.Driver.IsDriverBusy)
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error in POM Edit Page tabs style", ex);
            }
        }

    }
}
