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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.UserControls;
using Ginger.Run;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.Agents
{
    /// <summary>
    /// Interaction logic for ApplicationAgentsMapPage.xaml
    /// </summary>
    public partial class ApplicationAgentsMapPage : Page
    {        
        public ObservableList<ApplicationAgent> ApplicationAgents;
        GingerRunner mRunner;
        Context mContext;

        bool AllowAgentsManipulation;

        public ListBox MappingList
        {
            get { return xAppAgentsListBox; }
        }

        public ApplicationAgentsMapPage(GingerRunner runner, Context context, bool allowAgentsManipulation=true)
        {
            InitializeComponent();
            mRunner = runner;
            mContext = context;
            AllowAgentsManipulation = allowAgentsManipulation;
            xAppAgentsListBox.Tag = AllowAgentsManipulation;//Placed here for binding with list dataTemplate- need better place
            mRunner.PropertyChanged += MGR_PropertyChanged;
            
            RefreshApplicationAgentsList();
        }

        private void MGR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(mRunner.ApplicationAgents))
            {
                RefreshApplicationAgentsList();
            }
        }

        private void RefreshApplicationAgentsList()
        {
            this.Dispatcher.Invoke(() =>
            {
                ApplicationAgents = new ObservableList<ApplicationAgent>();

                foreach (ApplicationAgent Apag in mRunner.ApplicationAgents)
                {
                    if (mRunner.SolutionApplications.Where(x => x.AppName == Apag.AppName && x.Platform == ePlatformType.NA).FirstOrDefault() == null)
                    {
                        ApplicationAgents.Add(Apag);
                    }
                }

                xAppAgentsListBox.ItemsSource = ApplicationAgents;
            });
        }

        private void xStartCloseAgentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AllowAgentsManipulation)
            {
                ApplicationAgent AG = (ApplicationAgent)((ucButton)sender).DataContext;
                Agent agent = ((Agent)AG.Agent);
                if (agent.Status != Agent.eStatus.Running)
                {
                    //start Agent
                    Reporter.ToStatus(eStatusMsgKey.StartAgent, null, AG.AgentName, AG.AppName);
                    ((Agent)AG.Agent).ProjEnvironment = mContext.Environment;
                    ((Agent)AG.Agent).BusinessFlow = mContext.BusinessFlow;
                    ((Agent)AG.Agent).SolutionFolder = WorkSpace.Instance.Solution.Folder;
                    ((Agent)AG.Agent).DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
                    ((Agent)AG.Agent).StartDriver();
                    Reporter.HideStatusMessage();
                }
                else
                {
                    //close Agent
                    agent.Close();
                }
            }
        }

        private void XAgentNameComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ApplicationAgent applicationAgent = (ApplicationAgent)((ComboBox)sender).DataContext;
            List<IAgent> filteredOptionalAgents = applicationAgent.PossibleAgents;

            ////remove already mapped agents
            //List<IAgent> alreadyMappedAgents = mRunner.ApplicationAgents.Where(x => x.Agent != null).Select(x => x.Agent).ToList();
            //foreach (IAgent mappedAgent in alreadyMappedAgents)
            //{
            //    if (mappedAgent != applicationAgent.Agent)
            //    {
            //        if (filteredOptionalAgents.Contains(mappedAgent))
            //        {
            //            filteredOptionalAgents.Remove(mappedAgent);
            //        }
            //    }
            //}

            ((ComboBox)sender).ItemsSource = filteredOptionalAgents;
        }

        private void XAgentNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //save last used agent on the Solution Target Applications
            ApplicationAgent ag = ApplicationAgents.Where(x => x.Agent == (Agent)((ComboBox)sender).SelectedValue).FirstOrDefault();
            if (ag != null)
            {
                ApplicationPlatform ap = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == ag.AppName).FirstOrDefault();
                if (ap != null)
                {
                    ap.LastMappedAgentName = ag.AgentName;
                }
            }
        }
    }

    public class AgentForgroundTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (SolidColorBrush)(new BrushConverter().ConvertFrom("#152B37"));//blue
            }
            else
            {
                return (SolidColorBrush)(new BrushConverter().ConvertFrom("#DC3812"));//red
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AgentStatusColorTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch((Agent.eStatus)value)
            {
                case Agent.eStatus.Running:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#109717"));//green

                case Agent.eStatus.Starting:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFC268"));//orange

                default:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#DC3812"));//red
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AgentVisibilityTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AgentStatusTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((Agent.eStatus)value)
            {
                case Agent.eStatus.Running:
                    return "Agent is Running, Click to Close it";

                case Agent.eStatus.Starting:
                    return "Agent is Starting...";

                default:
                    return "Agent is Not Running, Click to Start it";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
