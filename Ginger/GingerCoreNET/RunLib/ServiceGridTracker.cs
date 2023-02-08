#region License
/*
Copyright © 2014-2023 European Support Limited

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

using GingerCoreNET.RunLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCoreNET.RunLib
{
    public class ServiceGridTracker
    {
        GingerGrid mGingerGrid;

        public ServiceGridTracker(GingerGrid gingerGrid)
        {
            mGingerGrid = gingerGrid;
            mGingerGrid.NodeList.CollectionChanged += NodeList_CollectionChanged;
            PrintGridInfo();
        }

        private void NodeList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PrintGridInfo();
        }

        void PrintGridInfo()
        {
            Console.WriteLine("Ginger Grid Info last update: " + DateTime.Now);
            Console.WriteLine("Ginger Node Count=" + mGingerGrid.NodeList.Count);
            Console.WriteLine("========================================================================================");
            Console.WriteLine("| # | Name       | Service ID          | Host      | OS       | IP          | Status   |");
            Console.WriteLine("========================================================================================");
            int i = 0;
            foreach (GingerNodeInfo GNI in mGingerGrid.NodeList)
            {
                i++;
                if (i > 2)
                {
                    Console.WriteLine("----------------------------------------------------------");
                }
                Console.WriteLine("| " + i + " | " + GNI.Name + " | " + GNI.ServiceId + " | " + GNI.Host + " | " + GNI.OS + " | " + GNI.IP + " | " + GNI.Status + " |");
            }
            Console.WriteLine("========================================================================================");
        }
    }
}
