#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.Reports.ValidationRules;
using Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionGeneral;
using Ginger.UserControlsLib;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for VariableEditPage.xaml
    /// </summary>
    public partial class VariableEditPage : GingerUIPage
    {
        private VariableBase mVariable;
        private RepositoryItemBase mParent;
        bool saveWasDone = false;
        static bool ExpandDetails = false;
        static bool HasUserSetPreference = false;

        GenericWindow _pageGenericWin = null;

        public enum eEditMode
        {
            SharedRepository = 0,
            Default = 1,
            Global = 4,
            FindAndReplace = 5,
            View = 6
        }

        public eEditMode editMode { get; set; }

        Context mContext;

        public VariableEditPage(VariableBase v, Context context, bool setGeneralConfigsAsReadOnly = false, eEditMode mode = eEditMode.Default, RepositoryItemBase parent = null, bool expandDetails = false)
        {
            InitializeComponent();

            this.Title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.Variable);
            mVariable = v;
            mVariable.SaveBackup();
            editMode = mode;
            mParent = parent;
            mContext = context;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xTypeLbl, Label.ContentProperty, mVariable, nameof(VariableBase.VariableType), BindingMode: BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xVarNameTxtBox, TextBox.TextProperty, mVariable, nameof(VariableBase.Name));
            xVarNameTxtBox.AddValidationRule(new ValidateNotContainSpecificChar(',', "Variable name can't contain a comma (', ')"));
            xVarNameTxtBox.AddValidationRule(new ValidateNotContainSpacesBeforeAfter());
            xShowIDUC.Init(mVariable);
            mVariable.NameBeforeEdit = mVariable.Name;
            xVarNameTxtBox.GotFocus += XVarNameTxtBox_GotFocus;
            xVarNameTxtBox.LostFocus += XVarNameTxtBox_LostFocus;

            if (parent is not null && parent.GOpsFlag)
            {
                xVarNameTxtBox.IsEnabled = false;
                xVarDescritpiontxtBox.IsEnabled = false;
            }

            if (parent is EnvApplication)
            {

                xTitleName.Text = $"{GingerDicser.GetTermResValue(eTermResKey.Parameter)} Type: ";
                xNameConfig.Text = $"{GingerDicser.GetTermResValue(eTermResKey.Parameter)} Type Configurations:";
            }
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xVarDescritpiontxtBox, TextBox.TextProperty, mVariable, nameof(VariableBase.Description));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xFormulaTxtBox, TextBox.TextProperty, mVariable, nameof(VariableBase.Formula), BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xCurrentValueTextBox, TextBox.TextProperty, mVariable, nameof(VariableBase.CurrentEffectiveValue), BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSetAsInputValueCheckBox, CheckBox.IsCheckedProperty, mVariable, nameof(VariableBase.SetAsInputValue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSetAsInputValueCheckBox, CheckBox.VisibilityProperty, mVariable, nameof(VariableBase.SupportSetValue), bindingConvertor: new BoolVisibilityConverter(), BindingMode: BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xMandatoryInputCheckBox, CheckBox.IsCheckedProperty, mVariable, nameof(VariableBase.MandatoryInput));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSetAsOutputValueCheckBox, CheckBox.IsCheckedProperty, mVariable, nameof(VariableBase.SetAsOutputValue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xPublishcheckbox, CheckBox.IsCheckedProperty, mVariable, nameof(RepositoryItemBase.Publish));

            if (mode == eEditMode.Global)
            {
                xSetAsInputValueCheckBox.Visibility = Visibility.Hidden;
                xMandatoryInputCheckBox.Visibility = Visibility.Hidden;
                xSetAsOutputValueCheckBox.Visibility = Visibility.Hidden;
                xSharedRepoInstanceUC.Visibility = Visibility.Collapsed;
                SharedRepoInstanceUC_Col.Width = new GridLength(0);
            }
            else
            {
                if (mode == eEditMode.SharedRepository)
                {
                    xSharedRepoInstanceUC.Visibility = Visibility.Collapsed;
                    SharedRepoInstanceUC_Col.Width = new GridLength(0);
                }
                if (mVariable.SupportSetValue)
                {
                    xSetAsInputValueCheckBox.Visibility = Visibility.Visible;
                }
                xSetAsOutputValueCheckBox.Visibility = Visibility.Visible;
                if (mContext != null && mContext.BusinessFlow != null)
                {
                    xSharedRepoInstanceUC.Init(mVariable, mContext.BusinessFlow);
                }
            }

            if (setGeneralConfigsAsReadOnly)
            {
                xVarNameTxtBox.IsEnabled = false;
                xVarDescritpiontxtBox.Background = System.Windows.Media.Brushes.Transparent;
                xVarDescritpiontxtBox.IsReadOnly = true;
                xTagsViewer.IsEnabled = false;
                xSharedRepoInstanceUC.IsEnabled = false;
                xSetAsInputValueCheckBox.IsEnabled = false;
                xMandatoryInputCheckBox.IsEnabled = false;
                xSetAsOutputValueCheckBox.IsEnabled = false;
                xPublishcheckbox.IsEnabled = false;
                xLinkedvariableCombo.IsEnabled = false;
                xPublishValueToLinkedVarBtn.IsEnabled = false;
            }
            if (editMode == eEditMode.View)
            {
                xVarTypeConfigFrame.IsEnabled = false;
            }
            else
            {
                xVarTypeConfigFrame.IsEnabled = true;
            }

            mVariable.PropertyChanged += mVariable_PropertyChanged;
            LoadVarPage();

            if (parent is EnvApplication)
            {
                LinkedVariableStackPanel.Visibility = Visibility.Collapsed;
                xTagsViewer.Visibility = Visibility.Collapsed;
            }
            else
            {
                SetLinkedVarCombo();
            }

            if (mVariable.Tags == null)
            {
                mVariable.Tags = [];
            }
            xTagsViewer.Init(mVariable.Tags);

            // Use the expandDetails parameter if provided, otherwise use the static ExpandDetails value
            // For BusinessFlow variables, default to expanded only if user hasn't set a preference yet
            bool shouldExpand = expandDetails || ExpandDetails || (!HasUserSetPreference && mParent is BusinessFlow);
            xDetailsExpander.IsExpanded = shouldExpand;
        }

        private void XVarNameTxtBox_GotFocus(object sender, RoutedEventArgs e)
        {
            mVariable.NameBeforeEdit = mVariable.Name;
        }

        private async void XVarNameTxtBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (mVariable.NameBeforeEdit != mVariable.Name)
            {
                await Task.Run(() => UpdateVariableNameChange());
            }
        }

        private void LoadVarPage()
        {
            try
            {
                if (mVariable.VariableEditPage != null)
                {
                    Type t = Assembly.GetExecutingAssembly().GetType("Ginger.Variables." + mVariable.VariableEditPage);
                    Page varTypeConfigsPage = null;
                    if (t != typeof(VariableDynamicPage))
                    {
                        varTypeConfigsPage = (Page)Activator.CreateInstance(t, mVariable);
                    }
                    else
                    {
                        varTypeConfigsPage = (Page)Activator.CreateInstance(t, mVariable, mContext);
                    }


                    if (varTypeConfigsPage != null)
                    {
                        xVarTypeConfigFrame.Content = varTypeConfigsPage;
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load the variable type configurations page", ex);
            }
        }

        private void mVariable_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariableBase.VariableEditPage))
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
            ObservableList<Button> winButtons = [];

            string[] varTypeElems = mVariable.GetType().ToString().Split('.');
            string varType = varTypeElems[^1];

            string title = "Edit " + RemoveVariableWord(mVariable.VariableUIType) + " " + GingerDicser.GetTermResValue(eTermResKey.Variable);

            switch (editMode)
            {
                case VariableEditPage.eEditMode.Default:
                case VariableEditPage.eEditMode.Global:
                case VariableEditPage.eEditMode.View:
                    Button okBtn = new Button
                    {
                        Content = "Ok"
                    };
                    okBtn.Click += new RoutedEventHandler(okBtn_Click);
                    winButtons.Add(okBtn);
                    break;
                case VariableEditPage.eEditMode.SharedRepository:
                    title = "Edit Shared Repository " + RemoveVariableWord(mVariable.VariableUIType) + " " + GingerDicser.GetTermResValue(eTermResKey.Variable);
                    Button saveBtn = new Button
                    {
                        Content = "Save"
                    };
                    saveBtn.Click += new RoutedEventHandler(saveBtn_Click);
                    winButtons.Add(saveBtn);
                    break;
                case VariableEditPage.eEditMode.FindAndReplace:
                    title = "Edit " + RemoveVariableWord(mVariable.VariableUIType) + " " + GingerDicser.GetTermResValue(eTermResKey.Variable);
                    Button FindAndRepalceSaveBtn = new Button
                    {
                        Content = "Save"
                    };
                    FindAndRepalceSaveBtn.Click += new RoutedEventHandler(FindAndRepalceSaveBtn_Click);
                    winButtons.Add(FindAndRepalceSaveBtn);
                    break;
            }

            if (editMode != eEditMode.View)
            {
                Button undoBtn = new Button
                {
                    Content = "Undo & Close"
                };
                undoBtn.Click += new RoutedEventHandler(undoBtn_Click);
                winButtons.Add(undoBtn);
                if (mVariable is not VariableString and not VariableSelectionList and not VariableDynamic)
                {
                    Button AutoValueBtn = new Button
                    {
                        Content = "Generate Auto Value"
                    };
                    AutoValueBtn.Click += new RoutedEventHandler(AutoValueBtn_Click);
                    winButtons.Add(AutoValueBtn);
                }
                if (mVariable is not VariableRandomString and not VariableRandomNumber)
                {
                    Button resetBtn = new Button
                    {
                        Content = "Reset Value"
                    };
                    resetBtn.Click += new RoutedEventHandler(resetBtn_Click);
                    winButtons.Add(resetBtn);
                }
            }

            this.Height = 800;
            this.Width = 800;
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, false, "Undo & Close", CloseWinClicked, startupLocationWithOffset: startupLocationWithOffset);
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
                    Reporter.ToUser(eUserMsgKey.Failedtosaveitems);
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
            if (Reporter.ToUser(eUserMsgKey.AskIfToUndoChanges) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                UndoChangesAndClose();
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
            string errorMsg = string.Empty;
            mVariable.GenerateAutoValue(ref errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                Reporter.ToUser(eUserMsgKey.VariablesAssignError, errorMsg);
            }
        }

        private void SetLinkedVarCombo()
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xLinkedvariableCombo, ComboBox.SelectedValueProperty, mVariable, nameof(VariableBase.LinkedVariableName));

            List<string> varsList = [];
            xLinkedvariableCombo.ItemsSource = varsList;
            varsList.Add(string.Empty); //added to allow unlinking of variable

            //get all similar variables from upper level to link to
            if (mContext != null && mContext.BusinessFlow != null)
            {
                foreach (VariableBase variable in mContext.BusinessFlow.GetAllHierarchyVariables())
                {
                    if (variable.GetType() == mVariable.GetType() && variable.Name != mVariable.Name)
                    {
                        if (varsList.Contains(variable.Name) == false)
                        {
                            varsList.Add(variable.Name);
                        }
                    }
                }
                varsList.Sort();
            }

            //add previous linked variable if needed
            if (string.IsNullOrEmpty(mVariable.LinkedVariableName) == false)
            {
                if (varsList.Contains(mVariable.LinkedVariableName) == false)
                {
                    varsList.Add(mVariable.LinkedVariableName);
                }
                xLinkedvariableCombo.SelectedValue = mVariable.LinkedVariableName;
                xLinkedvariableCombo.Text = mVariable.LinkedVariableName;
            }
        }

        private void publishValueToLinkedBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(mVariable.LinkedVariableName) == false)
            {
                try
                {
                    ActSetVariableValue setValueAct = new ActSetVariableValue
                    {
                        VariableName = mVariable.LinkedVariableName,
                        SetVariableValueOption = VariableBase.eSetValueOptions.SetValue,
                        Value = mVariable.Value,
                        RunOnBusinessFlow = mContext.BusinessFlow,
                        Context = mContext
                    };
                    setValueAct.Execute();

                    if (string.IsNullOrEmpty(setValueAct.Error) == false)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to publish the value to linked " + GingerDicser.GetTermResValue(eTermResKey.Variable) + ".." + System.Environment.NewLine + System.Environment.NewLine + "Error: " + setValueAct.Error);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to publish the value to linked " + GingerDicser.GetTermResValue(eTermResKey.Variable) + "." + System.Environment.NewLine + System.Environment.NewLine + "Error: " + ex.Message);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Missing linked " + GingerDicser.GetTermResValue(eTermResKey.Variable) + ", please configure.");
            }
        }

        private void linkedvariableCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mVariable.OnPropertyChanged(nameof(VariableBase.LinkedVariableName));
        }

        private void XDetailsExpander_ExpandCollapse(object sender, RoutedEventArgs e)
        {
            ExpandDetails = xDetailsExpander.IsExpanded;
            HasUserSetPreference = true;
        }

        public void UpdateVariableNameChange()
        {
            try
            {
                if (mVariable == null)
                {
                    return;
                }

                Reporter.ToStatus(eStatusMsgKey.StaticStatusProcess, null, string.Format("Updating new {0} name '{1}' on all usage instances...", GingerDicser.GetTermResValue(eTermResKey.Variable), mVariable.Name));
                if (mParent is Solution)
                {
                    if (editMode is eEditMode.Global)
                    {
                        ((Solution)mParent).SetUniqueVariableName(mVariable);
                    }
                    else
                    {
                        ObservableList<BusinessFlow> allBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                        Parallel.ForEach(allBF, bfl =>
                        {
                            bfl.SetUniqueVariableName(mVariable);
                            Parallel.ForEach(bfl.Activities, activity =>
                            {
                                Parallel.ForEach(activity.Acts, action =>
                                {
                                    bool changedwasDone = false;
                                    VariableBase.UpdateVariableNameChangeInItem(action, mVariable.NameBeforeEdit, mVariable.Name, ref changedwasDone);
                                });
                            });
                        });
                    }
                }
                else if (mParent is BusinessFlow bf)
                {
                    bf.SetUniqueVariableName(mVariable);
                    Parallel.ForEach(bf.Activities, activity =>
                    {
                        Parallel.ForEach(activity.Acts, action =>
                        {
                            bool changedwasDone = false;
                            VariableBase.UpdateVariableNameChangeInItem(action, mVariable.NameBeforeEdit, mVariable.Name, ref changedwasDone);
                        });
                    });
                }
                else if (mParent is Activity activ)
                {
                    activ.SetUniqueVariableName(mVariable);
                    Parallel.ForEach(activ.Acts, action =>
                    {
                        bool changedwasDone = false;
                        VariableBase.UpdateVariableNameChangeInItem(action, mVariable.NameBeforeEdit, mVariable.Name, ref changedwasDone);
                    });
                }
                else if (mParent is EnvApplication envApplication)
                {
                    envApplication.SetUniqueVariableName(mVariable);

                    ObservableList<BusinessFlow> bfs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();

                    foreach (BusinessFlow businessFlow in bfs)
                    {
                        foreach (Activity activity in businessFlow.Activities)
                        {
                            foreach (Act action in activity.Acts)
                            {
                                bool changedwasDone = false;
                                GeneralParam.UpdateNameChangeInItem(action, envApplication.Name, mVariable.NameBeforeEdit, mVariable.Name, ref changedwasDone);
                            }
                        }
                    }
                }
                if (mVariable.SetAsOutputValue || mParent is Solution)
                {
                    UpdateVariableNameInRunsets();
                }
                mVariable.NameBeforeEdit = mVariable.Name;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Update Name Change", ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }

        private void UpdateVariableNameInRunsets()
        {
            string variableOldName = mVariable.NameBeforeEdit;
            string variableNewName = mVariable.Name;

            foreach (RunSetConfig runset in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>())
            {
                foreach (GingerRunner runner in runset.GingerRunners)
                {
                    foreach (BusinessFlowRun bfRun in runner.BusinessFlowsRunList)
                    {
                        IEnumerable<VariableBase> variablesMappedToOldName = bfRun.BusinessFlowCustomizedRunVariables
                            .Where(variable =>
                            {
                                return string.Equals(variable.MappedOutputValue, variableOldName);
                            });

                        foreach (VariableBase variable in variablesMappedToOldName)
                        {
                            variable.MappedOutputValue = variableNewName;
                        }
                    }
                }
            }
        }

        private void InputOutputChecked(object sender, RoutedEventArgs e)
        {
            xPublishcheckbox.Visibility = Visibility.Visible;

            if (xSetAsInputValueCheckBox.IsChecked == true && mVariable.SupportSetValue)
            {
                xMandatoryInputCheckBox.Visibility = Visibility.Visible;
            }
        }

        private void InputOutputUnChecked(object sender, RoutedEventArgs e)
        {
            if (xSetAsInputValueCheckBox.IsChecked == false && xSetAsOutputValueCheckBox.IsChecked == false)
            {
                xPublishcheckbox.IsChecked = false;
                xPublishcheckbox.Visibility = Visibility.Collapsed;
            }

            if (xSetAsInputValueCheckBox.IsChecked == false)
            {
                xMandatoryInputCheckBox.IsChecked = false;
                xMandatoryInputCheckBox.Visibility = Visibility.Collapsed;
            }
        }

        protected override void IsVisibleChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (editMode == eEditMode.SharedRepository && mVariable != null && mParent == null)
            {
                CurrentItemToSave = mVariable;
                base.IsVisibleChangedHandler(sender, e);
            }
        }
    }
}
