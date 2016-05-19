using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SearchEngineTools
{
    public static class IntCompressor
    {
        public static byte[] ToCompressedInt(this int val)
        {
            int max = (int)Math.Ceiling(LastBit(val) / 7.0);
            if (max == 0)
                return new byte[] { 128 };
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

    public class CompressedSortedList
    {
        byte[] arr;

        public CompressedSortedList(IList<int> init)
        {
            FromList(init);
        }

        public CompressedSortedList()
        {

        }
        public void Add(int val)
        {
            var tmp = ToList();
            tmp.AddSorted(val);
            FromList(tmp);
        }

        public void FromList(IList<int> lst)
        {
            List<byte[]> tmp = new List<byte[]>();
            int bc = 0;
            for (int i = 0; i < lst.Count; ++i)
            {
                tmp.Add(lst[i].ToCompressedInt());
                bc += tmp[i].Length;
            }
            arr = new byte[bc];
            for (int byteNum = 0, intNum = 0, tByteNum = 0; byteNum < bc; ++byteNum)
            {
                arr[byteNum] = tmp[intNum][tByteNum];
                if (tByteNum + 1 == tmp[intNum].Length)
                {
                    intNum++;
                    tByteNum = 0;
                }
                else
                    tByteNum++;
            }
        }

        public int Count
        {
            get { return ToList().Count; }
        }

        public List<int> ToList()
        {
            if (arr == null || arr.Length == 0)
                return new List<int>();
            List<int> res = new List<int>();
            byte[] buffer = new byte[10];
            for (int i = 0, k = 0; i < arr.Length; ++i)
            {
                buffer[k] = arr[i];
                if (arr[i] < 128)
                    k++;
                else
                {
                    k = 0;
                    res.Add(buffer.FromCompressedInt());
                }
            }
            return res;
        }

        public int[] ToArray()
        {
            return ToList().ToArray();
        }
    }
}