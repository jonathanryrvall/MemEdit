using MemEditGUI.Models.MemoryManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.Models
{

    public sealed class GlobalStates
    {
        private static readonly GlobalStates instance = new GlobalStates();

        public Memory Memory;
        public MemorySource MemorySource;

        #region Singleton
        static GlobalStates()
        {
        }

        private GlobalStates()
        {
        }

        public static GlobalStates Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        /// <summary>
        /// Select a memory source
        /// </summary>
        public void SelectMemorySource(MemorySource memorySource)
        {
            MemorySource = memorySource;
            switch (memorySource.SourceType)
            {
                case SourceType.File:
                    Memory = new FILE(memorySource.Path);
                    break;
                case SourceType.Ram:
                    Memory = new RAM(memorySource.Process);
                    break;

            }
            MemorySource.SetMemory(Memory);
        }

        /// <summary>
        /// Select a file
        /// </summary>
        public void SelectFile(string path)
        {
            Memory = new FILE(path);
        }



    }


}
