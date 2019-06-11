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


    }
}
