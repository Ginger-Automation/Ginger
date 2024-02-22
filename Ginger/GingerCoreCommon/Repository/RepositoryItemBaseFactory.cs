using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Amdocs.Ginger.Common.Repository.Serialization;
using Amdocs.Ginger.Repository;
using MethodTimer;

#nullable enable
namespace Amdocs.Ginger.Common.Repository
{
    public static class RepositoryItemBaseFactory
    {
        private static readonly IReadOnlyList<string> GingerAssemblyNames = new List<string>()
        {
            "GingerCore",
            "GingerCoreNET",
            "GingerCoreCommon"
        };
        private static readonly Dictionary<string, Func<DeserializedSnapshot, RepositoryItemBase>> NameToConstructorMap = [];
        private static readonly Dictionary<string, Func<DeserializedSnapshot2, RepositoryItemBase>> NameToConstructorMap2 = [];
        private static bool IsLoaded = false;
        private static bool IsLoaded2 = false;
        private static AutoResetEvent LoadSyncEvent = new(true);
        private static AutoResetEvent LoadSyncEvent2 = new(true);

        public static void Load()
        {
            if (IsLoaded)
                return;
            Debug.WriteLine(Environment.CurrentManagedThreadId + " going to wait one");
            LoadSyncEvent.WaitOne();
            Debug.WriteLine(Environment.CurrentManagedThreadId + " crossed wait line");
            if (IsLoaded)
            {
                LoadSyncEvent.Set();
                return;
            }
            Debug.WriteLine(Environment.CurrentManagedThreadId + " starting loading");
            NameToConstructorMap.Clear();
            IEnumerable<Type> ribTypes = GetAllRepositoryItemBaseTypes();
            foreach (Type ribType in ribTypes)
            {
                Func<DeserializedSnapshot, RepositoryItemBase>? constructor = GetConstructorForRepositoryItemBaseType(ribType);
                if (constructor != null)
                    NameToConstructorMap[ribType.Name] = constructor;
            }
            IsLoaded = true;
            LoadSyncEvent.Set();
        }

        public static void Load2()
        {
            if (IsLoaded2)
                return;
            Debug.WriteLine(Environment.CurrentManagedThreadId + " going to wait one");
            LoadSyncEvent2.WaitOne();
            Debug.WriteLine(Environment.CurrentManagedThreadId + " crossed wait line");
            if (IsLoaded2)
            {
                LoadSyncEvent2.Set();
                return;
            }
            Debug.WriteLine(Environment.CurrentManagedThreadId + " starting loading");
            NameToConstructorMap2.Clear();
            IEnumerable<Type> ribTypes = GetAllRepositoryItemBaseTypes();
            foreach (Type ribType in ribTypes)
            {
                Func<DeserializedSnapshot2, RepositoryItemBase>? constructor = GetConstructorForRepositoryItemBaseType2(ribType);
                if (constructor != null)
                    NameToConstructorMap2[ribType.Name] = constructor;
            }
            IsLoaded2 = true;
            LoadSyncEvent2.Set();
        }

        private static IEnumerable<Type> GetAllRepositoryItemBaseTypes()
        {
            return
                GetGingerAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => 
                    type.IsAssignableTo(typeof(RepositoryItemBase)) &&
                    !type.IsAbstract);
        }

        private static IEnumerable<Assembly> GetGingerAssemblies()
        {
            return
                AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(assembly => GingerAssemblyNames.Any(name => 
                    assembly.FullName != null && 
                    assembly.FullName.Contains(name)));
        }

        private static Func<DeserializedSnapshot, RepositoryItemBase>? GetConstructorForRepositoryItemBaseType(Type ribType)
        {
            Type[] constructorParamTypes = [typeof(DeserializedSnapshot)];
            ConstructorInfo? constructor = ribType.GetConstructor(constructorParamTypes);

            if (constructor == null)
            {
                return null;
                //TODO: BetterRepositorySerializer: throw exception
                //throw new Exception($"No constructor found for type '{ribType.FullName}' that accepts '{nameof(DeserializedSnapshot)}' as parameter.");
            }

            ParameterExpression parameterExpression = 
                Expression.Parameter(typeof(DeserializedSnapshot));

            NewExpression newExpression = 
                Expression.New(constructor, parameterExpression);

            Expression<Func<DeserializedSnapshot, RepositoryItemBase>> lambdaExpression = 
                Expression.Lambda<Func<DeserializedSnapshot, RepositoryItemBase>>(newExpression, parameterExpression);

            return lambdaExpression.Compile();
        }

        private static Func<DeserializedSnapshot2, RepositoryItemBase>? GetConstructorForRepositoryItemBaseType2(Type ribType)
        {
            Type[] constructorParamTypes = [typeof(DeserializedSnapshot2)];
            ConstructorInfo? constructor = ribType.GetConstructor(constructorParamTypes);

            if (constructor == null)
            {
                return null;
                //TODO: BetterRepositorySerializer: throw exception
                //throw new Exception($"No constructor found for type '{ribType.FullName}' that accepts '{nameof(DeserializedSnapshot2)}' as parameter.");
            }

            ParameterExpression parameterExpression =
                Expression.Parameter(typeof(DeserializedSnapshot2));

            NewExpression newExpression =
                Expression.New(constructor, parameterExpression);

            Expression<Func<DeserializedSnapshot2, RepositoryItemBase>> lambdaExpression =
                Expression.Lambda<Func<DeserializedSnapshot2, RepositoryItemBase>>(newExpression, parameterExpression);

            return lambdaExpression.Compile();
        }

        public static T Create<T>(DeserializedSnapshot snapshot) where T : RepositoryItemBase
        {
            return (T)Create(typeof(T).Name, snapshot);
        }

        public static RepositoryItemBase Create(string name, DeserializedSnapshot snapshot)
        {
            Load();

            if (!NameToConstructorMap.TryGetValue(name, out Func<DeserializedSnapshot, RepositoryItemBase>? constructor) ||
                constructor == null)
                throw new Exception($"No constructor was loaded in factory for '{name}'.");

            return constructor(snapshot);
        }

        public static T Create<T>(DeserializedSnapshot2 snapshot) where T : RepositoryItemBase
        {
            return (T)Create(typeof(T).Name, snapshot);
        }

        public static RepositoryItemBase Create(string name, DeserializedSnapshot2 snapshot)
        {
            Load2();

            if (!NameToConstructorMap2.TryGetValue(name, out Func<DeserializedSnapshot2, RepositoryItemBase>? constructor) ||
                constructor == null)
                throw new Exception($"No constructor was loaded in factory for '{name}'.");

            return constructor(snapshot);
        }
    }
}
