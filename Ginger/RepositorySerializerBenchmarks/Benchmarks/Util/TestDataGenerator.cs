using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositorySerializerBenchmarks.Benchmarks.Util
{
    internal static class TestDataGenerator
    {
        internal static RepositoryItemBase[] Generate(int size)
        {
            if (!NewRepositorySerializerIsInitialized())
                InitializeNewRepositorySerializer();

            RepositoryItemBase[] testData = new RepositoryItemBase[size];

            string businessFlowXMLString = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks", "Resources", "TestBusinessFlow.Ginger.BusinessFlow.xml"));
            for (int index = 0; index < size; index++)
            {
                testData[index] = NewRepositorySerializer.DeserializeFromText(businessFlowXMLString);
            }

            return testData;
        }

        private static bool NewRepositorySerializerIsInitialized()
        {
            return NewRepositorySerializer.mClassDictionary.Count > 0;
        }

        private static void InitializeNewRepositorySerializer()
        {
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.GingerCoreCommon);

            _ = new ActDummy();
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.GingerCoreNET);
        }
    }
}
