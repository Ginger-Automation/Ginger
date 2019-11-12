using System.Windows.Controls;

namespace Ginger.GeneralWindows
{
    public class HelpLayoutEventArgs
    {
        public enum eEventType
        {
            ShowHelp,
        }

        public eEventType EventType;
        public Control FocusedControl;
        public string HelpText;

        public HelpLayoutEventArgs(eEventType eventType, Control focusedControl, string helpText)
        {
            this.EventType = eventType;
            this.FocusedControl = focusedControl;
            this.HelpText = helpText;
        }
    }
}
