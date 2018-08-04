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
        GenericWindow _GenWin;

        public MappedUIElementsPage(ApplicationPOMModel POM)
        {
            InitializeComponent();
            mPOM = POM;
            SetControlsGridView();
            xMappedElementsGrid.DataSourceList = mPOM.MappedUIElements;
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



    }
}
