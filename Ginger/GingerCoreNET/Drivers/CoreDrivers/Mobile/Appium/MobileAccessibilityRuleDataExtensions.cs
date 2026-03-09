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

        public static string[] NormalizeTagNames(string[] tags)
        {
            if (tags == null)
            {
                return tags;
            }

            string[] normalizedTags = new string[tags.Length];
            for (int i = 0; i < tags.Length; i++)
            {
                normalizedTags[i] = tags[i].Equals("bestpractice", StringComparison.OrdinalIgnoreCase)
                    ? "best-practice"
                    : tags[i];
            }
            return normalizedTags;
        }
    }
}
