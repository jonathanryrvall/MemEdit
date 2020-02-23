using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.Models.Scan
{
    public enum DataType
    {
        [Description("Signed 8 bit integer")]
        Int8,
        [Description("Signed 16 bit integer")]
        Int16,
        [Description("Signed 32 bit integer")]
        Int32,
        [Description("Signed 64 bit integer")]
        Int64,
        [Description("Unsigned 8 bit integer")]
        uInt8,
        [Description("Unsigned 16 bit integer")]
        uInt16,
        [Description("Unsigned 32 bit integer")]
        uInt32,
        [Description("Unsigned 64 bit integer")]
        uInt64,
        [Description("32 bit float")]
        Float32,
        [Description("64 bit double")]
        Double64,
        [Description("ASCII String (8 Bit/Character)")]
        ASCIIString,
        [Description("Unicode String (16 Bit/Character)")]
        UnicodeString,
    }

    public static class DataTypeExtensions
    {
        /// <summary>
        /// Returns the bytearray converted to a decimal number depending on scan datatype
        /// </summary>
        public static decimal ToDecimal(byte[] data,int i, DataType dataType)
        {
            double val = 0;

            switch (dataType)
            {
                case DataType.Int8:
                    return data[i] + 128;
                case DataType.uInt8:
                    return data[i];
                case DataType.Int16:
                    return BitConverter.ToInt16(data, i);
                case DataType.uInt16:
                    return BitConverter.ToUInt16(data, i);
                case DataType.Int32:
                    return BitConverter.ToInt32(data, i);
                case DataType.uInt32:
                    return BitConverter.ToUInt32(data, i);
                case DataType.Int64:
                    return BitConverter.ToInt64(data, i);
                case DataType.uInt64:
                    return BitConverter.ToUInt64(data, i);
                case DataType.Float32:
                    val = BitConverter.ToSingle(data, i);
                    break;
                case DataType.Double64:
                    val = BitConverter.ToDouble(data, i);
                    break;
            }

            // Clamp doubles and floats since ther bounds are far bigger than decimal
            if (val > (double)decimal.MaxValue)
            {
                return decimal.MaxValue;
            }
            if (val < (double)decimal.MinValue)
            {
                return decimal.MinValue;
            }

            return (decimal)val;
        }

        /// <summary>
        /// Returns the bytearray converted to a string depending on scan datatype
        /// </summary>
        public static string ToString(byte[] data, int i, DataType dataType)
        {
            if (data == null)
            {
                return "";
            }
            if (dataType == DataType.ASCIIString)
            {
                return Encoding.ASCII.GetString(data);
            }
            if (dataType == DataType.UnicodeString)
            {
                return Encoding.Unicode.GetString(data);
            }
            return ToDecimal(data, 0, dataType).ToString();
        }

        /// <summary>
        /// Parse a string value into bytes depending on settings
        /// </summary>
        public static byte[] ToBinary(string str, DataType dataType, bool  hex)
        {
            decimal dec = 0;
            
            // Parse hexadacimal value
            if (hex)
            {
                long hexVal = 0;
                long.TryParse(str, NumberStyles.HexNumber, null, out hexVal);
                dec = hexVal;
            }
            else
            {
                decimal.TryParse(str, out dec);
            }
     
            switch (dataType)
            {
                case DataType.Int8:
                    return BitConverter.GetBytes((sbyte)dec);
                case DataType.Int16:
                    return BitConverter.GetBytes((short)dec);
                case DataType.Int32:
                    return BitConverter.GetBytes((int)dec);
                case DataType.Int64:
                    return BitConverter.GetBytes((long)dec);
                case DataType.uInt8:
                    return BitConverter.GetBytes((byte)dec);
                case DataType.uInt16:
                    return BitConverter.GetBytes((ushort)dec);
                case DataType.uInt32:
                    return BitConverter.GetBytes((uint)dec);
                case DataType.uInt64:
                    return BitConverter.GetBytes((ulong)dec);
                case DataType.Float32:
                    return BitConverter.GetBytes((float)dec);
                case DataType.Double64:
                    return BitConverter.GetBytes((double)dec);
                case DataType.ASCIIString:
                    return Encoding.ASCII.GetBytes(str);
                case DataType.UnicodeString:
                    return Encoding.Unicode.GetBytes(str);
            }

            return Encoding.ASCII.GetBytes("Placeholder");
        }

    }
}
