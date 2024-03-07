using Amdocs.Ginger.Repository;
using Applitools;
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
        static TestDataGenerator()
        {
            if (!NewRepositorySerializerIsInitialized())
                InitializeNewRepositorySerializer();
        }

        internal static RepositoryItemBase[] GenerateRIBs(int size)
        {
            RepositoryItemBase[] testData = new RepositoryItemBase[size];

            string businessFlowXML = GetTestBusinessFlowXml();
            for (int index = 0; index < size; index++)
            {
                testData[index] = NewRepositorySerializer.DeserializeFromText(businessFlowXML);
            }

            return testData;
        }

        private static string GetTestBusinessFlowXml()
        {
            return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Benchmarks", "Resources", "TestBusinessFlow.Ginger.BusinessFlow.xml"));
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

        internal static string[] GenerateXMLs(int size)
        {
            string[] testData = new string[size];
            for(int index = 0; index < size; index++)
            {
                testData[index] = GetTestBusinessFlowXml();
            }

            return testData;
        }
    }
}
