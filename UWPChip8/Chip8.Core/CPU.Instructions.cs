using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Core
{
    public partial class CPU
    {
        public void ExecuteOpCode()
        {
            switch (_opcode.InstructionType)
            {
                case 0x0000:
                    switch (_opcode.Value)
                    {
                        case 0x00E0: // CLS
                            CLS();
                            break;
                        case 0x00EE: // RET
                            RET();
                            break;
                    }
                    break;
                case 0x1000: //JP addr
                    JP(_opcode.NNN);
                    break;
                case 0x2000: // CALL addr
                    Call(_opcode.NNN);
                    break;
                case 0x3000: // SE Vx, byte
                    SE(_opcode.X, (byte)(_opcode.NN));
                    break;
                case 0x4000: // SNE Vx, byte
                    SNE(_opcode.X, _opcode.NN);
                    break;
                case 0x5000: // SE Vx, Vy
                    SE(_opcode.X, _v[_opcode.Y]);
                    break;
                case 0x6000: // LD Vx, byte
                    LD(_opcode.X, _opcode.NN);
                    break;
                case 0x7000: // ADD Vx, byte
                    ADD(_opcode.X, _opcode.NN);
                    break;
                case 0x8000:
                    switch (_opcode.N)
                    {
                        case 0x0000: // LD Vx, Vy
                            LD(_opcode.X, _v[_opcode.Y]);
                            break;
                        case 0x0001: // OR Vx, Vy
                            OR(_opcode.X, _v[_opcode.Y]);
                            break;
                        case 0x0002: // AND Vx, Vy
                            AND(_opcode.X, _v[_opcode.Y]);
                            break;
                        case 0x0003: // XOR Vx, Vy
                            XOR(_opcode.X, _v[_opcode.Y]);
                            break;
                        case 0x0004: // ADD Vx, Vy
                            ADD(_opcode.X, _v[_opcode.Y]);
                            break;
                        case 0x0005: // SUB Vx, Vy
                            SUB(_opcode.X, _v[_opcode.Y]);
                            break;
                        case 0x0006: // SHR Vx, 1
                            SHR(_opcode.X);
                            break;
                        case 0x0007: // SUBN Vx, Vy
                            SUBN(_opcode.X, _v[_opcode.Y]);
                            break;
                        case 0x000E: // SHL Vx, 1
                            SHL(_opcode.X);
                            break;
                    }
                    break;
                case 0x9000: // SNE Vx, Vy
                    SNE(_opcode.X, _v[_opcode.Y]);
                    break;
                case 0xA000: // LD I, addr
                    LD_I(_opcode.NNN);
                    break;
                case 0xB000: // JP V0, addr
                    JP((ushort)(_opcode.NNN + _v[0]));
                    break;
                case 0xC000: // RND Vx, byte
                    RND(_opcode.X, _opcode.NN);
                    break;
                case 0xD000: // DRW Vx, Vy, nibble
                    DRW(_opcode.X, _opcode.Y, _opcode.N);
                    break;
                case 0xE000: // Input handling
                    switch (_opcode.NN)
                    {
                        case 0x009E: // SKP Vx
                            SKP(_v[_opcode.X]);
                            break;
                        case 0x00A1: // SKNP Vx
                            SKNP(_v[_opcode.X]);
                            break;
                    }
                    break;
                case 0xF000:
                    switch (_opcode.NN)
                    {
                        case 0x0007: // LD Vx, DT
                            LD(_opcode.X, (byte)DelayTimer.Get());
                            break;
                        case 0x000A: // LD Vx, K. TODO : Rough handle for checking key input
                            _halted = true;

                            for (var i = 0; i < 16; ++i)
                            {
                                if (keys[i] != 0)
                                {
                                    _v[_opcode.X] = (byte)i;

                                    _halted = false;
                                    continue;
                                }
                            }
                            break;
                        case 0x0015: // LD DT, Vx
                            DelayTimer.Set(_v[_opcode.X]);
                            break;
                        case 0x0018: // LD ST, Vx,
                            SoundTimer.Set(_v[_opcode.X]);
                            break;
                        case 0x001E: // ADD I, Vx
                            I.Set(I.GetValue() + _v[_opcode.X]);
                            break;
                        case 0x0029: // LD F, Vx
                            I.Set(_v[_opcode.X] * 5);
                            break;
                        case 0x0033: // LD B, Vx
                            BCD(_opcode.X);
                            break;
                        case 0x0055: // LD [I], Vx
                            for (byte i = 0; i <= _opcode.X; i++)
                            {
                                _memory[I.GetValue() + i] = _v[i];
                            }
                            break;
                        case 0x0065: // LD Vx, [I]
                            for (byte i = 0; i <= _opcode.X; i++)
                            {
                                _v[i] = _memory[I.GetValue() + i];
                            }
                            break;
                    }
                break;
            }
        }

        /// <summary>
        /// Clears the display buffer
        /// </summary>
        void CLS()
        {
            for (int i = 0; i < _displayBuffer.Length; i++)
            {
                _displayBuffer[i] = false;
            }
            DrawFlag = true;
        }

        /// <summary>
        /// Return from a subroutine
        /// </summary>
        void RET()
        {
            _pc = _stack[--_sp];
        }

        /// <summary>
        /// ADD Vx, kk
        /// 
        /// Adds Vx to kk and stores result at Vx
        /// 
        /// Sets carry register if necessary
        /// 
        /// Used By : 0x8xy4
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void ADD(byte ax, byte kk)
        {
            // TODO : Check this. This currently works because the operator + on two 
            // bytes is implicitly an integer and therefore can exceed 255. Dirty.
            _v[0xF] = (byte)((_v[ax] + kk > 255) ? 1 : 0);
            _v[ax] += kk;
        }

        /// <summary>
        /// JP kkk
        /// 
        /// Jumps to address kkk
        /// </summary>
        /// <param name="kkk">kkk</param>
        void JP(ushort kkk)
        {
            _pc = kkk;
        }

        /// <summary>
        /// Calls subroutine at the specified address and stores previous address on stack, advancing stack pointer.
        /// </summary>
        /// <param name="addr">addr</param>
        void Call(ushort addr)
        {
            _stack[_sp] = _pc;
            _pc = addr;
            _sp++;
        }

        /// <summary>
        /// SE Vx, kk : 0x5XY0, 0x3XKK
        /// 
        /// Skip next instruction if Vx is equal to kk
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void SE(byte ax, byte kk)
        {
            if (_v[ax] == kk)
            {
                _pc += 2;
            }
        }

        /// <summary>
        /// SUB Vx, kk
        /// 
        /// Subtracts kk from Vx and stores value at Vx
        /// 
        /// Sets Vf if borrow not performed
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void SUB(byte ax, byte kk)
        {
            _v[0x0f] = (byte)((_v[ax] >= kk) ? 1 : 0);
            _v[ax] -= kk;
        }

        /// <summary>
        /// SHR Vx : 0x8XY6
        /// 
        /// Logical Shift Right (1)
        /// 
        /// Sets Vf to LSB
        /// </summary>
        /// <param name="vx">ax</param>
        void SHR(byte ax)
        {
            _v[0xf] = (byte)(_v[ax] & 0x1);
            _v[ax] /= 2;
        }

        /// <summary>
        /// SUBN Vx, kk
        /// 
        /// Subtracts Vx from kk and stores value in Vx
        /// 
        /// Sets Vf if borrow not performed.
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void SUBN(byte ax, byte kk)
        {
            _v[0xf] = (byte)((_v[ax] <= kk) ? 1 : 0);
            _v[ax] = (byte)(kk - _v[ax]);
        }

        /// <summary>
        /// SHL Vx
        /// 
        /// Logical Shift Left (1)
        /// 
        /// Sets Vf to MSB
        /// </summary>
        /// <param name="ax">ax</param>
        void SHL(byte ax)
        {
            _v[0xf] = (byte)(_v[ax] >> 7);
            _v[ax] *= 2;
        }

        /// <summary>
        /// SNE Vx, kk
        /// 
        /// Skips next instruction if Vx does not equal kk
        /// </summary>
        /// <param name="ax">ax.</param>
        /// <param name="kk">kk.</param>
        void SNE(byte ax, byte kk)
        {
            if (_v[ax] != kk)
                _pc += 2;
        }

        /// <summary>
        /// OR Vx, kk
        /// 
        /// Bitwise OR on Vx and kk and store result in Vx
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void OR(byte ax, byte kk)
        {
            _v[ax] = (byte)(_v[ax] | kk);
        }

        /// <summary>
        /// AND Vx, kk
        /// 
        /// Bitwise AND on Vx and kk and store result in Vx
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void AND(byte ax, byte kk)
        {
            _v[ax] = (byte)(_v[ax] & kk);
        }

        /// <summary>
        /// XOR Vx, kk
        /// 
        /// Bitwise exclusive OR on Vx and kk and store result in Vx
        /// </summary>
        /// <param name="ax">ax.</param>
        /// <param name="kk">kk</param>
        void XOR(byte ax, byte kk)
        {
            _v[ax] = (byte)(0xFF & (_v[ax] ^ kk));
        }

        /// <summary>
        /// LD I, nnnn : ANNN
        /// 
        /// Load value into I register.
        /// </summary>
        /// <param name="value">Value.</param>
        void LD_I(int value)
        {
            I.Set(value);
        }

        /// <summary>
        /// RND Vx, kk : CXKK
        /// 
        /// Generates a random byte bitwise AND with kk and result stored in Vx
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void RND(byte ax, byte kk)
        {
            byte rndVal = (byte)_rnd.Next(0, 255 + 1);
            _v[ax] = (byte)(rndVal & kk);
        }

        /// <summary>
        /// Loads value kk into address ax
        /// </summary>
        /// <param name="ax"></param>
        /// <param name="kk"></param>
        void LD(byte ax, byte kk)
        {
            _v[ax] = kk;
        }

        /// <summary>
        /// 'Draw's a pixel to the display buffer
        /// 
        /// This is more or less a direct implementation of Alexander Dickson's JS
        /// Chip8 Emulator
        /// </summary>
        /// <param name="ax"></param>
        /// <param name="ay"></param>
        /// <param name="nibble"></param>
        void DRW(byte ax, byte ay, byte nibble)
        {
            _v[0xf] = 0;
            byte sprite = 0x0;
            int start = I.GetValue();

            for (int y = 0; y < nibble; y++)
            {
                sprite = _memory[start + y];
                for (int x = 0; x < 8; x++)
                {
                    if ((sprite & 0x80) > 0)
                    {
                        if (SetPixel((byte)((byte)_v[ax] + (byte)x), (byte)((byte)_v[ay] + y)))
                        {
                            _v[0xf] = 1;
                        }
                    }
                    sprite <<= 1;
                }
            }

            DrawFlag = true;
        }

        /// <summary>
        /// Sets a pixel from the DRW call
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool SetPixel(byte x, byte y)
        {
            var width = 64;
            var height = 32;
            var location = 0;

            if (x > width)
            {
                x -= (byte)width;
            }
            else if (x < 0)
            {
                x += (byte)width;
            }

            if (y > height)
            {
                y -= (byte)height;
            }
            else if (y < 0)
            {
                y += (byte)height;
            }
            location = x + (y * width);

            _displayBuffer[location] ^= true;

            return !Convert.ToBoolean(_displayBuffer[location]);
        }

        /// <summary>
        /// Skips next instruction is key IS pressed
        /// </summary>
        /// <param name="ax"></param>
        void SKP(byte ax)
        {
            if (keys[ax] == 1)
                _pc += 2;
        }

        /// <summary>
        /// Skips next instruction if key is NOT pressed
        /// </summary>
        /// <param name="ax"></param>
        void SKNP(byte ax)
        {
            if (this.keys[ax] != 1)
                this._pc += 2;
        }

        /// <summary>
        /// Stores BCD representation of Vx in memory locations I, I+1, I+2
        /// 
        /// This is a direct implementation of Alexander Dicksons BCD function
        /// </summary>
        /// <param name="ax"></param>
        void BCD(byte ax)
        {
            var number = _v[ax];

            for (int i = 3; i > 0; i--)
            {
                _memory[I.GetValue() + i - 1] = (byte)(number % 10);
                number /= 10;
            }
        }

        void LD_I_VX(byte ax)
        {
            int start = I.GetValue();

            for (int i = 0; i < ax; i++)
            {
                _memory[start + i] = _v[ax + i];
            }
        }
    }
}
