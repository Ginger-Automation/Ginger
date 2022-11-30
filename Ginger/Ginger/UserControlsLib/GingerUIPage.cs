using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib
{
    public abstract class GingerUIPage : Page
    {
        private RepositoryItemBase mCurrentItem;
        protected RepositoryItemBase CurrentItemToSave { get { return mCurrentItem; } set { if (mCurrentItem != value) { mCurrentItem = value; WorkSpace.Instance.CurrentSelectedItem = mCurrentItem; } } }
        protected GingerUIPage()
        {
            IsVisibleChanged += IsVisibleChangedHandler;
        }

        protected virtual void IsVisibleChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (mCurrentItem != null)
            {
                if ((bool)e.NewValue)
                {
                    WorkSpace.Instance.CurrentSelectedItem = mCurrentItem;
                }
                else
                {
                    if (WorkSpace.Instance.CurrentSelectedItem == mCurrentItem)
                    {
                        WorkSpace.Instance.CurrentSelectedItem = null;
                    }
                }
            }
        }
    }
}
