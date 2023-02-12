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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.APIModelLib;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerWPF.ApplicationModelsLib.APIModels;
using GingerWPF.ApplicationModelsLib.APIModels.APIModelWizard;
using GingerWPF.WizardLib;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using static Ginger.ExtensionMethods;
using static GingerWPF.ApplicationModelsLib.APIModels.APIModelWizard.AddAPIModelWizard;

namespace Ginger.ApplicationModelsLib.APIModels.APIModelWizard
{
    /// <summary>
    /// Interaction logic for AddAPIModelSelectTypePage.xaml
    /// </summary>
    public partial class AddAPIModelSelectTypePage : Page, IWizardPage
    {
        public AddAPIModelSelectTypePage()
        {
            InitializeComponent();
            GingerCore.General.FillComboFromEnumType(APITypeComboBox, typeof(eAPIType), null);
            APITypeComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            APITypeComboBox.Text = eAPIType.WSDL.ToString();
            XMLTemplatesGrid.SetTitleLightStyle = true;
            SetFieldsGrid();
        }

        private void SetFieldsGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(TemplateFile.FilePath), Header = "Request Sample File Path" });
            view.GridColsView.Add(new GridColView() { Field = nameof(TemplateFile.MatchingResponseFilePath), Header = "Matching Response File Path" });
            view.GridColsView.Add(new GridColView() { Field = "", WidthWeight = 15, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.PageGrid.Resources["MatchingResponseBrowse"] });
            XMLTemplatesGrid.SetAllColumnsDefaultView(view);
            XMLTemplatesGrid.InitViewItems();
            XMLTemplatesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddFileTemplates));
            XMLTemplatesGrid.btnClearAll.AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearAllTemplates));
            XMLTemplatesGrid.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteTemplate));
        }

        private void DeleteTemplate(object sender, RoutedEventArgs e)
        {

        }

        private void ClearAllTemplates(object sender, RoutedEventArgs e)
        {
            AddAPIModelWizard.XTFList.Clear();
        }

        private void AddFileTemplates(object sender, RoutedEventArgs e)
        {
            BrowseForTemplateFiles();
        }

        public AddAPIModelWizard AddAPIModelWizard;
        public WSDLParser mWSDLParser;



        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            if (WizardEventArgs.EventType == EventType.Init)
            {
                AddAPIModelWizard = ((AddAPIModelWizard)WizardEventArgs.Wizard);
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(XMLTemplatesGrid.AddCheckBox("Avoid Duplicates Nodes", null), CheckBox.IsCheckedProperty, AddAPIModelWizard, nameof(AddAPIModelWizard.AvoidDuplicatesNodes));
                mWSDLParser = new WSDLParser();
                AddAPIModelWizard.mWSDLParser = mWSDLParser;
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xURLTextBox, TextBox.TextProperty, AddAPIModelWizard, nameof(AddAPIModelWizard.URL));
                xURLTextBox.AddValidationRule(eValidationRule.CannotBeEmpty);
            }
            else if (WizardEventArgs.EventType == EventType.LeavingForNextPage)
            {

                if (APITypeComboBox.SelectedValue.ToString() == eAPIType.WSDL.ToString())
                {
                    AddAPIModelWizard.APIType = eAPIType.WSDL;
                    WizardEventArgs.CancelEvent = true;
                    if (!string.IsNullOrWhiteSpace(xURLTextBox.Text))
                    {
                        if (ValidateFile(xURLTextBox.Text))
                        {
                            WizardEventArgs.CancelEvent = false;
                            AddAPIModelWizard.mWSDLParser = mWSDLParser;
                        }
                    }
                }
                else if (APITypeComboBox.SelectedValue.ToString() == eAPIType.XMLTemplates.ToString())
                {
                    AddAPIModelWizard.APIType = eAPIType.XMLTemplates;
                    WizardEventArgs.CancelEvent = true;
                    if (!string.IsNullOrWhiteSpace(xURLTextBox.Text))
                    {
                        if (ValidateFile(xURLTextBox.Text))
                        {
                            WizardEventArgs.CancelEvent = false;
                        }
                    }
                    else if (XMLTemplatesGrid.DataSourceList != null && XMLTemplatesGrid.DataSourceList.Count > 0)
                    {
                        for (int i = 0; i < XMLTemplatesGrid.DataSourceList.Count; i++)
                        {
                            if (ValidateFile(((TemplateFile)XMLTemplatesGrid.DataSourceList[i]).FilePath))
                            {
                                WizardEventArgs.CancelEvent = false;
                            }
                            else
                            {
                                WizardEventArgs.CancelEvent = true;
                                break;
                            }
                        }
                    }
                }
                else if (APITypeComboBox.SelectedValue.ToString() == eAPIType.JsonTemplate.ToString())
                {
                    AddAPIModelWizard.APIType = eAPIType.JsonTemplate;
                    WizardEventArgs.CancelEvent = true;
                    if (!string.IsNullOrWhiteSpace(xURLTextBox.Text))
                    {
                        if (ValidateFile(xURLTextBox.Text))
                        {
                            WizardEventArgs.CancelEvent = false;
                        }
                    }
                    else if (XMLTemplatesGrid.DataSourceList != null && XMLTemplatesGrid.DataSourceList.Count > 0)
                    {
                        for (int i = 0; i < XMLTemplatesGrid.DataSourceList.Count; i++)
                        {
                            if (ValidateFile(((TemplateFile)XMLTemplatesGrid.DataSourceList[i]).FilePath))
                            {
                                WizardEventArgs.CancelEvent = false;
                            }
                            else
                            {
                                WizardEventArgs.CancelEvent = true;
                                break;
                            }
                        }
                    }
                }
                else if (APITypeComboBox.SelectedValue.ToString() == eAPIType.Swagger.ToString())
                {
                    AddAPIModelWizard.APIType = eAPIType.Swagger;
                    WizardEventArgs.CancelEvent = true;
                    if (!string.IsNullOrWhiteSpace(xURLTextBox.Text))
                    {
                        if (ValidateFile(xURLTextBox.Text))
                        {
                            WizardEventArgs.CancelEvent = false;
                        }
                    }
                }
            }
        }

        private void ValidateXMLTemplatesInputs(WizardEventArgs WizardEventArgs)
        {
            if (AddAPIModelWizard.XTFList.Count == 0)
            {
                //WizardEventArgs.AddError("XML Templates grid cannot be empty");
                return;
            }
            foreach (TemplateFile XTF in AddAPIModelWizard.XTFList)
            {
                string error = string.Empty;
                if (!ValidateTemplateURL(ref error))
                {
                    //  WizardEventArgs.AddError(error);
                }
            }
        }

        private bool ValidateTemplateURL(ref string error)
        {
            foreach (TemplateFile XTF in AddAPIModelWizard.XTFList)
            {
                if (!XTF.FilePath.ToUpper().EndsWith("XML"))
                {
                    error = "Please specify valid xml Template File";
                    return false;
                }
            }

            return true;
        }

        private XmlDocument GetDocumentFromWeb(string URLString)
        {
            XmlTextReader reader = new XmlTextReader(URLString);
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            return doc;
        }

        private void LoadBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string URLString = xURLTextBox.Text;
            XmlTextReader reader = new XmlTextReader(URLString);
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            XMLViewer.xmlDocument = doc;
        }

        private void APIType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (APITypeComboBox.SelectedValue.ToString() == eAPIType.WSDL.ToString() || APITypeComboBox.SelectedValue.ToString() == eAPIType.Swagger.ToString())
            {
                SecondRow.Height = new GridLength(30);
                ThirdRow.Height = new GridLength(40);
                XMLTemplatesGrid.Visibility = Visibility.Collapsed;
                xURLTextBox.IsEnabled = true;

                xURLTextBox.Text = string.Empty;

                if (URLRadioButton.IsChecked == true)
                {
                    if (APITypeComboBox.SelectedValue.ToString() == eAPIType.Swagger.ToString() || (string.IsNullOrEmpty(xURLTextBox.Text) && APITypeComboBox.SelectedValue.ToString() == eAPIType.WSDL.ToString()))
                        xBrowseLoadButton.Visibility = Visibility.Collapsed;
                    else if ((!string.IsNullOrEmpty(xURLTextBox.Text) && APITypeComboBox.SelectedValue.ToString() == eAPIType.WSDL.ToString()))
                        xBrowseLoadButton.Visibility = Visibility.Visible;
                }
                else if (FileRadioButton.IsChecked == true)
                {
                    xBrowseLoadButton.Visibility = Visibility.Visible;
                }

                XMLTemplatesLable.Visibility = Visibility.Collapsed;

            }
            else if (APITypeComboBox.SelectedValue.ToString() == eAPIType.XMLTemplates.ToString() || APITypeComboBox.SelectedValue.ToString() == eAPIType.JsonTemplate.ToString())
            {
                SecondRow.Height = new GridLength(0);
                ThirdRow.Height = new GridLength(0);
                XMLTemplatesGrid.Visibility = Visibility.Visible;
                AddAPIModelWizard.XTFList.Clear();
                XMLTemplatesGrid.DataSourceList = AddAPIModelWizard.XTFList;
                XMLTemplatesGrid.DataSourceList.CollectionChanged += XMLTemplatesGrid_CollectionChanged;
                XMLTemplatesGrid.ValidationRules.Add(ucGrid.eUcGridValidationRules.CantBeEmpty);

                xBrowseLoadButton.Visibility = Visibility.Collapsed;
                xPreviewButton.Visibility = Visibility.Collapsed;
                XMLTemplatesLable.Visibility = Visibility.Visible;
                BrowseButtonClicked(new object(), new RoutedEventArgs());

                xURLTextBox.ClearValidations(TextBox.TextProperty);
            }
            xPreviewButton.Visibility = Visibility.Collapsed;
            SourceRviewLable.Visibility = Visibility.Collapsed;

            if (AddAPIModelWizard != null)
            {
                AddAPIModelWizard.IsParsingWasDone = false;
                CleanLearnedAPIModels();
            }
        }

        void CleanLearnedAPIModels()
        {
            if (AddAPIModelWizard.LearnedAPIModelsList != null)
            {
                AddAPIModelWizard.LearnedAPIModelsList.Clear();
            }
            if (AddAPIModelWizard.DeltaModelsList != null)
            {
                AddAPIModelWizard.DeltaModelsList.Clear();
            }
        }

        private void XMLTemplatesGrid_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AddAPIModelWizard.IsParsingWasDone = false;
        }

        private void URLRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            if (URLLable != null)
                URLLable.Content = "URL";
            if (xBrowseLoadButton != null)
            {
                xBrowseLoadButton.ButtonText = "Load";
                xBrowseLoadButton.Visibility = Visibility.Collapsed;
            }
            if (xPreviewButton != null)
            {
                xPreviewButton.Visibility = Visibility.Collapsed;
            }
            if (XMLViewer != null)
            {
                XMLViewer.xmlDocument = null;
            }

            if (xURLTextBox != null)
                xURLTextBox.Text = string.Empty;
        }

        private void FileRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            if (URLLable != null)
                URLLable.Content = "File";
            if (xBrowseLoadButton != null)
            {
                xBrowseLoadButton.ButtonText = "Browse";
                xBrowseLoadButton.Visibility = Visibility.Visible;
            }
            if (xPreviewButton != null)
            {
                xPreviewButton.Visibility = Visibility.Collapsed;
            }
            if (XMLViewer != null)
            {
                XMLViewer.xmlDocument = null;
            }
            xURLTextBox.Text = string.Empty;
        }

        private void URLTextBoxTextChange(object sender, TextChangedEventArgs e)
        {
            xPreviewButton.Visibility = Visibility.Collapsed;
            SourceRviewLable.Visibility = Visibility.Collapsed;
            XMLViewer.Visibility = Visibility.Collapsed;
            XMLViewer.xmlDocument = new XmlDocument();

            ///To relearn the APIs once we move next.
            AddAPIModelWizard.IsParsingWasDone = false;

            CleanLearnedAPIModels();

            if (URLRadioButton.IsChecked == true)
            {
                if (!string.IsNullOrEmpty(xURLTextBox.Text) && APITypeComboBox.SelectedValue.ToString() != eAPIType.Swagger.ToString())
                {
                    xBrowseLoadButton.Visibility = Visibility.Visible;
                }
                else
                {
                    xBrowseLoadButton.Visibility = Visibility.Collapsed;
                }
            }
            else if (FileRadioButton.IsChecked == true)
            {
                if (!string.IsNullOrEmpty(xURLTextBox.Text) && APITypeComboBox.SelectedValue.ToString() == eAPIType.WSDL.ToString())
                {
                    xBrowseLoadButton.ButtonText = "Load";
                }
                else
                {
                    xBrowseLoadButton.ButtonText = "Browse";
                }
            }
        }

        private void BrowseButtonClicked(object sender, RoutedEventArgs e)
        {
            AddAPIModelWizard.IsParsingWasDone = false;
            if (APITypeComboBox.SelectedValue.ToString() == eAPIType.WSDL.ToString())
            {
                if (FileRadioButton.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(xURLTextBox.Text))
                    {
                        if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
                        {
                            Filter = "WSDL Files (*.wsdl)|*.wsdl" + "|XML Files (*.xml)|*.xml" + "|All Files (*.*)|*.*"
                        }, false) is string fileName)
                        {
                            xURLTextBox.Text = fileName;
                            ValidateFile(fileName);
                        }
                    }
                    else
                    {
                        ValidateFile();
                    }

                }
                else
                {
                    ValidateFile();
                }
            }
            else if (APITypeComboBox.SelectedValue.ToString() == eAPIType.Swagger.ToString())
            {
                AddAPIModelWizard.APIType = eAPIType.Swagger;
                if (FileRadioButton.IsChecked == true)
                {
                    System.Windows.Forms.OpenFileDialog dlg2 = new System.Windows.Forms.OpenFileDialog();

                    dlg2.Filter = "JSON Files (*.json)|*.json" + "|YAML Files (*.yaml)|*.yaml;*.yml" + "|All Files (*.*)|*.*";

                    System.Windows.Forms.DialogResult result = dlg2.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        if (ValidateFile(dlg2.FileName))
                        {
                            xURLTextBox.Text = dlg2.FileName;
                            AddAPIModelWizard.XTFList.Add(new TemplateFile() { FilePath = dlg2.FileName });
                        }
                    }
                }
                else
                {
                    string tempfile = System.IO.Path.GetTempFileName();
                    string filecontent = Amdocs.Ginger.Common.GeneralLib.HttpUtilities.Download(new System.Uri(xURLTextBox.Text));
                    System.IO.File.WriteAllText(tempfile, filecontent);
                    if (ValidateFile(tempfile))
                    {
                        AddAPIModelWizard.XTFList.Add(new TemplateFile() { FilePath = tempfile });
                    }
                }
            }

            else
            {
                BrowseForTemplateFiles();
            }
        }

        private void BrowseForTemplateFiles(string files = null)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Multiselect = true;

            dlg.Filter = GetRelevantFilter();

            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String file in dlg.FileNames)
                {
                    if (ValidateFile(file))
                    {
                        AddAPIModelWizard.XTFList.Add(new TemplateFile() { FilePath = file });
                    }
                }
            }
        }

        private bool ValidateFile(string fileName = "")
        {
            bool bIsFileValid = false;
            try
            {
                if (APITypeComboBox.SelectedValue.ToString() == eAPIType.WSDL.ToString())
                {
                    fileName = xURLTextBox.Text;
                    bIsFileValid = LoadWSDLFileValidation();
                }
                else if (APITypeComboBox.SelectedValue.ToString() == eAPIType.Swagger.ToString())
                {
                    bIsFileValid = CheckForSwaggerParser(fileName);
                }
                else if (APITypeComboBox.SelectedValue.ToString() == eAPIType.JsonTemplate.ToString())
                {
                    bIsFileValid = CheckForJsonParser(fileName);

                }
                else if (APITypeComboBox.SelectedValue.ToString() == eAPIType.XMLTemplates.ToString())
                {
                    bIsFileValid = CheckForXmlParser(fileName);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, ex.Message, ex);
                bIsFileValid = false;
            }
            finally
            {
                if (!bIsFileValid)
                {
                    CheckForValidParser(fileName);
                }
            }
            return bIsFileValid;
        }

        private void CheckForValidParser(string fileName = "")
        {
            if (LoadWSDLFileValidation(bShowMessage: false))
            {
                Reporter.ToUser(eUserMsgKey.FileOperationError, "Please use WSDL for this file");
            }
            else if (CheckForXmlParser(fileName))
            {
                Reporter.ToUser(eUserMsgKey.FileOperationError, "Please use XML for this file");
            }
            else if (CheckForSwaggerParser(fileName))
            {
                Reporter.ToUser(eUserMsgKey.FileOperationError, "Please use Swagger for this file");
            }
            else if (CheckForJsonParser(fileName))
            {
                Reporter.ToUser(eUserMsgKey.FileOperationError, "Please use JSON for this file");
            }
        }

        private bool CheckForXmlParser(string fileName)
        {
            try
            {
                XMLTemplateParser xmlParser = new XMLTemplateParser();
                ObservableList<ApplicationAPIModel> xmlList = new ObservableList<ApplicationAPIModel>();
                xmlList = xmlParser.ParseDocument(fileName, xmlList);
                if (xmlList == null || xmlList.Count == 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, ex.Message, ex);
                return false;
            }
        }

        private bool CheckForSwaggerParser(string fileName)
        {
            try
            {
                SwaggerParser swaggerParser = new SwaggerParser();
                ObservableList<ApplicationAPIModel> swaggerList = new ObservableList<ApplicationAPIModel>();
                swaggerList = swaggerParser.ParseDocument(fileName, swaggerList);
                if (swaggerList == null || swaggerList.Count == 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, ex.Message, ex);
                return false;
            }
        }

        private bool CheckForJsonParser(string fileName)
        {
            try
            {
                JSONTemplateParser jsonParser = new JSONTemplateParser();
                ObservableList<ApplicationAPIModel> jsonList = new ObservableList<ApplicationAPIModel>();
                jsonList = jsonParser.ParseDocument(fileName, jsonList);
                if (jsonList == null || jsonList.Count == 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, ex.Message, ex);
                return false;
            }
        }

        private bool LoadWSDLFileValidation(bool bShowMessage = true)
        {
            AddAPIModelWizard.ProcessStarted();
            xBrowseLoadButton.IsEnabled = false;
            string error = string.Empty;
            mWSDLParser = new WSDLParser();

            try
            {
                if (!mWSDLParser.ValidateWSDLURL(xURLTextBox.Text, URLRadioButton.IsChecked, ref error))
                {
                    throw new Exception(error);
                }

                XmlDocument doc = null;
                string s = xURLTextBox.Text;

                doc = GetDocumentFromWeb(s);
                PreviewContent = doc;
                xPreviewButton.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                if (bShowMessage)
                {
                    Reporter.ToUser(eUserMsgKey.FileOperationError, ex.Message);
                }
                error = error + Environment.NewLine + ex.Message;
                xPreviewButton.Visibility = Visibility.Collapsed;
                SourceRviewLable.Visibility = Visibility.Collapsed;
                XMLViewer.Visibility = Visibility.Collapsed;
                return false;
            }
            finally
            {
                AddAPIModelWizard.ProcessEnded();
                xBrowseLoadButton.IsEnabled = true;
            }
            if (string.IsNullOrEmpty(error))
            {
                if (bShowMessage)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Success : The File Loaded successfully");
                }
                return true;
            }
            return false;
        }

        private XmlDocument PreviewContent = null;

        private void PreviewButtonClicked(object sender, RoutedEventArgs e)
        {
            SourceRviewLable.Visibility = Visibility.Visible;
            XMLViewer.Visibility = Visibility.Visible;
            ShowPreview();
        }

        private async void ShowPreview()
        {
            xPreviewButton.IsEnabled = false;
            XmlDocument doc = null;
            string s = xURLTextBox.Text;
            await Task.Run(() => doc = GetDocumentFromWeb(s));
            XMLViewer.xmlDocument = doc;
            xPreviewButton.IsEnabled = true;
        }

        private void MatchingResponseBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                Filter = GetRelevantFilter()
            }, false) is string fileName)
            {
                TemplateFile selectedTampleteFile = (TemplateFile)XMLTemplatesGrid.CurrentItem;
                selectedTampleteFile.MatchingResponseFilePath = fileName;
            }
        }

        private string GetRelevantFilter()
        {
            if (APITypeComboBox.SelectedValue.ToString() == eAPIType.XMLTemplates.ToString())
            {

                return "XML Files (*.xml)|*.xml" + "|WSDL Files (*.wsdl)|*.wsdl" + "|All Files (*.*)|*.*";
            }
            else if (APITypeComboBox.SelectedValue.ToString() == eAPIType.JsonTemplate.ToString())
            {
                return "JSON Files (*.json)|*.json" + "|TXT Files (*.txt)|*.txt" + "|All Files (*.*)|*.*";
            }

            return string.Empty;
        }
    }

}
