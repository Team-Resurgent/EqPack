using System;
using System.Collections.Generic;

namespace EqPack
{
    public class Rc4
    {
        private int X { get; set; }

        private int Y { get; set; }

        private byte[] Data { get; set; }

        public Rc4()
        {
            Data = new byte[256];
        }

        public void SetKey(byte[] key, int keyLength)
        {
            X = 0;
            Y = 0;

            for (var i = 0; i < Data.Length; i++)
            {
                Data[i] = (byte)i;
            }

            var i1 = 0;
            var i2 = 0;
            for (var i = 0; i < Data.Length; i++)
            {
                i2 = (key[i1] + Data[i] + i2) % Data.Length;
                i1 = (i1 + 1) % keyLength;
                SwapByte(Data, i, i2);
            }
        }

        public void Crypt(byte[] data, int offset, int length)
        {
            if (offset + length > data.Length)
            {
                throw new IndexOutOfRangeException();
            }

            for (var i = 0; i < length; i++)
            {
                X = (X + 1) % Data.Length;
                Y = (Data[X] + Y) % Data.Length;
                SwapByte(Data, X, Y);
                data[offset + i] ^= Data[(Data[X] + Data[Y]) % Data.Length];
            }
        }

        private static void SwapByte(IList<byte> data, int firstIndex, int secondIndex)
        {
            (data[firstIndex], data[secondIndex]) = (data[secondIndex], data[firstIndex]);
        }
    }
}
