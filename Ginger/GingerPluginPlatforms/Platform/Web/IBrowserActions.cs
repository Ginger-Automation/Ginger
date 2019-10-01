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

using Ginger.Plugin.Platform.Web.Elements;
using System.Collections.Generic;

namespace Ginger.Plugin.Platform.Web
{
    /// <summary>
    /// Exposes Browser Action
    /// </summary>
    public interface IBrowserActions: IAlerts
    {

        /// <summary>
        /// Performs Navigation in Current Tab, New Tab or In a new Tab
        /// </summary>
        /// <param name="url"></param>
        /// <param name="OpenIn"></param>
        void Navigate(string url,string OpenIn);
        /// <summary>
        /// Gets  URL of Page currently Selected.
        /// </summary>
        /// <returns></returns>
        string GetCurrentUrl();

        /// <summary>
        /// Performs Back currently selected Window.
        /// </summary>
        void NavigateBack();

        /// <summary>
        /// Performs Forward currently selected Window.
        /// </summary>
        void NavigateForward();


        /// <summary>
        /// Refreshes Currently Selected Page.
        /// </summary>
        void Refresh();


        /// <summary>
        /// Gets Title of  selected Window.
        /// </summary>
        string GetTitle();


        /// <summary>
        ///Gets Handle of  selected Window.
        /// </summary>
        string GetWindowHandle();


        /// <summary>
        /// Closes cureent WIndow
        /// </summary>
        void CloseWindow();
        /// <summary>
        /// Gets you handle of all open Windows/Tabs.
        /// </summary>
        /// <returns></returns>
        IReadOnlyCollection<string> GetWindowHandles();
        /// <summary>
        /// Switches Focus to Provided Frame.
        /// </summary>
        /// <param name="WebElement"></param>
        void SwitchToFrame(IGingerWebElement WebElement);

        /// <summary>
        /// Switches to The Parent froame of Current Element.
        /// </summary>
        void SwitchToParentFrame();

        /// <summary>
        /// Switches to the default content..
        /// </summary>
        void SwitchToDefaultContent();


        /// <summary>
        /// Maximises CUrrent Browser Window
        /// </summary>
        void Maximize();

        /// <summary>
        /// Minimize CUrrent Browser Window
        /// </summary>
        void Minimize();

        /// <summary>
        /// FullScreen CUrrent Browser Window
        /// </summary>
        void FullScreen();

        /// <summary>
        /// Executes Script on currenttly selected Browser Window
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        object ExecuteScript(string script);


        /// <summary>
        /// Closes Current Tab.
        /// </summary>
        void CloseCurrentTab();
        /// <summary>
        /// Deletes AllCookies .
        /// </summary>
        void DeleteAllCookies();
        string GetPageSource();
    }
}
