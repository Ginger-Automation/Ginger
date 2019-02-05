#region License
/*
Copyright © 2014-2018 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
