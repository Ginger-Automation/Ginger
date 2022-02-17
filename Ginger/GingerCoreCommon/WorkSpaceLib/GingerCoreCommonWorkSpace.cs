using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.SolutionGeneral;

namespace Amdocs.Ginger.Common.WorkSpaceLib
{
    public class GingerCoreCommonWorkSpace
    {
        private static readonly GingerCoreCommonWorkSpace _instance = new GingerCoreCommonWorkSpace();
        public static GingerCoreCommonWorkSpace Instance
        {
            get
            {
                return _instance;
            }
        }

        public UserProfile UserProfile
        {
            get;
            set;
        }

        public Solution Solution
        {
            get;
            set;
        }

        public SolutionRepository SolutionRepository
        {
            get;
            set;
        }
    }
}
