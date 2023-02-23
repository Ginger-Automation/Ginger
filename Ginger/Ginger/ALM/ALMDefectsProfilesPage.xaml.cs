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

//using AlmDataContractsStd.Enums;
using AlmDataContractsStd.Enums;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore.ALM;
using GingerCoreNET.ALMLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.ALM
{
    /// <summary>
    /// Interaction logic for QCItemsFieldsConfigurationPage.xaml
    /// </summary>
    public partial class ALMDefectsProfilesPage : Page
    {
        ObservableList<ALMDefectProfile> mALMDefectProfiles = new ObservableList<ALMDefectProfile>();
        ObservableList<ExternalItemFieldBase> mALMDefectProfileFields = new ObservableList<ExternalItemFieldBase>();
        ObservableList<ExternalItemFieldBase> mALMDefectProfileFieldsExisted = new ObservableList<ExternalItemFieldBase>();
        bool _manualCheckedEvent = false;

        public ObservableList<ALMDefectProfile> ALMDefectProfiles
        {
            get { return mALMDefectProfiles; }
        }

        GenericWindow genWin = null;

        public static string LoadFieldsState = "aa";

        public ALMDefectsProfilesPage()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            InitializeComponent();

            mALMDefectProfiles = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>();

            grdDefectsProfiles.DataSourceList = mALMDefectProfiles;
            if (mALMDefectProfiles != null)
            {
                if (grdDefectsProfiles.CurrentItem != null)
                {
                    grdDefectsFields.DataSourceList = ((ALMDefectProfile)grdDefectsProfiles.CurrentItem).ALMDefectProfileFields;
                }
            }

            SetFieldsGrid();
            Mouse.OverrideCursor = null;
            _manualCheckedEvent = true;
        }

        List<GingerCore.GeneralLib.ComboEnumItem> ALMTypes = GingerCore.General.GetEnumValuesForCombo(typeof(ALMIntegrationEnums.eALMType));

        private void SetFieldsGrid()
        {
            if (mALMDefectProfiles != null)
            {
                foreach (ALMDefectProfile defectProfile in mALMDefectProfiles)
                {
                    StartTrackingDefectProfile(defectProfile);
                }
            }
            //Remove Rally
            var comboEnumItem = ALMTypes.Cast<GingerCore.GeneralLib.ComboEnumItem>().Where(x => x.text == ALMIntegrationEnums.eALMType.RALLY.ToString()).FirstOrDefault();
            ALMTypes.Remove(comboEnumItem);

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ALMDefectProfile.Name), WidthWeight = 30, Header = "Name", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            view.GridColsView.Add(new GridColView() { Field = nameof(ALMDefectProfile.Description), WidthWeight = 30, Header = "Description", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            view.GridColsView.Add(new GridColView() { Field = nameof(ALMDefectProfile.AlmType), WidthWeight = 30, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = ALMTypes, Header = "ALM Type", HorizontalAlignment = System.Windows.HorizontalAlignment.Center });
            view.GridColsView.Add(new GridColView() { Field = nameof(OptionalValue.IsDefault), WidthWeight = 10, Header = "Default", StyleType = GridColView.eGridColStyleType.Template, HorizontalAlignment = HorizontalAlignment.Center, CellTemplate = (DataTemplate)this.grdDefectsProfile.Resources["DefaultValueTemplate"] });

            grdDefectsProfiles.SetAllColumnsDefaultView(view);
            grdDefectsProfiles.btnEdit.Visibility = Visibility.Visible;
            grdDefectsProfiles.btnEdit.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditDefectsProfile));
            grdDefectsProfiles.btnAdd.Visibility = Visibility.Visible;
            grdDefectsProfiles.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddDefectsProfile));
            //grdDefectsProfiles.btnDelete.Visibility = System.Windows.Visibility.Visible;                                     //
            //grdDefectsProfiles.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteDefectsProfile));        // to fix - should use non-generic handler !!!
            grdDefectsProfiles.btnRefresh.Visibility = Visibility.Visible;
            grdDefectsProfiles.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshgrdDefectsProfilesHandler));

            grdDefectsProfiles.SetAllColumnsDefaultView(view);
            grdDefectsProfiles.InitViewItems();

            grdDefectsProfiles.Grid.SelectionChanged += grdDefectsProfiles_SelectionChanged;
            grdDefectsProfiles.RowChangedEvent += grdDefectsProfiles_RowChangedEvent;
            grdDefectsProfiles.SetTitleLightStyle = true;

            view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ExternalItemFieldBase.Name), Header = "Defect's Field Name", WidthWeight = 20, ReadOnly = true, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ExternalItemFieldBase.Mandatory), Header = "Field Is Mandatory", WidthWeight = 15, ReadOnly = true, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(ExternalItemFieldBase.SelectedValue), Header = "Selected Value", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(ExternalItemFieldBase.Fields.PossibleValues, ExternalItemFieldBase.Fields.SelectedValue, true), WidthWeight = 20 });

            grdDefectsFields.btnRefresh.Visibility = Visibility.Visible;
            grdDefectsFields.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshgrdDefectsFieldsHandler));
            ToolTipService.SetToolTip(grdDefectsFields.btnRefresh, new ToolTip { Content = "Refresh fields [Keep existing fields] of selected Defect profile", Style = FindResource("ToolTipStyle") as Style });
            ToolTipService.SetShowDuration(grdDefectsFields.btnRefresh, 15000);

            grdDefectsFields.btnAdd.Visibility = Visibility.Visible;
            ToolTipService.SetToolTip(grdDefectsFields.btnAdd, new ToolTip { Content = "Fetch latest fields [Overwrite existing fields] of selected Defect profile", Style = FindResource("ToolTipStyle") as Style });
            ToolTipService.SetShowDuration(grdDefectsFields.btnAdd, 15000);
            grdDefectsFields.SetBtnImage(grdDefectsFields.btnAdd, "@GetLatest2_32x32.png");
            grdDefectsFields.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(FetchgrdDefectsFieldsHandler));

            grdDefectsFields.SetAllColumnsDefaultView(view);
            grdDefectsFields.InitViewItems();
            grdDefectsFields.SetTitleLightStyle = true;
        }

        private void grdDefectsProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((mALMDefectProfiles != null) && (_manualCheckedEvent))
            {
                if (grdDefectsProfiles.CurrentItem != null)
                {
                    grdDefectsFields.DataSourceList = ((ALMDefectProfile)grdDefectsProfiles.CurrentItem).ALMDefectProfileFields;
                }
            }
        }

        private void grdDefectsProfiles_RowChangedEvent(object sender, EventArgs e)
        {
            //Moved to SelectionChanged
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button saveButton = new Button();
            saveButton.Content = "Save";
            saveButton.ToolTip = "Save 'To Update' fields";
            saveButton.Click += new RoutedEventHandler(Save);

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { saveButton });
        }

        private void DefaultValueRadioButton_SelectionChanged(object sender, RoutedEventArgs e)
        {
            foreach (ALMDefectProfile aLMDefectProfile in mALMDefectProfiles)
            {
                if (aLMDefectProfile.Guid != (Guid)((RadioButton)sender).Tag)
                {
                    aLMDefectProfile.IsDefault = false;
                }
                else
                {
                    aLMDefectProfile.IsDefault = true;
                }
            }
        }

        private void AddDefectsProfile(object sender, RoutedEventArgs e)
        {
            ALMDefectProfile newALMDefectProfile = new ALMDefectProfile();
            newALMDefectProfile.ToUpdate = true;
            if (mALMDefectProfiles.Count == 0)
            {
                newALMDefectProfile.IsDefault = true;
                newALMDefectProfile.ID = 1;
            }
            else
            {
                newALMDefectProfile.ID = mALMDefectProfiles.Select(x => x.ID).Max() + 1;
                newALMDefectProfile.IsDefault = false;
            }
            newALMDefectProfile.Name = "Some Name " + (newALMDefectProfile.ID + 1).ToString();
            newALMDefectProfile.Description = "Some Description " + (newALMDefectProfile.ID + 1).ToString();
            ALMConfig AlmConfig = ALMCore.GetDefaultAlmConfig();
            newALMDefectProfile.AlmType = AlmConfig.AlmType;
            mALMDefectProfiles.Add(newALMDefectProfile);

            newALMDefectProfile.ALMDefectProfileFields = FetchDefectFields(newALMDefectProfile.AlmType);
            newALMDefectProfile.ALMDefectProfileFields.Where(z => z.Mandatory).ToList().ForEach(x => x.SelectedValue = string.Empty);

            grdDefectsProfiles.Grid.SelectionChanged += grdDefectsProfiles_SelectionChanged;
            grdDefectsProfiles.DataSourceList = mALMDefectProfiles;
            grdDefectsFields.DataSourceList = newALMDefectProfile.ALMDefectProfileFields;
            grdDefectsProfiles.Grid.SelectedItem = newALMDefectProfile;
            StartTrackingDefectProfile(newALMDefectProfile);
        }

        private void EditDefectsProfile(object sender, RoutedEventArgs e)
        {
        }

        private void DeleteDefectsProfile(object sender, RoutedEventArgs e)
        {
            if (grdDefectsProfiles.Grid.SelectedItems.Count > 0)
            {
                if (Reporter.ToUser(eUserMsgKey.AskBeforeDefectProfileDeleting) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    List<ALMDefectProfile> selectedItemsToDelete = new List<ALMDefectProfile>();
                    foreach (ALMDefectProfile selectedProfile in grdDefectsProfiles.Grid.SelectedItems)
                        selectedItemsToDelete.Add(selectedProfile);
                    foreach (ALMDefectProfile profileToDelete in selectedItemsToDelete)
                        WorkSpace.Instance.SolutionRepository.DeleteRepositoryItem(profileToDelete);
                }
            }
        }

        private void RefreshgrdDefectsProfilesHandler(object sender, RoutedEventArgs e)
        {

        }

        private void RefreshgrdDefectsFieldsHandler(object sender, RoutedEventArgs e)
        {
            ALMDefectProfile AlmDefectProfile = (ALMDefectProfile)grdDefectsProfiles.CurrentItem;
            {
                mALMDefectProfileFields = FetchDefectFields(AlmDefectProfile.AlmType);
                mALMDefectProfileFields.Where(z => z.Mandatory).ToList().ForEach(x => x.SelectedValue = string.Empty);
                mALMDefectProfileFieldsExisted = new ObservableList<ExternalItemFieldBase>();
                foreach (ExternalItemFieldBase aLMDefectProfileField in mALMDefectProfileFields)
                {
                    ExternalItemFieldBase aLMDefectProfileFieldExisted = (ExternalItemFieldBase)aLMDefectProfileField.CreateCopy();
                    if (!string.IsNullOrEmpty(aLMDefectProfileField.ExternalID))
                    {
                        aLMDefectProfileFieldExisted.ExternalID = string.Copy(aLMDefectProfileField.ExternalID);
                    }
                    ExternalItemFieldBase field = AlmDefectProfile.ALMDefectProfileFields.Where(x => x.ID == aLMDefectProfileField.ID).FirstOrDefault();
                    if (field != null)
                    {
                        aLMDefectProfileFieldExisted.SelectedValue = field.SelectedValue;
                    }
                    aLMDefectProfileFieldExisted.PossibleValues = aLMDefectProfileField.PossibleValues;
                    mALMDefectProfileFieldsExisted.Add(aLMDefectProfileFieldExisted);
                }
                AlmDefectProfile.ALMDefectProfileFields = mALMDefectProfileFieldsExisted;
            }
        }
        private void FetchgrdDefectsFieldsHandler(object sender, RoutedEventArgs e)
        {
            ALMDefectProfile AlmDefectProfile = (ALMDefectProfile)grdDefectsProfiles.CurrentItem;
            AlmDefectProfile.ALMDefectProfileFields = FetchDefectFields(AlmDefectProfile.AlmType);
            grdDefectsFields.DataSourceList = AlmDefectProfile.ALMDefectProfileFields;
        }
        private void Save(object sender, RoutedEventArgs e)
        {
            if (mALMDefectProfiles.Where(x => x.IsDefault == true).ToList().Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.NoDefaultDefectProfileSelected);
                return;
            }

            //
            // Validation section
            foreach (ALMDefectProfile _ALMDefectProfile in mALMDefectProfiles.Where(x => x.ToUpdate == true).ToList())
            {
                // Mandatory fields validation
                ExternalItemFieldBase notPopulatedMandatoryField = _ALMDefectProfile.ALMDefectProfileFields.Where(x => x.Mandatory == true && x.ExternalID != "description"
                                                                                                                                            && x.ExternalID != "name"
                                                                                                                                            && x.ExternalID != "Summary" && (x.SelectedValue == null || x.SelectedValue == string.Empty)).FirstOrDefault();
                if (notPopulatedMandatoryField != null)
                {
                    Reporter.ToUser(eUserMsgKey.MissedMandatotryFields, notPopulatedMandatoryField.Name, _ALMDefectProfile.Name);
                    return;
                }

                // Wrong list selection validation
                ExternalItemFieldBase wrongSelectedField = _ALMDefectProfile.ALMDefectProfileFields.Where(x => ((string.Equals(x.Type, "LookupList", StringComparison.OrdinalIgnoreCase) || string.Equals(x.Type, "UserList", StringComparison.OrdinalIgnoreCase))) &&
                                                                                                                 x.SelectedValue != null && x.SelectedValue != string.Empty && 
                                                                                                                 x.PossibleValues.Count > 0 && (!x.PossibleValues.Contains(x.SelectedValue))).FirstOrDefault();    
                if (wrongSelectedField != null)
                {
                    Reporter.ToUser(eUserMsgKey.WrongValueSelectedFromTheList, wrongSelectedField.Name, _ALMDefectProfile.Name);
                    return;
                }

                // Numeric selection validation
                int numeric = 0;
                ExternalItemFieldBase wrongNonNumberValueField = _ALMDefectProfile.ALMDefectProfileFields.Where(x => (string.Equals(x.Type, "Number", StringComparison.OrdinalIgnoreCase)) &&
                                                                                                                      x.SelectedValue != null && x.SelectedValue != string.Empty && 
                                                                                                                      !(int.TryParse(x.SelectedValue, out numeric))).FirstOrDefault();
                if (wrongNonNumberValueField != null)
                {
                    Reporter.ToUser(eUserMsgKey.WrongNonNumberValueInserted, wrongNonNumberValueField.Name, _ALMDefectProfile.Name);
                    return;
                }

                // Date selection validation (QC receiving date in format yyyy-mm-dd)
                DateTime dt;
                ExternalItemFieldBase wrongDateValueField = _ALMDefectProfile.ALMDefectProfileFields.Where(x => (string.Equals(x.Type, "Date", StringComparison.OrdinalIgnoreCase)) &&
                                                                                                                      x.SelectedValue != null && x.SelectedValue != string.Empty &&
                                                                                                                      !(DateTime.TryParseExact(x.SelectedValue, "yyyy-mm-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))).FirstOrDefault();
                if (wrongDateValueField != null)
                {
                    Reporter.ToUser(eUserMsgKey.WrongDateValueInserted, wrongDateValueField.Name, _ALMDefectProfile.Name);
                    return;
                }
            }

            foreach (ALMDefectProfile _ALMDefectProfile in mALMDefectProfiles.Where(x => x.ToUpdate == true).ToList())
            {
                Reporter.ToStatus(eStatusMsgKey.SaveItem, null, _ALMDefectProfile.GetNameForFileName(), "item");
                if ((_ALMDefectProfile.ContainingFolder == null) || (_ALMDefectProfile.ContainingFolder == string.Empty))
                {
                    _ALMDefectProfile.ContainingFolder = System.IO.Path.Combine( WorkSpace.Instance.Solution.Folder, _ALMDefectProfile.ObjFolderName);
                    _ALMDefectProfile.FilePath = _ALMDefectProfile.ContainingFolder + @"\" + _ALMDefectProfile.FilePath;
                }
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(_ALMDefectProfile);
                Reporter.HideStatusMessage();
            }

            genWin.Close();
        }

        public static ALMDefectProfile SetALMDefectProfileWithDefaultValues(string name = null)
        {
            ALMDefectProfile newALMDefectProfile = new ALMDefectProfile();
            if ((WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>() != null) && (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>().Count > 0))
            {
                newALMDefectProfile.ID = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>().Max(x => x.ALMDefectProfilesSeq) + 1;
            }
            else
            {
                newALMDefectProfile.ID = 1;
            }
            if ((name != null) && (name != string.Empty))
            {
                newALMDefectProfile.Name = name;
            }
            else
            {
                newALMDefectProfile.Name = "Template #" + newALMDefectProfile.ID;
            }
            newALMDefectProfile.IsDefault = true;
            newALMDefectProfile.ShowAllIterationsElements = false;
            newALMDefectProfile.UseLocalStoredStyling = false;
            newALMDefectProfile.Description = string.Empty;

            return newALMDefectProfile;
        }

        ObservableList<ExternalItemFieldBase> FetchDefectFields(GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType AlmType)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ALMIntegration.Instance.UpdateALMType(AlmType, true);
                mALMDefectProfileFields = ALMIntegration.Instance.GetALMItemFieldsREST(true, ResourceType.DEFECT, null);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while Fetching Fields", ex);
            }
            finally
            {
                ALMIntegration.Instance.UpdateALMType(ALMCore.GetDefaultAlmConfig().AlmType);
                Mouse.OverrideCursor = null;
            }
            return mALMDefectProfileFields;
        }
        private void StartTrackingDefectProfile(ALMDefectProfile profile)
        {
            profile.StartDirtyTracking();
        }
    }
}
