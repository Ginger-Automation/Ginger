using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Microsoft.Azure.Cosmos.Linq;
using mshtml;
using NPOI.SS.Formula.Functions;
using SixLabors.ImageSharp.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ConflictResolve
{
    public static class RIBMerge
    {
        public static T? Merge<T>(ICollection<Comparison> comparisonResult) where T : RepositoryItemBase
        {
            return (T?)Merge(typeof(T), comparisonResult);
        }

        private static object? Merge(Type type, ICollection<Comparison> comparisonResult)
        {
            if (type.IsAssignableTo(typeof(RepositoryItemBase)))
                return MergeRIB(type, comparisonResult);
            else if (type.IsAssignableTo(typeof(System.Collections.ICollection)))
                return MergeCollection(type, comparisonResult);
            else
                return MergeSimpleValue(type, comparisonResult);

        }

        private static RepositoryItemBase? MergeRIB(Type type, ICollection<Comparison> ribComparisonResult)
        {
            if (!type.IsAssignableTo(typeof(RepositoryItemBase)))
                throw new InvalidOperationException($"Expected a {typeof(RepositoryItemBase).FullName} type.");

            RepositoryItemBase? rib = null;

            Comparison? comparisonForMerge = ribComparisonResult
                .FirstOrDefault(c => (c.State == State.Unmodified) || ((c.State == State.Added || c.State == State.Deleted) && c.Selected));

            if (comparisonForMerge != null)
            {
                rib = (RepositoryItemBase?)comparisonForMerge.Data;
            }
            else
            {
                comparisonForMerge = ribComparisonResult
                    .FirstOrDefault(childComparison => childComparison.State == State.Modified);

                if (comparisonForMerge == null)
                    throw new InvalidOperationException($"No appropriate {nameof(Comparison)} found for merging type {type.FullName}.");

                rib = (RepositoryItemBase?)Activator.CreateInstance(type);

                foreach (PropertyInfo property in type.GetProperties())
                {
                    if (property.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                        continue;

                    string propertyName = property.Name;

                    IEnumerable<Comparison> propertyComparisons = comparisonForMerge.ChildComparisons
                        .Where(c => string.Equals(c.Name, property.Name));

                    Comparison? propertyComparisonForMerge = propertyComparisons
                        .FirstOrDefault(c => c.State == State.Unmodified || ((c.State == State.Added || c.State == State.Deleted) && c.Selected));

                    if (propertyComparisonForMerge != null)
                    {
                        property.SetValue(rib, propertyComparisonForMerge.Data);
                    }
                    else
                    {
                        propertyComparisonForMerge = propertyComparisons
                            .FirstOrDefault(c => c.State == State.Modified);

                        if (propertyComparisonForMerge == null)
                            throw new InvalidOperationException($"No appropriate {nameof(Comparison)} found for merging property {property.Name}.");

                        //property.SetValue(rib, Merge(property.PropertyType, new ComparisonResult[] { propertyComparisonForMerge }));
                        property.SetValue(rib, Merge(propertyComparisonForMerge.DataType!, new Comparison[] { propertyComparisonForMerge }));
                    }
                }
                foreach (FieldInfo field in type.GetFields())
                {
                    if (field.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute)) == null)
                        continue;

                    string propertyName = field.Name;

                    IEnumerable<Comparison> fieldComparisons = comparisonForMerge.ChildComparisons
                        .Where(c => string.Equals(c.Name, field.Name));

                    Comparison? fieldComparisonForMerge = fieldComparisons
                        .FirstOrDefault(c => c.State == State.Unmodified || ((c.State == State.Added || c.State == State.Deleted) && c.Selected));

                    if (fieldComparisonForMerge != null)
                    {
                        field.SetValue(rib, fieldComparisonForMerge.Data);
                    }
                    else
                    {
                        fieldComparisonForMerge = fieldComparisons
                            .FirstOrDefault(c => c.State == State.Modified);

                        if (fieldComparisonForMerge == null)
                            throw new InvalidOperationException($"No appropriate {nameof(Comparison)} found for merging field {field.Name}.");

                        field.SetValue(rib, Merge(fieldComparisonForMerge.DataType!, new Comparison[] { fieldComparisonForMerge }));
                    }
                }
            }

            return rib;
        }

        private static System.Collections.ICollection? MergeCollection(Type type, ICollection<Comparison> collectionComparisonResult)
        {
            if(!type.IsAssignableTo(typeof(System.Collections.ICollection)))
                throw new InvalidOperationException($"Expected a {typeof(System.Collections.ICollection).FullName} type.");

            System.Collections.ICollection? collection = null;

            Comparison? comparisonForMerge = collectionComparisonResult
                .FirstOrDefault(c => (c.State == State.Unmodified) || ((c.State == State.Added || c.State == State.Deleted) && c.Selected));

            if(comparisonForMerge != null)
            {
                collection = (System.Collections.ICollection?)comparisonForMerge.Data;
            }
            else
            {
                comparisonForMerge = collectionComparisonResult
                    .FirstOrDefault(childComparison => childComparison.State == State.Modified);

                if (comparisonForMerge == null)
                    throw new InvalidOperationException($"No appropriate {nameof(Comparison)} found for merging type {type.FullName}.");

                System.Collections.ArrayList items = new();

                foreach(Comparison itemComparison in comparisonForMerge.ChildComparisons)
                {
                    if (itemComparison.State == State.Unmodified)
                    {
                        items.Add(itemComparison.Data);
                    } 
                    else if (itemComparison.State == State.Added || itemComparison.State == State.Deleted)
                    {
                        if (itemComparison.Selected)
                        {
                            items.Add(itemComparison.Data);
                        }
                    }
                    else if(itemComparison.State == State.Modified)
                    {
                        //items.Add(Merge(GetCollectionItemType(type), new ComparisonResult[] { itemComparison }));
                        items.Add(Merge(itemComparison.DataType!, new Comparison[] { itemComparison }));
                    }
                    else
                        throw new InvalidOperationException($"No appropriate {nameof(Comparison)} found for merging at index {items.Count}.");
                }

                collection = CreateCollectionOfType(type, items);
            }


            return collection;
        }

        private static Type GetCollectionItemType(Type collectionType)
        {
            Type collectionItemType;
            if (collectionType.GetGenericArguments().Length > 0)
                collectionItemType = collectionType.GetGenericArguments()[0];
            else
                collectionItemType = typeof(object);

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
                        throw new Exception($"No appropriate constructor found for collection of type {type.FullName}");

                    collection = (System.Collections.ICollection)constructor.Invoke(Array.Empty<object>());
                    MethodInfo addMethod = type.GetMethod(nameof(System.Collections.IList.Add))!;

                    foreach (object item in items)
                        addMethod.Invoke(collection, new object[] { item });
                }
            }
            else
                throw new NotImplementedException($"Cannot create collection of type {type.FullName}");

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
                .FirstOrDefault(c => (c.State == State.Unmodified) || ((c.State == State.Added || c.State == State.Deleted) && c.Selected));

            if(comparisonForMerge == null)
                throw new InvalidOperationException($"No appropriate {nameof(Comparison)} found for merging value of type {type.Name}.");

            value = comparisonForMerge.Data;

            return value;
        }
    }
}

/*
 using Amdocs.Ginger.Repository;
using Microsoft.Azure.Cosmos.Linq;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ConflictResolve
{
    public static class RIBMerge
    {
        public static T Merge<T>(ComparisonResult comparisonResult) where T : RepositoryItemBase
        {
            return (T)Merge(typeof(T), comparisonResult);
        }

        private static object Merge(Type type, ComparisonResult comparisonResult)
        {
            if (type.IsAssignableTo(typeof(RepositoryItemBase)))
                return MergeRIB(type, comparisonResult);
            else
                throw new NotImplementedException();

        }

        private static RepositoryItemBase MergeRIB(Type type, ComparisonResult ribComparisonResult)
        {
            if (!type.IsAssignableTo(typeof(RepositoryItemBase)))
                throw new InvalidOperationException($"Expected a {typeof(RepositoryItemBase).FullName} type.");

            RepositoryItemBase? rib = null;

            ComparisonResult? comparisonForMerge = ribComparisonResult
                .ChildComparisons
                .FirstOrDefault(childComparison => childComparison.State != State.Modified && childComparison.Selected);

            if (comparisonForMerge != null)
            {
                rib = (RepositoryItemBase?)comparisonForMerge.Value!;
            }
            else
            {
                comparisonForMerge = ribComparisonResult
                    .ChildComparisons
                    .FirstOrDefault(childComparison => childComparison.State == State.Modified);

                if (comparisonForMerge == null)
                    throw new InvalidOperationException($"No appropriate {nameof(ComparisonResult)} found for merge.");

                rib = (RepositoryItemBase)Activator.CreateInstance(type)!;
                foreach (PropertyInfo property in type.GetProperties())
                {
                    string propertyName = property.Name;
                    IEnumerable<ComparisonResult> propertyComparisons = ribChildComparison.ChildComparisons
                        .Where(c => string.Equals(c.Name, property.Name));
                    foreach (ComparisonResult comparison in propertyComparisons)
                    {
                        if (comparison.State != State.Modified)
                        {
                            if (comparison.Selected)
                            {
                                property.SetValue(rib, comparison.Value);
                                break;
                            }
                        }
                        else
                        {
                            property.SetValue(rib, Merge(property.PropertyType, comparison));
                            break;
                        }
                    }
                }
            }

            return rib;

            foreach (ComparisonResult ribChildComparison in ribComparisonResult.ChildComparisons)
            {
                if (ribChildComparison.State != State.Modified)
                {
                    if (ribChildComparison.Selected)
                    {
                        rib = (RepositoryItemBase?)ribChildComparison.Value!;
                        break;
                    }
                }
                else
                {
                    rib = (RepositoryItemBase)Activator.CreateInstance(type)!;
                    foreach (PropertyInfo property in type.GetProperties())
                    {
                        string propertyName = property.Name;
                        IEnumerable<ComparisonResult> propertyComparisons = ribChildComparison.ChildComparisons
                            .Where(c => string.Equals(c.Name, property.Name));
                        foreach (ComparisonResult comparison in propertyComparisons)
                        {
                            if (comparison.State != State.Modified)
                            {
                                if (comparison.Selected)
                                {
                                    property.SetValue(rib, comparison.Value);
                                    break;
                                }
                            }
                            else
                            {
                                property.SetValue(rib, Merge(property.PropertyType, comparison));
                                break;
                            }
                        }
                    }
                    break;
                }
            }

            if (rib == null)
                throw new InvalidOperationException($"No appropriate {nameof(ComparisonResult)} found for merge.");

            return rib;
        }
    }
}

 */