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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationsModels.ModelsUsages;
using Ginger.UserControls;
using GingerCore;
using GingerWPF.ApplicationModelsLib.ModelParams_Pages;
using GingerWPF.BindingLib;
using GingerWPF.UserControlsLib;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GingerWPF.ApplicationModelsLib.APIModels
{
    /// <summary>
    /// Interaction logic for OutputTemplatePage.xaml
    /// </summary>
    public partial class OutputTemplatePage : Page
    {
        private enum eGridView { All, NonSimulation }
        ApplicationAPIModel mApplicationAPIModel;

        public OutputTemplatePage(ApplicationAPIModel applicationAPIModel)
        {
            InitializeComponent();
            mApplicationAPIModel = applicationAPIModel;

            xOutputValuesGrid.AddToolbarTool("@Share_16x16.png", "Push Changes to All Relevant Actions", new RoutedEventHandler(PushChangesClicked));
            xOutputValuesGrid.AddToolbarTool("@Import_16x16.png", "Import output values from Response sample file", new RoutedEventHandler(ImpurtButtonClicked));


            xOutputValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddReturnValue));
            xOutputValuesGrid.AddSeparator();

            ControlsBinding.ObjFieldBinding(xOutputValuesGrid.AddCheckBox("Support Simulation", new RoutedEventHandler(RefreshOutputColumns)), CheckBox.IsCheckedProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.SupportSimulation));
            SetActReturnValuesGrid();

            xOutputValuesGrid.DataSourceList = mApplicationAPIModel.ReturnValues;
        }

        private void ImpurtButtonClicked(object sender, RoutedEventArgs e)
        {
            if (mApplicationAPIModel.ReturnValues.Count > 0)
            {
                if (Reporter.ToUser(eUserMsgKeys.APIModelAlreadyContainsReturnValues, mApplicationAPIModel.ReturnValues.Count) == MessageBoxResult.Yes)
                {
                    BrowseAndParseResponseFile();
                }
            }
            else
                BrowseAndParseResponseFile();
        }

        private void BrowseAndParseResponseFile()
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();

            dlg.Filter = "All Files (*.*)|*.*" + "|XML Files (*.xml)|*.xml" + "|WSDL Files (*.wsdl)|*.wsdl" + "|JSON Files (*.json)|*.json";
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                mApplicationAPIModel.ReturnValues.Clear();
                //FOREACH
                ObservableList<ActReturnValue> ReturnValues = APIConfigurationsDocumentParserBase.ParseResponseSampleIntoReturnValuesPerFileType(dlg.FileName);
                foreach (ActReturnValue ReturnValue in ReturnValues)
                {
                    mApplicationAPIModel.ReturnValues.Add(ReturnValue);
                }
                
            }
        }



        private void PushChangesClicked(object sender, RoutedEventArgs e)
        {
            ModelItemUsagePage modelItemUsagePage = new ModelItemUsagePage(mApplicationAPIModel, ApplicationModelBase.eModelUsageUpdateType.SinglePart, ApplicationModelBase.eModelParts.ReturnValues);
            modelItemUsagePage.ShowAsWindow();
        }

        private void AddReturnValue(object sender, RoutedEventArgs e)
        {
            mApplicationAPIModel.ReturnValues.Add(new ActReturnValue() { Active = true, DoNotConsiderAsTemp=true });
        }

        private void RefreshOutputColumns(object sender, RoutedEventArgs e)
        {
            if (mApplicationAPIModel.SupportSimulation)
                xOutputValuesGrid.ChangeGridView(eGridView.All.ToString());
            else
                xOutputValuesGrid.ChangeGridView(eGridView.NonSimulation.ToString());
        }

        private void SetActReturnValuesGrid()
        {
            GridViewDef SimView = new GridViewDef(eGridView.All.ToString());
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            SimView.GridColsView = viewCols;

            //Simulation view
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Active, WidthWeight = 50, StyleType = GridColView.eGridColStyleType.CheckBox });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Param, Header = "Parameter", WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Path, WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.SimulatedActual, Header = "Simulated Value", WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Expected, Header = "Expected Value", WidthWeight = 150 });
            viewCols.Add(new GridColView() { Field = ".....", Header = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ValueExpressionButton"] });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.StoreToValue, Header = "Store To ", WidthWeight = 150, StyleType = GridColView.eGridColStyleType.ComboBox, ComboboxDisplayMemberField = nameof(GlobalAppModelParameter.PlaceHolder), ComboboxSelectedValueField = nameof(GlobalAppModelParameter.Guid), ComboboxSortBy = nameof(GlobalAppModelParameter.PlaceHolder), CellValuesList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>() });
            
            //Default mode view
            GridViewDef defView = new GridViewDef(eGridView.NonSimulation.ToString());
            defView.GridColsView = new ObservableList<GridColView>();
            ObservableList<GridColView> defviewCols = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = ActReturnValue.Fields.SimulatedActual, Visible = false });
            defView.GridColsView.Add(new GridColView() { Field = "....", Visible = false });

            xOutputValuesGrid.SetAllColumnsDefaultView(SimView);
            xOutputValuesGrid.AddCustomView(defView);
            xOutputValuesGrid.InitViewItems();

            if (mApplicationAPIModel.SupportSimulation == true)
                xOutputValuesGrid.ChangeGridView(eGridView.All.ToString());
            else
                xOutputValuesGrid.ChangeGridView(eGridView.NonSimulation.ToString());

            xOutputValuesGrid.ShowTitle = Visibility.Collapsed;
            xOutputValuesGrid.ShowViewCombo = Visibility.Collapsed;
            xOutputValuesGrid.ShowEdit = Visibility.Collapsed;
            xOutputValuesGrid.ShowRefresh = Visibility.Collapsed;
        }

        private void GridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ModelExpectedValueParamSelectionPage ParamSelectionPage = new ModelExpectedValueParamSelectionPage(mApplicationAPIModel.MergedParamsList);
            AppModelParameter selectedParam = ParamSelectionPage.ShowAsWindow();

            if (selectedParam != null)
                ((ActReturnValue)xOutputValuesGrid.Grid.SelectedItem).Expected = GetParamWithStringTemplate(selectedParam);
        }

        private void SimulatedOutputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ModelExpectedValueParamSelectionPage ParamSelectionPage = new ModelExpectedValueParamSelectionPage(mApplicationAPIModel.MergedParamsList);
            AppModelParameter selectedParam = ParamSelectionPage.ShowAsWindow();

            if (selectedParam != null)
                ((ActReturnValue)xOutputValuesGrid.Grid.SelectedItem).SimulatedActual = GetParamWithStringTemplate(selectedParam);
        }


        private void GridParamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ModelExpectedValueParamSelectionPage ParamSelectionPage = new ModelExpectedValueParamSelectionPage(mApplicationAPIModel.MergedParamsList);
            AppModelParameter selectedParam = ParamSelectionPage.ShowAsWindow();

            if (selectedParam != null)
                ((ActReturnValue)xOutputValuesGrid.Grid.SelectedItem).Param = GetParamWithStringTemplate(selectedParam);
        }

        private void GridPathVEButton_Click(object sender, RoutedEventArgs e)
        {
            ModelExpectedValueParamSelectionPage ParamSelectionPage = new ModelExpectedValueParamSelectionPage(mApplicationAPIModel.MergedParamsList);
            AppModelParameter selectedParam = ParamSelectionPage.ShowAsWindow();

            if (selectedParam != null)
                ((ActReturnValue)xOutputValuesGrid.Grid.SelectedItem).Path = GetParamWithStringTemplate(selectedParam);
        }

        private string GetParamWithStringTemplate(AppModelParameter param)
        {
            return "{AppModelParam Name = " + param.PlaceHolder + "}";
        }

        private void GridDSVEButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DSConfig(object sender, RoutedEventArgs e)
        {
        }
        
        public void BindUiControls()
        {
        }

        private void ActionTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class ActReturnValueStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
        {
            string status = value.ToString();
            if (status.Equals(ActReturnValue.eStatus.Passed.ToString())) return System.Windows.Media.Brushes.Green;//System.Drawing.Brushes.Green;
            if (status.Equals(ActReturnValue.eStatus.Failed.ToString())) return System.Windows.Media.Brushes.Red;
            if (status.Equals(ActReturnValue.eStatus.Pending.ToString())) return System.Windows.Media.Brushes.Orange;
            if (status.Equals(ActReturnValue.eStatus.Skipped.ToString())) return System.Windows.Media.Brushes.Black;

            return System.Drawing.Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
