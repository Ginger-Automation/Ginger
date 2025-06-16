#region License
/*
Copyright © 2014-2025 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile.Appium;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Selenium;
using Ginger.Configurations;
using GingerCore.Actions;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using HtmlAgilityPack;
using Microsoft.VisualStudio.Services.Common;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

#nullable enable
namespace Amdocs.Ginger.CoreNET.ActionsLib.UI.Web
{
    public class ActAccessibilityTesting : Act, IActPluginExecution
    {
        public override String ActionType
        {
            get
            {
                return "Accessibility Testing";
            }
        }
        public override string ActionDescription { get { return "Accessibility Testing"; } }

        public override string ActionUserDescription { get { return "Accessibility Testing"; } }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }

        public override bool ValueConfigsNeeded { get { return false; } }

        // Public property to set the type of rules to fetch
        public string CurrentRuleType { get; set; } = "Web"; // Default to "Web"

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Mobile);
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
            return "Accessibility Testing";
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
            [EnumValueDescription("All")]
            All,
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

        public enum eMobileAccessibilityStandards
        {
            [EnumValueDescription("All Applicable Standards")]
            All,

            // --- WCAG Principles/Levels ---
            [EnumValueDescription("WCAG 2.1 Level A")]
            WCAG21A,
            [EnumValueDescription("WCAG 2.1 Level AA")]
            WCAG21AA,
            [EnumValueDescription("WCAG 2.1 Level AAA")]
            WCAG21AAA,

            // --- European Standard EN 301 549 ---
            [EnumValueDescription("EN 301 549 (General Standard)")]
            EN_301_549,
            [EnumValueDescription("EN 301 549 - 9.4.1.2 (Name, Role, Value)")]
            EN_9_4_1_2,
            [EnumValueDescription("EN 301 549 - 9.4.1.3 (Info and Relationships)")]
            EN_9_4_1_3,

            [EnumValueDescription("Best Practice Recommendation")]
            BestPractice
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

        private ObservableList<OperationValues> mStandardList;
        [IsSerializedForLocalRepository]
        public ObservableList<OperationValues> StandardList
        {
            get
            {
                return mStandardList;
            }
            set
            {
                if (value != mStandardList)
                {
                    mStandardList = value;
                    OnPropertyChanged(nameof(StandardList));
                }
            }
        }


        private ObservableList<OperationValues> mSeverityList;
        [IsSerializedForLocalRepository]
        public ObservableList<OperationValues> SeverityList
        {
            get
            {
                return mSeverityList;
            }
            set
            {
                if (value != mSeverityList)
                {
                    mSeverityList = value;
                    OnPropertyChanged(nameof(SeverityList));
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
        public ObservableList<AccessibilityRuleData> RulesItemsdata
        {
            get
            {
                return GetRuleList();
            }
        }



        public ObservableList<AccessibilityRuleData> GetRuleList()
        {
            AccessibilityRuleData AccessibilityRuleDataObjet = new AccessibilityRuleData();
            AccessibilityConfiguration mAccessibilityConfiguration;
            if (WorkSpace.Instance.SolutionRepository != null)
            {
                if (!WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<AccessibilityConfiguration>().Any())
                {
                    mAccessibilityConfiguration = new();
                }
                else
                {
                    mAccessibilityConfiguration = WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<AccessibilityConfiguration>();
                }
            }
            else
            {
                mAccessibilityConfiguration = new();
            }
            mAccessibilityConfiguration.ExcludedRules = mAccessibilityConfiguration.ExcludedRules != null ? mAccessibilityConfiguration.ExcludedRules : [];
            ObservableList<AccessibilityRuleData> ruleDatalist;
            try
            {
                string AccessbiltyString = GetAccessiblityrules(this.CurrentRuleType);
                ruleDatalist = AccessibilityRuleDataObjet.GetAccessibilityRules(AccessbiltyString);
                foreach (AccessibilityRuleData ruleData in ruleDatalist)
                {
                    if (mAccessibilityConfiguration.ExcludedRules != null && mAccessibilityConfiguration.ExcludedRules.Select(x => x.RuleID).Contains(ruleData.RuleID))
                    {
                        ruleData.Active = false;
                    }
                    else
                    {
                        ruleData.Active = true;
                    }
                }
                return ruleDatalist;
            }
            catch (Exception ex)
            {
                Error = "Error: during accessibility testing:" + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, $"Error: during accessibility testing in GetRuleList. Configuration: {mAccessibilityConfiguration?.Name}", ex);
            }
            return [];
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

        [Flags]
        public enum ReportTypes
        {
            Violations = 1,
            Incomplete = 2,
            Inapplicable = 4,
            Passes = 8,
            All = 15
        }

        public override eImageType Image { get { return eImageType.Accessibility; } }

        public eTarget GetAccessibilityTarget()
        {
            if (Enum.TryParse(GetInputParamValue(Fields.Target), out eTarget target))
            {
                return target;
            }
            return eTarget.Page;
        }

        // This property becomes the single source of filtered rules
        public ObservableList<AccessibilityRuleData> ActiveRulesForAnalysis
        {
            get
            {
                return GetFilteredRuleList();
            }
        }

        private ObservableList<AccessibilityRuleData> GetFilteredRuleList()
        {
            ObservableList<AccessibilityRuleData> allRules = GetRuleList();


            AccessibilityConfiguration mAccessibilityConfiguration;
            if (WorkSpace.Instance.SolutionRepository != null)
            {
                mAccessibilityConfiguration = WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<AccessibilityConfiguration>() ?? new AccessibilityConfiguration();
            }
            else
            {
                mAccessibilityConfiguration = new AccessibilityConfiguration();
            }

            // Start with the rules excluded by the user's configuration ('Active' field being false)
            HashSet<string> finalExcludedRuleIds = new HashSet<string>(
                allRules.Where(x => !x.Active).Select(x => x.RuleID),
                StringComparer.OrdinalIgnoreCase
            );

            // Apply filtering based on Analyzer mode
            if (GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(eAnalyzer.ByStandard))
            {
                if (StandardList == null || !StandardList.Any())
                {

                    Reporter.ToLog(eLogLevel.ERROR, "Error: 'ByStandard' analyzer selected, but no standards are provided.");

                    return new ObservableList<AccessibilityRuleData>();
                }


                HashSet<string> selectedStandardTags = new HashSet<string>(
                    StandardList.Select(item => item.Value.ToString().Equals("bestpractice", StringComparison.OrdinalIgnoreCase) ? "best-practice" : item.Value.ToString()),
                    StringComparer.OrdinalIgnoreCase
                );

                // Assuming Tag is a string or list of strings that can be matched.
                allRules = new ObservableList<AccessibilityRuleData>(
     allRules.Where(r =>
     {
         if (string.IsNullOrEmpty(r.Tags)) // Handle cases where Tags might be null or empty
         {
             return false;
         }

         // Split the comma-separated tags string into a collection of individual tags
         // .Select(s => s.Trim()) to remove leading/trailing whitespace from each tag
         // .ToList() to make it a List<string> if needed, or keep as IEnumerable<string>
         IEnumerable<string> ruleIndividualTags = r.Tags.Split(',')
                                                      .Select(s => s.Trim())
                                                      .Where(s => !string.IsNullOrWhiteSpace(s)); // Filter out empty strings from splitting

         // Check if any of the individual tags for the rule are in the selectedStandardTags
         return ruleIndividualTags.Any(ruleTag => selectedStandardTags.Contains(ruleTag));
     })
 );
                if (SeverityList != null && SeverityList.Any())
                {
                    List<string> selectedSeverities = SeverityList.Select(x => x.Value.ToLower()).ToList();


                    // Rules to exclude based on severity: if a rule's impact is NOT in the selected severities
                    var severityExcludedRuleIds = allRules
                        .Where(r => !selectedSeverities.Contains(r.Impact.ToLower())) // Exclude if its Impact is NOT among the selected
                        .Select(r => r.RuleID);

                    foreach (var ruleId in severityExcludedRuleIds)
                    {
                        finalExcludedRuleIds.Add(ruleId);
                    }
                }
            }
            else if (GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(eAnalyzer.BySeverity))
            {
                if (SeverityList == null || !SeverityList.Any())
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error: 'BySeverity' analyzer selected, but no severities are provided.");
                    // Optionally, set currentAct.Status = Failed and currentAct.Error
                    return new ObservableList<AccessibilityRuleData>();
                }

                List<string> selectedSeverities = SeverityList.Select(x => x.Value.ToLower()).ToList();

                // Filter rules: only include rules whose Impact matches a selected severity
                allRules = new ObservableList<AccessibilityRuleData>(
                    allRules.Where(r => selectedSeverities.Contains(r.Impact.ToLower()))
                );
            }

            // Final step: Apply the combined exclusions to the initially loaded rules
            // Create the final list of rules that are truly active for the analyzer
            ObservableList<AccessibilityRuleData> finalActiveRules = new ObservableList<AccessibilityRuleData>();
            foreach (AccessibilityRuleData ruleData in allRules)
            {
                if (!finalExcludedRuleIds.Contains(ruleData.RuleID))
                {
                    ruleData.Active = true; // Mark as active for consistency, though not strictly needed here
                    finalActiveRules.Add(ruleData);
                }
                else
                {
                    ruleData.Active = false; // Explicitly mark as inactive if excluded
                }
            }

            return finalActiveRules;
        }

        private static string GetAccessiblityrules(string platType)
        {
            try
            {
                if (platType.Equals("Mobile", StringComparison.OrdinalIgnoreCase))
                {
                    return EmbeddedResourceProvider.ReadEmbeddedFile("MobileAccessiblityRules.json");
                }
                else if (platType.Equals("Web", StringComparison.OrdinalIgnoreCase))
                {
                    return EmbeddedResourceProvider.ReadEmbeddedFile("AccessiblityRules.json");
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error reading accessibility rules from embedded file", ex);
                return string.Empty;
            }
        }

        public void CreateAxeHtmlReport(ISearchContext context, AxeResult results, string destination, ReportTypes requestedResults)
        {
            // Get the unwrapped element if we are using a wrapped element
            context = context is IWrapsElement ? (context as IWrapsElement).WrappedElement : context;

            var violationCount = GetCount(results.Violations);
            var incompleteCount = GetCount(results.Incomplete);
            var passCount = GetCount(results.Passes);
            var inapplicableCount = GetCount(results.Inapplicable);

            var doc = new HtmlDocument();
            doc.CreateComment("<!DOCTYPE html>\r\n");

            var htmlStructure = HtmlNode.CreateNode("<html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Accessibility Check</title><style></style></head><body><content></content><script></script></body></html>");
            doc.DocumentNode.AppendChild(htmlStructure);

            doc.DocumentNode.SelectSingleNode("//style").InnerHtml = GetCss(context);

            var contentArea = doc.DocumentNode.SelectSingleNode("//content");

            var reportTitle = doc.CreateElement("h1");
            reportTitle.InnerHtml = "Accessibility Check";
            contentArea.AppendChild(reportTitle);

            var metaFlex = doc.CreateElement("div");
            metaFlex.SetAttributeValue("id", "metadata");
            contentArea.AppendChild(metaFlex);

            var contextGroup = doc.CreateElement("div");
            contextGroup.SetAttributeValue("id", "context");
            metaFlex.AppendChild(contextGroup);

            var contextHeader = doc.CreateElement("h3");
            contextHeader.InnerHtml = "Context:";
            contextGroup.AppendChild(contextHeader);

            var contextContent = doc.CreateElement("div");
            contextContent.SetAttributeValue("class", "emOne");
            contextContent.SetAttributeValue("id", "reportContext");
            contextContent.InnerHtml = GetContextContent(results);
            contextGroup.AppendChild(contextContent);

            var imgGroup = doc.CreateElement("div");
            imgGroup.SetAttributeValue("id", "image");
            metaFlex.AppendChild(imgGroup);

            var imageHeader = doc.CreateElement("h3");
            imageHeader.InnerHtml = "Image:";
            imgGroup.AppendChild(imageHeader);

            var imageContent = doc.CreateElement("img");
            imageContent.SetAttributeValue("class", "thumbnail");
            imageContent.SetAttributeValue("id", "screenshotThumbnail");
            imageContent.SetAttributeValue("alt", "A Screenshot of the page");
            imageContent.SetAttributeValue("width", "33%");
            imageContent.SetAttributeValue("height", "auto");
            imgGroup.AppendChild(imageContent);

            var countsGroup = doc.CreateElement("div");
            countsGroup.SetAttributeValue("id", "counts");
            metaFlex.AppendChild(countsGroup);

            var countsHeader = doc.CreateElement("h3");
            countsHeader.InnerHtml = "Counts:";
            countsGroup.AppendChild(countsHeader);

            var countsContent = doc.CreateElement("div");
            countsContent.SetAttributeValue("class", "emOne");
            var countsString = GetCountContent(violationCount, incompleteCount, passCount, inapplicableCount, requestedResults);

            countsContent.InnerHtml = countsString.ToString();
            countsGroup.AppendChild(countsContent);

            var resultsFlex = doc.CreateElement("div");
            resultsFlex.SetAttributeValue("id", "results");
            contentArea.AppendChild(resultsFlex);

            if (violationCount > 0 && requestedResults.HasFlag(ReportTypes.Violations))
            {
                GetReadableAxeResults(results.Violations, ResultType.Violations, doc, resultsFlex);
            }

            if (incompleteCount > 0 && requestedResults.HasFlag(ReportTypes.Incomplete))
            {
                GetReadableAxeResults(results.Incomplete, ResultType.Incomplete, doc, resultsFlex);
            }

            if (passCount > 0 && requestedResults.HasFlag(ReportTypes.Passes))
            {
                GetReadableAxeResults(results.Passes, ResultType.Passes, doc, resultsFlex);
            }

            if (inapplicableCount > 0 && requestedResults.HasFlag(ReportTypes.Inapplicable))
            {
                GetReadableAxeResults(results.Inapplicable, ResultType.Inapplicable, doc, resultsFlex);
            }


            var modal = doc.CreateElement("div");
            modal.SetAttributeValue("id", "modal");
            contentArea.AppendChild(modal);

            var modalClose = doc.CreateElement("div");
            modalClose.InnerHtml = "X";
            modalClose.SetAttributeValue("id", "modalclose");
            modal.AppendChild(modalClose);

            var modalImage = doc.CreateElement("img");
            modalImage.SetAttributeValue("id", "modalimage");
            modal.AppendChild(modalImage);


            doc.DocumentNode.SelectSingleNode("//script").InnerHtml = EmbeddedResourceProvider.ReadEmbeddedFile("htmlReporterElements.js");

            doc.Save(destination, Encoding.UTF8);
        }

        private static void GetReadableAxeResults(AxeResultItem[] results, ResultType type, HtmlDocument doc, HtmlNode body)
        {
            var resultWrapper = doc.CreateElement("div");
            resultWrapper.SetAttributeValue("class", "resultWrapper");
            body.AppendChild(resultWrapper);

            var sectionButton = doc.CreateElement("button");
            sectionButton.SetAttributeValue("class", "sectionbutton active");
            resultWrapper.AppendChild(sectionButton);

            var sectionButtonHeader = doc.CreateElement("h2");
            sectionButtonHeader.SetAttributeValue("class", "buttonInfoText");
            sectionButtonHeader.InnerHtml = $"{type}: {GetCount(results)}";
            sectionButton.AppendChild(sectionButtonHeader);

            var sectionButtonExpando = doc.CreateElement("h2");
            sectionButtonExpando.SetAttributeValue("class", "buttonExpandoText");
            sectionButtonExpando.InnerHtml = "-";
            sectionButton.AppendChild(sectionButtonExpando);

            var section = doc.CreateElement("div");
            section.SetAttributeValue("class", "majorSection");
            section.SetAttributeValue("id", type + "Section");
            resultWrapper.AppendChild(section);

            var loops = 1;

            foreach (var element in results)
            {
                var childEl = doc.CreateElement("div");
                childEl.SetAttributeValue("class", "findings");
                childEl.InnerHtml = $@"{loops++}: {HttpUtility.HtmlEncode(element.Help)}";
                section.AppendChild(childEl);

                StringBuilder content = new StringBuilder();
                content.AppendLine($"Description: {HttpUtility.HtmlEncode(element.Description)}<br>");
                content.AppendLine($"Help: {HttpUtility.HtmlEncode(element.Help)}<br>");
                content.AppendLine($"Help URL: <a href=\"{element.HelpUrl}\">{element.HelpUrl}</a><br>");

                if (!string.IsNullOrEmpty(element.Impact))
                {
                    content.AppendLine($"Impact: {HttpUtility.HtmlEncode(element.Impact)}<br>");
                }

                content.AppendLine($"Tags: {HttpUtility.HtmlEncode(string.Join(", ", element.Tags))}<br>");

                if (element.Nodes.Length > 0)
                {
                    content.AppendLine($"Element(s):");
                }

                var childEl2 = doc.CreateElement("div");
                childEl2.SetAttributeValue("class", "emTwo");
                childEl2.InnerHtml = content.ToString();
                childEl.AppendChild(childEl2);

                foreach (var item in element.Nodes)
                {
                    var elementNodes = doc.CreateElement("div");
                    elementNodes.SetAttributeValue("class", "htmlTable");
                    childEl.AppendChild(elementNodes);

                    var htmlAndSelectorWrapper = doc.CreateElement("div");
                    htmlAndSelectorWrapper.SetAttributeValue("class", "emThree");
                    elementNodes.AppendChild(htmlAndSelectorWrapper);

                    HtmlNode htmlAndSelector = doc.CreateTextNode("Html:");
                    htmlAndSelectorWrapper.AppendChild(htmlAndSelector);

                    htmlAndSelector = doc.CreateElement("p");
                    htmlAndSelector.SetAttributeValue("class", "wrapOne");
                    htmlAndSelector.InnerHtml = $"{HttpUtility.HtmlEncode(item.Html)}";
                    htmlAndSelectorWrapper.AppendChild(htmlAndSelector);

                    htmlAndSelector = doc.CreateTextNode("Selector:");
                    htmlAndSelectorWrapper.AppendChild(htmlAndSelector);

                    content = new StringBuilder();
                    htmlAndSelector = doc.CreateElement("p");
                    htmlAndSelector.SetAttributeValue("class", "wrapTwo");

                    htmlAndSelector.InnerHtml = content.ToString();
                    htmlAndSelectorWrapper.AppendChild(htmlAndSelector);

                    AddFixes(item, type, doc, htmlAndSelectorWrapper);
                }
            }
        }

        private static void AddFixes(AxeResultNode resultsNode, ResultType type, HtmlDocument doc, HtmlNode htmlAndSelectorWrapper)
        {
            HtmlNode htmlAndSelector;

            var anyCheckResults = resultsNode.Any;
            var allCheckResults = resultsNode.All;
            var noneCheckResults = resultsNode.None;

            int checkResultsCount = anyCheckResults.Length + allCheckResults.Length + noneCheckResults.Length;

            // Add fixes if this is for violations
            if (ResultType.Violations.Equals(type) && checkResultsCount > 0)
            {
                htmlAndSelector = doc.CreateTextNode("To solve:");
                htmlAndSelectorWrapper.AppendChild(htmlAndSelector);

                htmlAndSelector = doc.CreateElement("p");
                htmlAndSelector.SetAttributeValue("class", "wrapTwo");
                htmlAndSelectorWrapper.AppendChild(htmlAndSelector);

                if (allCheckResults.Length > 0 || noneCheckResults.Length > 0)
                {
                    FixAllIssues(doc, htmlAndSelectorWrapper, allCheckResults, noneCheckResults);
                }

                if (anyCheckResults.Length > 0)
                {
                    FixAnyIssues(doc, htmlAndSelectorWrapper, anyCheckResults);
                }
            }
        }

        private static void FixAllIssues(HtmlDocument doc, HtmlNode htmlAndSelectorWrapper, AxeResultCheck[] allCheckResults, AxeResultCheck[] noneCheckResults)
        {
            HtmlNode htmlAndSelector;

            htmlAndSelector = doc.CreateElement("p");
            htmlAndSelector.SetAttributeValue("class", "wrapOne");
            StringBuilder content = new StringBuilder();

            content.AppendLine("Fix all of the following issues:");
            content.AppendLine("<ul>");

            foreach (var checkResult in allCheckResults)
            {
                content.AppendLine($"<li>{HttpUtility.HtmlEncode(checkResult.Impact.ToUpper())}: {HttpUtility.HtmlEncode(checkResult.Message)}</li>");
            }

            foreach (var checkResult in noneCheckResults)
            {
                content.AppendLine($"<li>{HttpUtility.HtmlEncode(checkResult.Impact.ToUpper())}: {HttpUtility.HtmlEncode(checkResult.Message)}</li>");
            }

            content.AppendLine("</ul>");
            htmlAndSelector.InnerHtml = content.ToString();
            htmlAndSelectorWrapper.AppendChild(htmlAndSelector);
        }

        private static void FixAnyIssues(HtmlDocument doc, HtmlNode htmlAndSelectorWrapper, AxeResultCheck[] anyCheckResults)
        {
            StringBuilder content = new StringBuilder();

            HtmlNode htmlAndSelector = doc.CreateElement("p");
            htmlAndSelector.SetAttributeValue("class", "wrapOne");
            content.AppendLine("Fix at least one of the following issues:");
            content.AppendLine("<ul>");

            foreach (var checkResult in anyCheckResults)
            {
                content.AppendLine($"<li>{HttpUtility.HtmlEncode(checkResult.Impact.ToUpper())}: {HttpUtility.HtmlEncode(checkResult.Message)}</li>");
            }

            content.AppendLine("</ul>");
            htmlAndSelector.InnerHtml = content.ToString();
            htmlAndSelectorWrapper.AppendChild(htmlAndSelector);
        }

        private static string GetCss(ISearchContext context)
        {
            return EmbeddedResourceProvider.ReadEmbeddedFile("htmlReporter.css").Replace("url('", $"url('{GetDataImageString(context)}");
        }

        private static string GetDataImageString(ISearchContext context)
        {
            ITakesScreenshot newScreen = (ITakesScreenshot)context;
            return $"data:image/png;base64,{Convert.ToBase64String(newScreen.GetScreenshot().AsByteArray)}";
        }

        private static string GetContextContent(AxeResult results)
        {
            var contextContent = new StringBuilder()
                .AppendLine($"Url: {results.Url}<br>")
                .AppendLine($"Orientation: {results.TestEnvironment.OrientationType}<br>")
                .AppendLine($"Size: {results.TestEnvironment.WindowWidth} x {results.TestEnvironment.WindowHeight}<br>")
                .AppendLine($"Time: {results.Timestamp}<br>")
                .AppendLine($"User agent: {results.TestEnvironment.UserAgent}<br>")
                .ToString();
            return contextContent;
        }

        private static string GetCountContent(int violationCount, int incompleteCount, int passCount, int inapplicableCount, ReportTypes requestedResults)
        {
            StringBuilder countString = new StringBuilder();

            if (requestedResults.HasFlag(ReportTypes.Violations))
            {
                countString.AppendLine($" Violation: {violationCount}<br>");
            }

            if (requestedResults.HasFlag(ReportTypes.Incomplete))
            {
                countString.AppendLine($" Incomplete: {incompleteCount}<br>");
            }

            if (requestedResults.HasFlag(ReportTypes.Passes))
            {
                countString.AppendLine($" Pass: {passCount}<br>");
            }

            if (requestedResults.HasFlag(ReportTypes.Inapplicable))
            {
                countString.AppendLine($" Inapplicable: {inapplicableCount}");
            }

            return countString.ToString();
        }

        private static int GetCount(AxeResultItem[] results)
        {
            int count = 0;
            foreach (AxeResultItem item in results)
            {
                foreach (AxeResultNode node in item.Nodes)
                {
                    count++;
                }

                // Still add one if no targets are included
                if (item.Nodes.Length == 0)
                {
                    count++;
                }
            }
            return count;
        }
        //Analyzer for Mobile
        public void AnalyzerMobileAccessibility(IWebDriver driver, IWebElement elementXPath = null)
        {
            try
            {
                Artifacts = [];
                MobileAccessibilityAnalyzer axeBuilder = new MobileAccessibilityAnalyzer(driver, ActiveRulesForAnalysis);
                axeBuilder.AnalyzerMobileAccessibility(driver, elementXPath, currentAct: this, axeBuilder);
            }
            catch (Exception ex)
            {
                Error = "Error during mobile accessibility testing: " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Error during mobile accessibility testing", ex);
            }
        }


        //Analyzer for Web
        public void AnalyzerAccessibility(IWebDriver Driver, IWebElement element, ePlatformType platformType)
        {
            AxeBuilder axeBuilder = null;
            try
            {
                Artifacts = [];
                axeBuilder = CreateAxeBuilder(Driver);
                if (Status == eRunStatus.Failed && !string.IsNullOrEmpty(Error))
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error: {Error}");
                    return;
                }
                AxeResult axeResult = null;

                if ((GetInputParamValue(ActAccessibilityTesting.Fields.Target) == ActAccessibilityTesting.eTarget.Element.ToString()))
                {
                    axeResult = axeBuilder.Analyze(element);
                }
                else
                {
                    axeResult = axeBuilder.Analyze();
                }

                string path = String.Empty;

                if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder != null)
                {
                    string folderPath = Path.Combine(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder, @"AccessibilityReport");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    string DatetimeFormate = DateTime.Now.ToString("ddMMyyyy_HHmmssfff");
                    string reportname = $"{ItemName}_AccessibilityReport_{DatetimeFormate}.html";
                    path = $"{folderPath}{Path.DirectorySeparatorChar}{reportname}";
                    Act.AddArtifactToAction(Path.GetFileName(path), this, path);
                    CreateAxeHtmlReport(Driver, axeResult, path, ActAccessibilityTesting.ReportTypes.All);
                    AddOrUpdateReturnParamActual(ParamName: "Accessibility report", ActualValue: path);
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
            ObservableList<AccessibilityRuleData> RuleData = RulesItemsdata;
            AxeBuilder axeBuilder = null;
            List<string> sevritylist = null;
            axeBuilder = new AxeBuilder(Driver)
            .WithOptions(new AxeRunOptions()
            {
                XPath = true
            });

            //Active field is for configuration page rules which are Active needs to Exclude in Analysis
            string[] ExcludeRules = RuleData.Where(x => !x.Active).Select(i => i.RuleID.ToString()).ToArray();
            if (GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(ActAccessibilityTesting.eAnalyzer.ByStandard))
            {
                if (StandardList != null && StandardList.Any())
                {
                    string[] Tag_array = StandardList.Select(i => i.Value.ToString()).ToArray();
                    for (int i = 0; i < Tag_array.Length; i++)
                    {
                        if (Tag_array[i].Equals("bestpractice", StringComparison.OrdinalIgnoreCase))
                        {
                            Tag_array[i] = "best-practice";
                        }
                    }
                    axeBuilder.WithTags(Tag_array);
                }
                else if (StandardList == null || !StandardList.Any())
                {
                    Status = eRunStatus.Failed;
                    Error = "Standard list is empty or not set.";
                    return axeBuilder;
                }


                if (SeverityList != null && SeverityList.Any())
                {
                    sevritylist = SeverityList.Select(x => x.Value.ToLower()).ToList();

                    string[] SeverityExcludeRules = RuleData.Where(x => !ExcludeRules.Contains(x.RuleID) && sevritylist.Contains(x.Impact.ToLower())).Select(i => i.RuleID.ToString()).ToArray();
                    ExcludeRules = ExcludeRules.Concat(SeverityExcludeRules).ToArray();
                }

                if (ExcludeRules != null && ExcludeRules.Any())
                {
                    axeBuilder.DisableRules(ExcludeRules);
                }
            }
            else if (GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(ActAccessibilityTesting.eAnalyzer.BySeverity))
            {
                if (SeverityList != null && SeverityList.Any())
                {
                    sevritylist = SeverityList.Select(x => x.Value.ToLower()).ToList();

                    string[] SevarityIncludeRules = RuleData.Where(x => !ExcludeRules.Contains(x.RuleID) && sevritylist.Contains(x.Impact.ToLower())).Select(i => i.RuleID.ToString()).ToArray();
                    if (SevarityIncludeRules != null && SevarityIncludeRules.Any())
                    {
                        axeBuilder.WithRules(SevarityIncludeRules);
                    }
                }
                else if (SeverityList == null || !SeverityList.Any())
                {
                    Status = eRunStatus.Failed;
                    Error = "Severity list is empty or not set.";
                    return axeBuilder;
                }


            }
            return axeBuilder;
        }
        public void SetAxeResultToAction(AxeResult axeResult)
        {
            bool hasAnyViolations = axeResult.Violations.Any();
            bool ActionResult = true;
            IEnumerable<string> AcceptableSeverity = Enumerable.Empty<string>().ToList();
            IEnumerable<string> Violationseverity = Enumerable.Empty<string>().ToList();
            if (GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(ActAccessibilityTesting.eAnalyzer.ByStandard))
            {
                Violationseverity = axeResult.Violations.Any() ? axeResult.Violations.Select(x => x.Impact.ToLower()) : Enumerable.Empty<string>().ToList();
                if (SeverityList != null && SeverityList.Any())
                {
                    AcceptableSeverity = SeverityList.Select(x => x.Value.ToLower()).ToList();
                    ActionResult = Violationseverity.Intersect(AcceptableSeverity).Any();
                }
                else
                {
                    ActionResult = Violationseverity.Any();
                }
            }
            else if (GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(ActAccessibilityTesting.eAnalyzer.BySeverity))
            {
                Violationseverity = axeResult.Violations.Any() ? axeResult.Violations.Select(x => x.Impact.ToLower()) : Enumerable.Empty<string>().ToList();
                if (SeverityList != null && SeverityList.Any())
                {
                    List<string> sevritylist = SeverityList.Select(x => x.Value.ToLower()).ToList();
                    foreach (string severity in sevritylist)
                    {
                        ActionResult = Violationseverity.Any(y => y.Equals(severity));
                    }
                }
                else
                {
                    ActionResult = Violationseverity.Any();
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
                AddOrUpdateReturnParamActual(ParamName: "ViolationSeverity", ActualValue: string.Join(",", Violationseverity.Distinct()));
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

    public class AccessibilityIssue
    {
        public string RuleId { get; set; } // e.g., "WCAG_1_1_1", "Android_TouchTargetSize"
        public string Description { get; set; }
        public string ElementIdentifier { get; set; } // A unique way to identify the element (e.g., XPath, resource-id)
        public string Severity { get; set; } // e.g., "Critical", "Moderate", "Minor"
        public string SuggestedFix { get; set; } // Guidance on how to fix the issue
        public string RelatedWCAG { get; set; } // e.g., "WCAG 2.1.1 (A)"


        public override string ToString()
        {
            return $"[{Severity}] {RuleId}: {Description} | Element: {ElementIdentifier} | Suggested Fix: {SuggestedFix}";
        }
    }

    internal static class EmbeddedResourceProvider
    {
        public static string ReadEmbeddedFile(string fileName)
        {
            var assembly = typeof(EmbeddedResourceProvider).Assembly;
            var resourceStream = assembly.GetManifestResourceStream($"Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Selenium.AccessibilityTestResources.{fileName}");
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
