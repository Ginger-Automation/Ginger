#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.BusinessFlowPages;
using Ginger.UserControlsLib;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using System;
using System.Collections.Specialized;
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

        public ApplicationPage(EnvApplication app, Context context)
        {
            InitializeComponent();
            mEnvApplication = app;
            mContext = context;
            CurrentItemToSave = mContext.Environment;


            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ApplicationNameTextBox, TextBox.TextProperty, app, nameof(EnvApplication.Name));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DescriptionTextBox, TextBox.TextProperty, app, nameof(EnvApplication.Description));

            if (mEnvApplication.GOpsFlag)
            {
                ApplicationNameTextBox.IsEnabled = false;
                DescriptionTextBox.IsEnabled = false;
            }

            UpdateParametersTabHeader();
            CollectionChangedEventManager.AddHandler(source: app.Variables, handler: GeneralParams_CollectionChanged);
            UpdateDBsTabHeader();
            CollectionChangedEventManager.AddHandler(source: app.Dbs, handler: Dbs_CollectionChanged);
            ColorSelectedTab();
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
}