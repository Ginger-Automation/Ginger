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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.ValidationRules;
using Ginger.Actions._Common.ActUIElementLib;
using GingerCore.Actions.Common;
using GingerWPF.WizardLib;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.ActionConversion
{
    /// <summary>
    /// Interaction logic for ConversionConfigurationWzardPage.xaml
    /// </summary>
    public partial class ConversionConfigurationWzardPage : Page, IWizardPage
    {
        ActionsConversionWizard mWizard;
        bool mDefaultTaretApp = true;
        public bool DefaultTaretApp
        {
            get
            {
                return mDefaultTaretApp;
            }
            set
            {
                mDefaultTaretApp = value;
            }
        }

        bool mNewActivityChecked = false;
        public bool NewActivityChecked
        {
            get
            {
                return mNewActivityChecked;
            }
            set
            {
                mNewActivityChecked = value;
                
            }
        }

        bool mSameActivityChecked = true;
        public bool SameActivityChecked
        {
            get
            {
                return mSameActivityChecked;
            }
            set
            {
                mSameActivityChecked = value;
                OnNewActivityChecked(!value);
            }
        }

        bool mPOMChecked = false;
        public bool POMChecked
        {
            get
            {
                return mPOMChecked;
            }
            set
            {
                mPOMChecked = value;
                OnPOMChecked(value);
            }
        }

        string mSelectedTargetApp = string.Empty;
        public string SelectedTargetApp
        {
            get
            {
                return mSelectedTargetApp;
            }
            set
            {
                mSelectedTargetApp = value;
                mWizard.SelectedTargetApp = value;
            }
        }

        public ConversionConfigurationWzardPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (ActionsConversionWizard)WizardEventArgs.Wizard;
                    break;
                case EventType.Active:
                    Init(WizardEventArgs);
                    break;
                case EventType.Finish:
                    break;
            }
        }
        
        private void Init(WizardEventArgs WizardEventArgs)
        {
            xCmbTargetApp.BindControl(mWizard.BusinessFlow.TargetApplications.Select(x => x.Name).ToList());
            if ((xCmbTargetApp != null) && (xCmbTargetApp.Items.Count > 0))
            {
                xCmbTargetApp.SelectedIndex = 0;
            }
            mWizard.DefaultTargetAppChecked = Convert.ToBoolean(xChkDefaultTargetApp.IsChecked);

            xGrdPane.RowDefinitions[2].Height = new GridLength(0);

            LocateByPOMElementPage locateByPOMElementPage = new LocateByPOMElementPage(null, nameof(ActUIElement.ElementType), mWizard, nameof(mWizard.SelectedPOMObjectName), true);
            xLocateValueEditFrame.Content = locateByPOMElementPage;
        }

        private void OnDefaultTargetAppChecked(object sender, RoutedEventArgs e)
        {
            if ((xCmbTargetApp != null) && (xCmbTargetApp.Items.Count > 0))
            {
                mWizard.DefaultTargetAppChecked = Convert.ToBoolean(xChkDefaultTargetApp.IsChecked);
                xCmbTargetApp.IsEnabled = true;
                xBtnRefresh.IsEnabled = true;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            xCmbTargetApp.BindControl(mWizard.BusinessFlow.TargetApplications.Select(x => x.Name).ToList());
            if ((xCmbTargetApp != null) && (xCmbTargetApp.Items.Count > 0))
            {
                xCmbTargetApp.SelectedIndex = 0;
            }
        }
        
        private void OnNewActivityChecked(bool isChecked)
        {
            if (mWizard != null)
            {
                mWizard.NewActivityChecked = isChecked;
                if (isChecked)
                {
                    xGrdPane.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Auto);
                }
                else
                {
                    xGrdPane.RowDefinitions[2].Height = new GridLength(0);
                }
            }
        }

        private void OnPOMChecked(bool isChecked)
        {
            if (mWizard != null)
            {
                mWizard.ConvertToPOMAction = isChecked;
                xLocateValueEditFrame.IsEnabled = mWizard.ConvertToPOMAction;
            }
        }
    }
}
