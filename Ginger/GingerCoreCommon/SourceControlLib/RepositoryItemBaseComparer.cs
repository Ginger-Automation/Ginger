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
    public static class RepositoryItemBaseComparer
    {
        public static ICollection<Comparison> Compare(string name, RepositoryItemBase? localItem, RepositoryItemBase? remoteItem)
        {
            ICollection<Comparison> comparisons = Array.Empty<Comparison>();
            if (localItem != null || remoteItem != null)
            {
                comparisons = CompareValue(name, new Value(localItem), new Value(remoteItem));
            }
            return comparisons;
        }

        private static ICollection<Comparison> CompareValue(string name, Value? localValue, Value? remoteValue)
        {
            if (IsRepositoryItemBaseValue(localValue, remoteValue))
            {
                return CompareRIBValue(name, localValue, remoteValue);
            }
            else if (IsCollectionValue(localValue, remoteValue))
            {
                return CompareCollectionValue(name, localValue, remoteValue);
            }
            else if (IsRepositoryItemHeader(localValue, remoteValue))
            {
                return CompareRIBHeaderValue(name, localValue, remoteValue);
            }
            else
            {
                return CompareSimpleValue(name, localValue, remoteValue);
            }
        }

        private static bool IsRepositoryItemBaseValue(Value? localValue, Value? remoteValue)
        {
            Type? localValueType = localValue?.Data?.GetType();
            Type? remoteValueType = remoteValue?.Data?.GetType();

            return
                (localValueType != null && localValueType.IsAssignableTo(typeof(RepositoryItemBase))) ||
                (remoteValueType != null && remoteValueType.IsAssignableTo(typeof(RepositoryItemBase)));
        }

        private static bool IsCollectionValue(Value? localValue, Value? remoteValue)
        {
            Type? localValueType = localValue?.Data?.GetType();
            Type? remoteValueType = remoteValue?.Data?.GetType();

            return
                (localValueType != null && localValueType.IsAssignableTo(typeof(System.Collections.ICollection))) ||
                (remoteValueType != null && remoteValueType.IsAssignableTo(typeof(System.Collections.ICollection)));
        }

        private static bool IsRepositoryItemHeader(Value? localValue, Value? remoteValue)
        {
            Type? localValueType = localValue?.Data?.GetType();
            Type? remoteValueType = remoteValue?.Data?.GetType();

            return
                (localValueType != null && localValueType.IsAssignableTo(typeof(RepositoryItemHeader))) ||
                (remoteValueType != null && remoteValueType.IsAssignableTo(typeof(RepositoryItemHeader)));
        }

        private static ICollection<Comparison> CompareRIBValue(string name, Value? localValue, Value? remoteValue)
        {
            //both local and remote doesn't have value
            if (localValue == null && remoteValue == null)
            {
                return new Comparison[] { new(name, Comparison.StateType.Unmodified, data: null) };
            }
            //remote doesn't have value but, local does
            else if (remoteValue == null)
            {
                //data in local is null
                if (localValue!.Data == null)
                {
                    return new Comparison[] { new(name, Comparison.StateType.Deleted, data: null) };
                }
                //data in local is not null
                else
                {
                    RepositoryItemBase localRIB = (RepositoryItemBase)localValue.Data!;

                    List<Comparison> childComparisons = [];

                    foreach (PropertyInfo property in localRIB.GetType().GetProperties())
                    {
                        if (!IsPropertySerialized(property))
                        {
                            continue;
                        }
                        object? localPropertyValue = property.GetValue(localRIB);
                        childComparisons.AddRange(CompareValue(property.Name, new Value(localPropertyValue), remoteValue: null));
                    }
                    foreach (FieldInfo field in localRIB.GetType().GetFields())
                    {
                        if (!IsPropertySerialized(field))
                        {
                            continue;
                        }
                        object? localPropertyValue = field.GetValue(localRIB);
                        childComparisons.AddRange(CompareValue(field.Name, new Value(localPropertyValue), remoteValue: null));
                    }
                    return new Comparison[] { new(name, state: Comparison.StateType.Deleted, childComparisons, data: localRIB) };
                }
            }

            //local doesn't have value but, remote does
            else if (localValue == null)
            {
                //data in remote is null
                if (remoteValue!.Data == null)
                {
                    return new Comparison[] { new(name, Comparison.StateType.Added, data: null) };
                }
                //data in remote is not null
                else
                {
                    RepositoryItemBase remoteRIB = (RepositoryItemBase)remoteValue.Data!;

                    List<Comparison> childComparisons = [];

                    foreach (PropertyInfo property in remoteRIB.GetType().GetProperties())
                    {
                        if (!IsPropertySerialized(property))
                        {
                            continue;
                        }
                        object? remotePropertyValue = property.GetValue(remoteRIB);
                        childComparisons.AddRange(CompareValue(property.Name, localValue: null, new Value(remotePropertyValue)));
                    }
                    foreach (FieldInfo field in remoteRIB.GetType().GetFields())
                    {
                        if (!IsPropertySerialized(field))
                        {
                            continue;
                        }
                        object? remotePropertyValue = field.GetValue(remoteRIB);
                        childComparisons.AddRange(CompareValue(field.Name, localValue: null, new Value(remotePropertyValue)));
                    }
                    return new Comparison[] { new(name, state: Comparison.StateType.Added, childComparisons, data: remoteRIB) };
                }
            }

            //both local and remote have value
            else
            {
                RepositoryItemBase localRIB = (RepositoryItemBase)localValue.Data!;
                RepositoryItemBase remoteRIB = (RepositoryItemBase)remoteValue.Data!;

                //data in both local and remote is null
                if (localRIB == null && remoteRIB == null)
                {
                    return new Comparison[] { new(name, Comparison.StateType.Unmodified, data: null) };
                }
                //data in local is null
                else if (localRIB == null)
                {
                    return CompareValue(name, localValue: null, remoteValue);
                }
                //data in remote is null
                else if (remoteRIB == null)
                {
                    return CompareValue(name, localValue, remoteValue: null);
                }
                //data in both local and remote is not null
                else
                {
                    if (localRIB.Guid == remoteRIB.Guid)
                    {
                        Type seniorType = GetSeniorType(localRIB.GetType(), remoteRIB.GetType());
                        List<Comparison> childComparisons = [];
                        foreach (PropertyInfo property in seniorType.GetProperties())
                        {
                            if (!IsPropertySerialized(property))
                            {
                                continue;
                            }
                            childComparisons.AddRange(
                                CompareValue(
                                    name: property.Name,
                                    localValue: new Value(property.GetValue(localRIB)),
                                    remoteValue: new Value(property.GetValue(remoteRIB))));
                        }
                        foreach (FieldInfo field in seniorType.GetFields())
                        {
                            if (!IsPropertySerialized(field))
                            {
                                continue;
                            }
                            childComparisons.AddRange(
                                CompareValue(
                                    name: field.Name,
                                    localValue: new Value(field.GetValue(localRIB)),
                                    remoteValue: new Value(field.GetValue(remoteRIB))));
                        }
                        Comparison.StateType state = childComparisons.All(c => c.State == Comparison.StateType.Unmodified) ? Comparison.StateType.Unmodified : Comparison.StateType.Modified;
                        if (state == Comparison.StateType.Unmodified)
                        {
                            return new Comparison[] { new(name, state, childComparisons, localRIB) };
                        }
                        else
                        {
                            return new Comparison[] { new(name, state, childComparisons, dataType: seniorType) };
                        }
                    }
                    else
                    {
                        List<Comparison> localChildComparisons = [];
                        foreach (PropertyInfo property in localRIB.GetType().GetProperties())
                        {
                            if (!IsPropertySerialized(property))
                            {
                                continue;
                            }
                            localChildComparisons.AddRange(
                                CompareValue(
                                    name: property.Name,
                                    localValue: new Value(property.GetValue(localRIB)),
                                    remoteValue: null));
                        }
                        foreach (FieldInfo field in localRIB.GetType().GetFields())
                        {
                            if (!IsPropertySerialized(field))
                            {
                                continue;
                            }
                            localChildComparisons.AddRange(
                                CompareValue(
                                    name: field.Name,
                                    localValue: new Value(field.GetValue(localRIB)),
                                    remoteValue: null));
                        }
                        Comparison localComparisonResult = new(name, state: Comparison.StateType.Deleted, localChildComparisons, data: localRIB);

                        List<Comparison> remoteChildComparisons = [];
                        foreach (PropertyInfo property in remoteRIB.GetType().GetProperties())
                        {
                            if (!IsPropertySerialized(property))
                            {
                                continue;
                            }
                            remoteChildComparisons.AddRange(
                                CompareValue(
                                    name: property.Name,
                                    localValue: null,
                                    remoteValue: new Value(property.GetValue(remoteRIB))));
                        }
                        foreach (FieldInfo field in remoteRIB.GetType().GetFields())
                        {
                            if (!IsPropertySerialized(field))
                            {
                                continue;
                            }
                            remoteChildComparisons.AddRange(
                                CompareValue(
                                    name: field.Name,
                                    localValue: null,
                                    remoteValue: new Value(field.GetValue(remoteRIB))));
                        }
                        Comparison remoteComparisonResult = new(name, state: Comparison.StateType.Added, remoteChildComparisons, data: remoteRIB);

                        localComparisonResult.SetSiblingComparison(remoteComparisonResult);

                        return new Comparison[] { localComparisonResult, remoteComparisonResult };
                    }
                }
            }
        }

        private static Type GetSeniorType(Type type1, Type type2)
        {
            Type seniorType;
            if (type1.IsAssignableTo(type2))
            {
                seniorType = type2;
            }
            else
            {
                seniorType = type1;
            }
            return seniorType;
        }

        private static bool IsPropertySerialized(PropertyInfo property)
        {
            return
                property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) != null ||
                string.Equals(property.Name, nameof(RepositoryItemBase.RepositoryItemHeader));
        }

        private static bool IsPropertySerialized(FieldInfo field)
        {
            return field.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) != null;
        }

        private static ICollection<Comparison> CompareCollectionValue(string name, Value? localValue, Value? remoteValue)
        {
            //both local and remote doesn't have value
            if (localValue == null && remoteValue == null)
            {
                return new Comparison[] { new(name, state: Comparison.StateType.Unmodified, data: null) };
            }
            //local doesn't have value but, remote does
            else if (localValue == null)
            {
                //data in remote is null
                if (remoteValue!.Data == null)
                {
                    return new Comparison[] { new(name, Comparison.StateType.Added, data: null) };
                }
                //data in remote is not null
                else
                {
                    System.Collections.ICollection remoteCollection = (System.Collections.ICollection)remoteValue.Data!;

                    List<Comparison> childComparisons = [];

                    int index = 0;
                    foreach (object? item in remoteCollection)
                    {
                        childComparisons.AddRange(
                            CompareValue(
                                name: $"[{index++}]",
                                localValue: null,
                                remoteValue: new Value(item)));
                    }
                    return new Comparison[] { new(name, state: Comparison.StateType.Added, childComparisons, data: remoteCollection) };
                }
            }

            //remote doesn't have value but, local does
            else if (remoteValue == null)
            {
                //data in local is null
                if (localValue!.Data == null)
                {
                    return new Comparison[] { new(name, Comparison.StateType.Deleted, data: null) };
                }
                //data in local is not null
                else
                {
                    System.Collections.ICollection localCollection = (System.Collections.ICollection)localValue.Data!;

                    List<Comparison> childComparisons = [];

                    int index = 0;
                    foreach (object? item in localCollection)
                    {
                        childComparisons.AddRange(
                            CompareValue(
                                name: $"[{index++}]",
                                localValue: new Value(item),
                                remoteValue: null));
                    }
                    return new Comparison[] { new(name, state: Comparison.StateType.Deleted, childComparisons, data: localCollection) };
                }
            }

            //both local and remote have value
            else
            {
                System.Collections.ICollection localCollection = (System.Collections.ICollection)localValue.Data!;
                System.Collections.ICollection remoteCollection = (System.Collections.ICollection)remoteValue.Data!;

                //data in both local and remote is null
                if (localCollection == null && remoteCollection == null)
                {
                    return new Comparison[] { new(name, Comparison.StateType.Unmodified, data: null) };
                }
                //data in local is null
                else if (localCollection == null)
                {
                    return CompareValue(name, localValue: null, remoteValue);
                }
                //data in remote is null
                else if (remoteCollection == null)
                {
                    return CompareValue(name, localValue, remoteValue: null);
                }

                List<Comparison> childComparisons = [];
                int index = 0;
                System.Collections.IEnumerator localCollectionEnumerator = localCollection.GetEnumerator();
                System.Collections.IEnumerator remoteCollectionEnumerator = remoteCollection.GetEnumerator();

                bool localCollectionHasValue = localCollectionEnumerator.MoveNext();
                bool remoteCollectionHasValue = remoteCollectionEnumerator.MoveNext();

                while (localCollectionHasValue || remoteCollectionHasValue)
                {
                    //TODO: make Value null if not available in collection
                    object? localItem = localCollectionHasValue ? localCollectionEnumerator.Current : null;
                    object? remoteItem = remoteCollectionHasValue ? remoteCollectionEnumerator.Current : null;

                    childComparisons.AddRange(
                        CompareValue(
                            name: $"[{index++}]",
                            localValue: new Value(localItem),
                            remoteValue: new Value(remoteItem)));

                    localCollectionHasValue = localCollectionEnumerator.MoveNext();
                    remoteCollectionHasValue = remoteCollectionEnumerator.MoveNext();
                }
                Comparison.StateType state = childComparisons.All(c => c.State == Comparison.StateType.Unmodified) ? Comparison.StateType.Unmodified : Comparison.StateType.Modified;
                if (state == Comparison.StateType.Unmodified)
                {
                    return new Comparison[] { new(name, state, childComparisons, localCollection) };
                }
                else
                {
                    return new Comparison[] { new(name, state, childComparisons, dataType: GetSeniorType(localCollection.GetType(), remoteCollection.GetType())) };
                }
            }
        }

        private static ICollection<Comparison> CompareRIBHeaderValue(string name, Value? localValue, Value? remoteValue)
        {
            //both local and remote doesn't have value
            if (localValue == null && remoteValue == null)
            {
                return new Comparison[] { new(name, Comparison.StateType.Unmodified, data: null) };
            }
            //remote doesn't have value but, local does
            else if (remoteValue == null)
            {
                //data in local is null
                if (localValue!.Data == null)
                {
                    Comparison comparison = new(name, Comparison.StateType.Deleted, data: null)
                    {
                        Selected = true,
                        IsSelectionEnabled = false
                    };
                    return new Comparison[] { comparison };
                }
                //data in local is not null
                else
                {
                    RepositoryItemHeader localRIH = (RepositoryItemHeader)localValue.Data!;
                    Comparison comparison = new(name, state: Comparison.StateType.Deleted, data: localRIH)
                    {
                        Selected = true,
                        IsSelectionEnabled = false
                    };
                    return new Comparison[] { comparison };
                }
            }

            //local doesn't have value but, remote does
            else if (localValue == null)
            {
                //data in remote is null
                if (remoteValue!.Data == null)
                {
                    Comparison comparison = new(name, Comparison.StateType.Added, data: null)
                    {
                        Selected = true,
                        IsSelectionEnabled = false
                    };
                    return new Comparison[] { comparison };
                }
                //data in remote is not null
                else
                {
                    RepositoryItemHeader remoteRIH = (RepositoryItemHeader)remoteValue.Data!;
                    Comparison comparison = new(name, state: Comparison.StateType.Added, remoteRIH)
                    {
                        Selected = true,
                        IsSelectionEnabled = false
                    };
                    return new Comparison[] { comparison };
                }
            }

            //both local and remote have value
            else
            {
                RepositoryItemHeader localRIH = (RepositoryItemHeader)localValue.Data!;
                RepositoryItemHeader remoteRIH = (RepositoryItemHeader)remoteValue.Data!;

                //data in both local and remote is null
                if (localRIH == null && remoteRIH == null)
                {
                    return new Comparison[] { new(name, Comparison.StateType.Unmodified, data: null) };
                }
                //data in local is null
                else if (localRIH == null)
                {
                    return CompareValue(name, localValue: null, remoteValue);
                }
                //data in remote is null
                else if (remoteRIH == null)
                {
                    return CompareValue(name, localValue, remoteValue: null);
                }
                //data in both local and remote is not null
                else
                {
                    if (RIBHeaderEquals(localRIH, remoteRIH))
                    {
                        return new Comparison[] { new(name, Comparison.StateType.Unmodified, data: remoteRIH) };
                    }
                    else
                    {
                        Comparison localComparison = new(name, Comparison.StateType.Deleted, data: localRIH);
                        Comparison remoteComparison = new(name, Comparison.StateType.Added, data: remoteRIH);
                        if (localRIH.LastUpdate > remoteRIH.LastUpdate)
                        {
                            localComparison.Selected = true;
                        }
                        else
                        {
                            remoteComparison.Selected = true;
                        }
                        localComparison.IsSelectionEnabled = false;
                        remoteComparison.IsSelectionEnabled = false;
                        return new Comparison[] { localComparison, remoteComparison };
                    }
                }
            }
        }

        private static bool RIBHeaderEquals(RepositoryItemHeader localRIH, RepositoryItemHeader remoteRIH)
        {
            bool areRIBHeadersEqual = true;
            foreach (PropertyInfo property in typeof(RepositoryItemHeader).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                object? localPropertyValue = property.GetValue(localRIH);
                object? remotePropertyValue = property.GetValue(remoteRIH);
                if (localRIH != null && !localRIH.Equals(remoteRIH))
                {
                    areRIBHeadersEqual = false;
                    break;
                }
            }

            return areRIBHeadersEqual;
        }

        private static ICollection<Comparison> CompareSimpleValue(string name, Value? localValue, Value? remoteValue)
        {
            if (localValue == null && remoteValue == null)
            {
                return new Comparison[] { new(name, state: Comparison.StateType.Unmodified, data: null) };
            }
            else if (localValue == null)
            {
                return new Comparison[] { new(name, state: Comparison.StateType.Added, data: remoteValue!.Data) };
            }
            else if (remoteValue == null)
            {
                return new Comparison[] { new(name, state: Comparison.StateType.Deleted, data: localValue!.Data) };
            }
            else
            {
                if (string.Equals(localValue!.Data?.ToString(), remoteValue!.Data?.ToString()))
                {
                    return new Comparison[] { new(name, state: Comparison.StateType.Unmodified, data: localValue!.Data) };
                }
                else
                {
                    Comparison localComparison = new(name, state: Comparison.StateType.Deleted, data: localValue!.Data);
                    Comparison remoteComparison = new(name, state: Comparison.StateType.Added, data: remoteValue!.Data);

                    localComparison.SetSiblingComparison(remoteComparison);

                    return
                    [
                        localComparison,
                        remoteComparison
                    ];
                }
            }
        }
    }

    public sealed class Value
    {
        public object? Data { get; }

        public Value(object? data)
        {
            Data = data;
        }
    }
}
