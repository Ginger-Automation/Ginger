using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for ucAgentControl.xaml
    /// </summary>
    public partial class ucAgentControl : UserControl
    {
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
            }
           
        }
        private static void OnSelectedAgentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ucAgentControl ucAgentControl = d as ucAgentControl;
            //ucAgentControl.xAgentsComboBox.SelectedItem = (Agent)e.NewValue;
        }

        private IWindowExplorer mIWindowExplorerDriver
        {
            get
            {
                if (SelectedAgent != null)
                    return ((IWindowExplorer)(SelectedAgent.Driver));
                else
                    return null;
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
                    xAgentConfigurationsGrid.IsEnabled = false;
                    break;

                case Agent.eStatus.Starting:
                    xAgentStatusBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Processing;
                    xAgentStatusBtn.ButtonImageForground = Brushes.Orange;                    
                    xAgentStatusBtn.ToolTip = "Agent is starting...";                    
                    xAgentConfigurationsGrid.IsEnabled = false;
                    break;

                case Agent.eStatus.Completed:
                case Agent.eStatus.Ready:
                case Agent.eStatus.Running:
                    xAgentStatusBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Power;
                    xAgentStatusBtn.ButtonImageForground = Brushes.Green;                   
                    xAgentStatusBtn.ToolTip = "Agent is On, click to turn it Off";
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
                    SelectedAgent.DSList = null; //App.LocalRepository.GetSolutionDataSources();
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
        }

        private void xAgentWindowsRefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateAgentWindows();
        }

        private void UpdateAgentWindows()
        {
            if (mIWindowExplorerDriver == null)
            {
                xAgentWindowsComboBox.ItemsSource = null;
                xAgentWindowsRefreshBtn.ButtonImageForground = Brushes.Gray;
                return;
            }

            xAgentWindowsRefreshBtn.ButtonImageForground = (SolidColorBrush)FindResource("$DarkBlue"); 

            List<AppWindow> winsList = ((IWindowExplorer)(SelectedAgent.Driver)).GetAppWindows();
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
            if (mIWindowExplorerDriver == null) return;

            AppWindow page = (AppWindow)xAgentWindowsComboBox.SelectedItem;
            if (page == null) return;
            mIWindowExplorerDriver.SwitchWindow(page.Title);
        }
    }
}
