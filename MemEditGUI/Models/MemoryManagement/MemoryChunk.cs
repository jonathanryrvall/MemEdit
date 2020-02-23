using MemEditGUI.Models.Scan;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.Models.MemoryManagement
{
    /// <summary>
    /// Chunk of memory read from file or ram
    /// </summary>
    public class MemoryChunk
    {
        public byte[] Data;
        public IntPtr StartAddress;
        public List<ScanResult> ScanResults = null;

        /// <summary>
        /// Returns address of this chunk
        /// </summary>
        public ChunkAddress GetAddress()
        {
            return new ChunkAddress(StartAddress, (IntPtr)Data.Length);
        }

        /// <summary>
        /// Scan chunk
        /// Since unsafe code cannot appear in iterator it must return a list, this is no big deal
        /// </summary>
        public unsafe void Scan(ScanSettings settings, ObservableCollection<ScanResult> oldResults, byte[] scanValue1, byte[] scanValue2)
        {
            ScanResults = new List<ScanResult>();

            // Step size whole chunk scan
            int step = settings.StepActive ? settings.StepSize : 1;

            // Steps part chunk scan
            ChunkAddress chunkAddress = GetAddress();
            IEnumerable<ScanResult> resultsInChunk = oldResults?.Where(r => Equals(r.ChunkAddress, chunkAddress));

            // Prevent GC from messing around with chunk data and scan value
            fixed (byte* memValue = Data, scanVal1 = scanValue1, scanVal2 = scanValue2)
            {

                int length = Data.Length - scanValue1.Length;

                // New fresh scan
                if (oldResults == null)
                {
                    //return results;
                    for (int i = 0; i < length; i += step)
                    {


                        bool match = Match2(scanVal1, scanVal2, memValue + i, null, scanValue1.Length, settings);

                        if (match != settings.ScanNot)
                        {
                            ScanResult result = new ScanResult();
                            result.ChunkAddress = GetAddress();
                            result.DataType = settings.ScanDataType;
                            result.Address = StartAddress + i;
                            result.RelativeAddress = i;
                            result.CurrentValue = new byte[scanValue1.Length];
                            Array.Copy(Data, i, result.CurrentValue, 0, result.CurrentValue.Length);
                            ScanResults.Add(result);
                        }

                    }
                }

                // Based on previous scan
                else
                {
                    foreach (ScanResult oldResult in resultsInChunk)
                    {
                        bool match = false;
                        fixed (byte* old = oldResult.CurrentValue)
                        {
                            match = Match2(scanVal1, scanVal2, memValue + oldResult.RelativeAddress, old, scanValue1.Length, settings);
                        }

                        if (match != settings.ScanNot)
                        {
                            ScanResult result = new ScanResult();
                            result.ChunkAddress = oldResult.ChunkAddress;
                            result.DataType = oldResult.DataType;
                            result.Address = oldResult.Address;
                            result.RelativeAddress = oldResult.RelativeAddress;
                            result.LastValue = oldResult.CurrentValue;
                            result.CurrentValue = new byte[scanValue1.Length];
                            Array.Copy(Data, oldResult.RelativeAddress, result.CurrentValue, 0, result.CurrentValue.Length);

                            ScanResults.Add(result);
                        }
                    }
                }

            }
        
        }


        /// <summary>
        /// If address match conditions
        /// This method should contain as few sanity checks at possible
        /// </summary>
        private unsafe bool Match2(byte* findValue1, byte* findValue2, byte* memValue, byte* oldValue, int size, ScanSettings settings)
        {
            // Exact value comparison, doing a comparison directly in c# code appears to run faster can Pinvoking memcmp
            if (settings.ScanMode == ScanMode.ExactValue)
            {
                //byte* x1 = findValue1;
                //byte* x2 = memValue;
                //for (int i = 0; i < size / 8; i++, x2 += 8)
                //    if (*((long*)x1) != *((long*)x2)) return false;
                //if ((size & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x2 += 4; }
                //if ((size & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x2 += 2; }
                //if ((size & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;


                //#warning Compare with larger datatypes such as long and int
                for (int i = 0; i < size; i++)
                {
                    if (*((byte*)(findValue1 + i)) != *((byte*)(memValue + i)))
                    {
                        return false;
                    }
                }
                return true;
            }

            // Value in memory is greater than the value assigned by user
            if (settings.ScanMode == ScanMode.GreaterThan)
            {
                switch (settings.ScanDataType)
                {
                    case DataType.Int8:
                        return *((sbyte*)memValue) > *((sbyte*)findValue1);
                    case DataType.uInt8:
                        return *((byte*)memValue) > *((byte*)findValue1);
                    case DataType.Int16:
                        return *((short*)memValue) > *((short*)findValue1);
                    case DataType.uInt16:
                        return *((ushort*)memValue) > *((ushort*)findValue1);
                    case DataType.Int32:
                        return *((int*)memValue) > *((int*)findValue1);
                    case DataType.uInt32:
                        return *((uint*)memValue) > *((uint*)findValue1);
                    case DataType.Int64:
                        return *((long*)memValue) > *((long*)findValue1);
                    case DataType.uInt64:
                        return *((ulong*)memValue) > *((ulong*)findValue1);
                    case DataType.Float32:
                        return *((float*)memValue) > *((float*)findValue1);
                    case DataType.Double64:
                        return *((double*)memValue) > *((double*)findValue1);
                }
            }

            // Value in memory is less than the value assigned by user
            if (settings.ScanMode == ScanMode.LessThan)
            {
                switch (settings.ScanDataType)
                {
                    case DataType.Int8:
                        return *((sbyte*)memValue) < *((sbyte*)findValue1);
                    case DataType.uInt8:
                        return *((byte*)memValue) < *((byte*)findValue1);
                    case DataType.Int16:
                        return *((short*)memValue) < *((short*)findValue1);
                    case DataType.uInt16:
                        return *((ushort*)memValue) < *((ushort*)findValue1);
                    case DataType.Int32:
                        return *((int*)memValue) < *((int*)findValue1);
                    case DataType.uInt32:
                        return *((uint*)memValue) < *((uint*)findValue1);
                    case DataType.Int64:
                        return *((long*)memValue) < *((long*)findValue1);
                    case DataType.uInt64:
                        return *((ulong*)memValue) < *((ulong*)findValue1);
                    case DataType.Float32:
                        return *((float*)memValue) < *((float*)findValue1);
                    case DataType.Double64:
                        return *((double*)memValue) < *((double*)findValue1);
                }
            }

            // Value in memory is between the two values assigned by user
            if (settings.ScanMode == ScanMode.Between)
            {
                switch (settings.ScanDataType)
                {
                    case DataType.Int8:
                        return *((sbyte*)memValue) > *((sbyte*)findValue1) &&
                               *((sbyte*)memValue) < *((sbyte*)findValue2);
                    case DataType.uInt8:
                        return *((byte*)memValue) > *((byte*)findValue1) &&
                               *((byte*)memValue) < *((byte*)findValue2);
                    case DataType.Int16:
                        return *((short*)memValue) > *((short*)findValue1) &&
                               *((short*)memValue) < *((short*)findValue2);
                    case DataType.uInt16:
                        return *((ushort*)memValue) > *((ushort*)findValue1) &&
                               *((ushort*)memValue) < *((ushort*)findValue2);
                    case DataType.Int32:
                        return *((int*)memValue) > *((int*)findValue1) &&
                               *((int*)memValue) < *((int*)findValue2);
                    case DataType.uInt32:
                        return *((uint*)memValue) > *((uint*)findValue1) &&
                               *((uint*)memValue) < *((uint*)findValue2);
                    case DataType.Int64:
                        return *((long*)memValue) > *((long*)findValue1) &&
                               *((long*)memValue) < *((long*)findValue2);
                    case DataType.uInt64:
                        return *((ulong*)memValue) > *((ulong*)findValue1) &&
                               *((ulong*)memValue) < *((ulong*)findValue2);
                    case DataType.Float32:
                        return *((float*)memValue) > *((float*)findValue1) &&
                               *((float*)memValue) < *((float*)findValue2);
                    case DataType.Double64:
                        return *((double*)memValue) > *((double*)findValue1) &&
                               *((double*)memValue) < *((double*)findValue2);
                }
            }

            // Value has changed!
            if (settings.ScanMode == ScanMode.Changed)
            {
                for (int i = 0; i < size; i++)
                {
                    if (*((byte*)(oldValue + i)) != *((byte*)(memValue + i)))
                    {
                        return true;
                    }
                }
                return false;
            }



#warning Must use more long instead of int!

            // Value in memory has increased relative to old value
            if (settings.ScanMode == ScanMode.Increased)
            {
                switch (settings.ScanDataType)
                {
                    case DataType.Int8:
                        return *((sbyte*)memValue) > *((sbyte*)oldValue);
                    case DataType.uInt8:
                        return *((byte*)memValue) > *((byte*)oldValue);
                    case DataType.Int16:
                        return *((short*)memValue) > *((short*)oldValue);
                    case DataType.uInt16:
                        return *((ushort*)memValue) > *((ushort*)oldValue);
                    case DataType.Int32:
                        return *((int*)memValue) > *((int*)oldValue);
                    case DataType.uInt32:
                        return *((uint*)memValue) > *((uint*)oldValue);
                    case DataType.Int64:
                        return *((long*)memValue) > *((long*)oldValue);
                    case DataType.uInt64:
                        return *((ulong*)memValue) > *((ulong*)oldValue);
                    case DataType.Float32:
                        return *((float*)memValue) > *((float*)oldValue);
                    case DataType.Double64:
                        return *((double*)memValue) > *((double*)oldValue);
                }
            }

            // Value in memory has increased relative to old value by xxxx
            if (settings.ScanMode == ScanMode.IncreasedBy)
            {
                switch (settings.ScanDataType)
                {
                    case DataType.Int8:
                        return *((sbyte*)memValue) == *((sbyte*)oldValue) + *((sbyte*)findValue1);
                    case DataType.uInt8:
                        return *((byte*)memValue) == *((byte*)oldValue) + *((byte*)findValue1);
                    case DataType.Int16:
                        return *((short*)memValue) == *((short*)oldValue) + *((short*)findValue1);
                    case DataType.uInt16:
                        return *((ushort*)memValue) == *((ushort*)oldValue) + *((ushort*)findValue1);
                    case DataType.Int32:
                        return *((int*)memValue) == *((int*)oldValue) + *((int*)findValue1);
                    case DataType.uInt32:
                        return *((uint*)memValue) == *((uint*)oldValue) + *((uint*)findValue1);
                    case DataType.Int64:
                        return *((long*)memValue) == *((long*)oldValue) + *((long*)findValue1);
                    case DataType.uInt64:
                        return *((ulong*)memValue) == *((ulong*)oldValue) + *((ulong*)findValue1);
                    case DataType.Float32:
                        return *((float*)memValue) == *((float*)oldValue) + *((float*)findValue1);
                    case DataType.Double64:
                        return *((double*)memValue) == *((double*)oldValue) + *((double*)findValue1);
                }
            }

            // Value in memory has increased relative to old value by a minimum of xxxx
            if (settings.ScanMode == ScanMode.IncreasedByMin)
            {
                switch (settings.ScanDataType)
                {
                    case DataType.Int8:
                        return *((sbyte*)memValue) >= *((sbyte*)oldValue) + *((sbyte*)findValue1);
                    case DataType.uInt8:
                        return *((byte*)memValue) >= *((byte*)oldValue) + *((byte*)findValue1);
                    case DataType.Int16:
                        return *((short*)memValue) >= *((short*)oldValue) + *((short*)findValue1);
                    case DataType.uInt16:
                        return *((ushort*)memValue) >= *((ushort*)oldValue) + *((ushort*)findValue1);
                    case DataType.Int32:
                        return *((int*)memValue) >= *((int*)oldValue) + *((int*)findValue1);
                    case DataType.uInt32:
                        return *((uint*)memValue) >= *((uint*)oldValue) + *((uint*)findValue1);
                    case DataType.Int64:
                        return *((long*)memValue) >= *((long*)oldValue) + *((long*)findValue1);
                    case DataType.uInt64:
                        return *((ulong*)memValue) >= *((ulong*)oldValue) + *((ulong*)findValue1);
                    case DataType.Float32:
                        return *((float*)memValue) >= *((float*)oldValue) + *((float*)findValue1);
                    case DataType.Double64:
                        return *((double*)memValue) >= *((double*)oldValue) + *((double*)findValue1);
                }
            }

            // Value in memory has increased relative to old value by a maximum of xxxx
            if (settings.ScanMode == ScanMode.IncreasedByMax)
            {
                switch (settings.ScanDataType)
                {
                    case DataType.Int8:
                        return *((sbyte*)memValue) <= *((sbyte*)oldValue) + *((sbyte*)findValue1) &&
                               *((sbyte*)memValue) > *((sbyte*)oldValue);
                    case DataType.uInt8:
                        return *((byte*)memValue) <= *((byte*)oldValue) + *((byte*)findValue1) &&
                               *((byte*)memValue) > *((byte*)oldValue);
                    case DataType.Int16:
                        return *((short*)memValue) <= *((short*)oldValue) + *((short*)findValue1) &&
                               *((short*)memValue) > *((short*)oldValue);
                    case DataType.uInt16:
                        return *((ushort*)memValue) <= *((ushort*)oldValue) + *((ushort*)findValue1) &&
                               *((ushort*)memValue) > *((ushort*)oldValue);
                    case DataType.Int32:
                        return *((int*)memValue) <= *((int*)oldValue) + *((int*)findValue1) &&
                               *((int*)memValue) > *((int*)oldValue);
                    case DataType.uInt32:
                        return *((uint*)memValue) <= *((uint*)oldValue) + *((uint*)findValue1) &&
                               *((uint*)memValue) > *((uint*)oldValue);
                    case DataType.Int64:
                        return *((long*)memValue) <= *((long*)oldValue) + *((long*)findValue1) &&
                               *((long*)memValue) > *((long*)oldValue);
                    case DataType.uInt64:
                        return *((ulong*)memValue) <= *((ulong*)oldValue) + *((ulong*)findValue1) &&
                               *((ulong*)memValue) > *((ulong*)oldValue);
                    case DataType.Float32:
                        return *((float*)memValue) <= *((float*)oldValue) + *((float*)findValue1) &&
                               *((float*)memValue) > *((float*)oldValue);
                    case DataType.Double64:
                        return *((double*)memValue) <= *((double*)oldValue) + *((double*)findValue1) &&
                               *((double*)memValue) > *((double*)oldValue);
                }
            }


#warning decreased by min and decreased by max etc can probably be done in between and greater and smaller than methods saving code and calculations

            // Value in memory has decreased relative to old value
            if (settings.ScanMode == ScanMode.Decreased)
            {
                switch (settings.ScanDataType)
                {
                    case DataType.Int8:
                        return *((sbyte*)memValue) < *((sbyte*)oldValue);
                    case DataType.uInt8:
                        return *((byte*)memValue) < *((byte*)oldValue);
                    case DataType.Int16:
                        return *((short*)memValue) < *((short*)oldValue);
                    case DataType.uInt16:
                        return *((ushort*)memValue) < *((ushort*)oldValue);
                    case DataType.Int32:
                        return *((int*)memValue) < *((int*)oldValue);
                    case DataType.uInt32:
                        return *((uint*)memValue) < *((uint*)oldValue);
                    case DataType.Int64:
                        return *((long*)memValue) < *((long*)oldValue);
                    case DataType.uInt64:
                        return *((ulong*)memValue) < *((ulong*)oldValue);
                    case DataType.Float32:
                        return *((float*)memValue) < *((float*)oldValue);
                    case DataType.Double64:
                        return *((double*)memValue) < *((double*)oldValue);
                }
            }

            // Value in memory has decreased relative to old value by xxxx
            if (settings.ScanMode == ScanMode.DecreasedBy)
            {
                switch (settings.ScanDataType)
                {
                    case DataType.Int8:
                        return *((sbyte*)memValue) == *((sbyte*)oldValue) - *((sbyte*)findValue1);
                    case DataType.uInt8:
                        return *((byte*)memValue) == *((byte*)oldValue) - *((byte*)findValue1);
                    case DataType.Int16:
                        return *((short*)memValue) == *((short*)oldValue) - *((short*)findValue1);
                    case DataType.uInt16:
                        return *((ushort*)memValue) == *((ushort*)oldValue) - *((ushort*)findValue1);
                    case DataType.Int32:
                        return *((int*)memValue) == *((int*)oldValue) - *((int*)findValue1);
                    case DataType.uInt32:
                        return *((uint*)memValue) == *((uint*)oldValue) - *((uint*)findValue1);
                    case DataType.Int64:
                        return *((long*)memValue) == *((long*)oldValue) - *((long*)findValue1);
                    case DataType.uInt64:
                        return *((ulong*)memValue) == *((ulong*)oldValue) - *((ulong*)findValue1);
                    case DataType.Float32:
                        return *((float*)memValue) == *((float*)oldValue) - *((float*)findValue1);
                    case DataType.Double64:
                        return *((double*)memValue) == *((double*)oldValue) - *((double*)findValue1);
                }
            }

            // Value in memory has decreased relative to old value by a minimum of xxxx
            if (settings.ScanMode == ScanMode.DecreasedByMin)
            {
                switch (settings.ScanDataType)
                {
                    case DataType.Int8:
                        return *((sbyte*)memValue) <= *((sbyte*)oldValue) - *((sbyte*)findValue1);
                    case DataType.uInt8:
                        return *((byte*)memValue) <= *((byte*)oldValue) - *((byte*)findValue1);
                    case DataType.Int16:
                        return *((short*)memValue) <= *((short*)oldValue) - *((short*)findValue1);
                    case DataType.uInt16:
                        return *((ushort*)memValue) <= *((ushort*)oldValue) - *((ushort*)findValue1);
                    case DataType.Int32:
                        return *((int*)memValue) <= *((int*)oldValue) - *((int*)findValue1);
                    case DataType.uInt32:
                        return *((uint*)memValue) <= *((uint*)oldValue) - *((uint*)findValue1);
                    case DataType.Int64:
                        return *((long*)memValue) <= *((long*)oldValue) - *((long*)findValue1);
                    case DataType.uInt64:
                        return *((ulong*)memValue) <= *((ulong*)oldValue) - *((ulong*)findValue1);
                    case DataType.Float32:
                        return *((float*)memValue) <= *((float*)oldValue) - *((float*)findValue1);
                    case DataType.Double64:
                        return *((double*)memValue) <= *((double*)oldValue) - *((double*)findValue1);
                }
            }

            // Value in memory has decreased relative to old value by a maximum of xxxx
            if (settings.ScanMode == ScanMode.DecreasedByMax)
            {
                switch (settings.ScanDataType)
                {
                    case DataType.Int8:
                        return *((sbyte*)memValue) >= *((sbyte*)oldValue) - *((sbyte*)findValue1) &&
                               *((sbyte*)memValue) < *((sbyte*)oldValue);
                    case DataType.uInt8:
                        return *((byte*)memValue) >= *((byte*)oldValue) - *((byte*)findValue1) &&
                               *((byte*)memValue) < *((byte*)oldValue);
                    case DataType.Int16:
                        return *((short*)memValue) >= *((short*)oldValue) - *((short*)findValue1) &&
                               *((short*)memValue) < *((short*)oldValue);
                    case DataType.uInt16:
                        return *((ushort*)memValue) >= *((ushort*)oldValue) - *((ushort*)findValue1) &&
                               *((ushort*)memValue) < *((ushort*)oldValue);
                    case DataType.Int32:
                        return *((int*)memValue) >= *((int*)oldValue) - *((int*)findValue1) &&
                               *((int*)memValue) < *((int*)oldValue);
                    case DataType.uInt32:
                        return *((uint*)memValue) >= *((uint*)oldValue) - *((uint*)findValue1) &&
                               *((uint*)memValue) < *((uint*)oldValue);
                    case DataType.Int64:
                        return *((long*)memValue) >= *((long*)oldValue) - *((long*)findValue1) &&
                               *((long*)memValue) < *((long*)oldValue);
                    case DataType.uInt64:
                        return *((ulong*)memValue) >= *((ulong*)oldValue) - *((ulong*)findValue1) &&
                               *((ulong*)memValue) < *((ulong*)oldValue);
                    case DataType.Float32:
                        return *((float*)memValue) >= *((float*)oldValue) - *((float*)findValue1) &&
                               *((float*)memValue) < *((float*)oldValue);
                    case DataType.Double64:
                        return *((double*)memValue) >= *((double*)oldValue) - *((double*)findValue1) &&
                               *((double*)memValue) < *((double*)oldValue);
                }
            }

            return false;
        }
    }

    /// <summary>
    /// An address of a chunk, this is used to reduce the amount of read from memory in secondary and tertiary scans
    /// by only scanning the chunks where the first scan results exist
    /// </summary>
    public struct ChunkAddress
    {
        public IntPtr Address;
        public IntPtr Size;

        public ChunkAddress(IntPtr address, IntPtr size)
        {
            Address = address;
            Size = size;
        }
    }
}
