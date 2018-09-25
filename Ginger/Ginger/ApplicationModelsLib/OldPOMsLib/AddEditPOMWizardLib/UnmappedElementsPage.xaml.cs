﻿using Amdocs.Ginger.Common;
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

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for UnmappedElementsPage.xaml
    /// </summary>
    public partial class UnmappedElementsPage : Page
    {
        ApplicationPOMModel mPOM;
        GenericWindow _GenWin;

        public UnmappedElementsPage(ApplicationPOMModel POM)
        {
            InitializeComponent();
            mPOM = POM;
            SetControlsGridView();
            xUnmappedElementsGrid.DataSourceList = mPOM.UnMappedUIElements;
        }

        private void SetControlsGridView()
        {

            xUnmappedElementsGrid.SetTitleLightStyle = true;

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Selected), Header = "", StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementTitle), Header = "Element Title", WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Value), WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.ElementType), Header = "Element Type", WidthWeight = 60 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.Path), WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.XPath), WidthWeight = 150 });

            xUnmappedElementsGrid.AddToolbarTool("@Import_16x16.png", "Add Items to mapped list", new RoutedEventHandler(AddButtonClicked));
            xUnmappedElementsGrid.SetAllColumnsDefaultView(view);
            xUnmappedElementsGrid.InitViewItems();
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            List<ElementInfo> ItemsToAddList =  mPOM.UnMappedUIElements.Where(x => x.Selected).ToList();

            foreach (ElementInfo EI in ItemsToAddList)
            {
                EI.Selected = false;
                mPOM.MappedUIElements.Add(EI);
                mPOM.UnMappedUIElements.Remove(EI);
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            string Title = "Unmappaed Elements Page";

            GingerCore.General.LoadGenericWindow(ref _GenWin, null, windowStyle, Title, this);
        }



    }
}
