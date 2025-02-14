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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.Application_Models
{
    /// <summary>
    /// Utility class for merging collections while preserving items from categories not present in the new data.
    /// </summary>
    public static class CategoryMergingUtils
    {
        /// <summary>
        /// Merges two collections by preserving items from the existing collection whose categories are not present in the latest collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collections.</typeparam>
        /// <param name="existing">The existing collection to merge from.</param>
        /// <param name="latest">The latest collection to merge with.</param>
        /// <param name="categorySelector">A function to extract the category from each item.</param>
        /// <returns>A new collection containing all items from the latest collection and preserved items from the existing collection.</returns>
        public static ObservableList<T> MergeByCategory<T>(
        ObservableList<T> existing,
        ObservableList<T> latest,
        Func<T, ePomElementCategory?> categorySelector)
        {
            var existingCategories = existing?
                .Where(x => categorySelector(x).HasValue)
                .Select(x => categorySelector(x).Value)
                .Distinct() ?? [];

            var newCategories = latest?
                .Where(x => categorySelector(x).HasValue)
                .Select(x => categorySelector(x).Value)
                .Distinct() ?? [];

            var missingCategories = existingCategories
                .Where(c => !newCategories.Contains(c))
                .ToArray();

            var preservedItems = existing?
                .Where(x => categorySelector(x).HasValue &&
                       missingCategories.Contains(categorySelector(x).Value))
                .ToArray() ?? [];

            return [.. (latest ?? []), .. preservedItems];
        }
    }
}
