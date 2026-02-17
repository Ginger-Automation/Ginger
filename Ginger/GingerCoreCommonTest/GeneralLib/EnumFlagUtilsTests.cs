#region License
/*
Copyright © 2014-2026 European Support Limited
 
Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
 
http://www.apache.org/licenses/LICENSE-2.0
 
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion
using Amdocs.Ginger.Common.GeneralLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreCommonTest.GeneralLib
{
    [TestClass]
    public class EnumFlagUtilsTests
    {
        [TestMethod]
        public void AreAllFlagsSet_UIntVerificationFlagsAreSet_ReturnsTrue()
        {
            UIntFlagEnum thisFlag = UIntFlagEnum.All;
            UIntFlagEnum flagsToVerify = UIntFlagEnum.Value2;

            bool areAllFlagsSet = thisFlag.AreAllFlagsSet(flagsToVerify);

            Assert.IsTrue(areAllFlagsSet, $"Verification flags are set in current value but, not verified correctly.");
        }

        [TestMethod]
        public void AreAllFlagsSet_UIntVerificationFlagsAreNotSet_ReturnsFalse()
        {
            UIntFlagEnum thisFlag = UIntFlagEnum.None;
            UIntFlagEnum flagsToVerify = UIntFlagEnum.Value2;

            bool areAllFlagsSet = thisFlag.AreAllFlagsSet(flagsToVerify);

            Assert.IsFalse(areAllFlagsSet, $"Verification flags are not set in current value but, not verified correctly.");
        }

        [TestMethod]
        public void ExcludeFlags_UIntExcludeFlags_ValueIsExcluded()
        {
            UIntFlagEnum thisFlag = UIntFlagEnum.Value2;
            UIntFlagEnum flagsToExclude = UIntFlagEnum.Value2;

            UIntFlagEnum valueAfterExclusion = thisFlag.ExcludeFlags(flagsToExclude);

            Assert.AreEqual(expected: 0u, actual: (uint)valueAfterExclusion, $"After exclusion, there shouldn't be any set bits left but that is not the case.");
        }

        internal enum UIntFlagEnum : uint
        {
            All = ~0u,
            None = 0u,
            Value1 = 1 << 0,
            Value2 = 1 << 1,
            Value3 = 1 << 2
        }
    }
}
