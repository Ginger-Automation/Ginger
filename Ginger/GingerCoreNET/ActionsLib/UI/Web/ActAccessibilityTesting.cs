using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

#nullable enable
namespace Amdocs.Ginger.CoreNET.ActionsLib.UI.Web
{
    public class ActAccessibilityTesting : Act, IActPluginExecution
    {
        public override String ActionType
        {
            get
            {
                return "ActAccessibility: ";
            }
        }
        public override string ActionDescription { get { return "Accessibility Testing"; } }

        public override string ActionUserDescription { get { return "Accessibility Testing"; } }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }

        public override bool ValueConfigsNeeded { get { return false; } }

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Web);
                }
                return mPlatforms;
            }
        }

        public override string ActionEditPage { get { return "ActAccessibilityTestingEditPage"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Action to Test Accessibility Testing on Web");
            TBH.AddLineBreak();
            TBH.AddText("Accessibility testing action is using Axe Core tool");
            TBH.AddLineBreak();
            TBH.AddText("Accessibility testing is the practice of making your web and mobile apps usable to as many people as possible");
        }

        public PlatformAction GetAsPlatformAction()
        {
            PlatformAction platformAction = new PlatformAction(this);




            foreach (ActInputValue aiv in this.InputValues)
            {
                if (!platformAction.InputParams.ContainsKey(aiv.Param))
                {
                    platformAction.InputParams.Add(aiv.Param, aiv.ValueForDriver);
                }
            }


            return platformAction;
        }

        public string GetName()
        {
            return "AccessibilityAction";
        }

        public new static partial class Fields
        {
            public static string Target = "Target";
            public static string ValueUC = "ValueUC";
            public static string ElementLocateValue = "ElementLocateValue";
            public static string ElementLocateBy = "ElementLocateBy";
            public static string PomGUID = "PomGUID";
            public static string Standard = "Standard";
            public static string ElementType = "ElementType";
            public static string ValueToSelect = "ValueToSelect";
        }

        public enum eTarget
        {
            [EnumValueDescription("Page")]
            Page,
            [EnumValueDescription("Element")]
            Element,
        }

        public override eLocateBy LocateBy
        {
            get
            {
                return GetOrCreateInputParam<eLocateBy>(Act.Fields.LocateBy, eLocateBy.NA);
            }
            set
            {
                AddOrUpdateInputParamValue(Act.Fields.LocateBy, value.ToString());
                OnPropertyChanged(Act.Fields.LocateBy);
                OnPropertyChanged(Act.Fields.Details);
            }
        }

        public override string LocateValue
        {
            get
            {
                return GetOrCreateInputParam(Act.Fields.LocateValue).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(Act.Fields.LocateValue, value);
                OnPropertyChanged(Act.Fields.LocateValue);
                OnPropertyChanged(Act.Fields.Details);
            }
        }

        public enum eTags
        {
            [EnumValueDescription("wcag2a")]
            wcag2a,
            [EnumValueDescription("wcag2aa")]
            wcag2aa,
            [EnumValueDescription("wcag21a")]
            wcag21a,
            [EnumValueDescription("wcag21aa")]
            wcag21aa,
        }

        public eTags Standard
        {
            get
            {
                return GetOrCreateInputParam<eTags>(Fields.Standard, eTags.wcag21a);
            }
            set
            {
                AddOrUpdateInputParamValue(Fields.Standard, value.ToString());

                OnPropertyChanged(nameof(eTags));
            }
        }

        public eLocateBy ElementLocateBy
        {
            get { return GetOrCreateInputParam<eLocateBy>(Fields.ElementLocateBy, eLocateBy.NA); }
            set
            {
                GetOrCreateInputParam(Fields.ElementLocateBy).Value = value.ToString();

                OnPropertyChanged(nameof(ElementLocateBy));

            }
        }
        public string ElementLocateValue
        {
            get
            {
                return GetOrCreateInputParam(Fields.ElementLocateValue).Value;
            }
            set
            {
                GetOrCreateInputParam(Fields.ElementLocateValue).Value = value;
                OnPropertyChanged(nameof(ElementLocateValue));
            }
        }

        public string ElementLocateValueForDriver
        {
            get
            {
                return this.GetInputParamCalculatedValue(Fields.ElementLocateValue);
            }
        }

        public eElementType ElementType
        {
            get { return GetOrCreateInputParam<eElementType>(Fields.ElementType, eElementType.Unknown); }
            set
            {
                GetOrCreateInputParam(Fields.ElementType).Value = value.ToString();

                OnPropertyChanged(nameof(ElementType));

            }
        }
    }
}
