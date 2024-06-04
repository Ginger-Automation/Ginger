#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Ginger.BusinessFlowPages;
using Ginger.UserControlsLib;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using NUglify.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.Environments
{
    /// <summary>
    /// Interaction logic for ApplicationPage.xaml
    /// </summary>
    public partial class ApplicationPage : GingerUIPage
    {
        EnvApplication mEnvApplication;

        Context mContext;

        //List of Environment Application except from the current Environment
        //with the same parent guid as the current application
        private readonly IEnumerable<EnvApplication> FilteredEnvApplication;
        private readonly ApplicationPlatform? CurrentTargetApplication;
        public ApplicationPage(EnvApplication app, Context context)
        {
            InitializeComponent();
            mEnvApplication = app;
            mContext = context;
            CurrentItemToSave = mContext.Environment;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ApplicationNameTextBox, TextBox.TextProperty, app, nameof(EnvApplication.Name));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DescriptionTextBox, TextBox.TextProperty, app, nameof(EnvApplication.Description));
            ApplicationNameTextBox.AddValidationRule(new EnvAppNameValidateRule());
            UpdateParametersTabHeader();
            CollectionChangedEventManager.AddHandler(source: app.Variables, handler: GeneralParams_CollectionChanged);
            UpdateDBsTabHeader();
            CollectionChangedEventManager.AddHandler(source: app.Dbs, handler: Dbs_CollectionChanged);
            ColorSelectedTab();
            ApplicationNameTextBox.TextChanged += ApplicationNameTextBox_TextChanged;

           this.FilteredEnvApplication = WorkSpace.Instance.SolutionRepository
                                            .GetAllRepositoryItems<ProjEnvironment>()
                                            .Where((projEnv) => !projEnv.Equals(mContext.Environment))
                                            .SelectMany(projEnv => projEnv.Applications)
                                            .Where(envApp => envApp.ParentGuid.Equals(mEnvApplication.ParentGuid));

            this.CurrentTargetApplication = WorkSpace.Instance.Solution
                                              .ApplicationPlatforms
                                              .FirstOrDefault((appPlat) => appPlat.Guid.Equals(mEnvApplication.ParentGuid));

        }

        private void ApplicationNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // changing the name of the environment application to the changed one.
            this.FilteredEnvApplication
                .ForEach((envApp) =>
                {
                    envApp.StartDirtyTracking();
                    envApp.Name = ApplicationNameTextBox.Text;
                });


            // changing the name of the target application whose guid is same as current env app's parent guid

            if (this.CurrentTargetApplication!=null)
            {
                this.CurrentTargetApplication.StartDirtyTracking();
                this.CurrentTargetApplication.AppName = ApplicationNameTextBox.Text;
            }
        }

        private void GeneralParams_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateParametersTabHeader();
        }

        private void UpdateParametersTabHeader()
        {
            xParamsTabHeaderText.Text = string.Format("Parameters ({0})", mEnvApplication.Variables.Count);
        }

        private void Dbs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateDBsTabHeader();
        }

        private void UpdateDBsTabHeader()
        {
            xDBsTabHeaderText.Text = string.Format("Databases ({0})", mEnvApplication.Dbs.Count);
        }

        private void AppTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorSelectedTab();
            //Lazy load of tabs content
            if (((TabItem)AppTab.SelectedItem).Name == xParamsTab.Name)
            {
                if (ParamsFrame.Content == null)
                {
                    ParamsFrame.ClearAndSetContent(new VariabelsListViewPage(mEnvApplication, null, General.eRIPageViewMode.Standalone));
                }
                return;
            }

            if (((TabItem)AppTab.SelectedItem).Name == xDBsTab.Name)
            {
                if (DBsFrame.Content == null)
                {
                    DBsFrame.ClearAndSetContent(new AppDataBasesPage(mEnvApplication, mContext));
                    return;
                }
            }

        }

        private void ColorSelectedTab()
        {
            //set the selected tab text style
            try
            {
                if (AppTab.SelectedItem != null)
                {
                    foreach (TabItem tab in AppTab.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)
                        {
                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (AppTab.SelectedItem == tab)
                                {
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                }
                                else
                                {
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$PrimaryColor_Black");
                                } ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Action Edit Page tabs style", ex);
            }
        }
    }

    public class EnvAppNameValidateRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string EnvAppName = (string)value;


            if (string.IsNullOrEmpty(EnvAppName))
            {
                return new ValidationResult(false, "Environment Application Name cannot be null or empty");
            }

            bool doesAppNameExist = WorkSpace.Instance.Solution.ApplicationPlatforms.Any(app=>string.Equals(app.AppName, EnvAppName));

            if (doesAppNameExist)
            {
                return new ValidationResult(false, "Environment Application Name already exists");
            }

            return new ValidationResult(true, null);
        }
    }
}