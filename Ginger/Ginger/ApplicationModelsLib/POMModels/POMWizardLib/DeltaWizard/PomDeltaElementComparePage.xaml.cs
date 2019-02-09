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
    public partial class PomDeltaElementComparePage : Page, IWizardPage
    {
        PomDeltaWizard mWizard;
        PomDeltaViewPage mPomDeltaViewPage = null;       

        public PomDeltaElementComparePage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            if (WizardEventArgs.EventType == EventType.Init)
            {
                mWizard = (PomDeltaWizard)WizardEventArgs.Wizard;               
                InitilizePomElementsMappingPage();
                mPomDeltaViewPage.SetAgent(mWizard.mAgent);
                xReLearnButton.Visibility = Visibility.Visible;
                LearnDelta();
            }
        }

        private async void LearnDelta()
        {
            if (!mWizard.mPomDeltaUtils.IsLearning)
            {
                try
                {
                    mWizard.ProcessStarted();
                    xReLearnButton.Visibility = Visibility.Collapsed;
                    xStopLoadButton.ButtonText = "Stop";
                    xStopLoadButton.IsEnabled = true;                    
                    xStopLoadButton.Visibility = Visibility.Visible;                    

                    await mWizard.mPomDeltaUtils.LearnDeltaAsync();
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.POMWizardFailedToLearnElement, ex.Message);
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
            if (mPomDeltaViewPage == null)
            {
                mPomDeltaViewPage = new PomDeltaViewPage(mWizard.mPomDeltaUtils.DeltaViewElements);
                xPomElementsMappingPageFrame.Content = mPomDeltaViewPage;
            }
        }

    }
}
