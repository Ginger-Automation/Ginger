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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GingerCore.Variables;
using GingerCore;
using System.Reflection;
using Ginger.Repository;
using GingerCore.Actions;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for VariableEditPage.xaml
    /// </summary>
    public partial class VariableEditPage : Page
    {
        private VariableBase mVariable;
        private RepositoryItemBase mParent;
        bool saveWasDone = false;

        GenericWindow _pageGenericWin = null;

        public enum eEditMode
        {
            SharedRepository = 0,
            BusinessFlow = 1,
            Activity = 2,
            Global = 4,
            FindAndReplace = 5,
            View = 6
        }

        public eEditMode editMode { get; set; }
                
        public VariableEditPage(VariableBase v, bool setGeneralConfigsAsReadOnly = false, eEditMode mode = eEditMode.BusinessFlow, RepositoryItemBase parent = null)
        {
            InitializeComponent();
           
            this.Title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Variable);
            mVariable = v;                   
            mVariable.SaveBackup();
            editMode = mode;
            mParent = parent;

            App.ObjFieldBinding(txtVarName, TextBox.TextProperty, mVariable, VariableBase.Fields.Name);
            App.ObjFieldBinding(txtVarDescritpion, TextBox.TextProperty, mVariable, VariableBase.Fields.Description);
            App.ObjFieldBinding(txtFormula, TextBox.TextProperty, mVariable, VariableBase.Fields.Formula, BindingMode.OneWay);
            App.ObjFieldBinding(txtCurrentValue, TextBox.TextProperty, mVariable, VariableBase.Fields.Value, BindingMode.OneWay);
            App.ObjFieldBinding(cbSetAsInputValue, CheckBox.IsCheckedProperty, mVariable, VariableBase.Fields.SetAsInputValue);
            App.ObjFieldBinding(cbSetAsOutputValue, CheckBox.IsCheckedProperty, mVariable, VariableBase.Fields.SetAsOutputValue);

            if (mode ==eEditMode.Global)
            {
                cbSetAsInputValue.Visibility = Visibility.Hidden;
                cbSetAsOutputValue.Visibility = Visibility.Hidden;
            }
            else
            {              
                cbSetAsInputValue.Visibility=Visibility.Visible;
                cbSetAsOutputValue.Visibility = Visibility.Visible;
            }
            if(setGeneralConfigsAsReadOnly)
            {
                txtVarName.IsReadOnly = true;
                txtVarDescritpion.IsReadOnly = true;
                cbSetAsInputValue.IsEnabled = false;
                cbSetAsOutputValue.IsEnabled = false;
            }             
            
            mVariable.PropertyChanged += mVariable_PropertyChanged;
            LoadVarPage();

            SetLinkedVarCombo();

            if (mVariable.Tags == null)
                mVariable.Tags = new ObservableList<Guid>();
            TagsViewer.Init(mVariable.Tags);

            if (editMode == eEditMode.BusinessFlow || editMode == eEditMode.Activity)
                SharedRepoInstanceUC.Init(mVariable, App.BusinessFlow);
            else
            {
                SharedRepoInstanceUC.Visibility = Visibility.Collapsed;
                SharedRepoInstanceUC_Col.Width = new GridLength(0);
            }
        }

        private void LoadVarPage()
        {           
            try
            {
                if (mVariable.VariableEditPage != null)
                {
                    Type t = Assembly.GetExecutingAssembly().GetType("Ginger.Variables." + mVariable.VariableEditPage);
                    Page varTypeConfigsPage = (Page)Activator.CreateInstance(t, mVariable);
                    if (varTypeConfigsPage != null)
                    {              
                        frmVarTypeInfo.Content = varTypeConfigsPage;
                    }                    
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to load the variable type configurations page", ex);
            }
        }

        private void mVariable_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == VariableBase.Fields.VariableEditPage)
            {
                LoadVarPage();
            }
        }

        private string RemoveVariableWord(string str)
        {
            return str.Replace(GingerDicser.GetTermResValue(eTermResKey.Variable), "").Trim();
        }

        public bool ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free, bool startupLocationWithOffset = false)
        {
            ObservableList<Button> winButtons = new ObservableList<Button>();

            string[] varTypeElems = mVariable.GetType().ToString().Split('.');
            string varType = varTypeElems[varTypeElems.Count() - 1];

            string title = "Edit " + RemoveVariableWord(mVariable.VariableUIType) + " " + GingerDicser.GetTermResValue(eTermResKey.Variable);

            switch (editMode)
            {                
                case VariableEditPage.eEditMode.BusinessFlow:
                case VariableEditPage.eEditMode.Activity:
                case VariableEditPage.eEditMode.Global:
                case VariableEditPage.eEditMode.View:
                    Button okBtn = new Button();
                    okBtn.Content = "Ok";
                    okBtn.Click += new RoutedEventHandler(okBtn_Click);
                    winButtons.Add(okBtn);
                    break;
                case VariableEditPage.eEditMode.SharedRepository:
                    title = "Edit Shared Repository " + RemoveVariableWord(mVariable.VariableUIType) + " " + GingerDicser.GetTermResValue(eTermResKey.Variable);
                    Button saveBtn = new Button();
                    saveBtn.Content = "Save";
                    saveBtn.Click += new RoutedEventHandler(saveBtn_Click);
                    winButtons.Add(saveBtn);
                    break;
                case VariableEditPage.eEditMode.FindAndReplace:
                    title = "Edit " + RemoveVariableWord(mVariable.VariableUIType) + " " + GingerDicser.GetTermResValue(eTermResKey.Variable);
                    Button FindAndRepalceSaveBtn = new Button();
                    FindAndRepalceSaveBtn.Content = "Save";
                    FindAndRepalceSaveBtn.Click += new RoutedEventHandler(FindAndRepalceSaveBtn_Click);
                    winButtons.Add(FindAndRepalceSaveBtn);
                    break; 
            }

            if (editMode != eEditMode.View)
            {
                Button undoBtn = new Button();
                undoBtn.Content = "Undo & Close";
                undoBtn.Click += new RoutedEventHandler(undoBtn_Click);
                winButtons.Add(undoBtn);
                if (!(mVariable is VariableString) && !(mVariable is VariableSelectionList) && !(mVariable is VariableDynamic))
                {
                    Button AutoValueBtn = new Button();
                    AutoValueBtn.Content = "Generate Auto Value";
                    AutoValueBtn.Click += new RoutedEventHandler(AutoValueBtn_Click);
                    winButtons.Add(AutoValueBtn);
                }
                if (!(mVariable is VariableRandomString) && !(mVariable is VariableRandomNumber))
                {
                    Button resetBtn = new Button();
                    resetBtn.Content = "Reset Value";
                    resetBtn.Click += new RoutedEventHandler(resetBtn_Click);
                    winButtons.Add(resetBtn);
                }
            }

            this.Height = 800;
            this.Width = 800;
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, string.Empty, CloseWinClicked, startupLocationWithOffset: startupLocationWithOffset);
            return saveWasDone;
        }

        private void FindAndRepalceSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SharedRepositoryOperations.CheckIfSureDoingChange(mVariable, "change") == true)
            {
                try
                {                    
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mParent);                    
                    saveWasDone = true;
                }
                catch
                {
                    Reporter.ToUser(eUserMsgKeys.Failedtosaveitems);
                }
                _pageGenericWin.Close();
            }
        }

        private void UndoChangesAndClose()
        {
            Mouse.OverrideCursor = Cursors.Wait;            
            mVariable.RestoreFromBackup(true);
            Mouse.OverrideCursor = null;

            _pageGenericWin.Close();
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.ToSaveChanges) == MessageBoxResult.No)
            {
                UndoChangesAndClose();
            }
            else
            {
                if (editMode == eEditMode.SharedRepository)
                    CheckIfUserWantToSave();
                else
                    _pageGenericWin.Close();
            }
        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            _pageGenericWin.Close();            
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckIfUserWantToSave();
        }

        private void CheckIfUserWantToSave()
        {
            if (SharedRepositoryOperations.CheckIfSureDoingChange(mVariable, "change") == true)
            {
                saveWasDone = true;                
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mVariable);
                _pageGenericWin.Close();
            }
        }

        private void resetBtn_Click(object sender, RoutedEventArgs e)
        {
            mVariable.ResetValue();
        }

        private void AutoValueBtn_Click(object sender, RoutedEventArgs e)
        {
            mVariable.GenerateAutoValue();
        }

        private void SetLinkedVarCombo()
        {
            App.ObjFieldBinding(linkedvariableCombo, ComboBox.SelectedValueProperty, mVariable, VariableBase.Fields.LinkedVariableName);

            List<string> varsList = new List<string>();
            linkedvariableCombo.ItemsSource = varsList;
            varsList.Add(string.Empty); //added to allow unlinking of variable

            //get all similar variables from upper level to link to
            if (App.BusinessFlow != null)
            {
                foreach (VariableBase variable in App.BusinessFlow.GetAllHierarchyVariables())
                {
                    if (variable.GetType() == mVariable.GetType() && variable.Name != mVariable.Name)
                    {
                        if (varsList.Contains(variable.Name) == false)
                            varsList.Add(variable.Name);
                    }
                }
                varsList.Sort();
            }            

            //add previous linked variable if needed
            if (string.IsNullOrEmpty(mVariable.LinkedVariableName)== false)
            {
                if (varsList.Contains(mVariable.LinkedVariableName) == false)
                    varsList.Add(mVariable.LinkedVariableName);                                
                linkedvariableCombo.SelectedValue = mVariable.LinkedVariableName;
                linkedvariableCombo.Text = mVariable.LinkedVariableName;
            }
        }

        private void publishValueToLinkedBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(mVariable.LinkedVariableName) == false)
            {
                try
                {
                    ActSetVariableValue setValueAct = new ActSetVariableValue();
                    setValueAct.VariableName = mVariable.LinkedVariableName;
                    setValueAct.SetVariableValueOption = ActSetVariableValue.eSetValueOptions.SetValue;
                    setValueAct.Value = mVariable.Value;
                    setValueAct.RunOnBusinessFlow = App.BusinessFlow;
                    setValueAct.Execute();

                    if (string.IsNullOrEmpty(setValueAct.Error) == false)
                    {
                        Reporter.ToUser(eUserMsgKeys.StaticErrorMessage, "Failed to publish the value to linked variable.." + System.Environment.NewLine + System.Environment.NewLine + "Error: " + setValueAct.Error);
                    }
                }
                catch(Exception ex)
                {
                    Reporter.ToUser(eUserMsgKeys.StaticErrorMessage, "Failed to publish the value to linked variable." + System.Environment.NewLine + System.Environment.NewLine+ "Error: " + ex.Message );
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Missing linked variable, please configure.");
            }
        }

        private void linkedvariableCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mVariable.OnPropertyChanged(VariableBase.Fields.LinkedVariableName);
        }
    }
}
