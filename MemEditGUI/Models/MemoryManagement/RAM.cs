using MemEditGUI.Models.Scan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.Models.MemoryManagement
{
    /// <summary>
    /// Ram memory
    /// </summary>
    public class RAM : Memory
    {
        private RAMCalls.SYSTEM_INFO sys_info;
        private IntPtr processHandle;
        private Process process;

        /// <summary>
        /// Create a new instance of RAM memory manager
        /// </summary>
        public RAM(Process process)
        {
            // Remember process
            this.process = process;

            // Getting minimum & maximum address
            sys_info = new RAMCalls.SYSTEM_INFO();
            RAMCalls.GetSystemInfo(out sys_info);

            // Opening the process with desired access level
            int accessLevel = (int)(ProcessAccessTypes.PROCESS_QUERY_INFORMATION | ProcessAccessTypes.PROCESS_VM_READ | ProcessAccessTypes.PROCESS_VM_WRITE);
            processHandle = RAMCalls.OpenProcess(accessLevel, false, process.Id);
          
            // Get ram memory size, since there is no point scannin gnon existent memory!
            MinAddress = sys_info.minimumApplicationAddress;
            MaxAddress = sys_info.maximumApplicationAddress;
        }

        /// <summary>
        /// Close handle
        /// </summary>
        public void CloseHandle()
        {
            RAMCalls.CloseHandle(processHandle);
        }

        /// <summary>
        /// Returns chunks of RAM memory
        /// </summary>
        protected override IEnumerable<MemoryChunk> Chunks(ScanSettings settings)
        {
            var memInfo = new RAMCalls.MEMORY_BASIC_INFORMATION();

            // Set min and max address based on RAM memory limitations
            IntPtr min = MinAddress;
            IntPtr max = MaxAddress;

            // Clamp memory based on scan settings
            min = (long)min > (long)settings.MinAddress ? min : settings.MinAddress;
            max = (long)max < (long)settings.MaxAddress ? max : settings.MaxAddress;


            while ((long)min < (long)max)
            {
                IntPtr memDump = RAMCalls.VirtualQueryEx(processHandle, min, out memInfo, (uint)Marshal.SizeOf(memInfo));

                //if (memDump == (IntPtr)0)
                //{
                //    int error = Marshal.GetLastWin32Error();
                //}


                if (memInfo.Protect == RAMCalls.AllocationProtectEnum.PAGE_READWRITE && memInfo.State == RAMCalls.StateEnum.MEM_COMMIT)
                {
                    byte[] buffer = new byte[(long)memInfo.RegionSize];
                    int bytesRead = 0;
                    RAMCalls.ReadProcessMemory((int)processHandle, memInfo.BaseAddress, buffer, memInfo.RegionSize, ref bytesRead);

                    MemoryChunk chunk = new MemoryChunk();
                    chunk.Data = buffer;
                    chunk.StartAddress = memInfo.BaseAddress;
                    yield return chunk;
                }
                min = (IntPtr)((long)min + (long)memInfo.RegionSize);
            }
        }

        /// <summary>
        /// Returns how large memory is
        /// </summary>
        public override long MemorySize()
        {
            return process.WorkingSet64;
        }

        /// <summary>
        /// Write to RAM memory
        /// </summary>
        public override IntPtr Write(IntPtr address, byte[] data)
        {
            IntPtr bytesWritten;
            RAMCalls.WriteProcessMemory(processHandle, address, data, data.Length, out bytesWritten);
            return bytesWritten;
        }

        /// <summary>
        /// Returns a specific chunk at a specific address
        /// </summary>
        protected override MemoryChunk GetChunk(ChunkAddress address)
        {
            byte[] buffer = new byte[(long)address.Size];
            int bytesRead = 0;
            RAMCalls.ReadProcessMemory((int)processHandle, address.Address, buffer, address.Size, ref bytesRead);

            MemoryChunk chunk = new MemoryChunk();
            chunk.Data = buffer;
            chunk.StartAddress = address.Address;
            return chunk;
        }
    }
}
