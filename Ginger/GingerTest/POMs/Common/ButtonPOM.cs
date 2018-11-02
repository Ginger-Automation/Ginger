using GingerTest.VideosLib;
using GingerWPFUnitTest;
using GingerWPFUnitTest.POMs;
using System.Threading;
using System.Windows.Controls;

namespace GingerTest.POMs.Common
{
    public class ButtonPOM : GingerPOMBase
    {
        Button mButton;
        public ButtonPOM(Button button)
        {
            mButton = button;
        }

        public void Click()
        {
            Execute(() => {

                if (GingerAutomator.Highlight)
                {
                    HighlightAdorner highlightAdorner = new HighlightAdorner(mButton);
                    DoEvents();
                    SleepWithDoEvents(100);
                    Speach.Say("Click Button " + mButton.Content.ToString());
                    Thread.Sleep(500);
                }
                mButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            });
            
        }
    }
}
