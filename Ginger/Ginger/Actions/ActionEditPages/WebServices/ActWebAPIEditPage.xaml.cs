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
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore.Actions;
using GingerCore.Actions.WebAPI;
using GingerCore.Actions.WebServices;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.WebServices
{
    /// <summary>
    /// Interaction logic for ActWebAPIEditPage.xaml
    /// </summary>
    public partial class ActWebAPIEditPage : Page
    {
        ActWebAPIBase mAct;
        ApplicationAPIUtils.eWebApiType mWebApiType;

        public ActWebAPIEditPage(ActWebAPIBase act)
        {
            mAct = act;
            if (act.GetType() == typeof(ActWebAPIRest))
                mWebApiType = ApplicationAPIUtils.eWebApiType.REST;
            else
                mWebApiType = ApplicationAPIUtils.eWebApiType.SOAP;

            InitializeComponent();
            BindUiControls();
            InitializeUIByActionType();
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
                    CookieMode.Init(mAct.GetOrCreateInputParam(ActWebAPIRest.Fields.CookieMode, ApplicationAPIUtils.eCookieMode.New.ToString()), typeof(ApplicationAPIUtils.eCookieMode), false, null);

                    //Request Type
                    RequestTypeComboBox.Init(mAct.GetOrCreateInputParam(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString()), typeof(ApplicationAPIUtils.eRequestType), false, null);

                    //HttpVersion content type
                    HttpVersioncombobox.Init(mAct.GetOrCreateInputParam(ActWebAPIRest.Fields.ReqHttpVersion, ApplicationAPIUtils.eHttpVersion.HTTPV11.ToString()), typeof(ApplicationAPIUtils.eHttpVersion), false, null);

                    //Request content type
                    ContentTypeComboBox.Init(mAct.GetOrCreateInputParam(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.JSon.ToString()), typeof(ApplicationAPIUtils.eContentType), false, ContentTypeChange);
                    
                    //Response Content Type
                    ResponseTypeComboBox.Init(mAct.GetOrCreateInputParam(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString()), typeof(ApplicationAPIUtils.eContentType), false, ResponseTypeComboBox_SelectionChanged);

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

            if ((mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded.ToString()))
            {
                FreeTextStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                FormDataGridPanel.Visibility = System.Windows.Visibility.Visible;
                DynamicElementGridPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            if  (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eContentType.FormData.ToString())
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
                    TemplateFileNameFileBrowser.ValueTextBox.Text = string.Empty;
                TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                BodyInputGridPannel.Visibility = System.Windows.Visibility.Collapsed;
                FreeTextStackPanel.Visibility = Visibility.Visible;
            }
            if ((mAct.GetInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton)) == ApplicationAPIUtils.eRequestBodyType.TemplateFile.ToString())
            {
                TemplateStackPanel.Visibility = System.Windows.Visibility.Visible;
                if (!String.IsNullOrEmpty((mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser))))
                    RequestBodyUCValueExpression.ValueTextBox.Text = string.Empty;
                FreeTextStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                BodyInputGridPannel.Visibility = System.Windows.Visibility.Collapsed;
                FreeTextStackPanel.Visibility = Visibility.Collapsed;
            }
            else if ((mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded.ToString()) || (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eContentType.FormData.ToString()))
            {
                if (!String.IsNullOrEmpty((mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser))))
                    RequestBodyUCValueExpression.ValueTextBox.Text = string.Empty;
                if (!String.IsNullOrEmpty((mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser))))
                    TemplateFileNameFileBrowser.ValueTextBox.Text = string.Empty;
                FreeTextStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                BodyInputGridPannel.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void BrowseTemplateFileButton_Click(object sender, RoutedEventArgs e)
        {
            string SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
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
                            newFileName = newFileName.Substring(0, newFileName.IndexOf(copySufix));
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
            string SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
            if (CertificatePath.ValueTextBox.Text != null)
            {
                // replace Absolute file name with relative to solution
                string FileName = CertificatePath.ValueTextBox.Text.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }

                CertificatePath.ValueTextBox.Text = FileName;

                bool ImportFileFlag = false;
                Boolean.TryParse(mAct.GetInputParamValue(ActWebAPIBase.Fields.ImportCetificateFile), out ImportFileFlag);
                if (ImportFileFlag)
                {
                    //TODO import Certificate File to solution folder
                    string targetPath = System.IO.Path.Combine(SolutionFolder, @"Documents\WebServices\Certificates");
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
                            newFileName = newFileName.Substring(0, newFileName.IndexOf(copySufix));
                        newFileName = newFileName + copySufix + fileNum.ToString() + System.IO.Path.GetExtension(destFile);
                        destFile = System.IO.Path.Combine(targetPath, newFileName);
                    }
                    
                    System.IO.File.Copy(FileName, destFile, true);
                    CertificatePath.ValueTextBox.Text = @"~\Documents\WebServices\Certificates\" + System.IO.Path.GetFileName(destFile);
                }
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
                    mAct.RequestKeyValues.ClearAll();
                if (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded.ToString())
                {
                    //switch combobox   & browse button off 
                    FormDataGrid.ChangeGridView("UrlEncoded");
                }
                else if (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eContentType.FormData.ToString())
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
            GridViewDef FormDataView = new GridViewDef("FormData");
            FormDataView.GridColsView = new ObservableList<GridColView>();

            FormDataView.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), Header = "Key", WidthWeight = 100 });
            List<ComboEnumItem> valueTypes = GingerCore.General.GetEnumValuesForCombo(typeof(WebAPIKeyBodyValues.eValueType));
            FormDataView.GridColsView.Add(new GridColView() { Field = nameof(WebAPIKeyBodyValues.ValueType), Header = "Value Type", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = valueTypes });
            FormDataView.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), Header = "Value/File Path", WidthWeight = 100 });
            FormDataView.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.controlGrid.Resources["VEGridValueExpressionButton"] });
            FormDataView.GridColsView.Add(new GridColView() { Field = "Browse", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.controlGrid.Resources["BrowseValueFilesButton"] });

            //Define URLEncoded GridView
            GridViewDef UrlEncodedView = new GridViewDef("UrlEncoded");
            UrlEncodedView.GridColsView = new ObservableList<GridColView>();
            ObservableList<GridColView> UrlViewCols = new ObservableList<GridColView>();
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

            if (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded.ToString())
            {
                //switch combobox   & browse button off 
                FormDataGrid.ChangeGridView("UrlEncoded");
            }
            else if (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == ApplicationAPIUtils.eContentType.FormData.ToString())
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
                System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();

                dlg.DefaultExt = "*.*";
                dlg.Filter = "All files (All Files)|*.*";
                string SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // replace Absolute file name with relative to solution
                    FileName = dlg.FileName.ToUpper();
                    if (FileName.Contains(SolutionFolder))
                    {
                        FileName = FileName.Replace(SolutionFolder, @"~\");
                    }
                    item.Value = FileName;
                    FormDataGrid.DataSourceList.CurrentItem = item;
                }
            }
        }

        private void AddRow(object sender, RoutedEventArgs e)
        {
            WebAPIKeyBodyValues Wa = new WebAPIKeyBodyValues();
            Wa.ValueType = WebAPIKeyBodyValues.eValueType.Text;
            mAct.RequestKeyValues.Add(Wa);
        }
        private void ResponseTypeComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (mAct.GetInputParamValue(ActWebAPIRest.Fields.ResponseContentType) == ApplicationAPIUtils.eContentType.JSon.ToString())
            {
                JSON.Visibility = Visibility.Visible;
            }
            else
                JSON.Visibility = Visibility.Collapsed;
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
                    mAct.DynamicElements.Add(new ActInputValue() { Param = SoapSecurityContent.ElementAt(i)});
                }

                else
                {
                    if (mAct.DynamicElements.Where(x => x.Param.Equals(SoapSecurityContent.ElementAt(i) )).Count() == 0)
                    {
                        mAct.DynamicElements.Add(new ActInputValue() { Param =SoapSecurityContent.ElementAt(i)});
                    }
                }
            }
        }
    }
}
