#region License
/*
Copyright © 2014-2024 European Support Limited

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

using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using Deque.AxeCore.Commons;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal interface IBrowserTab
    {
        public delegate Task OnTabClose(IBrowserTab closedTab);

        public bool IsClosed { get; }

        public Task<string> URLAsync();

        public Task GoToURLAsync(string url);

          public Task<string> TitleAsync();

        public Task NavigateBackAsync();

        public Task NavigateForwardAsync();

        public Task RefreshAsync();

        public Task<string> PageSourceAsync();

        public Task MouseClickAsync(Point point);

        public Task MouseRightClickAsync(Point point);

        public Task MoveMouseAsync(Point point);

        public Task<string> ExecuteJavascriptAsync(string script);

        public Task<string> ExecuteJavascriptAsync(string script, object arg);
        public Task InjectJavascriptAsync(string script);

        public Task<string> ExecuteJavascriptIframeAsync(string script);

        public Task<string> ExecuteJavascriptIframeAsync(string script, object arg);

        public Task InjectJavascriptIframeAsync(string script);

        public Task WaitTillLoadedAsync();

        public Task<string> ConsoleLogsAsync();

        public Task<string> BrowserLogsAsync();

        public Task<bool> SwitchFrameAsync(eLocateBy locatyBy, string locateValue);

        public Task SwitchToMainFrameAsync();

        public Task SwitchToParentFrameAsync();

        public Task<byte[]> ScreenshotAsync();

        public Task<byte[]> ScreenshotFullPageAsync();

        public Task<Size> ViewportSizeAsync();

        public Task SetViewportSizeAsync(Size size);

        /// <summary>
        /// Get a collection of <see cref="IBrowserElement"/> matching the provided locators.
        /// </summary>
        /// <param name="locateBy">Locate element based on which property.</param>
        /// <param name="locateValue">The value of the locating property.</param>
        /// <returns></returns>
        /// <exception cref="LocatorNotSupportedException">If the provided <see cref="eLocateBy"/> is not supported.</exception>
        public Task<IEnumerable<IBrowserElement>> GetElementsAsync(eLocateBy locateBy, string locateValue);

        public Task<IBrowserElement?> GetElementAsync(string javascript);

        public Task<AxeResult?> TestAccessibilityAsync(AxeRunOptions? options = null);

        public Task CloseAsync();

        public Task StartListenDialogsAsync();

        public Task<bool> WaitForUrlMatchAsync(string urlPattern, float timeout);

        public Task<bool> WaitForAlertAsync(float timeout);

        public Task<bool> WaitForElementsEnabledAsync(eLocateBy locateBy, string selector, float timeout);

        public Task<bool> WaitForElementsCheckedAsync(eLocateBy locateBy, string locateValue, float timeout);
        public Task<bool> WaitForElementsPresenceAsync(eLocateBy locateBy, string locateValue, float timeout);
        public Task<bool> WaitForElementsInvisibleAsync(eLocateBy locateBy, string locateValue, float timeout);
        public Task<bool> WaitForElementsVisibleAsync(eLocateBy locateBy, string locateValue, float timeout);

    }
}
