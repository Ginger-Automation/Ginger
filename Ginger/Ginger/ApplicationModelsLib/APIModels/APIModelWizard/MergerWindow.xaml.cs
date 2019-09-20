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
    public partial class MergerWindow : Page
    {
        //ObservableList<ApplicationAPIModel> modelsEvaluated;
        GenericWindow mWin;
        DeltaAPIModel mDeltaAPIModel;
        APIModelPage mergedAPIPage;

        public MergerWindow()
        {
            InitializeComponent();
        }

        public MergerWindow(DeltaAPIModel deltaAPIModel)
        {
            InitializeComponent();
            mDeltaAPIModel = deltaAPIModel;
            APIModelPage existingAPIPage = new APIModelPage(deltaAPIModel.matchingAPIModel, true);
            APIModelPage learnedAPIPage = new APIModelPage(deltaAPIModel.learnedAPI, true);

            xExistingAPIFrame.Content = existingAPIPage;
            xLearnedAPIFrame.Content = learnedAPIPage;
        }

        public Window ownerWindow;
        public void ShowAsWindow(bool showMergerWindow = true, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button OKButton = new Button();
            OKButton.Content = "OK";
            OKButton.Click += OKButton_Click;

            if (ownerWindow == null)
            {
                ownerWindow = App.MainWindow;
            }

            if (mDeltaAPIModel.MergedAPIModel == null)
            {
                ToggleMergerSection(Visibility.Collapsed);

                if (showMergerWindow)
                {
                    ToggleBaseModelSection(Visibility.Visible);
                }
                else
                {
                    ToggleBaseModelSection(Visibility.Collapsed);
                }
            }
            else
            {
                ToggleBaseModelSection(Visibility.Collapsed);
                if (showMergerWindow)
                {
                    ToggleMergerSection(Visibility.Visible);
                }
                else
                {
                    ToggleMergerSection(Visibility.Collapsed);
                }
            }

            GingerCore.General.LoadGenericWindow(ref mWin, ownerWindow, windowStyle, @"Compare & Merge", this, new ObservableList<Button> { OKButton }, true, "Cancel");
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void XMatchedAPIAsBase_Click(object sender, RoutedEventArgs e)
        {
            mDeltaAPIModel.MergedAPIModel = APIDeltaUtils.CreateAPIModelObject(mDeltaAPIModel.matchingAPIModel);
            SetMergedrFrame();
        }

        private void XLearnedAPIAsBase_Click(object sender, RoutedEventArgs e)
        {
            mDeltaAPIModel.MergedAPIModel = APIDeltaUtils.CreateAPIModelObject(mDeltaAPIModel.learnedAPI);
            SetMergedrFrame();
        }

        void SetMergedrFrame()
        {
            mergedAPIPage = new APIModelPage(mDeltaAPIModel.MergedAPIModel);
            xMergedAPIFrame.Content = mergedAPIPage;
            SetBaseAPIDone();
        }

        void SetBaseAPIDone()
        {
            ToggleBaseModelSection(Visibility.Collapsed);
            ToggleMergerSection(Visibility.Visible);
        }

        void ToggleBaseModelSection(Visibility visibilityOption)
        {
            xBaseSelectionSection.Visibility = visibilityOption;
            if (visibilityOption == Visibility.Visible)
            {
                xMergerTextRow.Height = new GridLength(30);
            }
        }

        void ToggleMergerSection(Visibility visibilityOption)
        {
            xMergerSplitter.Visibility = visibilityOption;
            xMergerWindowTxtBlock.Visibility = visibilityOption;
            xMergedAPIFrame.Visibility = visibilityOption;

            if (visibilityOption == Visibility.Collapsed)
            {
                xMergerTextRow.Height = new GridLength(0);
                xMergerAPIRow.Height = new GridLength(0);
            }
            else
            {
                xMergerTextRow.Height = new GridLength(30);
                xMergerAPIRow.Height = new GridLength(400, GridUnitType.Star);
            }
        }
    }
}
