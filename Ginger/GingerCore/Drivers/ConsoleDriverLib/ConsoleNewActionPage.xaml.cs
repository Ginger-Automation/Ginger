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

using Amdocs.Ginger.Common;
using System.Windows;
using System.Windows.Controls;
using GingerCore.Actions;
using Ginger;

namespace GingerCore.Drivers.ConsoleDriverLib
{
    /// <summary>
    /// Interaction logic for ConsoleNewActionPage.xaml
    /// </summary>
    public partial class ConsoleNewActionPage : Page
    {
        private ConsoleDriverBase mConsoleDriverBase;
        private BusinessFlow mBF;
        ActConsoleCommand ACC = new ActConsoleCommand();
      
        public ConsoleNewActionPage(ConsoleDriverBase CDB, BusinessFlow BF)
        {
            InitializeComponent();
            mConsoleDriverBase=CDB;
            mBF = BF;
            ACC.AddOrUpdateInputParamValue("Param 1", "");
            ACC.AddOrUpdateInputParamValue("Param 2", "");
            ACC.AddOrUpdateInputParamValue("Param 3", "");

            ParamsDatGrid.ItemsSource = ACC.InputValues;
            ParamsDatGrid.AutoGenerateColumns = false;
        }

        internal void ShowAsWindow(System.Windows.Window owner)
        {
            Button runActionBtn = new Button();
            runActionBtn.Content = "Run Action";
            runActionBtn.Click += new RoutedEventHandler(RunActionButton_Click);

            Button AddActionBtn = new Button();
            AddActionBtn.Content = "Add to " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            AddActionBtn.Click += new RoutedEventHandler(AddActionBtn_Click);
            
            ObservableList<Button> Buttons = new ObservableList<Button>();
            Buttons.Add(runActionBtn);
            Buttons.Add(AddActionBtn);
            
            GenericWindow genWin=null;
            GingerCore.General.LoadGenericWindow(ref genWin, owner, Ginger.eWindowShowStyle.Free, this.Title, this, Buttons);
        }

        private void AddActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mBF != null)
            {
                mBF.AddAct(ACC);
            }
            else
            {                
                Reporter.ToUser(eUserMsgKey.AskToSelectBusinessflow);
            }
        }

        private void RunActionButton_Click(object sender, RoutedEventArgs e)
        {
            ACC.ConsoleCommand = ActConsoleCommand.eConsoleCommand.ParametrizedCommand;
            ACC.Command = CommandTextBox.Text;
            ACC.Description = "Run Command " + ACC.Command;
            ACC.Active = true;            
            mConsoleDriverBase.RunAction(ACC);
        }
    }
}
