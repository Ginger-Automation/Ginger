#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common;

namespace Ginger.TwoLevelMenuLib
{
    public class TwoLevelMenu
    {
        public ObservableList<TopMenuItem> MenuList = [];

        internal void Add(TopMenuItem topMenuItem)
        {
            MenuList.Add(topMenuItem);
        }

        internal void Reset()
        {
            foreach (TopMenuItem topMenuItem in MenuList)
            {
                if (!WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures)
                {
                    if (WorkSpace.Instance.Solution != null && topMenuItem.Name == WorkSpace.Instance.Solution.ExternalIntegrationsTabName)
                    {
                        continue;
                    }
                }
                foreach (SubMenuItem subMenuItem in topMenuItem.SubItems)
                {
                    subMenuItem.ResetPage();
                }
            }
        }
    }
}
