using Ginger.Configurations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile.Appium
{
    /// <summary>
    /// Provides extension methods for filtering and modifying collections of <see cref="AccessibilityRuleData"/>.
    /// Includes methods to filter rules by tags, disable rules by their IDs, and select specific rules by their IDs.
    /// These methods are designed to work with <see cref="ObservableCollection{AccessibilityRuleData}"/> for use in mobile accessibility scenarios.
    /// </summary>
    public static class MobileAccessibilityRuleDataExtensions
    {
        public static ObservableCollection<AccessibilityRuleData> WithTags(this ObservableCollection<AccessibilityRuleData> source, string[] tagsToInclude)
        {
            if (tagsToInclude == null || !tagsToInclude.Any())
            {
                return source;
            }

            var lowerCaseTagsToInclude = new HashSet<string>(tagsToInclude.Select(t => t.ToLower()));

            var filteredRules = source.Where(rule =>
                !string.IsNullOrWhiteSpace(rule.Tags) &&
                rule.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(tag => tag.Trim().ToLower())
                    .Any(tag => lowerCaseTagsToInclude.Contains(tag))
            ).ToList();
            return [.. filteredRules];
        }

        public static ObservableCollection<AccessibilityRuleData> DisableRules(this ObservableCollection<AccessibilityRuleData> source, string[] ruleIDsToDisable)
        {
            if (ruleIDsToDisable == null || !ruleIDsToDisable.Any())
            {
                return source;
            }

            var ruleIDsSet = new HashSet<string>(ruleIDsToDisable, StringComparer.OrdinalIgnoreCase);

            var filteredRules = source.Where(rule => !ruleIDsSet.Contains(rule.RuleID)).ToList();
            return [.. filteredRules];
        }

        public static ObservableCollection<AccessibilityRuleData> WithRules(this ObservableCollection<AccessibilityRuleData> source, string[] ruleIDsToInclude)
        {
            if (ruleIDsToInclude == null || !ruleIDsToInclude.Any())
            {
                return [];
            }

            var ruleIDsSet = new HashSet<string>(ruleIDsToInclude, StringComparer.OrdinalIgnoreCase);

            var filteredRules = source.Where(rule => ruleIDsSet.Contains(rule.RuleID)).ToList();
            return [.. filteredRules];
        }
    }
}
