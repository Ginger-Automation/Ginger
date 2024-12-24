using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.NewSelfHealing
{
    internal sealed class MobileElementPropertyMatcher : ElementPropertyMatcher
    {
        private static readonly IReadOnlyDictionary<string, double> Weightage = new Dictionary<string, double>()
        {
            { "id", 2 }, { "name", 1.8 }, { "text", 1.8 }, { "element type", 1.4 }, 
            { "label", 1.5 }, { "class", 1.5 }
        };

        protected override double GetPropertyWeightage(string propertyName)
        {
            if (Weightage.TryGetValue(propertyName.ToLower(), out double value))
            {
                return value;
            }
            return 1;
        }
    }
}
