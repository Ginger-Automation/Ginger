using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Ginger.ConflictResolve
{
    public static class RIBCompare
    {

        public static ICollection<Comparison> Compare<T>(string name, T? localItem, T? remoteItem) where T : RepositoryItemBase
        {
            ICollection<Comparison> comparisons = CompareValue(name, new Value(localItem), new Value(remoteItem));
            //State state = comparisons.All(comparison => comparison.State == State.Unmodified) ? State.Unmodified : State.Modified;
            //if(state == State.Unmodified)
            //    return new ComparisonResult(name, state, comparisons, value: localItem);
            //else
            //    return new ComparisonResult(name, state, comparisons);
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
                return new Comparison[] { new(name, State.Unmodified, data: null) };

            //remote doesn't have value but, local does
            else if (remoteValue == null)
            {
                //data in local is null
                if (localValue!.Data == null)
                    return new Comparison[] { new(name, State.Deleted, data: null) };
                
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
                    return new Comparison[] { new(name, state: State.Deleted, childComparisons, data: localRIB) };
                }
            }

            //local doesn't have value but, remote does
            else if (localValue == null)
            {
                //data in remote is null
                if (remoteValue!.Data == null)
                    return new Comparison[] { new(name, State.Added, data: null) };
                
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
                        childComparisons.AddRange(CompareValue(property.Name, localValue: null, new Value(remoteValue)));
                    }
                    foreach (FieldInfo field in remoteRIB.GetType().GetFields())
                    {
                        if (!IsPropertySerialized(field))
                            continue;
                        object? remotePropertyValue = field.GetValue(remoteRIB);
                        childComparisons.AddRange(CompareValue(field.Name, localValue: null, new Value(remoteValue)));
                    }
                    return new Comparison[] { new(name, state: State.Added, childComparisons, data: remoteRIB) };
                }
            }

            //both local and remote have value
            else
            {
                RepositoryItemBase localRIB = (RepositoryItemBase)localValue.Data!;
                RepositoryItemBase remoteRIB = (RepositoryItemBase)remoteValue.Data!;

                //data in both local and remote is null
                if (localRIB == null && remoteRIB == null)
                    return new Comparison[] { new(name, State.Unmodified, data: null) };

                //data in local is null
                else if (localRIB == null)
                    return CompareValue(name, localValue: null, remoteValue);

                //data in remote is null
                else if (remoteRIB == null)
                    return CompareValue(name, localValue, remoteValue: null);

                //data in both local and remote is not null
                else
                {
                    Type seniorType = GetSeniorType(localRIB.GetType(), remoteRIB.GetType());

                    if (localRIB.Guid == remoteRIB.Guid)
                    {
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
                        State state = childComparisons.All(c => c.State == State.Unmodified) ? State.Unmodified : State.Modified;
                        if(state == State.Unmodified)
                            return new Comparison[] { new(name, state, childComparisons, localRIB) };
                        else
                            return new Comparison[] { new(name, state, childComparisons, dataType: seniorType) };
                    }
                    else
                    {
                        List<Comparison> localChildComparisons = new();
                        foreach (PropertyInfo property in seniorType.GetProperties())
                        {
                            if (!IsPropertySerialized(property))
                                continue;
                            localChildComparisons.AddRange(
                                CompareValue(
                                    name: property.Name, 
                                    localValue: new Value(property.GetValue(localRIB)), 
                                    remoteValue: null));
                        }
                        foreach (FieldInfo field in seniorType.GetFields())
                        {
                            if (!IsPropertySerialized(field))
                                continue;
                            localChildComparisons.AddRange(
                                CompareValue(
                                    name: field.Name,
                                    localValue: new Value(field.GetValue(localRIB)),
                                    remoteValue: null));
                        }
                        Comparison localComparisonResult = new(name, state: State.Deleted, localChildComparisons, data: localRIB);

                        List<Comparison> remoteChildComparisons = new();
                        foreach (PropertyInfo property in seniorType.GetProperties())
                        {
                            if (!IsPropertySerialized(property))
                                continue;
                            remoteChildComparisons.AddRange(
                                CompareValue(
                                    name: property.Name, 
                                    localValue: null, 
                                    remoteValue: new Value(property.GetValue(remoteRIB))));
                        }
                        foreach (FieldInfo field in seniorType.GetFields())
                        {
                            if (!IsPropertySerialized(field))
                                continue;
                            remoteChildComparisons.AddRange(
                                CompareValue(
                                    name: field.Name,
                                    localValue: null,
                                    remoteValue: new Value(field.GetValue(remoteRIB))));
                        }
                        Comparison remoteComparisonResult = new(name, state: State.Added, remoteChildComparisons, data: remoteRIB);

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
                return new Comparison[] { new(name, state: State.Unmodified, data: null) };

            //local doesn't have value but, remote does
            else if (localValue == null)
            {
                //data in remote is null
                if (remoteValue!.Data == null)
                    return new Comparison[] { new(name, State.Added, data: null) };

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
                    return new Comparison[] { new(name, state: State.Added, childComparisons, data: remoteCollection) };
                }
            }

            //remote doesn't have value but, local does
            else if (remoteValue == null)
            {
                //data in local is null
                if (localValue!.Data == null)
                    return new Comparison[] { new(name, State.Deleted, data: null) };

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
                    return new Comparison[] { new(name, state: State.Deleted, childComparisons, data: localCollection) };
                }
            }
            
            //both local and remote have value
            else
            {
                System.Collections.ICollection localCollection = (System.Collections.ICollection)localValue.Data!;
                System.Collections.ICollection remoteCollection = (System.Collections.ICollection)remoteValue.Data!;

                //data in both local and remote is null
                if (localCollection == null && remoteCollection == null)
                    return new Comparison[] { new(name, State.Unmodified, data: null) };

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
                State state = childComparisons.All(c => c.State == State.Unmodified) ? State.Unmodified : State.Modified;
                if(state == State.Unmodified)
                    return new Comparison[] { new(name, state, childComparisons, localCollection) };
                else
                    return new Comparison[] { new(name, state, childComparisons, dataType: GetSeniorType(localCollection.GetType(), remoteCollection.GetType())) };
            }
        }

        private static ICollection<Comparison> CompareSimpleValue(string name, Value? localValue, Value? remoteValue)
        {
            if (localValue == null && remoteValue == null)
                return new Comparison[] { new(name, state: State.Unmodified, data: null) };
            else if (localValue == null)
                return new Comparison[] { new(name, state: State.Added, data: remoteValue!.Data) };
            else if (remoteValue == null)
                return new Comparison[] { new(name, state: State.Deleted, data: localValue!.Data) };
            else
            {
                if (string.Equals(localValue!.Data?.ToString(), remoteValue!.Data?.ToString()))
                    return new Comparison[] { new(name, state: State.Unmodified, data: localValue!.Data) };
                else
                    return new List<Comparison>()
                    {
                        new(name, state: State.Deleted, data: localValue!.Data),
                        new(name, state: State.Added, data: remoteValue!.Data)
                    };
            }
        }
    }

    public enum State
    {
        Unmodified,
        Modified,
        Added,
        Deleted
    }

    public sealed class Value
    {
        public object? Data { get; }

        public Value(object? data)
        {
            Data = data;
        }
    }

    public sealed class Comparison : INotifyPropertyChanged
    {
        public string Name { get; }
        public State State { get; }

        public bool HasChildComparisons { get; } = false;
        public ICollection<Comparison> ChildComparisons { get; }

        public bool HasData { get; } = false;
        public object? Data { get; }
        public string DataAsString => $"{Data}";

        public Type? DataType { get; }

        private bool _selected;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool Selected 
        {
            get => _selected; 
            set
            {
                _selected = value;
                if((State == State.Added || State == State.Deleted) && HasChildComparisons)
                {
                    foreach (Comparison nestedChange in ChildComparisons)
                        nestedChange.Selected = _selected;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
            }
        }

        public Comparison(string name, State state, ICollection<Comparison> childComparisons, Type dataType)
        {
            Name = name;
            State = state;
            ChildComparisons = childComparisons;
            HasChildComparisons = true;
            Data = null!;
            DataType = dataType;
        }

        public Comparison(string name, State state, ICollection<Comparison> childComparisons, object? data)
        {
            Name = name;
            State = state;
            ChildComparisons = childComparisons;
            HasChildComparisons = true;
            Data = data;
            HasData = true;
        }

        public Comparison(string name, State state, object? data)
        {
            Name = name;
            State = state;
            Data = data;
            HasData = true;
            ChildComparisons = null!;
        }
    }
}

/*
public static class RIBCompare
    {
        public static ComparisonResult Compare<T>(T localValue, T remoteValue) where T : RepositoryItemBase
        {
            return CompareValue(localValue, remoteValue);
        }

        private static ComparisonResult CompareValue(object? localValue, object? remoteValue)
        {
            Type? localValueType = localValue?.GetType();
            Type? remoteValueType = remoteValue?.GetType();

            if (IsCollectionValue(localValueType, remoteValueType))
                return CompareCollectionValue((System.Collections.ICollection?)localValue, (System.Collections.ICollection?)remoteValue);

            else if (IsRepositoryItemBaseValue(localValueType, remoteValueType))
                return CompareRIBValue((RepositoryItemBase?)localValue, (RepositoryItemBase?)remoteValue);

            else
                return CompareSimpleValue(localValue, remoteValue);
            
            //HINT: return more than one value from this method, if GUID is different for RIB, return a DELETED and a ADDED result
        }

        private static bool IsCollectionValue(Type? localValueType, Type? remoteValueType)
        {
            return
                (localValueType != null && localValueType.IsAssignableTo(typeof(System.Collections.ICollection))) ||
                (remoteValueType != null && remoteValueType.IsAssignableTo(typeof(System.Collections.ICollection)));
        }

        private static bool IsRepositoryItemBaseValue(Type? localValueType, Type? remoteValueType)
        {
            return
                (localValueType != null && localValueType.IsAssignableTo(typeof(RepositoryItemBase))) ||
                (remoteValueType != null && remoteValueType.IsAssignableTo(typeof(RepositoryItemBase)));
        }

        private static CollectionValueComparisonResult CompareCollectionValue(System.Collections.ICollection? localCollectionValue, System.Collections.ICollection? remoteCollectionValue)
        {
            List<ComparisonResult> itemComparisonResults = new();
            State? state = null;

            //condition for both local and remote values being null should be handled in CompareValue method

            if (localCollectionValue == null)
            {
                foreach (object? remoteItem in remoteCollectionValue!)
                {
                    itemComparisonResults.Add(CompareValue(localValue: null, remoteItem));
                }
                state = State.Added;
            }

            else if (remoteCollectionValue == null)
            {
                foreach(object? localItem in localCollectionValue!)
                {
                    itemComparisonResults.Add(CompareValue(localItem, remoteValue: null));
                }
                state = State.Deleted;
            }

            else
            {

                System.Collections.IEnumerator localCollectionEnumerator = localCollectionValue.GetEnumerator();
                System.Collections.IEnumerator remoteCollectionEnumerator = remoteCollectionValue.GetEnumerator();

                bool localCollectionHasValue = localCollectionEnumerator.MoveNext();
                bool remoteCollectionHasValue = remoteCollectionEnumerator.MoveNext();

                while (localCollectionHasValue || remoteCollectionHasValue)
                {
                    object? localItem = localCollectionHasValue ? localCollectionEnumerator.Current : null;
                    object? remoteItem = remoteCollectionHasValue ? remoteCollectionEnumerator.Current : null;

                    //itemComparisonResults.Add(CompareValue(localItem, remoteItem));

                    //Below trick of pre-checking the item type works but, it leaks the information about value type into this method. Multiple return value technique can be used to fix this
                    if (IsRepositoryItemBaseValue(localItem?.GetType(), remoteItem?.GetType()))
                    {
                        itemComparisonResults.Add(CompareValue(localItem, null));
                        itemComparisonResults.Add(CompareValue(null, remoteItem));
                    }
                    else
                        itemComparisonResults.Add(CompareValue(localItem, remoteItem));

                    localCollectionHasValue = localCollectionEnumerator.MoveNext();
                    remoteCollectionHasValue = remoteCollectionEnumerator.MoveNext();
                }
            }

            if (state == null)
                return new CollectionValueComparisonResult(itemComparisonResults);
            else
                return new CollectionValueComparisonResult(itemComparisonResults, state.Value);
        }

        private static RIBValueComparisonResult CompareRIBValue(RepositoryItemBase? localRIBValue, RepositoryItemBase? remoteRIBValue)
        {
            Dictionary<string, ComparisonResult> propertyComparisonResults = new();
            State? state = null;

            //condition for both local and remote values being null should be handled in CompareValue method

            if(localRIBValue == null)
            {
                foreach(PropertyInfo property in remoteRIBValue!.GetType().GetProperties())
                {
                    if (property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                        continue;

                    object? remoteRIBPropertyValue = property.GetValue(remoteRIBValue);
                    propertyComparisonResults.Add(property.Name, CompareValue(localValue: null, remoteRIBPropertyValue));
                }
                state = State.Added;
            }

            else if (remoteRIBValue == null)
            {
                foreach (PropertyInfo property in localRIBValue!.GetType().GetProperties())
                {
                    if (property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                        continue;

                    object? localRIBPropertyValue = property.GetValue(localRIBValue);
                    propertyComparisonResults.Add(property.Name, CompareValue(localRIBPropertyValue, remoteValue: null));
                }
                state = State.Deleted;
            }

            else
            {
                Type localValueType = localRIBValue!.GetType();
                Type remoteValueType = remoteRIBValue!.GetType();

                Type seniorType;
                if (localValueType.IsAssignableTo(remoteValueType))
                    seniorType = remoteValueType;
                else
                    seniorType = localValueType;

                foreach (PropertyInfo property in seniorType.GetProperties())
                {
                    string propertyName = property.Name;
                    if (property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                        continue;

                    object? localRIBPropertyValue = property.GetValue(localRIBValue);
                    object? remoteRIBPropertyValue = property.GetValue(remoteRIBValue);

                    propertyComparisonResults.Add(property.Name, CompareValue(localRIBPropertyValue, remoteRIBPropertyValue));
                }
            }

            if (state == null)
                return new RIBValueComparisonResult(propertyComparisonResults);
            else
                return new RIBValueComparisonResult(propertyComparisonResults, state.Value);

            //HINT: return more than one value from this method, if GUID is different for RIB, return a DELETED and a ADDED result
        }

        public static SimpleValueComparisonResult CompareSimpleValue(object? localSimpleValue, object? remoteSimpleValue)
        {
            if (localSimpleValue == null && remoteSimpleValue == null)
                return new SimpleValueComparisonResult(string.Empty, string.Empty, State.Unmodified);

            if (localSimpleValue == null && remoteSimpleValue != null)
                return new SimpleValueComparisonResult(localValue: string.Empty, remoteSimpleValue.ToString(), State.Added);

            if (localSimpleValue != null && remoteSimpleValue == null)
                return new SimpleValueComparisonResult(localSimpleValue.ToString(), remoteValue: string.Empty, State.Deleted);

            if (string.Equals(localSimpleValue?.ToString(), remoteSimpleValue?.ToString()))
                return new SimpleValueComparisonResult(localSimpleValue?.ToString(), remoteSimpleValue?.ToString(), State.Unmodified);
            else
                return new SimpleValueComparisonResult(localSimpleValue?.ToString(), remoteSimpleValue?.ToString(), State.Modified);
        }
    }

    public class ComparisonResult
    {
        public State State { get; init; }

        public ComparisonResult(State state)
        {
            State = state;
        }

        public ComparisonResult()
        {

        }
    }

    public enum State
    { 
        Unmodified,
        Modified,
        Added,
        Deleted
    }

    public sealed class RIBValueComparisonResult : ComparisonResult
    {
        public IDictionary<string, ComparisonResult> PropertyComparisonResults { get; }

        public RIBValueComparisonResult(IDictionary<string,ComparisonResult> propertyComparisonResults)
        {
            PropertyComparisonResults = propertyComparisonResults;
            State = PropertyComparisonResults.All(propertyCR => propertyCR.Value.State == State.Unmodified) ? State.Unmodified : State.Modified;
        }

        public RIBValueComparisonResult(IDictionary<string, ComparisonResult> propertyComparisonResults, State state) : base(state)
        {
            PropertyComparisonResults = propertyComparisonResults;
        }
    }

    public sealed class CollectionValueComparisonResult : ComparisonResult
    {
        public ICollection<ComparisonResult> ItemComparisonResults { get; }

        public CollectionValueComparisonResult(ICollection<ComparisonResult> itemComparisonResults) : base()
        {
            ItemComparisonResults = itemComparisonResults;
            State = ItemComparisonResults.All(itemCR => itemCR.State == State.Unmodified) ? State.Unmodified : State.Modified;
        }

        public CollectionValueComparisonResult(ICollection<ComparisonResult> itemComparisonResults, State state) : base(state)
        {
            ItemComparisonResults = itemComparisonResults;
        }
    }

    public sealed class SimpleValueComparisonResult : ComparisonResult
    { 
        public string? LocalValue { get; }

        public string? RemoteValue { get; }

        public SimpleValueComparisonResult(string? localValue, string? remoteValue, State state) : base(state)
        {
            LocalValue = localValue;
            RemoteValue = remoteValue;
        }
    }
 */

/*
public static class RIBCompare
    {
        public static ComparisonResult Compare<T>(T localValue, T remoteValue) where T : RepositoryItemBase
        {
            return CompareValue(localValue, remoteValue);
        }

        private static ComparisonResult CompareValue(object? localValue, object? remoteValue)
        {
            Type? localValueType = localValue?.GetType();
            Type? remoteValueType = remoteValue?.GetType();

            if (IsCollectionValue(localValueType, remoteValueType))
                return CompareCollectionValue((System.Collections.ICollection?)localValue, (System.Collections.ICollection?)remoteValue);

            else if (IsRepositoryItemBaseValue(localValueType, remoteValueType))
                return CompareRIBValue((RepositoryItemBase?)localValue, (RepositoryItemBase?)remoteValue);

            else
                return CompareSimpleValue(localValue, remoteValue);
            
            //HINT: return more than one value from this method, if GUID is different for RIB, return a DELETED and a ADDED result
        }

        private static bool IsCollectionValue(Type? localValueType, Type? remoteValueType)
        {
            return
                (localValueType != null && localValueType.IsAssignableTo(typeof(System.Collections.ICollection))) ||
                (remoteValueType != null && remoteValueType.IsAssignableTo(typeof(System.Collections.ICollection)));
        }

        private static bool IsRepositoryItemBaseValue(Type? localValueType, Type? remoteValueType)
        {
            return
                (localValueType != null && localValueType.IsAssignableTo(typeof(RepositoryItemBase))) ||
                (remoteValueType != null && remoteValueType.IsAssignableTo(typeof(RepositoryItemBase)));
        }

        private static CollectionValueComparisonResult CompareCollectionValue(System.Collections.ICollection? localCollectionValue, System.Collections.ICollection? remoteCollectionValue)
        {
            List<ComparisonResult> itemComparisonResults = new();
            State? state = null;

            //condition for both local and remote values being null should be handled in CompareValue method

            if (localCollectionValue == null)
            {
                foreach (object? remoteItem in remoteCollectionValue!)
                {
                    itemComparisonResults.Add(CompareValue(localValue: null, remoteItem));
                }
                state = State.Added;
            }

            else if (remoteCollectionValue == null)
            {
                foreach(object? localItem in localCollectionValue!)
                {
                    itemComparisonResults.Add(CompareValue(localItem, remoteValue: null));
                }
                state = State.Deleted;
            }

            else
            {

                System.Collections.IEnumerator localCollectionEnumerator = localCollectionValue.GetEnumerator();
                System.Collections.IEnumerator remoteCollectionEnumerator = remoteCollectionValue.GetEnumerator();

                bool localCollectionHasValue = localCollectionEnumerator.MoveNext();
                bool remoteCollectionHasValue = remoteCollectionEnumerator.MoveNext();

                while (localCollectionHasValue || remoteCollectionHasValue)
                {
                    object? localItem = localCollectionHasValue ? localCollectionEnumerator.Current : null;
                    object? remoteItem = remoteCollectionHasValue ? remoteCollectionEnumerator.Current : null;

                    //itemComparisonResults.Add(CompareValue(localItem, remoteItem));

                    //Below trick of pre-checking the item type works but, it leaks the information about value type into this method. Multiple return value technique can be used to fix this
                    if (IsRepositoryItemBaseValue(localItem?.GetType(), remoteItem?.GetType()))
                    {
                        itemComparisonResults.Add(CompareValue(localItem, null));
                        itemComparisonResults.Add(CompareValue(null, remoteItem));
                    }
                    else
                        itemComparisonResults.Add(CompareValue(localItem, remoteItem));

                    localCollectionHasValue = localCollectionEnumerator.MoveNext();
                    remoteCollectionHasValue = remoteCollectionEnumerator.MoveNext();
                }
            }

            if (state == null)
                return new CollectionValueComparisonResult(itemComparisonResults);
            else
                return new CollectionValueComparisonResult(itemComparisonResults, state.Value);
        }

        private static RIBValueComparisonResult CompareRIBValue(RepositoryItemBase? localRIBValue, RepositoryItemBase? remoteRIBValue)
        {
            Dictionary<string, ComparisonResult> propertyComparisonResults = new();
            State? state = null;

            //condition for both local and remote values being null should be handled in CompareValue method

            if(localRIBValue == null)
            {
                foreach(PropertyInfo property in remoteRIBValue!.GetType().GetProperties())
                {
                    if (property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                        continue;

                    object? remoteRIBPropertyValue = property.GetValue(remoteRIBValue);
                    propertyComparisonResults.Add(property.Name, CompareValue(localValue: null, remoteRIBPropertyValue));
                }
                state = State.Added;
            }

            else if (remoteRIBValue == null)
            {
                foreach (PropertyInfo property in localRIBValue!.GetType().GetProperties())
                {
                    if (property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                        continue;

                    object? localRIBPropertyValue = property.GetValue(localRIBValue);
                    propertyComparisonResults.Add(property.Name, CompareValue(localRIBPropertyValue, remoteValue: null));
                }
                state = State.Deleted;
            }

            else
            {
                Type localValueType = localRIBValue!.GetType();
                Type remoteValueType = remoteRIBValue!.GetType();

                Type seniorType;
                if (localValueType.IsAssignableTo(remoteValueType))
                    seniorType = remoteValueType;
                else
                    seniorType = localValueType;

                foreach (PropertyInfo property in seniorType.GetProperties())
                {
                    string propertyName = property.Name;
                    if (property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                        continue;

                    object? localRIBPropertyValue = property.GetValue(localRIBValue);
                    object? remoteRIBPropertyValue = property.GetValue(remoteRIBValue);

                    propertyComparisonResults.Add(property.Name, CompareValue(localRIBPropertyValue, remoteRIBPropertyValue));
                }
            }

            if (state == null)
                return new RIBValueComparisonResult(propertyComparisonResults);
            else
                return new RIBValueComparisonResult(propertyComparisonResults, state.Value);

            //HINT: return more than one value from this method, if GUID is different for RIB, return a DELETED and a ADDED result
        }

        public static SimpleValueComparisonResult CompareSimpleValue(object? localSimpleValue, object? remoteSimpleValue)
        {
            if (localSimpleValue == null && remoteSimpleValue == null)
                return new SimpleValueComparisonResult(string.Empty, string.Empty, State.Unmodified);

            if (localSimpleValue == null && remoteSimpleValue != null)
                return new SimpleValueComparisonResult(localValue: string.Empty, remoteSimpleValue.ToString(), State.Added);

            if (localSimpleValue != null && remoteSimpleValue == null)
                return new SimpleValueComparisonResult(localSimpleValue.ToString(), remoteValue: string.Empty, State.Deleted);

            if (string.Equals(localSimpleValue?.ToString(), remoteSimpleValue?.ToString()))
                return new SimpleValueComparisonResult(localSimpleValue?.ToString(), remoteSimpleValue?.ToString(), State.Unmodified);
            else
                return new SimpleValueComparisonResult(localSimpleValue?.ToString(), remoteSimpleValue?.ToString(), State.Modified);
        }
    }

    public class ComparisonResult
    {
        public State State { get; init; }

        public ComparisonResult(State state)
        {
            State = state;
        }

        public ComparisonResult()
        {

        }
    }

    public enum State
    { 
        Unmodified,
        Modified,
        Added,
        Deleted
    }

    public sealed class RIBValueComparisonResult : ComparisonResult
    {
        public IDictionary<string, ComparisonResult> PropertyComparisonResults { get; }

        public RIBValueComparisonResult(IDictionary<string,ComparisonResult> propertyComparisonResults)
        {
            PropertyComparisonResults = propertyComparisonResults;
            State = PropertyComparisonResults.All(propertyCR => propertyCR.Value.State == State.Unmodified) ? State.Unmodified : State.Modified;
        }

        public RIBValueComparisonResult(IDictionary<string, ComparisonResult> propertyComparisonResults, State state) : base(state)
        {
            PropertyComparisonResults = propertyComparisonResults;
        }
    }

    public sealed class CollectionValueComparisonResult : ComparisonResult
    {
        public ICollection<ComparisonResult> ItemComparisonResults { get; }

        public CollectionValueComparisonResult(ICollection<ComparisonResult> itemComparisonResults) : base()
        {
            ItemComparisonResults = itemComparisonResults;
            State = ItemComparisonResults.All(itemCR => itemCR.State == State.Unmodified) ? State.Unmodified : State.Modified;
        }

        public CollectionValueComparisonResult(ICollection<ComparisonResult> itemComparisonResults, State state) : base(state)
        {
            ItemComparisonResults = itemComparisonResults;
        }
    }

    public sealed class SimpleValueComparisonResult : ComparisonResult
    { 
        public string? LocalValue { get; }

        public string? RemoteValue { get; }

        public SimpleValueComparisonResult(string? localValue, string? remoteValue, State state) : base(state)
        {
            LocalValue = localValue;
            RemoteValue = remoteValue;
        }
    }
 */

/*
using Amdocs.Ginger.Repository;
using Microsoft.Graph;
using OpenQA.Selenium.Appium;
using SixLabors.ImageSharp.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ginger.ConflictResolve
{
    public static class RIBCompare
    {
        public static ComparisonResult Compare<T>(T localValue, T remoteValue) where T : RepositoryItemBase
        {
            return CompareValue(localValue, remoteValue);
        }

        private static ComparisonResult CompareValue(object? localValue, object? remoteValue)
        {
            Type? localValueType = localValue?.GetType();
            Type? remoteValueType = remoteValue?.GetType();

            if (IsCollectionValue(localValueType, remoteValueType))
                return CompareCollectionValue((System.Collections.ICollection?)localValue, (System.Collections.ICollection?)remoteValue);

            else if (IsRepositoryItemBaseValue(localValueType, remoteValueType))
                return CompareRIBValue((RepositoryItemBase?)localValue, (RepositoryItemBase?)remoteValue);

            else
                return CompareSimpleValue(localValue, remoteValue);
        }

        private static bool IsCollectionValue(Type? localValueType, Type? remoteValueType)
        {
            return
                (localValueType != null && localValueType.IsAssignableTo(typeof(System.Collections.ICollection))) ||
                (remoteValueType != null && remoteValueType.IsAssignableTo(typeof(System.Collections.ICollection)));
        }

        private static bool IsRepositoryItemBaseValue(Type? localValueType, Type? remoteValueType)
        {
            return
                (localValueType != null && localValueType.IsAssignableTo(typeof(RepositoryItemBase))) ||
                (remoteValueType != null && remoteValueType.IsAssignableTo(typeof(RepositoryItemBase)));
        }

        private static CollectionValueComparisonResult CompareCollectionValue(System.Collections.ICollection? localCollectionValue, System.Collections.ICollection? remoteCollectionValue)
        {
            List<ComparisonResult> itemComparisonResults = new();
            if (localCollectionValue == null)
            {
                foreach (object? remoteItem in remoteCollectionValue!)
                {
                    itemComparisonResults.Add(CompareValue(localValue: null, remoteItem));
                }

            }

            else if (remoteCollectionValue == null)
            {
                foreach(object? localItem in localCollectionValue!)
                {
                    itemComparisonResults.Add(CompareValue(localItem, remoteValue: null));
                }
            }

            else
            {

                System.Collections.IEnumerator localCollectionEnumerator = localCollectionValue.GetEnumerator();
                System.Collections.IEnumerator remoteCollectionEnumerator = remoteCollectionValue.GetEnumerator();

                bool localCollectionHasValue = localCollectionEnumerator.MoveNext();
                bool remoteCollectionHasValue = remoteCollectionEnumerator.MoveNext();

                while (localCollectionHasValue || remoteCollectionHasValue)
                {
                    object? localItem = localCollectionHasValue ? localCollectionEnumerator.Current : null;
                    object? remoteItem = remoteCollectionHasValue ? remoteCollectionEnumerator.Current : null;

                    itemComparisonResults.Add(CompareValue(localItem, remoteItem));

                    localCollectionHasValue = localCollectionEnumerator.MoveNext();
                    remoteCollectionHasValue = remoteCollectionEnumerator.MoveNext();
                }
            }

            return new CollectionValueComparisonResult(itemComparisonResults);
        }

        private static RIBValueComparisonResult CompareRIBValue(RepositoryItemBase? localRIBValue, RepositoryItemBase? remoteRIBValue)
        {
            Dictionary<string, ComparisonResult> propertyComparisonResults = new();

            if(localRIBValue == null)
            {
                foreach(PropertyInfo property in remoteRIBValue!.GetType().GetProperties())
                {
                    if (property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                        continue;

                    object? remoteRIBPropertyValue = property.GetValue(remoteRIBValue);
                    propertyComparisonResults.Add(property.Name, CompareValue(localValue: null, remoteRIBPropertyValue));
                }
            }

            else if (remoteRIBValue == null)
            {
                foreach (PropertyInfo property in localRIBValue!.GetType().GetProperties())
                {
                    if (property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                        continue;

                    object? localRIBPropertyValue = property.GetValue(localRIBValue);
                    propertyComparisonResults.Add(property.Name, CompareValue(localRIBPropertyValue, remoteValue: null));
                }
            }

            else
            {
                Type localValueType = localRIBValue!.GetType();
                Type remoteValueType = remoteRIBValue!.GetType();

                Type seniorType;
                if (localValueType.IsAssignableTo(remoteValueType))
                    seniorType = remoteValueType;
                else
                    seniorType = localValueType;

                foreach (PropertyInfo property in seniorType.GetProperties())
                {
                    string propertyName = property.Name;
                    if (property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                        continue;

                    object? localRIBPropertyValue = property.GetValue(localRIBValue);
                    object? remoteRIBPropertyValue = property.GetValue(remoteRIBValue);

                    propertyComparisonResults.Add(property.Name, CompareValue(localRIBPropertyValue, remoteRIBPropertyValue));
                }
            }

            return new RIBValueComparisonResult(propertyComparisonResults);
        }

        public static SimpleValueComparisonResult CompareSimpleValue(object? localSimpleValue, object? remoteSimpleValue)
        {
            if (string.Equals(localSimpleValue?.ToString(), remoteSimpleValue?.ToString()))
                return new SimpleValueComparisonResult(localSimpleValue?.ToString(), remoteSimpleValue?.ToString(), State.Unmodified);
            else
                return new SimpleValueComparisonResult(localSimpleValue?.ToString(), remoteSimpleValue?.ToString(), State.Modified);
        }
    }

    public class ComparisonResult
    {
        public State State { get; init; }

        public ComparisonResult(State state)
        {
            State = state;
        }

        public ComparisonResult()
        {

        }
    }

    public enum State
    { 
        Unmodified,
        Modified,
        Added,
        Deleted
    }

    public sealed class RIBValueComparisonResult : ComparisonResult
    {
        public IDictionary<string, ComparisonResult> PropertyComparisonResults { get; }

        public RIBValueComparisonResult(IDictionary<string,ComparisonResult> propertyComparisonResults)
        {
            PropertyComparisonResults = propertyComparisonResults;
            State = PropertyComparisonResults.All(propertyCR => propertyCR.Value.State == State.Unmodified) ? State.Unmodified : State.Modified;
        }
    }

    public sealed class CollectionValueComparisonResult : ComparisonResult
    {
        public ICollection<ComparisonResult> ItemComparisonResults { get; }

        public CollectionValueComparisonResult(ICollection<ComparisonResult> itemComparisonResults) : base()
        {
            ItemComparisonResults = itemComparisonResults;
            State = ItemComparisonResults.All(itemCR => itemCR.State == State.Unmodified) ? State.Unmodified : State.Modified;
        }
    }

    public sealed class SimpleValueComparisonResult : ComparisonResult
    { 
        public string? LocalValue { get; }

        public string? RemoteValue { get; }

        public SimpleValueComparisonResult(string? localValue, string? remoteValue, State state) : base(state)
        {
            LocalValue = localValue;
            RemoteValue = remoteValue;
        }
    }
}

 */