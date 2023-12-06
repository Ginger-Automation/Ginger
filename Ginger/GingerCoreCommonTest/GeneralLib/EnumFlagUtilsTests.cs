using Amdocs.Ginger.Common.GeneralLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
