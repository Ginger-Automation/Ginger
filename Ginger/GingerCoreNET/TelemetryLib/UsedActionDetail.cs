using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class UsedActionDetail
    {
        public string Name { get; set; }
        [DefaultValueAttribute(0)]
        public int CountTotal { get; set; } = 0;
        [DefaultValueAttribute(0)]
        public int CountPassed { get; set; } = 0;
        [DefaultValueAttribute(0)]
        public int CountFailed { get; set; } = 0;

        public UsedActionDetail(string name, int countTotal, int countPassed, int countFailed)
        {
            Name = name;
            CountTotal = countTotal;
            CountPassed = countPassed;
        }

        public override string ToString()
        {
            return "Action Name: " + Name + ", Total Usage: " + CountTotal.ToString() + ", Passed: " + CountPassed.ToString() + ", Failed: " + CountFailed.ToString();
        }
    }
}
