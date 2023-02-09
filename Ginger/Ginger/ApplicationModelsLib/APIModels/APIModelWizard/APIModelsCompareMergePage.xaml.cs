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
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using GingerCoreNET.Application_Models;
using GingerWPF.ApplicationModelsLib.APIModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.APIModels.APIModelWizard
{
    /// <summary>
    /// Interaction logic for MergerWindow.xaml
    /// </summary>
    public partial class APIModelsCompareMergePage : Page
    {
        //ObservableList<ApplicationAPIModel> modelsEvaluated;
        GenericWindow mWin;
        DeltaAPIModel mDeltaAPIModel;
        APIModelPage mergedAPIPage;

        public APIModelsCompareMergePage()
        {
            InitializeComponent();
        }

        public APIModelsCompareMergePage(DeltaAPIModel deltaAPIModel, Window ownerWindow)
        {
            InitializeComponent();
            mDeltaAPIModel = deltaAPIModel;

            mOwnerWindow = ownerWindow;

            APIModelPage existingAPIPage = new APIModelPage(deltaAPIModel.matchingAPIModel, General.eRIPageViewMode.View);
            APIModelPage learnedAPIPage = new APIModelPage(deltaAPIModel.learnedAPI, General.eRIPageViewMode.View);

            xExistingAPIFrame.Content = existingAPIPage;
            xLearnedAPIFrame.Content = learnedAPIPage;
        }

        Window mOwnerWindow;
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            if (mOwnerWindow == null)
            {
                mOwnerWindow = App.MainWindow;
            }

            ToggleSections();

            this.Width = 1200;
            this.Height = 800;

            GingerCore.General.LoadGenericWindow(ref mWin, mOwnerWindow, windowStyle, @"Compare & Merge", this, null, true, "OK");
        }

        void ToggleSections()
        {
            if (mDeltaAPIModel.SelectedOperationEnum == DeltaAPIModel.eHandlingOperations.MergeChanges)
            {
                if (mDeltaAPIModel.MergedAPIModel == null)
                {
                    xMergerTextRow.Height = new GridLength(50);

                    xMatchedAPIAsBaseBtn.Visibility = Visibility.Visible;
                    xLearnedAPIAsBaseBtn.Visibility = Visibility.Visible;

                    xMergerSplitter.Visibility = Visibility.Collapsed;
                    xMergerWindowTxtBlock.Visibility = Visibility.Collapsed;
                    xMergedAPIBorder.Visibility = Visibility.Collapsed;
                    xMergedAPIFrame.Visibility = Visibility.Collapsed;

                    xMergerAPIRow.Height = new GridLength(0);
                }
                else
                {
                    xMergerTextRow.Height = new GridLength(30);

                    xMatchedAPIAsBaseBtn.Visibility = Visibility.Collapsed;
                    xLearnedAPIAsBaseBtn.Visibility = Visibility.Collapsed;

                    xMergerSplitter.Visibility = Visibility.Visible;
                    xMergerWindowTxtBlock.Visibility = Visibility.Visible;
                    xMergedAPIBorder.Visibility = Visibility.Visible;
                    xMergedAPIFrame.Visibility = Visibility.Visible;

                    xMergerAPIRow.Height = new GridLength(400, GridUnitType.Star);
                    SetMergedrFrame();
                }
            }
            else
            {
                xMergerTextRow.Height = new GridLength(0);
                xMergerAPIRow.Height = new GridLength(0);
            }
        }

        private void XMatchedAPIAsBaseBtn_Click(object sender, RoutedEventArgs e)
        {
            mDeltaAPIModel.MergedAPIModel = LearnAPIModelsUtils.CreateAPIModelObject(mDeltaAPIModel.matchingAPIModel);
            ToggleSections();
        }

        private void XLearnedAPIAsBaseBtn_Click(object sender, RoutedEventArgs e)
        {
            mDeltaAPIModel.MergedAPIModel = LearnAPIModelsUtils.CreateAPIModelObject(mDeltaAPIModel.learnedAPI);
            mDeltaAPIModel.MergedAPIModel.TargetApplicationKey = mDeltaAPIModel.matchingAPIModel.TargetApplicationKey;
            mDeltaAPIModel.MergedAPIModel.TagsKeys = mDeltaAPIModel.matchingAPIModel.TagsKeys;

            ToggleSections();
        }

        void SetMergedrFrame()
        {
            mergedAPIPage = new APIModelPage(mDeltaAPIModel.MergedAPIModel, General.eRIPageViewMode.Add);
            xMergedAPIFrame.Content = mergedAPIPage;
        }
    }
}
