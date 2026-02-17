#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using LiteDB;
using System.Collections.Generic;

namespace GingerCoreNETUnitTest.LiteDb
{
    class GingerBaseData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string RunStatus { get; set; }
        public string GUID { get; set; }
        public ObjectId _id { get; set; }
        public GingerBaseData()
        {
            _id = ObjectId.NewObjectId();
        }
    }
    class GingerBusinessFlow : GingerBaseData
    {
        public List<GingerActvityGroup> ActivitiesGroupColl { get; set; }
        public GingerBusinessFlow()
        {
            ActivitiesGroupColl = [];
        }
        public static ILiteCollection<GingerBusinessFlow> IncludeAllReferences(ILiteCollection<GingerBusinessFlow> gingerBusinessFlow)
        {
            return gingerBusinessFlow.Include((gBf) => gBf.ActivitiesGroupColl);
        }
    }

    class GingerActvityGroup : GingerBaseData
    {
        public GingerActvityGroup()
        {
        }
    }
}
