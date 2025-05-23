#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib.SwaggerApi;
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.ApplicationModelsLib.APIModels.APIModelWizard;
using Ginger.UserControls;
using GingerAutoPilot.APIModelLib.Postman;
using GingerCoreNET.Application_Models;
using GingerWPF.TreeViewItemsLib.ApplicationModelsTreeItems;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
        APIModelsCompareMergePage mergerWindow = null;

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

            xCompareBtnRow.Height = new GridLength(0);
            SetFieldsGrid();
        }

        private void BtnRefreshClicked(object sender, RoutedEventArgs e)
        {
            AddAPIModelWizard.IsParsingWasDone = false;
            Parse();
        }

        void BtnCompareAPIClicked(object sender, RoutedEventArgs e)
        {
            ObservableList<ApplicationAPIModel> selectedAPIModels = GingerCore.General.ConvertListToObservableList<ApplicationAPIModel>(AddAPIModelWizard.LearnedAPIModelsList.Where(m => m.IsSelected == true).ToList());

            AddAPIModelWizard.DeltaModelsList = new ObservableList<DeltaAPIModel>(APIDeltaUtils.DoAPIModelsCompare(selectedAPIModels).OrderBy(d => d.comparisonStatus));

            if (AddAPIModelWizard.DeltaModelsList.Where(f => f.matchingAPIModel != null).GroupBy(m => m.matchingAPIModel).Any())
            {
                foreach (DeltaAPIModel deltaAPIMod in AddAPIModelWizard.DeltaModelsList.Where(f => f.matchingAPIModel != null).GroupBy(m => m.matchingAPIModel).SelectMany(m => m))
                {
                    deltaAPIMod.MatchingAPIName = "[Warning] " + deltaAPIMod.MatchingAPIName;
                }
                Reporter.ToUser(eUserMsgKey.MultipleMatchingAPI, "Same existing API Models found as Matching for multiple Learned API Models. Consider the ones marked with 'Warning' tag in the list before selecting Overwriting operation for the same.", eUserMsgOption.OK);
            }

            xApisSelectionGrid.InitViewItems();

            xApisSelectionGrid.btnMarkAll.Visibility = Visibility.Collapsed;
            xCompareBtnRow.Height = new GridLength(0);

            //In case any item was selected on the Import Grid throws exception/error 
            //as selected Item won't anymore exist after updating the DataSource, hence, setting to null
            xApisSelectionGrid.grdMain.SelectedItem = null;

            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.DeltaModelsList;
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

                PrevURL = AddAPIModelWizard.URL;
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
                    bool mergeIssue = false;
                    bool notifyReplaceAPI = false;
                    AddAPIModelWizard.LearnedAPIModelsList.Clear();
                    List<DeltaAPIModel> repeatingMatchingModels = AddAPIModelWizard.DeltaModelsList.Where(m => m.SelectedOperationEnum is DeltaAPIModel.eHandlingOperations.ReplaceExisting or
                                                                DeltaAPIModel.eHandlingOperations.MergeChanges)
                                                                        .GroupBy(m => m.matchingAPIModel).SelectMany(m => m.Skip(1)).ToList();

                    bool matchingModelsReplacementIssue = repeatingMatchingModels.Any();
                    int apiCount = 0;
                    if (matchingModelsReplacementIssue)
                    {
                        System.Text.StringBuilder issueMsg = new System.Text.StringBuilder(Environment.NewLine + Environment.NewLine);
                        foreach (DeltaAPIModel deltaMod in repeatingMatchingModels)
                        {
                            apiCount++;
                            issueMsg.Append(apiCount + "). " + deltaMod.MatchingAPIName + Environment.NewLine);
                        }
                        Reporter.ToUser(eUserMsgKey.MultipleMatchingAPI, "Below Matching API Models selected to be replaced/overwritten multiple times:" + issueMsg.ToString());
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }

                    foreach (DeltaAPIModel deltaModel in AddAPIModelWizard.DeltaModelsList.Where(m => m.IsSelected == true))
                    {
                        ApplicationAPIModel selectedAPIModel = null;

                        if (deltaModel.SelectedOperationEnum == DeltaAPIModel.eHandlingOperations.Add)
                        {
                            selectedAPIModel = deltaModel.learnedAPI;
                        }
                        else if (deltaModel.SelectedOperationEnum == DeltaAPIModel.eHandlingOperations.MergeChanges)
                        {
                            if (deltaModel.MergedAPIModel == null)
                            {
                                mergeIssue = true;
                                break;
                            }
                            else
                            {
                                deltaModel.MergedAPIModel.ContainingFolder = deltaModel.matchingAPIModel.ContainingFolderFullPath;

                                selectedAPIModel = deltaModel.MergedAPIModel;
                                notifyReplaceAPI = true;
                            }
                        }
                        else if (deltaModel.SelectedOperationEnum == DeltaAPIModel.eHandlingOperations.ReplaceExisting)
                        {
                            deltaModel.learnedAPI.ContainingFolder = deltaModel.matchingAPIModel.ContainingFolderFullPath;

                            selectedAPIModel = deltaModel.learnedAPI;
                            notifyReplaceAPI = true;
                        }

                        if (selectedAPIModel != null)
                        {
                            AddAPIModelWizard.LearnedAPIModelsList.Add(selectedAPIModel);
                        }
                    }

                    if (mergeIssue)
                    {
                        Reporter.ToUser(eUserMsgKey.BaseAPIWarning, "Please configure Merged API on Compare and Merge window.");
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }

                    if (notifyReplaceAPI && Reporter.ToUser(eUserMsgKey.SureWantToContinue, "API Models", "API Models", eUserMsgOption.YesNo) == eUserMsgSelection.No)
                    {
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }
                }
            }
        }

        private async void Parse()
        {
            if (!AddAPIModelWizard.IsParsingWasDone)
            {
                xCompareBtnRow.Height = new GridLength(0);
                if (AddAPIModelWizard.DeltaModelsList != null)
                {
                    xApisSelectionGrid.DataSourceList = AddAPIModelWizard.LearnedAPIModelsList;
                    xApisSelectionGrid.ChangeGridView(nameof(eAddAPIWizardViewStyle.Add));
                    xApisSelectionGrid.btnMarkAll.Visibility = Visibility.Visible;
                }
                bool parseSuccess = false;
                if (AddAPIModelWizard.LearnedAPIModelsList != null)
                {
                    AddAPIModelWizard.LearnedAPIModelsList.Clear();
                }

                if (AddAPIModelWizard.APIType == AddAPIModelWizard.eAPIType.WSDL)
                {
                    WSDLP = AddAPIModelWizard.mWSDLParser;
                    bool ParsingStoped = false;
                    if (WSDLP.mStopParsing)
                    {
                        ParsingStoped = true;
                    }

                    WSDLP.mStopParsing = false;

                    if (PrevURL != AddAPIModelWizard.URL || ParsingStoped)
                    {
                        parseSuccess = await ShowWSDLOperations();
                    }
                    else
                    {
                        parseSuccess = true;
                    }
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
                else if (AddAPIModelWizard.APIType == AddAPIModelWizard.eAPIType.PostmanCollection)
                {
                    parseSuccess = await ShowPostmanCollectionParserOperations();
                }

                AddAPIModelWizard.IsParsingWasDone = parseSuccess;
                xCompareBtnRow.Height = new GridLength(50);
            }
        }

        private async Task<bool> ShowPostmanCollectionParserOperations()
        {
            using IFeatureTracker featureTracker = Reporter.StartFeatureTracking(FeatureId.AAMLearning);
            featureTracker.Metadata.Add("APIType", "PostmanCollection");
            featureTracker.Metadata.Add("FileType", "JSON");


            AddAPIModelWizard.ProcessStarted();
            bool parseSuccess = true;
            PostmanCollectionParser postmanCollectionParser = new();
            AddAPIModelWizard.LearnedAPIModelsList = [];
            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.LearnedAPIModelsList;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        postmanCollectionParser.ParseDocument(AddAPIModelWizard.URL, AddAPIModelWizard.LearnedAPIModelsList);
                        AddAPIModelWizard.InfoTitle = postmanCollectionParser.GetInfoTitle();
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Error Details: {ex.Message} Failed to Parse the Postman Collection file {AddAPIModelWizard.URL}");
                    }
                });
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.ParsingError, "Failed to Parse the Postman Collection  File" + AddAPIModelWizard.URL);
                Reporter.ToLog(eLogLevel.ERROR, "Error Details: " + ex.Message + " Failed to Parse the Postman Collection  file " + AddAPIModelWizard.URL);
                parseSuccess = false;
            }
            finally
            {
                AddAPIModelWizard.ProcessEnded();
            }

            return parseSuccess;
        }

        private async Task<bool> ShowSwaggerOperations()
        {
            using IFeatureTracker featureTracker = Reporter.StartFeatureTracking(FeatureId.AAMLearning);
            featureTracker.Metadata.Add("APIType", "Swagger");
            if (SwaggerParser.IsValidYaml((AddAPIModelWizard.URL)))
            {
                featureTracker.Metadata.Add("FileType", "YAML");
            }
            else
            {
                featureTracker.Metadata.Add("FileType", "JSON");
            }

            AddAPIModelWizard.ProcessStarted();
            bool parseSuccess = true;
            SwaggerParser SwaggerPar = new SwaggerParser();
            AddAPIModelWizard.LearnedAPIModelsList = [];
            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.LearnedAPIModelsList;


            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        SwaggerPar.ParseDocument(AddAPIModelWizard.URL, AddAPIModelWizard.LearnedAPIModelsList);

                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Error Details: {ex.Message} Failed to Parse the Swagger file {AddAPIModelWizard.URL}");
                    }
                });
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
            using IFeatureTracker featureTracker = Reporter.StartFeatureTracking(FeatureId.AAMLearning);
            featureTracker.Metadata.Add("APIType", "XMLTemplate");

            bool parseSuccess = true;
            AddAPIModelWizard.ProcessStarted();

            XMLTemplateParser WSDLP = new XMLTemplateParser();
            ObservableList<ApplicationAPIModel> AAMTempList = [];
            ObservableList<ApplicationAPIModel> AAMCompletedList = [];

            foreach (TemplateFile XTF in AddAPIModelWizard.XTFList)
            {
                try
                {
                    AAMTempList = await Task.Run(() =>
                    {
                        try
                        {
                            return WSDLP.ParseDocument(XTF.FilePath, AAMCompletedList, AddAPIModelWizard.AvoidDuplicatesNodes);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Error Details: {ex.Message} Failed to Parse the XML{XTF.FilePath}");
                            return null;
                        }
                    });
                    if (!string.IsNullOrEmpty(XTF.MatchingResponseFilePath))
                    {
                        AAMTempList.Last().ReturnValues = await Task.Run(() =>
                        {
                            try
                            {
                                return APIConfigurationsDocumentParserBase.ParseResponseSampleIntoReturnValuesPerFileType(XTF.MatchingResponseFilePath);
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, $"Error Details: {ex.Message} Failed to Parse the XML{XTF.FilePath}");
                                return null;
                            }
                        });
                    }
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
            using IFeatureTracker featureTracker = Reporter.StartFeatureTracking(FeatureId.AAMLearning);
            featureTracker.Metadata.Add("APIType", "JSON_Template");

            bool parseSuccess = true;
            JSONTemplateParser JsonTemplate = new JSONTemplateParser();
            ObservableList<ApplicationAPIModel> AAMTempList = [];
            ObservableList<ApplicationAPIModel> AAMCompletedList = [];

            foreach (TemplateFile XTF in AddAPIModelWizard.XTFList)
            {
                try
                {
                    AAMTempList = await Task.Run(() =>
                    {
                        try
                        {
                            return JsonTemplate.ParseDocument(XTF.FilePath, AAMCompletedList, AddAPIModelWizard.AvoidDuplicatesNodes);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Error Details: {ex.Message} Failed to Parse the JSon {XTF.FilePath}");
                            return [];
                        }
                    });
                    if (!string.IsNullOrEmpty(XTF.MatchingResponseFilePath))
                    {
                        AAMTempList.Last().ReturnValues = await Task.Run(() =>
                        {
                            try
                            {
                                return APIConfigurationsDocumentParserBase.ParseResponseSampleIntoReturnValuesPerFileType(XTF.MatchingResponseFilePath);
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, $"Error Details: {ex.Message} Failed to Parse the JSon {XTF.FilePath}");
                                return null;
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.ParsingError, "Failed to Parse the JSon" + XTF.FilePath);
                    Reporter.ToLog(eLogLevel.ERROR, "Error Details: " + ex.Message + " Failed to Parse the JSon " + XTF.FilePath);
                    parseSuccess = false;
                }
            }

            AddAPIModelWizard.LearnedAPIModelsList = AAMCompletedList;
            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.LearnedAPIModelsList;

            return parseSuccess;
        }

        private async Task<bool> ShowWSDLOperations()
        {
            using IFeatureTracker featureTracker = Reporter.StartFeatureTracking(FeatureId.AAMLearning);
            featureTracker.Metadata.Add("APIType", "WSDL");

            bool parseSuccess = true;

            AddAPIModelWizard.ProcessStarted();
            AddAPIModelWizard.LearnedAPIModelsList = [];
            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.LearnedAPIModelsList;
            try
            {
                //ObservableList<ApplicationAPIModel> aaaModelList = GingerCore.General.ConvertListToObservableList(AddAPIModelWizard.AAMList.Select(m => m.learnedAPI).ToList());
                await Task.Run(() =>
                {
                    try
                    {
                        WSDLP.ParseDocument(AddAPIModelWizard.URL, AddAPIModelWizard.LearnedAPIModelsList, false);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToUser(eUserMsgKey.ParsingError, "Failed to Parse the WSDL ", ex);
                    }
                });
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
            if (xApisSelectionGrid.DataSourceList == null || xApisSelectionGrid.DataSourceList.Count <= 0)
            {
                return;
            }

            if (xApisSelectionGrid.DataSourceList?.Count > 0)
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
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(DeltaAPIModel.Name), Header = "Name", WidthWeight = 250, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = nameof(DeltaAPIModel.Description), Header = "Description", WidthWeight = 250, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = nameof(DeltaAPIModel.MatchingAPIName), Header = "Matching API Model", WidthWeight = 300, BindingMode = BindingMode.OneWay, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["xMatchingModelTemplate"] },
                new GridColView() { Field = nameof(DeltaAPIModel.comparisonStatus), Header = "Comp. Status", WidthWeight = 150, MaxWidth = 150, AllowSorting = true, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["xDeltaStatusIconTemplate"], BindingMode = System.Windows.Data.BindingMode.OneWay },
                new GridColView() { Field = nameof(DeltaAPIModel.OperationsList), Header = "Comp. Operation", WidthWeight = 200, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(nameof(DeltaAPIModel.OperationsList), nameof(DeltaAPIModel.SelectedOperation), comboSelectionChangedHandler: XHandlingOperation_SelectionChanged) },
                new GridColView() { Field = nameof(DeltaAPIModel.SelectedOperation), Header = "Comp. & Merge", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["xCompareAndMergeTemplate"] },
            ]
            };
            xApisSelectionGrid.SetAllColumnsDefaultView(view);

            //# Custom View - Initial View
            GridViewDef initView = new GridViewDef(eAddAPIWizardViewStyle.Add.ToString())
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(DeltaAPIModel.IsSelected), Header = "Selected", Order = 0, WidthWeight = 50, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["xIsSelectedTemplate"] },
                new GridColView() { Field = nameof(DeltaAPIModel.MatchingAPIName), Header = "Matching API Model", WidthWeight = 20, Visible = false },
                new GridColView() { Field = nameof(DeltaAPIModel.comparisonStatus), Header = "Comp. Status", WidthWeight = 150, Visible = false, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["xDeltaStatusIconTemplate"], BindingMode = System.Windows.Data.BindingMode.OneWay },
                new GridColView() { Field = nameof(DeltaAPIModel.OperationsList), Header = "Comp. Operation", Visible = false },
                new GridColView() { Field = nameof(DeltaAPIModel.SelectedOperation), Header = "Comp. & Merge", Visible = false },
            ]
            };

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
            ShowMergerPage(sender);
        }

        void ShowMergerPage(object sender)
        {
            DeltaAPIModel deltaAPI = null;
            if (sender is FrameworkElement fEl)
            {
                deltaAPI = fEl.DataContext as DeltaAPIModel;
            }

            if (deltaAPI != null)
            {
                if (deltaAPI.mergerPageObject == null)
                {
                    deltaAPI.mergerPageObject = new APIModelsCompareMergePage(deltaAPI, (Window)AddAPIModelWizard.mWizardWindow);
                }

                (deltaAPI.mergerPageObject as APIModelsCompareMergePage).ShowAsWindow();
            }
        }

        private void XHandlingOperation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string mergerDescription = Convert.ToString(DeltaAPIModel.GetEnumDescription(DeltaAPIModel.eHandlingOperations.MergeChanges));

            if (sender is ComboBox handlingOpCB && handlingOpCB.SelectedItem != null)
            {
                DeltaAPIModel.eHandlingOperations selectedOperation = DeltaAPIModel.GetValueFromDescription<DeltaAPIModel.eHandlingOperations>(Convert.ToString(handlingOpCB.SelectedItem));

                DeltaAPIModel deltaAPI = null;
                if (sender is FrameworkElement fEl)
                {
                    deltaAPI = fEl.DataContext as DeltaAPIModel;
                }

                if (deltaAPI != null)
                {
                    deltaAPI.SelectedOperation = Convert.ToString(handlingOpCB.SelectedItem);

                    // Update default Operation Enum field
                    deltaAPI.SelectedOperationEnum = selectedOperation;
                    if (selectedOperation == DeltaAPIModel.eHandlingOperations.DoNotAdd)
                    {
                        deltaAPI.IsSelected = false;
                    }
                    else
                    {
                        deltaAPI.IsSelected = true;
                    }
                }

                // Launch the Merger Window as Merge Changes is selected.
                if (handlingOpCB.SelectedItem.Equals(mergerDescription))
                {
                    ShowMergerPage(sender);
                }
            }
        }

        private object GetFrameElementDataContext(object sender)
        {
            if (sender is FrameworkElement fEl)
            {
                return fEl.DataContext;
            }
            else
            {
                return null;
            }
        }

        private void XManualMatchBtn_Click(object sender, RoutedEventArgs e)
        {
            DeltaAPIModel deltaAPI = null;
            deltaAPI = GetFrameElementDataContext(sender) as DeltaAPIModel;

            if (deltaAPI != null)
            {
                int indexOfCurrentDelta = xApisSelectionGrid.DataSourceList.IndexOf(deltaAPI);
                SingleItemTreeViewSelectionPage apiModelTreeSelectionPage = null;
                AppApiModelsFolderTreeItem apiRoot = new AppApiModelsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>());

                apiModelTreeSelectionPage = new SingleItemTreeViewSelectionPage("API Models", eImageType.APIModel, apiRoot, SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true,
                                                                                    new System.Tuple<string, string>(nameof(ApplicationAPIModel.APIType), deltaAPI.learnedAPI.APIType.ToString()));

                List<object> selectedList = apiModelTreeSelectionPage.ShowAsWindow("Matching API Models", (Window)AddAPIModelWizard.mWizardWindow);

                ApplicationAPIModel selectedAPIModel = null;
                if (selectedList != null)
                {
                    selectedAPIModel = selectedList.FirstOrDefault() as ApplicationAPIModel;
                    //deltaAPI.matchingAPIModel = selectedAPIModel;

                    ObservableList<ApplicationAPIModel> selectedMatchingAPIList = [selectedAPIModel];
                    ObservableList<ApplicationAPIModel> apiModelsListLearned = [deltaAPI.learnedAPI];

                    ObservableList<DeltaAPIModel> comparisonOutputDelta = APIDeltaUtils.DoAPIModelsCompare(apiModelsListLearned, selectedMatchingAPIList);

                    deltaAPI = comparisonOutputDelta.FirstOrDefault();

                    xApisSelectionGrid.DataSourceList[indexOfCurrentDelta] = deltaAPI;
                }
            }

        }

        private void xClearMatchBtn_Click(object sender, RoutedEventArgs e)
        {
            DeltaAPIModel deltaAPI = null;
            deltaAPI = GetFrameElementDataContext(sender) as DeltaAPIModel;

            int modelIndex = xApisSelectionGrid.DataSourceList.IndexOf(deltaAPI);
            if (deltaAPI != null)
            {
                if (deltaAPI.matchingAPIModel != null)
                {
                    deltaAPI.matchingAPIModel = null;
                    deltaAPI.comparisonStatus = DeltaAPIModel.eComparisonOutput.Unknown;
                    deltaAPI.SelectedOperationEnum = DeltaAPIModel.eHandlingOperations.Add;
                    deltaAPI.SelectedOperation = DeltaAPIModel.GetEnumDescription(deltaAPI.SelectedOperationEnum);

                    xApisSelectionGrid.DataSourceList.RemoveAt(modelIndex);
                    xApisSelectionGrid.DataSourceList.Insert(modelIndex, deltaAPI);
                }
            }
        }
    }
}
