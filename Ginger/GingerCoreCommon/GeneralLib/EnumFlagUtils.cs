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
using System;

#nullable enable
namespace Amdocs.Ginger.Common.GeneralLib
{
    public enum TestEnum { Red, Green }

    public static class EnumFlagUtils
    {
        /// <summary>
        /// Determines whether all of the provided flags are set in the current value.
        /// </summary>
        /// <typeparam name="T">Type of flag enum.</typeparam>
        /// <param name="thisFlags">currently set flags.</param>
        /// <param name="flagsToVerify">flags to verify if they are set or not.</param>
        /// <returns><see langword="true"/> if all of the provided flags are set in the current value, otherwise <see langword="false"/>.</returns>
        public static bool AreAllFlagsSet<T>(this T thisFlags, T flagsToVerify) where T : Enum
        {
            ArgumentNullException.ThrowIfNull(flagsToVerify);

            //Binary operations explanation
            //commonSetBits = thisFlags & flagsToVerify (commonSetBits are only those bits are set which are common in both, e.g. 100101 & 010100 = 000100)
            //hasAllVerificationBits = commonSetBits == flagsToVerify (if commonSetBits has all the bits of flagsToVerify then, it will be equal to flagsToVerify, e.g. 010100 == 010100)

            if (TryCastToByteAndOperate(thisFlags, flagsToVerify, out byte thisFlagsAsByte, out byte flagsToVerifyAsByte))
            {
                byte commonSetBits = (byte)(thisFlagsAsByte & flagsToVerifyAsByte);
                bool hasAllVerificationBits = commonSetBits == flagsToVerifyAsByte;
                return hasAllVerificationBits;
            }
            else if (TryCastToShortAndOperate(thisFlags, flagsToVerify, out short thisFlagsAsShort, out short flagsToVerifyAsShort))
            {
                short commonSetBits = (short)(thisFlagsAsShort & flagsToVerifyAsShort);
                bool hasAllVerificationBits = commonSetBits == flagsToVerifyAsShort;
                return hasAllVerificationBits;
            }
            else if (TryCastToUShortAndOperate(thisFlags, flagsToVerify, out ushort thisFlagsAsUShort, out ushort flagsToVerifyAsUShort))
            {
                ushort commonSetBits = (ushort)(thisFlagsAsUShort & flagsToVerifyAsUShort);
                bool hasAllVerificationBits = commonSetBits == flagsToVerifyAsUShort;
                return hasAllVerificationBits;
            }
            else if (TryCastToIntAndOperate(thisFlags, flagsToVerify, out int thisFlagsAsInt, out int flagsToVerifyAsInt))
            {
                int commonSetBits = thisFlagsAsInt & flagsToVerifyAsInt;
                bool hasAllVerificationBits = commonSetBits == flagsToVerifyAsInt;
                return hasAllVerificationBits;
            }
            else if (TryCastToUIntAndOperate(thisFlags, flagsToVerify, out uint thisFlagsAsUInt, out uint flagsToVerifyAsUInt))
            {
                uint commonSetBits = thisFlagsAsUInt & flagsToVerifyAsUInt;
                bool hasAllVerificationBits = commonSetBits == flagsToVerifyAsUInt;
                return hasAllVerificationBits;
            }
            else if (TryCastToLongAndOperate(thisFlags, flagsToVerify, out long thisFlagsAsLong, out long flagsToVerifyAsLong))
            {
                long commonSetBits = thisFlagsAsLong & flagsToVerifyAsLong;
                bool hasAllVerificationBits = commonSetBits == flagsToVerifyAsLong;
                return hasAllVerificationBits;
            }
            else if (TryCastToULongAndOperate(thisFlags, flagsToVerify, out ulong thisFlagsAsULong, out ulong flagsToVerifyAsULong))
            {
                ulong commonSetBits = thisFlagsAsULong & flagsToVerifyAsULong;
                bool hasAllVerificationBits = commonSetBits == flagsToVerifyAsULong;
                return hasAllVerificationBits;
            }
            else
            {
                throw CreateExceptionForInvalidEnumUnderlyingType<T>();
            }
        }

        /// <summary>
        /// Determines whether any of the provided flags are set in the current value.
        /// </summary>
        /// <typeparam name="T">Type of flag enum.</typeparam>
        /// <param name="thisFlags">currently set flags.</param>
        /// <param name="flagsToVerify">flags to verify if they are set or not.</param>
        /// <returns><see langword="true"/> if any of the provided flags are set in the current value, otherwise <see langword="false"/>.</returns>
        public static bool AreAnyFlagsSet<T>(this T thisFlags, T flagsToVerify) where T : Enum
        {
            ArgumentNullException.ThrowIfNull(flagsToVerify);

            //Binary operations explanation
            //commonSetBits = thisFlags & flagsToVerify (commonSetBits are only those bits are set which are common in both, e.g. 100101 & 010100 = 000100)
            //hasAnySetBit = commonSetBits > 0 (if there is any bit set in commonSetBits then, it's value will be greater than 0)

            if (TryCastToByteAndOperate(thisFlags, flagsToVerify, out byte thisFlagsAsByte, out byte flagsToVerifyAsByte))
            {
                byte commonSetBits = (byte)(thisFlagsAsByte & flagsToVerifyAsByte);
                bool hasAnySetBit = commonSetBits > 0;
                return hasAnySetBit;
            }
            else if (TryCastToShortAndOperate(thisFlags, flagsToVerify, out short thisFlagsAsShort, out short flagsToVerifyAsShort))
            {
                short commonSetBits = (short)(thisFlagsAsShort & flagsToVerifyAsShort);
                bool hasAnySetBit = commonSetBits > 0;
                return hasAnySetBit;
            }
            else if (TryCastToUShortAndOperate(thisFlags, flagsToVerify, out ushort thisFlagsAsUShort, out ushort flagsToVerifyAsUShort))
            {
                ushort commonSetBits = (ushort)(thisFlagsAsUShort & flagsToVerifyAsUShort);
                bool hasAnySetBit = commonSetBits > 0;
                return hasAnySetBit;
            }
            else if (TryCastToIntAndOperate(thisFlags, flagsToVerify, out int thisFlagsAsInt, out int flagsToVerifyAsInt))
            {
                int commonSetBits = thisFlagsAsInt & flagsToVerifyAsInt;
                bool hasAnySetBit = commonSetBits > 0;
                return hasAnySetBit;
            }
            else if (TryCastToUIntAndOperate(thisFlags, flagsToVerify, out uint thisFlagsAsUInt, out uint flagsToVerifyAsUInt))
            {
                uint commonSetBits = thisFlagsAsUInt & flagsToVerifyAsUInt;
                bool hasAnySetBit = commonSetBits > 0;
                return hasAnySetBit;
            }
            else if (TryCastToLongAndOperate(thisFlags, flagsToVerify, out long thisFlagsAsLong, out long flagsToVerifyAsLong))
            {
                long commonSetBits = thisFlagsAsLong & flagsToVerifyAsLong;
                bool hasAnySetBit = commonSetBits > 0;
                return hasAnySetBit;
            }
            else if (TryCastToULongAndOperate(thisFlags, flagsToVerify, out ulong thisFlagsAsULong, out ulong flagsToVerifyAsULong))
            {
                ulong commonSetBits = thisFlagsAsULong & flagsToVerifyAsULong;
                bool hasAnySetBit = commonSetBits > 0;
                return hasAnySetBit;
            }
            else
            {
                throw CreateExceptionForInvalidEnumUnderlyingType<T>();
            }
        }

        /// <summary>
        /// Exclude the provided flags from the current value.
        /// </summary>
        /// <typeparam name="T">Type of flag enum.</typeparam>
        /// <param name="thisFlags">currently set flags.</param>
        /// <param name="flagsToExclude">flags to exclude from the current value.</param>
        /// <returns>Value with all the provided flags excluded from the current value.</returns>
        public static T ExcludeFlags<T>(this T thisFlags, T flagsToExclude) where T : Enum
        {
            ArgumentNullException.ThrowIfNull(flagsToExclude);

            //Binary operations explanation
            //commonSetBits = thisFlags & flagsToExclude (commonSetBits are only those bits are set which are common in both, e.g. 100101 & 010100 = 000100)
            //bitsAfterExclusion = thisFlags ^ commonSetBits (bitsAfterExclusion are those bits from thisFlags remaining after excluding commonSetBits, e.g. 100101 ^ 000100 = 100001)

            if (TryCastToByteAndOperate(thisFlags, flagsToExclude, out byte thisFlagsAsByte, out byte flagsToExcludeAsByte))
            {
                byte commonSetBits = (byte)(thisFlagsAsByte & flagsToExcludeAsByte);
                byte bitsAfterExclusion = (byte)(thisFlagsAsByte ^ commonSetBits);
                return (T)(object)bitsAfterExclusion;
            }
            else if (TryCastToShortAndOperate(thisFlags, flagsToExclude, out short thisFlagsAsShort, out short flagsToExcludeAsShort))
            {
                short commonSetBits = (short)(thisFlagsAsShort & flagsToExcludeAsShort);
                short bitsAfterExclusion = (short)(thisFlagsAsShort ^ commonSetBits);
                return (T)(object)bitsAfterExclusion;
            }
            else if (TryCastToUShortAndOperate(thisFlags, flagsToExclude, out ushort thisFlagsAsUShort, out ushort flagsToExcludeAsUShort))
            {
                ushort commonSetBits = (ushort)(thisFlagsAsUShort & flagsToExcludeAsUShort);
                ushort bitsAfterExclusion = (ushort)(thisFlagsAsUShort ^ commonSetBits);
                return (T)(object)bitsAfterExclusion;
            }
            else if (TryCastToIntAndOperate(thisFlags, flagsToExclude, out int thisFlagsAsInt, out int flagsToExcludeAsInt))
            {
                int commonSetBits = thisFlagsAsInt & flagsToExcludeAsInt;
                int bitsAfterExclusion = thisFlagsAsInt ^ commonSetBits;
                return (T)(object)bitsAfterExclusion;
            }
            else if (TryCastToUIntAndOperate(thisFlags, flagsToExclude, out uint thisFlagsAsUInt, out uint flagsToExcludeAsUInt))
            {
                uint commonSetBits = thisFlagsAsUInt & flagsToExcludeAsUInt;
                uint bitsAfterExclusion = thisFlagsAsUInt ^ commonSetBits;
                return (T)(object)bitsAfterExclusion;
            }
            else if (TryCastToLongAndOperate(thisFlags, flagsToExclude, out long thisFlagsAsLong, out long flagsToExcludeAsLong))
            {
                long commonSetBits = thisFlagsAsLong & flagsToExcludeAsLong;
                long bitsAfterExclusion = thisFlagsAsLong ^ commonSetBits;
                return (T)(object)bitsAfterExclusion;
            }
            else if (TryCastToULongAndOperate(thisFlags, flagsToExclude, out ulong thisFlagsAsULong, out ulong flagsToExcludeAsULong))
            {
                ulong commonSetBits = thisFlagsAsULong & flagsToExcludeAsULong;
                ulong bitsAfterExclusion = thisFlagsAsULong ^ commonSetBits;
                return (T)(object)bitsAfterExclusion;
            }
            else
            {
                throw CreateExceptionForInvalidEnumUnderlyingType<T>();
            }
        }

        public static T IncludeFlags<T>(this T thisFlags, T flagsToInclude) where T : Enum
        {
            ArgumentNullException.ThrowIfNull(flagsToInclude);

            //Binary operations explanation
            //combinedSetBits = thisFlags | flagsToExclude (combinedSetBits are those bits which are set in either, e.g. 100101 | 010100 = 110101)

            if (TryCastToByteAndOperate(thisFlags, flagsToInclude, out byte thisFlagsAsByte, out byte flagsToIncludeAsByte))
            {
                byte combinedSetBits = (byte)(thisFlagsAsByte | flagsToIncludeAsByte);
                return (T)(object)combinedSetBits;
            }
            else if (TryCastToShortAndOperate(thisFlags, flagsToInclude, out short thisFlagsAsShort, out short flagsToIncludeAsShort))
            {
                short combinedSetBits = (short)(thisFlagsAsShort | flagsToIncludeAsShort);
                return (T)(object)combinedSetBits;
            }
            else if (TryCastToUShortAndOperate(thisFlags, flagsToInclude, out ushort thisFlagsAsUShort, out ushort flagsToIncludeAsUShort))
            {
                ushort combinedSetBits = (ushort)(thisFlagsAsUShort | flagsToIncludeAsUShort);
                return (T)(object)combinedSetBits;
            }
            else if (TryCastToIntAndOperate(thisFlags, flagsToInclude, out int thisFlagsAsInt, out int flagsToIncludeAsInt))
            {
                int combinedSetBits = thisFlagsAsInt | flagsToIncludeAsInt;
                return (T)(object)combinedSetBits;
            }
            else if (TryCastToUIntAndOperate(thisFlags, flagsToInclude, out uint thisFlagsAsUInt, out uint flagsToIncludeAsUInt))
            {
                uint combinedSetBits = thisFlagsAsUInt | flagsToIncludeAsUInt;
                return (T)(object)combinedSetBits;
            }
            else if (TryCastToLongAndOperate(thisFlags, flagsToInclude, out long thisFlagsAsLong, out long flagsToIncludeAsLong))
            {
                long combinedSetBits = thisFlagsAsLong | flagsToIncludeAsLong;
                return (T)(object)combinedSetBits;
            }
            else if (TryCastToULongAndOperate(thisFlags, flagsToInclude, out ulong thisFlagsAsULong, out ulong flagsToIncludeAsULong))
            {
                ulong combinedSetBits = thisFlagsAsULong | flagsToIncludeAsULong;
                return (T)(object)combinedSetBits;
            }
            else
            {
                throw CreateExceptionForInvalidEnumUnderlyingType<T>();
            }
        }

        private static bool TryCastToByteAndOperate<T>(T value1, T value2, out byte convertedValue1, out byte convertedValue2) where T : Enum
        {
            if (Enum.GetUnderlyingType(typeof(T)) == typeof(byte))
            {
                IFormatProvider? nullFormatProvider = null;
                convertedValue1 = ((IConvertible)value1).ToByte(nullFormatProvider);
                convertedValue2 = ((IConvertible)value2).ToByte(nullFormatProvider);
                return true;
            }
            else
            {
                convertedValue1 = default;
                convertedValue2 = default;
                return false;
            }
        }

        private static bool TryCastToUShortAndOperate<T>(T value1, T value2, out ushort convertedValue1, out ushort convertedValue2) where T : Enum
        {
            if (Enum.GetUnderlyingType(typeof(T)) == typeof(ushort))
            {
                IFormatProvider? nullFormatProvider = null;
                convertedValue1 = ((IConvertible)value1).ToUInt16(nullFormatProvider);
                convertedValue2 = ((IConvertible)value2).ToUInt16(nullFormatProvider);
                return true;
            }
            else
            {
                convertedValue1 = default;
                convertedValue2 = default;
                return false;
            }
        }

        private static bool TryCastToShortAndOperate<T>(T value1, T value2, out short convertedValue1, out short convertedValue2) where T : Enum
        {
            if (Enum.GetUnderlyingType(typeof(T)) == typeof(short))
            {
                IFormatProvider? nullFormatProvider = null;
                convertedValue1 = ((IConvertible)value1).ToInt16(nullFormatProvider);
                convertedValue2 = ((IConvertible)value2).ToInt16(nullFormatProvider);
                return true;
            }
            else
            {
                convertedValue1 = default;
                convertedValue2 = default;
                return false;
            }
        }

        private static bool TryCastToUIntAndOperate<T>(T value1, T value2, out uint convertedValue1, out uint convertedValue2) where T : Enum
        {
            if (Enum.GetUnderlyingType(typeof(T)) == typeof(uint))
            {
                IFormatProvider? nullFormatProvider = null;
                convertedValue1 = ((IConvertible)value1).ToUInt32(nullFormatProvider);
                convertedValue2 = ((IConvertible)value2).ToUInt32(nullFormatProvider);
                return true;
            }
            else
            {
                convertedValue1 = default;
                convertedValue2 = default;
                return false;
            }
        }

        private static bool TryCastToIntAndOperate<T>(T value1, T value2, out int convertedValue1, out int convertedValue2) where T : Enum
        {
            if (Enum.GetUnderlyingType(typeof(T)) == typeof(int))
            {
                IFormatProvider? nullFormatProvider = null;
                convertedValue1 = ((IConvertible)value1).ToInt32(nullFormatProvider);
                convertedValue2 = ((IConvertible)value2).ToInt32(nullFormatProvider);
                return true;
            }
            else
            {
                convertedValue1 = default;
                convertedValue2 = default;
                return false;
            }
        }

        private static bool TryCastToULongAndOperate<T>(T value1, T value2, out ulong convertedValue1, out ulong convertedValue2) where T : Enum
        {
            if (Enum.GetUnderlyingType(typeof(T)) == typeof(ulong))
            {
                IFormatProvider? nullFormatProvider = null;
                convertedValue1 = ((IConvertible)value1).ToUInt64(nullFormatProvider);
                convertedValue2 = ((IConvertible)value2).ToUInt64(nullFormatProvider);
                return true;
            }
            else
            {
                convertedValue1 = default;
                convertedValue2 = default;
                return false;
            }
        }

        private static bool TryCastToLongAndOperate<T>(T value1, T value2, out long convertedValue1, out long convertedValue2) where T : Enum
        {
            if (Enum.GetUnderlyingType(typeof(T)) == typeof(long))
            {
                IFormatProvider? nullFormatProvider = null;
                convertedValue1 = ((IConvertible)value1).ToInt64(nullFormatProvider);
                convertedValue2 = ((IConvertible)value2).ToInt64(nullFormatProvider);
                return true;
            }
            else
            {
                convertedValue1 = default;
                convertedValue2 = default;
                return false;
            }
        }

        private static InvalidOperationException CreateExceptionForInvalidEnumUnderlyingType<T>()
        {
            return new InvalidOperationException($"{typeof(T).FullName} must inherit from {typeof(byte).Name}, {typeof(ushort)}, {typeof(short)}, {typeof(uint)}, {typeof(int)}, {typeof(ulong)} or {typeof(long)}.");
        }
    }
}
