using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using MemEditGUI.Models.Scan;
using System.Windows.Shell;
using MemEditGUI.Models;
using MemEditGUI.Models.MemoryManagement;
using MemEditGUI.Views;

namespace MemEditGUI.ViewModels
{
    /// <summary>
    /// View model for the main edit and read memory view
    /// </summary>
    public class EditMemoryViewModel : ViewModelBase
    {
        public Action CloseAction { get; set; }

        public DelegateCommand ScanCommand { get; set; }
        public DelegateCommand ClearCommand { get; set; }
        public DelegateCommand UpdateScanCommand { get; set; }
        public RelayCommand UpdateTrackedCommand { get; set; }
        public RelayCommand DeleteResultCommand { get; set; }
        public RelayCommand TrackSelectedCommand { get; set; }
        public DelegateCommand TrackManualCommand { get; set; }
        public RelayCommand DeleteTrackCommand { get; set; }
        public RelayCommand WriteMemoryCommand { get; set; }
        public DelegateCommand StopScanCommand { get; set; }

        private MemorySource memorySource;

        private ScanProgress scanProgress;
        private ScanSettings scanSettings;
        private ObservableCollection<ScanResult> scanResults;
        private ObservableCollection<ScanResult> trackedAddresses;

        private int scanTime = 0;
        private bool isScanning = false;

        /// <summary>
        /// Scan progress management, setting this will also setup event
        /// </summary>
        public ScanProgress ScanProgress
        {
            get { return scanProgress; }
            set { SetProperty(ref scanProgress, value); }
        }

        /// <summary>
        /// Scan settings that are available to user
        /// </summary>
        public ScanSettings ScanSettings
        {
            get { return scanSettings; }
            set { SetProperty(ref scanSettings, value); }
        }

        /// <summary>
        /// Memory source
        /// </summary>
        public MemorySource MemorySource
        {
            get { return memorySource; }
            set { SetProperty(ref memorySource, value); }
        }

        /// <summary>
        /// Scan results duh!
        /// </summary>
        public ObservableCollection<ScanResult> ScanResults
        {
            get { return scanResults; }
            set { SetProperty(ref scanResults, value); }
        }

        /// <summary>
        /// Collection of tracked addresses
        /// </summary>
        public ObservableCollection<ScanResult> TrackedAddresses
        {
            get { return trackedAddresses; }
            set { SetProperty(ref trackedAddresses, value); }
        }

        /// <summary>
        /// Time it took for last scan
        /// </summary>
        public int ScanTime
        {
            get { return scanTime; }
            set { SetProperty(ref scanTime, value); }
        }

        /// <summary>
        /// If a scan is currently running
        /// </summary>
        public bool IsScanning
        {
            get { return isScanning; }
            set
            {
                SetProperty(ref isScanning, value);
                OnPropertyChanged("ScanProgressState");
            }
        }

        /// <summary>
        /// Taskbar progressbar state
        /// </summary>
        public TaskbarItemProgressState ScanProgressState
        {
            get { return isScanning ? TaskbarItemProgressState.Indeterminate : TaskbarItemProgressState.None; }
        }


        /// <summary>
        /// Ctor
        /// </summary>
        public EditMemoryViewModel()
        {
            ScanSettings = new ScanSettings();
            MemorySource = GlobalStates.Instance.MemorySource;
            TrackedAddresses = new ObservableCollection<ScanResult>();
            ScanSettings.MinAddress = GlobalStates.Instance.Memory.MinAddress;
            ScanSettings.MaxAddress = GlobalStates.Instance.Memory.MaxAddress;

            // Scan progress
            ScanProgress = new ScanProgress();
            GlobalStates.Instance.Memory.OnProgress += scanProgress.ScanProgress_Progress;

            // Command bindings
            ClearCommand = new DelegateCommand(ClearButton_Click);
            ScanCommand = new DelegateCommand(ScanButton_Click);
            TrackSelectedCommand = new RelayCommand(TrackSelectedButton_Click);
            DeleteTrackCommand = new RelayCommand(DeleteTrackButton_Click);
            DeleteResultCommand = new RelayCommand(DeleteResultButton_Click);
            WriteMemoryCommand = new RelayCommand(WriteMemoryButton_Click);
            TrackManualCommand = new DelegateCommand(TrackManualButton_Click);
            UpdateScanCommand = new DelegateCommand(UpdateScanButton_Click);
            UpdateTrackedCommand = new RelayCommand(UpdateTrackedButton_Click);
            StopScanCommand = new DelegateCommand(StopScanButton_Click);
        }

        /// <summary>
        /// Clear scan
        /// </summary>
        private void ClearButton_Click()
        {
            ScanResults = null;
        }



        /// <summary>
        /// Clear scan
        /// </summary>
        private void ScanButton_Click()
        {
            // Check if scan is possible
            string errorMessage = "";
            if (!scanSettings.CanScan(scanResults != null, out errorMessage))
            {
                MessageBoxView.Show("MemEdit - Cannot scan!", errorMessage, MessageLevel.Error);
                return;
            }

            // Disable view during scan
            IsScanning = true;

            // Start stopwatch
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Actual scan is done in separate thread to not freeze ui
            Task.Run(() =>
            {
                // Do actual scan
                var scanResults = GlobalStates.Instance.Memory.Scan(scanSettings, ScanResults);

                // Stop stopwatch
                stopwatch.Stop();

                // No longer scanning
                IsScanning = false;

                // Set scan time in main thread
                App.Current.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    ScanResults = scanResults;
                    ScanTime = (int)stopwatch.ElapsedMilliseconds;
                }));
            });
        }

        /// <summary>
        /// Stop scan!
        /// </summary>
        private void StopScanButton_Click()
        {
            GlobalStates.Instance.Memory.StopScan();
        }

        /// <summary>
        /// Track selected process
        /// </summary>
        private void TrackSelectedButton_Click(object selectedAddresses)
        {
            foreach (ScanResult r in (IList)selectedAddresses)
            {
                trackedAddresses.Add(r);
            }
        }

        /// <summary>
        /// Track selected address
        /// </summary>
        private void TrackManualButton_Click()
        {
            trackedAddresses.Add(new ScanResult());
        }

        /// <summary>
        /// Update address values
        /// </summary>
        private void UpdateScanButton_Click()
        {
            GlobalStates.Instance.Memory.Update(scanResults);
        }

        /// <summary>
        /// Update address values
        /// </summary>
        private void UpdateTrackedButton_Click(object selectedAddresses)
        {
            IEnumerable<ScanResult> updateAddresses = (selectedAddresses as IList).Cast<ScanResult>();
            GlobalStates.Instance.Memory.Update(updateAddresses);
        }

        /// <summary>
        /// Delete selected track
        /// </summary>
        private void DeleteTrackButton_Click(object selectedAddresses)
        {
            List<ScanResult> deleteAddresses = new List<ScanResult>();
            foreach (ScanResult r in (IList)selectedAddresses)
            {
                deleteAddresses.Add(r);
            }
            foreach (ScanResult r in deleteAddresses)
            {
                trackedAddresses.Remove(r);
            }
        }

        /// <summary>
        /// Delete selected scan result
        /// </summary>
        private void DeleteResultButton_Click(object selectedAddresses)
        {
            List<ScanResult> deleteAddresses = new List<ScanResult>();
            foreach (ScanResult r in (IList)selectedAddresses)
            {
                deleteAddresses.Add(r);
            }
            foreach (ScanResult r in deleteAddresses)
            {
                scanResults.Remove(r);
            }
        }


        /// <summary>
        /// Write memory of selected address
        /// </summary>
        private void WriteMemoryButton_Click(object selectedAddresses)
        {
            bool showWarning = (selectedAddresses as IList).Count > 1;
            string writeValue = InputBoxView.Show(showWarning);
            if (writeValue == "")
            {
                return;
            }
            foreach (ScanResult r in selectedAddresses as IList)
            {
                byte[] writeData = DataTypeExtensions.ToBinary(writeValue, r.DataType, scanSettings.IsHex);
                GlobalStates.Instance.Memory.Write(r.Address, writeData);
            }
        }

        /// <summary>
        /// Window is closing!
        /// </summary>
        public void Closing(object sender, CancelEventArgs e)
        {
            // Close handle to process
            if (GlobalStates.Instance.Memory is RAM)
            {
                (GlobalStates.Instance.Memory as RAM).CloseHandle();
            }
        }



#warning Check so scan conditions are legal
    }
}
