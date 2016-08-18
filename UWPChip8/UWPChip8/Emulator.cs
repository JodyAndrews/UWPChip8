using Chip8.Core;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace UWPChip8
{
    class Emulator
    {
        #region Fields

        /// <summary>
        /// A reference to our core CPU
        /// </summary>
        private CPU _cpu;

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor for this instance of a CPU
        /// </summary>
        public Emulator()
        {
            _cpu = new CPU();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Easy reference to the CPU's display buffer
        /// </summary>
        public bool[] DisplayBuffer
        {
            get { return _cpu.DisplayBuffer; }
        }

        /// <summary>
        /// Easy reference to the CPU's Powered Up state
        /// </summary>
        public bool PoweredUp
        {
            get { return _cpu.PoweredUp; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads a file from the file system asynchronously and powers up our CPU.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task LoadRom(StorageFile file)
        {
            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            Cart cart = new Cart(fileBytes);
            _cpu.PowerUp(cart);
        }

        /// <summary>
        /// Handles initialization and re-initialization of the CPU's state
        /// </summary>
        public void Initialize()
        {
            _cpu.Initialize();
        }

        /// <summary>
        /// Sends key input states to the CPU. The states are evaluated internally and not processed immediately.
        /// </summary>
        /// <param name="pressedLetter"></param>
        /// <param name="state"></param>
        public void ProcessKey(char pressedLetter, byte state)
        {
            switch (pressedLetter)
            {
                case '1':
                    _cpu.SetKey(1, state);
                    break;
                case '2':
                    _cpu.SetKey(2, state);
                    break;
                case '3':
                    _cpu.SetKey(3, state);
                    break;
                case '4':
                    _cpu.SetKey(12, state);
                    break;
                case 'Q':
                    _cpu.SetKey(4, state);
                    break;
                case 'W':
                    _cpu.SetKey(5, state);
                    break;
                case 'E':
                    _cpu.SetKey(6, state);
                    break;
                case 'R':
                    _cpu.SetKey(13, state);
                    break;
                case 'A':
                    _cpu.SetKey(7, state);
                    break;
                case 'S':
                    _cpu.SetKey(8, state);
                    break;
                case 'D':
                    _cpu.SetKey(9, state);
                    break;
                case 'F':
                    _cpu.SetKey(14, state);
                    break;
                case 'Z':
                    _cpu.SetKey(10, state);
                    break;
                case 'X':
                    _cpu.SetKey(0, state);
                    break;
                case 'C':
                    _cpu.SetKey(11, state);
                    break;
                case 'V':
                    _cpu.SetKey(15, state);
                    break;
            }
        }

        /// <summary>
        /// Iterates a number of cycles on the CPU per call. Note that the drawflag is set/unset but is otherwise unused. TODO : Check whether we benefit from 
        /// using the flag in the context of Win2D without double buffering. Chip-8 sprites will flicker anyway, maybe we can solve this with some foo.
        /// </summary>
        public void ExecuteIteration()
        {
            for (int i = 0; i < 10; i++)
                _cpu.IterateCycle();

            if (_cpu.DrawFlag)
            {
                _cpu.DrawFlag = false;
            }

            _cpu.UpdateTimers();
        }

        #endregion
    }
}
