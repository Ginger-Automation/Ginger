using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

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
            ActivitiesGroupColl = new List<GingerActvityGroup>();
        }
    }

    class GingerActvityGroup : GingerBaseData
    {
        public GingerActvityGroup()
        {
        }
    }
}
