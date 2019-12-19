using GingerCore;
using GingerWPF.UserControlsLib;
using GingerWPFUnitTest.POMs;
using System;
using System.Windows.Controls;

namespace GingerTest.POMs
{
    public class BusinessFlowPOM : GingerPOMBase
    {
        private SingleItemTreeViewExplorerPagePOM mTreeView;
        public event EventHandler ItemDoubleClick;
        public bool TreeItemDoubleClicked = false;

        public BusinessFlowPOM(SingleItemTreeViewExplorerPage page)
        {
            mTreeView = new SingleItemTreeViewExplorerPagePOM(page);
        }

        public SingleItemTreeViewExplorerPagePOM BusinessFlowsTree { get { return mTreeView; } }

        public BusinessFlow selectBusinessFlow(string name)
        {
            mTreeView.SelectItem(name);
            BusinessFlow businessFlow = (BusinessFlow)mTreeView.GetSelectedItemNodeObject();
            return businessFlow;
        }

        public void GoToAutomate()
        {
            mTreeView.GetSelectedItemEditPage();
        }

        public void AutomatePage(string name)
        {
            var elByName = FindElementByName(mTreeView.GetSelectedItemEditPage(), "xAutomateBtn");

            if (elByName != null)
            {
                if (elByName is Amdocs.Ginger.UserControls.ucButton)
                {
                    Dispatcher.Invoke(() =>
                    {
                        (elByName as Amdocs.Ginger.UserControls.ucButton).DoClick();
                        SleepWithDoEvents(100);
                    });
                }
            }
        }

    }
}

