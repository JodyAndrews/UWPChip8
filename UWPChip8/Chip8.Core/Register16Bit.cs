using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Core
{
    public class Register16Bit
    {
        int _value;

        public void Set(int value)
        {
            _value = value;
        }

        public int GetValue()
        {
            return _value;
        }
    }
}