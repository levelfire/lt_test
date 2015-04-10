using System;
using System.Text;

namespace Server_Form.LogicMoudle
{
    public class DESCryptor
    {
        private static readonly ulong[] bytebit = { 128, 64, 32, 16, 8, 4, 2, 1 };

        private static readonly ulong[] bigbyte = { 0x800000, 0x400000, 0x200000, 0x100000, 
                                                   0x80000,  0x40000,  0x20000,  0x10000, 
                                                   0x8000,   0x4000,   0x2000,   0x1000, 
                                                   0x800,    0x400,    0x200,    0x100, 
                                                   0x80,     0x40,     0x20,     0x10, 
                                                   0x8, 0x4, 0x2, 0x1 };

        private static readonly ulong[] pc1 = { 56, 48, 40, 32, 24, 16, 8, 0, 
                                                   57, 49, 41, 33, 25, 17, 9, 1, 
                                                   58, 50, 42, 34, 26, 18, 10, 2,
				                                   59, 51, 43, 35, 62, 54, 46, 38, 
                                                   30, 22, 14, 6, 61, 53, 45, 37, 
                                                   29, 21, 13, 5, 60, 52, 44, 36, 
                                                   28, 20, 12, 4, 27, 19, 11, 3 };

        private static readonly ulong[] totrot = { 1, 2, 4, 6, 8, 10, 12, 14, 15, 17, 19, 21, 23, 25, 27, 28 };

        private static readonly ulong[] pc2 = { 13, 16, 10, 23, 0, 4, 2, 27, 
                                                    14, 5, 20, 9, 22, 18, 11, 3, 
                                                   25, 7, 15, 6, 26, 19, 12, 1, 
                                                   40, 51, 30, 36, 46, 54, 29, 39, 
                                                   50, 44, 32, 47, 43, 48, 38, 55, 
                                                   33, 52, 45, 41, 49, 35, 28, 31 };

        private static readonly ulong[] SP1 = { 0x01010400, 0x00000000, 0x00010000, 0x01010404, 0x01010004, 0x00010404, 0x00000004, 0x00010000, 
                                                   0x00000400, 0x01010400, 0x01010404, 0x00000400, 0x01000404, 0x01010004, 0x01000000, 0x00000004,
				                                   0x00000404, 0x01000400, 0x01000400, 0x00010400, 0x00010400, 0x01010000, 0x01010000, 0x01000404, 
                                                   0x00010004, 0x01000004, 0x01000004, 0x00010004, 0x00000000, 0x00000404, 0x00010404, 0x01000000, 
                                                   0x00010000, 0x01010404, 0x00000004, 0x01010000, 0x01010400, 0x01000000, 0x01000000, 0x00000400, 
                                                   0x01010004, 0x00010000, 0x00010400, 0x01000004, 0x00000400, 0x00000004, 0x01000404, 0x00010404, 
                                                   0x01010404, 0x00010004, 0x01010000, 0x01000404, 0x01000004, 0x00000404, 0x00010404, 0x01010400, 
                                                   0x00000404, 0x01000400, 0x01000400, 0x00000000, 0x00010004, 0x00010400, 0x00000000, 0x01010004 };


        private static readonly ulong[] SP2 = { 0x80108020, 0x80008000, 0x00008000, 0x00108020, 0x00100000, 0x00000020, 0x80100020, 0x80008020, 
                                                   0x80000020, 0x80108020, 0x80108000, 0x80000000, 0x80008000, 0x00100000, 0x00000020, 0x80100020,
				                                   0x00108000, 0x00100020, 0x80008020, 0x00000000, 0x80000000, 0x00008000, 0x00108020, 0x80100000, 
                                                   0x00100020, 0x80000020, 0x00000000, 0x00108000, 0x00008020, 0x80108000, 0x80100000, 0x00008020, 
                                                   0x00000000, 0x00108020, 0x80100020, 0x00100000, 0x80008020, 0x80100000, 0x80108000, 0x00008000, 
                                                   0x80100000, 0x80008000, 0x00000020, 0x80108020, 0x00108020, 0x00000020, 0x00008000, 0x80000000, 
                                                   0x00008020, 0x80108000, 0x00100000, 0x80000020, 0x00100020, 0x80008020, 0x80000020, 0x00100020, 
                                                   0x00108000, 0x00000000, 0x80008000, 0x00008020, 0x80000000, 0x80100020, 0x80108020, 0x00108000 };

        private static readonly ulong[] SP3 = { 0x00000208, 0x08020200, 0x00000000, 0x08020008, 0x08000200, 0x00000000, 0x00020208, 0x08000200, 
                                                   0x00020008, 0x08000008, 0x08000008, 0x00020000, 0x08020208, 0x00020008, 0x08020000, 0x00000208,
				                                   0x08000000, 0x00000008, 0x08020200, 0x00000200, 0x00020200, 0x08020000, 0x08020008, 0x00020208, 
                                                   0x08000208, 0x00020200, 0x00020000, 0x08000208, 0x00000008, 0x08020208, 0x00000200, 0x08000000, 
                                                   0x08020200, 0x08000000, 0x00020008, 0x00000208, 0x00020000, 0x08020200, 0x08000200, 0x00000000, 
                                                   0x00000200, 0x00020008, 0x08020208, 0x08000200, 0x08000008, 0x00000200, 0x00000000, 0x08020008, 
                                                   0x08000208, 0x00020000, 0x08000000, 0x08020208, 0x00000008, 0x00020208, 0x00020200, 0x08000008, 
                                                   0x08020000, 0x08000208, 0x00000208, 0x08020000, 0x00020208, 0x00000008, 0x08020008, 0x00020200 };

        private static readonly ulong[] SP4 = { 0x00802001, 0x00002081, 0x00002081, 0x00000080, 0x00802080, 0x00800081, 0x00800001, 0x00002001, 
                                                   0x00000000, 0x00802000, 0x00802000, 0x00802081, 0x00000081, 0x00000000, 0x00800080, 0x00800001,
				                                   0x00000001, 0x00002000, 0x00800000, 0x00802001, 0x00000080, 0x00800000, 0x00002001, 0x00002080, 
                                                   0x00800081, 0x00000001, 0x00002080, 0x00800080, 0x00002000, 0x00802080, 0x00802081, 0x00000081, 
                                                   0x00800080, 0x00800001, 0x00802000, 0x00802081, 0x00000081, 0x00000000, 0x00000000, 0x00802000, 
                                                   0x00002080, 0x00800080, 0x00800081, 0x00000001, 0x00802001, 0x00002081, 0x00002081, 0x00000080, 
                                                   0x00802081, 0x00000081, 0x00000001, 0x00002000, 0x00800001, 0x00002001, 0x00802080, 0x00800081, 
                                                   0x00002001, 0x00002080, 0x00800000, 0x00802001, 0x00000080, 0x00800000, 0x00002000, 0x00802080 };

        private static readonly ulong[] SP5 = { 0x00000100, 0x02080100, 0x02080000, 0x42000100, 0x00080000, 0x00000100, 0x40000000, 0x02080000, 
                                                   0x40080100, 0x00080000, 0x02000100, 0x40080100, 0x42000100, 0x42080000, 0x00080100, 0x40000000,
				                                   0x02000000, 0x40080000, 0x40080000, 0x00000000, 0x40000100, 0x42080100, 0x42080100, 0x02000100, 
                                                   0x42080000, 0x40000100, 0x00000000, 0x42000000, 0x02080100, 0x02000000, 0x42000000, 0x00080100, 
                                                   0x00080000, 0x42000100, 0x00000100, 0x02000000, 0x40000000, 0x02080000, 0x42000100, 0x40080100, 
                                                   0x02000100, 0x40000000, 0x42080000, 0x02080100, 0x40080100, 0x00000100, 0x02000000, 0x42080000, 
                                                   0x42080100, 0x00080100, 0x42000000, 0x42080100, 0x02080000, 0x00000000, 0x40080000, 0x42000000, 
                                                   0x00080100, 0x02000100, 0x40000100, 0x00080000, 0x00000000, 0x40080000, 0x02080100, 0x40000100 };

        private static readonly ulong[] SP6 = { 0x20000010, 0x20400000, 0x00004000, 0x20404010, 0x20400000, 0x00000010, 0x20404010, 0x00400000, 
                                                   0x20004000, 0x00404010, 0x00400000, 0x20000010, 0x00400010, 0x20004000, 0x20000000, 0x00004010,
				                                   0x00000000, 0x00400010, 0x20004010, 0x00004000, 0x00404000, 0x20004010, 0x00000010, 0x20400010, 
                                                   0x20400010, 0x00000000, 0x00404010, 0x20404000, 0x00004010, 0x00404000, 0x20404000, 0x20000000, 
                                                   0x20004000, 0x00000010, 0x20400010, 0x00404000, 0x20404010, 0x00400000, 0x00004010, 0x20000010, 
                                                   0x00400000, 0x20004000, 0x20000000, 0x00004010, 0x20000010, 0x20404010, 0x00404000, 0x20400000, 
                                                   0x00404010, 0x20404000, 0x00000000, 0x20400010, 0x00000010, 0x00004000, 0x20400000, 0x00404010, 
                                                   0x00004000, 0x00400010, 0x20004010, 0x00000000, 0x20404000, 0x20000000, 0x00400010, 0x20004010 };

        private static readonly ulong[] SP7 = { 0x00200000, 0x04200002, 0x04000802, 0x00000000, 0x00000800, 0x04000802, 0x00200802, 0x04200800, 
                                                   0x04200802, 0x00200000, 0x00000000, 0x04000002, 0x00000002, 0x04000000, 0x04200002, 0x00000802,
				                                   0x04000800, 0x00200802, 0x00200002, 0x04000800, 0x04000002, 0x04200000, 0x04200800, 0x00200002, 
                                                   0x04200000, 0x00000800, 0x00000802, 0x04200802, 0x00200800, 0x00000002, 0x04000000, 0x00200800, 
                                                   0x04000000, 0x00200800, 0x00200000, 0x04000802, 0x04000802, 0x04200002, 0x04200002, 0x00000002, 
                                                   0x00200002, 0x04000000, 0x04000800, 0x00200000, 0x04200800, 0x00000802, 0x00200802, 0x04200800, 
                                                   0x00000802, 0x04000002, 0x04200802, 0x04200000, 0x00200800, 0x00000000, 0x00000002, 0x04200802, 
                                                   0x00000000, 0x00200802, 0x04200000, 0x00000800, 0x04000002, 0x04000800, 0x00000800, 0x00200002 };

        private static readonly ulong[] SP8 = { 0x10001040, 0x00001000, 0x00040000, 0x10041040, 0x10000000, 0x10001040, 0x00000040, 0x10000000, 
                                                   0x00040040, 0x10040000, 0x10041040, 0x00041000, 0x10041000, 0x00041040, 0x00001000, 0x00000040,
				                                   0x10040000, 0x10000040, 0x10001000, 0x00001040, 0x00041000, 0x00040040, 0x10040040, 0x10041000, 
                                                   0x00001040, 0x00000000, 0x00000000, 0x10040040, 0x10000040, 0x10001000, 0x00041040, 0x00040000, 
                                                   0x00041040, 0x00040000, 0x10041000, 0x00001000, 0x00000040, 0x10040040, 0x00001000, 0x00041040, 
                                                   0x10001000, 0x00000040, 0x10000040, 0x10040000, 0x10040040, 0x10000000, 0x00040000, 0x10001040, 
                                                   0x00000000, 0x10041040, 0x00040040, 0x10000040, 0x10040000, 0x10001000, 0x10001040, 0x00000000, 
                                                   0x10041040, 0x00041000, 0x00041000, 0x00001040, 0x00001040, 0x00040040, 0x10000000, 0x10041000 };

        private string skey;
        protected byte[] key;
        protected ulong[] encKey;
        protected ulong[] decKey;

        /*构造函数*/
        public DESCryptor(string key)
        {
            this.skey = key;
            this.key = Encoding.UTF8.GetBytes(this.skey);

            this.encKey = generateWorkingKey(true, this.key, 0);
            this.decKey = generateWorkingKey(false, this.key, 0);
        }

        public string decrypt(string input)
        {
            byte[] block = Convert.FromBase64String(input);

            int block_len = block.Length / 8;
            int block_remain = block.Length % 8;
            if (block_remain > 0)
            {
                block_len++;
            }
            int byte_pos = 0;
            int block_turn = 0;
            int i = 0;
            ulong index = 0;
            string str_out = "";
            byte[] sub_block = new byte[8];
            byte[] out_block = new byte[8 * block_len];
            while (block_turn < block_len)
            {
                for (i = 0; i < 8; i++)
                {
                    if (byte_pos < block.Length)
                    {
                        sub_block[i] = block[byte_pos];
                    }
                    else
                    {
                        sub_block[i] = 32;
                    }
                    byte_pos++;
                }
                desFunc(decKey, sub_block, 0, ref sub_block, ref index);
                //str_out += Encoding.UTF8.GetString(sub_block);
                for (i = 0; i < 8; i++)
                {
                    out_block[block_turn * 8 + i] = sub_block[i];
                }
                block_turn++;
            }


            //desFunc(decKey, block, index, ref block, ref index);
            //string str_out = Encoding.UTF8.GetString(block);
            str_out = Encoding.UTF8.GetString(out_block);

            return str_out.Trim();
        }

        public string encrypt(string input)
        {
            byte[] block = Encoding.UTF8.GetBytes(input);

            //Console.WriteLine("encrypt input:");
            //Console.WriteLine("  '" + input + "'(" + input.Length + ")" );
            //Console.WriteLine("trans to bytes: (" + block.Length + ")");
            //Console.Write("   ");
            //int i = 0;
            //for (i = 0; i < block.Length; i++)
            //{
            //    Console.Write(" " + block[i]);
            //}
            //Console.WriteLine(" ");

            //return input;
            int block_len = block.Length / 8;
            int block_remain = block.Length % 8;
            if (block_remain > 0)
            {
                block_len++;
            }
            int byte_pos = 0;
            int block_turn = 0;
            int i = 0;
            ulong index = 0;
            string str_out = "";
            byte[] sub_block = new byte[8];
            byte[] out_block = new byte[8 * block_len];
            while (block_turn < block_len)
            {
                for (i = 0; i < 8; i++)
                {
                    if (byte_pos < block.Length)
                    {
                        sub_block[i] = block[byte_pos];
                    }
                    else
                    {
                        sub_block[i] = 32;
                    }
                    byte_pos++;
                }
                desFunc(encKey, sub_block, 0, ref sub_block, ref index);
                //str_out += Convert.ToBase64String(sub_block);
                for (i = 0; i < 8; i++)
                {
                    out_block[block_turn * 8 + i] = sub_block[i];
                }
                block_turn++;
            }

            str_out += Convert.ToBase64String(out_block);
            Console.WriteLine("system convert");
            Console.WriteLine("   " + str_out);

            string my_base64 = Base64Convert.ToBase64String(out_block);
            Console.WriteLine("my convert");
            Console.WriteLine("   " + my_base64);

            return str_out;
        }

        protected ulong[] generateWorkingKey(bool encrypting, byte[] key, ulong off)
        {
            ulong[] newKey = new ulong[32];
            bool[] pc1m = new bool[56];
            bool[] pcr = new bool[56];

            ulong l;
            ulong j;
            ulong i;

            for (j = 0; j < 56; j++)
            {
                l = pc1[j];

                pc1m[j] = ((key[off + (l >> 3)] & bytebit[l & 07]) != 0);
            }

            for (i = 0; i < 16; i++)
            {
                ulong m;
                ulong n;

                if (encrypting)
                {
                    m = i << 1;
                }
                else
                {
                    m = (15 - i) << 1;
                }

                n = m + 1;
                newKey[m] = newKey[n] = 0;

                for (j = 0; j < 28; j++)
                {
                    l = j + totrot[i];
                    if (l < 28)
                    {
                        pcr[j] = pc1m[l];
                    }
                    else
                    {
                        pcr[j] = pc1m[l - 28];
                    }
                }

                for (j = 28; j < 56; j++)
                {
                    l = j + totrot[i];
                    if (l < 56)
                    {
                        pcr[j] = pc1m[l];
                    }
                    else
                    {
                        pcr[j] = pc1m[l - 28];
                    }
                }

                for (j = 0; j < 24; j++)
                {
                    if (pcr[pc2[j]])
                    {
                        newKey[m] |= bigbyte[j];
                    }

                    if (pcr[pc2[j + 24]])
                    {
                        newKey[n] |= bigbyte[j];
                    }
                }
            }

            for (i = 0; i != 32; i += 2)
            {
                ulong i1;
                ulong i2;

                i1 = newKey[i];
                i2 = newKey[i + 1];

                newKey[i] = ((i1 & 0x00fc0000) << 6) | ((i1 & 0x00000fc0) << 10) | ((i2 & 0x00fc0000) >> 10)
                              | ((i2 & 0x00000fc0) >> 6);

                newKey[i + 1] = ((i1 & 0x0003f000) << 12) | ((i1 & 0x0000003f) << 16) | ((i2 & 0x0003f000) >> 4)
                              | (i2 & 0x0000003f);
            }

            return newKey;
        }

        /**
		 * the DES engine.
		 */
        protected void desFunc(ulong[] wKey, byte[] inp, ulong inOff, ref byte[] outp, ref ulong outOff)
        {
            ulong work;
            ulong right;
            ulong left;

            left = (ulong)(inp[inOff + 0] & 0xff) << 24;
            left |= (ulong)(inp[inOff + 1] & 0xff) << 16;
            left |= (ulong)(inp[inOff + 2] & 0xff) << 8;
            left |= (ulong)(inp[inOff + 3] & 0xff);

            right = (ulong)(inp[inOff + 4] & 0xff) << 24;
            right |= (ulong)(inp[inOff + 5] & 0xff) << 16;
            right |= (ulong)(inp[inOff + 6] & 0xff) << 8;
            right |= (ulong)(inp[inOff + 7] & 0xff);

            work = ((left >> 4) ^ right) & 0x0f0f0f0f;
            right ^= work;
            left ^= (work << 4);
            work = ((left >> 16) ^ right) & 0x0000ffff;
            right ^= work;
            left ^= (work << 16);
            work = ((right >> 2) ^ left) & 0x33333333;
            left ^= work;
            right ^= (work << 2);
            work = ((right >> 8) ^ left) & 0x00ff00ff;
            left ^= work;
            right ^= (work << 8);
            right = ((right << 1) | ((right >> 31) & 1)) & 0xffffffff;
            work = (left ^ right) & 0xaaaaaaaa;
            left ^= work;
            right ^= work;
            left = ((left << 1) | ((left >> 31) & 1)) & 0xffffffff;

            ulong round;
            for (round = 0; round < 8; round++)
            {
                ulong fval;

                work = (right << 28) | (right >> 4);
                work ^= wKey[round * 4 + 0];
                fval = SP7[work & 0x3f];
                fval |= SP5[(work >> 8) & 0x3f];
                fval |= SP3[(work >> 16) & 0x3f];
                fval |= SP1[(work >> 24) & 0x3f];
                work = right ^ wKey[round * 4 + 1];
                fval |= SP8[work & 0x3f];
                fval |= SP6[(work >> 8) & 0x3f];
                fval |= SP4[(work >> 16) & 0x3f];
                fval |= SP2[(work >> 24) & 0x3f];
                left ^= fval;
                work = (left << 28) | (left >> 4);
                work ^= wKey[round * 4 + 2];
                fval = SP7[work & 0x3f];
                fval |= SP5[(work >> 8) & 0x3f];
                fval |= SP3[(work >> 16) & 0x3f];
                fval |= SP1[(work >> 24) & 0x3f];
                work = left ^ wKey[round * 4 + 3];
                fval |= SP8[work & 0x3f];
                fval |= SP6[(work >> 8) & 0x3f];
                fval |= SP4[(work >> 16) & 0x3f];
                fval |= SP2[(work >> 24) & 0x3f];
                right ^= fval;
            }

            right = (right << 31) | (right >> 1);
            work = (left ^ right) & 0xaaaaaaaa;
            left ^= work;
            right ^= work;
            left = (left << 31) | (left >> 1);
            work = ((left >> 8) ^ right) & 0x00ff00ff;
            right ^= work;
            left ^= (work << 8);
            work = ((left >> 2) ^ right) & 0x33333333;
            right ^= work;
            left ^= (work << 2);
            work = ((right >> 16) ^ left) & 0x0000ffff;
            left ^= work;
            right ^= (work << 16);
            work = ((right >> 4) ^ left) & 0x0f0f0f0f;
            left ^= work;
            right ^= (work << 4);

            outp[outOff + 0] = (byte)(((right >> 24) & 0xff));
            outp[outOff + 1] = (byte)(((right >> 16) & 0xff));
            outp[outOff + 2] = (byte)(((right >> 8) & 0xff));
            outp[outOff + 3] = (byte)((right & 0xff));
            outp[outOff + 4] = (byte)(((left >> 24) & 0xff));
            outp[outOff + 5] = (byte)(((left >> 16) & 0xff));
            outp[outOff + 6] = (byte)(((left >> 8) & 0xff));
            outp[outOff + 7] = (byte)((left & 0xff));
        }
    }
    public class Base64Convert
    {
        private static readonly char[] BASE64_CHARS_ARRAY = {'A','B','C','D','E','F','G','H',
					                                     'I','J','K','L','M','N','O','P',   
					                                     'Q','R','S','T','U','V','W','X',   
					                                     'Y','Z','a','b','c','d','e','f',   
					                                     'g','h','i','j','k','l','m','n',   
					                                     'o','p','q','r','s','t','u','v',   
					                                     'w','x','y','z','0','1','2','3',   
					                                     '4','5','6','7','8','9','+','/'};

        private static readonly int[] BASE64_DECODE_CHARS_ARRAY = {64, 64, 64, 64, 64, 64, 64, 64, 
					                                          64, 64, 64, 64, 64, 64, 64, 64, 
					                                          64, 64, 64, 64, 64, 64, 64, 64,
					                                          64, 64, 64, 64, 64, 64, 64, 64,
				 	                                          64, 64, 64, 64, 64, 64, 64, 64,
					                                          64, 64, 64, 62, 64, 64, 64, 63,
					                                          52, 53, 54, 55, 56, 57, 58, 59,
					                                          60, 61, 64, 64, 64, 64, 64, 64,
					                                          64,  0,  1,  2,  3,  4,  5,  6,
					                                           7,  8,  9, 10, 11, 12, 13, 14,   
					                                          15, 16, 17, 18, 19, 20, 21, 22,   
					                                          23, 24, 25, 64, 64, 64, 64, 64,   
					                                          64, 26, 27, 28, 29, 30, 31, 32,   
					                                          33, 34, 35, 36, 37, 38, 39, 40,   
					                                          41, 42, 43, 44, 45, 46, 47, 48,   
					                                          49, 50, 51, 64, 64, 64, 64, 64};

        public static byte[] FromBase64String(string data)
        {
            int dataBuffer_1 = 0;
            int dataBuffer_2 = 0;
            int dataBuffer_3 = 0;

            int len = data.Length;
            int out_len = (len * 3) >> 2;
            byte[] tmp_arr = new byte[out_len];

            int pos = 0;

            int i = 0;
            for (i = 0; i < len; i += 4)
            {
                dataBuffer_1 = BASE64_DECODE_CHARS_ARRAY[Convert.ToChar(data.Substring(i + 1, 1))];
                dataBuffer_2 = BASE64_DECODE_CHARS_ARRAY[Convert.ToChar(data.Substring(i + 2, 1))];
                dataBuffer_3 = BASE64_DECODE_CHARS_ARRAY[Convert.ToChar(data.Substring(i + 3, 1))];

                tmp_arr[pos++] = (byte)((BASE64_DECODE_CHARS_ARRAY[Convert.ToChar(data.Substring(i, 1))] << 2) + ((dataBuffer_1 & 0x30) >> 4));
                tmp_arr[pos++] = (byte)(((dataBuffer_1 & 0x0f) << 4) + ((dataBuffer_2 & 0x3c) >> 2));
                tmp_arr[pos++] = (byte)(((dataBuffer_2 & 0x03) << 6) + dataBuffer_3);
            }

            if (dataBuffer_2 == 64)
            {
                out_len -= 2;
            }
            else if (dataBuffer_3 == 64)
            {
                out_len--;
            }
            // Return decoded data
            byte[] output = new byte[out_len];
            for (i = 0; i < out_len; i++)
            {
                output[i] = tmp_arr[i];
            }

            return output;
        }

        public static string ToBase64String(byte[] data)
        {
            string output = "";
            int data_bytesAvailable = data.Length;
            int cycle = data_bytesAvailable / 3;

            int pos = 0;
            int c = 0;
            while (cycle > 0)
            {
                cycle--;

                c = (data[pos++]) << 16 | (data[pos++] << 8) | (data[pos++]);
                output += Convert.ToString(BASE64_CHARS_ARRAY[c >> 18 & 0x3f])
                        + Convert.ToString(BASE64_CHARS_ARRAY[c >> 12 & 0x3f])
                        + Convert.ToString(BASE64_CHARS_ARRAY[c >> 6 & 0x3f])
                        + Convert.ToString(BASE64_CHARS_ARRAY[c & 0x3f]);
            }
            if (data_bytesAvailable % 3 == 1)
            {
                output += Convert.ToString(BASE64_CHARS_ARRAY[(data[pos] & 0xfc) >> 2])
                        + Convert.ToString(BASE64_CHARS_ARRAY[((data[pos] & 0x03) << 4)])
                        + "==";
            }
            else if (data_bytesAvailable % 3 == 2)
            {
                output += Convert.ToString(BASE64_CHARS_ARRAY[(data[pos] & 0xfc) >> 2])
                        + Convert.ToString(BASE64_CHARS_ARRAY[((data[pos++] & 0x03) << 4) | ((data[pos]) >> 4)])
                        + Convert.ToString(BASE64_CHARS_ARRAY[((data[pos] & 0x0f) << 2)])
                        + "=";
            }
            // Return encoded data
            return output;
        }
    }
}
