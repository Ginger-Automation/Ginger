#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Common.APIModelLib;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger;
using Ginger.ApplicationModelsLib.APIModels;
using Ginger.ApplicationModelsLib.APIModels.APIModelWizard;
using Ginger.UserControls;
using GingerCore;
using GingerCoreNET.Application_Models;
using GingerCoreNET.GeneralLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static GingerWPF.ApplicationModelsLib.APIModels.APIModelWizard.AddAPIModelWizard;

namespace GingerWPF.ApplicationModelsLib.APIModels.APIModelWizard
{
    /// <summary>
    /// Interaction logic for ScanAPIModelWizardPage.xaml
    /// </summary>
    public partial class ScanAPIModelWizardPage : Page, IWizardPage
    {
        private string PrevURL;
        WSDLParser WSDLP;
        public AddAPIModelWizard AddAPIModelWizard;
        MergerWindow mergerWindow = null;

        public enum eAddAPIWizardViewStyle
        {
            Add,
            Compare
        }

        public ScanAPIModelWizardPage()
        {
            InitializeComponent();
            xApisSelectionGrid.SetTitleLightStyle = true;
            xApisSelectionGrid.btnMarkAll.Visibility = Visibility.Visible;
            xApisSelectionGrid.MarkUnMarkAllActive += MarkUnMarkAllActions;
            xApisSelectionGrid.btnRefresh.Visibility = Visibility.Visible;
            xApisSelectionGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnRefreshClicked));
            xCompreExistingItemBtn.Visibility = Visibility.Collapsed;
            SetFieldsGrid();
        }

        private void BtnRefreshClicked(object sender, RoutedEventArgs e)
        {
            AddAPIModelWizard.IsParsingWasDone = false;
            Parse();
        }

        void BtnCompareAPIClicked(object sender, RoutedEventArgs e)
        {
            AddAPIModelWizard.DeltaModelsList = new ObservableList<DeltaAPIModel>();
            APIDeltaUtils.ComparisonUtility(AddAPIModelWizard.LearnedAPIModelsList, AddAPIModelWizard.DeltaModelsList);

            xApisSelectionGrid.InitViewItems();

            xApisSelectionGrid.btnMarkAll.Visibility = Visibility.Collapsed;

            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.DeltaModelsList;

            xCompreExistingItemBtn.Visibility = Visibility.Collapsed;
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            if (WizardEventArgs.EventType == EventType.Init)
            {
                xApisSelectionGrid.ValidationRules.Add(Ginger.ucGrid.eUcGridValidationRules.CantBeEmpty);
                AddAPIModelWizard = ((AddAPIModelWizard)WizardEventArgs.Wizard);
                WSDLP = AddAPIModelWizard.mWSDLParser;
            }
            else if (WizardEventArgs.EventType == EventType.Prev)
            {
                if (AddAPIModelWizard.APIType == AddAPIModelWizard.eAPIType.WSDL)
                {
                    PrevURL = AddAPIModelWizard.URL;
                    WSDLP.mStopParsing = true;
                }

            }
            else if (WizardEventArgs.EventType == EventType.Cancel)
            {
                if (WSDLP != null && AddAPIModelWizard != null)
                {
                    WSDLP.mStopParsing = true;
                }
            }
            else if (WizardEventArgs.EventType == EventType.Active)
            {
                if (WizardEventArgs != null)
                {
                    Parse();
                }
            }
            else if (WizardEventArgs.EventType == EventType.LeavingForNextPage)
            {
                if (AddAPIModelWizard.DeltaModelsList != null && AddAPIModelWizard.DeltaModelsList.Count > 0)
                {
                    AddAPIModelWizard.LearnedAPIModelsList.Clear();
                    foreach (DeltaAPIModel deltaModel in AddAPIModelWizard.DeltaModelsList.Where(m => m.IsSelected == true))
                    {
                        ApplicationAPIModel selectedAPIModel = null;

                        if (deltaModel.DefaultOperationEnum == DeltaAPIModel.eHandlingOperations.Add)
                            selectedAPIModel = deltaModel.learnedAPI;
                        else if (deltaModel.DefaultOperationEnum == DeltaAPIModel.eHandlingOperations.MergeChanges)
                            selectedAPIModel = deltaModel.MergedAPIModel;
                        else if (deltaModel.DefaultOperationEnum == DeltaAPIModel.eHandlingOperations.ReplaceExisting)
                            selectedAPIModel = deltaModel.learnedAPI;

                        if (selectedAPIModel != null)
                            AddAPIModelWizard.LearnedAPIModelsList.Add(selectedAPIModel);
                    }
                }
            }
        }

        private async void Parse()
        {
            if (!AddAPIModelWizard.IsParsingWasDone)
            {
                bool parseSuccess = false;
                if (AddAPIModelWizard.LearnedAPIModelsList != null)
                    AddAPIModelWizard.LearnedAPIModelsList.Clear();

                if (AddAPIModelWizard.APIType == AddAPIModelWizard.eAPIType.WSDL)
                {
                    WSDLP = AddAPIModelWizard.mWSDLParser;
                    bool ParsingStoped = false;
                    if (WSDLP.mStopParsing)
                        ParsingStoped = true;
                    WSDLP.mStopParsing = false;

                    if (PrevURL != AddAPIModelWizard.URL || ParsingStoped)
                        parseSuccess = await ShowWSDLOperations();
                    else
                        parseSuccess = true;
                }
                else if (AddAPIModelWizard.APIType == AddAPIModelWizard.eAPIType.XMLTemplates)
                {
                    parseSuccess = await ShowXMLTemplatesOperations();
                }
                else if (AddAPIModelWizard.APIType == AddAPIModelWizard.eAPIType.JsonTemplate)
                {
                    parseSuccess = await ShowJsonTemplatesOperations();
                }
                else if (AddAPIModelWizard.APIType == AddAPIModelWizard.eAPIType.Swagger)
                {
                    parseSuccess = await ShowSwaggerOperations();
                }

                AddAPIModelWizard.IsParsingWasDone = parseSuccess;
                xCompreExistingItemBtn.Visibility = Visibility.Visible;
            }
        }

        private async Task<bool> ShowSwaggerOperations()
        {
            AddAPIModelWizard.ProcessStarted();
            bool parseSuccess = true;
            SwaggerParser SwaggerPar = new SwaggerParser();
            AddAPIModelWizard.LearnedAPIModelsList = new ObservableList<ApplicationAPIModel>();
            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.LearnedAPIModelsList;


            try
            {
                await Task.Run(() => SwaggerPar.ParseDocument(AddAPIModelWizard.URL, AddAPIModelWizard.LearnedAPIModelsList));
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.ParsingError, "Failed to Parse the Swagger File" + AddAPIModelWizard.URL);
                Reporter.ToLog(eLogLevel.ERROR, "Error Details: " + ex.Message + " Failed to Parse the Swagger file " + AddAPIModelWizard.URL);
                parseSuccess = false;
            }
            AddAPIModelWizard.ProcessEnded();

            return parseSuccess;
        }

        private async Task<bool> ShowXMLTemplatesOperations()
        {
            bool parseSuccess = true;
            AddAPIModelWizard.ProcessStarted();

            XMLTemplateParser WSDLP = new XMLTemplateParser();
            ObservableList<ApplicationAPIModel> AAMTempList = new ObservableList<ApplicationAPIModel>();
            ObservableList<ApplicationAPIModel> AAMCompletedList = new ObservableList<ApplicationAPIModel>();

            foreach (TemplateFile XTF in AddAPIModelWizard.XTFList)
            {
                try
                {
                    AAMTempList = await Task.Run(() => WSDLP.ParseDocument(XTF.FilePath, AAMCompletedList, AddAPIModelWizard.AvoidDuplicatesNodes));
                    if (!string.IsNullOrEmpty(XTF.MatchingResponseFilePath))
                        AAMTempList.Last().ReturnValues = await Task.Run(() => APIConfigurationsDocumentParserBase.ParseResponseSampleIntoReturnValuesPerFileType(XTF.MatchingResponseFilePath));
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.ParsingError, "Failed to Parse the XML" + XTF.FilePath);
                    Reporter.ToLog(eLogLevel.ERROR, "Error Details: " + ex.Message + "Failed to Parse the XML" + XTF.FilePath);
                    parseSuccess = false;
                }
            }

            AddAPIModelWizard.LearnedAPIModelsList = AAMCompletedList;
            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.LearnedAPIModelsList;
            AddAPIModelWizard.ProcessEnded();

            return parseSuccess;
        }
        private async Task<bool> ShowJsonTemplatesOperations()
        {
            bool parseSuccess = true;
            JSONTemplateParser JsonTemplate = new JSONTemplateParser();
            ObservableList<ApplicationAPIModel> AAMTempList = new ObservableList<ApplicationAPIModel>();
            ObservableList<ApplicationAPIModel> AAMCompletedList = new ObservableList<ApplicationAPIModel>();

            foreach (TemplateFile XTF in AddAPIModelWizard.XTFList)
            {
                try
                {
                    AAMTempList = await Task.Run(() => JsonTemplate.ParseDocument(XTF.FilePath, AAMCompletedList, AddAPIModelWizard.AvoidDuplicatesNodes));
                    if (!string.IsNullOrEmpty(XTF.MatchingResponseFilePath))
                        AAMTempList.Last().ReturnValues = await Task.Run(() => APIConfigurationsDocumentParserBase.ParseResponseSampleIntoReturnValuesPerFileType(XTF.MatchingResponseFilePath));
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.ParsingError, "Failed to Parse the JSon" + XTF.FilePath);
                    Reporter.ToLog(eLogLevel.ERROR,"Error Details: " + ex.Message + " Failed to Parse the JSon " + XTF.FilePath);
                    parseSuccess = false;
                }
            }

            AddAPIModelWizard.LearnedAPIModelsList = AAMCompletedList;
            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.LearnedAPIModelsList;

            return parseSuccess;
        }

        private async Task<bool> ShowWSDLOperations()
        {
            bool parseSuccess = true;

            AddAPIModelWizard.ProcessStarted();
            AddAPIModelWizard.LearnedAPIModelsList = new ObservableList<ApplicationAPIModel>();
            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.LearnedAPIModelsList;
            try
            {
                //ObservableList<ApplicationAPIModel> aaaModelList = GingerCore.General.ConvertListToObservableList(AddAPIModelWizard.AAMList.Select(m => m.learnedAPI).ToList());
                await Task.Run(() => WSDLP.ParseDocument(AddAPIModelWizard.URL, AddAPIModelWizard.LearnedAPIModelsList, false));
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.ParsingError, "Failed to Parse the WSDL " + ex.Message);
                parseSuccess = false;
            }

            AddAPIModelWizard.ProcessEnded();

            return parseSuccess;
        }

        private void MarkUnMarkAllActions(bool ActiveStatus)
        {
            if (xApisSelectionGrid.DataSourceList.Count <= 0) return;
            if (xApisSelectionGrid.DataSourceList.Count > 0)
            {
                ObservableList<ApplicationAPIModel> lstMarkUnMarkAPI = (ObservableList<ApplicationAPIModel>)xApisSelectionGrid.DataSourceList;
                foreach (ApplicationAPIModel AAMB in lstMarkUnMarkAPI)
                {
                    AAMB.IsSelected = ActiveStatus;
                }
                xApisSelectionGrid.DataSourceList = lstMarkUnMarkAPI;
            }
        }

        private void SetFieldsGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.IsSelected), Header = "Selected", WidthWeight = 30, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["IsSelectedTemplate"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.Name), Header = "Name", WidthWeight=250, BindingMode = BindingMode.OneWay});
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.Description), Header = "Description", WidthWeight = 250, BindingMode = BindingMode.OneWay });

            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.MatchingAPIName), Header = "Matching API Model", WidthWeight = 300, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.comparisonStatus), Header = "Comparison Status", WidthWeight = 150, MaxWidth=150, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["xDeltaStatusIconTemplate"], BindingMode = System.Windows.Data.BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.OperationsList), Header = "Difference's Handling Operation", WidthWeight = 200, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(DeltaAPIModel.OperationsList), nameof(DeltaAPIModel.defaultOperation), comboSelectionChangedHandler: XHandlingOperation_SelectionChanged) });
            //view.GridColsView.Add(new GridColView() { Field = nameof(LearnedAPIModels.OperationsList), Header = "Difference's Handling Operation", WidthWeight = 50, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = nameof(LearnedAPIModels.OperationsList) });
            view.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.defaultOperation), Header = "Compare & Merge", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["CompareAndMergeTemplate"]});
            xApisSelectionGrid.SetAllColumnsDefaultView(view);

            //# Custom View - Initial View
            GridViewDef initView = new GridViewDef(eAddAPIWizardViewStyle.Add.ToString());
            initView.GridColsView = new ObservableList<GridColView>();
            initView.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.MatchingAPIName), Header = "Matching API Model", WidthWeight = 20, Visible = false });
            initView.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.comparisonStatus), Header = "Comparison Status", WidthWeight = 150, Visible = false, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["xDeltaStatusIconTemplate"], BindingMode = System.Windows.Data.BindingMode.OneWay });
            //view.GridColsView.Add(new GridColView() { Field = nameof(APIModelsDelta.OperationsList), Header = "Difference's Handling Operation", Visible = false, WidthWeight=30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["xHandlingOperationTemplate"] });
            initView.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.OperationsList), Header = "Difference's Handling Operation", Visible = false});
            initView.GridColsView.Add(new GridColView() { Field = nameof(DeltaAPIModel.defaultOperation), Header = "Compare & Merge", Visible = false });
            xApisSelectionGrid.AddCustomView(initView);
            xApisSelectionGrid.ShowViewCombo = Visibility.Collapsed;

            xApisSelectionGrid.ChangeGridView(initView.Name);
        }

        private void ExportAPIFiles(List<ApplicationAPIModel> AAMList)
        {
            foreach (ApplicationAPIModel AAM in AAMList)
            {
                AAM.ContainingFolder = AddAPIModelWizard.APIModelFolder.FolderFullPath;
                AddAPIModelWizard.APIModelFolder.AddRepositoryItem(AAM);
            }
        }

        public class SoapData
        {
            public int Id { get; set; }
            public string RequestXml { get; set; }
            public string ResponseXml { get; set; }
            public string NameSpace { get; set; }
            public string OperationName { get; set; }
        }

        private void IsSelected_FieldSelection_Click(object sender, RoutedEventArgs e)
        {
            ObservableList<ApplicationAPIModel> CheckedList = GingerCore.General.ConvertListToObservableList(AddAPIModelWizard.LearnedAPIModelsList.Where(x => x.IsSelected == true).ToList());
        }

        private void xCompareAndMergeButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMergerWindow(sender);
        }

        void ShowMergerWindow(object sender)
        {
            DeltaAPIModel deltaAPI = null;
            var fEl = sender as FrameworkElement;
            if (fEl != null)
                deltaAPI = fEl.DataContext as DeltaAPIModel;
            if (deltaAPI != null)
            {
                bool showMergerWindow = false;
                if (deltaAPI.DefaultOperationEnum == DeltaAPIModel.eHandlingOperations.MergeChanges)
                    showMergerWindow = true;

                if(mergerWindow == null)
                    mergerWindow = new MergerWindow(deltaAPI);

                mergerWindow.ownerWindow = (Window)AddAPIModelWizard.mWizardWindow;
                mergerWindow.ShowAsWindow(showMergerWindow);
            }
        }

        private void XHandlingOperation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox handlingOpCB = sender as ComboBox;
            string mergerDescription = Convert.ToString(DeltaAPIModel.GetEnumDescription(DeltaAPIModel.eHandlingOperations.MergeChanges));

            DeltaAPIModel.eHandlingOperations selectedOperation = DeltaAPIModel.GetValueFromDescription<DeltaAPIModel.eHandlingOperations>(handlingOpCB.SelectedItem.ToString());

            DeltaAPIModel deltaAPI = null;
            var fEl = sender as FrameworkElement;
            if (fEl != null)
                deltaAPI = fEl.DataContext as DeltaAPIModel;
            if (deltaAPI != null)
            {
                deltaAPI.defaultOperation = Convert.ToString(handlingOpCB.SelectedItem);

                // Update default Operation Enum field
                deltaAPI.DefaultOperationEnum = selectedOperation;
                //if (selectedOperation == DeltaAPIModel.eHandlingOperations.DoNotAdd)
                //    deltaAPI.IsSelected = false;
                //else
                //    deltaAPI.IsSelected = true;
            }

            // Launch the Merger Window as Merge Changes is selected.
            if (handlingOpCB.SelectedItem.Equals(mergerDescription))
            {
                ShowMergerWindow(sender);
            }
        }
    }
}
