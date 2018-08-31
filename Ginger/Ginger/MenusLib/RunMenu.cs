using Ginger.GeneralWindows;
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

            TopMenuItem RunSetMenu = new TopMenuItem(GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.RunSet), ConsoleKey.R, "Run Set AID", GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.RunSet, "Create, Design and Execute "));
            RunSetMenu.Add(GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.RunSet), GetRunSetPage, ConsoleKey.R, "", "AID");
            twoLevelMenu.Add(RunSetMenu);

            TopMenuItem ExecutionsHistoryMenu = new TopMenuItem("Executions History", ConsoleKey.E, "Executions History AID", "View executions history of all Run Sets");
            ExecutionsHistoryMenu.Add("Executions History", GetExecutionsHistoryPage, ConsoleKey.E, "", "AID");
            twoLevelMenu.Add(ExecutionsHistoryMenu);

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
    }
}
