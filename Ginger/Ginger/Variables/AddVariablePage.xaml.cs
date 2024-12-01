#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Environments;
using GingerCore.Variables;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for AddVariablePage.xaml
    /// </summary>
    public partial class AddVariablePage : Page
    {
        VariablesListViewHelper mLibraryVarsHelper;
        ObservableList<VariableBase> mLibraryVarsList;
        VariablesListViewHelper mSharedRepoVarsHelper;
        ObservableList<VariableBase> mSharedRepoVarsList;
        GenericWindow _pageGenericWin = null;
        eVariablesLevel mVariablesLevel;
        RepositoryItemBase mVariablesParentObj;
        Context mContext;

        public AddVariablePage(eVariablesLevel variablesLevel, RepositoryItemBase variablesParentObj, Context context)
        {
            InitializeComponent();

            mVariablesLevel = variablesLevel;
            mContext = context;
            mVariablesParentObj = variablesParentObj;


            if (variablesLevel.Equals(eVariablesLevel.Activity) || variablesLevel.Equals(eVariablesLevel.BusinessFlow))
            {
                ControlsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ControlsPanel.Visibility = Visibility.Collapsed;

            }

            SetUIControlsContent();
        }

        private void SetUIControlsContent()
        {
            mLibraryVarsList = LoadLibraryVarsList();
            mLibraryVarsHelper = new VariablesListViewHelper(mLibraryVarsList, mVariablesParentObj, mVariablesLevel, mContext, General.eRIPageViewMode.Add)
            {
                AllowExpandItems = false
            };

            xLibraryTabListView.xListView.SelectionChanged += OnSelectionChanged;
            xLibraryTabListView.SetDefaultListDataTemplate(mLibraryVarsHelper);
            xLibraryTabListView.DataSourceList = mLibraryVarsList;
            xLibraryTabListView.MouseDoubleClick += XLibraryTabListView_MouseDoubleClick;
            xLibraryTabListView.ExpandCollapseBtnVisiblity = Visibility.Collapsed;
            xLibraryTabListView.SearchGridVisibility = Visibility.Collapsed;
            xLibraryTabListView.TagsVisibility = Visibility.Collapsed;
            xVariableDetailsDockPanel.Visibility = Visibility.Visible;
            if (mVariablesLevel.Equals(eVariablesLevel.EnvApplication))
            {
                xLibraryTabHeaderText.Text = string.Format("{0} Library ({1})", "Parameter", mLibraryVarsList.Count);
                xLibraryTab.Style = this.FindResource("$CoolMainTab") as Style;
                xSharedRepoTabListView.Visibility = Visibility.Collapsed;
                xSharedRepoTab.Visibility = Visibility.Collapsed;
                VariablePage.MinHeight = 400;
                VariablePage.MinWidth = 700;
                xLibraryTabListView.MaxHeight = 400;

            }
            else
            {
                xLibraryTabHeaderText.Text = string.Format("{0} Library ({1})", GingerDicser.GetTermResValue(eTermResKey.Variables), mLibraryVarsList.Count);
                mSharedRepoVarsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
                mSharedRepoVarsHelper = new VariablesListViewHelper(mLibraryVarsList, mVariablesParentObj, mVariablesLevel, mContext, General.eRIPageViewMode.AddFromShardRepository);
                xSharedRepoTabHeaderText.Text = string.Format("Shared Repository {0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Variables), mSharedRepoVarsList.Count);
                xSharedRepoTabListView.SetDefaultListDataTemplate(mSharedRepoVarsHelper);
                xSharedRepoTabListView.DataSourceList = mSharedRepoVarsList;
                xSharedRepoTabListView.MouseDoubleClick += XSharedRepoTabListView_MouseDoubleClick;
                VariablePage.MinHeight = 600;
                VariablePage.MinWidth = 800;
                xLibraryTabListView.MaxHeight = 600;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var SelectedListView = xLibraryTabListView.xListView.SelectedItem;

            if (SelectedListView == null)
            {
                ValueStackPanel.Visibility = Visibility.Collapsed;
                return;
            }

            if (SelectedListView is VariableRandomNumber or VariableRandomString or VariableTimer or VariableSelectionList)
            {
                ValueStackPanel.Visibility = Visibility.Collapsed;
            }

            else if (SelectedListView is VariableString or VariableNumber or VariablePasswordString or VariableSequence or VariableDynamic)
            {
                ValueStackPanel.Visibility = Visibility.Visible;

            }

            else if (SelectedListView is VariableDateTime)
            {
                ValueStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void XLibraryTabListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AddLibraryVariables();
        }

        private void XSharedRepoTabListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (xSharedRepoTabListView.CurrentItem != null)
            {
                VariableEditPage w = new VariableEditPage((VariableBase)xSharedRepoTabListView.CurrentItem, mContext, false, VariableEditPage.eEditMode.SharedRepository);
                w.ShowAsWindow(eWindowShowStyle.Dialog);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectVariable);
            }
        }

        private ObservableList<VariableBase> LoadLibraryVarsList()
        {

            ObservableList<VariableBase> list = [];

            var varTypes = from type in typeof(VariableBase).Assembly.GetTypes()
                           where type.IsSubclassOf(typeof(VariableBase))
                           && type != typeof(VariableBase)
                           select type;
            if (mVariablesLevel.Equals(eVariablesLevel.EnvApplication))
            {
                var selectedVarTypes = varTypes.Where((type) => type.Name.Equals(typeof(VariablePasswordString).Name) || type.Name.Equals(typeof(VariableString).Name) || type.Name.Equals(typeof(VariableNumber).Name) || type.Name.Equals(typeof(VariableDynamic).Name));

                varTypes = selectedVarTypes;
            }

            VariableString variableString = null;
            int varStrIndex = -1;
            int pointer = 0;
            foreach (Type t in varTypes)
            {
                VariableBase v = (VariableBase)Activator.CreateInstance(t);

                if (v is VariableString vs)
                {
                    variableString = vs;
                    varStrIndex = pointer;
                }
                v.Name = (mVariablesLevel.Equals(eVariablesLevel.EnvApplication)) ? v.VariableUIType.Replace("Variable", "Parameter") : v.VariableUIType;
                if (!v.IsObsolete)
                {
                    list.Add(v);
                    pointer++;
                }

            }

            if (varStrIndex != -1 && variableString != null)
            {
                var tempVariable = list[0];
                list[0] = variableString;
                list[varStrIndex] = tempVariable;
            }

            return list;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {

            ObservableList<Button> buttons = [];

            if (mVariablesLevel.Equals(eVariablesLevel.EnvApplication))
            {
                this.Title = "Add Parameter";

                Button addParameterBtn = new()
                {
                    Content = "Add Parameter"
                };
                addParameterBtn.Click += new RoutedEventHandler(AddVariableButton_Click);

                Button addParameterBtnToAllEn = new()
                {
                    Content = "Add Parameter to all Environments"
                };
                addParameterBtnToAllEn.Click += new RoutedEventHandler(AddVariablesToAllTheEnvironmentsButton_Click);
                buttons.Add(addParameterBtnToAllEn);
                buttons.Add(addParameterBtn);
            }

            else
            {
                this.Title = "Add " + GingerDicser.GetTermResValue(eTermResKey.Variable);
                Button addVarBtn = new Button
                {
                    Content = "Add " + GingerDicser.GetTermResValue(eTermResKey.Variable)
                };
                addVarBtn.Click += new RoutedEventHandler(AddVariableButton_Click);
                buttons.Add(addVarBtn);
            }

            this.Width = 400;
            this.Height = 400;

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, buttons);
        }

        private void AddVariableButton_Click(object sender, RoutedEventArgs e)
        {
            bool isVariableAdded = false;
            if (xLibraryTab.IsSelected)
            {
                isVariableAdded = AddLibraryVariables();
            }
            else
            {
                isVariableAdded = AddSharedRepoVariables();
            }

            if (isVariableAdded)
            {
                _pageGenericWin.Close();
            }
        }


        /// <summary>
        /// ONLY for Environment Page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddVariablesToAllTheEnvironmentsButton_Click(object sender, RoutedEventArgs e)
        {
            VariableBase varToAdd = (VariableBase)xLibraryTabListView.xListView.SelectedItem;


            string Name = variableName.Text;
            string Description = variableDescription.Text;
            string? Value = varToAdd is VariableDateTime dateTimeVarToAdd ? dateTimeVarToAdd.MinDateTime.ToString() : variableValue.Text;

            if (Name.Trim().Length == 0)
            {
                NameError.Visibility = Visibility.Visible;
                return;
            }

            NameError.Visibility = Visibility.Hidden;


            if (varToAdd == null)
            {
                return;
            }

            varToAdd = (VariableBase)varToAdd.CreateCopy();
            varToAdd.Name = Name;
            varToAdd.Description = Description;
            varToAdd.SetInitialSetup();
            varToAdd.SetInitialValue(Value ?? string.Empty);

            ObservableList<ProjEnvironment> ProjEnvironments = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();

            ProjEnvironments.ForEach((projEnv) =>
            {
                projEnv.StartDirtyTracking();

                projEnv.Applications.Where((envApp) =>
                {
                    return envApp.Name.Equals(((EnvApplication)mVariablesParentObj).Name) && !envApp.Variables.Any((var) => var.Name.Equals(varToAdd.Name));
                })
                .ForEach((filteredApp) =>
                {
                    filteredApp.StartDirtyTracking();

                    filteredApp.Variables.Add((VariableBase)varToAdd.CreateCopy());
                });
            });


            _pageGenericWin.Close();
        }

        private bool AddLibraryVariables()
        {
            VariableBase varToAdd = (VariableBase)xLibraryTabListView.xListView.SelectedItem;
            string Name = variableName.Text;
            string Description = variableDescription.Text;
            string? Value = xLibraryTabListView.xListView.SelectedItem is VariableDateTime selectedDateTimeVar ? selectedDateTimeVar.MinDateTime.ToString() : variableValue.Text;

            if (Name.Trim().Length == 0)
            {
                NameError.Visibility = Visibility.Visible;
                return false;
            }

            NameError.Visibility = Visibility.Hidden;


            if (varToAdd == null)
            {
                return false;
            }


            VariableBase addedVar = (VariableBase)varToAdd.CreateCopy();
            addedVar.SetInitialSetup();
            addedVar.Name = Name;
            addedVar.Description = Description;
            addedVar.SetInitialValue(Value ?? string.Empty);

            if (mVariablesLevel.Equals(eVariablesLevel.Activity) || mVariablesLevel.Equals(eVariablesLevel.BusinessFlow))
            {
                addedVar.SetAsInputValue = xSetAsInputValueCheckBox.IsChecked ?? false;
                addedVar.SetAsOutputValue = xSetAsOutputValueCheckBox.IsChecked ?? false;
                addedVar.MandatoryInput = xMandatoryInputCheckBox.IsChecked ?? false;
                addedVar.Publish = xPublishcheckbox.IsChecked ?? false;
            }

            AddVarToParent(addedVar);
            return true;
        }

        private bool AddSharedRepoVariables()
        {
            foreach (VariableBase varToAdd in xSharedRepoTabListView.List.SelectedItems)
            {
                AddVarToParent((VariableBase)varToAdd.CreateInstance(true));
            }

            return true;
        }

        private void AddVarToParent(VariableBase newVar)
        {
            switch (mVariablesLevel)
            {
                case eVariablesLevel.Solution:
                    if (((Solution)mVariablesParentObj).Variables.CurrentItem != null)
                    {
                        ((Solution)mVariablesParentObj).AddVariable(newVar, ((Solution)mVariablesParentObj).Variables.IndexOf((VariableBase)((Solution)mVariablesParentObj).Variables.CurrentItem) + 1);
                    }
                    else
                    {
                        ((Solution)mVariablesParentObj).AddVariable(newVar);
                    }
                    ((Solution)mVariablesParentObj).Variables.CurrentItem = newVar;
                    break;
                case eVariablesLevel.BusinessFlow:
                    if (((BusinessFlow)mVariablesParentObj).Variables.CurrentItem != null)
                    {
                        ((BusinessFlow)mVariablesParentObj).AddVariable(newVar, ((BusinessFlow)mVariablesParentObj).Variables.IndexOf((VariableBase)((BusinessFlow)mVariablesParentObj).Variables.CurrentItem) + 1);
                    }
                    else
                    {
                        ((BusinessFlow)mVariablesParentObj).AddVariable(newVar);
                    }
                    ((BusinessFlow)mVariablesParentObj).Variables.CurrentItem = newVar;
                    break;
                case eVariablesLevel.Activity:
                    if (((Activity)mVariablesParentObj).Variables.CurrentItem != null)
                    {
                        ((Activity)mVariablesParentObj).AddVariable(newVar, ((Activity)mVariablesParentObj).Variables.IndexOf((VariableBase)((Activity)mVariablesParentObj).Variables.CurrentItem) + 1);
                    }
                    else
                    {
                        ((Activity)mVariablesParentObj).AddVariable(newVar);
                    }
                     ((Activity)mVariablesParentObj).Variables.CurrentItem = newVar;
                    break;
                case eVariablesLevel.EnvApplication:

                    ((EnvApplication)mVariablesParentObj).AddVariable(newVar);
                    break;
            }
        }

        private void InputOutputChecked(object sender, RoutedEventArgs e)
        {
            xPublishcheckbox.Visibility = Visibility.Visible;

            if ((bool)xSetAsInputValueCheckBox.IsChecked)
            {
                xMandatoryInputCheckBox.Visibility = Visibility.Visible;
            }
        }

        private void InputOutputUnChecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)xSetAsInputValueCheckBox.IsChecked && !(bool)xSetAsOutputValueCheckBox.IsChecked)
            {
                xPublishcheckbox.IsChecked = false;
                xPublishcheckbox.Visibility = Visibility.Collapsed;
            }

            if (!(bool)xSetAsInputValueCheckBox.IsChecked)
            {
                xMandatoryInputCheckBox.IsChecked = false;
                xMandatoryInputCheckBox.Visibility = Visibility.Collapsed;
            }
        }
    }
}
