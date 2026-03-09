#region License
/*
Copyright © 2014-2026 European Support Limited

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
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.ActionHandlers
{
    internal class ActSmartSyncHandler
    {
        private ActSmartSync _actSmartSync;
        private IBrowserTab _browserTab;
        private IBrowserElementLocator _elementLocator;
        private Act mAct;

        internal ActSmartSyncHandler(ActSmartSync actSmartSync, IBrowserTab browserTab, IBrowserElementLocator elementLocator)
        {
            _actSmartSync = actSmartSync;
            _browserTab = browserTab;
            _elementLocator = elementLocator;
        }

        /// <summary>
        /// Handles the smart synchronization action asynchronously.
        /// </summary>
        /// <param name="act">The action to be handled.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task HandleAsync(Act act, int timeout)
        {
            mAct = act;
            Stopwatch st = new Stopwatch();
            try
            {
                st.Start();
                IBrowserElement element;

                if (_actSmartSync.SmartSyncAction == ActSmartSync.eSmartSyncAction.WaitUntilDisplay)
                {
                    do
                    {
                        if (st.ElapsedMilliseconds > timeout)
                        {
                            mAct.Error = "Smart Sync of WaitUntilDisplay is timeout";
                            break;
                        }
                        await Task.Delay(100);
                        element = await GetFirstMatchingElementAsync();
                    } while (element == null || !await element.IsVisibleAsync() || !await element.IsEnabledAsync());
                }
                else if (_actSmartSync.SmartSyncAction == ActSmartSync.eSmartSyncAction.WaitUntilDisapear)
                {
                    do
                    {
                        if (st.ElapsedMilliseconds > timeout)
                        {
                            mAct.Error = "Smart Sync of WaitUntilDisapear is timeout";
                            break;
                        }
                        await Task.Delay(100);
                        element = await GetFirstMatchingElementAsync();
                    } while (element != null || (element != null && !await element.IsVisibleAsync()));
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to handle Smart Sync", ex);
            }
            finally
            {
                st.Stop();
            }
        }
        /// <summary>
        /// Gets the first matching browser element asynchronously.
        /// </summary>
        /// <returns>The first matching browser element.</returns>
        private async Task<IBrowserElement> GetFirstMatchingElementAsync()
        {
            IEnumerable<IBrowserElement> elements = await _elementLocator.FindMatchingElements(mAct.LocateBy, mAct.LocateValue);
            IBrowserElement? firstElement = elements.FirstOrDefault();
            return firstElement;
        }
    }
}
