using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Threading;
using MemEditGUI.Models.MemoryManagement;

namespace MemEditGUI.Models
{
    /// <summary>
    /// What type of memory source
    /// </summary>
    public enum SourceType
    {
        File,
        Ram
    }

    /// <summary>
    /// Items in process list
    /// </summary>
    public class MemorySource : BindableBase
    {
        public Process Process;
        private int pid;
        private string name;
        private string path;
        private DateTime startTime;
        private ImageSource img;
        private SourceType sourceType;
        private string showSize;
        private Memory memory;

        /// <summary>
        /// Create a new process list item from process
        /// </summary>
        public MemorySource(Process process)
        {
            SourceType = SourceType.Ram;
            Process = process;
            pid = process.Id;
            name =  process.ProcessName;
            try
            {
                path = GetMainModuleFilepath(pid);
                startTime = process.StartTime;

                Icon icon = Icon.ExtractAssociatedIcon(path);
                img = ToImageSource(icon);
            }
            catch { }

            img?.Freeze();
           
        }

        /// <summary>
        /// Set memory of memory source, this is a bit of an ugly solution
        /// </summary>
        public void SetMemory(Memory memory)
        {
            this.memory = memory;
            UpdateSize();
        }
        
        /// <summary>
        /// Update memory size!
        /// </summary>
        public void UpdateSize()
        {
            ShowSize = ByteCalculator.GetHumanReadable(memory.MemorySize());
        }


        /// <summary>
        /// Create a memorysource from a file
        /// </summary>
        public MemorySource(string path)
        {
            SourceType = SourceType.File;
            this.path = path;
            name = System.IO.Path.GetFileName(path);

            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(path);
                img = ToImageSource(icon);


            }
            catch { }

            img?.Freeze();
        }



        /// <summary>
        /// Returns path for process
        /// </summary>
        private string GetMainModuleFilepath(int processId)
        {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using (var results = searcher.Get())
                {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Convert icon to image source
        /// </summary>
        private ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        public int PID
        {
            get { return pid; }
            set { SetProperty(ref pid, value); }
        }
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }
        public string Path
        {
            get { return path; }
            set { SetProperty(ref path, value); }
        }
        public DateTime StartTime
        {
            get { return startTime; }
            set { SetProperty(ref startTime, value); }
        }
        public ImageSource Img
        {
            get { return img; }
            set { SetProperty(ref img, value); }
        }
        public SourceType SourceType
        {
            get { return sourceType; }
            set { SetProperty(ref sourceType, value); }
        }
        public string ShowSize
        {
            get { return showSize; }
            set { SetProperty(ref showSize, value); }
        }



    }
}
