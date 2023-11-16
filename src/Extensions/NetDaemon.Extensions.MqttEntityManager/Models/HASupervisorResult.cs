using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDaemon.Models
{
    internal sealed class HASupervisorResult<DataType>
    {

        public string Result { get; set; } = default!;

        public DataType Data { get; set; } = default!;
    }
}
