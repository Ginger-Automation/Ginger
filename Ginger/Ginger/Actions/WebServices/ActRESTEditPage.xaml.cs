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
            URLUCValueExpression.Init(mActREST.EndPointURL);
            URLDomainUCValueExpression.Init(mActREST.URLDomain, ActInputValue.Fields.Value);
            URLPasswordUCValueExpression.Init(mActREST.URLPass, ActInputValue.Fields.Value);
            URLUserUCValueExpression.Init(mActREST.URLUser, ActInputValue.Fields.Value);
            RequestBodyUCValueExpression.Init(mActREST.RequestBody);
            RequestRespondXmlSaveTextBox.Init(mActREST.SaveRequestResponseFolderPath);

            TemplateFileNameFileBrowser.Init(mActREST.TemplateFile); 
            App.FillComboFromEnumVal(RequestTypeComboBox, mActREST.RequestType);
            App.ObjFieldBinding(RequestTypeComboBox, ComboBox.TextProperty, mActREST,  ActREST.Fields.RequestType);

            //httpversion content type
            App.FillComboFromEnumVal(HttpVersioncombobox, mActREST.ReqHttpVersion);
            App.ObjFieldBinding(HttpVersioncombobox, ComboBox.SelectedValueProperty, mActREST, ActREST.Fields.ReqHttpVersion);

            //Request content type
            App.FillComboFromEnumVal(ContentTypeComboBox, mActREST.ContentType);
            App.ObjFieldBinding(ContentTypeComboBox, ComboBox.SelectedValueProperty, mActREST, ActREST.Fields.ContentType);
            
            //Response Content Type
            App.FillComboFromEnumVal(ResponseTypeComboBox, mActREST.ResponseContentType);
            App.ObjFieldBinding(ResponseTypeComboBox, ComboBox.SelectedValueProperty, mActREST, ActREST.Fields.ResponseContentType);
            //Security Type
            App.FillComboFromEnumVal(SecurityTypeComboBox, mActREST.SecurityType);
            App.ObjFieldBinding(SecurityTypeComboBox, ComboBox.SelectedValueProperty, mActREST, ActREST.Fields.SecurityType);

            //Cookie Mode 
            App.FillComboFromEnumVal(CookieMode, mActREST.CookieMode);
            App.ObjFieldBinding(CookieMode, ComboBox.SelectedValueProperty, mActREST, ActREST.Fields.CookieMode);

            App.ObjFieldBinding(restRequest, CheckBox.IsCheckedProperty, mActREST, ActREST.Fields.RestRequestSave);
            App.ObjFieldBinding(restResponse, CheckBox.IsCheckedProperty, mActREST, ActREST.Fields.RestResponseSave);            
            App.ObjFieldBinding(templateFileRadioBtn, RadioButton.IsCheckedProperty, mActREST,ActREST.Fields.UseRequestBody);
            App.ObjFieldBinding(freeBodyRadioBtn, RadioButton.IsCheckedProperty, mActREST, ActREST.Fields.UseTemplateFile);
            App.ObjFieldBinding(RestFailActionOnBadRespose, CheckBox.IsCheckedProperty, mActREST, ActREST.Fields.DoNotFailActionOnBadRespose);
            App.ObjFieldBinding(AcceptAllSSLCertificate, CheckBox.IsCheckedProperty, mActREST, ActREST.Fields.AcceptAllSSLCertificate);

            GingerCore.General.ObjFieldBinding(xUseLegacyJSONParsingCheckBox, CheckBox.IsCheckedProperty, mActREST, ActREST.Fields.UseLegacyJSONParsing);

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
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, ActInputValue.Fields.Value);
            VEEW.ShowAsWindow();
        }

        private void HttpHeadersInputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)HttpHeadersGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage (AIV, ActInputValue.Fields.Value);
            VEEW.ShowAsWindow ();
        }

        private void SetDynamicGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = ActInputValue.Fields.Param, Header = "Locator", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = ActInputValue.Fields.Value, Header = "Replace With", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["InputValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = ActInputValue.Fields.ValueForDriver, Header = "Replace With Value For Driver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

            DynamicElementsGrid.SetAllColumnsDefaultView(view);
            DynamicElementsGrid.InitViewItems();

            DynamicElementsGrid.DataSourceList = mActREST.DynamicElements;
        }

        private void SetHeadersGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = ActInputValue.Fields.Param, Header = "Header", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = ActInputValue.Fields.Value, Header = "Value", WidthWeight = 150 });
            view.GridColsView.Add (new GridColView () { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["HttpHeadersValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = ActInputValue.Fields.ValueForDriver, Header = "Replace With Value For Driver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

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
            string SolutionFolder =  WorkSpace.UserProfile.Solution.Folder.ToUpper();

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
