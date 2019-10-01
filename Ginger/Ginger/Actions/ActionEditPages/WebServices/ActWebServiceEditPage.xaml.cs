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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.Actions.WebServices
{
    /// <summary>
    /// Interaction logic for ActWebServiceEditPage.xaml
    /// </summary>
    public partial class ActWebServiceEditPage : Page
    {
        private ActWebService mAct;
        public ActWebServiceEditPage(ActWebService act)
        {
            InitializeComponent();
            mAct = act;
            Bind();
            if (mAct.URLUser.Value != "" && mAct.URLUser.Value != null)
            {
                CustomNetworkCreds.IsChecked = true;
            }
        }

        public void Bind()
        {
            URLUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.URL, nameof(ActInputValue.Value));
            SoapActionUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.SOAPAction, nameof(ActInputValue.Value));
            XMLFileNameUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.XMLfileName, nameof(ActInputValue.Value));
            URLDomainUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.URLDomain, nameof(ActInputValue.Value));
            URLPasswordUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.URLPass, nameof(ActInputValue.Value));
            URLUserUCValueExpression.Init(Context.GetAsContext(mAct.Context), mAct.URLUser, nameof(ActInputValue.Value));
            SetDynamicGrid();
            DynamicXMLElementsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddDynamicXMLelement));          
        }

        private void AddDynamicXMLelement(object sender, RoutedEventArgs e)
        {
            string PlaceHolderName = "{Place Holder " + (mAct.DynamicXMLElements.Count + 1) + "}";
            mAct.DynamicXMLElements.Add(new ActInputValue() { Param = PlaceHolderName });
        }

        private void InputGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)DynamicXMLElementsGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), Context.GetAsContext(mAct.Context));
            VEEW.ShowAsWindow();
        }

        private void SetDynamicGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), Header="Locator" ,WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), Header="Replace With" ,WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["InputValueExpressionButton"] });            
            view.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.ValueForDriver), Header = "Replace With Value For Driver", WidthWeight = 150, BindingMode = BindingMode.OneWay });

            DynamicXMLElementsGrid.SetAllColumnsDefaultView(view);
            DynamicXMLElementsGrid.InitViewItems();

           DynamicXMLElementsGrid.DataSourceList = mAct.DynamicXMLElements;
        }

        private void CustomNetworkCreds_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (CustomNetworkCreds == null || DefaultNetworkCreds == null) return;
            if (cb.IsChecked == true)
            {
                DefaultNetworkCreds.IsChecked = false;
                SP_CustomCreds.Visibility = Visibility.Visible;
                RowCreds.Height = new GridLength(185);
                URLDomainUCValueExpression.ValueTextBox.Text = mAct.URLDomain.Value;
                URLPasswordUCValueExpression.ValueTextBox.Text = mAct.URLPass.Value;
                URLUserUCValueExpression.ValueTextBox.Text = mAct.URLUser.Value;
            }
            else if (DefaultNetworkCreds.IsChecked == false)
            {
                DefaultNetworkCreds.IsChecked = true;
                SP_CustomCreds.Visibility = Visibility.Collapsed;
                URLDomainUCValueExpression.ValueTextBox.Text = "";
                URLPasswordUCValueExpression.ValueTextBox.Text = "";
                URLUserUCValueExpression.ValueTextBox.Text = "";
                RowCreds.Height = new GridLength(31);
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
                RowCreds.Height = new GridLength(31);
                URLDomainUCValueExpression.ValueTextBox.Text = "";
                URLPasswordUCValueExpression.ValueTextBox.Text = "";
                URLUserUCValueExpression.ValueTextBox.Text = "";
            }
            else if (CustomNetworkCreds.IsChecked == false)
            {
                CustomNetworkCreds.IsChecked = true;
                SP_CustomCreds.Visibility = Visibility.Visible;
                URLDomainUCValueExpression.ValueTextBox.Text = mAct.URLDomain.Value;
                URLPasswordUCValueExpression.ValueTextBox.Text = mAct.URLPass.Value;
                URLUserUCValueExpression.ValueTextBox.Text = mAct.URLUser.Value;
                RowCreds.Height = new GridLength(185);
            }
        }
        
        private void BrowseXmlPathButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.DefaultExt = "*.xml";
            string SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // replace Absolute file name with relative to solution
                string FileName = dlg.FileName.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }

                XMLFileNameUCValueExpression.ValueTextBox.Text = FileName;
            }
        }
    }
}
