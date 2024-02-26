using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Selenium;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Ginger.Configurations;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
            public static string Analyzer = "Analyzer";
        }

        public enum eTarget
        {
            [EnumValueDescription("Page")]
            Page,
            [EnumValueDescription("Element")]
            Element,
        }

        public enum eAnalyzer
        {
            [EnumValueDescription("By Standard")]
            ByStandard,
            [EnumValueDescription("By Severity")]
            BySeverity,
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
            [EnumValueDescription("WCAG 2.0 Level A")]
            wcag2a,
            [EnumValueDescription("WCAG 2.0 Level AA")]
            wcag2aa,
            [EnumValueDescription("WCAG 2.1 Level A")]
            wcag21a,
            [EnumValueDescription("WCAG 2.1 Level AA")]
            wcag21aa,
            [EnumValueDescription("WCAG 2.2 Level A")]
            wcag22a,
            [EnumValueDescription("WCAG 2.2 Level AA")]
            wcag22aa,
            [EnumValueDescription("Best Practice")]
            bestpractice,
        }

        public enum eSeverity
        {
            [EnumValueDescription("Serious")]
            Serious,
            [EnumValueDescription("Minor")]
            Minor,
            [EnumValueDescription("Critical")]
            Critical,
            [EnumValueDescription("Moderate")]
            Moderate,
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

        private ObservableList<OperationValues> mOperationValueList;
        [IsSerializedForLocalRepository]
        public ObservableList<OperationValues> OperationValueList
        {
            get
            {
                return mOperationValueList;
            }
            set
            {
                if (value != mOperationValueList)
                {
                    mOperationValueList = value;
                    OnPropertyChanged(nameof(OperationValueList));               
                }
            }
        }


        private ObservableList<OperationValues> mSeverityOperationValueList;
        [IsSerializedForLocalRepository]
        public ObservableList<OperationValues> SeverityOperationValueList
        {
            get
            {
                return mSeverityOperationValueList;
            }
            set
            {
                if (value != mSeverityOperationValueList)
                {
                    mSeverityOperationValueList = value;
                    OnPropertyChanged(nameof(SeverityOperationValueList));
                }
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
        public ObservableList<AccessibilityRuleData> RulesItemsdata;



        public ObservableList<AccessibilityRuleData> GetRuleList()
        {
            AccessibilityConfiguration accessibilityConfiguration = new AccessibilityConfiguration();
            ObservableList<AccessibilityRuleData> ruleDatalist = new ObservableList<AccessibilityRuleData>();
            try
            {
                ruleDatalist = accessibilityConfiguration.GetAccessibilityRules();
            }
            catch (Exception ex)
            {

            }
            return ruleDatalist;
        }

        private Dictionary<string, object> _items;

        public Dictionary<string, object> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;

            }
        }
        private Dictionary<string, object> _SeverityItems;

        public Dictionary<string, object> SeverityItems
        {
            get
            {
                return _SeverityItems;
            }
            set
            {
                _SeverityItems = value;

            }
        }

        public void AnalyserAccessibility(IWebDriver Driver,IWebElement element)
        {
            AxeBuilder axeBuilder = null;
            try
            {
                axeBuilder = CreateAxeBuilder(Driver);
                AxeResult axeResult = null;

                if ((GetInputParamValue(ActAccessibilityTesting.Fields.Target) == ActAccessibilityTesting.eTarget.Element.ToString()))
                {
                    axeResult = axeBuilder.Analyze(element);
                }
                else
                {
                    axeResult = axeBuilder.Analyze();
                }

                SetAxeResultToAction(axeResult);
            }
            catch (Exception ex)
            {
                Error = "Error: during accessibility testing:" + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Error: during accessibility testing", ex);
                return;
            }
        }

        public AxeBuilder CreateAxeBuilder(IWebDriver Driver)
        {
            AxeBuilder axeBuilder = null;
            axeBuilder = new AxeBuilder(Driver)
            .WithOptions(new AxeRunOptions()
            {
                XPath = true
            });
            List<string> sevritylist = SeverityOperationValueList.Select(x => x.Value.ToLower()).ToList();
            if (GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == ActAccessibilityTesting.eAnalyzer.ByStandard.ToString())
            {
                if (OperationValueList != null &&OperationValueList.Any())
                {
                    string[] Tag_array = OperationValueList.Select(i => i.Value.ToString()).ToArray();
                    axeBuilder.WithTags(Tag_array);
                }
                string[] ExcludeRules = RulesItemsdata.Where(x => sevritylist.Contains(x.Impact.ToLower())).Select(i => i.RuleID.ToString()).ToArray();
                if (ExcludeRules != null && ExcludeRules.Any())
                {
                    axeBuilder.DisableRules(ExcludeRules);
                }
            }
            else if (GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == ActAccessibilityTesting.eAnalyzer.BySeverity.ToString())
            {
                string[] IncludeRules = RulesItemsdata.Where(x => sevritylist.Contains(x.Impact.ToLower())).Select(i => i.RuleID.ToString()).ToArray();
                if (IncludeRules != null && IncludeRules.Any())
                {
                    axeBuilder.WithRules(IncludeRules);
                }
            }
            return axeBuilder;
        }

        public void SetAxeResultToAction(AxeResult axeResult)
        {
            bool hasAnyViolations = axeResult.Violations.Any();
            bool ActionResult = false;

            if (GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == ActAccessibilityTesting.eAnalyzer.ByStandard.ToString() && SeverityOperationValueList != null && SeverityOperationValueList.Any())
            {
                List<string> sevritylist = SeverityOperationValueList.Select(x => x.Value.ToLower()).ToList();
                List<string> Violationsevrity = axeResult.Violations.Any() ? axeResult.Violations.Select(x => x.Impact.ToLower()).ToList() : new List<string>();
                foreach (string severity in sevritylist)
                {
                    ActionResult = !Violationsevrity.Any(y => y.Equals(severity));
                }
            }
            else if (GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == ActAccessibilityTesting.eAnalyzer.BySeverity.ToString() && SeverityOperationValueList != null && SeverityOperationValueList.Any())
            {
                List<string> sevritylist = SeverityOperationValueList.Select(x => x.Value.ToLower()).ToList();
                List<string> Violationsevrity = axeResult.Violations.Any() ? axeResult.Violations.Select(x => x.Impact.ToLower()).ToList() : new List<string>();
                foreach (string severity in sevritylist)
                {
                    ActionResult = Violationsevrity.Any(y => y.Equals(severity));
                }
            }
            var jsonresponse = JsonConvert.SerializeObject(axeResult, Newtonsoft.Json.Formatting.Indented);
            RawResponseValues = jsonresponse;
            AddOrUpdateReturnParamActual(ParamName: "Raw Response", ActualValue: jsonresponse);
            if (hasAnyViolations)
            {
                if (ActionResult)
                {
                    Status = eRunStatus.Failed;
                }
                else
                {
                    Status = eRunStatus.Passed;
                }
                Error = $"Accessibility testing resulted in violations.";
                AddOrUpdateReturnParamActual(ParamName: "ViolationCount", ActualValue: axeResult.Violations.Length.ToString());
                AddOrUpdateReturnParamActual(ParamName: "ViolationList", ActualValue: String.Join(",", axeResult.Violations.Select(x => x.Id)));
                int violatedNodeIndex = 0;
                foreach (AxeResultItem violation in axeResult.Violations)
                {
                    foreach (AxeResultNode node in violation.Nodes)
                    {
                        violatedNodeIndex++;
                        AddOrUpdateReturnParamActualWithPath(ParamName: "ViolationId", ActualValue: violation.Id, violatedNodeIndex.ToString());
                        if (node.XPath != null)
                        {
                            AddOrUpdateReturnParamActualWithPath(ParamName: "NodeXPath", ActualValue: node.XPath.ToString(), violatedNodeIndex.ToString());
                        }

                        AddOrUpdateReturnParamActualWithPath(ParamName: "ViolationHelp", ActualValue: violation.Help, violatedNodeIndex.ToString());
                        AddOrUpdateReturnParamActualWithPath(ParamName: "ViolationSeverity", ActualValue: violation.Impact, violatedNodeIndex.ToString());
                    }
                }
            }
            else
            {
                Status = eRunStatus.Passed;
            }
        }
    }
}
