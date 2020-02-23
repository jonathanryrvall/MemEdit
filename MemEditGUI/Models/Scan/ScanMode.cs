using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemEditGUI.Models.Scan
{
    public enum ScanMode
    {
        [Description("Exact value")]
        ExactValue,
        [Description("Greater than")]
        GreaterThan,
        [Description("Less than")]
        LessThan,
        [Description("Between")]
        Between,
        [Description("Increased")]
        Increased,
        [Description("Increased by")]
        IncreasedBy,
        [Description("Increased by max")]
        IncreasedByMax,
        [Description("Increased by min")]
        IncreasedByMin,
        [Description("Decreased")]
        Decreased,
        [Description("Decreased by")]
        DecreasedBy,
        [Description("Decreased by min")]
        DecreasedByMin,
        [Description("Decreased by max")]
        DecreasedByMax,
        [Description("Changed")]
        Changed
    }
}
