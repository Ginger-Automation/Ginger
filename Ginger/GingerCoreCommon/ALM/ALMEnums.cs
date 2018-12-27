using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GingerCore.ALM
{
    public enum FilterByStatus
    {
        [Description("All")]
        All,
        [Description("Only Passed")]
        OnlyPassed,
        [Description("Only Failed")]
        OnlyFailed
    }
}
