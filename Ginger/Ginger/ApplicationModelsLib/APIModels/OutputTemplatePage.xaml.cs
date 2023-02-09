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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.ApplicationsModels.ModelsUsages;
using Ginger.UserControls;
using GingerWPF.ApplicationModelsLib.ModelParams_Pages;
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
        Ginger.General.eRIPageViewMode mPageViewMode;

        public OutputTemplatePage(ApplicationAPIModel applicationAPIModel, Ginger.General.eRIPageViewMode pageViewMode = Ginger.General.eRIPageViewMode.Standalone)
        {
            InitializeComponent();
            mApplicationAPIModel = applicationAPIModel;

            mPageViewMode = pageViewMode;

            if (pageViewMode == Ginger.General.eRIPageViewMode.View || pageViewMode == Ginger.General.eRIPageViewMode.ViewAndExecute)
            {
                xOutputValuesGrid.ShowAdd = Visibility.Collapsed;
                xOutputValuesGrid.ShowUpDown = Visibility.Collapsed;
                xOutputValuesGrid.ShowDelete = Visibility.Collapsed;
                xOutputValuesGrid.ShowSearch = Visibility.Collapsed;
                xOutputValuesGrid.ShowClearAll = Visibility.Collapsed;
                xOutputValuesGrid.ShowCopy = Visibility.Visible;

                xOutputValuesGrid.IsReadOnly = true;
            }
            else
            {
                xOutputValuesGrid.AddToolbarTool("@Share_16x16.png", "Push Changes to All Relevant Actions", new RoutedEventHandler(PushChangesClicked));
                xOutputValuesGrid.AddToolbarTool("@Import_16x16.png", "Import output values from Response sample file", new RoutedEventHandler(ImpurtButtonClicked));

                xOutputValuesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddReturnValue));
                xOutputValuesGrid.AddSeparator();

                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xOutputValuesGrid.AddCheckBox("Support Simulation", new RoutedEventHandler(RefreshOutputColumns)), CheckBox.IsCheckedProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.SupportSimulation));
            }

            if(pageViewMode == Ginger.General.eRIPageViewMode.Add)
            {
                xOutputValuesGrid.ShowPaste = Visibility.Visible;
            }

            SetActReturnValuesGrid();

            xOutputValuesGrid.DataSourceList = mApplicationAPIModel.ReturnValues;
        }

        private void ImpurtButtonClicked(object sender, RoutedEventArgs e)
        {
            if (mApplicationAPIModel.ReturnValues.Count > 0)
            {
                if (Reporter.ToUser(eUserMsgKey.APIModelAlreadyContainsReturnValues, mApplicationAPIModel.ReturnValues.Count) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    BrowseAndParseResponseFile();
                }
            }
            else
                BrowseAndParseResponseFile();
        }

        private void BrowseAndParseResponseFile()
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "All Files (*.*)|*.*" + "|XML Files (*.xml)|*.xml" + "|WSDL Files (*.wsdl)|*.wsdl" + "|JSON Files (*.json)|*.json"
            }, false) is string fileName)
            {
                mApplicationAPIModel.ReturnValues.Clear();
                //FOREACH
                ObservableList<ActReturnValue> ReturnValues = APIConfigurationsDocumentParserBase.ParseResponseSampleIntoReturnValuesPerFileType(fileName);
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
            mApplicationAPIModel.ReturnValues.Add(new ActReturnValue() { Active = true, DoNotConsiderAsTemp = true });
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
            bool setColumnsReadOnly = (mPageViewMode == Ginger.General.eRIPageViewMode.View || mPageViewMode == Ginger.General.eRIPageViewMode.ViewAndExecute);

            //Simulation view
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Active, WidthWeight = 50, StyleType = GridColView.eGridColStyleType.CheckBox, ReadOnly = setColumnsReadOnly });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Param, Header = "Parameter", WidthWeight = 150, ReadOnly = setColumnsReadOnly });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Path, WidthWeight = 150, ReadOnly = setColumnsReadOnly });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.SimulatedActual, Header = "Simulated Value", WidthWeight = 150, ReadOnly = setColumnsReadOnly });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.Expected, Header = "Expected Value", WidthWeight = 150, ReadOnly = setColumnsReadOnly });
            viewCols.Add(new GridColView() { Field = "...", Header = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ValueExpressionButton"], ReadOnly = setColumnsReadOnly });
            viewCols.Add(new GridColView() { Field = ActReturnValue.Fields.StoreToValue, Header = "Store To", ReadOnly = setColumnsReadOnly, WidthWeight = 150, StyleType = GridColView.eGridColStyleType.ComboBox, ComboboxDisplayMemberField = nameof(GlobalAppModelParameter.PlaceHolder), ComboboxSelectedValueField = nameof(GlobalAppModelParameter.Guid), CellValuesList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>() });
            viewCols.Add(new GridColView() { Field = "Clear Store To", Header = "Clear Store To", ReadOnly = setColumnsReadOnly, WidthWeight = 35, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ClearStoreToBtnTemplate"] });

            //Default mode view
            GridViewDef defView = new GridViewDef(eGridView.NonSimulation.ToString());
            defView.GridColsView = new ObservableList<GridColView>();
            ObservableList<GridColView> defviewCols = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = ActReturnValue.Fields.SimulatedActual, Visible = false, ReadOnly = setColumnsReadOnly });
            defView.GridColsView.Add(new GridColView() { Field = "....", Visible = false, ReadOnly = setColumnsReadOnly });

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

        private void GridClearStoreToBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xOutputValuesGrid.Grid.SelectedItem != null)
            {
                ((ActReturnValue)xOutputValuesGrid.Grid.SelectedItem).StoreToValue = null;
            }
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
