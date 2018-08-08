using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using System;
using System.Collections.Generic;
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
    public partial class MappedUIElementsPage : Page
    {

        ApplicationPOMModel mPOM;
        public ObservableList<ElementLocator> mLocators = new ObservableList<ElementLocator>();
        GenericWindow _GenWin;

        public MappedUIElementsPage(ApplicationPOMModel POM)
        {
            InitializeComponent();
            mPOM = POM;

            

            SetControlsGridView();
            xMappedElementsGrid.DataSourceList = mPOM.MappedUIElements;

            InitLocatorsGrid();

        }

        private void SetControlsGridView()
        {

            xMappedElementsGrid.SetTitleLightStyle = true;

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Selected), Header = "Element Title", WidthWeight = 100, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTitle), Header = "Element Title", WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Value), WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementType), Header = "Element Type", WidthWeight = 60 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Path), WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.XPath), WidthWeight = 150 });

            xMappedElementsGrid.SetAllColumnsDefaultView(view);
            xMappedElementsGrid.InitViewItems();
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
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateBy), WidthWeight = 10 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.LocateValue), WidthWeight = 30 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Help), WidthWeight = 20 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ElementLocator.Count), WidthWeight = 10 });

            xLocatorsGrid.SetAllColumnsDefaultView(defView);
            xLocatorsGrid.InitViewItems();
            xLocatorsGrid.DataSourceList = mLocators;
            xLocatorsGrid.SetTitleStyle((Style)TryFindResource("@ucTitleStyle_4"));
        }

        //private void TestSelectedLocator(object sender, RoutedEventArgs e)
        //{
        //    //ElementLocator EL = (ElementLocator)mLocators.CurrentItem;
        //    //ObservableList<ElementInfo> list = mWindowExplorerDriver.GetElements(EL);
        //    //EL.Count = list.Count;
        //}


        //private void TestAllLocators(object sender, RoutedEventArgs e)
        //{
        //    //foreach (ElementLocator EL in mLocators)
        //    //{
        //    //    ObservableList<ElementInfo> list = mWindowExplorerDriver.GetElements(EL);
        //    //    EL.Count = list.Count;
        //    //}
        //}


        private void MappedElementsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            mLocators = ((ElementInfo)((DataGrid)sender).SelectedItem).Locators;
        }
    }
}
