using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.Models.Scan
{
    /// <summary>
    /// Event handler for scan progress, sending data such as how many results and how many bytes
    /// </summary>
    public delegate void ScanProgressEventHandler(object sender, ScanProgressEventArgs e);

    /// <summary>
    /// Scan progress manager
    /// </summary>
    public class ScanProgress : BindableBase
    {
        private string scannedBytesText;
        private int resultCount;
        private double progressPercentage;
     

        private int updateStep = 0;
        private const int UPDATE_INTERVAL = 2;


        /// <summary>
        /// Show how many actual bytes have been scanned
        /// </summary>
        public string ScannedBytesText
        {
            get { return scannedBytesText; }
            set { SetProperty(ref scannedBytesText, value); }
        }

        /// <summary>
        /// How many results that have been found so far during scan
        /// </summary>
        public int ResultCount
        {
            get { return resultCount; }
            set { SetProperty(ref resultCount, value); }
        }

        /// <summary>
        /// Progress in percent
        /// </summary>
        public double ProgressPercentage
        {
            get { return progressPercentage; }
            set { SetProperty(ref progressPercentage, value); }
        }

        /// <summary>
        /// Scan progress event, invoke gui thread for setting progress
        /// </summary>
        public void ScanProgress_Progress(object sender, ScanProgressEventArgs e)
        {
            //// Dont update every scan cycle to avoid thread invokes
            //updateStep++;
            //if (updateStep < UPDATE_INTERVAL)
            //{
            //    return;
            //}
            //updateStep = 0;

            // Invoke main thread for progress report!
            App.Current.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                SetProgress(e);
            }));
        }

        /// <summary>
        /// Set scan progress!
        /// </summary>
        private void SetProgress(ScanProgressEventArgs e)
        {
            ScannedBytesText = ByteCalculator.GetHumanReadable(e.ByteCount);
            ResultCount = e.ResultCount;
            ProgressPercentage = 1000 * (double)e.ByteCount / (double)e.ExpectedBytes;
        }
    }

    /// <summary>
    /// Event args for scan progress
    /// </summary>
    public class ScanProgressEventArgs : EventArgs
    {
        public long ByteCount;
        public int ResultCount;
        public int ChunkCount;
        public long ExpectedBytes;
        
        public ScanProgressEventArgs(long byteCount, int resultCount, int chunkCount, long expectedBytes)
        {
            ByteCount = byteCount;
            ResultCount = resultCount;
            ChunkCount = chunkCount;
            ExpectedBytes = expectedBytes;
        }
    }
}
