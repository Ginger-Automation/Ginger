using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.Drivers.ConsoleDriverLib
{
    public interface IGingerConsole
    {
        public void Open();

        public void Close();

        public string RunConsoleCommand(string command, string waitForText = null);

        public void TakeScreenshot(Act act);

        public void WriteConsoleText(string txt, bool applyFormat = false);
    }
}
