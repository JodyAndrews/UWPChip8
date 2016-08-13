using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Core
{
    public partial class CPU
    {
        #region Fields

        byte[] _memory;
        int[] _stack = new int[16];
        int _sp;
        int _pc;
        byte[] _v;
        OpCode _opcode = new OpCode();
        Random _rnd = new Random();
        bool[] _displayBuffer = new bool[Config.ScreenWidth * Config.ScreenHeight];
        int[] keys = new int[16];
        bool _halted = false;

        #endregion

        #region Constructors

        public CPU()
        {
            Initialize();
        }

        #endregion

        #region Properties

        public bool[] DisplayBuffer { get { return _displayBuffer; } }

        public bool DrawFlag { get; set; } = false;

        public void Initialize()
        {
            _v = new byte[16];
            _memory = new byte[4 * 1024];
            _pc = 0x200;
            DelayTimer.Set(0);

            for (int i = 0; i < keys.Length; i++)
                keys[i] = 0;
        }

        #endregion

        #region Methods

        public void PowerUp(Cart cart)
        {
            // Copy the carts bytes to the starting mem of 0x200
            Array.Copy(cart.Bytes, 0, _memory, 0x200, cart.Bytes.Length);

            // Reproduced with thanks from Alex Dicksons JS Chip-8 Emulator
            var hexChars = new byte[]{
              0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
			  0x20, 0x60, 0x20, 0x20, 0x70, // 1
			  0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
			  0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
			  0x90, 0x90, 0xF0, 0x10, 0x10, // 4
			  0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
			  0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
			  0xF0, 0x10, 0x20, 0x40, 0x40, // 7
			  0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
			  0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
			  0xF0, 0x90, 0xF0, 0x90, 0x90, // A
			  0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
			  0xF0, 0x80, 0x80, 0x80, 0xF0, // C
			  0xE0, 0x90, 0x90, 0x90, 0xE0, // D
			  0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
			  0xF0, 0x80, 0xF0, 0x80, 0x80 // F
			};

            Array.Copy(hexChars, 0, _memory, 0x0, hexChars.Length);
        }

        public void EndExecution()
        {
            Initialize();
        }

        public void SetKey(int index, int val)
        {
            keys[index] = val;
        }

        public void IterateCycle()
        {
            if (!_halted)
            {
                _opcode.Set((ushort)((_memory[_pc] << 8) | _memory[_pc + 1]));
                _pc += 2;
            }

            try
            {
                ExecuteOpCode();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("errored");
            }
        }

        public void UpdateTimers()
        {
            if (DelayTimer.Get() > 0)
                DelayTimer.Decrement();

            if (SoundTimer.Get() > 0)
            {
                //if (SoundTimer.Get() == 1)
                //	Console.WriteLine("Beep");

                SoundTimer.Decrement();
            }
        }

        #endregion
    }
}
