using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.Enums
{
    public enum eSetValueOptions
    {
        [EnumValueDescription("Set Value")]
        SetValue,
        [EnumValueDescription("Reset Value")]
        ResetValue,
        [EnumValueDescription("Auto Generate Value")]
        AutoGenerateValue,
        [EnumValueDescription("Start Timer")]
        StartTimer,
        [EnumValueDescription("Stop Timer")]
        StopTimer,
        [EnumValueDescription("Continue Timer")]
        ContinueTimer,
        [EnumValueDescription("Clear Special Characters")]
        ClearSpecialChar
    }
}
