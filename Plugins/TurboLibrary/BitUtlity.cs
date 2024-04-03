using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboLibrary
{
    /// <summary>
    /// A utility for coding with bit data.
    /// </summary>
    public static class BitUtility
    {
        public static bool HasBit(uint self, int index)
        {
            return (self & (1u << index)) != 0;
        }

        public static uint SetBit(uint flags, int val, bool set)
        {
            if (set)
                return flags | (uint)(1u << val);
            else
                return flags & ~(uint)(1u << val);
        }

        /// <summary>
        /// Decodes bits from the given byte with the bit position and number of bits to decode.
        /// </summary>
        public static uint DecodeBit(this byte value, int firstBit, int numBits)
        {
            return (uint)((value >> firstBit) & ((1u << numBits) - 1));
        }

        /// <summary>
        /// Decodes bits from the given ushort with the bit position and number of bits to decode.
        /// </summary>
        public static uint DecodeBit(this ushort value, int firstBit, int numBits)
        {
            return (uint)((value >> firstBit) & ((1u << numBits) - 1));
        }

        /// <summary>
        /// Decodes bits from the given uint32 with the bit position and number of bits to decode.
        /// </summary>
        public static uint DecodeBit(this uint value, int firstBit, int numBits)
        {
            return (uint)((value >> firstBit) & ((1u << numBits) - 1));
        }

        /// <summary>
        /// Encodes bits from the given byte with the bit position and number of bits to decode.
        /// </summary>
        public static ushort EncodeBit(this byte self, byte value, int firstBit, int bits)
        {
            // Clear the bits required for the value and fit it into them by truncating.
            ushort mask = (byte)(((1u << bits) - 1) << firstBit);
            self &= (byte)~mask;
            value = (byte)((value << firstBit) & mask);

            // Set the value.
            return (byte)(self | value);
        }

        /// <summary>
        /// Encodes bits from the given ushort with the bit position and number of bits to decode.
        /// </summary>
        public static ushort EncodeBit(this ushort self, ushort value, int firstBit, int bits)
        {
            // Clear the bits required for the value and fit it into them by truncating.
            ushort mask = (ushort)(((1u << bits) - 1) << firstBit);
            self &= (ushort)~mask;
            value = (ushort)((value << firstBit) & mask);

            // Set the value.
            return (ushort)(self | value);
        }

        /// <summary>
        /// Encodes bits from the given uint32 with the bit position and number of bits to decode.
        /// </summary>
        public static uint EncodeBit(this uint self, int value, int firstBit, int bits)
        {
            // Clear the bits required for the value and fit it into them by truncating.
            uint mask = ((1u << bits) - 1) << firstBit;
            self &= ~mask;
            value = (value << firstBit) & (int)mask;

            // Set the value.
            return (uint)(self | value);
        }

        /// <summary>
        /// Encodes bits from the given uint32 with the bit position and number of bits to decode.
        /// </summary>
        public static uint EncodeBit(this uint self, uint value, int firstBit, int bits)
        {
            // Clear the bits required for the value and fit it into them by truncating.
            uint mask = ((1u << bits) - 1) << firstBit;
            self &= ~mask;
            value = (value << firstBit) & mask;

            // Set the value.
            return self | value;
        }
    }
}