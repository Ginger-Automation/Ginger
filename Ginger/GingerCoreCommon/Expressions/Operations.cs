using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Amdocs.Ginger.Common.Expressions
{
    public enum eOperator
    {
        [EnumValueDescription("=")]
        Equals,
        [EnumValueDescription("<>")]
        NotEquals,
        [EnumValueDescription(">")]
        GreaterThan,
        [EnumValueDescription(">=")]
        GreaterThanEquals,
        [EnumValueDescription("<")]
        LessThan,
        [EnumValueDescription("<=")]
        LessThanEquals,
        [EnumValueDescription("Contains")]
        Contains,
        [EnumValueDescription("Does Not Contains")]
        DoesNotContains,
        Evaluate,
        Legacy,

    }
    public class Operations
    {
  
    }
}
