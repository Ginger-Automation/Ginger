using Amdocs.Ginger.Common;
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
    public partial class MergerPage : Page
    {
        //ObservableList<ApplicationAPIModel> modelsEvaluated;
        GenericWindow mWin;
        DeltaAPIModel mDeltaAPIModel;
        APIModelPage mergedAPIPage;

        public MergerPage()
        {
            InitializeComponent();
        }

        public MergerPage(DeltaAPIModel deltaAPIModel, Window ownerWindow)
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

            GingerCore.General.LoadGenericWindow(ref mWin, mOwnerWindow, windowStyle, @"Compare & Merge", this, null, true, "OK");
        }

        void ToggleSections()
        {
            if (mDeltaAPIModel.DefaultOperationEnum == DeltaAPIModel.eHandlingOperations.MergeChanges)
            {
                xMergerTextRow.Height = new GridLength(30);
                if (mDeltaAPIModel.MergedAPIModel == null)
                {
                    xMatchedAPIAsBaseBtn.Visibility = Visibility.Visible;
                    xLearnedAPIAsBaseBtn.Visibility = Visibility.Visible;

                    xMergerSplitter.Visibility = Visibility.Collapsed;
                    xMergerWindowTxtBlock.Visibility = Visibility.Collapsed;
                    xMergedAPIFrame.Visibility = Visibility.Collapsed;

                    xMergerAPIRow.Height = new GridLength(0);
                }
                else
                {
                    xMatchedAPIAsBaseBtn.Visibility = Visibility.Collapsed;
                    xLearnedAPIAsBaseBtn.Visibility = Visibility.Collapsed;

                    xMergerSplitter.Visibility = Visibility.Visible;
                    xMergerWindowTxtBlock.Visibility = Visibility.Visible;
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
            mDeltaAPIModel.MergedAPIModel = APIDeltaUtils.CreateAPIModelObject(mDeltaAPIModel.matchingAPIModel);
            ToggleSections();
        }

        private void XLearnedAPIAsBaseBtn_Click(object sender, RoutedEventArgs e)
        {
            mDeltaAPIModel.MergedAPIModel = APIDeltaUtils.CreateAPIModelObject(mDeltaAPIModel.learnedAPI);
            ToggleSections();
        }

        void SetMergedrFrame()
        {
            mergedAPIPage = new APIModelPage(mDeltaAPIModel.MergedAPIModel);
            xMergedAPIFrame.Content = mergedAPIPage;
        }
    }
}
