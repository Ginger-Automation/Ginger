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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Variables;
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

            SetUIControlsContent();
        }

        private void SetUIControlsContent()
        {
            mLibraryVarsList = LoadLibraryVarsList();
            mLibraryVarsHelper = new VariablesListViewHelper(mLibraryVarsList, mVariablesParentObj, mVariablesLevel, mContext, General.eRIPageViewMode.Add);
            mLibraryVarsHelper.AllowExpandItems = false;
            xLibraryTabHeaderText.Text = string.Format("{0} Library ({1})", GingerDicser.GetTermResValue(eTermResKey.Variables), mLibraryVarsList.Count);
            xLibraryTabListView.SetDefaultListDataTemplate(mLibraryVarsHelper);
            xLibraryTabListView.DataSourceList = mLibraryVarsList;
            xLibraryTabListView.MouseDoubleClick += XLibraryTabListView_MouseDoubleClick;

            mSharedRepoVarsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
            mSharedRepoVarsHelper = new VariablesListViewHelper(mLibraryVarsList, mVariablesParentObj, mVariablesLevel, mContext, General.eRIPageViewMode.AddFromShardRepository);
            xSharedRepoTabHeaderText.Text = string.Format("Shared Repository {0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Variables), mSharedRepoVarsList.Count);
            xSharedRepoTabListView.SetDefaultListDataTemplate(mSharedRepoVarsHelper);
            xSharedRepoTabListView.DataSourceList = mSharedRepoVarsList;
            xSharedRepoTabListView.MouseDoubleClick += XSharedRepoTabListView_MouseDoubleClick;
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
            ObservableList<VariableBase> list = new ObservableList<VariableBase>();
            var varTypes = from type in typeof(VariableBase).Assembly.GetTypes()
                           where type.IsSubclassOf(typeof(VariableBase))
                           && type != typeof(VariableBase)
                           select type;

            foreach (Type t in varTypes)
            {
                VariableBase v = (VariableBase)Activator.CreateInstance(t);
                v.Name = v.VariableUIType;
                if (!v.IsObsolete)
                {
                    list.Add(v);
                }
            }

            return list;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            this.Title = "Add " + GingerDicser.GetTermResValue(eTermResKey.Variable);

            Button addVarBtn = new Button();
            addVarBtn.Content = "Add " + GingerDicser.GetTermResValue(eTermResKey.Variable);
            addVarBtn.Click += new RoutedEventHandler(AddVariableButton_Click);

            this.Width = 400;
            this.Height = 400;

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { addVarBtn });
        }

        private void AddVariableButton_Click(object sender, RoutedEventArgs e)
        {
            if (xLibraryTab.IsSelected)
            {
                AddLibraryVariables();
            }
            else
            {
                AddSharedRepoVariables();
            }

            _pageGenericWin.Close();
        }

        private void AddLibraryVariables()
        {
            foreach (VariableBase varToAdd in xLibraryTabListView.List.SelectedItems)
            {
                VariableBase addedVar = (VariableBase)varToAdd.CreateCopy();
                addedVar.SetInitialSetup();
                AddVarToParent(addedVar);
            }
        }

        private void AddSharedRepoVariables()
        {
            foreach (VariableBase varToAdd in xSharedRepoTabListView.List.SelectedItems)
            {
                AddVarToParent((VariableBase)varToAdd.CreateInstance(true));
            }
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
            }
        }
    }
}
