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
using Ginger.UserControls;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for EditBusinessFlowAppsPage.xaml
    /// </summary>
    public partial class EditBusinessFlowAppsPage : Page
    {
         BusinessFlow mBusinessFlow;
         ObservableList<ApplicationPlatform> mApplicationsPlatforms = new ObservableList<ApplicationPlatform>();
         GenericWindow _pageGenericWin = null;

         public EditBusinessFlowAppsPage(BusinessFlow BizFlow)
         {
             InitializeComponent();

             this.Title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Target Application(s)";

             mBusinessFlow = BizFlow;
             SetGridView();
         }

         private void SetGridView()
         {
             GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
             view.GridColsView = new ObservableList<GridColView>();

             view.GridColsView.Add(new GridColView() { Field = "Selected", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
             view.GridColsView.Add(new GridColView() { Field = "AppName", Header = "Application Name", WidthWeight = 50, ReadOnly=true, BindingMode=BindingMode.OneWay});
             view.GridColsView.Add(new GridColView() { Field = "Platform", Header = "Platform", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay });

             AppsGrid.SetAllColumnsDefaultView(view);
             AppsGrid.InitViewItems();

             foreach (ApplicationPlatform AP in App.UserProfile.Solution.ApplicationPlatforms)
             {
                 ApplicationPlatform AP1 = new ApplicationPlatform();
                 AP1.AppName = AP.AppName;
                 AP1.Platform = AP.Platform;

                 // If this App was selected before then mark it 
                 TargetApplication APS = (TargetApplication)(from x in mBusinessFlow.TargetApplications where x.Name == AP.AppName select x).FirstOrDefault();

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
            int SelectedPlatformCount = 0;
            mBusinessFlow.TargetApplications.Clear();
            foreach (ApplicationPlatform TA in mApplicationsPlatforms)
            {
                if (TA.Selected)
                {
                    TargetApplication tt = new TargetApplication();
                    tt.AppName = TA.AppName;
                    tt.Selected = true;
                    mBusinessFlow.TargetApplications.Add(tt);
                    SelectedPlatformCount++;
                }
            }

            if (App.BusinessFlow == mBusinessFlow)
            {
                App.UpdateApplicationsAgentsMapping();
            }
            if (SelectedPlatformCount > 0|| mApplicationsPlatforms.Count==0)
            {
                _pageGenericWin.Close();
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.BusinessFlowNeedTargetApplication);
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog,bool ShowCancelButton=true)
        {
            Button okBtn = new Button();
            okBtn.Content = "Ok";
            okBtn.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, ShowCancelButton, "Cancel");
        }

        internal void ResetPlatformSelection()
        {
            foreach (var item in mApplicationsPlatforms)
            {
                item.Selected=false;
            }
        }
    }
}
