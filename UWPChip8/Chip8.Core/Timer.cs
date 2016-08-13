using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Core
{
    public class Timer
    {
        int _value;

        public int Get()
        {
            return _value;
        }

        public void Set(int value)
        {
            _value = value;
        }

        public int Decrement()
        {
            _value--;
            return _value;
        }
    }
}