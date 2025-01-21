#region License
/*
Copyright © 2014-2024 European Support Limited

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
    public static class CategoryMergingUtils
    {
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
