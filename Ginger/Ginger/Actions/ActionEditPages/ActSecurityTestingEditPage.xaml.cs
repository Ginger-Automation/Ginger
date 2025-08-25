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
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Linq;
using System.Windows.Controls;
using static Amdocs.Ginger.CoreNET.ActionsLib.UI.Web.ActSecurityTesting;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActSecurityTestingEditPage.xaml
    /// </summary>
    public partial class ActSecurityTestingEditPage : Page
    {
        private ActSecurityTesting mAct;
        readonly PlatformInfoBase mPlatform;

        public ActSecurityTestingEditPage(ActSecurityTesting act)
        {
            InitializeComponent();
            mAct = act;
            if (act.Platform == ePlatformType.NA)
            {
                act.Platform = GetActionPlatform();
            }
            mPlatform = PlatformInfoBase.GetPlatformImpl(act.Platform);

            //bind controls
            ObservableList<OperationValues> AlertList = GetAlertList();

            mAct.AlertItems = [];
            foreach (OperationValues alert in AlertList)
            {
                if (!string.IsNullOrEmpty(alert.Value.ToString()))
                {
                    mAct.AlertItems.Add(alert.Value.ToString(), alert);
                }
            }
            xSeverityCB.ItemsSource = mAct.AlertItems;
            xSeverityCB.Init(mAct, nameof(mAct.AlertList));

            GingerCore.General.FillComboFromEnumType(xScanTypeComboBox, typeof(eScanType), null);
            xScanTypeComboBox.Text = GingerCore.General.GetEnumValueDescription(typeof(eScanType), eScanType.Active);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xScanTypeComboBox, ComboBox.SelectedValueProperty, mAct, ActSecurityTesting.Fields.ScanType);

        }

        private ePlatformType GetActionPlatform()
        {
            ePlatformType platform;
            if (mAct.Context != null && (Context.GetAsContext(mAct.Context)).BusinessFlow != null)
            {
                string targetapp = (Context.GetAsContext(mAct.Context)).BusinessFlow.CurrentActivity.TargetApplication;
                platform = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == targetapp).Platform;
            }
            else
            {
                platform = WorkSpace.Instance.Solution.ApplicationPlatforms[0].Platform;
            }
            return platform;
        }

        private void xScanTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xScanTypeComboBox.SelectedValue is string selectedScanType)
            {
                if (Enum.TryParse(typeof(ActSecurityTesting.eScanType), selectedScanType, out var scanTypeEnum))
                {
                    mAct.ScanType = (ActSecurityTesting.eScanType)scanTypeEnum;
                }
            }


        public ObservableList<OperationValues> GetAlertList()
        {
            ObservableList<OperationValues> SeverityList =
            [
                new OperationValues() { Value = nameof(ActSecurityTesting.eAlertTypes.High) },
                new OperationValues() { Value = nameof(ActSecurityTesting.eAlertTypes.Low) },
                new OperationValues() { Value = nameof(ActSecurityTesting.eAlertTypes.Medium) },
                new OperationValues() { Value = nameof(ActSecurityTesting.eAlertTypes.FalsePositive) },
                new OperationValues() { Value = nameof(ActSecurityTesting.eAlertTypes.Informational) },
            ];
            return SeverityList;
        }
    }
}
