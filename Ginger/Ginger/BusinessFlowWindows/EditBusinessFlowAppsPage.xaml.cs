#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Ginger.UserControls;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using MongoDB.Driver.Linq;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for EditBusinessFlowAppsPage.xaml
    /// </summary>
    public partial class EditBusinessFlowAppsPage : Page
    {
        BusinessFlow mBusinessFlow;
        ObservableList<ApplicationPlatform> mApplicationsPlatforms = [];
        GenericWindow _pageGenericWin = null;
        private bool IsNewBusinessflow = false;
        public EditBusinessFlowAppsPage(BusinessFlow BizFlow, bool IsNewBF = false)
        {

            InitializeComponent();
            AppsGrid.SelectionMode = DataGridSelectionMode.Single;

            this.Title = $"Edit {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}";

            mBusinessFlow = BizFlow;
            IsNewBusinessflow = IsNewBF;
            SetGridView();
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
                [
                    new GridColView() { Field = nameof(ApplicationPlatform.PlatformImage), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 },
                    new GridColView() { Field = "AppName", Header = "Application Name", WidthWeight = 50, ReadOnly = true, BindingMode = BindingMode.OneWay },
                    // Use description so UI shows "Mobile/TV"
                    new GridColView() { Field = nameof(ApplicationPlatform.PlatformDescription), Header = "Platform", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay },
                ]
            };

            AppsGrid.SetAllColumnsDefaultView(view);
            AppsGrid.InitViewItems();

            foreach (ApplicationPlatform AP in WorkSpace.Instance.Solution.ApplicationPlatforms)
            {
                ApplicationPlatform AP1 = new ApplicationPlatform
                {
                    AppName = AP.AppName,
                    Platform = AP.Platform,
                    Guid = AP.Guid
                };

                // If this App was selected before then mark it 
                TargetApplication APS = (TargetApplication)(from x in mBusinessFlow.TargetApplications where x?.Name == AP.AppName select x).FirstOrDefault();

                if (APS != null)
                {
                    AP1.Selected = true;
                }

                mApplicationsPlatforms.Add(AP1);
            }

            AppsGrid.DataSourceList = mApplicationsPlatforms;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsNewBusinessflow)
            {
                SetTargetApplications();
                if (mBusinessFlow.TargetApplications?.Count != 0)
                {
                    mBusinessFlow.CurrentActivity.TargetApplication = mBusinessFlow.TargetApplications[0].Name;
                }
            }
            else
            {
                if (!mApplicationsPlatforms.Any(x => x.Selected))
                {
                    Reporter.ToUser(eUserMsgKey.BusinessFlowNeedTargetApplication, GingerDicser.GetTermResValue(eTermResKey.TargetApplication));
                    return;
                }
                SetTargetApplications();
                if (mBusinessFlow.TargetApplications.Count == 1)
                {
                    foreach (Activity activity in mBusinessFlow.Activities)
                    {
                        activity.TargetApplication = mBusinessFlow.TargetApplications[0].Name;
                    }
                }
            }
            if (mBusinessFlow.TargetApplications.Count > 0 || mApplicationsPlatforms.Count == 0)
            {
                _pageGenericWin.Close();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.BusinessFlowNeedTargetApplication, GingerDicser.GetTermResValue(eTermResKey.TargetApplication));
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool ShowCancelButton = true)
        {
            Button okBtn = new Button
            {
                Content = "Ok"
            };
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: okBtn, eventName: nameof(ButtonBase.Click), handler: OKButton_Click);
            ObservableList<Button> winButtons = [okBtn];

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, ShowCancelButton, "Cancel");
        }

        internal void ResetPlatformSelection()
        {
            foreach (var item in mApplicationsPlatforms)
            {
                item.Selected = false;
            }
        }

        public void SetTargetApplications()
        {
            //mBusinessFlow.TargetApplications.Clear();
            //remove deleted
            for (int indx = 0; indx < mBusinessFlow.TargetApplications.Count; indx++)
            {
                if (mApplicationsPlatforms.FirstOrDefault(x => x.Selected && x.AppName == mBusinessFlow.TargetApplications[indx].Name) == null)
                {
                    mBusinessFlow.TargetApplications.RemoveAt(indx);
                    indx--;
                }
            }

            //add new

            ApplicationPlatform TA = (ApplicationPlatform)AppsGrid.GetSelectedItem();

            if (mBusinessFlow.TargetApplications.FirstOrDefault(x => x.Name == TA.AppName) == null)
            {
                TargetApplication tt = new TargetApplication
                {
                    AppName = TA.AppName,
                    TargetGuid = TA.Guid,
                    Selected = true
                };
                mBusinessFlow.TargetApplications.Add(tt);
            }
        }
    }
}
