#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowPages.AddActionMenu;
using Ginger.Run;
using Ginger.WindowExplorer;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for WindowsExplorerNavPage.xaml
    /// </summary>
    public partial class WindowsExplorerNavPage : Page, INavPanelPage
    {
        Context mContext;
        private IWindowExplorer mWindowExplorerDriver;
        IWindowExplorer WindowExplorerDriver
        {
            get
            {
                return mWindowExplorerDriver;
            }
            set
            {
                mWindowExplorerDriver = value;
            }
        }
        List<AgentPageMappingHelper> mWinExplorerPageList = null;
        WindowExplorerPage mCurrentLoadedPage = null;

        public WindowsExplorerNavPage(Context context)
        {
            InitializeComponent();

            mContext = context;            
            context.PropertyChanged += Context_PropertyChanged;

            if (mContext.Agent != null)
            {
                WindowExplorerDriver = ((AgentOperations)mContext.Agent.AgentOperations).Driver as IWindowExplorer;
            }
            else
            {
                WindowExplorerDriver = null;
            }

            LoadWindowExplorerPage();            
        }
        
        /// <summary>
        /// Context Property changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.IsVisible && MainAddActionsNavigationPage.IsPanelExpanded)
            {
                if (e.PropertyName == nameof(Context.AgentStatus) || e.PropertyName == nameof(Context.Agent))
                {
                    if (mContext.Agent != null)
                    {
                        WindowExplorerDriver = ((AgentOperations)mContext.Agent.AgentOperations).Driver as IWindowExplorer;
                    }
                    else
                    {
                        WindowExplorerDriver = null;
                    }

                    if (e.PropertyName == nameof(Context.Agent) && mContext.Agent != null)
                    {
                        LoadWindowExplorerPage();
                    }
                    else
                    {
                        mCurrentLoadedPage.SetDriver(WindowExplorerDriver);
                    }
                }
            }
        }

        /// <summary>
        /// This method is used to do the search
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoSearchControls()
        {            
            return await mCurrentLoadedPage.DoSearchControls();
        }

        /// <summary>
        /// This method is used to get the new WindowExplorerPage based on Context and Agent
        /// </summary>
        /// <returns></returns>
        private void LoadWindowExplorerPage()
        {
            this.Dispatcher.Invoke(() =>
            {
                bool isLoaded = false;
                if (mWinExplorerPageList != null && mWinExplorerPageList.Count > 0)
                {
                    AgentPageMappingHelper objHelper = mWinExplorerPageList.Where(x => x.ObjectAgent.DriverType == mContext.Agent.DriverType &&
                                                                                    x.ObjectAgent.ItemName == mContext.Agent.ItemName).FirstOrDefault();
                    if (objHelper != null && objHelper.ObjectWindowPage != null)
                    {
                        mCurrentLoadedPage = (WindowExplorerPage)objHelper.ObjectWindowPage;
                        isLoaded = true;
                    }
                }

                if (!isLoaded)
                {
                    ApplicationAgent appAgent = AgentHelper.GetAppAgent(mContext.BusinessFlow.CurrentActivity, (GingerExecutionEngine)mContext.Runner, mContext);
                    if (appAgent != null)
                    {
                        mCurrentLoadedPage = new WindowExplorerPage(appAgent, mContext);
                        mCurrentLoadedPage.SetDriver(WindowExplorerDriver);
                        if (mWinExplorerPageList == null)
                        {
                            mWinExplorerPageList = new List<AgentPageMappingHelper>();
                        }
                        mWinExplorerPageList.Add(new AgentPageMappingHelper(mContext.Agent, mCurrentLoadedPage));
                    }
                }

                xSelectedItemFrame.Content = mCurrentLoadedPage;
            });
        }

        public void ReLoadPageItems()
        {
            if (mContext.Agent != null)
            {
                WindowExplorerDriver = ((AgentOperations)mContext.Agent.AgentOperations).Driver as IWindowExplorer;
                LoadWindowExplorerPage();
            }
            else
            {
                WindowExplorerDriver = null;
            }

            mCurrentLoadedPage.SetDriver(WindowExplorerDriver);
        }
    }
}
