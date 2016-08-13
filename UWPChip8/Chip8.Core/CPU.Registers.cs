using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Core
{
    public partial class CPU
    {
        public Register16Bit I { get; set; } = new Register16Bit();
    }
}
