using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Ginger.ApplicationModelsLib.POMModels
{
    /// <summary>
    /// Interaction logic for MappedUIElementsPage.xaml
    /// </summary>
    public partial class PomElementsPage : Page
    {

        ApplicationPOMModel mPOM;
        public ObservableList<ElementLocator> mLocators = new ObservableList<ElementLocator>();
        public ObservableList<ControlProperty> mProperties = new ObservableList<ControlProperty>();
        GenericWindow _GenWin;
        public IWindowExplorer mWinExplorer;

        public bool DriverIsBusy { get; set; }

        public PomAllElementsPage.eElementsContext mContext;

        public PomElementsPage(ApplicationPOMModel POM, PomAllElementsPage.eElementsContext context, IWindowExplorer winExplorer)
        {
            InitializeComponent();
            mPOM = POM;
            mWinExplorer = winExplorer;
            mContext = context;

            mLocators.CollectionChanged += Locators_CollectionChanged;
            mProperties.CollectionChanged += Properties_CollectionChanged;

            SetControlsGridView();

            if (mContext == PomAllElementsPage.eElementsContext.Mapped)
                xMainElementsGrid.DataSourceList = mPOM.MappedUIElements;
            else
                xMainElementsGrid.DataSourceList = mPOM.UnMappedUIElements;

            InitControlPropertiesGridView();
            InitLocatorsGrid();

        }

        private void Properties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                xPropertiesTextBlock.Text = string.Format("Properties ({0})", mProperties.Count);
            });
        }

        private void Locators_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                xLocatorsTextBlock.Text = string.Format("Locators ({0})", mLocators.Count);
            });
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            if (DriverIsBusy)
            {
                Reporter.ToUser(eUserMsgKeys.POMDriverIsBusy);
                return;
            }
            List<ElementInfo> ItemsToAddList = xMainElementsGrid.grdMain.SelectedItems.Cast<ElementInfo>().ToList();

            foreach (ElementInfo EI in ItemsToAddList)
            {
                mPOM.MappedUIElements.Add(EI);
                mPOM.UnMappedUIElements.Remove(EI);
            }
        }

        private void RemoveButtonClicked(object sender, RoutedEventArgs e)
        {
            if (DriverIsBusy)
            {
                Reporter.ToUser(eUserMsgKeys.POMDriverIsBusy);
                return;
            }

            List<ElementInfo> ItemsToRemoveList = xMainElementsGrid.grdMain.SelectedItems.Cast<ElementInfo>().ToList();

            foreach (ElementInfo EI in ItemsToRemoveList)
            {
                mPOM.MappedUIElements.Remove(EI);
                mPOM.UnMappedUIElements.Add(EI);
            }
        }

        internal void SetWindowExplorer(IWindowExplorer windowExplorerDriver)
        {
            mWinExplorer = windowExplorerDriver;
        }

        private void SetControlsGridView()
        {

            xMainElementsGrid.SetTitleLightStyle = true;


            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTitle), Header = "Element Title", WidthWeight = 60, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Description), Header = "Description", WidthWeight = 100, AllowSorting = true });

            List<GingerCore.General.ComboEnumItem> ElementTypeList = GingerCore.General.GetEnumValuesForCombo(typeof(eElementType));
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTypeEnum), Header = "Element Type", WidthWeight = 60, AllowSorting = true , StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = ElementTypeList });

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Value), WidthWeight = 100, AllowSorting = true });
            //view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Path), WidthWeight = 100, AllowSorting = true });
            //view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.XPath), WidthWeight = 150, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = "", WidthWeight = 8, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xHighlightButtonTemplate"] });

            if (mContext == PomAllElementsPage.eElementsContext.Mapped)
            {
                xMainElementsGrid.AddToolbarTool("@RoadSign_16x16.png", "Remove Items from mapped list", new RoutedEventHandler(RemoveButtonClicked));
                xMainElementsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddMappedElementRow));
                xMainElementsGrid.ShowDelete = Visibility.Collapsed;

            }
            else
            {
                xMainElementsGrid.AddToolbarTool("@RoadSign_16x16.png", "Add Items to mapped list", new RoutedEventHandler(AddButtonClicked));
                xMainElementsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddUnMappedElementRow));
                
                xMainElementsGrid.SetbtnDeleteHandler(DeleteUnMappedElementRow);
            }

            //xMainElementsGrid.AddToolbarTool("@Spy_24x24.png", "Live Spy- Hover with the mouse over the Element you want to spy and Click/Hold Down 'Ctrl' Key", new RoutedEventHandler(LiveSpyHandler));


            xMainElementsGrid.SetAllColumnsDefaultView(view);
            xMainElementsGrid.InitViewItems();
        }

        private void DeleteUnMappedElementRow(object sender, RoutedEventArgs e)
        {
            if (DriverIsBusy)
            {
                Reporter.ToUser(eUserMsgKeys.POMDriverIsBusy);
                return;
            }

            mPOM.UnMappedUIElements.Remove(mMainElementsGridCurrentItem);
        }

        System.Windows.Threading.DispatcherTimer dispatcherTimer = null;

        private void LiveSpyHandler(object sender, RoutedEventArgs e)
        {
            if (DriverIsBusy)
            {
                Reporter.ToUser(eUserMsgKeys.POMDriverIsBusy);
                return;
            }

            if (LiveSpyButton.IsChecked == true)
            {
                xStatusLable.Content = "Spying on";
                if (dispatcherTimer == null)
                {
                    dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(timenow);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                }

                dispatcherTimer.IsEnabled = true;
            }
            else
            {
                xCreateNewElement.Visibility = Visibility.Collapsed;
                xStatusLable.Content = "Spying off";
                dispatcherTimer.IsEnabled = false;
            }
        }

        private void StopSpying()
        {
            xCreateNewElement.Visibility = Visibility.Collapsed;
            xStatusLable.Content = "Spying off";
            dispatcherTimer.IsEnabled = false;
        }

        ElementInfo mSpyElement;

        private void timenow(object sender, EventArgs e)
        {
            // Get control info only if control key is pressed
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                xStatusLable.Content = "Spying Element, Please Wait...";
                xCreateNewElement.Visibility = Visibility.Collapsed;
                GingerCore.General.DoEvents();
                mSpyElement = mWinExplorer.GetControlFromMousePosition();
                if (mSpyElement != null)
                {
                    xStatusLable.Content = "Element found";
                    FocusSpyItemOnElementsGrid();
                    mWinExplorer.HighLightElement(mSpyElement);
                }
                else
                {
                    xStatusLable.Content = "Failed to spy element.";
                    GingerCore.General.DoEvents();
                }
            }
        }

        private void FocusSpyItemOnElementsGrid()
        {
            bool elementfocused = false;
            if (mSpyElement == null) return;
            foreach (ElementInfo EI in mPOM.MappedUIElements)
            {
                mWinExplorer.UpdateElementInfoFields(EI);//Not sure if needed

                if (EI.XPath == mSpyElement.XPath && EI.Path == mSpyElement.Path)
                {
                    elementfocused = true;
                    mPOM.MappedUIElements.CurrentItem = EI;
                    xMainElementsGrid.ScrollToViewCurrentItem();
                    break;
                }
            }

            foreach (ElementInfo EI in mPOM.UnMappedUIElements)
            {
                mWinExplorer.UpdateElementInfoFields(EI);//Not sure if needed

                if (EI.XPath == mSpyElement.XPath && EI.Path == mSpyElement.Path)
                {
                    elementfocused = true;
                    mPOM.UnMappedUIElements.CurrentItem = EI;
                    xMainElementsGrid.ScrollToViewCurrentItem();
                    break;
                }
            }

            if (!elementfocused)
            {
                xStatusLable.Content = "Element has not been found on the list, Click here to create new Element ";
                xCreateNewElement.Visibility = Visibility.Visible;
            }
        }


        private void AddMappedElementRow(object sender, RoutedEventArgs e)
        {
            if (DriverIsBusy)
            {
                Reporter.ToUser(eUserMsgKeys.POMDriverIsBusy);
                return;
            }

            ElementInfo EI = new ElementInfo();
            mPOM.MappedUIElements.Add(EI);
            mPOM.MappedUIElements.CurrentItem = EI;
            xMainElementsGrid.ScrollToViewCurrentItem();
        }

        private void AddUnMappedElementRow(object sender, RoutedEventArgs e)
        {
            if (DriverIsBusy)
            {
                Reporter.ToUser(eUserMsgKeys.POMDriverIsBusy);
                return;
            }

            ElementInfo EI = new ElementInfo();
            mPOM.UnMappedUIElements.Add(EI);
            mPOM.UnMappedUIElements.CurrentItem = EI;
            xMainElementsGrid.ScrollToViewCurrentItem();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            string Title = "Mappaed Elements Page";

            GingerCore.General.LoadGenericWindow(ref _GenWin, null, windowStyle, Title, this);
        }

        private void InitLocatorsGrid()
        {
            //TODO: need to add Help text or convert to icon...
            //xLocatorsGrid.AddButton("Test", TestSelectedLocator);
            //xLocatorsGrid.AddButton("Test All", TestAllLocators);

            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            //view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.Selected), Header = "Selected", WidthWeight = 10, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Active), WidthWeight = 30, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });

            List<GingerCore.General.ComboEnumItem> locateByList = GingerCore.General.GetEnumValuesForCombo(typeof(eLocateBy));
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), Header = "Locate By", WidthWeight = 40, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = locateByList });

            //defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), WidthWeight = 10 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), Header = "Locate Value", WidthWeight = 150 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 70, ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = "Test", WidthWeight = 15, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestElementButtonTemplate"] });
            //defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.TestStatus), Header = "Test Status", WidthWeight = 20 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.TestStatusIcon), Header = "Test Status", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["xTestStatusIconTemplate"] });

            xLocatorsGrid.AddToolbarTool("@Play_16x16.png", "Test All Elements Locators", new RoutedEventHandler(TestAllElementsLocators));
            xLocatorsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddLocatorHandler));
            xLocatorsGrid.SetAllColumnsDefaultView(defView);
            xLocatorsGrid.InitViewItems();
            xLocatorsGrid.DataSourceList = mLocators;
            xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        private void AddLocatorHandler(object sender, RoutedEventArgs e)
        {
            ElementLocator newElementLocator = new ElementLocator();
            mMainElementsGridCurrentItem.Locators.Add(newElementLocator);
            mLocators.Add(newElementLocator);
        }


        private void InitControlPropertiesGridView()
        {
            // Grid View
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Name), WidthWeight = 8, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ControlProperty.Value), WidthWeight = 20, ReadOnly = true });

            xPropertiesGrid.SetAllColumnsDefaultView(view);
            xPropertiesGrid.InitViewItems();
            xPropertiesGrid.SetTitleLightStyle = true;
            xPropertiesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddPropertyHandler));
            xPropertiesGrid.DataSourceList = mProperties;
        }

        private void AddPropertyHandler(object sender, RoutedEventArgs e)
        {
            mProperties.Add(new ControlProperty());
        }

        bool IsFirstSelection = true;

        private void MappedElementsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            if (IsFirstSelection)
            {
                xDetailsExpander.IsExpanded = true;
                IsFirstSelection = false;
            }


            mLocators.Clear();
            mProperties.Clear();
            if (((DataGrid)sender).SelectedItem != null)
            {
                ElementInfo SelectedElement = (ElementInfo)((DataGrid)sender).SelectedItem;
                if (SelectedElement.ElementTitle != null)
                {
                    xDetailsExpanderLabel.Content ="'" + SelectedElement.ElementTitle + "' Details";
                }
                foreach (ElementLocator EL in SelectedElement.Locators)
                    mLocators.Add(EL);

                foreach (ControlProperty CP in SelectedElement.Properties)
                    mProperties.Add(CP);

                //ObservableList<ControlProperty> ElementsProperties = SelectedElement.GetElementProperties();
                //if (mWinExplorer != null)
                //{
                //    ObservableList<ControlProperty> ElementsProperties = mWinExplorer.GetElementProperties(SelectedElement);
                //    foreach (ControlProperty CP in ElementsProperties)
                //        mProperties.Add(CP);
                //}

            }
        }

        public ucGrid MainElementsGrid
        {
            get
            {
                return xMainElementsGrid;
            }

        }


        private ElementInfo mMainElementsGridCurrentItem { get { return (ElementInfo)xMainElementsGrid.CurrentItem; } }

        private ElementLocator mLocatorsGridCurrentItem { get { return (ElementLocator)xLocatorsGrid.CurrentItem; } }



        private void HighlightElementClicked(object sender, RoutedEventArgs e)
        {
            if (DriverIsBusy)
            {
                Reporter.ToUser(eUserMsgKeys.POMDriverIsBusy);
                return;
            }

            mWinExplorer.HighLightElement(mMainElementsGridCurrentItem);
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
            if (DriverIsBusy)
            {
                Reporter.ToUser(eUserMsgKeys.POMDriverIsBusy);
                return;
            }

            mWinExplorer.TestElementLocator(mLocatorsGridCurrentItem);
            
        }

        private void TestAllElementsLocators(object sender, RoutedEventArgs e)
        {
            if (DriverIsBusy)
            {
                Reporter.ToUser(eUserMsgKeys.POMDriverIsBusy);
                return;
            }

            mWinExplorer.TestAllElementsLocators(mLocators);
        }

        private void CreateNewElemetClicked(object sender, RoutedEventArgs e)
        {
            if (mContext == PomAllElementsPage.eElementsContext.Mapped)
            {
                mPOM.MappedUIElements.Add(mSpyElement);
            }
            else
            {
                mPOM.UnMappedUIElements.Add(mSpyElement);
            }

            xCreateNewElement.Visibility = Visibility.Collapsed;
            xStatusLable.Content = "Element added to the list";
        }
    }
}
