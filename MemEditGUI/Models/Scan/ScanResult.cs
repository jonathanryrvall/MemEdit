using MemEditGUI.Models.MemoryManagement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.Models.Scan
{
    public class ScanResult : BindableBase
    {
        private IntPtr address;
        private long relativeAddress;
        private byte[] currentValue;
        private byte[] lastValue;
        private string description;
        private DataType dataType;
        private ChunkAddress chunkAddress;
     
        /// <summary>
        /// Address shown to user in hexadecimal form
        /// </summary>
        public string ShowAddress
        {
            get
            {
                return ((long)address).ToString("X16");
            }
            set
            {
                address = (IntPtr)long.Parse(value, NumberStyles.HexNumber);
            }
        }

        /// <summary>
        /// Address in memory
        /// </summary>
        public IntPtr Address
        {
            get { return address; }
            set { SetProperty(ref address, value); }
        }

        /// <summary>
        /// Address relative to chunk start address
        /// </summary>
        public long RelativeAddress
        {
            get { return relativeAddress; }
            set { SetProperty(ref relativeAddress, value); }
        }

        /// <summary>
        /// Current value in scan
        /// </summary>
        public byte[] CurrentValue
        {
            get { return currentValue; }
            set
            {
                SetProperty(ref currentValue, value);
                OnPropertyChanged("ShowCurrentValue");
            }
        }

        /// <summary>
        /// Last scan value
        /// </summary>
        public byte[] LastValue
        {
            get { return lastValue; }
            set
            {
                SetProperty(ref lastValue, value);
                OnPropertyChanged("ShowLastValue");
            }
        }

        /// <summary>
        /// Description assigned by user
        /// </summary>
        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }
        
        /// <summary>
        /// Data type
        /// </summary>
        public DataType DataType
        {
            get { return dataType; }
            set { SetProperty(ref dataType, value); }
        }

        /// <summary>
        /// Address of chunk
        /// </summary>
        public ChunkAddress ChunkAddress
        {
            get { return chunkAddress; }
            set { SetProperty(ref chunkAddress, value); }
        }

        /// <summary>
        /// The current value translated into a human readable string
        /// </summary>
        public string ShowCurrentValue
        {
            get { return DataTypeExtensions.ToString(currentValue, 0, dataType).ToString(); }
        }

        /// <summary>
        /// The last value translated into a human readable string
        /// </summary>
        public string ShowLastValue
        {
            get { return DataTypeExtensions.ToString(lastValue, 0, dataType).ToString(); }
        }



    }
}
