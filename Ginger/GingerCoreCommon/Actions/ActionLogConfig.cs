using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Actions
{
    public class ActionLogConfig : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public string ActionLogText { get; set; }

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

        public override string ItemName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

}
