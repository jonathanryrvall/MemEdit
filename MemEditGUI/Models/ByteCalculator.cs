using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.Models
{
    public static class ByteCalculator
    {
        /// <summary>
        /// Returns a human readable format of how many bytes there are
        /// </summary>
      public static string GetHumanReadable(long bytes)
        {
            double showBytes = bytes;
            string showBytesSuffix = "B";
            if (showBytes > 1024)
            {
                showBytes /= 1024;
                showBytesSuffix = "KiB";
            }
            if (showBytes > 1024)
            {
                showBytes /= 1024;
                showBytesSuffix = "MiB";
            }
            if (showBytes > 1024)
            {
                showBytes /= 1024;
                showBytesSuffix = "GiB";
            }
            if (showBytes > 1024)
            {
                showBytes /= 1024;
                showBytesSuffix = "TiB";
            }
            if (showBytes > 1024)
            {
                showBytes /= 1024;
                showBytesSuffix = "PiB";
            }
            return showBytes.ToString("#.00") + " " + showBytesSuffix;
        }
    }
}
