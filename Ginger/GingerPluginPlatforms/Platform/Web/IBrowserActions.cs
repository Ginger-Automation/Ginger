using Ginger.Plugin.Platform.Web.Elements;
using System.Collections.Generic;

namespace Ginger.Plugin.Platform.Web
{
    public interface IBrowserActions
    {
        void Navigate(string url,string OpenIn);

        string GetCurrentUrl();
        void NavigateBack();
        void NavigateForward();
        void Refresh();
        string GetTitle();


        string GetWindowHandle();

        void CloseWindow();
        IReadOnlyCollection<string> GetWindowHandles();
        void SwitchToFrame(IGingerWebElement WebElement);
        void SwitchToParentFrame();

        void Maximize();
        void Minimize();
        void FullScreen();

        object ExecuteScript(string script);
        void CloseCurrentTab();
        void AcceptMessageBox();
        void SetAlertBoxText(string value);
        void DismissMessageBox();
        void DeleteAllCookies();
    }
}
