using MemEditGUI.Models.Scan;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemEditGUI.Models.MemoryManagement
{
    /// <summary>
    /// A memory, agnostic of what type RAM or FILE
    /// Handles scanning, reading and writing memory
    /// </summary>
    public abstract class Memory
    {
        public IntPtr MinAddress;
        public IntPtr MaxAddress;

        public event ScanProgressEventHandler OnProgress;

        private volatile bool stopScan;


        /// <summary>
        /// Update results and tracked addresses
        /// </summary>
        public void Update(IEnumerable<ScanResult> results)
        {
            IEnumerable<MemoryChunk> chunks = SpecificChunks(results);
            foreach (MemoryChunk chunk in chunks)
            {
                UpdateChunk(chunk, results);
            }
        }

        /// <summary>
        /// Update addresses in a chunk
        /// </summary>
        private void UpdateChunk(MemoryChunk chunk, IEnumerable<ScanResult> results)
        {
            ChunkAddress chunkAddress = chunk.GetAddress();
            IEnumerable<ScanResult> resultsInChunk = results.Where(r => Equals(r.ChunkAddress, chunkAddress));

            foreach (ScanResult result in resultsInChunk)
            {
                byte[] buffer = new byte[result.CurrentValue.Length];
                Array.Copy(chunk.Data, result.RelativeAddress, buffer, 0, result.CurrentValue.Length);
                result.LastValue = result.CurrentValue;
                result.CurrentValue = buffer;
            }


        }

        /// <summary>
        /// User request scan to prematurely stop, set stop flag
        /// </summary>
        public void StopScan()
        {
            stopScan = true;
        }

        /// <summary>
        /// Scan memory!
        /// </summary>
        public ObservableCollection<ScanResult> Scan(ScanSettings settings, ObservableCollection<ScanResult> oldResults)
        {
            ObservableCollection<ScanResult> newResults = new ObservableCollection<ScanResult>();
            IEnumerable<MemoryChunk> chunks = null;

            // Stop scan flag
            stopScan = false;

            // Keep track of scan progress
            long byteCount = 0;
            int resultCount = 0;
            int chunkCount = 0;
            long expectedBytes = MemorySize();
        
            // No previous scan has been done, scan new!
            if (oldResults == null)
            {
                chunks = Chunks(settings);
            }

            // Base this scan on previous scan
            else
            {
                chunks = SpecificChunks(oldResults);
            }


            using (var seq = chunks.GetEnumerator())
            {
                bool hasChunks = seq.MoveNext();


                while (hasChunks)
                {
                    List<Thread> threads = new List<Thread>();
                    List<MemoryChunk> scanChunks = new List<MemoryChunk>();

                    // Setup threads
                    for (int t = 0; t < settings.ScanThreads; t++)
                    {
                        MemoryChunk chunk = seq.Current;

                        // Remember chunk
                        scanChunks.Add(chunk);

                        // Setup thread
                        Thread nt = new Thread(() =>
                        {
                            // Theese need to be here to not have the same in multiple threads
                            byte[] scanValue1 = DataTypeExtensions.ToBinary(settings.ValueOne, settings.ScanDataType, settings.IsHex);
                            byte[] scanValue2 = DataTypeExtensions.ToBinary(settings.ValueTwo, settings.ScanDataType, settings.IsHex);

                            chunk.Scan(settings, oldResults, scanValue1, scanValue2);
                        });

                        // Start thread and remember it
                        nt.Start();
                        threads.Add(nt);

                        // No more chunks?
                        if (!seq.MoveNext())
                        {
                            hasChunks = false;
                            break;
                        }
                    }

                    // Wait for threads to finish
                    foreach (Thread t in threads)
                    {
                        t.Join();
                    }

                    // Add results of scan to collection
                    foreach (MemoryChunk chunk in scanChunks)
                    {
                        chunkCount++;
                        byteCount += chunk.Data.Length;
                        resultCount += chunk.ScanResults.Count;
                        foreach (ScanResult r in chunk.ScanResults)
                        {
                            newResults.Add(r);

                            if (newResults.Count >= settings.MaxResults)
                            {
                                return newResults;
                            }
                        }
                    }

                    // Report progress
                    OnProgress(this, new ScanProgressEventArgs(byteCount, resultCount, chunkCount, expectedBytes));

                    // Prematurely stop scan by user request
                    if (stopScan)
                    {
                        break;
                    }
                }



            }

            return newResults;
        }




        /// <summary>
        /// Returns only specific chunks that are relevant to the scan
        /// </summary>
        private IEnumerable<MemoryChunk> SpecificChunks(IEnumerable<ScanResult> oldResults)
        {
            // Create a list of chunk addresses to read
            List<ChunkAddress> addresses = new List<ChunkAddress>();
            foreach (ScanResult r in oldResults)
            {
                if (!addresses.Contains(r.ChunkAddress))
                {
                    addresses.Add(r.ChunkAddress);
                }
            }

            // Read chunks belonging to the addresses
            foreach (ChunkAddress a in addresses)
            {
                yield return GetChunk(a);
            }
        }


        /// <summary>
        /// Returns chunks of memory
        /// </summary>
        protected abstract IEnumerable<MemoryChunk> Chunks(ScanSettings settings);


        /// <summary>
        /// Returns a specific chunk at a specific address
        /// </summary>
        protected abstract MemoryChunk GetChunk(ChunkAddress address);


        /// <summary>
        /// Returns how large memory is
        /// </summary>
        public abstract long MemorySize();


        /// <summary>
        /// Write memory to a specific address
        /// </summary>
        public abstract IntPtr Write(IntPtr address, byte[] data);
    }
}
