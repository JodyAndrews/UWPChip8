using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Core
{
    public partial class CPU
    {
        public Timer DelayTimer { get; set; } = new Timer();
        public Timer SoundTimer { get; set; } = new Timer();
    }
}
