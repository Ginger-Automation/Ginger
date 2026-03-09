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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCoreNET.Application_Models;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    /// <summary>
    /// Interaction logic for PomNewAddedElementSelectionPage.xaml
    /// </summary>
    public partial class PomNewAddedElementSelectionPage : Page
    {
        public PomDeltaViewPage mPomDeltaViewPage;
        private Agent mAgent;
        private PomDeltaUtils mPomDeltaUtils;
        private ObservableList<DeltaElementInfo> mDeltaElements;
        private GenericWindow mGenericWindow = null;
        private ObservableList<ElementInfo> mElementInfoList = [];


        public PomNewAddedElementSelectionPage(ObservableList<DeltaElementInfo> deltaElementInfos, PomDeltaUtils pomDeltaUtils, string searchText, UserControls.GridColView gridColView)
        {
            InitializeComponent();
            mPomDeltaUtils = pomDeltaUtils;
            mDeltaElements = deltaElementInfos;

            mAgent = mPomDeltaUtils.Agent;

            mPomDeltaViewPage = new PomDeltaViewPage(mPomDeltaUtils, gridColView, mAgent, deltaElementInfos);
            mPomDeltaViewPage.SetAgent(mAgent);
            mPomDeltaViewPage.xMainElementsGrid.Grid.Columns[1].Visibility = Visibility.Collapsed;
            mPomDeltaViewPage.xMainElementsGrid.btnMarkAll.Visibility = Visibility.Collapsed;

            mPomDeltaViewPage.xMainElementsGrid.txtSearch.Text = searchText;
            xNewPomElementsPageFrame.ClearAndSetContent(mPomDeltaViewPage);


            // set LiveSpy Agent
            xLiveSpy.DriverAgent = mAgent;
            string allProperties = string.Empty;
            PropertyChangedEventManager.AddHandler(source: mAgent, handler: XLiveSpy_PropertyChanged, propertyName: allProperties);

        }

        private void XLiveSpy_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var elementInfo = xLiveSpy.SpySelectedElement;

            if (elementInfo != null)
            {
                ElementInfo matchingOriginalElement = xLiveSpy.mWinExplorer.GetMatchingElement(elementInfo, mElementInfoList);
                if (matchingOriginalElement != null)
                {
                    xLiveSpy.SetLableStatusText("Element found in new added list");
                    xLiveSpy.mWinExplorer.LearnElementInfoDetails(elementInfo);
                    var deltaElement = mDeltaElements.FirstOrDefault(x => x.ElementInfo.XPath.Equals(elementInfo.XPath));
                    if (deltaElement != null)
                    {
                        mDeltaElements.CurrentItem = deltaElement;
                        mPomDeltaViewPage.xMainElementsGrid.ScrollToViewCurrentItem();
                    }

                }
                else
                {
                    xLiveSpy.SetLableStatusText("Element not found in new added list");
                }
            }
        }


        internal DeltaElementInfo ShowAsWindow(string winTitle)
        {
            ObservableList<Button> windowButtons = [];

            Button selectBtn = new Button
            {
                Content = "Select"
            };
            WeakEventManager<ButtonBase, RoutedEventArgs>.AddHandler(source: selectBtn, eventName: nameof(ButtonBase.Click), handler: selectBtn_Click);
            windowButtons.Add(selectBtn);
            this.Height = 600;
            this.Width = 800;
            GenericWindow.LoadGenericWindow(ref mGenericWindow, null, eWindowShowStyle.Dialog, winTitle, this, windowButtons, true, "Cancel", CancelBtn_Click);
            return mPomDeltaViewPage.mSelectedElement;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mGenericWindow != null)
            {
                mPomDeltaViewPage.xMainElementsGrid.Grid.SelectedItem = null;
                mGenericWindow.Close();
            }
        }

        private void selectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mGenericWindow != null)
            {
                mGenericWindow.Close();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            mElementInfoList = mPomDeltaUtils.GetElementInfoListFromDeltaElementInfo(mDeltaElements);
        }
    }
}
