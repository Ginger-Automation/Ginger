using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Drivers;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    /// <summary>
    /// Interaction logic for POMDeltaPage.xaml
    /// </summary>
    public partial class PomDeltaElementCompareWizardPage : Page, IWizardPage
    {
        PomDeltaWizard mWizard;
        PomDeltaViewPage mPomDeltaViewPage = null;
        bool mFirstLearnWasDone = false;

        public PomDeltaElementCompareWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (PomDeltaWizard)WizardEventArgs.Wizard;                  
                    break;

                case EventType.Active:
                    if (!mFirstLearnWasDone)
                    {
                        LearnDelta();
                        mFirstLearnWasDone = true;
                    }
                    break;
            }
        }

        private async void LearnDelta()
        {
            if (!mWizard.mPomDeltaUtils.IsLearning)
            {
                try
                {
                    mWizard.ProcessStarted();
                    InitilizePomElementsMappingPage();//we recreating page as workaround for clearing grid filters                    
                    xReLearnButton.Visibility = Visibility.Collapsed;
                    xStopLoadButton.ButtonText = "Stop";
                    xStopLoadButton.IsEnabled = true;                    
                    xStopLoadButton.Visibility = Visibility.Visible;                    

                    await Task.Run(() => mWizard.mPomDeltaUtils.LearnDelta());
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.POMWizardFailedToLearnElement, ex.Message);
                    //Reporter.ToLog(eLogLevel.ERROR, "Error occured during POM Element update", ex);
                    mWizard.mPomDeltaUtils.StopLearning();
                }
                finally
                {
                    xStopLoadButton.Visibility = Visibility.Collapsed;
                    xReLearnButton.Visibility = Visibility.Visible;
                    mWizard.ProcessEnded();
                }
            }
        }

        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            xStopLoadButton.ButtonText = "Stopping...";
            xStopLoadButton.IsEnabled = false;
            mWizard.mPomDeltaUtils.StopLearning();
        }

        private void ReLearnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.POMDeltaWizardReLearnWillEraseModification) == eUserMsgSelection.Yes)
            {
                mWizard.mPomDeltaUtils.StopLearning();

                LearnDelta();
            }
        }

        private void InitilizePomElementsMappingPage()
        {
            mPomDeltaViewPage = new PomDeltaViewPage(mWizard.mPomDeltaUtils.DeltaViewElements);
            mPomDeltaViewPage.SetAgent(mWizard.mPomDeltaUtils.Agent);
            xPomElementsMappingPageFrame.Content = mPomDeltaViewPage;
        }


    }
}
