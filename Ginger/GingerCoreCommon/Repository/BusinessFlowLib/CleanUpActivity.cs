using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common.Enums;
using GingerCore;

namespace Amdocs.Ginger.Common.Repository.BusinessFlowLib
{
    public class CleanUpActivity: Activity
    {
        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.Eraser;
            }
        }

        public override string ActivityType
        {
            get
            {
                return GingerDicser.GetTermResValue(eTermResKey.Activity, "Clean Up");
            }
        }
    }
}
