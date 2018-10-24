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
                if (xMainElementsGrid.CurrentItem != null)
                {
                    return (ElementInfo)xMainElementsGrid.CurrentItem;
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
                if (xLocatorsGrid.CurrentItem != null)
                {
                    return (ElementLocator)xLocatorsGrid.CurrentItem;
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

            SetElementsGridView();
            SetControlPropertiesGridView();
            SetLocatorsGridView();
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

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementName), Header = "Name", WidthWeight = 60, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Description), Header = "Description", WidthWeight = 100, AllowSorting = true });

            List<GingerCore.General.ComboEnumItem> ElementTypeList = GingerCore.General.GetEnumValuesForCombo(typeof(eElementType));
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeEnum), Header = "Type", WidthWeight = 60, AllowSorting = true, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = ElementTypeList });

            //view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Value), WidthWeight = 100, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = "", WidthWeight = 8, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xHighlightButtonTemplate"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.StatusIcon), Header = "Status", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.IsAutoLearned), Header = "Auto Learned", WidthWeight = 20, AllowSorting = true, ReadOnly = true });

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
            xMainElementsGrid.PasteItemEvent -= PasteElementEvent;
            xMainElementsGrid.PasteItemEvent += PasteElementEvent;

            xMainElementsGrid.DataSourceList = mElements;
        }

        private void MainElementsGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            ElementInfo ei = (ElementInfo)xMainElementsGrid.CurrentItem;
            if (ei.IsAutoLearned)
            {
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
            ElementInfo EI = new ElementInfo();
            EI.IsAutoLearned = false;
            mPOM.MappedUIElements.Add(EI);
            mPOM.MappedUIElements.CurrentItem = EI;
            xMainElementsGrid.ScrollToViewCurrentItem();
        }

        private void AddUnMappedElementRow(object sender, RoutedEventArgs e)
        {
            ElementInfo EI = new ElementInfo();
            mPOM.UnMappedUIElements.Add(EI);
            mPOM.UnMappedUIElements.CurrentItem = EI;
            xMainElementsGrid.ScrollToViewCurrentItem();
        }

        private void SetLocatorsGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 30, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            List<GingerCore.General.ComboEnumItem> locateByList = GingerCore.General.GetEnumValuesForCombo(typeof(eLocateBy));
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 40, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList, });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), Header = "Locate Value", WidthWeight = 150 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 70, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.IsAutoLearned), Header = "Auto Learned", WidthWeight = 20, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = "Test", WidthWeight = 15, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestElementButtonTemplate"] });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.StatusIcon), Header = "Status", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });
            xLocatorsGrid.SetAllColumnsDefaultView(defView);
            xLocatorsGrid.InitViewItems();

            xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
            xLocatorsGrid.AddToolbarTool(eImageType.Play, "Test All Elements Locators", new RoutedEventHandler(TestAllElementsLocators));
            xLocatorsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddLocatorButtonClicked));
            xLocatorsGrid.SetbtnDeleteHandler(new RoutedEventHandler(DeleteLocatorClicked));

            xLocatorsGrid.grdMain.PreparingCellForEdit += LocatorsGrid_PreparingCellForEdit;

            xLocatorsGrid.PasteItemEvent -= PasteLocatorEvent;
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
            mSelectedElement.Locators.Add(new ElementLocator());
        }

        private void LocatorsGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header.ToString() != nameof(ElementLocator.Active) && mSelectedLocator.IsAutoLearned)
            {
                e.EditingElement.IsEnabled = false;
            }
        }

        private void SetControlPropertiesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Name), WidthWeight = 8, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Value), WidthWeight = 20, ReadOnly = true });

            xPropertiesGrid.SetAllColumnsDefaultView(view);
            xPropertiesGrid.InitViewItems();
            xPropertiesGrid.SetTitleLightStyle = true;
            xPropertiesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddPropertyHandler));
        }

        private void AddPropertyHandler(object sender, RoutedEventArgs e)
        {
            mSelectedElement.Properties.Add(new ControlProperty());
        }

        private void MappedElementsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (mSelectedElement != null)
                {
                    xDetailsExpander.IsEnabled = true;
                    if (IsFirstSelection)
                    {
                        xDetailsExpander.IsExpanded = true;
                        IsFirstSelection = false;
                    }

                    if (mSelectedElement.ElementTitle != null)
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
                    xDetailsExpander.IsEnabled = false;
                    xDetailsExpander.IsExpanded = false;
                }
            });

            GingerCore.General.DoEvents();
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
            Row2.Height = new GridLength(100, GridUnitType.Star);
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

        private void xMainElementsGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            xMainElementsGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        private void xPropertiesGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            xPropertiesGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        private void xLocatorsGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            xLocatorsGrid.Grid.CommitEdit(DataGridEditingUnit.Row, true);
        }

        public void FinishEditInGrids()
        {
            xMainElementsGrid.Grid.CommitEdit();
            xLocatorsGrid.Grid.CommitEdit();
        }
    }
}
