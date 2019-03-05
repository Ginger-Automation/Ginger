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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GingerCore;
using GingerCore.Variables;
using Ginger.UserControls;
using Ginger.Environments;
using Ginger.SolutionGeneral;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for AddVariablePage.xaml
    /// </summary>
    public partial class AddVariablePage : Page
    {
        GenericWindow _pageGenericWin = null;
        private eVariablesLevel mVariablesLevel;
        private object mVariablesParentObj;

        Context mContext = new Context();

        public AddVariablePage(eVariablesLevel variablesLevel, object variablesParentObj)
        {
            InitializeComponent();

            this.Title = "Add " + GingerDicser.GetTermResValue(eTermResKey.Variable);

            mVariablesLevel = variablesLevel;
            mVariablesParentObj = variablesParentObj;
            if(variablesLevel == eVariablesLevel.BusinessFlow)
            { 
                mContext.BusinessFlow = (BusinessFlow)variablesParentObj;
            }
            SetVariablesGridView();
            LoadGridData();
            VariablesGrid.RowDoubleClick += VariablesGrid_grdMain_MouseDoubleClick;
        }

        private void LoadGridData()
        {
            ObservableList<VariableBase> l = new ObservableList<VariableBase>();
            var varTypes = from type in typeof(VariableBase).Assembly.GetTypes()
                           where type.IsSubclassOf(typeof(VariableBase))
                           && type != typeof(VariableBase)
                           select type;


            foreach (Type t in varTypes)
            {
                VariableBase v = (VariableBase)Activator.CreateInstance(t);
                l.Add(v);
            }

            VariablesGrid.DataSourceList = l;
        }

        private void SetVariablesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = nameof(VariableBase.VariableUIType) , Header= GingerDicser.GetTermResValue(eTermResKey.Variable) + " Type", BindingMode=BindingMode.OneWay, ReadOnly=true});

            VariablesGrid.SetAllColumnsDefaultView(view);
            VariablesGrid.InitViewItems();
        }

        private void AddVariableButton_Click(object sender, RoutedEventArgs e)
        {
            AddVariable();
        }

        private void VariablesGrid_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            AddVariable();
        }

        private void AddVariable()
        {
            VariableBase newVar= (VariableBase)((VariableBase)VariablesGrid.CurrentItem).CreateCopy();
            if (mVariablesParentObj != null)
            {
                switch (mVariablesLevel)
                {
                    case eVariablesLevel.Solution:
                        ((Solution)mVariablesParentObj).AddVariable(newVar);
                        break;
                    case eVariablesLevel.BusinessFlow:
                        ((BusinessFlow)mVariablesParentObj).AddVariable(newVar);
                        break;
                    case eVariablesLevel.Activity:
                        ((Activity)mVariablesParentObj).AddVariable(newVar);
                        break;
                }
            }
            else
            {
                return;
            }

            VariableEditPage varEditPage = new VariableEditPage(newVar, mContext);
            _pageGenericWin.Close();
            varEditPage.ShowAsWindow(eWindowShowStyle.Dialog);

            //make sure name is unique
            switch (mVariablesLevel)
            {
                case eVariablesLevel.Solution:
                    ((Solution)mVariablesParentObj).SetUniqueVariableName(newVar);
                    break;
                case eVariablesLevel.BusinessFlow:
                    ((BusinessFlow)mVariablesParentObj).SetUniqueVariableName(newVar);
                    break;
                case eVariablesLevel.Activity:
                    ((Activity)mVariablesParentObj).SetUniqueVariableName(newVar);
                    break;
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button addVarBtn = new Button();
            addVarBtn.Content = "Add " + GingerDicser.GetTermResValue(eTermResKey.Variable);
            addVarBtn.Click += new RoutedEventHandler(AddVariableButton_Click);
         
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { addVarBtn });
        }
    }
}
