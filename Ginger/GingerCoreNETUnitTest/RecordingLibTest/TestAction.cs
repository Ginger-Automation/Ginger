#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCoreNETUnitTest.RecordingLibTest
{
    public class TestAction : Act
    {
        public override string ActionType => throw new NotImplementedException();

        public override string ActionDescription => throw new NotImplementedException();

        public override bool ObjectLocatorConfigsNeeded => throw new NotImplementedException();

        public override bool ValueConfigsNeeded => throw new NotImplementedException();

        public override List<ePlatformType> Platforms => throw new NotImplementedException();

        public override string ActionEditPage => throw new NotImplementedException();

        public override string ActionUserDescription => throw new NotImplementedException();

        public eLocateBy ElementLocateBy { get; internal set; }

        public string ElementAction { get; internal set; }

        public string ElementLocateValue { get; internal set; }

        public string ElementType { get; internal set; }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            throw new NotImplementedException();
        }
    }
}
