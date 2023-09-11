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
        //public static ICollection<Comparison> Compare<T>(string name, T? localItem, T? remoteItem) where T : RepositoryItemBase
        //{
        //    ICollection<Comparison> comparisons = CompareValue(name, new Value(localItem), new Value(remoteItem));
        //    //State state = comparisons.All(comparison => comparison.State == State.Unmodified) ? State.Unmodified : State.Modified;
        //    //if(state == State.Unmodified)
        //    //    return new ComparisonResult(name, state, comparisons, value: localItem);
        //    //else
        //    //    return new ComparisonResult(name, state, comparisons);
        //    return comparisons;
        //}

        public static ICollection<Comparison> Compare(string name, RepositoryItemBase? localItem, RepositoryItemBase? remoteItem)
        {
            ICollection<Comparison> comparisons = CompareValue(name, new Value(localItem), new Value(remoteItem));
            return comparisons;
        }

        private static ICollection<Comparison> CompareValue(string name, Value? localValue, Value? remoteValue)
        {
            if (IsRepositoryItemBaseValue(localValue, remoteValue))
                return CompareRIBValue(name, localValue, remoteValue);
            else if (IsCollectionValue(localValue, remoteValue))
                return CompareCollectionValue(name, localValue, remoteValue);
            else
                return CompareSimpleValue(name, localValue, remoteValue);
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

        private static ICollection<Comparison> CompareRIBValue(string name, Value? localValue, Value? remoteValue)
        {
            //both local and remote doesn't have value
            if (localValue == null && remoteValue == null)
                return new Comparison[] { new(name, Comparison.StateType.Unmodified, data: null) };

            //remote doesn't have value but, local does
            else if (remoteValue == null)
            {
                //data in local is null
                if (localValue!.Data == null)
                    return new Comparison[] { new(name, Comparison.StateType.Deleted, data: null) };

                //data in local is not null
                else
                {
                    RepositoryItemBase localRIB = (RepositoryItemBase)localValue.Data!;

                    List<Comparison> childComparisons = new();

                    foreach (PropertyInfo property in localRIB.GetType().GetProperties())
                    {
                        if (!IsPropertySerialized(property))
                            continue;
                        object? localPropertyValue = property.GetValue(localRIB);
                        childComparisons.AddRange(CompareValue(property.Name, new Value(localPropertyValue), remoteValue: null));
                    }
                    foreach (FieldInfo field in localRIB.GetType().GetFields())
                    {
                        if (!IsPropertySerialized(field))
                            continue;
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
                    return new Comparison[] { new(name, Comparison.StateType.Added, data: null) };

                //data in remote is not null
                else
                {
                    RepositoryItemBase remoteRIB = (RepositoryItemBase)remoteValue.Data!;

                    List<Comparison> childComparisons = new();

                    foreach (PropertyInfo property in remoteRIB.GetType().GetProperties())
                    {
                        if (!IsPropertySerialized(property))
                            continue;
                        object? remotePropertyValue = property.GetValue(remoteRIB);
                        childComparisons.AddRange(CompareValue(property.Name, localValue: null, new Value(remotePropertyValue)));
                    }
                    foreach (FieldInfo field in remoteRIB.GetType().GetFields())
                    {
                        if (!IsPropertySerialized(field))
                            continue;
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
                    return new Comparison[] { new(name, Comparison.StateType.Unmodified, data: null) };

                //data in local is null
                else if (localRIB == null)
                    return CompareValue(name, localValue: null, remoteValue);

                //data in remote is null
                else if (remoteRIB == null)
                    return CompareValue(name, localValue, remoteValue: null);

                //data in both local and remote is not null
                else
                {
                    if (localRIB.Guid == remoteRIB.Guid)
                    {
                        Type seniorType = GetSeniorType(localRIB.GetType(), remoteRIB.GetType());
                        List<Comparison> childComparisons = new();
                        foreach (PropertyInfo property in seniorType.GetProperties())
                        {
                            if (!IsPropertySerialized(property))
                                continue;
                            childComparisons.AddRange(
                                CompareValue(
                                    name: property.Name,
                                    localValue: new Value(property.GetValue(localRIB)),
                                    remoteValue: new Value(property.GetValue(remoteRIB))));
                        }
                        foreach (FieldInfo field in seniorType.GetFields())
                        {
                            if (!IsPropertySerialized(field))
                                continue;
                            childComparisons.AddRange(
                                CompareValue(
                                    name: field.Name,
                                    localValue: new Value(field.GetValue(localRIB)),
                                    remoteValue: new Value(field.GetValue(remoteRIB))));
                        }
                        Comparison.StateType state = childComparisons.All(c => c.State == Comparison.StateType.Unmodified) ? Comparison.StateType.Unmodified : Comparison.StateType.Modified;
                        if (state == Comparison.StateType.Unmodified)
                            return new Comparison[] { new(name, state, childComparisons, localRIB) };
                        else
                            return new Comparison[] { new(name, state, childComparisons, dataType: seniorType) };
                    }
                    else
                    {
                        List<Comparison> localChildComparisons = new();
                        foreach (PropertyInfo property in localRIB.GetType().GetProperties())
                        {
                            if (!IsPropertySerialized(property))
                                continue;
                            localChildComparisons.AddRange(
                                CompareValue(
                                    name: property.Name,
                                    localValue: new Value(property.GetValue(localRIB)),
                                    remoteValue: null));
                        }
                        foreach (FieldInfo field in localRIB.GetType().GetFields())
                        {
                            if (!IsPropertySerialized(field))
                                continue;
                            localChildComparisons.AddRange(
                                CompareValue(
                                    name: field.Name,
                                    localValue: new Value(field.GetValue(localRIB)),
                                    remoteValue: null));
                        }
                        Comparison localComparisonResult = new(name, state: Comparison.StateType.Deleted, localChildComparisons, data: localRIB);

                        List<Comparison> remoteChildComparisons = new();
                        foreach (PropertyInfo property in remoteRIB.GetType().GetProperties())
                        {
                            if (!IsPropertySerialized(property))
                                continue;
                            remoteChildComparisons.AddRange(
                                CompareValue(
                                    name: property.Name,
                                    localValue: null,
                                    remoteValue: new Value(property.GetValue(remoteRIB))));
                        }
                        foreach (FieldInfo field in remoteRIB.GetType().GetFields())
                        {
                            if (!IsPropertySerialized(field))
                                continue;
                            remoteChildComparisons.AddRange(
                                CompareValue(
                                    name: field.Name,
                                    localValue: null,
                                    remoteValue: new Value(field.GetValue(remoteRIB))));
                        }
                        Comparison remoteComparisonResult = new(name, state: Comparison.StateType.Added, remoteChildComparisons, data: remoteRIB);

                        return new Comparison[] { localComparisonResult, remoteComparisonResult };
                    }
                }
            }
        }

        private static Type GetSeniorType(Type type1, Type type2)
        {
            Type seniorType;
            if (type1.IsAssignableTo(type2))
                seniorType = type2;
            else
                seniorType = type1;

            return seniorType;
        }

        private static bool IsPropertySerialized(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) != null;
        }

        private static bool IsPropertySerialized(FieldInfo field)
        {
            return field.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) != null;
        }

        private static ICollection<Comparison> CompareCollectionValue(string name, Value? localValue, Value? remoteValue)
        {
            //both local and remote doesn't have value
            if (localValue == null && remoteValue == null)
                return new Comparison[] { new(name, state: Comparison.StateType.Unmodified, data: null) };

            //local doesn't have value but, remote does
            else if (localValue == null)
            {
                //data in remote is null
                if (remoteValue!.Data == null)
                    return new Comparison[] { new(name, Comparison.StateType.Added, data: null) };

                //data in remote is not null
                else
                {
                    System.Collections.ICollection remoteCollection = (System.Collections.ICollection)remoteValue.Data!;

                    List<Comparison> childComparisons = new();

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
                    return new Comparison[] { new(name, Comparison.StateType.Deleted, data: null) };

                //data in local is not null
                else
                {
                    System.Collections.ICollection localCollection = (System.Collections.ICollection)localValue.Data!;

                    List<Comparison> childComparisons = new();

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
                    return new Comparison[] { new(name, Comparison.StateType.Unmodified, data: null) };

                //data in local is null
                else if (localCollection == null)
                    return CompareValue(name, localValue: null, remoteValue);

                //data in remote is null
                else if (remoteCollection == null)
                    return CompareValue(name, localValue, remoteValue: null);

                List<Comparison> childComparisons = new();
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
                    return new Comparison[] { new(name, state, childComparisons, localCollection) };
                else
                    return new Comparison[] { new(name, state, childComparisons, dataType: GetSeniorType(localCollection.GetType(), remoteCollection.GetType())) };
            }
        }

        private static ICollection<Comparison> CompareSimpleValue(string name, Value? localValue, Value? remoteValue)
        {
            if (localValue == null && remoteValue == null)
                return new Comparison[] { new(name, state: Comparison.StateType.Unmodified, data: null) };
            else if (localValue == null)
                return new Comparison[] { new(name, state: Comparison.StateType.Added, data: remoteValue!.Data) };
            else if (remoteValue == null)
                return new Comparison[] { new(name, state: Comparison.StateType.Deleted, data: localValue!.Data) };
            else
            {
                if (string.Equals(localValue!.Data?.ToString(), remoteValue!.Data?.ToString()))
                    return new Comparison[] { new(name, state: Comparison.StateType.Unmodified, data: localValue!.Data) };
                else
                    return new List<Comparison>()
                    {
                        new(name, state: Comparison.StateType.Deleted, data: localValue!.Data),
                        new(name, state: Comparison.StateType.Added, data: remoteValue!.Data)
                    };
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
