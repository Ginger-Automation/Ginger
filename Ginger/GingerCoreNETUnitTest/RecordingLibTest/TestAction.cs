using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Timers;

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
