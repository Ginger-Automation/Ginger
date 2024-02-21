using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using Ginger.Configurations;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Newtonsoft.Json;
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
            [EnumValueDescription("ByTag")]
            ByTag,
            [EnumValueDescription("ByRule")]
            ByRule,
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
                    if (ExcludeRuleList != null)
                    {
                        ObservableList<AccessibilityRuleData>  RuleListasperTag = GetRulesAsPerTags();
                        ExcludeRuleList.Clear();
                        foreach (AccessibilityRuleData ruleData in RuleListasperTag)
                        {
                            ExcludeRuleList.Add(ruleData);
                        }
                    }                
                }
            }
        }

        private ObservableList<AccessibilityRuleData> mExcludeRuleList;

        [IsSerializedForLocalRepository]
        public ObservableList<AccessibilityRuleData> ExcludeRuleList
        {
            get
            {
                return mExcludeRuleList;
            }
            set
            {
                if (value != mExcludeRuleList)
                {
                    mExcludeRuleList = value;
                    OnPropertyChanged(nameof(ExcludeRuleList));
                }
            }
        }

        private ObservableList<AccessibilityRuleData> mIncludeRuleList;

        [IsSerializedForLocalRepository]
        public ObservableList<AccessibilityRuleData> IncludeRuleList
        {
            get
            {
                return mIncludeRuleList;
            }
            set
            {
                if (value != mIncludeRuleList)
                {
                    mIncludeRuleList = value;
                    OnPropertyChanged(nameof(IncludeRuleList));
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

        public ObservableList<AccessibilityRuleData> GetRulesAsPerTags()
        {
            List<AccessibilityRuleData> Exculedrules = new  List<AccessibilityRuleData>();
            if(OperationValueList != null && OperationValueList.Count > 0 && RulesItemsdata != null)
            {
                Exculedrules = RulesItemsdata.Where(x => OperationValueList.Select(y => y.Value).ToList().Contains(x.Tags)).ToList(); 
            }
            else
            {
                RulesItemsdata = RulesItemsdata == null ? GetRuleList() : RulesItemsdata;
            }
            foreach(AccessibilityRuleData ruleData in Exculedrules)
            {
                ruleData.Active = ExcludeRuleList!= null && ExcludeRuleList.Any(x=>x.RuleID == ruleData.RuleID) ? ExcludeRuleList.FirstOrDefault(x => x.RuleID == ruleData.RuleID).Active : false;
            }
            return Exculedrules.Any() ? new ObservableList<AccessibilityRuleData>(Exculedrules) : RulesItemsdata;
        }

        public ObservableList<AccessibilityRuleData> GetAllRules()
        {
            ObservableList<AccessibilityRuleData> Inculedrules = new ObservableList<AccessibilityRuleData>();

            Inculedrules = RulesItemsdata == null ? GetRuleList() : RulesItemsdata;
            foreach (AccessibilityRuleData ruleData in Inculedrules)
            {
                ruleData.Active = IncludeRuleList != null && IncludeRuleList.Any(x => x.RuleID == ruleData.RuleID) ? IncludeRuleList.FirstOrDefault(x => x.RuleID == ruleData.RuleID).Active : false;
            }
            return new ObservableList<AccessibilityRuleData>(Inculedrules);
        }


        public ObservableList<AccessibilityRuleData> GetRuleList()
        {
            AccessibilityConfiguration accessibilityConfiguration = new AccessibilityConfiguration();
            
            ObservableList<AccessibilityRuleData> ruleDatalist = accessibilityConfiguration.GetAccessibilityRules();
            try
            {
                RulesItemsdata = ruleDatalist;
                foreach (AccessibilityRuleData item in ruleDatalist)
                {
                    item.PropertyChanged += RuleData_PropertyChanged;
                }
            }
            catch (Exception ex)
            {

            }
            return ruleDatalist;
        }

        private void RuleData_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ExcludeRuleList));
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
    }
}
