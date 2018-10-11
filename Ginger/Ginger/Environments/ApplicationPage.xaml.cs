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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Core;
using Amdocs.Ginger.UserControls;
using GingerCore;
using GingerCore.Environments;

namespace Ginger.Environments
{
    /// <summary>
    /// Interaction logic for ApplicationPage.xaml
    /// </summary>
    public partial class ApplicationPage : Page
    {
        EnvApplication mEnvApplication;
        public ApplicationPage(EnvApplication app)
        {
            InitializeComponent();
            mEnvApplication = app;

            App.ObjFieldBinding(ApplicationNameTextBox, TextBox.TextProperty, app, EnvApplication.Fields.Name);
            App.ObjFieldBinding(DescriptionTextBox, TextBox.TextProperty, app, EnvApplication.Fields.Description);

            UpdateParametersTabHeader();
            app.GeneralParams.CollectionChanged += GeneralParams_CollectionChanged;
            UpdateDBsTabHeader();
            app.Dbs.CollectionChanged += Dbs_CollectionChanged;
            UpdateLoginuserTabHeader();
            app.LoginUsers.CollectionChanged += LoginUsers_CollectionChanged;
            ColorSelectedTab();
        }

        private void LoginUsers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateLoginuserTabHeader();
        }

        private void UpdateLoginuserTabHeader()
        {
            xUserTabHeaderText.Text = string.Format("Login Users ({0})", mEnvApplication.LoginUsers.Count);
        }

        private void GeneralParams_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateParametersTabHeader();
        }

        private void UpdateParametersTabHeader()
        {
            xParamsTabHeaderText.Text = string.Format("Parameters ({0})", mEnvApplication.GeneralParams.Count);
        }

        private void Dbs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateDBsTabHeader();
        }

        private void UpdateDBsTabHeader()
        {
            xDBsTabHeaderText.Text = string.Format("DataBases ({0})", mEnvApplication.Dbs.Count);
        }

        private void AppTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorSelectedTab();
            //Lazy load of tabs content
            if (((TabItem)AppTab.SelectedItem).Name == xParamsTab.Name)
            {
                if (ParamsFrame.Content == null)
                {
                    ParamsFrame.Content =  new AppGeneralParamsPage(mEnvApplication);                 
                }
                return;
            }

            if (((TabItem)AppTab.SelectedItem).Name == xDBsTab.Name)
            {
                if (DBsFrame.Content == null)
                {                    
                    DBsFrame.Content = new AppDataBasesPage(mEnvApplication);
                    return;
                }
            }

            if (((TabItem)AppTab.SelectedItem).Name == xUsersTab.Name)
            {
                if (UsersFrame.Content == null)
                {
                    UsersFrame.Content = new AppLoginUsersPage(mEnvApplication);                    
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

                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (AppTab.SelectedItem == tab)
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("@Skin1_ColorB");
                                else
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("@Skin1_ColorA");

                                ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error in Action Edit Page tabs style", ex);
            }
        }
    }
}