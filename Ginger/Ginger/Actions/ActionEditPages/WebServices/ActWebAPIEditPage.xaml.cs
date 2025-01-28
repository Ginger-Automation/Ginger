#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Ginger.UserControls;
using Ginger.UserControlsLib.TextEditor;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.WebAPI;
using GingerCore.Actions.WebServices;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.WebServices
{
    /// <summary>
    /// Interaction logic for ActWebAPIEditPage.xaml
    /// </summary>
    public partial class ActWebAPIEditPage : Page
    {
        private const string webServicesCertificatePath = @"Documents\WebServices\Certificates";
        ActWebAPIBase mAct;
        ApplicationAPIUtils.eWebApiType mWebApiType;

        public ActWebAPIEditPage(ActWebAPIBase act)
        {
            mAct = act;
            if (act.GetType() == typeof(ActWebAPIRest))
            {
                mWebApiType = ApplicationAPIUtils.eWebApiType.REST;
            }
            else
            {
                mWebApiType = ApplicationAPIUtils.eWebApiType.SOAP;
            }

            InitializeComponent();
            BindUiControls();
            InitializeUIByActionType();

            CertificatePath.ValueTextBox.LostFocus += ValueTextBox_LostFocus;
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            BrowseSSLCertificate(sender, e);
        }

        private void InitializeUIByActionType()
        {
            switch (mWebApiType)
            {
                case ApplicationAPIUtils.eWebApiType.REST:
                    //binding
                    //setting visibility :
                    PanelSoap.Visibility = Visibility.Collapsed;
                    UseWSSecurityHeader.Visibility = Visibility.Collapsed;
                    //Cookie Mode
                    CookieMode.Init(mAct.GetOrCreateInputParam(ActWebAPIRest.Fields.CookieMode, ApplicationAPIUtils.eCookieMode.Session.ToString()), typeof(ApplicationAPIUtils.eCookieMode), false, null);

                    //Request Type
                    RequestTypeComboBox.Init(mAct.GetOrCreateInputParam(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString()), typeof(ApplicationAPIUtils.eRequestType), false, null);

                    //HttpVersion content type
                    HttpVersioncombobox.Init(mAct.GetOrCreateInputParam(ActWebAPIRest.Fields.ReqHttpVersion, ApplicationAPIUtils.eHttpVersion.HTTPV11.ToString()), typeof(ApplicationAPIUtils.eHttpVersion), false, null);

                    //Request content type
                    ContentTypeComboBox.Init(mAct.GetOrCreateInputParam(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eRequestContentType.JSon.ToString()), typeof(ApplicationAPIUtils.eRequestContentType), false, ContentTypeChange);

                    //Response Content Type
                    ResponseTypeComboBox.Init(mAct.GetOrCreateInputParam(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eResponseContentType.JSon.ToString()), typeof(ApplicationAPIUtils.eResponseContentType), false, ResponseTypeComboBox_SelectionChanged);
                    //ResponseTypeComboBox.Init(mAct.GetOrCreateInputParam(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString()),
                    //    GetFilteredContentTypes(), 
                    //    false, ResponseTypeComboBox_SelectionChanged);

                    //Request Template file:
                    TemplateFileNameFileBrowser.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.TemplateFileNameFileBrowser), true, true, UCValueExpression.eBrowserType.File, "txt; *.xml; *.json;", new RoutedEventHandler(BrowseTemplateFileButton_Click));
                    break;

                case ApplicationAPIUtils.eWebApiType.SOAP:
                    //binding visibility
                    RequestContent.Visibility = Visibility.Collapsed;
                    RestHeader.Visibility = Visibility.Collapsed;
                    UseWSSecurityHeader.Visibility = Visibility.Visible;
                    //binding
                    //SOAP Action
                    SoapActionUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActWebAPISoap.Fields.SOAPAction), true, false, UCValueExpression.eBrowserType.Folder);

                    //Request Template file:
                    TemplateFileNameFileBrowser.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.TemplateFileNameFileBrowser), true, true, UCValueExpression.eBrowserType.File, "txt; *.xml;", new RoutedEventHandler(BrowseTemplateFileButton_Click));
                    break;
            }
        }

        public void BindUiControls()
        {
            //URL fields:

            URLUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.EndPointURL), true, false, UCValueExpression.eBrowserType.Folder);
            URLUserUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.URLUser), true, false, UCValueExpression.eBrowserType.Folder);
            URLDomainUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.URLDomain), true, false, UCValueExpression.eBrowserType.Folder);
            URLPasswordUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.URLPass), true, false, UCValueExpression.eBrowserType.Folder);

            //Network Credential selection radio button:
            NetworkCredentialsRadioButton.Init(typeof(ApplicationAPIUtils.eNetworkCredentials), NetworkCeredentials, mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.NetworkCredentialsRadioButton, ApplicationAPIUtils.eNetworkCredentials.Default.ToString()), NetworkCreds_SelectionChanged);

            //Request Body Selection radio button:
            RequestBodyTypeRadioButton.Init(typeof(ApplicationAPIUtils.eRequestBodyType), BodySelection, mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.FreeText.ToString()), RequestBodyType_Selection);

            //CertficiateRadioButtons :
            CertificateTypeRadioButton.Init(typeof(ApplicationAPIUtils.eCretificateType), CertificateSelection, mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString()), CertificateSelection_Changed);

            //Response validation checkbox: 
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(DoNotFailActionOnBadRespose, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.DoNotFailActionOnBadRespose, "False"));

            //Use Legacy JSON Parsing
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(UseLegacyJSONParsingCheckBox, CheckBox.IsCheckedProperty, mAct, ActWebAPIBase.Fields.UseLegacyJSONParsing);

            //Request Body fields:
            RequestBodyUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.RequestBody), true, false, UCValueExpression.eBrowserType.Folder, "*", null);
            RequestBodyUCValueExpression.AdjustHight(200);
            RequestBodyUCValueExpression.ValueTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            //Import Request File
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(DoNotImportRequestFile, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.ImportRequestFile, "False"));

            //SSL Certificates:
            CertificatePath.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.CertificatePath), true, true, UCValueExpression.eBrowserType.File, "*.*", new RoutedEventHandler(BrowseSSLCertificate));
            CertificatePasswordUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.CertificatePassword), true, false, UCValueExpression.eBrowserType.Folder);

            //Import Certificate
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(DoNotCertificateImportFile, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.ImportCetificateFile, "False"));

            //Security:
            SecurityTypeComboBox.Init(mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.SecurityType, ApplicationAPIUtils.eSercurityType.None.ToString()), typeof(ApplicationAPIUtils.eSercurityType), false, null);

            //Authorization:
            AuthTypeComboBox.Init(mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.AuthorizationType, ApplicationAPIUtils.eAuthType.NoAuthentication.ToString()), typeof(ApplicationAPIUtils.eAuthType), false, AuthorizationBox);

            //Authorization
            AuthUserUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.AuthUsername), true, false, UCValueExpression.eBrowserType.Folder);
            AuthPasswordUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.GetOrCreateInputParam(ActWebAPIBase.Fields.AuthPassword), true, false, UCValueExpression.eBrowserType.Folder);

            DynamicElementsGrid.Init(Context.GetAsContext(mAct.Context), mAct.DynamicElements, "Body Content Parameters", "Place Holder", "Value", "Calculated Value");

            HttpHeadersGrid.Init(Context.GetAsContext(mAct.Context), mAct.HttpHeaders, "Request Headers", "Header", "Value", "Calculated Value");

            //FormDataGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddRow));
            SetKeyValuesGrid(mAct.RequestKeyValues);

            CheckNetworkCredentials();
            CheckRequestBodySelection();
            CheckCertificateSelection();
        }

        private void CheckNetworkCredentials()
        {
            if (mAct.GetInputParamValue(ActWebAPIBase.Fields.NetworkCredentialsRadioButton) == ApplicationAPIUtils.eNetworkCredentials.Custom.ToString())
            {
                SP_CustomCreds.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                SP_CustomCreds.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void CheckRequestBodySelection()
        {

            if ((mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eRequestContentType.XwwwFormUrlEncoded.ToString()))
            {
                FreeTextStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                FormDataGridPanel.Visibility = System.Windows.Visibility.Visible;
                DynamicElementGridPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eRequestContentType.FormData.ToString())
            {
                FreeTextStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                FormDataGridPanel.Visibility = System.Windows.Visibility.Visible;
                DynamicElementGridPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                if (mAct.GetInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton) == ApplicationAPIUtils.eRequestBodyType.FreeText.ToString())
                {
                    FreeTextStackPanel.Visibility = System.Windows.Visibility.Visible;
                    TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                    FormDataGridPanel.Visibility = System.Windows.Visibility.Collapsed;
                    DynamicElementGridPanel.Visibility = System.Windows.Visibility.Visible;
                }
                if (mAct.GetInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton) == ApplicationAPIUtils.eRequestBodyType.TemplateFile.ToString())
                {
                    FreeTextStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    TemplateStackPanel.Visibility = System.Windows.Visibility.Visible;
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                    FormDataGridPanel.Visibility = System.Windows.Visibility.Collapsed;
                    DynamicElementGridPanel.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private void CheckCertificateSelection()
        {
            if (mAct.GetInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton) == ApplicationAPIUtils.eCretificateType.Custom.ToString())
            {
                CertificateStackPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                CertificateStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void NetworkCreds_SelectionChanged(object sender, RoutedEventArgs e)
        {
            mAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.NetworkCredentialsRadioButton, (((RadioButton)sender).Tag).ToString());
            if (mAct.GetInputParamValue(ActWebAPIBase.Fields.NetworkCredentialsRadioButton) == ApplicationAPIUtils.eNetworkCredentials.Default.ToString())
            {
                SP_CustomCreds.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (mAct.GetInputParamValue(ActWebAPIBase.Fields.NetworkCredentialsRadioButton) == ApplicationAPIUtils.eNetworkCredentials.Custom.ToString())
            {
                SP_CustomCreds.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void RequestBodyType_Selection(object sender, RoutedEventArgs e)
        {
            mAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, (((RadioButton)sender).Tag).ToString());
            if ((mAct.GetInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton)) == ApplicationAPIUtils.eRequestBodyType.FreeText.ToString())
            {
                FreeTextStackPanel.Visibility = System.Windows.Visibility.Visible;
                if (!String.IsNullOrEmpty((mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser))))
                {
                    TemplateFileNameFileBrowser.ValueTextBox.Text = string.Empty;
                }

                TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                BodyInputGridPannel.Visibility = System.Windows.Visibility.Collapsed;
                FreeTextStackPanel.Visibility = Visibility.Visible;
            }
            if ((mAct.GetInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton)) == ApplicationAPIUtils.eRequestBodyType.TemplateFile.ToString())
            {
                TemplateStackPanel.Visibility = System.Windows.Visibility.Visible;
                if (!String.IsNullOrEmpty((mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser))))
                {
                    RequestBodyUCValueExpression.ValueTextBox.Text = string.Empty;
                }

                FreeTextStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                BodyInputGridPannel.Visibility = System.Windows.Visibility.Collapsed;
                FreeTextStackPanel.Visibility = Visibility.Collapsed;
            }
            else if ((mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eRequestContentType.XwwwFormUrlEncoded.ToString()) || (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eRequestContentType.FormData.ToString()))
            {
                if (!String.IsNullOrEmpty((mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser))))
                {
                    RequestBodyUCValueExpression.ValueTextBox.Text = string.Empty;
                }

                if (!String.IsNullOrEmpty((mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser))))
                {
                    TemplateFileNameFileBrowser.ValueTextBox.Text = string.Empty;
                }

                FreeTextStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                BodyInputGridPannel.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void BrowseTemplateFileButton_Click(object sender, RoutedEventArgs e)
        {
            string SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
            if (TemplateFileNameFileBrowser.ValueTextBox.Text != null)
            {
                // replace Absolute file name with relative to solution
                string FileName = TemplateFileNameFileBrowser.ValueTextBox.Text.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }

                TemplateFileNameFileBrowser.ValueTextBox.Text = FileName;

                bool ImportFileFlag = false;
                Boolean.TryParse(mAct.GetInputParamValue(ActWebAPIBase.Fields.ImportRequestFile), out ImportFileFlag);
                if (ImportFileFlag)
                {
                    //TODO import request File
                    string targetPath = System.IO.Path.Combine(SolutionFolder, @"Documents\WebServices\RequestFile");
                    if (!System.IO.Directory.Exists(targetPath))
                    {
                        System.IO.Directory.CreateDirectory(targetPath);
                    }
                    string destFile = System.IO.Path.Combine(targetPath, System.IO.Path.GetFileName(FileName));
                    int fileNum = 1;
                    string copySufix = "_Copy";
                    while (System.IO.File.Exists(destFile))
                    {
                        fileNum++;
                        string newFileName = System.IO.Path.GetFileNameWithoutExtension(destFile);
                        if (newFileName.IndexOf(copySufix) != -1)
                        {
                            newFileName = newFileName[..newFileName.IndexOf(copySufix)];
                        }

                        newFileName = newFileName + copySufix + fileNum.ToString() + System.IO.Path.GetExtension(destFile);
                        destFile = System.IO.Path.Combine(targetPath, newFileName);
                    }
                    System.IO.File.Copy(FileName, destFile, true);

                    TemplateFileNameFileBrowser.ValueTextBox.Text = @"~\Documents\WebServices\RequestFile\" + System.IO.Path.GetFileName(destFile);
                }

            }
        }

        private void BodyExpanded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(RequestBodyUCValueExpression.ValueTextBox.Text) || (!string.IsNullOrEmpty(TemplateFileNameFileBrowser.ValueTextBox.Text)))
            {
                BodyExpander.IsExpanded = true;
            }
        }

        private void BodyCollapsed(object sender, RoutedEventArgs e)
        {
            BodyExpander.IsExpanded = false;
        }

        private void SecExpanded(object sender, RoutedEventArgs e)
        {
            if ((!string.IsNullOrEmpty(CertificatePath.ValueTextBox.Text)) || (!string.IsNullOrEmpty(CertificatePasswordUCValueExpression.ValueTextBox.Text)))
            {
                SecurityExpander.IsExpanded = true;
            }
        }

        private void SecCollapsed(object sender, RoutedEventArgs e)
        {
            SecurityExpander.IsExpanded = false;
        }

        private void BrowseSSLCertificate(object sender, RoutedEventArgs e)
        {
            if (CertificatePath.ValueTextBox.Text != null)
            {
                string certFilePath = CertificatePath.ValueTextBox.Text.Replace(@"~\", WorkSpace.Instance.Solution.Folder, StringComparison.InvariantCultureIgnoreCase);

                if (IsToImportCertificateFile() && !certFilePath.Contains(webServicesCertificatePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    string targetDirPath = Path.Combine(WorkSpace.Instance.Solution.Folder, webServicesCertificatePath);
                    string destFilePath = GetUniqueFilePath(Path.Combine(targetDirPath, Path.GetFileName(certFilePath)));

                    if (!Directory.Exists(targetDirPath))
                    {
                        Directory.CreateDirectory(targetDirPath);
                    }

                    File.Copy(certFilePath, destFilePath, true);
                    certFilePath = destFilePath;
                }

                CertificatePath.ValueTextBox.Text = certFilePath.Replace(WorkSpace.Instance.Solution.Folder, @"~\", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private bool IsToImportCertificateFile()
        {
            bool.TryParse(mAct.GetInputParamValue(ActWebAPIBase.Fields.ImportCetificateFile), out var importFileFlag);
            return importFileFlag;
        }

        private static string GetUniqueFilePath(string destinationFilePath)
        {
            int fileNum = 1;
            string copySufix = "_copy";
            string targetDirPath = Path.GetDirectoryName(destinationFilePath);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(destinationFilePath);
            while (File.Exists(destinationFilePath))
            {
                fileNum++;
                if (fileNameWithoutExt.Contains(copySufix, StringComparison.CurrentCulture))
                {
                    fileNameWithoutExt = fileNameWithoutExt[..fileNameWithoutExt.IndexOf(copySufix)];
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(fileNameWithoutExt);
                sb.Append(copySufix);
                sb.Append(fileNum);
                sb.Append(Path.GetExtension(destinationFilePath));

                fileNameWithoutExt = sb.ToString();
                destinationFilePath = Path.Combine(targetDirPath, fileNameWithoutExt);
            }

            return destinationFilePath;
        }

        private void DoNotCertificateImportFile_Checked(object sender, RoutedEventArgs e)
        {
            if (IsToImportCertificateFile() && ((CheckBox)sender).IsLoaded)
            {
                BrowseSSLCertificate(sender, e);
            }
        }

        private void CertificateSelection_Changed(object sender, RoutedEventArgs e)
        {
            mAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, (((RadioButton)sender).Tag).ToString());
            if ((mAct.GetInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton)) == ApplicationAPIUtils.eCretificateType.AllSSL.ToString())
            {
                CertificateStackPanel.Visibility = Visibility.Collapsed;
            }
            else if ((mAct.GetInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton)) == ApplicationAPIUtils.eCretificateType.Custom.ToString())
            {
                CertificateStackPanel.Visibility = Visibility.Visible;
            }
        }

        private void ContentTypeChange(object sender, RoutedEventArgs e)
        {
            switch (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType))
            {
                case "JSon":
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                    DynamicElementGridPanel.Visibility = System.Windows.Visibility.Visible;
                    CheckRequestBodySelection();
                    break;
                case "TextPlain":
                    //hide grid
                    //show textbox/filepath
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                    DynamicElementGridPanel.Visibility = System.Windows.Visibility.Visible;
                    CheckRequestBodySelection();
                    break;
                case "XML":
                    //hide grid
                    //show textbox/filepath
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                    DynamicElementGridPanel.Visibility = System.Windows.Visibility.Visible;
                    CheckRequestBodySelection();
                    break;
                case "XwwwFormUrlEncoded":
                    //hide  textbox/filepath
                    //show grid
                    BodyInputGridPannel.Visibility = System.Windows.Visibility.Collapsed;
                    FreeTextStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                    FormDataGridPanel.Visibility = System.Windows.Visibility.Visible;
                    DynamicElementGridPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RefreshRequestKeyValuesGrid();
                    break;
                case "FormData":
                    //hide  textbox/filepath
                    //show grid
                    BodyInputGridPannel.Visibility = System.Windows.Visibility.Collapsed;
                    FreeTextStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                    FormDataGridPanel.Visibility = System.Windows.Visibility.Visible;
                    DynamicElementGridPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RefreshRequestKeyValuesGrid();
                    break;
            }
        }

        public void RefreshRequestKeyValuesGrid()
        {
            //Clearing values only if page is loaded to avoid clearing values when the pages is being loaded
            if (this.IsLoaded)
            {
                if (mAct.RequestKeyValues != null)
                {
                    mAct.RequestKeyValues.ClearAll();
                }

                if (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eRequestContentType.XwwwFormUrlEncoded.ToString())
                {
                    //switch combobox   & browse button off 
                    FormDataGrid.ChangeGridView("UrlEncoded");
                }
                else if (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eRequestContentType.FormData.ToString())
                {
                    //switch combobox  & browse button on
                    FormDataGrid.ChangeGridView("FormData");
                }
            }
        }

        private void AuthorizationBox(object sender, RoutedEventArgs e)
        {
            if (mAct.GetInputParamValue(ActWebAPIBase.Fields.AuthorizationType) == ApplicationAPIUtils.eAuthType.BasicAuthentication.ToString())
            {
                Auth_Creds.Visibility = Visibility.Visible;
            }
            else if (mAct.GetInputParamValue(ActWebAPIBase.Fields.AuthorizationType) != ApplicationAPIUtils.eAuthType.BasicAuthentication.ToString())
            {
                Auth_Creds.Visibility = Visibility.Collapsed;
            }
        }

        public void SetKeyValuesGrid(ObservableList<WebAPIKeyBodyValues> RequestKeyValues)
        {
            FormDataGrid.Title = "Request Key Values";
            FormDataGrid.SetTitleStyle((Style)TryFindResource("@ucGridTitleLightStyle"));

            //View with Browse and Combobox -->Form Data 
            GridViewDef FormDataView = new GridViewDef("FormData")
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ActInputValue.Param), Header = "Key", WidthWeight = 100 },
            ]
            };
            List<ComboEnumItem> valueTypes = GingerCore.General.GetEnumValuesForCombo(typeof(WebAPIKeyBodyValues.eValueType));
            FormDataView.GridColsView.Add(new GridColView() { Field = nameof(WebAPIKeyBodyValues.ValueType), Header = "Value Type", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = valueTypes });
            FormDataView.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), Header = "Value/File Path", WidthWeight = 100 });
            FormDataView.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.controlGrid.Resources["VEGridValueExpressionButton"] });
            FormDataView.GridColsView.Add(new GridColView() { Field = "Browse", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.controlGrid.Resources["BrowseValueFilesButton"] });

            //Define URLEncoded GridView
            GridViewDef UrlEncodedView = new GridViewDef("UrlEncoded")
            {
                GridColsView = []
            };
            ObservableList<GridColView> UrlViewCols = [];
            UrlEncodedView.GridColsView.Add(new GridColView() { Field = nameof(WebAPIKeyBodyValues.ValueType), Visible = false });
            UrlEncodedView.GridColsView.Add(new GridColView() { Field = "Browse", Visible = false });

            FormDataGrid.SetAllColumnsDefaultView(FormDataView);
            FormDataGrid.AddCustomView(UrlEncodedView);
            FormDataGrid.InitViewItems();
            FormDataGrid.DataSourceList = RequestKeyValues;

            FormDataGrid.ShowRefresh = Visibility.Collapsed;
            FormDataGrid.ShowUpDown = Visibility.Collapsed;
            FormDataGrid.ShowEdit = Visibility.Collapsed;
            FormDataGrid.ShowViewCombo = Visibility.Collapsed;

            FormDataGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddRow));

            if (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eRequestContentType.XwwwFormUrlEncoded.ToString())
            {
                //switch combobox   & browse button off 
                FormDataGrid.ChangeGridView("UrlEncoded");
            }
            else if (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eRequestContentType.FormData.ToString())
            {
                //switch combobox  & browse button on
                FormDataGrid.ChangeGridView("FormData");
            }
        }

        private void VEGridInputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)FormDataGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }

        private void GridInputGridBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            WebAPIKeyBodyValues item = (WebAPIKeyBodyValues)FormDataGrid.CurrentItem;

            string FileName = string.Empty;

            if (item.ValueType == WebAPIKeyBodyValues.eValueType.File)
            {
                if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
                {
                    DefaultExt = "*.*",
                    Filter = "All files (All Files)|*.*"
                }) is string fileName)
                {
                    item.Value = fileName;
                    FormDataGrid.DataSourceList.CurrentItem = item;
                }
            }
        }

        private void AddRow(object sender, RoutedEventArgs e)
        {
            WebAPIKeyBodyValues Wa = new WebAPIKeyBodyValues
            {
                ValueType = WebAPIKeyBodyValues.eValueType.Text
            };
            mAct.RequestKeyValues.Add(Wa);
        }
        private void ResponseTypeComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (mAct.GetInputParamValue(ActWebAPIRest.Fields.ResponseContentType) == ApplicationAPIUtils.eResponseContentType.JSon.ToString())
            {
                JSON.Visibility = Visibility.Visible;
            }
            else
            {
                JSON.Visibility = Visibility.Collapsed;
            }
        }

        private void ReplaceWSSecurityHeader_Click(object sender, RoutedEventArgs e)
        {
            string txtBoxBodyContent = RequestBodyUCValueExpression.ValueTextBox.Text;
            List<string> SoapSecurityContent = HttpWebClientUtils.GetSoapSecurityHeaderContent(ref txtBoxBodyContent);

            RequestBodyUCValueExpression.ValueTextBox.Text = txtBoxBodyContent;
            if (string.IsNullOrEmpty(txtBoxBodyContent))
            {
                return;
            }
            for (int i = 1; i < SoapSecurityContent.Count; i++)
            {
                if (mAct.DynamicElements.Count == 0)
                {
                    mAct.DynamicElements.Add(new ActInputValue() { Param = SoapSecurityContent.ElementAt(i) });
                }

                else
                {
                    if (!mAct.DynamicElements.Any(x => x.Param.Equals(SoapSecurityContent.ElementAt(i))))
                    {
                        mAct.DynamicElements.Add(new ActInputValue() { Param = SoapSecurityContent.ElementAt(i) });
                    }
                }
            }
        }

        private void xViewRawRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            HttpWebClientUtils webAPIUtils = new HttpWebClientUtils();
            string requestContent = webAPIUtils.GetRawRequestContentPreview(mAct);
            if (requestContent != string.Empty)
            {
                string tempFilePath = GingerCoreNET.GeneralLib.General.CreateTempTextFile(requestContent);
                if (System.IO.File.Exists(tempFilePath))
                {
                    DocumentEditorPage docPage = new DocumentEditorPage(tempFilePath, enableEdit: false, UCTextEditorTitle: string.Empty)
                    {
                        Width = 800,
                        Height = 800
                    };
                    docPage.ShowAsWindow("Raw Request Preview");
                    System.IO.File.Delete(tempFilePath);
                    return;
                }
            }
            Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to load raw request preview, see log for details.");
        }

        /// <summary>
        /// Handles the LostKeyboardFocus event for the CertificatePasswordUCValueExpression control.
        /// Encrypts the password if needed.
        /// </summary>
        private void CertificatePasswordUCValueExpression_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            EncryptPasswordIfNeeded();
        }

        /// <summary>
        /// Determines if the password is a value expression.
        /// </summary>
        /// <returns>True if the password is a value expression; otherwise, false.</returns>
        private bool IsPasswordValueExpression()
        {
            return ValueExpression.IsThisAValueExpression(mAct.GetInputParamValue(ActWebAPIBase.Fields.CertificatePassword));
        }

        /// <summary>
        /// Encrypts the password if it is not already encrypted and not a value expression.
        /// </summary>
        private void EncryptPasswordIfNeeded()
        {
            string password = mAct.GetInputParamValue(ActWebAPIBase.Fields.CertificatePassword);
            if (!string.IsNullOrEmpty(password) && !IsPasswordValueExpression() && !EncryptionHandler.IsStringEncrypted(password))
            {
                CertificatePasswordUCValueExpression.ValueTextBox.Text = EncryptionHandler.EncryptwithKey(password);
            }
        }
    }
}