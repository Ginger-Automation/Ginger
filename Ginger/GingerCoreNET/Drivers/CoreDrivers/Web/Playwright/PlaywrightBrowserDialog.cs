using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPlaywrightBrowser = Microsoft.Playwright.IBrowser;
using IPlaywrightBrowserContext = Microsoft.Playwright.IBrowserContext;
using IPlaywrightPage = Microsoft.Playwright.IPage;
using IPlaywrightDialog = Microsoft.Playwright.IDialog;
using IPlaywrightLocator = Microsoft.Playwright.ILocator;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal sealed class PlaywrightBrowserDialog : IBrowserDialog
    {
        private readonly IPlaywrightDialog _playwrightDialog;
        private readonly IBrowserDialog.OnDialogHandle _onDialogHandle;

        internal PlaywrightBrowserDialog(IPlaywrightDialog playwrightDialog, IBrowserDialog.OnDialogHandle onDialogHandle)
        {
            _playwrightDialog = playwrightDialog;
            _onDialogHandle = onDialogHandle;
        }

        public Task<string> GetMessageAsync()
        {
            return Task.FromResult(_playwrightDialog.Message);
        }

        public async Task AcceptAsync()
        {
            await _playwrightDialog.AcceptAsync();
            await _onDialogHandle.Invoke(handledDialog: this);
        }

        public async Task DismissAsync()
        {
            await _playwrightDialog.DismissAsync();
            await _onDialogHandle.Invoke(handledDialog: this);
        }
    }
}
