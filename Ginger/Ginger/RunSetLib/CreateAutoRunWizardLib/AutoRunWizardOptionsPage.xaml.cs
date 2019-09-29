#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using GingerCore.GeneralLib;
using GingerWPF.WizardLib;
using System.Windows.Controls;

namespace Ginger.RunSetLib.CreateCLIWizardLib
{
    /// <summary>
    /// Interaction logic for CLISourceControlPage.xaml
    /// </summary>
    public partial class AutoRunWizardOptionsPage : Page, IWizardPage
    {
        AutoRunWizard mAutoRunWizard;
        
        public AutoRunWizardOptionsPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mAutoRunWizard = (AutoRunWizard)WizardEventArgs.Wizard;
                    if (WorkSpace.Instance.Solution.SourceControl == null)
                    {
                        xDownloadsolutionCheckBox.IsEnabled = false;
                        mAutoRunWizard.CliHelper.DownloadUpgradeSolutionFromSourceControl = false;
                    }
                    else
                    {
                        xDownloadsolutionCheckBox.IsEnabled = true;
                        mAutoRunWizard.CliHelper.DownloadUpgradeSolutionFromSourceControl = true;
                    }
                    mAutoRunWizard.CliHelper.ShowAutoRunWindow = false;
                    mAutoRunWizard.CliHelper.RunAnalyzer = true;
                    BindingHandler.ObjFieldBinding(xDownloadsolutionCheckBox, CheckBox.IsCheckedProperty, mAutoRunWizard.CliHelper, nameof(CLIHelper.DownloadUpgradeSolutionFromSourceControl));
                    BindingHandler.ObjFieldBinding(xGingerRunEXEWindowShow, CheckBox.IsCheckedProperty, mAutoRunWizard.CliHelper, nameof(CLIHelper.ShowAutoRunWindow));
                    BindingHandler.ObjFieldBinding(xRunAnalyzerCheckBox, CheckBox.IsCheckedProperty, mAutoRunWizard.CliHelper, nameof(CLIHelper.RunAnalyzer));
                    xArtifactsPathTextBox.Init(mAutoRunWizard.mContext, mAutoRunWizard.AutoRunConfiguration, nameof(RunSetAutoRunConfiguration.ArtifactsPath), isVENeeded: false, isBrowseNeeded: true, browserType: Activities.UCValueExpression.eBrowserType.Folder);
                    break;
            }
        }
    }
}
