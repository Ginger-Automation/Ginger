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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.UserControls;
using Ginger.Run;
using Ginger.SolutionWindows;
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
        GingerExecutionEngine mRunner;
        Context mContext;
        bool AllowAgentsManipulation;
        public delegate void OnBusinessFlowTargetApplicationChange();
        public static event OnBusinessFlowTargetApplicationChange BusinessFlowTargetApplicationChanged;

        public ListBox MappingList
        {
            get { return xAppAgentsListBox; }
        }

        public ApplicationAgentsMapPage(GingerExecutionEngine runner, Context context, bool allowAgentsManipulation = true)
        {
            InitializeComponent();
            mRunner = runner;
            mContext = context;
            AllowAgentsManipulation = allowAgentsManipulation;
            xAppAgentsListBox.Tag = AllowAgentsManipulation;//Placed here for binding with list dataTemplate- need better place
            mRunner.GingerRunner.PropertyChanged += MGR_PropertyChanged;
            TargetApplicationsPage.OnActivityUpdate += RefreshApplicationAgentsList;
            xKeepAgentsOn.Visibility = Visibility.Collapsed;
            if (!AllowAgentsManipulation && !WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunModeParallel)
            {
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xKeepAgentsOn, CheckBox.IsCheckedProperty, mRunner.GingerRunner, nameof(GingerRunner.KeepAgentsOn));
                xKeepAgentsOn.Visibility = Visibility.Visible;
            }

            RefreshApplicationAgentsList();
        }

        private void MGR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(mRunner.GingerRunner.ApplicationAgents))
            {
                RefreshApplicationAgentsList();
            }
        }

        public void RefreshApplicationAgentsList()
        {
            this.Dispatcher.Invoke(() =>
            {
                ApplicationAgents = [];
                foreach (ApplicationAgent Apag in mRunner.GingerRunner.ApplicationAgents)
                {
                    if (Apag.ApplicationAgentOperations == null)
                    {
                        Apag.ApplicationAgentOperations = new ApplicationAgentOperations(Apag);
                    }
                    if (mRunner.SolutionApplications?.FirstOrDefault(x => x.AppName == Apag.AppName && x.Platform == ePlatformType.NA) == null)
                    {
                        if (Apag.Agent == null && Apag.PossibleAgents.Any())
                        {
                            Apag.Agent = Apag.PossibleAgents[0] as Agent;
                        }

                        ApplicationAgents.Add(Apag);
                    }
                }
                xAppAgentsListBox.ItemsSource = ApplicationAgents;
            });
        }

        public IEnumerable<string> GetAllTargetApplicationNames()
        {
            if (mContext.BusinessFlow != null)
            {
                return mContext.BusinessFlow.Activities.Select((activity) => activity.TargetApplication);
            }

            else if (mRunner != null && mRunner.BusinessFlows != null)
            {
                return mRunner.BusinessFlows.SelectMany((businessFlow) => businessFlow.Activities).Select((activity) => activity.TargetApplication);
            }
            return null;
        }

        private async void xStartCloseAgentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AllowAgentsManipulation)
            {
                try
                {
                    ApplicationAgent AG = (ApplicationAgent)((ucButton)sender).DataContext;
                    Agent agent = AG.Agent;

                    switch (((AgentOperations)agent.AgentOperations).Status)
                    {
                        case Agent.eStatus.Completed:
                        case Agent.eStatus.Ready:
                        case Agent.eStatus.Running:
                            //Close Agent
                            Reporter.ToStatus(eStatusMsgKey.StopAgent, null, AG.AgentName, AG.AppName);
                            await System.Threading.Tasks.Task.Run(() =>
                            {
                                agent.AgentOperations.Close();
                            });
                            break;

                        case Agent.eStatus.Starting:
                            //Do nothing till Agent finish to start
                            break;

                        case Agent.eStatus.FailedToStart:
                        case Agent.eStatus.NotStarted:
                        default:
                            //Start Agent
                            Reporter.ToStatus(eStatusMsgKey.StartAgent, null, AG.AgentName, AG.AppName);
                            AG.Agent.ProjEnvironment = mContext.Environment;
                            AG.Agent.BusinessFlow = mContext.BusinessFlow;
                            AG.Agent.SolutionFolder = WorkSpace.Instance.Solution.Folder;
                            AG.Agent.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
                            await System.Threading.Tasks.Task.Run(() =>
                            {
                                AG.Agent.AgentOperations.StartDriver();
                            });
                            break;
                    }
                }
                finally
                {
                    Reporter.HideStatusMessage();
                }

            }
        }

        private void XAgentNameComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ApplicationAgent applicationAgent = (ApplicationAgent)((ComboBox)sender).DataContext;

            ApplicationAgentOperations applicationAgentOperations = new ApplicationAgentOperations(applicationAgent);
            applicationAgent.ApplicationAgentOperations = applicationAgentOperations;

            List<IAgent> filteredOptionalAgents = applicationAgent.PossibleAgents;

            ((ComboBox)sender).ItemsSource = filteredOptionalAgents;
        }

        private void XAgentNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //save last used agent on the Solution Target Applications
            ApplicationAgent ag = ApplicationAgents.FirstOrDefault(x => x.Agent == (Agent)((ComboBox)sender).SelectedValue);
            if (ag != null)
            {
                ApplicationPlatform ap = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == ag.AppName);
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
            return (Agent.eStatus)value switch
            {
                Agent.eStatus.Running => (SolidColorBrush)(new BrushConverter().ConvertFrom("#109717")),//green
                Agent.eStatus.Starting => (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFC268")),//yellow
                _ => (SolidColorBrush)(new BrushConverter().ConvertFrom("#DC3812")),//red
            };
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
            return (Agent.eStatus)value switch
            {
                Agent.eStatus.Running => "Agent is Running, Click to Close it",
                Agent.eStatus.Starting => "Agent is Starting...",
                _ => "Agent is Not Running, Click to Start it",
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AgentStartImageTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Agent.eStatus)value switch
            {
                Agent.eStatus.Running => eImageType.ToggleOn,
                Agent.eStatus.Starting => eImageType.ToggleOff,
                _ => (object)eImageType.ToggleOff,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}