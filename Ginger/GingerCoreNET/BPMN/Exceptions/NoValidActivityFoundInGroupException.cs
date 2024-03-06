using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN.Exceptions
{
    public sealed class NoValidActivityFoundInGroupException : BPMNConversionException
    {
        public NoValidActivityFoundInGroupException(string msg) : base(msg) { }

        public static NoValidActivityFoundInGroupException WithDefaultMessage(string activityGroupName)
        {
            return new($"No valid {GingerDicser.GetTermResValue(eTermResKey.Activity)} found in {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)} '{activityGroupName}'.");
        }
    }
}
