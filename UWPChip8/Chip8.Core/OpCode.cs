﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Core
{
    public class OpCode
    {
        int _value;

        public int Value { get { return _value; } }
        public int InstructionType
        {
            get
            {
                return (int)(_value & 0xF000);
            }
        }

        public byte X { get { return (byte)((_value & 0x0F00) >> 8); } }
        public byte Y { get { return (byte)((_value & 0x00F0) >> 4); } }
        public byte N
        {
            get
            {
                return (byte)((_value & 0x000F));
            }
        }
        public byte NN
        {
            get
            {
                return (byte)((_value & 0x00FF));
            }
        }
        public ushort NNN
        {
            get
            {
                return (ushort)((_value & 0x0FFF));
            }
        }

        public void Set(int value)
        {
            this._value = value;
        }

        public void Reset()
        {
            this._value = 0x0;
        }

        public override string ToString()
        {
            //string.Format("0x{0:X}", _opcode.Value)
            return string.Format("[OpCode: Value=0x{0:X}, InstructionType={1}, X={2}, Y={3}, N={4}, NN={5}, NNN={6}]", Value, InstructionType, X, Y, N, NN, NNN);
        }
    }
}
