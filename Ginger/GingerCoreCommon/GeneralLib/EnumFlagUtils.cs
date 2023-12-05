using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.Common.GeneralLib
{
    public enum TestEnum { Red, Green }

    public static class EnumFlagUtils
    {
        /// <summary>
        /// Determines whether all the provided flags are set in the current value.
        /// </summary>
        /// <typeparam name="T">Type of flag enum.</typeparam>
        /// <param name="thisFlags">currently set flags.</param>
        /// <param name="flagsToVerify">flags to verify if they are set or not.</param>
        /// <returns><see langword="true"/> if all the provided flags are set in the current value, otherwise <see langword="false"/>.</returns>
        public static bool AreFlagsSet<T>(this T thisFlags, T flagsToVerify) where T : Enum
        {
            ArgumentNullException.ThrowIfNull(flagsToVerify);
            if(TryCastToByteAndOperate(thisFlags, flagsToVerify, out byte thisFlagsAsByte, out byte flagsToVerifyAsByte))
            {
                return (thisFlagsAsByte & flagsToVerifyAsByte) == flagsToVerifyAsByte;
            }
            else if(TryCastToShortAndOperate(thisFlags, flagsToVerify, out short thisFlagsAsShort, out short flagsToVerifyAsShort))
            {
                return (thisFlagsAsShort & flagsToVerifyAsShort) == flagsToVerifyAsShort;
            }
            else if(TryCastToUShortAndOperate(thisFlags, flagsToVerify, out ushort thisFlagsAsUShort, out ushort flagsToVerifyAsUShort))
            {
                return (thisFlagsAsUShort & flagsToVerifyAsUShort) == flagsToVerifyAsUShort;
            }
            else if(TryCastToIntAndOperate(thisFlags, flagsToVerify, out int thisFlagsAsInt, out int flagsToVerifyAsInt))
            {
                return (thisFlagsAsInt & flagsToVerifyAsInt) == flagsToVerifyAsInt;
            }
            else if(TryCastToUIntAndOperate(thisFlags, flagsToVerify, out uint thisFlagsAsUInt, out uint flagsToVerifyAsUInt))
            {
                return (thisFlagsAsUInt & flagsToVerifyAsUInt) == flagsToVerifyAsUInt;
            }
            else if (TryCastToLongAndOperate(thisFlags, flagsToVerify, out long thisFlagsAsLong, out long flagsToVerifyAsLong))
            {
                return (thisFlagsAsLong & flagsToVerifyAsLong) == flagsToVerifyAsLong;
            }
            else
            {
                throw CreateExceptionForInvalidEnumUnderlyingType<T>();
            }
        }

        public static T ExcludeFlags<T>(this T thisFlags, T flagsToExclude) where T : Enum
        {
            ArgumentNullException.ThrowIfNull(flagsToExclude);
            //we first do an bitwise AND (&) operation between thisFlags and flagsToExclude, this will give us only those bits which are set in both thisFlags and flagsToExclude
            //then we do a bitwise XOR (^) operation between thisFlags and bits from AND operation, this give us only those bits which are set in thisFlags but not set in AND operation output
            if (TryCastToByteAndOperate(thisFlags, flagsToExclude, out byte thisFlagsAsByte, out byte flagsToExcludeAsByte))
            {
                return (T)(object)(thisFlagsAsByte ^ (thisFlagsAsByte & flagsToExcludeAsByte));
            }
            else if (TryCastToShortAndOperate(thisFlags, flagsToExclude, out short thisFlagsAsShort, out short flagsToExcludeAsShort))
            {
                return (T)(object)(thisFlagsAsShort ^ (thisFlagsAsShort & flagsToExcludeAsShort));
            }
            else if (TryCastToUShortAndOperate(thisFlags, flagsToExclude, out ushort thisFlagsAsUShort, out ushort flagsToExcludeAsUShort))
            {
                return (T)(object)(thisFlagsAsUShort ^ (thisFlagsAsUShort & flagsToExcludeAsUShort));
            }
            else if (TryCastToIntAndOperate(thisFlags, flagsToExclude, out int thisFlagsAsInt, out int flagsToExcludeAsInt))
            {
                return (T)(object)(thisFlagsAsInt ^ (thisFlagsAsInt & flagsToExcludeAsInt));
            }
            else if (TryCastToUIntAndOperate(thisFlags, flagsToExclude, out uint thisFlagsAsUInt, out uint flagsToExcludeAsUInt))
            {
                return (T)(object)(thisFlagsAsUInt ^ (thisFlagsAsUInt & flagsToExcludeAsUInt));
            }
            else if (TryCastToLongAndOperate(thisFlags, flagsToExclude, out long thisFlagsAsLong, out long flagsToExcludeAsLong))
            {
                return (T)(object)(thisFlagsAsLong ^ (thisFlagsAsLong & flagsToExcludeAsLong));
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
