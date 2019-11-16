using System.Windows.Controls;

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
        public Control FocusedControl;
        public string HelpText;

        public HelpLayoutArgs(string helpLayoutKey, Control focusedControl, string helpText)
        {
            this.HelpLayoutKey = helpLayoutKey;
            this.FocusedControl = focusedControl;
            this.HelpText = helpText;
        }
    }
}
