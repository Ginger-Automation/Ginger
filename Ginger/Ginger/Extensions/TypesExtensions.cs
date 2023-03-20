#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System;
using System.ComponentModel;
using System.Windows.Data;

namespace Ginger.Extensions
{
    public static class TypesExtensions
    {
        // This class is for extension for types like: String, int etc...
        // just an example that we can extend also a regular string to have our own extension method
        // Instead of writing:  if (string.IsnullOrEmpty(str)) ..
        // we can do            if (str.IsnullOrEmpty()) ...
        // a little bit shorter :)

        public static bool IsNullOrEmpty(this string str)
        {
            return String.IsNullOrEmpty(str);
        }

        //TODO: add sort direction flag and enable several columns
        public static ICollectionView AsCollectionViewOrderBy(this IObservableList list, string property)
        {
            ICollectionView cv = CollectionViewSource.GetDefaultView(list);
            SortDescription s = new SortDescription(property, ListSortDirection.Ascending);
            cv.SortDescriptions.Add(s);
            return cv;
        }

    }
}
