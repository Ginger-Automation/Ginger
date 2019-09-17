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
using Ginger;
using Ginger.UserControls;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.ApplicationModelsLib.APIModelWizard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GingerWPF.ApplicationModelsLib.APIModels
{
    public partial class APIModelPage : Page
    {
        ApplicationAPIModel mApplicationAPIModel;
        ModelParamsPage page;
        private bool saveWasDone = false;
        public APIModelPage(ApplicationAPIModel applicationAPIModelBase)
        {
            mApplicationAPIModel = applicationAPIModelBase;

            InitializeComponent();
            BindUiControls();
            InitializeUIByActionType();
            SetCustomCredPanel();
            SetCertificatePanel();
            SecondRow.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 380;

            WorkSpace.Instance.RefreshGlobalAppModelParams(mApplicationAPIModel);
            page = new ModelParamsPage(mApplicationAPIModel);
            xDynamicParamsFrame.Content = page;

            OutputTemplatePage outputTemplatePage = new OutputTemplatePage(mApplicationAPIModel);
            xOutputTemplateFrame.Content = outputTemplatePage;

            mApplicationAPIModel.AppModelParameters.CollectionChanged += AppModelParameters_CollectionChanged;
            mApplicationAPIModel.GlobalAppModelParameters.CollectionChanged += AppModelParameters_CollectionChanged;
            UpdateModelParametersTabHeader();
            mApplicationAPIModel.ReturnValues.CollectionChanged += ReturnValues_CollectionChanged;
            UpdateOutputTemplateTabHeader();
        }

        private void InitializeUIByActionType()
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtName, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.Name));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtDescription, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.Description));

            FillTargetAppsComboBox();
            xTargetApplicationComboBox.Init(mApplicationAPIModel, nameof(ApplicationAPIModel.TargetApplicationKey));
            xTagsViewer.Init(mApplicationAPIModel.TagsKeys);

            xAPITypeComboBox.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.APIType), typeof(ApplicationAPIUtils.eWebApiType));
            xAPITypeComboBox.ComboBox.SelectionChanged += ComboBox_SelectionChanged;

            CookieMode.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.CookieMode), typeof(ApplicationAPIUtils.eCookieMode));
            RequestTypeComboBox.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.RequestType), typeof(ApplicationAPIUtils.eRequestType));
            HttpVersioncombobox.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.ReqHttpVersion), typeof(ApplicationAPIUtils.eHttpVersion));
            ContentTypeComboBox.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.ContentType), typeof(ApplicationAPIUtils.eContentType), ContentTypeChange);
            ResponseTypeComboBox.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.ResponseContentType), typeof(ApplicationAPIUtils.eContentType));
            //Check maybe the binding of TemplateFileNameFileBrowser need to be different between soap and rest
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TemplateFileNameFileBrowser, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.TemplateFileNameFileBrowser));

            xTargetApplicationComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            xAPITypeComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            CookieMode.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            RequestTypeComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            HttpVersioncombobox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            ContentTypeComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            ResponseTypeComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;


            ApplicationAPIModel AAMS = mApplicationAPIModel as ApplicationAPIModel;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SoapActionTextBox, TextBox.TextProperty, mApplicationAPIModel, nameof(AAMS.SOAPAction));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TemplateFileNameFileBrowser, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.TemplateFileNameFileBrowser));

            SetViewByAPIType(mApplicationAPIModel.APIType);
        }

        private void ReturnValues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateOutputTemplateTabHeader();
        }

        private void AppModelParameters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateModelParametersTabHeader();
        }

        private void UpdateModelParametersTabHeader()
        {
            xDynamicParamsTab.Text = string.Format("Model Parameters ({0})", mApplicationAPIModel.AppModelParameters.Count + mApplicationAPIModel.GlobalAppModelParameters.Count);
        }

        private void UpdateOutputTemplateTabHeader()
        {
            xOutputTemplateTab.Text = string.Format("Output Values Template ({0})", mApplicationAPIModel.ReturnValues.Count);
        }

        private void FillTargetAppsComboBox()
        {
            //get key object 
            if (mApplicationAPIModel.TargetApplicationKey != null)
            {
                RepositoryItemKey key =  WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.Guid == mApplicationAPIModel.TargetApplicationKey.Guid).Select(x => x.Key).FirstOrDefault();
                if (key != null)
                {
                    mApplicationAPIModel.TargetApplicationKey = key;
                }
                else
                {                                        
                    Reporter.ToUser(eUserMsgKey.MissingTargetApplication, "The mapped " + mApplicationAPIModel.Key.ItemName + " Target Application was not found, please select new Target Application");
                }
            }
            xTargetApplicationComboBox.ComboBox.ItemsSource =  WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x=>x.Platform == ePlatformType.WebServices).ToList();
            xTargetApplicationComboBox.ComboBox.SelectedValuePath = nameof(ApplicationPlatform.Key);
            xTargetApplicationComboBox.ComboBox.DisplayMemberPath = nameof(ApplicationPlatform.AppName);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetViewByAPIType(mApplicationAPIModel.APIType);
        }

        private void SetViewByAPIType(ApplicationAPIUtils.eWebApiType apiType)
        {
            switch (apiType)
            {
                case ApplicationAPIUtils.eWebApiType.REST:
                    PanelSoap.Visibility = Visibility.Collapsed;
                    RequestContent.Visibility = Visibility.Visible;
                    RestHeader.Visibility = Visibility.Visible;
                    UseWSSecurityHeader.Visibility = Visibility.Collapsed;
                    break;

                case ApplicationAPIUtils.eWebApiType.SOAP:
                    RequestContent.Visibility = Visibility.Collapsed;
                    RestHeader.Visibility = Visibility.Collapsed;
                    PanelSoap.Visibility = Visibility.Visible;
                    UseWSSecurityHeader.Visibility = Visibility.Visible;
                    break;
            }
        }
        public void BindUiControls()
        {
            //URL fields:
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(EndPointURLTextBox, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.EndpointURL));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(URLUserTextBox, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.URLUser));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(URLDomainTextBox, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.URLDomain));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(URLPasswordTextBox, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.URLPass));

            //Network Credential selection radio button:
            switch (mApplicationAPIModel.NetworkCredentials)
            {
                case ApplicationAPIUtils.eNetworkCredentials.Default:
                    DefaultNetworkCredentialsRadioButton.IsChecked = true;
                    break;

                case ApplicationAPIUtils.eNetworkCredentials.Custom:
                    CustomNetworkCredentialsRadioButton.IsChecked = true;
                    break;
            }

            //Request Body Selection radio button:
            switch (mApplicationAPIModel.RequestBodyType)
            {
                case ApplicationAPIUtils.eRequestBodyType.FreeText:
                    FreeTextRadioButton.IsChecked = true;
                    break;
                case ApplicationAPIUtils.eRequestBodyType.TemplateFile:
                    TemplateFileRadioButton.IsChecked = true;
                    break;
            }

            //CertficiateRadioButtons :
            switch (mApplicationAPIModel.CertificateType)
            {
                case ApplicationAPIUtils.eCretificateType.AllSSL:
                    SSLCertificateTypeAllCertificatesRadioButton.IsChecked = true;
                    break;
                case ApplicationAPIUtils.eCretificateType.Custom:
                    SSLCertificateTypeCustomRadioButton.IsChecked = true;
                    break;
            }

            //Do Not Fail Action On Bad Response
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DoNotFailActionOnBadRespose, CheckBox.IsCheckedProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.DoNotFailActionOnBadRespose));

            //Request Body fields:
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RequestBodyTextBox, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.RequestBody));
            RequestBodyTextBox.Height = 200;

            //Import Request File
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DoNotImportRequestFile, CheckBox.IsCheckedProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.ImportRequestFile));

            //SSL Certificates:
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(CertificatePath, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.CertificatePath));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(CertificatePasswordUCValueExpression, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.CertificatePassword));

            //Import Certificate
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DoNotCertificateImportFile, CheckBox.IsCheckedProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.ImportCetificateFile));

            //Security:
            SecurityTypeComboBox.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.SecurityType), typeof(ApplicationAPIUtils.eSercurityType));
            SecurityTypeComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;

            //Authorization:
            AuthTypeComboBox.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.AuthorizationType), typeof(ApplicationAPIUtils.eAuthType), AuthorizationBox);
            AuthTypeComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AuthUserTextBox, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.AuthUsername));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AuthPasswordTextBox, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.AuthPassword));

            SetHTTPHeadersGrid();
            SetKeyValuesGrid();
            CheckRequestBodySelection();
            CheckCertificateSelection();
        }

        private void ContentTypeChange(object sender, RoutedEventArgs e)
        {
            switch (mApplicationAPIModel.ContentType)
            {
                case ApplicationAPIUtils.eContentType.JSon:
                    RequestBodyTypePanel.Visibility = Visibility.Visible;
                    CheckRequestBodySelection();
                    break;
                case ApplicationAPIUtils.eContentType.TextPlain:
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                    CheckRequestBodySelection();
                    break;
                case ApplicationAPIUtils.eContentType.XML:
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                    CheckRequestBodySelection();
                    break;
                case ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded:
                    BodyInputGridPannel.Visibility = System.Windows.Visibility.Collapsed;
                    FreeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                    FormDataGridPanel.Visibility = System.Windows.Visibility.Visible;
                    RefreshRequestKeyValuesGrid();
                    break;
                case ApplicationAPIUtils.eContentType.FormData:
                    BodyInputGridPannel.Visibility = System.Windows.Visibility.Collapsed;
                    FreeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                    FormDataGridPanel.Visibility = System.Windows.Visibility.Visible;
                    RefreshRequestKeyValuesGrid();
                    break;
            }
        }

        public void RefreshRequestKeyValuesGrid()
        {
            if (this.IsLoaded && mApplicationAPIModel.APIModelBodyKeyValueHeaders != null)
                mApplicationAPIModel.APIModelBodyKeyValueHeaders.Clear();

            if ((mApplicationAPIModel.APIType == ApplicationAPIUtils.eWebApiType.REST) && mApplicationAPIModel.ContentType == ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded)
            {
                //switch combobox   & browse button off 
                FormDataGrid.ChangeGridView("UrlEncoded");
            }
            else if ((mApplicationAPIModel.APIType == ApplicationAPIUtils.eWebApiType.REST) && mApplicationAPIModel.ContentType == ApplicationAPIUtils.eContentType.FormData)
            {
                //switch combobox  & browse button on
                FormDataGrid.ChangeGridView("FormData");
            }
        }

        private void SetHTTPHeadersGrid()
        {
            HttpHeadersGrid.Title = "Request Headers";
            HttpHeadersGrid.SetTitleStyle((Style)TryFindResource("@ucGridTitleLightStyle"));

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(APIModelKeyValue.Param), Header = "Header", WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(APIModelKeyValue.Value), Header = "Value", WidthWeight = 100 });

            HttpHeadersGrid.SetAllColumnsDefaultView(view);
            HttpHeadersGrid.InitViewItems();
            HttpHeadersGrid.DataSourceList = mApplicationAPIModel.HttpHeaders;

            HttpHeadersGrid.ShowRefresh = Visibility.Collapsed;
            HttpHeadersGrid.ShowUpDown = Visibility.Collapsed;
            HttpHeadersGrid.ShowEdit = Visibility.Collapsed;

            HttpHeadersGrid.btnAdd.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(AddHttpHeaderRow));
            HttpHeadersGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddHttpHeaderRow));
        }


        private void AddHttpHeaderRow(object sender, RoutedEventArgs e)
        {
            APIModelKeyValue AMBKV = new APIModelKeyValue();
            mApplicationAPIModel.HttpHeaders.Add(AMBKV);
        }

        private void AddFormDataGridRow(object sender, RoutedEventArgs e)
        {
            APIModelBodyKeyValue Wa = new APIModelBodyKeyValue();
            Wa.ValueType = APIModelBodyKeyValue.eValueType.Text;
            mApplicationAPIModel.APIModelBodyKeyValueHeaders.Add(Wa);
        }

        private void SecExpanded(object sender, RoutedEventArgs e)
        {
            if ((!string.IsNullOrEmpty(CertificatePath.Text)) || (!string.IsNullOrEmpty(CertificatePasswordUCValueExpression.Text)))
            {
                SecurityExpander.IsExpanded = true;
            }
        }

        private void GridInputGridBrowseButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BodyExpanded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(RequestBodyTextBox.Text) || (!string.IsNullOrEmpty(TemplateFileNameFileBrowser.Text)))
            {
                BodyExpander.IsExpanded = true;
            }
        }

        private void CheckRequestBodySelection()
        {
            if ((mApplicationAPIModel.APIType == ApplicationAPIUtils.eWebApiType.REST) && mApplicationAPIModel.ContentType == ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded)
            {
                FreeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                FormDataGridPanel.Visibility = System.Windows.Visibility.Visible;
            }
            if ((mApplicationAPIModel.APIType == ApplicationAPIUtils.eWebApiType.REST) && mApplicationAPIModel.ContentType == ApplicationAPIUtils.eContentType.FormData)
            {
                FreeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                FormDataGridPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                if (mApplicationAPIModel.RequestBodyType == ApplicationAPIUtils.eRequestBodyType.FreeText)
                {
                    FreeStackPanel.Visibility = System.Windows.Visibility.Visible;
                    TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                    FormDataGridPanel.Visibility = System.Windows.Visibility.Collapsed;
                }
                if (mApplicationAPIModel.RequestBodyType == ApplicationAPIUtils.eRequestBodyType.TemplateFile)
                {
                    FreeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    TemplateStackPanel.Visibility = System.Windows.Visibility.Visible;
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                    FormDataGridPanel.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private void CheckCertificateSelection()
        {
            if (mApplicationAPIModel.CertificateType == ApplicationAPIUtils.eCretificateType.Custom)
            {
                CertificateStackPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                CertificateStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void BodyCollapsed(object sender, RoutedEventArgs e)
        {
            BodyExpander.IsExpanded = false;
        }

        private void SecCollapsed(object sender, RoutedEventArgs e)
        {
            SecurityExpander.IsExpanded = false;
        }


        private void NetworkCreds_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender == null || BodySelection == null)
                return;

            RadioButton rBtn = sender as RadioButton;
            if ((bool)rBtn.IsChecked)
            {
                switch (rBtn.Name)
                {
                    case "DefaultNetworkCredentialsRadioButton":
                        if (mApplicationAPIModel.NetworkCredentials != ApplicationAPIUtils.eNetworkCredentials.Default)
                        {
                            mApplicationAPIModel.NetworkCredentials = ApplicationAPIUtils.eNetworkCredentials.Default;
                            SP_CustomCreds.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        break;

                    case "CustomNetworkCredentialsRadioButton":
                        if (mApplicationAPIModel.NetworkCredentials != ApplicationAPIUtils.eNetworkCredentials.Custom)
                        {
                            mApplicationAPIModel.NetworkCredentials = ApplicationAPIUtils.eNetworkCredentials.Custom;
                            SP_CustomCreds.Visibility = System.Windows.Visibility.Visible;
                        }
                        break;
                }
            }
        }

        private void RequestBodyType_Selection(object sender, RoutedEventArgs e)
        {
            if (sender == null || BodySelection == null)
                return;

            RadioButton rBtn = sender as RadioButton;
            if ((bool)rBtn.IsChecked)
            {
                switch (rBtn.Name)
                {
                    case "FreeTextRadioButton":
                        if (mApplicationAPIModel.RequestBodyType != ApplicationAPIUtils.eRequestBodyType.FreeText)
                        {
                            mApplicationAPIModel.RequestBodyType = ApplicationAPIUtils.eRequestBodyType.FreeText;
                            FreeStackPanel.Visibility = System.Windows.Visibility.Visible;
                            if (!String.IsNullOrEmpty(mApplicationAPIModel.TemplateFileNameFileBrowser))
                                TemplateFileNameFileBrowser.Text = string.Empty;
                            TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                            RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                            BodyInputGridPannel.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        break;

                    case "TemplateFileRadioButton":
                        if (mApplicationAPIModel.RequestBodyType != ApplicationAPIUtils.eRequestBodyType.TemplateFile)
                        {
                            mApplicationAPIModel.RequestBodyType = ApplicationAPIUtils.eRequestBodyType.TemplateFile;
                            TemplateStackPanel.Visibility = System.Windows.Visibility.Visible;
                            if (!String.IsNullOrEmpty(mApplicationAPIModel.TemplateFileNameFileBrowser))
                                RequestBodyTextBox.Text = string.Empty;
                            FreeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                            RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                            BodyInputGridPannel.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        break;

                    default:
                        if (mApplicationAPIModel.ContentType == ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded || mApplicationAPIModel.ContentType == ApplicationAPIUtils.eContentType.FormData)
                        {
                            if (!String.IsNullOrEmpty(mApplicationAPIModel.TemplateFileNameFileBrowser))
                                RequestBodyTextBox.Text = string.Empty;
                            if (!String.IsNullOrEmpty(mApplicationAPIModel.TemplateFileNameFileBrowser))
                                TemplateFileNameFileBrowser.Text = string.Empty;
                            FreeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                            TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                            RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                            BodyInputGridPannel.Visibility = System.Windows.Visibility.Visible;
                        }
                        break;
                }
            }
        }


        private void TemplateFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = "All Files (*.*)|*.*";
            dlg.FilterIndex = 1;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (DoNotImportRequestFile.IsChecked == true)
                {

                    string NewPath = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, "Documents", Path.GetFileName(dlg.FileName));
                    if (!Directory.Exists(Path.GetDirectoryName(NewPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(NewPath));
                    File.Copy(dlg.FileName, NewPath, true);
                    TemplateFileNameFileBrowser.Text = NewPath.Replace(WorkSpace.Instance.SolutionRepository.SolutionFolder, "~\\");
                }
                else
                    TemplateFileNameFileBrowser.Text = dlg.FileName;
            }
        }

        private void CertificatePathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = "All Files (*.*)|*.*";
            dlg.FilterIndex = 1;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (DoNotCertificateImportFile.IsChecked == true)
                {
                    string NewPath = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, "Documents", Path.GetFileName(dlg.FileName));
                    if (!Directory.Exists(Path.GetDirectoryName(NewPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(NewPath));
                    File.Copy(dlg.FileName, NewPath, true);
                    CertificatePath.Text = NewPath.Replace(WorkSpace.Instance.SolutionRepository.SolutionFolder, "~\\");
                }
                else
                    CertificatePath.Text = dlg.FileName;
            }
        }

        private void SetCustomCredPanel()
        {
            if (mApplicationAPIModel.NetworkCredentials == ApplicationAPIUtils.eNetworkCredentials.Custom)
                SP_CustomCreds.Visibility = Visibility.Visible;
            else SP_CustomCreds.Visibility = Visibility.Collapsed;
        }

        private void SetCertificatePanel()
        {
            if (mApplicationAPIModel.CertificateType == ApplicationAPIUtils.eCretificateType.AllSSL)
                CertificateStackPanel.Visibility = Visibility.Collapsed;
            else CertificateStackPanel.Visibility = Visibility.Visible;
        }

        private void CertificateSelection_Changed(object sender, RoutedEventArgs e)
        {
            if (sender == null || BodySelection == null)
                return;

            RadioButton rBtn = sender as RadioButton;
            if ((bool)rBtn.IsChecked)
            {
                switch (rBtn.Name)
                {
                    case "SSLCertificateTypeAllCertificatesRadioButton":
                        mApplicationAPIModel.CertificateType = ApplicationAPIUtils.eCretificateType.AllSSL;
                        break;
                    case "SSLCertificateTypeCustomRadioButton":
                        mApplicationAPIModel.CertificateType = ApplicationAPIUtils.eCretificateType.Custom;
                        break;
                }
                SetCertificatePanel();
            }
        }

        public void SetKeyValuesGrid()
        {
            FormDataGrid.Title = "Request Key Values";
            FormDataGrid.SetTitleStyle((Style)TryFindResource("@ucGridTitleLightStyle"));

            //View with Browse and Combobox -->Form Data 
            GridViewDef FormDataView = new GridViewDef("FormData");
            FormDataView.GridColsView = new ObservableList<GridColView>();

            FormDataView.GridColsView.Add(new GridColView() { Field = nameof(APIModelBodyKeyValue.Param), Header = "Key", WidthWeight = 100 });
            List<ComboEnumItem> valueTypes = GingerCore.General.GetEnumValuesForCombo(typeof(APIModelBodyKeyValue.eValueType));
            FormDataView.GridColsView.Add(new GridColView() { Field = APIModelBodyKeyValue.Fields.ValueType, Header = "Value Type", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = valueTypes });
            FormDataView.GridColsView.Add(new GridColView() { Field = nameof(APIModelBodyKeyValue.Value), Header = "Value/File Path", WidthWeight = 100 });

            //Define URLEncoded GridView
            GridViewDef UrlEncodedView = new GridViewDef("UrlEncoded");
            UrlEncodedView.GridColsView = new ObservableList<GridColView>();
            ObservableList<GridColView> UrlViewCols = new ObservableList<GridColView>();
            UrlEncodedView.GridColsView.Add(new GridColView() { Field = APIModelBodyKeyValue.Fields.ValueType, Visible = false });

            FormDataGrid.SetAllColumnsDefaultView(FormDataView);
            FormDataGrid.AddCustomView(UrlEncodedView);
            FormDataGrid.InitViewItems();
            FormDataGrid.DataSourceList = mApplicationAPIModel.APIModelBodyKeyValueHeaders;

            FormDataGrid.ShowRefresh = Visibility.Collapsed;
            FormDataGrid.ShowUpDown = Visibility.Collapsed;
            FormDataGrid.ShowEdit = Visibility.Collapsed;
            FormDataGrid.ShowViewCombo = Visibility.Collapsed;

            FormDataGrid.btnAdd.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(AddFormDataGridRow));
            FormDataGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddFormDataGridRow));

            if ((mApplicationAPIModel.APIType == ApplicationAPIUtils.eWebApiType.REST) && mApplicationAPIModel.ContentType == ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded)
            {
                //switch combobox   & browse button off 
                FormDataGrid.ChangeGridView("UrlEncoded");
            }
            else if ((mApplicationAPIModel.APIType == ApplicationAPIUtils.eWebApiType.REST) && mApplicationAPIModel.ContentType == ApplicationAPIUtils.eContentType.FormData)
            {
                //switch combobox  & browse button on
                FormDataGrid.ChangeGridView("FormData");
            }
        }

        private void ActionTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set the selected tab text style
            try
            {
                if (APIModelTabs.SelectedItem != null)
                {
                    foreach (TabItem tab in APIModelTabs.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (APIModelTabs.SelectedItem == tab)
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                else
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$Color_DarkBlue");

                                ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Action Edit Page tabs style", ex);
            }
        }

        private void AuthorizationBox(object sender, SelectionChangedEventArgs e)
        {
            if (mApplicationAPIModel.AuthorizationType == ApplicationAPIUtils.eAuthType.BasicAuthentication)
                Auth_Creds.Visibility = Visibility.Visible;
            else if (mApplicationAPIModel.AuthorizationType == ApplicationAPIUtils.eAuthType.NoAuthentication)
                Auth_Creds.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Apply Ws-Security Header 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReplaceWSSecurityHeader_Click(object sender, RoutedEventArgs e)
        {
            string txtBoxBodyContent = RequestBodyTextBox.Text;
            List<string> SoapSecurityContent = ApplicationModelBase.GetSoapSecurityHeaderContent(ref txtBoxBodyContent);
            string wsSecuritySettings = SoapSecurityContent.ElementAt(0);

            RequestBodyTextBox.Text = txtBoxBodyContent;
            ActInputValue user = new ActInputValue();
            if (string.IsNullOrEmpty(txtBoxBodyContent))
            {
                return;
            }
            SetCredentialPlaceHolders(SoapSecurityContent);
        }
        /// <summary>
        /// Security Header Credentials Added
        /// </summary>
        /// <param name="SoapSecurityContent"></param>
        private void SetCredentialPlaceHolders(List<string> SoapSecurityContent)
        {
            for (int i = 1; i < SoapSecurityContent.Count; i++)
            {
                AppModelParameter newAppModelParam = new AppModelParameter();
                if (page.ParamsList.Count == 0)
                {
                    newAppModelParam.PlaceHolder = SoapSecurityContent.ElementAt(i);
                }
                else
                {
                    if (page.ParamsList.Where(x => x.PlaceHolder.Equals(SoapSecurityContent.ElementAt(i))).Count() == 0)
                    {
                        newAppModelParam.PlaceHolder = SoapSecurityContent.ElementAt(i);
                    }
                }

                page.ParamsList.Add(newAppModelParam);
            }
        }


        public enum eEditMode
        {
            View = 0,
            Design = 1,
            FindAndReplace = 2,
            //Global = 4
        }

        public eEditMode editMode { get; set; }

        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false, eEditMode e = eEditMode.Design)
        {
            mApplicationAPIModel.StartDirtyTracking();
            //changed the style to be free - since many other windows get stuck and doesn't show
            // Need to find a solution if 2 windows show as Dialog...
            string title = "";
            this.Width = 1100;
            this.Height = 800;

            editMode = e;
            ObservableList<Button> winButtons = new ObservableList<Button>();
            switch (editMode)
            {
                case eEditMode.Design:
                    Button okBtn2 = new Button();
                    okBtn2.Content = "Ok";
                    okBtn2.Click += new RoutedEventHandler(okBtn_Click);
                    Button undoBtn2 = new Button();
                    undoBtn2.Content = "Undo & Close";
                    undoBtn2.Click += new RoutedEventHandler(undoBtn_Click);
                    winButtons.Add(okBtn2);
                    winButtons.Add(undoBtn2);
                    break;

                case eEditMode.View:
                    title = "View " + mApplicationAPIModel.Name + " API Model";
                    Button okBtnView = new Button();
                    okBtnView.Content = "Ok";
                    okBtnView.Click += new RoutedEventHandler(okBtn_Click);
                    winButtons.Add(okBtnView);
                    break;
                case eEditMode.FindAndReplace:
                    title = "Edit " + mApplicationAPIModel.Name + " API Model";
                    mApplicationAPIModel.SaveBackup();
                    Button saveBtnAnalyzer = new Button();
                    saveBtnAnalyzer.Content = "Save";
                    saveBtnAnalyzer.Click += new RoutedEventHandler(saveBtn_Click);
                    Button undoBtnAnalyzer = new Button();
                    undoBtnAnalyzer.Content = "Undo & Close";
                    undoBtnAnalyzer.Click += new RoutedEventHandler(undoBtn_Click);
                    winButtons.Add(undoBtnAnalyzer);
                    winButtons.Add(saveBtnAnalyzer);
                    break;
            }
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, string.Empty, CloseWinClicked, startupLocationWithOffset: startupLocationWithOffset);
            return saveWasDone;
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mApplicationAPIModel);
            saveWasDone = true;
            _pageGenericWin.Close();
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.ToSaveChanges) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                UndoChangesAndClose();
            }
            else
            {
                _pageGenericWin.Close();
            }
        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        GenericWindow _pageGenericWin = null;

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            _pageGenericWin.Close();
        }

        private void UndoChangesAndClose()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            mApplicationAPIModel.RestoreFromBackup();
            Mouse.OverrideCursor = null;

            _pageGenericWin.Close();
        }

    }
}
