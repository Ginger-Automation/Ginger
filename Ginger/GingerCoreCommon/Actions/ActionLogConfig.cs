using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Actions
{
    public class ActionLogConfig
    {
        [IsSerializedForLocalRepository]
        public string LogText { get; set; }

        [IsSerializedForLocalRepository]
        public bool LogInputVariables { get; set; }

        [IsSerializedForLocalRepository]
        public bool LogOutputVariables { get; set; }
    }

}
