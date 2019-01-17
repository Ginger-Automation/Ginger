using GingerCoreNET.RunLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GingerWeb.RepositoryLib
{
    public class GingerNodeInfoWrapper
    {
        GingerNodeInfo mGingerNodeInfo;

        public GingerNodeInfoWrapper(GingerNodeInfo gingerNodeInfo)
        {
            mGingerNodeInfo = gingerNodeInfo;
        }

        public string name { get { return mGingerNodeInfo.Name; } }

        public string serviceId { get { return mGingerNodeInfo.ServiceId; } }

        public Guid sessionID { get { return mGingerNodeInfo.SessionID; } }

        public int actionCount { get { return mGingerNodeInfo.ActionCount; } }
    }
}
