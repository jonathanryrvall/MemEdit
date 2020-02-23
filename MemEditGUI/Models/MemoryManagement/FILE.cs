using MemEditGUI.Models.Scan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.Models.MemoryManagement
{
    public class FILE : Memory
    {
        const int MAX_CHUNK_SIZE = 0x001FFFFF;
        private string path;

        /// <summary>
        /// Create a new file memory
        /// </summary>
        public FILE(string path)
        {
            this.path = path;

            MinAddress = (IntPtr)0;
            MaxAddress = (IntPtr)new FileInfo(path).Length;
        }



        /// <summary>
        /// Returns chunks of a file
        /// </summary>
        protected override IEnumerable<MemoryChunk> Chunks(ScanSettings settings)
        {
            // Set min and max address based on file size
            MaxAddress = (IntPtr)new FileInfo(path).Length;
            IntPtr min = MinAddress;
            IntPtr max = MaxAddress;

            // Clamp memory based on scan settings
            min = (long)min > (long)settings.MinAddress ? min : settings.MinAddress;
            max = (long)max < (long)settings.MaxAddress ? max : settings.MaxAddress;

           
            long searchValueSize = DataTypeExtensions.ToBinary(settings.ValueOne,settings.ScanDataType, settings.IsHex).Length;
            long chunkJumps = MAX_CHUNK_SIZE - searchValueSize;

            using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                // Skip beginning of file
                reader.BaseStream.Seek((long)min, SeekOrigin.Begin);

                // Read until end of file
                while (reader.BaseStream.Position < (long)max)
                {

                    // Calculate size of buffer
                    long bufferSize = (long)max - (long)reader.BaseStream.Position;
                    bufferSize = MAX_CHUNK_SIZE < bufferSize ? MAX_CHUNK_SIZE : bufferSize;

                    // Read buffer
                    IntPtr posBeforeRead = (IntPtr)reader.BaseStream.Position;
                    byte[] buffer = reader.ReadBytes((int)bufferSize);

                    // New chunk of memory
                    MemoryChunk chunk = new MemoryChunk();
                    chunk.Data = buffer;
                    chunk.StartAddress = posBeforeRead;
                    yield return chunk;

                    // Jump back a step so it does not cut a value in half
                    if (reader.BaseStream.Position != (long)max)
                    {
                        reader.BaseStream.Seek(reader.BaseStream.Position - searchValueSize, SeekOrigin.Begin);
                    }

                }

            }
        }

        /// <summary>
        /// Write to specific addresses in a file
        /// </summary>
        public override IntPtr Write(IntPtr address, byte[] data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(path)))
            {
                writer.BaseStream.Seek((long)address, SeekOrigin.Begin);
                writer.Write(data);
            }
            return (IntPtr)data.Length;
        }

        /// <summary>
        /// Returns a specific chunk at a specific address
        /// </summary>
        protected override MemoryChunk GetChunk(ChunkAddress address)
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                // Jump to chunk position
                reader.BaseStream.Seek((long)address.Address, SeekOrigin.Begin);

                // Read buffer
                IntPtr posBeforeRead = (IntPtr)reader.BaseStream.Position;
                byte[] buffer = reader.ReadBytes((int)address.Size);

                // New chunk of memory
                MemoryChunk chunk = new MemoryChunk();
                chunk.Data = buffer;
                chunk.StartAddress = address.Address;
                return chunk;
            }
        }


        /// <summary>
        /// Returns how large memory is
        /// </summary>
        public override long MemorySize()
        {
            return new FileInfo(path).Length;
        }
    }
}
