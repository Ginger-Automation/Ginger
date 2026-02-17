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
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using Amdocs.Ginger.CoreNET.Execution;
using Deque.AxeCore.Commons;
using Ginger.Configurations;
using GingerCore.Actions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.ActionHandlers
{
    internal sealed class ActAccessibilityTestingHandler
    {
        private readonly ActAccessibilityTesting _act;
        private readonly IBrowserTab _tab;
        private readonly IBrowserElementLocator? _browserElementLocator;

        internal ActAccessibilityTestingHandler(ActAccessibilityTesting act, IBrowserTab tab, IBrowserElementLocator? browserElementLocator)
        {
            _act = act;
            _tab = tab;
            _browserElementLocator = browserElementLocator;
        }

        internal async Task HandleAsync()
        {
            try
            {
                AxeResult? result;
                AxeRunOptions options = CreateAxeRunOptions();

                if (_act.GetAccessibilityTarget() == ActAccessibilityTesting.eTarget.Page)
                {
                    result = await TestPageAccessibilityAsync(options);
                }
                else
                {
                    result = await TestElementAccessibilityAsync(options);
                }

                await ProcessResultAsync(result);
            }
            catch (Exception ex)
            {
                _act.Error = ex.Message;
            }
        }

        private AxeRunOptions CreateAxeRunOptions()
        {
            AxeRunOptions axeRunOptions = new()
            {
                XPath = true,
            };
            ObservableList<AccessibilityRuleData> RuleData = _act.RulesItemsdata;
            string[] ExcludeRules = RuleData.Where(x => !x.Active).Select(i => i.RuleID.ToString()).ToArray();
            if (_act.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == ActAccessibilityTesting.eAnalyzer.ByStandard.ToString())
            {
                if (_act.StandardList != null && _act.StandardList.Any())
                {
                    string[] Tag_array = _act.StandardList.Select(i => i.Value.ToString()).ToArray();
                    for (int i = 0; i < Tag_array.Length; i++)
                    {
                        if (Tag_array[i].Equals("bestpractice", StringComparison.OrdinalIgnoreCase))
                        {
                            Tag_array[i] = "best-practice";
                        }
                    }
                    axeRunOptions.RunOnly = new RunOnlyOptions
                    {
                        Type = "tag",
                        Values = Tag_array.ToList()
                    };
                }
                else if (_act.StandardList == null || !_act.StandardList.Any())
                {
                    _act.Status = eRunStatus.Failed;
                    _act.Error = "Standard list is empty or not set.";
                    return axeRunOptions;
                }

                if (_act.SeverityList != null && _act.SeverityList.Any())
                {
                    List<string> sevritylist = _act.SeverityList.Select(x => x.Value.ToLower()).ToList();
                    string[] SeverityExcludeRules = RuleData.Where(x => !ExcludeRules.Contains(x.RuleID) && sevritylist.Contains(x.Impact.ToLower())).Select(i => i.RuleID.ToString()).ToArray();
                    ExcludeRules = ExcludeRules.Concat(SeverityExcludeRules).ToArray();
                }
                if (ExcludeRules != null && ExcludeRules.Any())
                {
                    var rulesMap = new Dictionary<string, RuleOptions>();
                    foreach (var rule in ExcludeRules)
                    {
                        rulesMap[rule] = new RuleOptions
                        {
                            Enabled = false
                        };
                    }
                    axeRunOptions.Rules = rulesMap;
                }

            }
            else if (_act.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == ActAccessibilityTesting.eAnalyzer.BySeverity.ToString())
            {
                if (_act.SeverityList != null && _act.SeverityList.Any())
                {
                    List<string> sevritylist = _act.SeverityList.Select(x => x.Value.ToLower()).ToList();

                    string[] SevarityIncludeRules = RuleData.Where(x => !ExcludeRules.Contains(x.RuleID) && sevritylist.Contains(x.Impact.ToLower())).Select(i => i.RuleID.ToString()).ToArray();
                    if (SevarityIncludeRules != null && SevarityIncludeRules.Any())
                    {
                        axeRunOptions.RunOnly = new RunOnlyOptions
                        {
                            Type = "rule",
                            Values = SevarityIncludeRules.ToList()
                        };
                    }
                }
                else if (_act.SeverityList == null || !_act.SeverityList.Any())
                {
                    _act.Status = eRunStatus.Failed;
                    _act.Error = "Severity list is empty or not set.";
                    return axeRunOptions;
                }
            }

            return axeRunOptions;
        }

        private async Task<AxeResult?> TestPageAccessibilityAsync(AxeRunOptions options)
        {
            return await _tab.TestAccessibilityAsync(options);
        }

        private async Task<AxeResult?> TestElementAccessibilityAsync(AxeRunOptions options)
        {
            if (_browserElementLocator == null)
            {
                throw new InvalidOperationException($"{nameof(IBrowserElementLocator)} not found to perform accessibility test");
            }

            IBrowserElement? element = await _browserElementLocator.FindFirstMatchingElement(_act.LocateBy, _act.LocateValueCalculated);
            if (element == null)
            {
                throw new Exception($"No element found '{_act.LocateBy}' and value '{_act.LocateValueCalculated}'");
            }

            return await element.TestAccessibilityAsync(options);
        }

        private async Task ProcessResultAsync(AxeResult? result)
        {
            if (result == null)
            {
                return;
            }

            string path = String.Empty;

            if (WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder != null)
            {
                string folderPath = Path.Combine(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder, @"AccessibilityReport");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string DatetimeFormate = DateTime.Now.ToString("ddMMyyyy_HHmmssfff");
                string reportname = $"{_act.ItemName}_AccessibilityReport_{DatetimeFormate}.html";
                path = $"{folderPath}{Path.DirectorySeparatorChar}{reportname}";
                Act.AddArtifactToAction(Path.GetFileName(path), _act, path);
            }

            CreateAxeHtmlReport(result, await _tab.ScreenshotAsync(), path, ActAccessibilityTesting.ReportTypes.All);
            _act.AddOrUpdateReturnParamActual(ParamName: "Accessibility report", ActualValue: path);

            SetAxeResultToAction(result);
        }

        private void CreateAxeHtmlReport(AxeResult results, byte[] pageScreenshot, string destination, ActAccessibilityTesting.ReportTypes requestedResults)
        {
            var violationCount = GetCount(results.Violations);
            var incompleteCount = GetCount(results.Incomplete);
            var passCount = GetCount(results.Passes);
            var inapplicableCount = GetCount(results.Inapplicable);

            var doc = new HtmlDocument();
            doc.CreateComment("<!DOCTYPE html>\r\n");

            var htmlStructure = HtmlNode.CreateNode("<html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Accessibility Check</title><style></style></head><body><content></content><script></script></body></html>");
            doc.DocumentNode.AppendChild(htmlStructure);

            doc.DocumentNode.SelectSingleNode("//style").InnerHtml = GetCss(pageScreenshot);

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

            if (violationCount > 0 && requestedResults.HasFlag(ActAccessibilityTesting.ReportTypes.Violations))
            {
                GetReadableAxeResults(results.Violations, ResultType.Violations, doc, resultsFlex);
            }

            if (incompleteCount > 0 && requestedResults.HasFlag(ActAccessibilityTesting.ReportTypes.Incomplete))
            {
                GetReadableAxeResults(results.Incomplete, ResultType.Incomplete, doc, resultsFlex);
            }

            if (passCount > 0 && requestedResults.HasFlag(ActAccessibilityTesting.ReportTypes.Passes))
            {
                GetReadableAxeResults(results.Passes, ResultType.Passes, doc, resultsFlex);
            }

            if (inapplicableCount > 0 && requestedResults.HasFlag(ActAccessibilityTesting.ReportTypes.Inapplicable))
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

        private static string GetCountContent(int violationCount, int incompleteCount, int passCount, int inapplicableCount, ActAccessibilityTesting.ReportTypes requestedResults)
        {
            StringBuilder countString = new StringBuilder();

            if (requestedResults.HasFlag(ActAccessibilityTesting.ReportTypes.Violations))
            {
                countString.AppendLine($" Violation: {violationCount}<br>");
            }

            if (requestedResults.HasFlag(ActAccessibilityTesting.ReportTypes.Incomplete))
            {
                countString.AppendLine($" Incomplete: {incompleteCount}<br>");
            }

            if (requestedResults.HasFlag(ActAccessibilityTesting.ReportTypes.Passes))
            {
                countString.AppendLine($" Pass: {passCount}<br>");
            }

            if (requestedResults.HasFlag(ActAccessibilityTesting.ReportTypes.Inapplicable))
            {
                countString.AppendLine($" Inapplicable: {inapplicableCount}");
            }

            return countString.ToString();
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

        private static string GetCss(byte[] pageScreenshot)
        {
            return EmbeddedResourceProvider.ReadEmbeddedFile("htmlReporter.css").Replace("url('", $"url('{GetDataImageString(pageScreenshot)}");
        }

        private static string GetDataImageString(byte[] pageScreenshot)
        {
            return $"data:image/png;base64,{Convert.ToBase64String(pageScreenshot)}";
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

        private void SetAxeResultToAction(AxeResult axeResult)
        {
            bool hasAnyViolations = axeResult.Violations.Any();
            bool ActionResult = true;
            IEnumerable<string> AcceptableSeverity = Enumerable.Empty<string>().ToList();
            IEnumerable<string> Violationseverity = Enumerable.Empty<string>().ToList();
            if (_act.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == ActAccessibilityTesting.eAnalyzer.ByStandard.ToString() && _act.SeverityList != null && _act.SeverityList.Any())
            {
                AcceptableSeverity = _act.SeverityList.Select(x => x.Value.ToLower()).ToList();
                Violationseverity = axeResult.Violations.Any() ? axeResult.Violations.Select(x => x.Impact.ToLower()) : Enumerable.Empty<string>().ToList();
                ActionResult = Violationseverity.Intersect(AcceptableSeverity).Any();
            }
            else if (_act.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == ActAccessibilityTesting.eAnalyzer.BySeverity.ToString() && _act.SeverityList != null && _act.SeverityList.Any())
            {
                List<string> sevritylist = _act.SeverityList.Select(x => x.Value.ToLower()).ToList();
                Violationseverity = axeResult.Violations.Any() ? axeResult.Violations.Select(x => x.Impact.ToLower()) : Enumerable.Empty<string>().ToList();
                foreach (string severity in sevritylist)
                {
                    ActionResult = Violationseverity.Any(y => y.Equals(severity));
                }
            }
            var jsonresponse = JsonConvert.SerializeObject(axeResult, Newtonsoft.Json.Formatting.Indented);
            _act.RawResponseValues = jsonresponse;
            _act.AddOrUpdateReturnParamActual(ParamName: "Raw Response", ActualValue: jsonresponse);
            if (hasAnyViolations)
            {
                if (ActionResult)
                {
                    _act.Status = eRunStatus.Failed;
                }
                else
                {
                    _act.Status = eRunStatus.Passed;
                }
                _act.Error = $"Accessibility testing resulted in violations.";
                _act.AddOrUpdateReturnParamActual(ParamName: "ViolationCount", ActualValue: axeResult.Violations.Length.ToString());
                _act.AddOrUpdateReturnParamActual(ParamName: "ViolationList", ActualValue: String.Join(",", axeResult.Violations.Select(x => x.Id)));
                _act.AddOrUpdateReturnParamActual(ParamName: "ViolationSeverity", ActualValue: string.Join(",", Violationseverity.Distinct()));
                int violatedNodeIndex = 0;
                foreach (AxeResultItem violation in axeResult.Violations)
                {
                    foreach (AxeResultNode node in violation.Nodes)
                    {
                        violatedNodeIndex++;
                        _act.AddOrUpdateReturnParamActualWithPath(ParamName: "ViolationId", ActualValue: violation.Id, violatedNodeIndex.ToString());
                        if (node.XPath != null)
                        {
                            _act.AddOrUpdateReturnParamActualWithPath(ParamName: "NodeXPath", ActualValue: node.XPath.ToString(), violatedNodeIndex.ToString());
                        }

                        _act.AddOrUpdateReturnParamActualWithPath(ParamName: "ViolationHelp", ActualValue: violation.Help, violatedNodeIndex.ToString());
                        _act.AddOrUpdateReturnParamActualWithPath(ParamName: "ViolationSeverity", ActualValue: violation.Impact, violatedNodeIndex.ToString());
                    }
                }
            }
            else
            {
                _act.Status = eRunStatus.Passed;
            }
        }
    }
}
