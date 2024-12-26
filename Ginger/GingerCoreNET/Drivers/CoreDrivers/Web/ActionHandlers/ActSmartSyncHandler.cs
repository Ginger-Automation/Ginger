using GingerCore.Actions;
using GingerCore.Drivers;
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
        internal async Task HandleAsync(Act act)
        {
            mAct = act;

            int MaxTimeout = DriverBase.GetMaxTimeout(_actSmartSync);
            Stopwatch st = new Stopwatch();
            try
            {
                st.Start();
                IBrowserElement element;

                if (_actSmartSync.SmartSyncAction == ActSmartSync.eSmartSyncAction.WaitUntilDisplay)
                {
                    do
                    {
                        if (st.ElapsedMilliseconds > MaxTimeout * 1000)
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
                        if (st.ElapsedMilliseconds > MaxTimeout * 1000)
                        {
                            mAct.Error = "Smart Sync of WaitUntilDisapear is timeout";
                            break;
                        }
                        await Task.Delay(100);
                        element = await GetFirstMatchingElementAsync();
                    } while (element != null);
                }
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
