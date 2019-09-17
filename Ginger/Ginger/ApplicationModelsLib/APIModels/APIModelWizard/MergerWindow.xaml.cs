using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCoreNET.Application_Models;
using GingerWPF.ApplicationModelsLib.APIModels;
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

namespace Ginger.ApplicationModelsLib.APIModels.APIModelWizard
{
    /// <summary>
    /// Interaction logic for MergerWindow.xaml
    /// </summary>
    public partial class MergerWindow : Page
    {
        //ObservableList<ApplicationAPIModel> modelsEvaluated;
        GenericWindow mWin;
        ApplicationAPIModel mergedAPIModel = new ApplicationAPIModel();

        public MergerWindow()
        {
            InitializeComponent();
        }
        public MergerWindow(ApplicationAPIModel matchingAPI, ApplicationAPIModel learnedAPI, bool ShowMergerSection = false)
        {
            InitializeComponent();

            APIModelPage existingAPIPage = new APIModelPage(matchingAPI);
            APIModelPage learnedAPIPage = new APIModelPage(learnedAPI);
            APIModelPage mergedAPIPage = new APIModelPage(mergedAPIModel);

            xExistingAPIFrame.Content = existingAPIPage;
            xLearnedAPIFrame.Content = learnedAPIPage;
            xMergedAPIFrame.Content = mergedAPIPage;

            if (ShowMergerSection)
            {
                //xMergerRow.Visibility = Visibility.Visible;
                xAPIMergerSectionScroller.Visibility = Visibility.Visible;
            }
            else
            {
                //xMergerRow.Visibility = Visibility.Collapsed;
                xAPIMergerSectionScroller.Visibility = Visibility.Collapsed;
            }
            //xMergedAPIFrame.Visibility = Visibility.Collapsed;

            //ObservableList<string> props = new ObservableList<string>();
            //foreach (System.Reflection.PropertyInfo prop in matchingAPI.GetType().GetProperties())
            //{
            //    props.Add(Convert.ToString(prop.GetValue(matchingAPI)));
            //}
            //xExistingAPIGrid.DataSourceList = props;

            //ObservableList<string> learnedProps = new ObservableList<string>();
            //foreach (System.Reflection.PropertyInfo prop in learnedAPI.GetType().GetProperties())
            //{
            //    learnedProps.Add(Convert.ToString(prop.GetValue(learnedAPI)));
            //}
            //xExistingAPIGrid.DataSourceList = learnedProps;

        }

        //private void SetFieldsGrid()
        //{
        //    GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
        //    view.GridColsView = new ObservableList<GridColView>();
        //    view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.ExistingSelected), Header = "Select", WidthWeight = 30, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.CheckBox });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.GetType().GetProperty()), Header = "Name", WidthWeight = 250, BindingMode = BindingMode.OneWay });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.Description), Header = "Description", WidthWeight = 250, BindingMode = BindingMode.OneWay });

        //    view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.MatchingAPIName), Header = "Matching API Model", WidthWeight = 300, BindingMode = BindingMode.OneWay });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.comparisonStatus), Header = "Comparison Status", WidthWeight = 150, MaxWidth = 150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["xDeltaStatusIconTemplate"], BindingMode = System.Windows.Data.BindingMode.OneWay });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.OperationsList), Header = "Difference's Handling Operation", WidthWeight = 200, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(DeltaAPIModel.OperationsList), nameof(DeltaAPIModel.defaultOperation), false) });
        //    //view.GridColsView.Add(new GridColView() { Field = nameof(LearnedAPIModels.OperationsList), Header = "Difference's Handling Operation", WidthWeight = 50, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = nameof(LearnedAPIModels.OperationsList) });
        //    view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.defaultOperation), Header = "Compare & Merge", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["CompareAndMergeTemplate"] });

        //    xExistingAPIGrid.SetAllColumnsDefaultView(view);
        //    xNewAPIGrid.SetAllColumnsDefaultView(view);

        //    xExistingAPIGrid.ShowViewCombo = Visibility.Collapsed;
        //    xNewAPIGrid.ShowViewCombo = Visibility.Collapsed;

        //    //# Custom View - Initial View
        //    xExistingAPIGrid.InitViewItems();
        //    xNewAPIGrid.InitViewItems();

        //}
        public Window ownerWindow;
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button OKButton = new Button();
            OKButton.Content = "OK";
            OKButton.Click += OKButton_Click;

            if (ownerWindow == null)
                ownerWindow = App.MainWindow;

            GingerCore.General.LoadGenericWindow(ref mWin, ownerWindow, windowStyle, @"Compare & Merge", this, new ObservableList<Button> { OKButton }, true, "Cancel");
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double vOffsetPos = xAPICompareSectionScroller.VerticalOffset;
            xAPIMergerSectionScroller.ScrollToVerticalOffset(vOffsetPos);
        }
    }
}
