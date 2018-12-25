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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.Agents
{
    /// <summary>
    /// Interaction logic for ucAgentControl.xaml //test
    /// </summary>
    public partial class ucAgentControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));                
            }
        }

        public delegate void AgentStartedUCEventHandler();

        public event AgentStartedUCEventHandler AgentStartedUCEvent;

        public void AgentStartedEvent()
        {
            if (AgentStartedUCEvent != null)
            {
                AgentStartedUCEvent();
            }
        }

        ObservableList<Agent> mOptionalAgentsList = null;

        public ucAgentControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedAgentProperty = DependencyProperty.Register("SelectedAgent", typeof(Agent), typeof(ucAgentControl), new PropertyMetadata(null, new PropertyChangedCallback(OnSelectedAgentChanged)));
      

        public Agent SelectedAgent
        {
            get
            {
                return (Agent)GetValue(SelectedAgentProperty);
            }
            set
            {
                SetValue(SelectedAgentProperty, value);
                OnPropertyChanged(nameof(SelectedAgent));
                OnPropertyChanged(nameof(AgentIsRunning));
            }
           
        }
        private static void OnSelectedAgentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ucAgentControl ucAgentControl = d as ucAgentControl;
            //ucAgentControl.xAgentsComboBox.SelectedItem = (Agent)e.NewValue;
        }

        public IWindowExplorer IWindowExplorerDriver
        {
            get
            {
                if (SelectedAgent != null)
                    return ((IWindowExplorer)(SelectedAgent.Driver));
                else
                    return null;
            }
        }
        
        public bool AgentIsRunning
        {
            get
            {
                if(SelectedAgent != null && SelectedAgent.Status == Agent.eStatus.Running)
                    return true;
                else
                    return false;
            }
        }

        public void Init(ObservableList<Agent> optionalAgentsList)
        {
            mOptionalAgentsList = optionalAgentsList;
            xAgentsComboBox.ItemsSource = mOptionalAgentsList;
            xAgentsComboBox.DisplayMemberPath = nameof(Agent.Name);

            if (mOptionalAgentsList != null && mOptionalAgentsList.Count > 0)
                xAgentsComboBox.SelectedItem = mOptionalAgentsList[0];

            App.ObjFieldBinding(xAgentsComboBox, ComboBox.SelectedItemProperty, this, nameof(this.SelectedAgent));
            SetAgentStatusView();
        }       

        private void xAgentsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedAgent == null) return;

            SelectedAgent.PropertyChanged -= SelectedAgent_PropertyChanged;
            SelectedAgent.PropertyChanged += SelectedAgent_PropertyChanged;
            SetAgentStatusView();
            UpdateAgentWindows();
            OnPropertyChanged(nameof(SelectedAgent));
            OnPropertyChanged(nameof(AgentIsRunning));
        }

        private void SelectedAgent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (((Agent)sender) == SelectedAgent)
            {
                if (e.PropertyName == nameof(Agent.Status))
                {
                    SetAgentStatusView();
                    UpdateAgentWindows();
                }
            }
        }

        private void SetAgentStatusView()
        { 
            if (SelectedAgent == null)
            {                
                xAgentStatusBtn.ButtonImageForground = Brushes.Gray;
                xAgentStatusBtn.ToolTip = "Please select Agent";
                xAgentConfigsExpander.IsExpanded = false;
                xAgentConfigsExpander.IsEnabled = false;
                xAgentConfigurationsGrid.IsEnabled = false;
                return;
            }         

            switch(SelectedAgent.Status)
            {
                case Agent.eStatus.FailedToStart:
                case Agent.eStatus.NotStarted:
                    xAgentStatusBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Power;
                    xAgentStatusBtn.ButtonImageForground = Brushes.Red;
                    xAgentStatusBtn.ToolTip = "Agent is Off, click to turn it On";
                    xAgentConfigsExpander.IsExpanded = false;
                    xAgentConfigsExpander.IsEnabled = false;
                    xAgentConfigurationsGrid.IsEnabled = false;
                    break;

                case Agent.eStatus.Starting:
                    xAgentStatusBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Processing;
                    xAgentStatusBtn.ButtonImageForground = Brushes.Orange;                    
                    xAgentStatusBtn.ToolTip = "Agent is starting...";
                    xAgentConfigsExpander.IsExpanded = false;
                    xAgentConfigsExpander.IsEnabled = false;
                    xAgentConfigurationsGrid.IsEnabled = false;
                    break;

                case Agent.eStatus.Completed:
                case Agent.eStatus.Ready:
                case Agent.eStatus.Running:
                    xAgentStatusBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Power;
                    xAgentStatusBtn.ButtonImageForground = Brushes.Green;                   
                    xAgentStatusBtn.ToolTip = "Agent is On, click to turn it Off";
                    xAgentConfigsExpander.IsExpanded = true;
                    xAgentConfigsExpander.IsEnabled = true;
                    xAgentConfigurationsGrid.IsEnabled = true;
                    break;
            }

            GingerCore.General.DoEvents();
        }

        private void xAgentStatusBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAgent == null) return;

            switch (SelectedAgent.Status)
            {
                case Agent.eStatus.FailedToStart:
                case Agent.eStatus.NotStarted:
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.StartAgent, null, SelectedAgent.Name, "");
                    if (SelectedAgent.Status == Agent.eStatus.Running) SelectedAgent.Close();
                    SelectedAgent.SolutionFolder = App.UserProfile.Solution.Folder;
                    SelectedAgent.ProjEnvironment = null;// App.AutomateTabEnvironment;
                    SelectedAgent.BusinessFlow = null; //App.BusinessFlow; ;                    
                    SelectedAgent.DSList = null; //WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
                    SelectedAgent.StartDriver();
                    Reporter.CloseGingerHelper();
                    //If there is errorMessageFromDriver is populated then do not wait. 
                    if (SelectedAgent.Driver != null && String.IsNullOrEmpty(SelectedAgent.Driver.ErrorMessageFromDriver))
                        SelectedAgent.WaitForAgentToBeReady();
                    Agent.eStatus Status = SelectedAgent.Status;
                    if (Status != Agent.eStatus.Running && Status != Agent.eStatus.Starting)
                    {
                        string errorMessage = SelectedAgent.Driver.ErrorMessageFromDriver;
                        if (String.IsNullOrEmpty(errorMessage))
                            errorMessage = "Failed to Connect the agent";
                        Reporter.ToGingerHelper(eGingerHelperMsgKey.StartAgentFailed, null, errorMessage);
                    }
                    SelectedAgent.Tag = "Started with Agent Control";
                    AgentStartedEvent();
                    break;

                case Agent.eStatus.Starting:
                    SelectedAgent.Driver.cancelAgentLoading = true;
                    break;

                case Agent.eStatus.Completed:
                case Agent.eStatus.Ready:
                case Agent.eStatus.Running:
                    SelectedAgent.Close();
                    break;
            }

            //SelectedAgent = SelectedAgent; //OnPropertyChanged(nameof(SelectedAgent));
            UpdateBinding();
            OnPropertyChanged(nameof(AgentIsRunning));
        }

        /// <summary>
        /// Been used for updating validation rules which checks also Agent status 
        /// </summary>
        private void UpdateBinding()
        {
            BindingExpression bindingExpression = null;
            bindingExpression = this.GetBindingExpression(Ginger.Agents.ucAgentControl.SelectedAgentProperty);
            if (bindingExpression != null)
                bindingExpression.UpdateSource();
        }

        private void xAgentWindowsRefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateAgentWindows();
        }

        private void UpdateAgentWindows()
        {
            if (IWindowExplorerDriver == null)
            {
                xAgentWindowsComboBox.ItemsSource = null;
                //xAgentWindowsRefreshBtn.ButtonImageForground = Brushes.Gray;
                return;
            }
            List<AppWindow> winsList = null;

            if (SelectedAgent.Status == Agent.eStatus.Completed || SelectedAgent.Status == Agent.eStatus.Ready || SelectedAgent.Status == Agent.eStatus.Running)
            {
                winsList = ((IWindowExplorer)(SelectedAgent.Driver)).GetAppWindows();
            }
            xAgentWindowsComboBox.ItemsSource = winsList;
            xAgentWindowsComboBox.DisplayMemberPath = nameof(AppWindow.WinInfo);

            //defualt selection            
            if (winsList != null && winsList.Count > 0)
            {
                AppWindow activeWindow = ((IWindowExplorer)(SelectedAgent.Driver)).GetActiveWindow();
                if (activeWindow != null)
                {
                    foreach (AppWindow win in winsList)
                    {
                        if (win.Title == activeWindow.Title && win.Path == activeWindow.Path)
                        {
                            xAgentWindowsComboBox.SelectedItem = win;
                            return;
                        }
                    }
                }

                xAgentWindowsComboBox.SelectedItem = winsList[0];
            }
        }

        private void xAgentWindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IWindowExplorerDriver == null) return;

            AppWindow page = (AppWindow)xAgentWindowsComboBox.SelectedItem;
            if (page == null) return;
            IWindowExplorerDriver.SwitchWindow(page.Title);
        }
    }
}
