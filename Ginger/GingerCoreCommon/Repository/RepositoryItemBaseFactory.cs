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
        private static bool IsLoaded = false;
        private static readonly AutoResetEvent LoadSyncEvent = new(true);

        public static void Load()
        {
            if (IsLoaded)
                return;
            LoadSyncEvent.WaitOne();
            if (IsLoaded)
            {
                LoadSyncEvent.Set();
                return;
            }
            NameToConstructorMap.Clear();
            IEnumerable<Type> ribTypes = GetAllRepositoryItemBaseTypes();
            foreach (Type ribType in ribTypes)
            {
                Func<DeserializedSnapshot, RepositoryItemBase>? constructor = GetConstructorForRepositoryItemBaseType2(ribType);
                if (constructor != null)
                    NameToConstructorMap[ribType.Name] = constructor;
            }
            IsLoaded = true;
            LoadSyncEvent.Set();
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

        private static Func<DeserializedSnapshot, RepositoryItemBase>? GetConstructorForRepositoryItemBaseType2(Type ribType)
        {
            Type[] constructorParamTypes = [typeof(DeserializedSnapshot)];
            ConstructorInfo? constructor = ribType.GetConstructor(constructorParamTypes);

            if (constructor == null)
            {
                return null;
                //TODO: BetterRepositorySerializer: throw exception
                //throw new Exception($"No constructor found for type '{ribType.FullName}' that accepts '{nameof(DeserializedSnapshot2)}' as parameter.");
            }

            ParameterExpression parameterExpression =
                Expression.Parameter(typeof(DeserializedSnapshot));

            NewExpression newExpression =
                Expression.New(constructor, parameterExpression);

            Expression<Func<DeserializedSnapshot, RepositoryItemBase>> lambdaExpression =
                Expression.Lambda<Func<DeserializedSnapshot, RepositoryItemBase>>(newExpression, parameterExpression);

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
    }
}
