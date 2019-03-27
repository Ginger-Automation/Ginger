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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GingerCore;
using Ginger.UserControls;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using GingerCore.Drivers;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.Reports
{
    /// <summary>
    /// Interaction logic for ExecutionResultsConfiguration.xaml
    /// </summary>
    public partial class HTMLReportTemplatePage : Page
    {
        GenericWindow _pageGenericWin = null;
        private bool _existingTemplatePage = false;
        
        private HTMLReportTemplatePage mInstance;
        private HTMLReportConfiguration _HTMLReportConfiguration = new HTMLReportConfiguration();
        private HTMLReportConfiguration _newHTMLReportConfiguration = null;

        private Cursor _previousCursor;
        WebBrowserPage WBP;
        public System.Windows.Controls.WebBrowser browser;

        private string mPreviewDummyReportDataPath = string.Empty;
        private string mPreviewDummyReportPath = string.Empty;

        public HTMLReportConfiguration newHTMLReportConfiguration
        {
            get
            {
                return _newHTMLReportConfiguration;
            }
        }

        public HTMLReportTemplatePage Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new HTMLReportTemplatePage();

                return mInstance;
            }
        }

        public HTMLReportTemplatePage()
        {
            _HTMLReportConfiguration = new HTMLReportConfiguration("");
            InitializeComponent();
            SetControlsNewTemplate();
            SetDefaultLogoImage();
            SetHTMLReportsConfigFieldsGridsView();
            SetHTMLReportsConfigFieldsGridsData(_HTMLReportConfiguration);
        }

        public HTMLReportTemplatePage(HTMLReportConfiguration HTMLReportConfiguration)
        {
            InitializeComponent();

            _existingTemplatePage = true;
            _HTMLReportConfiguration = EnchancingLoadedFieldsWithDataAndValidating(HTMLReportConfiguration);
            _HTMLReportConfiguration.PropertyChanged += _HTMLReportConfiguration_PropertyChanged;
            SetControls();
            SetLoadedLogoImage();
            SetHTMLReportsConfigFieldsGridsView();
            SetHTMLReportsConfigFieldsGridsData(_HTMLReportConfiguration);
        }

       

        private void SetHTMLReportsConfigFieldsGridsView()
        {
            GridViewDef viewSummaryView = new GridViewDef(GridViewDef.DefaultViewName);
            viewSummaryView.GridColsView = new ObservableList<GridColView>();
            viewSummaryView.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldName, WidthWeight = 65, ReadOnly = true, Header = "Field Name" });
            viewSummaryView.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldType, WidthWeight = 20, ReadOnly = true, Header = "Field Type", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            viewSummaryView.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSelected, WidthWeight = 20, Header = "Selected", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdSummaryViewField.Resources["FieldIsAddedToReportSummaryView"] });
            viewSummaryView.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSectionCollapsed, WidthWeight = 20, Header = "Collapsed", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdSummaryViewField.Resources["SectionCollapsedSummaryView"] });            
            grdSummaryViewFields.SetAllColumnsDefaultView(viewSummaryView);
            grdSummaryViewFields.InitViewItems();

            GridViewDef viewGingers = new GridViewDef(GridViewDef.DefaultViewName);
            viewGingers.GridColsView = new ObservableList<GridColView>();
            viewGingers.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldName, WidthWeight = 65, ReadOnly = true, Header = "Field Name" });
            viewGingers.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldType, WidthWeight = 20, ReadOnly = true, Header = "Field Type", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            viewGingers.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSelected, WidthWeight = 20, Header = "Selected", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdGingersField.Resources["FieldIsAddedToReportGingers"] });
            viewGingers.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSectionCollapsed, WidthWeight = 20, Header = "Collapsed", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdGingersField.Resources["SectionCollapsedGingers"] });
            grdGingersFields.SetAllColumnsDefaultView(viewGingers);
            grdGingersFields.InitViewItems();

            GridViewDef viewBusinessFlow = new GridViewDef(GridViewDef.DefaultViewName);
            viewBusinessFlow.GridColsView = new ObservableList<GridColView>();
            viewBusinessFlow.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldName, WidthWeight = 65, ReadOnly = true, Header = "Field Name" });
            viewBusinessFlow.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldType, WidthWeight = 20, ReadOnly = true, Header = "Field Type", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            viewBusinessFlow.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSelected, WidthWeight = 20, Header = "Selected", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdBusinessFlowField.Resources["FieldIsAddedToReportBFs"] });
            viewBusinessFlow.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSectionCollapsed, WidthWeight = 20, Header = "Collapsed", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdBusinessFlowField.Resources["SectionCollapsedBusinessFlow"] });
            grdBusinessFlowFields.SetAllColumnsDefaultView(viewBusinessFlow);
            grdBusinessFlowFields.InitViewItems();

            GridViewDef viewActivities = new GridViewDef(GridViewDef.DefaultViewName);
            viewActivities.GridColsView = new ObservableList<GridColView>();
            viewActivities.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldName, WidthWeight = 65, ReadOnly = true, Header = "Field Name" });
            viewActivities.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldType, WidthWeight = 20, ReadOnly = true, Header = "Field Type", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            viewActivities.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSelected, WidthWeight = 20, Header = "Selected", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdActivitiesField.Resources["FieldIsAddedToReportActivities"] });
            viewActivities.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSectionCollapsed, WidthWeight = 20, Header = "Collapsed", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdActivitiesField.Resources["SectionCollapsedActivities"] });
            grdActivitiesFields.SetAllColumnsDefaultView(viewActivities);
            grdActivitiesFields.InitViewItems();

            GridViewDef viewActivityGroups = new GridViewDef(GridViewDef.DefaultViewName);
            viewActivityGroups.GridColsView = new ObservableList<GridColView>();
            viewActivityGroups.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldName, WidthWeight = 65, ReadOnly = true, Header = "Field Name" });
            viewActivityGroups.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldType, WidthWeight = 20, ReadOnly = true, Header = "Field Type", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            viewActivityGroups.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSelected, WidthWeight = 20, Header = "Selected", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdActivityGroupsField.Resources["FieldIsAddedToReportActivityGroups"] });
            viewActivityGroups.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSectionCollapsed, WidthWeight = 20, Header = "Collapsed", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdActivityGroupsField.Resources["SectionCollapsedActivityGroups"] });
            grdActivityGroupsFields.SetAllColumnsDefaultView(viewActivityGroups);
            grdActivityGroupsFields.InitViewItems();

            GridViewDef viewActions = new GridViewDef(GridViewDef.DefaultViewName);
            viewActions.GridColsView = new ObservableList<GridColView>();
            viewActions.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldName, WidthWeight = 65, ReadOnly = true, Header = "Field Name" });
            viewActions.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldType, WidthWeight = 20, ReadOnly = true, Header = "Field Type", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            viewActions.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSelected, WidthWeight = 20, Header = "Selected", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdActionsField.Resources["FieldIsAddedToReportActions"] });
            viewActions.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSectionCollapsed, WidthWeight = 20, Header = "Collapsed", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdActionsField.Resources["SectionCollapsedActions"] });
            grdActionsFields.SetAllColumnsDefaultView(viewActions);
            grdActionsFields.InitViewItems();

            GridViewDef viewEmailSummaryView = new GridViewDef(GridViewDef.DefaultViewName);
            viewEmailSummaryView.GridColsView = new ObservableList<GridColView>();
            viewEmailSummaryView.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldName, WidthWeight = 65, ReadOnly = true, Header = "Field Name" });
            viewEmailSummaryView.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.FieldType, WidthWeight = 20, ReadOnly = true, Header = "Field Type", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            viewEmailSummaryView.GridColsView.Add(new GridColView() { Field = HTMLReportConfigFieldToSelect.Fields.IsSelected, WidthWeight = 20, Header = "Selected", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.grdEmailSummaryViewField.Resources["FieldIsAddedToEmailReportSummaryView"] });
            grdEmailSummaryViewFields.SetAllColumnsDefaultView(viewEmailSummaryView);
            grdEmailSummaryViewFields.InitViewItems();
        }      


        private void SetHTMLReportsConfigFieldsGridsData(HTMLReportConfiguration HTMLReportConfiguration)
        {
            grdSummaryViewFields.DataSourceList = HTMLReportConfiguration.RunSetFieldsToSelect;
            grdEmailSummaryViewFields.DataSourceList = HTMLReportConfiguration.EmailSummaryViewFieldsToSelect;
            grdGingersFields.DataSourceList = HTMLReportConfiguration.GingerRunnerFieldsToSelect;
            grdBusinessFlowFields.DataSourceList = HTMLReportConfiguration.BusinessFlowFieldsToSelect;
            grdActivityGroupsFields.DataSourceList = HTMLReportConfiguration.ActivityGroupFieldsToSelect;
            grdActivitiesFields.DataSourceList = HTMLReportConfiguration.ActivityFieldsToSelect;
            grdActionsFields.DataSourceList = HTMLReportConfiguration.ActionFieldsToSelect;
        }

        private static ObservableList<HTMLReportConfigFieldToSelect> GetReportLevelMembers(Type reportLevelType)
        {


            return HTMLReportConfiguration.GetReportLevelMembers(reportLevelType);
        }

        public static HTMLReportConfiguration EnchancingLoadedFieldsWithDataAndValidating(HTMLReportConfiguration HTMLReportConfiguration)
        {
      
            return HTMLReportConfiguration.EnchancingLoadedFieldsWithDataAndValidating(HTMLReportConfiguration);
        }

        private static ObservableList<HTMLReportConfigFieldToSelect> EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames fieldsToSelectListName, Type reportType, HTMLReportConfiguration HTMLReportConfiguration)
        {
            return HTMLReportConfiguration.EnchancingLoadedFieldsWithDataAndValidatingPerLevel(fieldsToSelectListName, reportType, HTMLReportConfiguration);
        }

        private void SetControls()
        {
            SetIsDefualtImage();
            NewTemplateNameTextBox.Text = _HTMLReportConfiguration.Name.ToString();
            TemplateDescriptionTextBox.BindControl(_HTMLReportConfiguration, nameof(HTMLReportConfiguration.Description));
            htmlShowFirstIterationOnRadioBtn.IsChecked = _HTMLReportConfiguration.ShowAllIterationsElements;
            htmlShowFirstIterationOffRadioBtn.IsChecked = !_HTMLReportConfiguration.ShowAllIterationsElements;
            htmlUseLocalStoredStylingOnRadioBtn.IsChecked = _HTMLReportConfiguration.UseLocalStoredStyling;
            htmlUseLocalStoredStylingOffRadioBtn.IsChecked = !_HTMLReportConfiguration.UseLocalStoredStyling;
            SetControlsEvents();

            switch ((HTMLReportConfiguration.ReportsLevel)Enum.Parse(typeof(HTMLReportConfiguration.ReportsLevel), _HTMLReportConfiguration.ReportLowerLevelToShow))
            {
                case HTMLReportConfiguration.ReportsLevel.SummaryViewLevel:
                    gingers_HeaderSelection.IsChecked = false;
                    break;
                case HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel:
                    bf_HeaderSelection.IsChecked = false;
                    break;
                case HTMLReportConfiguration.ReportsLevel.BusinessFlowLevel:
                    activities_HeaderSelection.IsChecked = false;
                    break;
                case HTMLReportConfiguration.ReportsLevel.ActivityLevel:
                    actions_HeaderSelection.IsChecked = false;
                    break;
                case HTMLReportConfiguration.ReportsLevel.ActionLevel:
                    // do nothing                    
                    break;
            }
            tbiSummaryViewFields.IsSelected = true;
        }

        private void SetControlsNewTemplate()
        {
            SetIsDefualtImage();
            NewTemplateNameTextBox.Text = _HTMLReportConfiguration.Name.ToString();
            TemplateDescriptionTextBox.Text = _HTMLReportConfiguration.Description.ToString();
            //htmlDefaultOnRadioBtn.IsChecked = _HTMLReportConfiguration.IsDefault;
            //htmlDefaultOffRadioBtn.IsChecked = !_HTMLReportConfiguration.IsDefault;
            htmlShowFirstIterationOnRadioBtn.IsChecked = _HTMLReportConfiguration.ShowAllIterationsElements;
            htmlShowFirstIterationOffRadioBtn.IsChecked = !_HTMLReportConfiguration.ShowAllIterationsElements;

            SetControlsEvents();
        }

        private void SetControlsEvents()
        {
            gingers_HeaderSelection.Checked += gingers_HeaderSelection_Checked;
            gingers_HeaderSelection.Unchecked += gingers_HeaderSelection_Unchecked;
            bf_HeaderSelection.Checked += bf_HeaderSelection_Checked;
            bf_HeaderSelection.Unchecked += bf_HeaderSelection_Unchecked;
            activities_HeaderSelection.Checked += activities_HeaderSelection_Checked;
            activities_HeaderSelection.Unchecked += activities_HeaderSelection_Unchecked;
            actions_HeaderSelection.Checked += actions_HeaderSelection_Checked;
            actions_HeaderSelection.Unchecked += actions_HeaderSelection_Unchecked;
        }

        private void SetLoadedLogoImage()
        {
            imgLogo.Source = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(_HTMLReportConfiguration.LogoBase64Image.ToString()));
        }

        private void SetDefaultLogoImage()
        {
            imgLogo.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@amdocs_logo.jpg"));
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            ObservableList<Button> winButtons = new ObservableList<Button>();

            Button SaveAllButton = new Button();
            SaveAllButton.Content = "Save";
            SaveAllButton.Click += new RoutedEventHandler(SaveButton_Click);
            winButtons.Add(SaveAllButton);

            //htmlDefaultSwitchPnl.Visibility = System.Windows.Visibility.Visible;

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.Name = NewTemplateNameTextBox.Text.ToString();
            _HTMLReportConfiguration.Description = TemplateDescriptionTextBox.Text.ToString();
            _newHTMLReportConfiguration = _HTMLReportConfiguration;
            _pageGenericWin.Hide();
             WorkSpace.Instance.Solution.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.ReportConfiguration);

            if (_existingTemplatePage)
            {
                Reporter.ToStatus(eStatusMsgKey.SaveItem, null, _HTMLReportConfiguration.GetNameForFileName(), "item");                
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(_HTMLReportConfiguration);
                Reporter.HideStatusMessage();
            }
        }

        private void FieldIsAddedToReport_Checked(object sender, RoutedEventArgs e)
        {
            switch ((sender as CheckBox).Name)
            {
                case "grdSummaryView_FieldSelection":
                    _HTMLReportConfiguration.RunSetFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdSummaryViewFields.CurrentItem).FieldKey)).FirstOrDefault().IsSelected = (bool)(sender as CheckBox).IsChecked;
                    break;
                case "grdEmailSummaryView_FieldSelection":
                    _HTMLReportConfiguration.EmailSummaryViewFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdEmailSummaryViewFields.CurrentItem).FieldKey)).FirstOrDefault().IsSelected = (bool)(sender as CheckBox).IsChecked;
                    break;
                case "grdGingers_FieldSelection":
                    _HTMLReportConfiguration.GingerRunnerFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdGingersFields.CurrentItem).FieldKey)).FirstOrDefault().IsSelected = (bool)(sender as CheckBox).IsChecked;
                    break;
                case "grdBusinessFlow_FieldSelection":
                    _HTMLReportConfiguration.BusinessFlowFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdBusinessFlowFields.CurrentItem).FieldKey)).FirstOrDefault().IsSelected = (bool)(sender as CheckBox).IsChecked;
                    break;
                case "grdActivities_FieldSelection":
                    _HTMLReportConfiguration.ActivityFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdActivitiesFields.CurrentItem).FieldKey)).FirstOrDefault().IsSelected = (bool)(sender as CheckBox).IsChecked;
                    break;
                case "grdActivityGroups_FieldSelection":
                    _HTMLReportConfiguration.ActivityGroupFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdActivityGroupsFields.CurrentItem).FieldKey)).FirstOrDefault().IsSelected = (bool)(sender as CheckBox).IsChecked;
                    break;
                case "grdActions_FieldSelection":
                    _HTMLReportConfiguration.ActionFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdActionsFields.CurrentItem).FieldKey)).FirstOrDefault().IsSelected = (bool)(sender as CheckBox).IsChecked;
                    break;
            }
        }

        private void SectionCollapsed_Checked(object sender, RoutedEventArgs e)
        {
            switch ((sender as CheckBox).Name)
            {
                case "grdSummaryView_SectionCollapsed":
                    _HTMLReportConfiguration.RunSetFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdSummaryViewFields.CurrentItem).FieldKey)).FirstOrDefault().IsSectionCollapsed = (bool)(sender as CheckBox).IsChecked;
                    break;
                case "grdEmailSummaryView_SectionCollapsed":
                    _HTMLReportConfiguration.EmailSummaryViewFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdEmailSummaryViewFields.CurrentItem).FieldKey)).FirstOrDefault().IsSectionCollapsed = (bool)(sender as CheckBox).IsChecked;
                    break;
                case "grdGingers_SectionCollapsed":
                    _HTMLReportConfiguration.GingerRunnerFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdGingersFields.CurrentItem).FieldKey)).FirstOrDefault().IsSectionCollapsed = (bool)(sender as CheckBox).IsChecked;
                    break;
                case "grdBusinessFlow_SectionCollapsed":
                    _HTMLReportConfiguration.BusinessFlowFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdBusinessFlowFields.CurrentItem).FieldKey)).FirstOrDefault().IsSectionCollapsed = (bool)(sender as CheckBox).IsChecked;
                    break;
                case "grdActivities_SectionCollapsed":
                    _HTMLReportConfiguration.ActivityFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdActivitiesFields.CurrentItem).FieldKey)).FirstOrDefault().IsSectionCollapsed = (bool)(sender as CheckBox).IsChecked;
                    break;
                case "grdActions_SectionCollapsed":
                    _HTMLReportConfiguration.ActionFieldsToSelect.Where(x => (x.FieldKey == ((HTMLReportConfigFieldToSelect)grdActionsFields.CurrentItem).FieldKey)).FirstOrDefault().IsSectionCollapsed = (bool)(sender as CheckBox).IsChecked;
                    break;
            }
        }

        private void NewTemplateName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _HTMLReportConfiguration.Name = NewTemplateNameTextBox.Text.ToString();
        }

        public HTMLReportConfiguration CreateNewHTMLReportTemplateShowAsWindow()
        {
            Instance.ShowAsWindow();
            return Instance._newHTMLReportConfiguration;
        }

        private void htmlDefaultOnRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.IsDefault = true;
        }

        private void htmlDefaultOffRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.IsDefault = false;
        }

        private void htmlShowFirstIterationOnRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.ShowAllIterationsElements = true;
        }

        private void htmlShowFirstIterationOffRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.ShowAllIterationsElements = false;
        }

        private void SelectHTMLReportsImageFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog op = new System.Windows.Forms.OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg";
            if (op.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fileLength = new FileInfo(op.FileName).Length;
                if (fileLength <= 30000)
                {
                    imgLogo.Source = new BitmapImage(new Uri(op.FileName));
                    if ((op.FileName != null) && (op.FileName != string.Empty))
                    {
                        using (var ms = new MemoryStream())
                        {
                            BitmapImage bi = new BitmapImage(new Uri(op.FileName));
                            Tuple<int, int> sizes = Ginger.General.RecalculatingSizeWithKeptRatio(bi, Ginger.Reports.GingerExecutionReport.GingerExecutionReport.logoWidth, Ginger.Reports.GingerExecutionReport.GingerExecutionReport.logoHight);

                            BitmapImage bi_resized = new BitmapImage();
                            bi_resized.BeginInit();
                            bi_resized.UriSource = new Uri(op.FileName);
                            bi_resized.DecodePixelHeight = sizes.Item2;
                            bi_resized.DecodePixelWidth = sizes.Item1;
                            bi_resized.EndInit();

                            _HTMLReportConfiguration.LogoBase64Image = Ginger.General.BitmapToBase64(Ginger.General.BitmapImage2Bitmap(bi_resized));
                        }
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.ImageSize, "30");
                }
            }
        }

        private void TemplateDescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _HTMLReportConfiguration.Description = TemplateDescriptionTextBox.Text.ToString();
        }

        private void gingers_HeaderSelection_Checked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.ReportLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString();
            tbiGingersField.IsSelected = true;

            grdGingersField.IsEnabled = true;
            bf_HeaderSelection.IsEnabled = true;
            tbBusinessFlowField.IsEnabled = true;

            bf_HeaderSelection.Unchecked -= new RoutedEventHandler(bf_HeaderSelection_Unchecked);
            bf_HeaderSelection.IsChecked = false;
            bf_HeaderSelection.Unchecked += new RoutedEventHandler(bf_HeaderSelection_Unchecked);
        }

        private void gingers_HeaderSelection_Unchecked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.ReportLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString();
            tbiSummaryViewFields.IsSelected = true;

            grdGingersField.IsEnabled = false;

            bf_HeaderSelection.Unchecked -= new RoutedEventHandler(bf_HeaderSelection_Unchecked);
            bf_HeaderSelection.IsChecked = false;
            bf_HeaderSelection.Unchecked -= new RoutedEventHandler(bf_HeaderSelection_Unchecked);

            bf_HeaderSelection.IsEnabled = false;
            grdBusinessFlowField.IsEnabled = false;
            tbBusinessFlowField.IsEnabled = false;

            activities_HeaderSelection.Unchecked -= new RoutedEventHandler(activities_HeaderSelection_Unchecked);
            activities_HeaderSelection.IsChecked = false;
            activities_HeaderSelection.Unchecked += new RoutedEventHandler(activities_HeaderSelection_Unchecked);

            activities_HeaderSelection.IsEnabled = false;
            tbiActivitiesField.IsEnabled = false;
            grdActivitiesField.IsEnabled = false;

            actions_HeaderSelection.Unchecked -= new RoutedEventHandler(actions_HeaderSelection_Unchecked);
            actions_HeaderSelection.IsChecked = false;
            actions_HeaderSelection.Unchecked += new RoutedEventHandler(actions_HeaderSelection_Unchecked);

            actions_HeaderSelection.IsEnabled = false;
            tbiActionsField.IsEnabled = false;
            grdActionsField.IsEnabled = false;
        }

        private void bf_HeaderSelection_Checked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.ReportLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.BusinessFlowLevel.ToString();
            tbBusinessFlowField.IsSelected = true;

            grdBusinessFlowField.IsEnabled = true;

            activities_HeaderSelection.Unchecked -= new RoutedEventHandler(activities_HeaderSelection_Unchecked);
            activities_HeaderSelection.IsChecked = false;
            activities_HeaderSelection.Unchecked += new RoutedEventHandler(activities_HeaderSelection_Unchecked);

            activities_HeaderSelection.IsEnabled = true;
            tbiActivitiesField.IsEnabled = true;
        }

        private void bf_HeaderSelection_Unchecked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.ReportLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString();
            tbiGingersField.IsSelected = true;

            grdBusinessFlowField.IsEnabled = false;

            activities_HeaderSelection.Unchecked -= new RoutedEventHandler(activities_HeaderSelection_Unchecked);
            activities_HeaderSelection.IsChecked = false;
            activities_HeaderSelection.Unchecked += new RoutedEventHandler(activities_HeaderSelection_Unchecked);

            activities_HeaderSelection.IsEnabled = false;
            tbiActivitiesField.IsEnabled = false;
            grdActivitiesField.IsEnabled = false;

            actions_HeaderSelection.Unchecked -= new RoutedEventHandler(actions_HeaderSelection_Unchecked);
            actions_HeaderSelection.IsChecked = false;
            actions_HeaderSelection.Unchecked += new RoutedEventHandler(actions_HeaderSelection_Unchecked);

            actions_HeaderSelection.IsEnabled = false;
            tbiActionsField.IsEnabled = false;
            grdActionsField.IsEnabled = false;
        }

        private void activities_HeaderSelection_Checked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.ReportLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.ActivityLevel.ToString();
            tbiActivitiesField.IsSelected = true;

            grdActivitiesField.IsEnabled = true;

            actions_HeaderSelection.Unchecked -= new RoutedEventHandler(actions_HeaderSelection_Unchecked);
            actions_HeaderSelection.IsChecked = false;
            actions_HeaderSelection.Unchecked += new RoutedEventHandler(actions_HeaderSelection_Unchecked);

            actions_HeaderSelection.IsEnabled = true;
            tbiActionsField.IsEnabled = true;
        }

        private void activities_HeaderSelection_Unchecked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.ReportLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.BusinessFlowLevel.ToString();
            tbBusinessFlowField.IsSelected = true;

            grdActivitiesField.IsEnabled = false;

            actions_HeaderSelection.Unchecked -= new RoutedEventHandler(actions_HeaderSelection_Unchecked);
            actions_HeaderSelection.IsChecked = false;
            actions_HeaderSelection.Unchecked += new RoutedEventHandler(actions_HeaderSelection_Unchecked);

            actions_HeaderSelection.IsEnabled = false;
            tbiActionsField.IsEnabled = false;
            grdActionsField.IsEnabled = false;
        }

        private void actions_HeaderSelection_Checked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.ReportLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.ActionLevel.ToString();
            tbiActionsField.IsSelected = true;

            grdActionsField.IsEnabled = true;
        }

        private void actions_HeaderSelection_Unchecked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.ReportLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.ActivityLevel.ToString();
            tbiActivitiesField.IsSelected = true;

            grdActionsField.IsEnabled = false;
        }

        private void htmlUseLocalStoredStylingOnRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.UseLocalStoredStyling = true;
        }

        private void htmlUseLocalStoredStylingOffRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            _HTMLReportConfiguration.UseLocalStoredStyling = false;
        }

        private void tabConfigurations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbiPreview.IsSelected)
            {
                if (WBP == null)
                {
                    RefreshPreview();
                }
            }
        }

        private void RefreshPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshPreview();
        }


        private void RefreshPreview()
        {
            _previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            HTMLReportsConfiguration currentConf =  WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            
            //changing the solution because report should not be created in installtion folder due to permissions issues + it can be multiple users will run same Ginger on server
            if (Directory.Exists(mPreviewDummyReportPath))            
                ClearDirectoryContent(mPreviewDummyReportPath);            
            else            
                PrepareDummyReportData();
            
            Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(mPreviewDummyReportDataPath),
                                                                                                        false,
                                                                                                        _HTMLReportConfiguration,
                                                                                                        mPreviewDummyReportPath, false,currentConf.HTMLReportConfigurationMaximalFolderSize);

            WBP = new WebBrowserPage();
            frmBrowser.Content = WBP;
            browser = WBP.GetBrowser();
            browser.Navigate(System.IO.Path.Combine(mPreviewDummyReportPath, "GingerExecutionReport.html"));

            Mouse.OverrideCursor = _previousCursor;
        }

        private void PrepareDummyReportData()
        {
            try
            {
                string dummyReportOriginalZipFilePath = Assembly.GetExecutingAssembly().Location.Replace("Ginger.exe", "") + @"Reports\GingerExecutionReport\PreviewDummyReport\RunSet.zip";
                if (File.Exists(dummyReportOriginalZipFilePath))
                {
                    string tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath() + "GingerHtmlPreviewReport");
                    mPreviewDummyReportDataPath = System.IO.Path.Combine(tempFolder, "Data");
                    if (Directory.Exists(mPreviewDummyReportDataPath))                    
                        ClearDirectoryContent(mPreviewDummyReportDataPath);
                    else
                        Directory.CreateDirectory(mPreviewDummyReportDataPath);
                    ZipFile.ExtractToDirectory(dummyReportOriginalZipFilePath, mPreviewDummyReportDataPath);

                    string unzippedDataTestPath= System.IO.Path.Combine(mPreviewDummyReportDataPath, "RunSet.txt");
                    if (File.Exists(unzippedDataTestPath) == false)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Missing HTML Report preview unzipped data on: " + unzippedDataTestPath);
                        mPreviewDummyReportPath = string.Empty;
                    }
                    else
                    {
                        mPreviewDummyReportPath = System.IO.Path.Combine(tempFolder, "Report");
                        Directory.CreateDirectory(mPreviewDummyReportPath);
                    }
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Missing HTML Report preview Dummy report Zip file on: " + dummyReportOriginalZipFilePath);
                }                
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to prepare the dummy report data for HTML Report Preview", ex);
            }
        }

        private void ClearDirectoryContent(string DirPath)
        {
            //clear directory
            System.IO.DirectoryInfo di = new DirectoryInfo(DirPath);
            foreach (FileInfo file in di.GetFiles())
                file.Delete();
            foreach (DirectoryInfo dir in di.GetDirectories())
                dir.Delete(true);
        }

        private void SetIsDefualtImage()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (_HTMLReportConfiguration.IsDefault)
                    xDefualtImage.Visibility = Visibility.Visible;
                else
                    xDefualtImage.Visibility = Visibility.Collapsed;
            });
        }

        private void _HTMLReportConfiguration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HTMLReportConfiguration.IsDefault))
            {
                SetIsDefualtImage();
            }
        }
    }
}
