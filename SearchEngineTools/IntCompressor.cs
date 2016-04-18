using System;
using System.Collections;
using System.IO;

namespace SearchEngineTools
{
    public static class IntCompressor
    {
        public static byte[] ToCompressedInt(this int val)
        {
            int max = (int)Math.Ceiling(LastBit(val) / 7.0);
            byte[] result = new byte[max];
            BitArray ba = new BitArray(new[] { val });
            for (int i = 0, k = -1; k < max; ++i)
            {
                if (i % 7 == 0)
                {
                    k++;
                    i = 0;
                }
                if (ba[7 * k + i])
                    result[k] += (byte)(1 << i);
            }
            result[max - 1] += 128;
            return result;
        }

        public static int FromCompressedInt(this byte[] input)
        {
            byte cur;
            int val = 0;
            int i = 0;
            do
            {
                cur = input[i];
                for (int k = 0; k < 7; ++k)
                    if (((1 << k) & cur) > 0)
                        val += 1 << (i * 7 + k);
                i++;
            } while (cur < 128);
            return val;
        }

        public static int ReadCompressedInt(this BinaryReader br)
        {
            byte cur;
            int val = 0;
            int i = 0;
            do
            {
                cur = br.ReadByte();
                for (int k = 0; k < 7; ++k)
                    if (((1 << k) & cur) > 0)
                        val += 1 << (i * 7 + k);
                i++;
            } while (cur < 128);
            return val;
        }

        public static void WriteCompressedInt(this BinaryWriter sw, int val)
        {
            sw.Write(ToCompressedInt(val));
        }

        static int LastBit(int val)
        {
            for (int i = 0; i < 32; ++i)
            {
                if (val >> i == 0)
                    return i;
            }
            return 32;
        }

        

    }
}