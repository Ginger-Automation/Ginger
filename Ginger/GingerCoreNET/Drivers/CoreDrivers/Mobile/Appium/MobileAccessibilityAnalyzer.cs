using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using Ginger.Configurations;
using GingerCore.Actions;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile.Appium
{
    public class MobileAccessibilityAnalyzer
    {
        private const int MIN_TOUCH_TARGET_SIZE_PX = 48;

        private IWebDriver _driver;
        private ObservableList<AccessibilityRuleData> _activeRulesForAnalysis; // This will hold the filtered rules

        // Assuming a standard screen size for off-screen checks.
        // In a real scenario, you'd get this from the driver or a device capability.
        private const int SCREEN_WIDTH_PX = 1080;
        private const int SCREEN_HEIGHT_PX = 2220;

        // Regex to detect if a string consists ONLY of special characters (non-alphanumeric, non-whitespace)
        private static readonly Regex SpecialCharactersOnlyRegex = new Regex(@"^[^a-zA-Z0-9\s]+$");
        // Regex for common generic link texts
        private static readonly Regex GenericLinkTextRegex = new Regex(@"\b(click here|tap here|learn more|read more|details|link)\b", RegexOptions.IgnoreCase);

        // Constructor now accepts the list of active rules
        public MobileAccessibilityAnalyzer(IWebDriver driver, ObservableList<AccessibilityRuleData> activeRules)
        {
            this._driver = driver;
            this._activeRulesForAnalysis = activeRules ?? new ObservableList<AccessibilityRuleData>(); // Initialize to empty list if null
        }

        public List<AccessibilityIssue> AnalyzeFullPage()
        {
            List<AccessibilityIssue> pageIssues = new List<AccessibilityIssue>();
            Dictionary<string, List<string>> accessibleLabelMap = new Dictionary<string, List<string>>(); // To detect duplicate labels

            // If no active rules are provided, there's nothing to analyze for.
            if (!_activeRulesForAnalysis.Any())
            {
                Reporter.ToLog(eLogLevel.INFO, "No active accessibility rules provided for analysis.");
                return pageIssues;
            }

            try
            {
                string pageSource = _driver.PageSource;
                XDocument xmlDoc = XDocument.Parse(pageSource);

                // Comprehensive XPath to select potentially interactive or important elements.
                string xpathQuery = "//*[" +
                                    "@clickable or @focusable or @selected or @enabled or @checked or " +
                                    "@content-desc or @text or @resource-id or @class or @hint or " +
                                    "@label or @name or @value or @accessible or @hittable or " +
                                    "@bounds or @rect or @password or @importantForAccessibility or @inputType or @keyboardType" +
                                    "]";

                IEnumerable<XElement> elements = xmlDoc.XPathSelectElements(xpathQuery);

                if (!elements.Any())
                {
                    Reporter.ToLog(eLogLevel.INFO, "No interactive or content-bearing elements found on this page based on the XPath query.");
                    return pageIssues;
                }

                foreach (XElement element in elements)
                {
                    // First pass: collect accessible names for duplicate check (this is a global rule)
                    string primaryAccessibleLabel = GetPrimaryAccessibleLabel(element);
                    string elementIdentifier = GetElementIdentifier(element);

                    if (!string.IsNullOrWhiteSpace(primaryAccessibleLabel))
                    {
                        string normalizedLabel = primaryAccessibleLabel.Trim().ToLower();
                        if (!accessibleLabelMap.ContainsKey(normalizedLabel))
                        {
                            accessibleLabelMap[normalizedLabel] = new List<string>();
                        }
                        accessibleLabelMap[normalizedLabel].Add(elementIdentifier);
                    }

                    // Now, analyze the element for individual issues using the filtered active rules
                    List<AccessibilityIssue> issues = AnalyzeXmlElement(element);
                    pageIssues.AddRange(issues);
                }

                // --- Post-processing for global rules like Duplicate Accessibility Label ---
                // Only run this check if the "DuplicateAccessibilityLabel" rule is active.
                if (_activeRulesForAnalysis.Any(r => r.RuleID == "DuplicateAccessibilityLabel"))
                {
                    foreach (var entry in accessibleLabelMap)
                    {
                        if (entry.Value.Count > 1)
                        {
                            pageIssues.Add(new AccessibilityIssue
                            {
                                RuleId = "DuplicateAccessibilityLabel",
                                Description = $"Multiple elements share the accessible label '{entry.Key}'. This can confuse screen reader users.",
                                ElementIdentifier = $"Affected Elements: {string.Join(";", entry.Value)}",
                                Severity = GetRuleSeverity("DuplicateAccessibilityLabel"), // Get severity from rule data
                                SuggestedFix = GetRuleSuggestedFix("DuplicateAccessibilityLabel"), // Get fix from rule data
                                RelatedWCAG = GetRuleRelatedWCAG("DuplicateAccessibilityLabel") // Get WCAG from rule data
                            });
                        }
                    }
                }


                if (pageIssues.Count > 0)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Page Accessibility Issues Found:");
                    foreach (var issue in pageIssues)
                    {
                        Reporter.ToLog(eLogLevel.INFO, $" - {issue}");
                    }
                }
                else
                {
                    Reporter.ToLog(eLogLevel.INFO, "No significant accessibility issues found on this page.");
                }
            }
            catch (System.Xml.XmlException xmlEx)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"XML Parsing Error: {xmlEx.Message}. Please check if the page source is valid XML.", xmlEx);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error analyzing page: {ex.Message}.", ex);
            }

            return pageIssues;
        }

        /// <summary>
        /// Helper to get the primary accessible label from an element.
        /// </summary>
        private string GetPrimaryAccessibleLabel(XElement element)
        {
            return element.Attribute("content-desc")?.Value ??
                   element.Attribute("label")?.Value ?? // iOS
                   element.Attribute("name")?.Value ??   // iOS
                   element.Attribute("text")?.Value ??
                   element.Attribute("hint")?.Value; // Android hint
        }

        /// <summary>
        /// Helper to construct a unique identifier for the element for reporting.
        /// </summary>
        private string GetElementIdentifier(XElement element)
        {
            string elementType = element.Name.LocalName;
            string resourceId = element.Attribute("resource-id")?.Value;
            string bounds = element.Attribute("bounds")?.Value;
            string rect = element.Attribute("rect")?.Value;
            string identifier = $"Type: {elementType} | ID: {resourceId ?? "N/A"}";
            if (!string.IsNullOrWhiteSpace(bounds)) identifier += $" | Bounds: {bounds}";
            else if (!string.IsNullOrWhiteSpace(rect)) identifier += $" | Rect: {rect}";
            return identifier;
        }

        /// <summary>
        /// Helper to get severity for a rule from the loaded active rules.
        /// </summary>
        private string GetRuleSeverity(string ruleId)
        {
            return _activeRulesForAnalysis.FirstOrDefault(r => r.RuleID == ruleId)?.Impact ?? "Unknown";
        }

        /// <summary>
        /// Helper to get suggested fix for a rule from the loaded active rules.
        /// </summary>
        private string GetRuleSuggestedFix(string ruleId)
        {
            return _activeRulesForAnalysis.FirstOrDefault(r => r.RuleID == ruleId)?.SuggestedFix ?? "N/A";
        }

        /// <summary>
        /// Helper to get related WCAG for a rule from the loaded active rules.
        /// </summary>
        private string GetRuleRelatedWCAG(string ruleId)
        {
            return _activeRulesForAnalysis.FirstOrDefault(r => r.RuleID == ruleId)?.RelatedWCAG ?? "N/A";
        }


        private List<AccessibilityIssue> AnalyzeXmlElement(XElement element)
        {
            List<AccessibilityIssue> issues = new List<AccessibilityIssue>();

            string elementType = element.Name.LocalName;
            string resourceId = element.Attribute("resource-id")?.Value;
            string contentDesc = element.Attribute("content-desc")?.Value;
            string text = element.Attribute("text")?.Value;
            string label = element.Attribute("label")?.Value;
            string name = element.Attribute("name")?.Value;
            string bounds = element.Attribute("bounds")?.Value;
            string rect = element.Attribute("rect")?.Value;
            string className = element.Attribute("class")?.Value;
            string type = element.Attribute("type")?.Value; // iOS XCUIElementType
            string enabledAttr = element.Attribute("enabled")?.Value;
            string importantForAccessibility = element.Attribute("importantForAccessibility")?.Value; // Android
            string passwordAttr = element.Attribute("password")?.Value; // Android EditText specific
            string inputTypeAttr = element.Attribute("inputType")?.Value; // Android EditText specific
            string keyboardTypeAttr = element.Attribute("keyboardType")?.Value; // iOS TextField specific
            string hintAttr = element.Attribute("hint")?.Value; // Android input hint


            bool isClickable = element.Attribute("clickable")?.Value.ToLower() == "true";
            bool isFocusable = element.Attribute("focusable")?.Value.ToLower() == "true";
            bool isAccessible = element.Attribute("accessible")?.Value.ToLower() == "true";
            bool isHittable = element.Attribute("hittable")?.Value.ToLower() == "true";
            bool isEnabled = enabledAttr == null || enabledAttr.ToLower() == "true"; // If attribute is missing, assume enabled

            string primaryAccessibleLabel = GetPrimaryAccessibleLabel(element);
            string elementIdentifier = GetElementIdentifier(element);

            // Declare these variables at the method level so they are accessible to all rules
            int elementX = 0, elementY = 0, elementWidth = 0, elementHeight = 0;
            bool isEditableField = false; // Initialize here

            // Parse bounds/rect once if needed by multiple rules
            if (!string.IsNullOrWhiteSpace(bounds)) // Android bounds: [x1,y1][x2,y2]
            {
                Match match = Regex.Match(bounds, @"\[(\d+),(\d+)\]\[(\d+),(\d+)\]");
                if (match.Success)
                {
                    elementX = int.Parse(match.Groups[1].Value);
                    elementY = int.Parse(match.Groups[2].Value);
                    int x2 = int.Parse(match.Groups[3].Value);
                    int y2 = int.Parse(match.Groups[4].Value);
                    elementWidth = x2 - elementX;
                    elementHeight = y2 - elementY;
                }
            }
            else if (!string.IsNullOrWhiteSpace(rect)) // iOS rect: {{x,y},{width,height}}
            {
                Match match = Regex.Match(rect, @"\{\{(\d+),(\d+)\},\{(\d+),(\d+)\}\}");
                if (match.Success)
                {
                    elementX = int.Parse(match.Groups[1].Value);
                    elementY = int.Parse(match.Groups[2].Value);
                    elementWidth = int.Parse(match.Groups[3].Value);
                    elementHeight = int.Parse(match.Groups[4].Value);
                }
            }
            // Determine if it's an editable field once
            isEditableField = (className?.Contains("EditText") == true || elementType.Contains("XCUIElementTypeTextField") || elementType.Contains("XCUIElementTypeSecureTextField"));


            // --- Accessibility Rule Checks (each wrapped in a check for _activeRulesForAnalysis) ---

            // Rule: AccessibleName_Missing (WCAG 1.1.1, 2.4.6, 4.1.2)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "AccessibleName_Missing"))
            {
                if ((isClickable || isFocusable || isAccessible || isHittable) &&
                    string.IsNullOrWhiteSpace(primaryAccessibleLabel))
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = "AccessibleName_Missing",
                        Description = "Interactive element lacks a descriptive accessible label (content-desc, label, name, or text).",
                        ElementIdentifier = elementIdentifier,
                        Severity = GetRuleSeverity("AccessibleName_Missing"),
                        SuggestedFix = GetRuleSuggestedFix("AccessibleName_Missing"),
                        RelatedWCAG = GetRuleRelatedWCAG("AccessibleName_Missing")
                    });
                }
            }

            // Rule: Image_MissingContentDesc (WCAG 1.1.1)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "Image_MissingContentDesc"))
            {
                if ((elementType.Contains("ImageView") || elementType.Contains("ImageButton") ||
                     elementType.Contains("Image") || elementType.Contains("XCUIElementTypeImage")) &&
                    string.IsNullOrWhiteSpace(primaryAccessibleLabel))
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = "Image_MissingContentDesc",
                        Description = "Image or ImageButton is missing a content-desc/label/name for screen readers.",
                        ElementIdentifier = elementIdentifier,
                        Severity = GetRuleSeverity("Image_MissingContentDesc"),
                        SuggestedFix = GetRuleSuggestedFix("Image_MissingContentDesc"),
                        RelatedWCAG = GetRuleRelatedWCAG("Image_MissingContentDesc")
                    });
                }
            }

            // Rule: GenericText_Interactive (WCAG 2.4.4, 2.4.6)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "GenericText_Interactive"))
            {
                if ((isClickable || isFocusable || isAccessible || isHittable) && !string.IsNullOrWhiteSpace(primaryAccessibleLabel))
                {
                    if (GenericLinkTextRegex.IsMatch(primaryAccessibleLabel))
                    {
                        issues.Add(new AccessibilityIssue
                        {
                            RuleId = "GenericText_Interactive",
                            Description = $"Interactive element has generic or non-descriptive text/label: '{primaryAccessibleLabel}'.",
                            ElementIdentifier = elementIdentifier,
                            Severity = GetRuleSeverity("GenericText_Interactive"),
                            SuggestedFix = GetRuleSuggestedFix("GenericText_Interactive"),
                            RelatedWCAG = GetRuleRelatedWCAG("GenericText_Interactive")
                        });
                    }
                }
            }

            // Rule: RedundantDescription (WCAG 2.4.4)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "RedundantDescription"))
            {
                if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(contentDesc) &&
                    text.Equals(contentDesc, StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = "RedundantDescription",
                        Description = $"Element has redundant text and content-desc ('{text}' and '{contentDesc}'). Screen readers might announce it twice.",
                        ElementIdentifier = elementIdentifier,
                        Severity = GetRuleSeverity("RedundantDescription"),
                        SuggestedFix = GetRuleSuggestedFix("RedundantDescription"),
                        RelatedWCAG = GetRuleRelatedWCAG("RedundantDescription")
                    });
                }
                if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(label) &&
                    text.Equals(label, StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = "RedundantDescription",
                        Description = $"Element has redundant text and label ('{text}' and '{label}'). Screen readers might announce it twice.",
                        ElementIdentifier = elementIdentifier,
                        Severity = GetRuleSeverity("RedundantDescription"),
                        SuggestedFix = GetRuleSuggestedFix("RedundantDescription"),
                        RelatedWCAG = GetRuleRelatedWCAG("RedundantDescription")
                    });
                }
            }

            // Rule: TouchTarget_TooSmall (WCAG 2.5.5)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "TouchTarget_TooSmall"))
            {
                // elementX, elementY, elementWidth, elementHeight are now declared at method scope
                if ((isClickable || isHittable) && (elementWidth > 0 && elementHeight > 0))
                {
                    if (elementWidth < MIN_TOUCH_TARGET_SIZE_PX || elementHeight < MIN_TOUCH_TARGET_SIZE_PX)
                    {
                        issues.Add(new AccessibilityIssue
                        {
                            RuleId = "TouchTarget_TooSmall",
                            Description = $"Interactive element has a touch target size of {elementWidth}x{elementHeight}px, which is smaller than the recommended minimum of {MIN_TOUCH_TARGET_SIZE_PX}x{MIN_TOUCH_TARGET_SIZE_PX}px.",
                            ElementIdentifier = elementIdentifier,
                            Severity = GetRuleSeverity("TouchTarget_TooSmall"),
                            SuggestedFix = GetRuleSuggestedFix("TouchTarget_TooSmall"),
                            RelatedWCAG = GetRuleRelatedWCAG("TouchTarget_TooSmall")
                        });
                    }
                }
            }

            // Rule: EditableField_NoLabel (WCAG 3.3.2)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "EditableField_NoLabel"))
            {
                // isEditableField is now declared at method scope
                if (isEditableField && string.IsNullOrWhiteSpace(primaryAccessibleLabel))
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = "EditableField_NoLabel",
                        Description = "Editable text field lacks an accessible label/description.",
                        ElementIdentifier = elementIdentifier,
                        Severity = GetRuleSeverity("EditableField_NoLabel"),
                        SuggestedFix = GetRuleSuggestedFix("EditableField_NoLabel"),
                        RelatedWCAG = GetRuleRelatedWCAG("EditableField_NoLabel")
                    });
                }
            }

            // Rule: InformationalElement_Interactive (WCAG 4.1.2)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "InformationalElement_Interactive"))
            {
                if ((elementType.Contains("TextView") || elementType.Contains("StaticText")) &&
                    (isClickable || isFocusable || isHittable || isAccessible) &&
                    string.IsNullOrWhiteSpace(primaryAccessibleLabel) && !string.IsNullOrWhiteSpace(text))
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = "InformationalElement_Interactive",
                        Description = $"Informational element ('{text}') is marked as interactive but lacks a clear accessible role or descriptive name/label.",
                        ElementIdentifier = elementIdentifier,
                        Severity = GetRuleSeverity("InformationalElement_Interactive"),
                        SuggestedFix = GetRuleSuggestedFix("InformationalElement_Interactive"),
                        RelatedWCAG = GetRuleRelatedWCAG("InformationalElement_Interactive")
                    });
                }
            }

            // Rule: State_Missing (WCAG 4.1.2)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "State_Missing"))
            {
                if ((elementType.Contains("CheckBox") || elementType.Contains("RadioButton") || elementType.Contains("Switch")) ||
                    (elementType.Contains("XCUIElementTypeSwitch") || elementType.Contains("XCUIElementTypeRadioButton")))
                {
                    string checkedAttr = element.Attribute("checked")?.Value;
                    string selectedAttr = element.Attribute("selected")?.Value;

                    if (string.IsNullOrWhiteSpace(checkedAttr) && string.IsNullOrWhiteSpace(selectedAttr))
                    {
                        issues.Add(new AccessibilityIssue
                        {
                            RuleId = "State_Missing",
                            Description = $"Checkbox/Radio button/Switch is missing 'checked' or 'selected' state attribute.",
                            ElementIdentifier = elementIdentifier,
                            Severity = GetRuleSeverity("State_Missing"),
                            SuggestedFix = GetRuleSuggestedFix("State_Missing"),
                            RelatedWCAG = GetRuleRelatedWCAG("State_Missing")
                        });
                    }
                }
            }

            // Rule: EnabledState_Mismatch (WCAG 4.1.2)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "EnabledState_Mismatch"))
            {
                if ((isClickable || isFocusable || isHittable) && enabledAttr?.ToLower() == "false")
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = "EnabledState_Mismatch",
                        Description = "Interactive element is marked as disabled but may still be reachable or announced incorrectly.",
                        ElementIdentifier = elementIdentifier,
                        Severity = GetRuleSeverity("EnabledState_Mismatch"),
                        SuggestedFix = GetRuleSuggestedFix("EnabledState_Mismatch"),
                        RelatedWCAG = GetRuleRelatedWCAG("EnabledState_Mismatch")
                    });
                }
            }

            // Rule: Element_OffScreen / Element_PartiallyOffScreen (WCAG 1.3.1, 2.4.7)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "Element_OffScreen" || r.RuleID == "Element_PartiallyOffScreen"))
            {
                // elementX, elementY, elementWidth, elementHeight are now declared at method scope
                if (elementWidth > 0 && elementHeight > 0)
                {
                    bool isOffScreen = elementX >= SCREEN_WIDTH_PX || elementY >= SCREEN_HEIGHT_PX ||
                                       (elementX + elementWidth <= 0) || (elementY + elementHeight <= 0);

                    bool isPartiallyOffScreen = !isOffScreen && (
                                                elementX < 0 || elementY < 0 ||
                                                (elementX + elementWidth > SCREEN_WIDTH_PX) ||
                                                (elementY + elementHeight > SCREEN_HEIGHT_PX)
                                               );

                    if (isOffScreen && _activeRulesForAnalysis.Any(r => r.RuleID == "Element_OffScreen"))
                    {
                        issues.Add(new AccessibilityIssue
                        {
                            RuleId = "Element_OffScreen",
                            Description = "Element is completely off-screen and inaccessible.",
                            ElementIdentifier = elementIdentifier,
                            Severity = GetRuleSeverity("Element_OffScreen"),
                            SuggestedFix = GetRuleSuggestedFix("Element_OffScreen"),
                            RelatedWCAG = GetRuleRelatedWCAG("Element_OffScreen")
                        });
                    }
                    else if (isPartiallyOffScreen && _activeRulesForAnalysis.Any(r => r.RuleID == "Element_PartiallyOffScreen"))
                    {
                        issues.Add(new AccessibilityIssue
                        {
                            RuleId = "Element_PartiallyOffScreen",
                            Description = "Element is partially off-screen, potentially causing issues with visibility or interaction.",
                            ElementIdentifier = elementIdentifier,
                            Severity = GetRuleSeverity("Element_PartiallyOffScreen"),
                            SuggestedFix = GetRuleSuggestedFix("Element_PartiallyOffScreen"),
                            RelatedWCAG = GetRuleRelatedWCAG("Element_PartiallyOffScreen")
                        });
                    }
                }
            }

            // Rule: ImportantForAccessibility_No (Android Specific) (WCAG 4.1.2)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "ImportantForAccessibility_No"))
            {
                if (importantForAccessibility?.ToLower() == "no" && element.HasElements &&
                    (isClickable || isFocusable || !string.IsNullOrWhiteSpace(contentDesc) || !string.IsNullOrWhiteSpace(text)))
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = "ImportantForAccessibility_No",
                        Description = "Element has 'importantForAccessibility=\"no\"' but contains interactive or meaningful content that might be hidden from assistive technologies.",
                        ElementIdentifier = elementIdentifier,
                        Severity = GetRuleSeverity("ImportantForAccessibility_No"),
                        SuggestedFix = GetRuleSuggestedFix("ImportantForAccessibility_No"),
                        RelatedWCAG = GetRuleRelatedWCAG("ImportantForAccessibility_No")
                    });
                }
            }

            // Rule: PasswordField_ValueExposed (WCAG 1.3.1, 2.4.6)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "PasswordField_ValueExposed"))
            {
                if (passwordAttr?.ToLower() == "true" && !string.IsNullOrWhiteSpace(text) && text.Length > 0)
                {
                    if (text.Equals("password", StringComparison.OrdinalIgnoreCase))
                    {
                        issues.Add(new AccessibilityIssue
                        {
                            RuleId = "PasswordField_ValueExposed",
                            Description = $"Password field's visible text or content-desc/label might expose sensitive information or be too generic: '{text}'.",
                            ElementIdentifier = elementIdentifier,
                            Severity = GetRuleSeverity("PasswordField_ValueExposed"),
                            SuggestedFix = GetRuleSuggestedFix("PasswordField_ValueExposed"),
                            RelatedWCAG = GetRuleRelatedWCAG("PasswordField_ValueExposed")
                        });
                    }
                }
            }

            // Rule: SpecialCharacterLabel (WCAG 2.4.6)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "SpecialCharacterLabel"))
            {
                if (!string.IsNullOrWhiteSpace(primaryAccessibleLabel) &&
                    SpecialCharactersOnlyRegex.IsMatch(primaryAccessibleLabel))
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = "SpecialCharacterLabel",
                        Description = $"Element's accessible label consists only of special characters: '{primaryAccessibleLabel}'. This is not descriptive.",
                        ElementIdentifier = elementIdentifier,
                        Severity = GetRuleSeverity("SpecialCharacterLabel"),
                        SuggestedFix = GetRuleSuggestedFix("SpecialCharacterLabel"),
                        RelatedWCAG = GetRuleRelatedWCAG("SpecialCharacterLabel")
                    });
                }
            }

            // Rule: InputType_Generic (WCAG 1.3.5)
            // Renamed from InputType_GenericButSpecificLabel based on your JSON's "InputType_GenericButSpecificLabel"
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "InputType_GenericButSpecificLabel")) // Use the exact RuleID from JSON
            {
                // isEditableField is now declared at method scope
                if (isEditableField)
                {
                    bool hasSpecificInputType = !string.IsNullOrWhiteSpace(inputTypeAttr) || !string.IsNullOrWhiteSpace(keyboardTypeAttr);
                    bool isGenericTextType = (inputTypeAttr?.ToLower() == "text" || keyboardTypeAttr?.ToLower() == "default");

                    if (!hasSpecificInputType || isGenericTextType)
                    {
                        if (passwordAttr?.ToLower() != "true")
                        {
                            issues.Add(new AccessibilityIssue
                            {
                                RuleId = "InputType_GenericButSpecificLabel", // Corrected RuleId
                                Description = $"Editable field might benefit from a more specific input type (e.g., email, number, phone) for better user experience and autofill.",
                                ElementIdentifier = elementIdentifier,
                                Severity = GetRuleSeverity("InputType_GenericButSpecificLabel"), // Corrected RuleId
                                SuggestedFix = GetRuleSuggestedFix("InputType_GenericButSpecificLabel"), // Corrected RuleId
                                RelatedWCAG = GetRuleRelatedWCAG("InputType_GenericButSpecificLabel") // Corrected RuleId
                            });
                        }
                    }
                }
            }


            // Rule: LabelInName_Mismatch (WCAG 2.4.6, 4.1.2)
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "LabelInName_Mismatch"))
            {
                if ((isClickable || isFocusable || isAccessible || isHittable) && !string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(primaryAccessibleLabel))
                {
                    if (!primaryAccessibleLabel.Contains(text, StringComparison.OrdinalIgnoreCase))
                    {
                        issues.Add(new AccessibilityIssue
                        {
                            RuleId = "LabelInName_Mismatch",
                            Description = $"Interactive element's accessible label ('{primaryAccessibleLabel}') does not contain its visible text ('{text}'). This can be confusing for screen reader users.",
                            ElementIdentifier = elementIdentifier,
                            Severity = GetRuleSeverity("LabelInName_Mismatch"),
                            SuggestedFix = GetRuleSuggestedFix("LabelInName_Mismatch"),
                            RelatedWCAG = GetRuleRelatedWCAG("LabelInName_Mismatch")
                        });
                    }
                }
            }

            // Rule: TraversalOrder_Review (WCAG 2.4.3)
            // This is a "needs-review" rule, typically not a direct "failure" but a flag for manual check.
            if (_activeRulesForAnalysis.Any(r => r.RuleID == "TraversalOrder_Review"))
            {
                if (isFocusable) // Or other criteria indicating it's part of the focus order
                {
                    issues.Add(new AccessibilityIssue
                    {
                        RuleId = "TraversalOrder_Review",
                        Description = "Focusable element detected. Manual review is required to ensure logical traversal order for keyboard/directional navigation.",
                        ElementIdentifier = elementIdentifier,
                        Severity = GetRuleSeverity("TraversalOrder_Review"),
                        SuggestedFix = GetRuleSuggestedFix("TraversalOrder_Review"),
                        RelatedWCAG = GetRuleRelatedWCAG("TraversalOrder_Review")
                    });
                }
            }


            return issues;
        }

        // Inside your AnalyzerMobileAccessibility method in ActAccessibilityTesting
        public void AnalyzerMobileAccessibility(IWebDriver Driver, IWebElement element, Act currentAct, MobileAccessibilityAnalyzer mobileAnalyzer)
        {
            try
            {
                currentAct.Artifacts = new ObservableList<ArtifactDetails>(); // Initialize artifacts list
                currentAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running;

                if (currentAct.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && !string.IsNullOrEmpty(currentAct.Error))
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error: {currentAct.Error}");
                    return;
                }

                List<AccessibilityIssue> mobileAxeResult = null;


                if (currentAct.GetInputParamValue(ActAccessibilityTesting.Fields.Target) == ActAccessibilityTesting.eTarget.Element.ToString())
                {
                    Reporter.ToLog(eLogLevel.INFO, "Element-specific mobile accessibility analysis is not yet fully implemented. Performing full page analysis.");
                    mobileAxeResult = mobileAnalyzer.AnalyzeFullPage();
                }
                else
                {
                    mobileAxeResult = mobileAnalyzer.AnalyzeFullPage();
                }

                string path = String.Empty;

                if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder != null)
                {
                    string folderPath = Path.Combine(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder, "MobileAccessibilityReport");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    string DatetimeFormate = DateTime.Now.ToString("ddMMyyyy_HHmmssfff");
                    string reportname = $"{currentAct.ItemName}_MobileAccessibilityReport_{DatetimeFormate}.html";
                    path = Path.Combine(folderPath, reportname);

                    // Create the HTML report
                    CreateMobileAccessibilityHtmlReport(mobileAxeResult, path);

                    Act.AddArtifactToAction(Path.GetFileName(path), currentAct, path);
                    currentAct.AddOrUpdateReturnParamActual(ParamName: "Mobile Accessibility report", ActualValue: path);
                }

                // You might want to set a 'failed' status if critical issues are found
                if (mobileAxeResult.Any(issue => issue.Severity == "Critical"))
                {
                    currentAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    currentAct.Error = "Critical mobile accessibility issues found.";
                    Reporter.ToLog(eLogLevel.ERROR, currentAct.Error);
                }
                else if (mobileAxeResult.Any()) // If there are issues but no critical ones
                {
                    currentAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed; // Or Warning
                    currentAct.Error = "Mobile accessibility issues found (no critical).";
                    Reporter.ToLog(eLogLevel.WARN, currentAct.Error);
                }
                // If no issues found, status remains Passed from initial setting.
            }
            catch (Exception ex)
            {
                currentAct.Error = "Error during mobile accessibility testing: " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Error during mobile accessibility testing", ex);
                currentAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }
        }

        /// <summary>
        /// Generates an HTML report from the list of mobile accessibility issues.
        /// </summary>
        /// <param name="issues">The list of accessibility issues.</param>
        /// <param name="outputPath">The file path where the HTML report will be saved.</param>
        private void CreateMobileAccessibilityHtmlReport(List<AccessibilityIssue> issues, string outputPath)
        {
            StringBuilder html = new StringBuilder();

            html.Append("<!DOCTYPE html>");
            html.Append("<html lang='en'>");
            html.Append("<head>");
            html.Append("    <meta charset='UTF-8'>");
            html.Append("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.Append("    <title>Mobile Accessibility Report</title>");
            html.Append("    <style>");
            html.Append("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; background-color: #f4f7f6; color: #333; }");
            html.Append("        .container { max-width: 1000px; margin: auto; background-color: #fff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.08); }");
            html.Append("        h1 { color: #2c3e50; text-align: center; margin-bottom: 30px; }");
            html.Append("        h2 { color: #34495e; border-bottom: 2px solid #eee; padding-bottom: 10px; margin-top: 40px; }");
            html.Append("        .summary-box { background-color: #ecf0f1; padding: 20px; border-radius: 5px; margin-bottom: 30px; display: flex; justify-content: space-around; text-align: center; }");
            html.Append("        .summary-item { flex: 1; padding: 0 10px; }");
            html.Append("        .summary-item h3 { margin-bottom: 5px; color: #555; }");
            html.Append("        .summary-item p { font-size: 2em; font-weight: bold; margin: 0; }");
            html.Append("        .critical { color: #e74c3c; }");
            html.Append("        .moderate { color: #f39c12; }");
            html.Append("        .minor { color: #3498db; }");
            html.Append("        table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            html.Append("        th, td { padding: 12px 15px; text-align: left; border-bottom: 1px solid #ddd; }");
            html.Append("        th { background-color: #e0e0e0; color: #333; font-weight: bold; }");
            html.Append("        tr:nth-child(even) { background-color: #f9f9f9; }");
            html.Append("        .issue-details { background-color: #fdfdfd; border-left: 5px solid; padding: 15px; margin-bottom: 15px; border-radius: 5px; }");
            html.Append("        .issue-details.Critical { border-color: #e74c3c; }");
            html.Append("        .issue-details.Moderate { border-color: #f39c12; }");
            html.Append("        .issue-details.Minor { border-color: #3498db; }");
            html.Append("        .issue-details p { margin: 5px 0; line-height: 1.6; }");
            html.Append("        .issue-details strong { color: #555; }");
            html.Append("        .no-issues { text-align: center; color: #27ae60; font-size: 1.2em; padding: 30px; border: 1px dashed #2ecc71; border-radius: 5px; }");
            html.Append("        .footer { text-align: center; margin-top: 50px; font-size: 0.9em; color: #777; }");
            html.Append("    </style>");
            html.Append("</head>");
            html.Append("<body>");
            html.Append("    <div class='container'>");
            html.Append("        <h1>Mobile Accessibility Report</h1>");

            int criticalCount = issues.Count(i => i.Severity == "Critical");
            int moderateCount = issues.Count(i => i.Severity == "Moderate");
            int minorCount = issues.Count(i => i.Severity == "Minor");
            int totalIssues = issues.Count;

            html.Append("        <div class='summary-box'>");
            html.Append("            <div class='summary-item'><h3 class='critical'>Critical</h3><p class='critical'>" + criticalCount + "</p></div>");
            html.Append("            <div class='summary-item'><h3 class='moderate'>Moderate</h3><p class='moderate'>" + moderateCount + "</p></div>");
            html.Append("            <div class='summary-item'><h3 class='minor'>Minor</h3><p class='minor'>" + minorCount + "</p></div>");
            html.Append("            <div class='summary-item'><h3>Total Issues</h3><p>" + totalIssues + "</p></div>");
            html.Append("        </div>");



            if (issues.Any())
            {
                html.Append("        <h2>Detailed Issues</h2>");
                foreach (var issue in issues.OrderByDescending(i => i.Severity == "Critical") // Critical first
                                            .ThenByDescending(i => i.Severity == "Moderate") // then Moderate
                                            .ThenBy(i => i.RuleId)) // then by RuleId
                {
                    html.Append($"        <div class='issue-details {issue.Severity}'>");
                    html.Append($"            <p><strong>Rule ID:</strong> {issue.RuleId}</p>");
                    html.Append($"            <p><strong>Description:</strong> {issue.Description}</p>");
                    html.Append($"            <p><strong>Element:</strong> <code>{issue.ElementIdentifier}</code></p>");
                    html.Append($"            <p><strong>Severity:</strong> <span class='{issue.Severity.ToLower()}'>{issue.Severity}</span></p>");
                    html.Append($"            <p><strong>Suggested Fix:</strong> {issue.SuggestedFix}</p>");
                    html.Append("        </div>");
                }
            }
            else
            {
                html.Append("        <div class='no-issues'>");
                html.Append("            <p>🎉 No accessibility issues found on this page!</p>");
                html.Append("        </div>");
            }

            html.Append("        <div class='footer'>");
            html.Append("            <p>Report generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "</p>");
            html.Append("        </div>");
            html.Append("    </div>");
            html.Append("</body>");
            html.Append("</html>");

            try
            {
                File.WriteAllText(outputPath, html.ToString());
                Reporter.ToLog(eLogLevel.INFO, $"Mobile accessibility report saved to: {outputPath}");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to write mobile accessibility report to {outputPath}: {ex.Message}", ex);
            }
        }
    }

}
