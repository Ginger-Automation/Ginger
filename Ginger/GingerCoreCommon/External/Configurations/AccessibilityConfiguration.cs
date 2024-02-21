using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.Repository;

namespace Ginger.Configurations
{
    public class AccessibilityConfiguration : RepositoryItemBase
    {
        private string mStandard;
        [IsSerializedForLocalRepository]
        public string Standard
        {
            get
            {
                return mStandard;
            }
            set
            {
                if (mStandard != value)
                {
                    mStandard = value;
                    OnPropertyChanged(nameof(Standard));
                }
            }
        }

        private ObservableList<AccessibilityRuleData> mDefaultRuleList;

        [IsSerializedForLocalRepository]
        public ObservableList<AccessibilityRuleData> DefaultRuleList
        {
            get
            {
                return mDefaultRuleList;
            }
            set
            {
                if (value != mDefaultRuleList)
                {
                    mDefaultRuleList = value;
                    OnPropertyChanged(nameof(DefaultRuleList));
                }
            }
        }


        private string mSeverity;
        [IsSerializedForLocalRepository]
        public string Severity
        {
            get
            {
                return mSeverity;
            }
            set
            {
                if (mSeverity != value)
                {
                    mSeverity = value;
                    OnPropertyChanged(nameof(Severity));
                }
            }
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



        public override string ItemName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public ObservableList<AccessibilityRuleData> GetAccessibilityRules()
        {
            ObservableList<AccessibilityRuleData> accessibilityRules = new ObservableList<AccessibilityRuleData>();
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "area-alt", Description = "Ensures <area> elements of image maps have alternate text", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "c487ae"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-allowed-attr", Description = "Ensures an element's role supports its ARIA attributes", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "5c01ea"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-braille-equivalent", Description = "Ensure aria-braillelabel and aria-brailleroledescription have a non-braille equivalent", Impact = "Serious", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-command-name", Description = "Ensures every ARIA button, link and menuitem has an accessible name", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "97a4e1"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-conditional-attr", Description = "Ensures ARIA attributes are used as described in the specification of the element's role", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "5c01ea"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-deprecated-role", Description = "Ensures elements do not use deprecated roles", Impact = "Minor", Tags = "wcag2a" ,ACTRules = "674b10"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-hidden-body", Description = "Ensures aria-hidden = true is not present on the document body.", Impact = "Critical", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-hidden-focus", Description = "Ensures aria-hidden elements are not focusable nor contain focusable elements", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "6cfa84"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-input-field-name", Description = "Ensures every ARIA input field has an accessible name", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "e086e5"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-meter-name", Description = "Ensures every ARIA meter node has an accessible name", Impact = "Serious", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-progressbar-name", Description = "Ensures every ARIA progressbar node has an accessible name", Impact = "Serious", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-prohibited-attr", Description = "Ensures ARIA attributes are not prohibited for an element's role", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "5c01ea"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-required-attr", Description = "Ensures elements with ARIA roles have all required ARIA attributes", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "4e8ab6"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-required-children", Description = "Ensures elements with an ARIA role that require child roles contain them", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "bc4a75, ff89c9"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-required-parent", Description = "Ensures elements with an ARIA role that require parent roles are contained by them", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "ff89c9"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-roles", Description = "Ensures all elements with a role attribute use a valid value", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "674b10"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-toggle-field-name", Description = "Ensures every ARIA toggle field has an accessible name", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "e086e5"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-tooltip-name", Description = "Ensures every ARIA tooltip node has an accessible name", Impact = "Serious", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-valid-attr-value", Description = "Ensures all ARIA attributes have valid values", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "6a7281"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-valid-attr", Description = "Ensures attributes that begin with aria- are valid ARIA attributes", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "5f99a7"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "blink", Description = "Ensures <blink> elements are not used", Impact = "Serious", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "button-name", Description = "Ensures buttons have discernible text", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "97a4e1, m6b1q3"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "bypass", Description = "Ensures each page has at least one mechanism for a user to bypass navigation and jump straight to the content", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "cf77f2, 047fe0, b40fd1, 3e12e1, ye5d6e"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "color-contrast", Description = "Ensures the contrast between foreground and background colors meets WCAG 2 AA minimum contrast ratio thresholds", Impact = "Serious", Tags = "wcag2aa" ,ACTRules = "afw4f7, 09o5cg"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "definition-list", Description = "Ensures <dl> elements are structured correctly", Impact = "Serious", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "dlitem", Description = "Ensures <dt> and <dd> elements are contained by a <dl>", Impact = "Serious", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "document-title", Description = "Ensures each HTML document contains a non-empty <title> element", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "2779a5"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "duplicate-id-aria", Description = "Ensures every id attribute value used in ARIA and in labels is unique", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "3ea0c8"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "form-field-multiple-labels", Description = "Ensures form field does not have multiple label elements", Impact = "Moderate", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "frame-focusable-content", Description = "Ensures <frame> and <iframe> elements with focusable content do not have tabindex=-1", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "akn7bn"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "frame-title-unique", Description = "Ensures <iframe> and <frame> elements contain a unique title attribute", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "4b1c6c"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "frame-title", Description = "Ensures <iframe> and <frame> elements have an accessible name", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "cae760"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "html-has-lang", Description = "Ensures every HTML document has a lang attribute", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "b5c3f8"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "html-lang-valid", Description = "Ensures the lang attribute of the <html> element has a valid value", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "bf051a"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "html-xml-lang-mismatch", Description = "Ensure that HTML elements with both valid lang and xml:lang attributes agree on the base language of the page", Impact = "Moderate", Tags = "wcag2a" ,ACTRules = "5b7ae0"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "image-alt", Description = "Ensures <img> elements have alternate text or a role of none or presentation", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "23a2a8"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "input-button-name", Description = "Ensures input buttons have discernible text", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "97a4e1"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "input-image-alt", Description = "Ensures < input type = image > elements have alternate text", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "59796f"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "label", Description = "Ensures every form element has a label", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "e086e5"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "link-in-text-block", Description = "Ensure links are distinguished from surrounding text in a way that does not rely on color", Impact = "Serious", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "link-name", Description = "Ensures links have discernible text", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "c487ae"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "list", Description = "Ensures that lists are structured correctly", Impact = "Serious", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "listitem", Description = "Ensures <li> elements are used semantically", Impact = "Serious", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "marquee", Description = "Ensures <marquee> elements are not used", Impact = "Serious", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "meta-refresh", Description = "Ensures < meta http - equiv = refresh > is not used for delayed refresh", Impact = "Critical", Tags = "wcag2a", ACTRules = "bc659a, bisz58"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "meta-viewport", Description = "Ensures < meta name = viewport > does not disable text scaling and zooming", Impact = "Critical", Tags = "wcag2aa" ,ACTRules = "b4f0c3"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "nested-interactive", Description = "Ensures interactive controls are not nested as they are not always announced by screen readers or can cause focus problems for assistive technologies", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "307n5z"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "no-autoplay-audio", Description = "Ensures <video> or <audio> elements do not autoplay audio for more than 3 seconds without a control mechanism to stop or mute the audio", Impact = "Moderate", Tags = "wcag2a" ,ACTRules = "80f0bf"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "object-alt", Description = "Ensures <object> elements have alternate text", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "8fc3b6"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "role-img-alt", Description = "Ensures[role = img] elements have alternate text", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "23a2a8"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "scrollable-region-focusable", Description = "Ensure elements that have scrollable content are accessible by keyboard", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "0ssw9k"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "select-name", Description = "Ensures select element has an accessible name", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "e086e5"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "server-side-image-map", Description = "Ensures that server-side image maps are not used", Impact = "Minor", Tags = "wcag2a" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "svg-img-alt", Description = "Ensures <svg> elements with an img, graphics-document or graphics-symbol role have an accessible text", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "7d6734"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "td-headers-attr", Description = "Ensure that each cell in a table that uses the headers attribute refers only to other cells in that table", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "a25f45"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "th-has-data-cells", Description = "Ensure that <th> elements and elements with role=columnheader/rowheader have data cells they describe", Impact = "Serious", Tags = "wcag2a" ,ACTRules = "d0f69e"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "valid-lang", Description = "Ensures lang attributes have valid values", Impact = "Serious", Tags = "wcag2aa" ,ACTRules = "de46e4"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "video-caption", Description = "Ensures <video> elements have captions", Impact = "Critical", Tags = "wcag2a" ,ACTRules = "eac66b"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "autocomplete-valid", Description = "Ensure the autocomplete attribute is correct and suitable for the form field", Impact = "Serious", Tags = "wcag21aa" ,ACTRules = "73f2c2"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "avoid-inline-spacing", Description = "Ensure that text spacing set through style attributes can be adjusted with custom stylesheets", Impact = "Serious", Tags = "wcag21aa" ,ACTRules = "24afc2, 9e45ec, 78fd32"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "target-size", Description = "Ensure touch target have sufficient size and space", Impact = "Serious", Tags = "wcag22aa" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "accesskeys", Description = "Ensures every accesskey attribute value is unique", Impact = "Serious", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-allowed-role", Description = "Ensures role attribute has an appropriate value for the element", Impact = "Minor", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-dialog-name", Description = "Ensures every ARIA dialog and alertdialog node has an accessible name", Impact = "Serious", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-text", Description = "Ensures role = text is used on elements with no focusable descendants", Impact = "Serious", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "aria-treeitem-name", Description = "Ensures every ARIA treeitem node has an accessible name", Impact = "Serious", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "empty-heading", Description = "Ensures headings have discernible text", Impact = "Minor", Tags = "bestpractice" ,ACTRules = "ffd0e9"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "empty-table-header", Description = "Ensures table headers have discernible text", Impact = "Minor", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "frame-tested", Description = "Ensures <iframe> and <frame> elements contain the axe-core script", Impact = "Critical", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "heading-order", Description = "Ensures the order of headings is semantically correct", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "image-redundant-alt", Description = "Ensure image alternative is not repeated as text", Impact = "Minor", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "label-title-only", Description = "Ensures that every form element has a visible label and is not solely labeled using hidden labels, or the title or aria-describedby attributes", Impact = "Serious", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "landmark-banner-is-top-level", Description = "Ensures the banner landmark is at top level", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "landmark-complementary-is-top-level", Description = "Ensures the complementary landmark or aside is at top level", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "landmark-contentinfo-is-top-level", Description = "Ensures the contentinfo landmark is at top level", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "landmark-main-is-top-level", Description = "Ensures the main landmark is at top level", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "landmark-no-duplicate-banner", Description = "Ensures the document has at most one banner landmark", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "landmark-no-duplicate-contentinfo", Description = "Ensures the document has at most one contentinfo landmark", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "landmark-no-duplicate-main", Description = "Ensures the document has at most one main landmark", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "landmark-one-main", Description = "Ensures the document has a main landmark", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "landmark-unique", Description = "Landmarks should have a unique role or role/label/title (i.e. accessible name) combination", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "meta-viewport-large", Description = "Ensures < meta name = viewport > can scale a significant amount", Impact = "Minor", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "page-has-heading-one", Description = "Ensure that the page, or at least one of its frames contains a level-one heading", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "presentation-role-conflict", Description = "Elements marked as presentational should not have global ARIA or tabindex to ensure all screen readers ignore them", Impact = "Minor", Tags = "bestpractice" ,ACTRules = "46ca7f"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "region", Description = "Ensures all page content is contained by landmarks", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "scope-attr-valid", Description = "Ensures the scope attribute is used correctly on tables", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "skip-link", Description = "Ensure all skip links have a focusable target", Impact = "Moderate", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "tabindex", Description = "Ensures tabindex attribute values are not greater than 0", Impact = "Serious", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "table-duplicate-name", Description = "Ensure the <caption> element does not contain the same text as the summary attribute", Impact = "Minor", Tags = "bestpractice" ,ACTRules = ""});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "color-contrast-enhanced", Description = "Ensures the contrast between foreground and background colors meets WCAG 2 AAA enhanced contrast ratio thresholds", Impact = "Serious", Tags = "wcag2aaa" ,ACTRules = "09o5cg"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "identical-links-same-purpose", Description = "Ensure that links with the same accessible name serve a similar purpose", Impact = "Minor", Tags = "wcag2aaa" ,ACTRules = "b20e66"});
            accessibilityRules.Add( new AccessibilityRuleData { Active = false, RuleID = "meta-refresh-no-exceptions", Description = "Ensures < meta http - equiv = refresh > is not used for delayed refresh", Impact = "Minor", Tags = "wcag2aaa", ACTRules = "bisz58"});
            return accessibilityRules;
        }

        

    }
}
