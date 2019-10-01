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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowPages.AddActionMenu;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for LiveSpyNavAction.xaml
    /// </summary>
    public partial class LiveSpyNavPage : Page, INavPanelPage
    {
        Context mContext;
        IWindowExplorer mDriver;
        List<AgentPageMappingHelper> mLiveSpyPageDictonary = null;
        LiveSpyPage mCurrentLoadedPage = null;

        public LiveSpyNavPage(Context context)
        {
            InitializeComponent();

            mContext = context;
            context.PropertyChanged += Context_PropertyChanged;

            if (mContext.Agent != null)
            {
                mDriver = mContext.Agent.Driver as IWindowExplorer;
            }
            else
            {
                mDriver = null;
            }

            LoadLiveSpyPage(mContext);
        }

        public void ReLoadPageItems()
        {
            if (mContext.Agent != null)
            {
                mDriver = mContext.Agent.Driver as IWindowExplorer;
                LoadLiveSpyPage(mContext);
            }
            else
            {
                mDriver = null;
            }

            mCurrentLoadedPage.SetDriver(mDriver);
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
                        mDriver = mContext.Agent.Driver as IWindowExplorer;
                    }
                    else
                    {
                        mDriver = null;
                    }

                    if (e.PropertyName == nameof(Context.Agent) && mContext.Agent != null)
                    {
                        LoadLiveSpyPage(mContext);
                    }
                    else
                    {
                        mCurrentLoadedPage.SetDriver(mDriver);
                    }
                }
            }
        }

        /// <summary>
        /// This method is used to get the new LiveSpyPage based on Context and Agent
        /// </summary>
        /// <returns></returns>
        private void LoadLiveSpyPage(Context context)
        {
            this.Dispatcher.Invoke(() =>
            {
                bool isLoaded = false;
                if (mLiveSpyPageDictonary != null && mLiveSpyPageDictonary.Count > 0 && context.Agent != null)
                {
                    AgentPageMappingHelper objHelper = mLiveSpyPageDictonary.Find(x => x.ObjectAgent.DriverType == context.Agent.DriverType &&
                                                                                    x.ObjectAgent.ItemName == context.Agent.ItemName);
                    if (objHelper != null && objHelper.ObjectWindowPage != null)
                    {
                        mCurrentLoadedPage = (LiveSpyPage)objHelper.ObjectWindowPage;
                        isLoaded = true;
                    }
                }

                if (!isLoaded)
                {
                    ApplicationAgent appAgent = AgentHelper.GetAppAgent(mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext);
                    if (appAgent != null)
                    {
                        mCurrentLoadedPage = new LiveSpyPage(mContext, mDriver);
                        if (mLiveSpyPageDictonary == null)
                        {
                            mLiveSpyPageDictonary = new List<AgentPageMappingHelper>();
                        }
                        mLiveSpyPageDictonary.Add(new AgentPageMappingHelper(context.Agent, mCurrentLoadedPage));
                    }
                }

                xSelectedItemFrame.Content = mCurrentLoadedPage;
            });
        }
    }
}
