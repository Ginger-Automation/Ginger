using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Amdocs.Ginger.CoreNET.NewSelfHealing
{
    internal class ElementPropertyMatcher
    {
        private readonly ILogger? _logger;

        internal ElementPropertyMatcher(ILogger? logger = null)
        {
            _logger = logger;
        }

        internal virtual double Match(ElementInfo expected, ElementInfo actual, ePomElementCategory? expectedCategory)
        {

            if (expected == null)
            {
                throw new ArgumentNullException(paramName: nameof(expected));
            }
            if (actual == null)
            {
                throw new ArgumentNullException(paramName: nameof(actual));
            }

            _logger?.LogTrace("matching expected element({expectedElementName}-{expectedElementId}) with actual element({actualElementName}-{actualElementId})", expected.ElementName, expected.Guid, actual.ElementName, actual.Guid);

            List<ControlProperty> expectedProperties = expectedCategory != null ? new((expected.Properties ?? []).Where(p => p != null && p.Category != null && p.Category.Equals(expectedCategory))) : new((expected.Properties ?? []).Where(p => p != null));
            List<ControlProperty> actualProperties = expectedCategory != null ? new((actual.Properties ?? []).Where(p => p != null && p.Category != null && p.Category.Equals(expectedCategory))) : new((actual.Properties ?? []).Where(p => p != null));

            double actualScore = 0;
            double totalScore = 0;

            foreach (ControlProperty expectedProperty in expectedProperties)
            {
                ControlProperty? actualProperty = FindEquivalentProperty(expectedProperty, actualProperties);
                if (actualProperty == null)
                    continue;

                PropertyMatchResult result = MatchProperties(expectedProperty, actualProperty);
                actualScore += result.ActualScore;
                totalScore += result.TotalScore;
            }

            double matchScore = Math.Round(actualScore / totalScore, 2);
            _logger?.LogTrace("expected element({expectedElementName}-{expectedElementId}) and actual element({actualElementName}-{actualElementId}) matched with score {matchScore}", expected.ElementName, expected.Guid, actual.ElementName, actual.Guid, matchScore);
            return matchScore;
        }

        protected virtual ControlProperty? FindEquivalentProperty(ControlProperty expected, IEnumerable<ControlProperty> list)
        {
            if (string.IsNullOrWhiteSpace(expected.Name))
            {
                _logger?.LogTrace("cannot find equivalent property because target property name is not available");
                return null;
            }

            _logger?.LogTrace("finding equivalent property for '{expectedPropName}'", expected.Name);

            foreach (ControlProperty property in list)
            {
                bool hasSameName = string.Equals(expected.Name, property.Name, StringComparison.OrdinalIgnoreCase);
                if (hasSameName)
                {
                    _logger?.LogTrace("found '{actualPropName}' actual property as equivalent to '{expectedPropName}' expected property", property.Name, expected.Name);
                    return property;
                }

                IEnumerable<string> alternateNames = GetAlternatePropertyNames(expected.Name);
                foreach (string alternateName in alternateNames)
                {
                    bool hasSameAlternateName = string.Equals(alternateName, property.Name, StringComparison.OrdinalIgnoreCase);
                    if (hasSameAlternateName)
                    {
                        _logger?.LogTrace("found '{actualPropName}' actual property as equivalent to '{expectedPropName}' expected property based on '{alternateName}' alternate name", property.Name, expected.Name, alternateName);
                        return property;
                    }
                }
            }

            _logger?.LogTrace("no equivalent property found for '{targetPropName}'", expected.Name);
            return null;
        }

        protected virtual IEnumerable<string> GetAlternatePropertyNames(string propertyName)
        {
            //TODO: provide alternate names for certain properties (like iframe, i-frame etc.)
            _logger?.LogTrace("no alternate names available for '{propName}' property", propertyName);
            return [];
        }

        protected virtual PropertyMatchResult MatchProperties(ControlProperty expected, ControlProperty actual)
        {
            double weightage = GetPropertyWeightage(expected.Name);

            bool isCaseSensitiveMatch = string.Equals(expected.Value, actual.Value);
            if (isCaseSensitiveMatch)
            {
                _logger?.LogTrace("expected '{expectedPropName}' property and actual {actualPropName} property value matched with case-sensitivity", expected.Name, actual.Name);
                return new()
                {
                    ActualScore = 1 * weightage,
                    TotalScore = 1 * weightage,
                };
            }

            bool isCaseInsensitiveMatch = string.Equals(expected.Value, actual.Value, StringComparison.OrdinalIgnoreCase);
            if (isCaseInsensitiveMatch)
            {
                _logger?.LogTrace("expected '{expectedPropName}' property and actual {actualPropName} property value matched with case-insensitivity", expected.Name, actual.Name);
                return new()
                {
                    ActualScore = 0.8 * weightage,
                    TotalScore = 1 * weightage,
                };
            }

            _logger?.LogTrace("expected '{expectedPropName}' property and actual {actualPropName} property value didn't match", expected.Name, actual.Name);
            return new()
            {
                ActualScore = 0,
                TotalScore = 1 * weightage,
            };
        }

        protected virtual double GetPropertyWeightage(string? propertyName)
        {
            //TODO: provide specific weightage for certain properties (like id, text, height etc.) for better match calculation
            _logger?.LogTrace("using default property weightage of 1 for property '{propertyName}'", propertyName);
            return 1;
        }

        internal readonly struct PropertyMatchResult : IEquatable<PropertyMatchResult>
        {
            internal required double ActualScore { get; init; }

            internal required double TotalScore { get; init; }

            public bool Equals(PropertyMatchResult other)
            {
                return ActualScore == other.ActualScore && TotalScore == other.TotalScore;
            }
        }
    }
}
