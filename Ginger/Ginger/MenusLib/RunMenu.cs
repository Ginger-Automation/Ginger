#region License
/*
Copyright © 2014-2018 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
using Ginger.GeneralWindows;
using Ginger.GingerGridLib;
using Ginger.Run;
using Ginger.TwoLevelMenuLib;
using System;
using System.Windows.Controls;

namespace Ginger.MenusLib
{
    public static class RunMenu
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
            TwoLevelMenu twoLevelMenuGet = new TwoLevelMenu();

            TopMenuItem runSetMenu = new TopMenuItem(eImageType.RunSet, GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.RunSet), ConsoleKey.R, "Run Set AID", GingerCore.GingerDicser.GetTermResValue(GingerCore.eTermResKey.RunSet, "Create, Design and Execute "));
            runSetMenu.Add(eImageType.RunSet, "", GetRunSetPage, ConsoleKey.R, "", "AID");
            twoLevelMenuGet.Add(runSetMenu);

            TopMenuItem gingerGridMenu = new TopMenuItem(eImageType.List, "Gingers Grid", ConsoleKey.G, "Ginger Grid AID", "Grid showing all connected Ginger Nodes");
            gingerGridMenu.Add(eImageType.List, "", GetGingerGridPage, ConsoleKey.G, "", "AID");
            twoLevelMenuGet.Add(gingerGridMenu);
            
            TopMenuItem executionsHistoryMenu = new TopMenuItem(eImageType.History, "Executions History", ConsoleKey.E, "Executions History AID", "View executions history of all Run Sets");
            executionsHistoryMenu.Add(eImageType.History, "", GetExecutionsHistoryPage, ConsoleKey.E, "", "AID");
            twoLevelMenuGet.Add(executionsHistoryMenu);

            return twoLevelMenuGet;
        }

        static NewRunSetPage runSetPage;
        private static Page GetRunSetPage()
        {            
            if (runSetPage == null)
            {
                runSetPage= new NewRunSetPage();
            }
            return runSetPage;
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
