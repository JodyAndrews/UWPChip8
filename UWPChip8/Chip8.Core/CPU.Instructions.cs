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
            switch (_opcode.InstructionType) // Identify upper 4 bits
            {
                case 0x0000:
                    switch (_opcode.Value)
                    {
                        case 0x00E0:

                            for (int i = 0; i < _displayBuffer.Length; i++)
                            {
                                _displayBuffer[i] = false;
                            }
                            DrawFlag = true;
                            break;
                        case 0x00EE:
                            _pc = _stack[--_sp];
                            break;
                    }
                    break;
                case 0x1000:
                    JP(_opcode.NNN);
                    break;
                case 0x2000:
                    Call(_opcode.Value);
                    break;
                case 0x3000:
                    SE(_opcode.X, (byte)(_opcode.NN));
                    break;
                case 0x4000:
                    SNE(_opcode.X, _opcode.NN);
                    break;
                case 0x5000:
                    SE(_opcode.X, _v[_opcode.Y]);
                    break;
                case 0x6000:
                    _v[_opcode.X] = _opcode.NN;
                    break;
                case 0x7000:
                    _v[_opcode.X] = (byte)(_v[_opcode.X] + _opcode.NN);
                    break;
                case 0x8000:
                    switch (_opcode.N)
                    {
                        case 0x0000: // 0x8xy0
                                     // Load value at vy into vx;
                            LD(_opcode.X, _v[_opcode.Y]);
                            break;
                        case 0x0001: // 0x8xy1
                            OR(_opcode.X, _opcode.Y);
                            break;
                        case 0x0002:
                            AND(_opcode.X, _opcode.Y);
                            break;
                        case 0x0003:
                            XOR(_opcode.X, _opcode.Y);
                            break;
                        case 0x0004:
                            ADD(_opcode.X, _opcode.Y);
                            break;
                        case 0x0005:
                            SUB(_opcode.X, _opcode.Y);
                            break;
                        case 0x0006:
                            SHR(_opcode.X);
                            break;
                        case 0x0007:
                            SUBN(_opcode.X, _opcode.Y);
                            break;
                        case 0x000E:
                            SHL(_opcode.X);
                            break;
                    }
                    break;
                case 0x9000:
                    SNE(_opcode.X, _v[_opcode.Y]);
                    break;
                case 0xA000:
                    LD_I(_opcode.NNN);
                    break;
                case 0xB000:
                    Jump_V0((byte)(_opcode.NNN));
                    break;
                case 0xC000:
                    RND(_opcode.X, _opcode.NN);
                    break;
                case 0xD000:
                    DRW(_opcode.X, _opcode.Y, _opcode.N);
                    break;
                case 0xE000:
                    switch (_opcode.NN)
                    {
                        case 0x009E:
                            if (keys[_v[_opcode.X]] == 1)
                                _pc += 2;

                            break;

                        case 0x00A1:
                            if (this.keys[_v[_opcode.X]] != 1)
                            {
                                this._pc += 2;
                            }
                            break;
                    }
                    break;
                case 0xF000:
                    switch (_opcode.NN)
                    {
                        case 0x0007: // Set Vx = delay timer
                            LD(_opcode.X, (byte)DelayTimer.Get());
                            break;
                        case 0x000A:
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
                        case 0x0015:
                            DelayTimer.Set(_v[_opcode.X]);
                            break;
                        case 0x0018:
                            SoundTimer.Set(_v[_opcode.X]);
                            break;
                        case 0x001E:
                            I.Set(I.GetValue() + _v[_opcode.X]);
                            break;
                        case 0x0029:
                            I.Set(_v[_opcode.X] * 5);
                            break;
                        case 0x0033:
                            LD_B(_opcode.X);
                            break;
                        case 0x0055:
                            //LD_I_VX((byte)((opCode & 0x0F00) >> 8));

                            for (byte i = 0; i <= _opcode.X; i++)
                            {
                                _memory[I.GetValue() + i] = _v[i];
                            }
                            break;
                        case 0x0065:
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
        /// JP kkk
        /// 
        /// Jumps to address kkk
        /// </summary>
        /// <param name="kkk">kkk.</param>
        void JP(ushort kkk)
        {
            _pc = kkk;
        }

        /// <summary>
        /// Calls subroutine at the specified address  (opcode & 0x0FFF).
        /// </summary>
        /// <param name="opcode">Opcode.</param>
        void Call(int opcode)
        {
            _stack[_sp] = _pc;
            _sp++;
            _pc = _opcode.NNN;
        }

        /// <summary>
        /// SE Vx, kk : 0x5XY0, 0x3XKK
        /// 
        /// Skip next instruction if Vx is equal to kk
        /// </summary>
        /// <param name="ax">Ax.</param>
        /// <param name="ay">Ay.</param>
        void SE(byte ax, byte kk)
        {
            if (_v[ax] == kk)
            {
                _pc += 2;
            }
        }

        /// <summary>
        /// ADD Vx, Vy
        /// 
        /// Adds Vx to Vy and stores result at Vx
        /// 
        /// Sets carry register if necessary
        /// 
        /// Used By : 0x8xy4
        /// </summary>
        /// <param name="ax">ax.</param>
        /// <param name="ay">ay.</param>
        void ADD(byte ax, byte ay)
        {
            // TODO : Check this
            _v[0xF] = (byte)((_v[ax] + _v[ay] > 255) ? 1 : 0);
            _v[ax] += _v[ay];
        }

        /// <summary>
        /// SUB Vx, Vy
        /// 
        /// Subtracts Vy from Vx and stores value at Vx
        /// 
        /// Sets Vf if borrow not performed
        /// 
        /// Used By : 0x8xy5
        /// </summary>
        /// <param name="ax">ax.</param>
        /// <param name="ay">ay.</param>
        void SUB(byte ax, byte ay)
        {
            _v[0x0f] = (byte)((_v[ax] >= _v[ay]) ? 1 : 0);
            _v[ax] -= _v[ay];
        }

        /// <summary>
        /// SHR Vx : 0x8XY6
        /// 
        /// Logical Shift Right (1)
        /// 
        /// Sets Vf to LSB
        /// </summary>
        /// <param name="vx">Vx.</param>
        void SHR(byte ax)
        {
            _v[0xf] = (byte)(_v[ax] & 0x1);
            _v[ax] /= 2;
        }

        /// <summary>
        /// SUBN Vx, Vy : 0x8XY7
        /// 
        /// Subtracts Vx from Vy and stores value in Vx
        /// 
        /// Sets Vf if borrow not performed.
        /// </summary>
        /// <param name="ax">Ax.</param>
        /// <param name="ay">Ay.</param>
        void SUBN(byte ax, byte ay)
        {
            _v[0xf] = (byte)((_v[ax] <= _v[ay]) ? 1 : 0);
            _v[ax] = (byte)(_v[ay] - _v[ax]);
        }

        /// <summary>
        /// SHL Vx : 0x8XYE
        /// 
        /// Logical Shift Left (1)
        /// 
        /// Sets Vf to MSB
        /// </summary>
        /// <param name="ax">Ax.</param>
        void SHL(byte ax)
        {
            _v[0xf] = (byte)(_v[ax] >> 7);
            _v[ax] *= 2;
        }

        /// <summary>
        /// SNE Vx, kk : 0x9XY0
        /// 
        /// Skips next instruction if Vx does not equal kk
        /// </summary>
        /// <param name="ax">Ax.</param>
        /// <param name="ay">Ay.</param>
        void SNE(byte ax, byte kk)
        {
            if (_v[ax] != kk)
                _pc += 2;
        }

        /// <summary>
        /// OR Vx, Vy : 0x8XY1
        /// 
        /// Bitwise OR on Vx and Vy and store result in Vx
        /// </summary>
        /// <param name="ax">ax.</param>
        /// <param name="ay">ay.</param>
        void OR(byte ax, byte ay)
        {
            _v[ax] = (byte)(_v[ax] | _v[ay]);
        }

        /// <summary>
        /// AND Vx, Vy : 0x8xy2
        /// 
        /// Bitwise AND on Vx and By and store result in Vx
        /// </summary>
        /// <param name="ax">Vx.</param>
        /// <param name="ay">Vy.</param>
        void AND(byte ax, byte ay)
        {
            _v[ax] = (byte)(_v[ax] & _v[ay]);
        }

        /// <summary>
        /// XOR Vx, Vy
        /// 
        /// Bitwise exclusive OR on Vx and Vy and store result in Vx
        /// </summary>
        /// <param name="ax">Vx.</param>
        /// <param name="ay">Vy.</param>
        void XOR(byte ax, byte ay)
        {
            _v[ax] = (byte)(0xFF & (_v[ax] ^ _v[ay]));
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

        void Jump_V0(int addr)
        {
            _pc = addr + _v[0];
        }

        /// <summary>
        /// RND Vx, kk : CXKK
        /// 
        /// Generates a random byte bitwise AND with kk and result stored in Vx
        /// </summary>
        /// <param name="ax">Ax.</param>
        /// <param name="kk">Kk.</param>
        void RND(byte ax, byte kk)
        {
            byte rndVal = (byte)_rnd.Next(0, 255 + 1);
            _v[ax] = (byte)(rndVal & kk);
        }

        void LD(byte vx, byte kk)
        {
            _v[vx] = kk;
        }

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

        void SKP(byte ax)
        {
            throw new NotImplementedException();
        }

        void SKNP(byte ax)
        {
            //_pc++;
            throw new NotImplementedException();
        }

        void LD_B(byte ax)
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
