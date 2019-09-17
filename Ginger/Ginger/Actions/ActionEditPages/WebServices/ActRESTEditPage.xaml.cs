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

using Amdocs.Ginger.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GingerCore.Actions.REST;
using Ginger.UserControls;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.Actions.WebServices
{
    /// <summary>
    /// Interaction logic for ActRestEditPage.xaml
    /// </summary>
    public partial class ActRESTEditPage : Page
    {
        ActREST mActREST;

        public ActRESTEditPage(ActREST a)
        {
            InitializeComponent();

            mActREST = a;
            URLUCValueExpression.Init(Context.GetAsContext(mActREST.Context), mActREST.EndPointURL);
            URLDomainUCValueExpression.Init(Context.GetAsContext(mActREST.Context), mActREST.URLDomain, nameof(ActInputValue.Value));
            URLPasswordUCValueExpression.Init(Context.GetAsContext(mActREST.Context), mActREST.URLPass, nameof(ActInputValue.Value));
            URLUserUCValueExpression.Init(Context.GetAsContext(mActREST.Context), mActREST.URLUser, nameof(ActInputValue.Value));
            RequestBodyUCValueExpression.Init(Context.GetAsContext(mActREST.Context), mActREST.RequestBody);
            RequestRespondXmlSaveTextBox.Init(Context.GetAsContext(mActREST.Context), mActREST.SaveRequestResponseFolderPath);

            TemplateFileNameFileBrowser.Init(Context.GetAsContext(mActREST.Context), mActREST.TemplateFile); 
            GingerCore.General.FillComboFromEnumObj(RequestTypeComboBox, mActREST.RequestType);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RequestTypeComboBox, ComboBox.TextProperty, mActREST,  ActREST.Fields.RequestType);

            //httpversion content type
            GingerCore.General.FillComboFromEnumObj(HttpVersioncombobox, mActREST.ReqHttpVersion);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(HttpVersioncombobox, ComboBox.SelectedValueProperty, mActREST, ActREST.Fields.ReqHttpVersion);

            //Request content type
            GingerCore.General.FillComboFromEnumObj(ContentTypeComboBox, mActREST.ContentType);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ContentTypeComboBox, ComboBox.SelectedValueProperty, mActREST, ActREST.Fields.ContentType);
            
            //Response Content Type
            GingerCore.General.FillComboFromEnumObj(ResponseTypeComboBox, mActREST.ResponseContentType);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ResponseTypeComboBox, ComboBox.SelectedValueProperty, mActREST, ActREST.Fields.ResponseContentType);
            //Security Type
            GingerCore.General.FillComboFromEnumObj(SecurityTypeComboBox, mActREST.SecurityType);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SecurityTypeComboBox, ComboBox.SelectedValueProperty, mActREST, ActREST.Fields.SecurityType);

            //Cookie Mode 
            GingerCore.General.FillComboFromEnumObj(CookieMode, mActREST.CookieMode);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(CookieMode, ComboBox.SelectedValueProperty, mActREST, ActREST.Fields.CookieMode);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(restRequest, CheckBox.IsCheckedProperty, mActREST, ActREST.Fields.RestRequestSave);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(restResponse, CheckBox.IsCheckedProperty, mActREST, ActREST.Fields.RestResponseSave);            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(templateFileRadioBtn, RadioButton.IsCheckedProperty, mActREST,ActREST.Fields.UseRequestBody);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(freeBodyRadioBtn, RadioButton.IsCheckedProperty, mActREST, ActREST.Fields.UseTemplateFile);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(RestFailActionOnBadRespose, CheckBox.IsCheckedProperty, mActREST, ActREST.Fields.DoNotFailActionOnBadRespose);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AcceptAllSSLCertificate, CheckBox.IsCheckedProperty, mActREST, ActREST.Fields.AcceptAllSSLCertificate);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xUseLegacyJSONParsingCheckBox, CheckBox.IsCheckedProperty, mActREST, ActREST.Fields.UseLegacyJSONParsing);

            SetDynamicGrid();

            DynamicElementsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddDynamicXMLelement));

            SetHeadersGrid();
            HttpHeadersGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddHttpHeader));
        }

        private void AddDynamicXMLelement(object sender, RoutedEventArgs e)
        {
            string PlaceHolderName = "{Place Holder " + (mActREST.DynamicElements.Count + 1) + "}";
            mActREST.DynamicElements.Add(new ActInputValue() { Param = PlaceHolderName });
        }

        private void AddHttpHeader(object sender, RoutedEventArgs e)
        {
            string PlaceHolderName = "{Place Holder " + (mActREST.HttpHeaders.Count + 1) + "}";
            mActREST.HttpHeaders.Add(new ActInputValue() { Param = PlaceHolderName });
        }

        private void InputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)DynamicElementsGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), Context.GetAsContext(mActREST.Context));
            VEEW.ShowAsWindow();
        }

        private void HttpHeadersInputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)HttpHeadersGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage (AIV, nameof(ActInputValue.Value), Context.GetAsContext(mActREST.Context));
            VEEW.ShowAsWindow ();
        }

        private void SetDynamicGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), Header = "Locator", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), Header = "Replace With", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["InputValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = "Replace With Value For Driver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

            DynamicElementsGrid.SetAllColumnsDefaultView(view);
            DynamicElementsGrid.InitViewItems();

            DynamicElementsGrid.DataSourceList = mActREST.DynamicElements;
        }

        private void SetHeadersGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), Header = "Header", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), Header = "Value", WidthWeight = 150 });
            view.GridColsView.Add (new GridColView () { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["HttpHeadersValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = "Replace With Value For Driver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

            HttpHeadersGrid.SetAllColumnsDefaultView(view);
            HttpHeadersGrid.InitViewItems();

            HttpHeadersGrid.DataSourceList = mActREST.HttpHeaders;
        }

        private void CustomNetworkCreds_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (CustomNetworkCreds == null || DefaultNetworkCreds == null) return;
            if (cb.IsChecked == true)
            {
                DefaultNetworkCreds.IsChecked = false;
                SP_CustomCreds.Visibility = Visibility.Visible;
                RowCreds.Height = new GridLength(190);
                URLDomainUCValueExpression.ValueTextBox.Text = mActREST.URLDomain.Value;
                URLPasswordUCValueExpression.ValueTextBox.Text = mActREST.URLPass.Value;
                URLUserUCValueExpression.ValueTextBox.Text = mActREST.URLUser.Value;
            }
            else if (DefaultNetworkCreds.IsChecked == false)
            {
                DefaultNetworkCreds.IsChecked = true;
                SP_CustomCreds.Visibility = Visibility.Collapsed;
                URLDomainUCValueExpression.ValueTextBox.Text = "";
                URLPasswordUCValueExpression.ValueTextBox.Text = "";
                URLUserUCValueExpression.ValueTextBox.Text = "";
                RowCreds.Height = new GridLength(35);
            }
        }

        private void DefaultNetworkCreds_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (CustomNetworkCreds == null || DefaultNetworkCreds == null) return;
            if (cb.IsChecked == true)
            {
                CustomNetworkCreds.IsChecked = false;
                SP_CustomCreds.Visibility = Visibility.Collapsed;
                RowCreds.Height = new GridLength(35);
                URLDomainUCValueExpression.ValueTextBox.Text = "";
                URLPasswordUCValueExpression.ValueTextBox.Text = "";
                URLUserUCValueExpression.ValueTextBox.Text = "";
            }
            else if (CustomNetworkCreds.IsChecked == false)
            {
                CustomNetworkCreds.IsChecked = true;
                SP_CustomCreds.Visibility = Visibility.Visible;
                URLDomainUCValueExpression.ValueTextBox.Text = mActREST.URLDomain.Value;
                URLPasswordUCValueExpression.ValueTextBox.Text = mActREST.URLPass.Value;
                URLUserUCValueExpression.ValueTextBox.Text = mActREST.URLUser.Value;
                RowCreds.Height = new GridLength(190);
            }
        }

        private void RequestBodyType_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (freeBodyRadioBtn == null || templateFileRadioBtn == null) return;
            if (freeBodyRadioBtn.IsChecked == true)
            {
                FreeStackPanel.Visibility = System.Windows.Visibility.Visible;
                if (mActREST.TemplateFile.Value != null)
                    TemplateFileNameFileBrowser.ValueTextBox.Text = string.Empty;
                TemplateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                TemplateStackPanel.Visibility = System.Windows.Visibility.Visible;
                if (mActREST.RequestBody.Value != null)
                    RequestBodyUCValueExpression.ValueTextBox.Text = string.Empty;
                FreeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ResetPanelVisibility()
        {
            if (restRequest == null || restResponse == null) return;
            if (restRequest.IsChecked == true || restResponse.IsChecked == true)
            {
                requestResponseStackPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                requestResponseStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                RequestRespondXmlSaveTextBox.ValueTextBox.Text = string.Empty;
            }
        }

        private void SaveXml_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            ResetPanelVisibility();        
        }              

        private void BrowseTemplateFileButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            string SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();

            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // replace Absolute file name with relative to solution
                string FileName = dlg.FileName.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }

                TemplateFileNameFileBrowser.ValueTextBox.Text = FileName;
            }                   
        }

        private void BrowseRequestResponseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();

            folderDlg.ShowNewFolderButton = true;
            System.Windows.Forms.DialogResult result = folderDlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                RequestRespondXmlSaveTextBox.ValueTextBox.Text = folderDlg.SelectedPath;
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        } 
    }
}
