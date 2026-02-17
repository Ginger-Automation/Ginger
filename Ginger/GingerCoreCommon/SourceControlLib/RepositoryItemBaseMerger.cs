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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amdocs.Ginger.Repository;

#nullable enable
namespace Amdocs.Ginger.Common.SourceControlLib
{
    public static class RepositoryItemBaseMerger
    {
        public static T? Merge<T>(ICollection<Comparison> comparisonResult) where T : RepositoryItemBase
        {
            return (T?)MergeInternal(typeof(T), comparisonResult);
        }

        public static RepositoryItemBase? Merge(Type type, ICollection<Comparison> comparisonResult)
        {
            return (RepositoryItemBase?)MergeInternal(type, comparisonResult);
        }

        private static object? MergeInternal(Type type, ICollection<Comparison> comparisonResult)
        {
            if (type.IsAssignableTo(typeof(RepositoryItemBase)))
            {
                return MergeRIB(type, comparisonResult);
            }
            else if (type.IsAssignableTo(typeof(System.Collections.ICollection)))
            {
                return MergeCollection(type, comparisonResult);
            }
            else
            {
                return MergeSimpleValue(type, comparisonResult);
            }
        }

        private static RepositoryItemBase? MergeRIB(Type type, ICollection<Comparison> ribComparisonResult)
        {
            if (!type.IsAssignableTo(typeof(RepositoryItemBase)))
            {
                throw new InvalidOperationException($"Expected a {typeof(RepositoryItemBase).FullName} type.");
            }

            RepositoryItemBase? rib = null;

            Comparison? comparisonForMerge = ribComparisonResult
                .FirstOrDefault(c => (c.State == Comparison.StateType.Unmodified) || ((c.State == Comparison.StateType.Added || c.State == Comparison.StateType.Deleted) && c.Selected));

            if (comparisonForMerge != null)
            {
                rib = (RepositoryItemBase?)comparisonForMerge.Data;
            }
            else
            {
                comparisonForMerge = ribComparisonResult
                    .FirstOrDefault(childComparison => childComparison.State == Comparison.StateType.Modified);

                if (comparisonForMerge == null)
                {
                    //in case of a Modify-Delete conflict, user might choose to not keep the modified file, so there is no comparison to merge.
                    return null;
                }

                rib = (RepositoryItemBase?)Activator.CreateInstance(type);

                foreach (PropertyInfo property in type.GetProperties())
                {
                    if (property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                    {
                        continue;
                    }

                    string propertyName = property.Name;

                    IEnumerable<Comparison> propertyComparisons = comparisonForMerge.ChildComparisons
                        .Where(c => string.Equals(c.Name, property.Name));

                    Comparison? propertyComparisonForMerge = propertyComparisons
                        .FirstOrDefault(c => c.State == Comparison.StateType.Unmodified || ((c.State == Comparison.StateType.Added || c.State == Comparison.StateType.Deleted) && c.Selected));

                    if (propertyComparisonForMerge != null)
                    {
                        property.SetValue(rib, propertyComparisonForMerge.Data);
                    }
                    else
                    {
                        propertyComparisonForMerge = propertyComparisons
                            .FirstOrDefault(c => c.State == Comparison.StateType.Modified);

                        if (propertyComparisonForMerge == null)
                        {
                            throw new InvalidOperationException($"No appropriate {nameof(Comparison)} found for merging property {property.Name}.");
                        }

                        property.SetValue(rib, MergeInternal(propertyComparisonForMerge.DataType!, new Comparison[] { propertyComparisonForMerge }));
                    }
                }
                foreach (FieldInfo field in type.GetFields())
                {
                    if (field.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                    {
                        continue;
                    }

                    string propertyName = field.Name;

                    IEnumerable<Comparison> fieldComparisons = comparisonForMerge.ChildComparisons
                        .Where(c => string.Equals(c.Name, field.Name));

                    Comparison? fieldComparisonForMerge = fieldComparisons
                        .FirstOrDefault(c => c.State == Comparison.StateType.Unmodified || ((c.State == Comparison.StateType.Added || c.State == Comparison.StateType.Deleted) && c.Selected));

                    if (fieldComparisonForMerge != null)
                    {
                        field.SetValue(rib, fieldComparisonForMerge.Data);
                    }
                    else
                    {
                        fieldComparisonForMerge = fieldComparisons
                            .FirstOrDefault(c => c.State == Comparison.StateType.Modified);

                        if (fieldComparisonForMerge == null)
                        {
                            throw new InvalidOperationException($"No appropriate {nameof(Comparison)} found for merging field {field.Name}.");
                        }
                        field.SetValue(rib, MergeInternal(fieldComparisonForMerge.DataType!, new Comparison[] { fieldComparisonForMerge }));
                    }
                }
            }

            return rib;
        }

        private static System.Collections.ICollection? MergeCollection(Type type, ICollection<Comparison> collectionComparisonResult)
        {
            if (!type.IsAssignableTo(typeof(System.Collections.ICollection)))
            {
                throw new InvalidOperationException($"Expected a {typeof(System.Collections.ICollection).FullName} type.");
            }

            System.Collections.ICollection? collection = null;

            Comparison? comparisonForMerge = collectionComparisonResult
                .FirstOrDefault(c => (c.State == Comparison.StateType.Unmodified) || ((c.State == Comparison.StateType.Added || c.State == Comparison.StateType.Deleted) && c.Selected));

            if (comparisonForMerge != null)
            {
                collection = (System.Collections.ICollection?)comparisonForMerge.Data;
            }
            else
            {
                comparisonForMerge = collectionComparisonResult
                    .FirstOrDefault(childComparison => childComparison.State == Comparison.StateType.Modified);

                if (comparisonForMerge == null)
                {
                    throw new InvalidOperationException($"No appropriate {nameof(Comparison)} found for merging type {type.FullName}.");
                }

                System.Collections.ArrayList items = [];

                foreach (Comparison itemComparison in comparisonForMerge.ChildComparisons)
                {
                    if (itemComparison.State == Comparison.StateType.Unmodified)
                    {
                        items.Add(itemComparison.Data);
                    }
                    else if (itemComparison.State is Comparison.StateType.Added or Comparison.StateType.Deleted)
                    {
                        if (itemComparison.Selected && IsUniqueRIBItem(items, itemComparison.Data))
                        {
                            items.Add(itemComparison.Data);
                        }
                    }
                    else if (itemComparison.State == Comparison.StateType.Modified)
                    {
                        items.Add(MergeInternal(itemComparison.DataType!, new Comparison[] { itemComparison }));
                    }
                    else
                    {
                        throw new InvalidOperationException($"No appropriate {nameof(Comparison)} found for merging at index {items.Count}.");
                    }
                }

                collection = CreateCollectionOfType(type, items);
            }


            return collection;
        }

        private static bool IsUniqueRIBItem(System.Collections.IEnumerable itemCollection, object? item)
        {
            if (item is null or not RepositoryItemBase)
            {
                return true;
            }

            RepositoryItemBase ribItem = (RepositoryItemBase)item;

            bool isUnique = true;
            foreach (object? collectionItem in itemCollection)
            {
                if (collectionItem is RepositoryItemBase ribCollectionItem && ribCollectionItem.Guid == ribItem.Guid)
                {
                    isUnique = false;
                    break;
                }
            }

            return isUnique;
        }

        private static Type GetCollectionItemType(Type collectionType)
        {
            Type collectionItemType;
            if (collectionType.GetGenericArguments().Length > 0)
            {
                collectionItemType = collectionType.GetGenericArguments()[0];
            }
            else
            {
                collectionItemType = typeof(object);
            }

            return collectionItemType;
        }

        private static System.Collections.ICollection CreateCollectionOfType(Type type, System.Collections.ArrayList items)
        {
            System.Collections.ICollection collection;
            if (IsIList(type))
            {
                ConstructorInfo? constructor = type.GetConstructors()
                    .FirstOrDefault(c =>
                        c.GetParameters().Length == 1 &&
                        c.GetParameters()[0].ParameterType.IsAssignableTo(typeof(System.Collections.IEnumerable)));

                if (constructor != null)
                {
                    ParameterInfo parameter = constructor.GetParameters()[0];
                    Type[] parameterGenericArguments = parameter.ParameterType.GetGenericArguments();
                    if (parameterGenericArguments.Length > 0)
                    {
                        Type genericListType = typeof(List<>).MakeGenericType(parameterGenericArguments);

                        object itemsAsGenericIEnumerable = genericListType.GetConstructor(Type.EmptyTypes)!.Invoke(Array.Empty<object?>());

                        foreach (object item in items)
                        {
                            MethodInfo addMethod = genericListType.GetMethod(nameof(System.Collections.IList.Add))!;
                            addMethod.Invoke(itemsAsGenericIEnumerable, new object[] { item });
                        }

                        collection = (System.Collections.ICollection)constructor.Invoke(new object[] { itemsAsGenericIEnumerable });
                    }
                    else
                    {
                        collection = (System.Collections.ICollection)constructor.Invoke(new object[] { items });
                    }
                }
                else
                {
                    constructor = type.GetConstructor(Type.EmptyTypes);

                    if (constructor == null)
                    {
                        throw new NotImplementedException($"No appropriate constructor found for collection of type {type.FullName}");
                    }
                    collection = (System.Collections.ICollection)constructor.Invoke(Array.Empty<object>());
                    MethodInfo addMethod = type.GetMethod(nameof(System.Collections.IList.Add))!;

                    foreach (object item in items)
                    {
                        addMethod.Invoke(collection, new object[] { item });
                    }
                }
            }
            else
            {
                throw new NotImplementedException($"Cannot create collection of type {type.FullName}");
            }

            return collection;
        }

        private static bool IsIList(Type type)
        {
            return type.IsAssignableTo(typeof(System.Collections.IList));
        }

        private static object? MergeSimpleValue(Type type, ICollection<Comparison> simpleValueComparisonResult)
        {
            object? value = null;

            Comparison? comparisonForMerge = simpleValueComparisonResult
                .FirstOrDefault(c => (c.State == Comparison.StateType.Unmodified) || ((c.State == Comparison.StateType.Added || c.State == Comparison.StateType.Deleted) && c.Selected));

            if (comparisonForMerge == null)
            {
                throw new InvalidOperationException($"No appropriate {nameof(Comparison)} found for merging value of type {type.Name}.");
            }

            value = comparisonForMerge.Data;

            return value;
        }
    }
}
