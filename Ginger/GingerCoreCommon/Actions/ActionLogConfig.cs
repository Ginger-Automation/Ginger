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

        [IsSerializedForLocalRepository]
        public bool LogRunStatus { get; set; }

        [IsSerializedForLocalRepository]
        public bool LogError { get; set; }

        [IsSerializedForLocalRepository]
        public bool LogElapsedTime { get; set; }
    }

}
