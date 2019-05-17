using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using GingerCore.Actions;
using System;

namespace GingerCoreNETUnitTest.RecordingLibTest
{
    public class TestPlatform : IPlatformInfo
    {
        public Act GetPlatformAction(ElementInfo eInfo, ElementActionCongifuration actConfig)
        {
            TestAction elementAction = null;

            if (actConfig.LearnedElementInfo == null)
            {
                elementAction = new TestAction()
                {
                    Description = actConfig.Description,
                    ElementLocateBy = (eLocateBy)System.Enum.Parse(typeof(eLocateBy), Convert.ToString(actConfig.LocateBy)),
                    ElementAction = actConfig.Operation,
                    ElementLocateValue = actConfig.LocateValue,
                    ElementType = Convert.ToString(actConfig.Type),
                    Value = actConfig.ElementValue
                }; 
            }
            else
            {
                elementAction = new TestAction()
                {
                    Description = actConfig.Description,
                    ElementLocateBy = eLocateBy.POMElement,
                    ElementAction = actConfig.Operation,
                    ElementLocateValue = actConfig.LocateValue,
                    ElementType = Convert.ToString(actConfig.Type),
                    Value = actConfig.ElementValue
                };
            }

            return elementAction;
        }
    }
}
