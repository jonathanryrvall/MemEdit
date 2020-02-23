using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using MemEditGUI.Extensions;

namespace MemEditGUI.Models.Scan
{

    /// <summary>
    /// Settings for scan
    /// </summary>
    public class ScanSettings : BindableBase
    {
        private string valueOne = "";
        private string valueTwo = "";

        private bool scanNot = false;
        private DataType scanDataType = DataType.Int32;
        private ScanMode scanMode;
        private bool isHex;
        private IntPtr minAddress;
        private IntPtr maxAddress;
        private bool stepActive = false;
        private int stepSize = 4;
        private int maxResults = 10000;
        private int scanThreads = 2;
        private int maxScanThreads = 4;

     
        public ScanSettings()
        {
            GetMaxScanThreads();
        }

        /// <summary>
        /// Retrieve and store the max count of threads of the cpu(s)
        /// </summary>
        private void GetMaxScanThreads()
        {
            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
            {
                MaxScanThreads = int.Parse(item["NumberOfLogicalProcessors"].ToString());
                break;
            }
        }

        /// <summary>
        /// Min scan value in string format
        /// </summary>
        public string ValueOne
        {
            get { return valueOne; }
            set { SetProperty(ref valueOne, value); }
        }

        /// <summary>
        /// Max scan value in string format
        /// </summary>
        public string ValueTwo
        {
            get { return valueTwo; }
            set { SetProperty(ref valueTwo, value); }
        }

        /// <summary>
        /// Returns true if value two field should be editable
        /// </summary>
        public bool ValueOneEnabled
        {
            get
            {
                return ScanMode != ScanMode.Changed &&
                       ScanMode != ScanMode.Decreased &&
                       ScanMode != ScanMode.Increased;
            }
        }

        /// <summary>
        /// Returns true if value two field should be editable
        /// </summary>
        public bool ValueTwoEnabled
        {
            get { return ScanMode == ScanMode.Between; }
        }


        /// <summary>
        /// Invert scan conditions, add everything that does not match!
        /// </summary>
        public bool ScanNot
        {
            get { return scanNot; }
            set { SetProperty(ref scanNot, value); }
        }

        /// <summary>
        /// Datatype to scan for
        /// </summary>
        public DataType ScanDataType
        {
            get { return scanDataType; }
            set { SetProperty(ref scanDataType, value); }
        }

        /// <summary>
        /// Mode to scan, if value is larger, smaller etc
        /// </summary>
        public ScanMode ScanMode
        {
            get { return scanMode; }
            set
            {
                SetProperty(ref scanMode, value);
                OnPropertyChanged("ValueOneEnabled");
                OnPropertyChanged("ValueTwoEnabled");
            }
        }

        /// <summary>
        /// If scan value is written in hexadecimal
        /// </summary>
        public bool IsHex
        {
            get { return isHex; }
            set { SetProperty(ref isHex, value); }
        }

        /// <summary>
        /// Get a list of datatypes that can be scanned
        /// </summary>
        public static Dictionary<DataType, string> ScanDataTypes
        {
            get
            {
                var dataTypes = new Dictionary<DataType, string>();

                foreach (DataType t in (DataType[])Enum.GetValues(typeof(DataType)))
                {
                    string desc = EnumExtensions.Description(t);
                    dataTypes.Add(t, desc);
                }
                return dataTypes;
            }
        }

        /// <summary>
        /// Get a list of modes that can be scanned
        /// </summary>
        public Dictionary<ScanMode, string> ScanModes
        {
            get
            {
                var scanModes = new Dictionary<ScanMode, string>();

                foreach (ScanMode t in (ScanMode[])Enum.GetValues(typeof(ScanMode)))
                {
                    string desc = EnumExtensions.Description(t);
                    scanModes.Add(t, desc);
                }
                return scanModes;
            }
        }

        /// <summary>
        /// Min address as a IntPtr
        /// </summary>
        public IntPtr MinAddress
        {
            get { return minAddress; }
            set { SetProperty(ref minAddress, value); }
        }

        /// <summary>
        /// Max address as a IntPtr
        /// </summary>
        public IntPtr MaxAddress
        {
            get { return maxAddress; }
            set { SetProperty(ref maxAddress, value); }
        }

        /// <summary>
        /// If does steps in scan should be active or not
        /// </summary>
        public bool StepActive
        {
            get { return stepActive; }
            set { SetProperty(ref stepActive, value); }
        }

        /// <summary>
        /// Size of steps
        /// </summary>
        public int StepSize
        {
            get { return stepSize; }
            set { SetProperty(ref stepSize, value); }
        }

        /// <summary>
        /// Max results count, to prevent stupid queries to freeze the application
        /// </summary>
        public int MaxResults
        {
            get { return maxResults; }
            set { SetProperty(ref maxResults, value); }
        }

    
        /// <summary>
        /// Max results count, to prevent stupid queries to freeze the application
        /// </summary>
        public int ScanThreads
        {
            get { return scanThreads; }
            set { SetProperty(ref scanThreads, value); }
        }

        /// <summary>
        /// Max scan threads
        /// </summary>
        public int MaxScanThreads
        {
            get { return maxScanThreads; }
            set { SetProperty(ref maxScanThreads, value); }
        }


        /// <summary>
        /// Min address in hexadecimal format
        /// </summary>
        public string HexMinAddress
        {
            get
            {
                return ((long)minAddress).ToString("X16");
            }
            set
            {
                minAddress = (IntPtr)long.Parse(value, NumberStyles.HexNumber);
            }
        }

        /// <summary>
        /// Max address in hexadecimal format
        /// </summary>
        public string HexMaxAddress
        {
            get
            {
                return ((long)maxAddress).ToString("X16");
            }
            set
            {
                minAddress = (IntPtr)long.Parse(value, NumberStyles.HexNumber);
            }
        }

        /// <summary>
        /// Can scan with the settings user entered?
        /// </summary>
        public bool CanScan(bool previousScan, out string message)
        {
            // Scan modes unavailable at first scan
            if (!previousScan)
            {
                if (scanMode == ScanMode.Changed ||
                    scanMode == ScanMode.Decreased ||
                    scanMode == ScanMode.DecreasedBy ||
                    scanMode == ScanMode.DecreasedByMax ||
                    scanMode == ScanMode.DecreasedByMin ||
                    scanMode == ScanMode.Increased ||
                    scanMode == ScanMode.IncreasedBy ||
                    scanMode == ScanMode.IncreasedByMax ||
                    scanMode == ScanMode.IncreasedByMin)
                {
                    message = "Cannot use this scan mode when no previous scan has been made!";
                    return false;
                }
            }

            // Value 1 larger than value two?
            if (scanMode == ScanMode.Between)
            {
               
            }

            message = "";
            return true;
        }

    }
}
