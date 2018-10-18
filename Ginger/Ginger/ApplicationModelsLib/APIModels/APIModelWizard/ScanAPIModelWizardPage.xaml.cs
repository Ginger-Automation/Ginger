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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.APIModelLib;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using GingerCoreNET.GeneralLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        public ScanAPIModelWizardPage()
        {
            InitializeComponent();
            xApisSelectionGrid.SetTitleLightStyle = true;
            xApisSelectionGrid.btnMarkAll.Visibility = Visibility.Visible;
            xApisSelectionGrid.MarkUnMarkAllActive += MarkUnMarkAllActions;
            xApisSelectionGrid.btnRefresh.Visibility = Visibility.Visible;
            xApisSelectionGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnRefreshClicked));
            SetFieldsGrid();
        }
        private void BtnRefreshClicked(object sender, RoutedEventArgs e)
        {
            AddAPIModelWizard.IsParsingWasDone = false;
            Parse();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            if (WizardEventArgs.EventType == EventType.Init)
            {
                xApisSelectionGrid.ValidationRules.Add(Ginger.ucGrid.eUcGridValidationRules.CantBeEmpty);
            }
            else if (WizardEventArgs.EventType == EventType.Prev)
            {
                if (AddAPIModelWizard.APIType == AddAPIModelWizard.eAPIType.WSDL)
                {
                    PrevURL = AddAPIModelWizard.URL;
                    WSDLP.mStopParsing = true;
                }

                // AddAPIModelWizard.ProcessEnded();
            }
            else if (WizardEventArgs.EventType == EventType.Cancel)
            {
                if (WSDLP != null && AddAPIModelWizard != null)
                {
                    WSDLP.mStopParsing = true;
                    // AddAPIModelWizard.ProcessStarted = Visibility.Collapsed;
                }
            }
            else if (WizardEventArgs.EventType == EventType.Active)
            {
                if (WizardEventArgs != null)
                {
                    AddAPIModelWizard = ((AddAPIModelWizard)WizardEventArgs.Wizard);
                    //AddAPIModelWizard.FinishEnabled = false;
                    //AddAPIModelWizard.NextEnabled = false;
                    //WizardEventArgs.IgnoreDefaultNextButtonSettings = true;
                    Parse();
                }
            }
            //else if (WizardEventArgs.EventType == EventType.LeavingForNextPage)
            //{
            //    if (AddAPIModelWizard.AAMList != null)
            //    {
            //        ObservableList<ApplicationAPIModel> List = General.ConvertListToObservableList(AddAPIModelWizard.AAMList.Where(x => x.IsSelected == true).ToList());
            //        AddAPIModelWizard.SelectedAAMList = List;
            //    }
            //}
        }

        private async void Parse()
        {                       
            if (!AddAPIModelWizard.IsParsingWasDone)
            {
                bool parseSuccess = false;
                if (AddAPIModelWizard.AAMList != null)
                    AddAPIModelWizard.AAMList.Clear();

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
                //if (parseSuccess)
                  //  AddAPIModelWizard.NextEnabled = true;
            }
            //else
            //{
            //    // AddAPIModelWizard.NextEnabled = true;
            //}
        }

        private async Task<bool> ShowSwaggerOperations()
        {
            AddAPIModelWizard.ProcessStarted(); 
            bool parseSuccess = true;
            SwaggerParser SwaggerPar = new SwaggerParser();
            ObservableList<ApplicationAPIModel> AAMTempList = new ObservableList<ApplicationAPIModel>();
            ObservableList<ApplicationAPIModel> AAMCompletedList = new ObservableList<ApplicationAPIModel>();


           
                try
                {
                    AAMTempList = await Task.Run(() => SwaggerPar.ParseDocument(AddAPIModelWizard.URL));
                    AAMCompletedList=AAMTempList;
                }
                catch (Exception ex)
                {               
                    Reporter.ToUser(eUserMsgKeys.ParsingError, "Failed to Parse the Swagger File" + AddAPIModelWizard.URL);
                    GingerCoreNET.ReporterLib.Reporter.ToLog(GingerCoreNET.ReporterLib.eLogLevel.ERROR, "Error Details: " + ex.Message + " Failed to Parse the Swagger file " + AddAPIModelWizard.URL);
                    parseSuccess = false;
                }
            

            AddAPIModelWizard.AAMList = AAMCompletedList;
            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.AAMList;
            // AddAPIModelWizard.FinishEnabled = false;
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
                    AAMTempList = await Task.Run(() => WSDLP.ParseDocument(XTF.FilePath, AddAPIModelWizard.AvoidDuplicatesNodes));
                    if (!string.IsNullOrEmpty(XTF.MatchingResponseFilePath))
                        AAMTempList[0].ReturnValues = await Task.Run(() => APIConfigurationsDocumentParserBase.ParseResponseSampleIntoReturnValuesPerFileType(XTF.MatchingResponseFilePath)); 
                    AAMCompletedList.Add(AAMTempList[0]);
                }
                catch (Exception ex)
                {                   
                    Reporter.ToUser(eUserMsgKeys.ParsingError, "Failed to Parse the XML" + XTF.FilePath);
                    GingerCoreNET.ReporterLib.Reporter.ToLog(GingerCoreNET.ReporterLib.eLogLevel.ERROR, "Error Details: " + ex.Message + "Failed to Parse the XML" + XTF.FilePath);
                    parseSuccess = false;
                }
            }

            AddAPIModelWizard.AAMList = AAMCompletedList;
            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.AAMList;
            // AddAPIModelWizard.FinishEnabled = false;
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
                    AAMTempList = await Task.Run(() => JsonTemplate.ParseDocument(XTF.FilePath, AddAPIModelWizard.AvoidDuplicatesNodes));
                    if (!string.IsNullOrEmpty(XTF.MatchingResponseFilePath))
                        AAMTempList[0].ReturnValues = await Task.Run(() => APIConfigurationsDocumentParserBase.ParseResponseSampleIntoReturnValuesPerFileType(XTF.MatchingResponseFilePath));
                    AAMCompletedList.Add(AAMTempList[0]);
                }
                catch (Exception ex)
                {                    
                    Reporter.ToUser(eUserMsgKeys.ParsingError, "Failed to Parse the JSon" + XTF.FilePath);
                    GingerCoreNET.ReporterLib.Reporter.ToLog(GingerCoreNET.ReporterLib.eLogLevel.ERROR,"Error Details: " + ex.Message + " Failed to Parse the JSon " + XTF.FilePath);
                    parseSuccess = false;
                }
            }

            AddAPIModelWizard.AAMList = AAMCompletedList;
            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.AAMList;
            // AddAPIModelWizard.FinishEnabled = false;

            return parseSuccess;
        }

        private async Task<bool> ShowWSDLOperations()
        {
            bool parseSuccess = true;
            AddAPIModelWizard.ProcessStarted();
            ObservableList<ApplicationAPIModel> AAMSList = new ObservableList<ApplicationAPIModel>();
            try
            {
                AAMSList = await Task.Run(() => WSDLP.ParseDocument(AddAPIModelWizard.URL));
            }
            catch (Exception ex)
            {                
                Reporter.ToUser(eUserMsgKeys.ParsingError, "Failed to Parse the WSDL");
                parseSuccess = false;
            }

            AddAPIModelWizard.ProcessEnded();
            AddAPIModelWizard.AAMList = AAMSList;
            xApisSelectionGrid.DataSourceList = AddAPIModelWizard.AAMList;

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

                //if(ActiveStatus == false)
                //    AddAPIModelWizard.NextEnabled = false;
                //else
                //    AddAPIModelWizard.NextEnabled = true;

                xApisSelectionGrid.DataSourceList = lstMarkUnMarkAPI;
            }
        }

        private void SetFieldsGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationAPIModel.IsSelected), Header = "Selected", WidthWeight = 10, MaxWidth = 50, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.MainGrid.Resources["IsSelectedTemplate"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationAPIModel.Name), Header = "Name", WidthWeight = 20 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ApplicationAPIModel.Description), Header = "Description", WidthWeight = 20 });
            xApisSelectionGrid.SetAllColumnsDefaultView(view);
            xApisSelectionGrid.InitViewItems();
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
            ObservableList<ApplicationAPIModel> CheckedList = GingerCore.General.ConvertListToObservableList(AddAPIModelWizard.AAMList.Where(x => x.IsSelected == true).ToList());
            //if(CheckedList.Count > 0)
            //    AddAPIModelWizard.NextEnabled = true;
            //else
            //    AddAPIModelWizard.NextEnabled = false;
        }        
    }
}
