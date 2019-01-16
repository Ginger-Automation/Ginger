using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GingerWeb.RepositoryLib
{
    public class BusinessFlowWrapper
    {
        BusinessFlow mBusinessFlow;

        public BusinessFlowWrapper(BusinessFlow businessFlow)
        {
            mBusinessFlow = businessFlow;
        }

        public string name { get { return mBusinessFlow.Name; } }

        public string description { get { return mBusinessFlow.Description; } }

        public string fileName { get { return mBusinessFlow.FileName; } }

    }
}
