using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositorySerializerBenchmarks.Benchmarks.Util
{
    internal static class TestActivitiesGenerator
    {
        internal static Activity[] Generate(int size)
        {
            Activity[] activities = new Activity[size];
            for (int index = 0; index < size; index++)
                activities[index] = GenerateActivity();
            return activities;
        }

        private static Activity GenerateActivity()
        {
            Activity activity = new()
            {
                ActivityName = $"Activity_{new Random().Next(int.MaxValue)}",
                Acts = new ObservableList<IAct>(GenerateActs(10))
            };
            return activity;
        }

        private static IEnumerable<IAct> GenerateActs(int size)
        {
            IAct[] acts = new IAct[size];
            for (int index = 0; index < size; index++)
            {
                acts[index] = new ActDummy()
                {
                    Description = $"Dummy_{new Random().Next(int.MaxValue)}"
                };
            }
            return acts;
        }
    }
}
