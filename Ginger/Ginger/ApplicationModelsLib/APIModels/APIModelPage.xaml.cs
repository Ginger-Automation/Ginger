#region License
/*
Copyright � 2014-2022 European Support Limited

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
using Amdocs.Ginger.CoreNET.External.WireMock;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.ApplicationModelsLib.WireMockAPIModels;
using Ginger.UserControls;
using Ginger.UserControlsLib;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.ApplicationModelsLib.APIModelWizard;
using GingerWPF.TreeViewItemsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace GingerWPF.ApplicationModelsLib.APIModels
{
    public partial class APIModelPage : GingerUIPage
    {
        private WireMockMappingController wmController = new();

        ApplicationAPIModel mApplicationAPIModel;
        ModelParamsPage modelParamsPage;
        private bool saveWasDone = false;
        General.eRIPageViewMode mPageViewMode;
        public APIModelPage(ApplicationAPIModel applicationAPIModelBase, General.eRIPageViewMode viewMode = General.eRIPageViewMode.Standalone)
        {
            mApplicationAPIModel = applicationAPIModelBase;
            CurrentItemToSave = mApplicationAPIModel;

            InitializeComponent();
            BindUiControls();
            InitializeUIByActionType();
            SetCustomCredPanel();
            SetCertificatePanel();
            SecondRow.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 380;

            WorkSpace.Instance.RefreshGlobalAppModelParams(mApplicationAPIModel);
            modelParamsPage = new ModelParamsPage(mApplicationAPIModel, viewMode);
            xDynamicParamsFrame.ClearAndSetContent(modelParamsPage);

            OutputTemplatePage outputTemplatePage = new OutputTemplatePage(mApplicationAPIModel, viewMode);
            xOutputTemplateFrame.ClearAndSetContent(outputTemplatePage);

            WireMockTemplatePage wiremockTemplatePage = new WireMockTemplatePage(mApplicationAPIModel, viewMode);
            xWireMockTemplateFrame.ClearAndSetContent(wiremockTemplatePage);
            wiremockTemplatePage.GridUpdated += WireMockTemplatePage_GridUpdated;
            TreeViewItemGenericBase.MappingCreated += OnMappingCreated;


            mApplicationAPIModel.AppModelParameters.CollectionChanged += AppModelParameters_CollectionChanged;
            mApplicationAPIModel.GlobalAppModelParameters.CollectionChanged += AppModelParameters_CollectionChanged;
            UpdateModelParametersTabHeader();
            mApplicationAPIModel.ReturnValues.CollectionChanged += ReturnValues_CollectionChanged;
            UpdateOutputTemplateTabHeader();
            _ = UpdateWireMockTemplateTabHeader();

            mPageViewMode = viewMode;

            if (mPageViewMode is General.eRIPageViewMode.View or General.eRIPageViewMode.ViewAndExecute)
            {
                UpdatePageAsReadOnly();
            }

            if (mPageViewMode == General.eRIPageViewMode.Add)
            {
                HttpHeadersGrid.ShowPaste = Visibility.Visible;
            }
        }

        private async void OnMappingCreated(object sender, EventArgs e)
        {
            // Refresh the page
            UpdateWireMockTemplateTabHeader();
        }

        void UpdatePageAsReadOnly()
        {
            txtName.IsReadOnly = true;
            txtDescription.IsReadOnly = true;
            xTagsViewer.IsEnabled = false;
            xTargetApplicationComboBox.IsEnabled = false;
            xAPITypeComboBox.IsEnabled = false;
            EndPointURLTextBox.IsReadOnly = true;
            RequestTypeComboBox.IsEnabled = false;
            HttpVersioncombobox.IsEnabled = false;
            ResponseTypeComboBox.IsEnabled = false;
            CookieMode.IsEnabled = false;
            NetworkCeredentials.IsEnabled = false;
            SP_CustomCreds.IsEnabled = false;
            SoapActionTextBox.IsReadOnly = true;
            DoNotFailActionOnBadRespose.IsEnabled = false;

            HttpHeadersGrid.IsReadOnly = true;
            HttpHeadersGrid.ShowCopy = Visibility.Visible;
            HttpHeadersGrid.ShowAdd = Visibility.Collapsed;
            HttpHeadersGrid.ShowDelete = Visibility.Collapsed;
            HttpHeadersGrid.ShowClearAll = Visibility.Collapsed;

            ContentTypeComboBox.IsEnabled = false;
            BodySelection.IsEnabled = false;
            UseWSSecurityHeader.IsEnabled = false;
            RequestBodyTextBox.IsReadOnly = true;
            TemplateFileNameFileBrowser.IsReadOnly = true;
            TemplateFileBrowseButton.IsEnabled = false;
            DoNotImportRequestFile.IsEnabled = false;

            controlGrid.IsEnabled = false;

            FormDataGrid.IsReadOnly = true;
            FormDataGrid.ShowCopy = Visibility.Visible;
            FormDataGrid.ShowAdd = Visibility.Collapsed;
            FormDataGrid.ShowDelete = Visibility.Collapsed;
            FormDataGrid.ShowClearAll = Visibility.Collapsed;

            CertificateSelection.IsEnabled = false;
            CertificateFileGrid.IsEnabled = false;
            DoNotCertificateImportFile.IsEnabled = false;
            CertificatePasswordUCValueExpression.IsReadOnly = true;
            SecurityTypeComboBox.IsEnabled = false;
            AuthTypeComboBox.IsEnabled = false;
            AuthUserTextBox.IsReadOnly = true;
            AuthPasswordTextBox.IsReadOnly = true;

        }

        private void InitializeUIByActionType()
        {
            xShowIDUC.Init(mApplicationAPIModel);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtName, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.Name));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtDescription, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.Description));

            xTAlabel.Content = $"{GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.TargetApplication)}:";
            FillTargetAppsComboBox();
            xTargetApplicationComboBox.Init(mApplicationAPIModel, nameof(ApplicationAPIModel.TargetApplicationKey));
            xTagsViewer.Init(mApplicationAPIModel.TagsKeys);

            xAPITypeComboBox.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.APIType), typeof(ApplicationAPIUtils.eWebApiType));
            xAPITypeComboBox.ComboBox.SelectionChanged += ComboBox_SelectionChanged;

            CookieMode.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.CookieMode), typeof(ApplicationAPIUtils.eCookieMode));
            RequestTypeComboBox.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.RequestType), typeof(ApplicationAPIUtils.eRequestType));
            HttpVersioncombobox.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.ReqHttpVersion), typeof(ApplicationAPIUtils.eHttpVersion));
            ContentTypeComboBox.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.RequestContentType), typeof(ApplicationAPIUtils.eRequestContentType), ContentTypeChange);
            ResponseTypeComboBox.Init(mApplicationAPIModel, nameof(mApplicationAPIModel.ResponseContentType), typeof(ApplicationAPIUtils.eResponseContentType));
            //Check maybe the binding of TemplateFileNameFileBrowser need to be different between soap and rest
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TemplateFileNameFileBrowser, TextBox.TextProperty, mApplicationAPIModel, nameof(mApplicationAPIModel.TemplateFileNameFileBrowser));

            xTargetApplicationComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            xAPITypeComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            CookieMode.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            RequestTypeComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            HttpVersioncombobox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            ContentTypeComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            ResponseTypeComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;


            ApplicationAPIModel AAMS = mApplicationAPIModel;
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

        private async Task UpdateWireMockTemplateTabHeader()
        {
            try
            {
                int count = await WireMockTemplateTabCount();
                xWireMockTemplateTab.Text = string.Format("WireMock Mapping ({0})", count);
            }
            catch (Exception ex)
            {

                Reporter.ToLog(eLogLevel.ERROR, "error in getting wiremock mapping count", ex);
            }
        }

        public async Task<int> WireMockTemplateTabCount()
        {
            try
            {
                var mappings = await wmController.DeserializeWireMockResponseAsync();

                string ApiName = mApplicationAPIModel.Name;

                // Filter the mappings based on the Name
                int filteredMappings = mappings.Where(mapping => mapping.Name == ApiName).Count();
                return filteredMappings;
            }
            catch (Exception ex)
            {

                Reporter.ToLog(eLogLevel.ERROR, "error in getting wiremock mappings count", ex);
                return 0;
            }
        }
        private void FillTargetAppsComboBox()
        {
            //get key object 
            if (mApplicationAPIModel.TargetApplicationKey != null)
            {
                RepositoryItemKey key = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.Guid == mApplicationAPIModel.TargetApplicationKey.Guid).Select(x => x.Key).FirstOrDefault();
                if (key != null)
                {
                    mApplicationAPIModel.TargetApplicationKey = key;
                }
                else if (mApplicationAPIModel.TargetApplicationKey.ItemName != null && key == null)//if API Model is imported/copied from other solution
                {
                    var platform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(mApplicationAPIModel.TargetApplicationKey);
                    if (platform != ePlatformType.NA)
                    {
                        mApplicationAPIModel.TargetApplicationKey = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.Platform == platform).Select(x => x.Key).FirstOrDefault();
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.MissingTargetApplication, "The mapped " + mApplicationAPIModel.Key.ItemName + $" {GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.TargetApplication)} was not found, please select new {GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.TargetApplication)}");
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.MissingTargetApplication, "The mapped " + mApplicationAPIModel.Key.ItemName + $" {GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.TargetApplication)} was not found, please select new {GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.TargetApplication)}");
                }
            }
            xTargetApplicationComboBox.ComboBox.ItemsSource = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.Platform == ePlatformType.WebServices).ToList();
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
            xRealAPIRadioButton.SetBinding(RadioButton.IsCheckedProperty, new Binding
            {
                Source = mApplicationAPIModel,
                Path = new PropertyPath(nameof(mApplicationAPIModel.UseLiveAPI)),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                NotifyOnValidationError = true
            });
            xMockAPIRadioButton.SetBinding(RadioButton.IsCheckedProperty, new Binding
            {
                Source = mApplicationAPIModel,
                Path = new PropertyPath(nameof(mApplicationAPIModel.UseLiveAPI)),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                NotifyOnValidationError = true,
                Converter = new BoolInverterConverter(),
            });

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
                case ApplicationAPIUtils.eCertificateType.AllSSL:
                    SSLCertificateTypeAllCertificatesRadioButton.IsChecked = true;
                    break;
                case ApplicationAPIUtils.eCertificateType.Custom:
                    SSLCertificateTypeCustomRadioButton.IsChecked = true;
                    break;
                case ApplicationAPIUtils.eCertificateType.Ignore:
                    IgnoreSSLCertification.IsChecked = true;
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
            switch (mApplicationAPIModel.RequestContentType)
            {
                case ApplicationAPIUtils.eRequestContentType.JSon:
                    RequestBodyTypePanel.Visibility = Visibility.Visible;
                    CheckRequestBodySelection();
                    break;
                case ApplicationAPIUtils.eRequestContentType.TextPlain:
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                    CheckRequestBodySelection();
                    break;
                case ApplicationAPIUtils.eRequestContentType.XML:
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                    CheckRequestBodySelection();
                    break;
                case ApplicationAPIUtils.eRequestContentType.XwwwFormUrlEncoded:
                    BodyInputGridPannel.Visibility = System.Windows.Visibility.Collapsed;
                    FreeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                    RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                    FormDataGridPanel.Visibility = System.Windows.Visibility.Visible;
                    RefreshRequestKeyValuesGrid();
                    break;
                case ApplicationAPIUtils.eRequestContentType.FormData:
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
            {
                mApplicationAPIModel.APIModelBodyKeyValueHeaders.Clear();
            }

            if ((mApplicationAPIModel.APIType == ApplicationAPIUtils.eWebApiType.REST) && mApplicationAPIModel.RequestContentType == ApplicationAPIUtils.eRequestContentType.XwwwFormUrlEncoded)
            {
                //switch combobox   & browse button off 
                FormDataGrid.ChangeGridView("UrlEncoded");
            }
            else if ((mApplicationAPIModel.APIType == ApplicationAPIUtils.eWebApiType.REST) && mApplicationAPIModel.RequestContentType == ApplicationAPIUtils.eRequestContentType.FormData)
            {
                //switch combobox  & browse button on
                FormDataGrid.ChangeGridView("FormData");
            }
        }

        private void SetHTTPHeadersGrid()
        {
            bool isFieldReadOnly = (mPageViewMode is Ginger.General.eRIPageViewMode.View or General.eRIPageViewMode.ViewAndExecute);

            HttpHeadersGrid.Title = "Request Headers";
            HttpHeadersGrid.SetTitleStyle((Style)TryFindResource("@ucGridTitleLightStyle"));

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(APIModelKeyValue.Param), Header = "Header", ReadOnly = isFieldReadOnly, WidthWeight = 100 },
                new GridColView() { Field = nameof(APIModelKeyValue.Value), Header = "Value", ReadOnly = isFieldReadOnly, WidthWeight = 100 },
            ]
            };

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
            APIModelBodyKeyValue Wa = new APIModelBodyKeyValue
            {
                ValueType = APIModelBodyKeyValue.eValueType.Text
            };
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
            if ((mApplicationAPIModel.APIType == ApplicationAPIUtils.eWebApiType.REST) && mApplicationAPIModel.RequestContentType == ApplicationAPIUtils.eRequestContentType.XwwwFormUrlEncoded)
            {
                FreeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestBodyTypePanel.Visibility = System.Windows.Visibility.Collapsed;
                FormDataGridPanel.Visibility = System.Windows.Visibility.Visible;
            }
            if ((mApplicationAPIModel.APIType == ApplicationAPIUtils.eWebApiType.REST) && mApplicationAPIModel.RequestContentType == ApplicationAPIUtils.eRequestContentType.FormData)
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
            if (mApplicationAPIModel.CertificateType == ApplicationAPIUtils.eCertificateType.Custom)
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
            {
                return;
            }

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
            {
                return;
            }

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
                            {
                                TemplateFileNameFileBrowser.Text = string.Empty;
                            }

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
                            {
                                RequestBodyTextBox.Text = string.Empty;
                            }

                            FreeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                            RequestBodyTypePanel.Visibility = System.Windows.Visibility.Visible;
                            BodyInputGridPannel.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        break;

                    default:
                        if (mApplicationAPIModel.RequestContentType is ApplicationAPIUtils.eRequestContentType.XwwwFormUrlEncoded or ApplicationAPIUtils.eRequestContentType.FormData)
                        {
                            if (!String.IsNullOrEmpty(mApplicationAPIModel.TemplateFileNameFileBrowser))
                            {
                                RequestBodyTextBox.Text = string.Empty;
                            }

                            if (!String.IsNullOrEmpty(mApplicationAPIModel.TemplateFileNameFileBrowser))
                            {
                                TemplateFileNameFileBrowser.Text = string.Empty;
                            }

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
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*",
                FilterIndex = 1
            };
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (DoNotImportRequestFile.IsChecked == true)
                {

                    string NewPath = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, "Documents", Path.GetFileName(dlg.FileName));
                    if (!Directory.Exists(Path.GetDirectoryName(NewPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(NewPath));
                    }

                    File.Copy(dlg.FileName, NewPath, true);
                    TemplateFileNameFileBrowser.Text = NewPath.Replace(WorkSpace.Instance.SolutionRepository.SolutionFolder, "~\\");
                }
                else
                {
                    TemplateFileNameFileBrowser.Text = dlg.FileName;
                }
            }
        }

        private void CertificatePathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*",
                FilterIndex = 1
            };
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (DoNotCertificateImportFile.IsChecked == true)
                {
                    string NewPath = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, "Documents", Path.GetFileName(dlg.FileName));
                    if (!Directory.Exists(Path.GetDirectoryName(NewPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(NewPath));
                    }

                    File.Copy(dlg.FileName, NewPath, true);
                    CertificatePath.Text = NewPath.Replace(WorkSpace.Instance.SolutionRepository.SolutionFolder, "~\\");
                }
                else
                {
                    CertificatePath.Text = dlg.FileName;
                }
            }
        }

        private void SetCustomCredPanel()
        {
            if (mApplicationAPIModel.NetworkCredentials == ApplicationAPIUtils.eNetworkCredentials.Custom)
            {
                SP_CustomCreds.Visibility = Visibility.Visible;
            }
            else
            {
                SP_CustomCreds.Visibility = Visibility.Collapsed;
            }
        }

        private void SetCertificatePanel()
        {
            if (mApplicationAPIModel.CertificateType == ApplicationAPIUtils.eCertificateType.Custom)
            {
                CertificateStackPanel.Visibility = Visibility.Visible;
            }
            else
            {
                CertificateStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void CertificateSelection_Changed(object sender, RoutedEventArgs e)
        {
            if (sender == null || BodySelection == null)
            {
                return;
            }

            RadioButton rBtn = sender as RadioButton;
            if ((bool)rBtn.IsChecked)
            {
                switch (rBtn.Name)
                {
                    case "SSLCertificateTypeAllCertificatesRadioButton":
                        mApplicationAPIModel.CertificateType = ApplicationAPIUtils.eCertificateType.AllSSL;
                        break;
                    case "SSLCertificateTypeCustomRadioButton":
                        mApplicationAPIModel.CertificateType = ApplicationAPIUtils.eCertificateType.Custom;
                        break;
                    case "IgnoreSSLCertification":
                        mApplicationAPIModel.CertificateType = ApplicationAPIUtils.eCertificateType.Ignore;
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
            GridViewDef FormDataView = new GridViewDef("FormData")
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(APIModelBodyKeyValue.Param), Header = "Key", WidthWeight = 100 },
            ]
            };
            List<ComboEnumItem> valueTypes = GingerCore.General.GetEnumValuesForCombo(typeof(APIModelBodyKeyValue.eValueType));
            FormDataView.GridColsView.Add(new GridColView() { Field = APIModelBodyKeyValue.Fields.ValueType, Header = "Value Type", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = valueTypes });
            FormDataView.GridColsView.Add(new GridColView() { Field = nameof(APIModelBodyKeyValue.Value), Header = "Value/File Path", WidthWeight = 100 });

            //Define URLEncoded GridView
            GridViewDef UrlEncodedView = new GridViewDef("UrlEncoded")
            {
                GridColsView = []
            };
            ObservableList<GridColView> UrlViewCols = [];
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

            if ((mApplicationAPIModel.APIType == ApplicationAPIUtils.eWebApiType.REST) && mApplicationAPIModel.RequestContentType == ApplicationAPIUtils.eRequestContentType.XwwwFormUrlEncoded)
            {
                //switch combobox   & browse button off 
                FormDataGrid.ChangeGridView("UrlEncoded");
            }
            else if ((mApplicationAPIModel.APIType == ApplicationAPIUtils.eWebApiType.REST) && mApplicationAPIModel.RequestContentType == ApplicationAPIUtils.eRequestContentType.FormData)
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
                        {
                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (APIModelTabs.SelectedItem == tab)
                                {
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                }
                                else
                                {
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$PrimaryColor_Black");
                                } ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
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
            {
                Auth_Creds.Visibility = Visibility.Visible;
            }
            else if (mApplicationAPIModel.AuthorizationType == ApplicationAPIUtils.eAuthType.NoAuthentication)
            {
                Auth_Creds.Visibility = Visibility.Collapsed;
            }
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
                string parameterName = SoapSecurityContent.ElementAt(i);
                bool isParamsListEmpty = modelParamsPage.ParamsList.Count == 0;
                bool isParameterAlreadyAdded = modelParamsPage.ParamsList.Any(modelParam => modelParam.PlaceHolder.Equals(parameterName));

                if (isParamsListEmpty || !isParameterAlreadyAdded)
                {
                    AppModelParameter newAppModelParam = new AppModelParameter() { PlaceHolder = parameterName };

                    OptionalValue? optionalValue = GetOptionalValueFor(parameterName);
                    if (optionalValue != null)
                    {
                        newAppModelParam.OptionalValuesList.Add(optionalValue);
                    }

                    modelParamsPage.ParamsList.Add(newAppModelParam);
                }
            }
        }

        private static OptionalValue? GetOptionalValueFor(string parameterName)
        {
            if (parameterName.Equals("{GETUTCTIMESTAMP}"))
            {
                return new OptionalValue() { IsDefault = true, Value = "{Function Fun=GetUTCTimeStamp()}" };
            }
            else if (parameterName.Equals("{GET_HASHED_WSSECPASSWORD}"))
            {
                return new OptionalValue() { IsDefault = true, Value = @"{Function Fun=GenerateHashCode(""{Function Fun=GetGUID()}{WSSECPASSWORD}"")}=" };
            }
            else if (parameterName.Equals("{GET_HASH_CODE}"))
            {
                return new OptionalValue() { IsDefault = true, Value = "{Function Fun=GetHashCode({Function Fun=GetGUID()})}" };
            }

            return null;
        }

        public enum eEditMode
        {
            View = 0,
            Design = 1,
            FindAndReplace = 2,
            Edit = 3,
            //Global = 4
        }

        public eEditMode editMode { get; set; }

        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false, eEditMode e = eEditMode.Design, Window parentWindow = null)
        {
            mApplicationAPIModel.StartDirtyTracking();
            //changed the style to be free - since many other windows get stuck and doesn't show
            // Need to find a solution if 2 windows show as Dialog...
            string title = "";
            this.Width = 1100;
            this.Height = 800;

            editMode = e;
            ObservableList<Button> winButtons = [];
            switch (editMode)
            {
                case eEditMode.Design:
                    Button okBtn2 = new Button
                    {
                        Content = "Ok"
                    };
                    okBtn2.Click += new RoutedEventHandler(okBtn_Click);
                    Button undoBtn2 = new Button
                    {
                        Content = "Undo & Close"
                    };
                    undoBtn2.Click += new RoutedEventHandler(undoBtn_Click);
                    winButtons.Add(okBtn2);
                    winButtons.Add(undoBtn2);
                    break;

                case eEditMode.View:
                    title = "View " + mApplicationAPIModel.Name + " API Model";
                    Button okBtnView = new Button
                    {
                        Content = "Ok"
                    };
                    okBtnView.Click += new RoutedEventHandler(okBtn_Click);
                    winButtons.Add(okBtnView);
                    break;
                case eEditMode.FindAndReplace:
                    title = "Edit " + mApplicationAPIModel.Name + " API Model";
                    mApplicationAPIModel.SaveBackup();
                    Button saveBtnAnalyzer = new Button
                    {
                        Content = "Save"
                    };
                    saveBtnAnalyzer.Click += new RoutedEventHandler(saveBtn_Click);
                    Button undoBtnAnalyzer = new Button
                    {
                        Content = "Undo & Close"
                    };
                    undoBtnAnalyzer.Click += new RoutedEventHandler(undoBtn_Click);
                    winButtons.Add(undoBtnAnalyzer);
                    winButtons.Add(saveBtnAnalyzer);
                    break;
                case eEditMode.Edit:
                    title = mApplicationAPIModel.Name + " Edit Page";
                    mApplicationAPIModel.SaveBackup();

                    Button saveBtn = new Button() { Content = "Save" };
                    saveBtn.Click += saveBtn_Click;

                    Button undoChangesBtn = new Button() { Content = "Undo & Close" };
                    undoChangesBtn.Click += UndoChangesBtn_Click;

                    winButtons.Add(saveBtn);
                    winButtons.Add(undoChangesBtn);

                    break;
            }
            if (parentWindow == null)
            {
                parentWindow = App.MainWindow;
            }

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, parentWindow, windowStyle, title, this, winButtons, false, string.Empty, CloseWinClicked, startupLocationWithOffset: startupLocationWithOffset);
            return saveWasDone;
        }

        private void UndoChangesBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
            mApplicationAPIModel.SetDirtyStatusToNoChange();
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

        public void xRealAPIRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mApplicationAPIModel.UseLiveAPI = true;
            xRealAPIRadioButton.IsChecked = true;
        }

        public void xMockAPIRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mApplicationAPIModel.UseLiveAPI = false;
            xMockAPIRadioButton.IsChecked = true;
        }


        private async void WireMockTemplatePage_GridUpdated(object sender, EventArgs e)
        {
            await UpdateWireMockTemplateTabHeader();
        }

        public class BoolInverterConverter : IValueConverter
        {
            #region IValueConverter Members

            public object Convert(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
            {
                if (value is bool)
                {
                    return !(bool)value;
                }
                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
            {
                if (value is bool)
                {
                    return !(bool)value;
                }
                return value;
            }

            #endregion
        }
    }
}
