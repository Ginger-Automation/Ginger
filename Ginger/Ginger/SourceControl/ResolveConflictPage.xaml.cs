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
using GingerCore;
using GingerCore.Helpers;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SourceControl
{
    /// <summary>
    /// Interaction logic for ResolveConflictPage.xaml
    /// </summary>
    public partial class ResolveConflictPage : Page
    {
        private enum eResolveOperations
        {
            [EnumValueDescription("Accept Server Changes for All Conflicts")]
            AcceptServer,
            [EnumValueDescription("Keep Local Changes for All Conflicts")]
            KeepLocal
        }

        public bool IsResolved = true; 

        GenericWindow genWin = null;
        string mConflictPath = string.Empty;
        eResolveOperations mResolveOperation = eResolveOperations.AcceptServer;

        public ResolveConflictPage(string conflictPath)
        {
            InitializeComponent();

            mConflictPath = conflictPath;

            SetMessageText();

            GingerCore.General.FillComboFromEnumObj(ConflictResolveOperationCombo, mResolveOperation);
            ConflictResolveOperationCombo.Text = ConflictResolveOperationCombo.Items[0].ToString();
            ConflictResolveOperationCombo.SelectionChanged += ConflictResolveOperationCombo_SelectionChanged;
        }

        private void ConflictResolveOperationCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConflictResolveOperationCombo.SelectedValue.ToString() == eResolveOperations.AcceptServer.ToString())
                mResolveOperation = eResolveOperations.AcceptServer;
            else
                mResolveOperation = eResolveOperations.KeepLocal;
        }

        private void SetMessageText()
        {
            UserMessaheTextBlock.Text = string.Empty;
            TextBlockHelper TBH = new TextBlockHelper(UserMessaheTextBlock);
            TBH.AddText("Source control conflicts has been identified for the path:");
            TBH.AddLineBreak();
            TBH.AddBoldText(mConflictPath);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("Conflicts are usually been created when 2 users working on the same item in parallel and one of the users check-in his changes.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddFormattedText("You probably won't be able to use the item which mention in the path till conflicts will be resolved.", System.Windows.Media.Brushes.Red,true,true);
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("Please select below if you want to keep your changes or get server changes for each conflict.");
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button resolveBtn = new Button();
            resolveBtn.Content = "Resolve";
            resolveBtn.Click += new RoutedEventHandler(resolve_Click);

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Source Control Conflicts", this, new ObservableList<Button> { resolveBtn }, true, "Do Not Resolve", CloseWindow);
        }

        private void resolve_Click(object sender, EventArgs e)
        {
            Reporter.ToStatus(eStatusMsgKey.ResolveSourceControlConflicts);
            switch (mResolveOperation)
            {
                case eResolveOperations.AcceptServer:
                    SourceControlIntegration.ResolveConflicts(WorkSpace.Instance.Solution.SourceControl, mConflictPath, eResolveConflictsSide.Server);
                    break;
                case eResolveOperations.KeepLocal:
                    SourceControlIntegration.ResolveConflicts(WorkSpace.Instance.Solution.SourceControl, mConflictPath, eResolveConflictsSide.Local);
                    break;
                default:
                    //do nothing
                    break;
            }
            Reporter.HideStatusMessage();
            CloseWindow();
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            IsResolved = false;
            CloseWindow();
        }

        private void CloseWindow()
        {
            genWin.Close();
        }
    }
}