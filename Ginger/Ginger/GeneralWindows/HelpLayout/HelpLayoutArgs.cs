using System.Windows;

namespace Ginger.GeneralWindows
{
    public class HelpLayoutArgs
    {
        //public enum eEventType
        //{
        //    ShowHelp,
        //}

        //public eEventType EventType;
        public string HelpLayoutKey;
        public FrameworkElement FocusedControl;
        public string HelpText;

        public HelpLayoutArgs(string helpLayoutKey, FrameworkElement focusedControl, string helpText)
        {
            this.HelpLayoutKey = helpLayoutKey;
            this.FocusedControl = focusedControl;
            this.HelpText = helpText;
        }
    }
}
