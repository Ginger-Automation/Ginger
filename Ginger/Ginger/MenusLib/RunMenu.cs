using amdocs.ginger.GingerCoreNET;
using Ginger.GeneralWindows;
using Ginger.GingerGridLib;
using Ginger.Run;
using Ginger.TwoLevelMenuLib;
using System;
using System.Windows.Controls;

namespace Ginger.MenusLib
{
    public class RunMenu
    {
        public static TwoLevelMenu twoLevelMenu;

        private static TwoLevelMenuPage mMenusPage = null;
        public static TwoLevelMenuPage MenusPage
        {
            get
            {
                if (mMenusPage == null)
                {
                    mMenusPage = new TwoLevelMenuPage(GetMenu());
                    App.UserProfile.PropertyChanged += UserProfile_PropertyChanged;
                }
                return mMenusPage;
            }
        }

        private static void UserProfile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserProfile.Solution))
            {
                MenusPage.Reset();
            }
        }

        private static TwoLevelMenu GetMenu()
        {
            TwoLevelMenu twoLevelMenu = new TwoLevelMenu();

            TopMenuItem runSetMenu = new TopMenuItem(GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.RunSet), ConsoleKey.R, "Run Set AID", GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.RunSet, "Create, Design and Execute "));
            runSetMenu.Add("", GetRunSetPage, ConsoleKey.R, "", "AID");
            twoLevelMenu.Add(runSetMenu);

            TopMenuItem gingerGridMenu = new TopMenuItem("Gingers Grid", ConsoleKey.G, "Ginger Grid AID", "Grid showing all connected Ginger Nodes");
            gingerGridMenu.Add("", GetGingerGridPage, ConsoleKey.G, "", "AID");
            twoLevelMenu.Add(gingerGridMenu);
            

            TopMenuItem executionsHistoryMenu = new TopMenuItem("Executions History", ConsoleKey.E, "Executions History AID", "View executions history of all Run Sets");
            executionsHistoryMenu.Add("", GetExecutionsHistoryPage, ConsoleKey.E, "", "AID");
            twoLevelMenu.Add(executionsHistoryMenu);

            return twoLevelMenu;
        }

        private static Page GetRunSetPage()
        {            
            return new NewRunSetPage();
        }
        private static Page GetExecutionsHistoryPage()
        {
            return new RunSetsExecutionsHistoryPage(RunSetsExecutionsHistoryPage.eExecutionHistoryLevel.Solution);
        }

        private static Page GetGingerGridPage()
        {
            return new GingerGridPage(WorkSpace.Instance.LocalGingerGrid);
        }
    }
}
