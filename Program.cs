using System;
using System.IO;
using System.Runtime.InteropServices;
using InfinityBit.IO;

namespace InfinityBit
{
    #region TestStruct

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TestStruct
    {
        public int x;
        public int y;
        public byte type;

        public TestStruct(int x, int y, byte type)
        {
            this.x = x; this.y = y; this.type = type;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, type);
        }
    }

    #endregion

    internal static class Program
    {
        static void PrintArray<T>(T[] array)
        {
            foreach (T item in array)
            {
                Console.Write(item + " ");
            }
            Console.WriteLine("({0} items)", array.Length);
        }

        static void FloatToBytesSample()
        {
            float[] floats = new float[] { 0, 1.5f, 3, -1 };
            ArrayReinterpreter<float> reinterpreter = new ArrayReinterpreter<float>();

            Console.Write("Floats: ");
            PrintArray(floats);

            Console.Write("Bytes: ");
            reinterpreter.AsByteArray(floats, bytes => PrintArray(bytes));
            Console.WriteLine();
        }

        static void StructToBytesSample()
        {
            TestStruct[] structs = new TestStruct[]
            {
                new TestStruct(1, 2, 3),
                new TestStruct(-1, 0, 17),
            };
            ArrayReinterpreter<TestStruct> reinterpreter = new ArrayReinterpreter<TestStruct>();

            Console.Write("Structs: ");
            PrintArray(structs);

            Console.Write("Bytes: ");
            reinterpreter.AsByteArray(structs, bytes => PrintArray(bytes));
            Console.WriteLine();
        }

        static void ManipulationSample()
        {
            TestStruct tst = new TestStruct(7, 8, 9);
            ArrayReinterpreter<TestStruct> reinterpreter = new ArrayReinterpreter<TestStruct>();

            Console.WriteLine("Before: {0}", tst);

            reinterpreter.AsByteArray(ref tst, bytes => bytes[8] = 15);
            Console.WriteLine("After: {0}\n", tst);
        }

        static void BytesToUIntSample1()
        {
            byte[] bytes = { 1, 2, 3, 4, 5, 6, 7, 8 };
            ArrayReinterpreter<uint> reinterpreter = new ArrayReinterpreter<uint>();

            Console.Write("Bytes: ");
            PrintArray(bytes);

            Console.Write("Uints: ");
            reinterpreter.AsStructArray(bytes, uints =>
            {                
                foreach (uint u in uints)
                {
                    Console.Write("0x{0:X} ", u);
                }
                Console.WriteLine("({0} items)", uints.Length);
            });
            Console.WriteLine();
        }

        static void MakeFromBytesSample()
        {
            byte[] bytes = new byte[] { 0x11, 0x22, 0x33, 0x44 };
            ArrayReinterpreter<uint> reinterpreter = new ArrayReinterpreter<uint>();

            Console.Write("From bytes: ");
            PrintArray(bytes);

            uint result = reinterpreter.AsStructArray(bytes);
            Console.WriteLine("Was made uint: 0x{0:X}\n", result);
        }

        static void FastStreamWriteSample(string fileName)
        {
            const int count = 5;
            ushort[] shorts = new ushort[] { 0x0011, 0x2233, 0x4455 };
            double[] doubles = new double[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                FastStructStream fastStream = new FastStructStream(fs);

                fastStream.Write(shorts.Length);
                fastStream.Write(shorts);

                fastStream.Write(count);
                fastStream.Write(doubles, 3, count);
            }
        }

        static void FastStreamReadSample(string fileName)
        {
            ushort[] shorts;
            double[] doubles;

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                FastStructStream fastStream = new FastStructStream(fs);

                int shortsLength = fastStream.Read<int>();
                shorts = new ushort[shortsLength];
                fastStream.Read(shorts);

                int doublesLength = fastStream.Read<int>();
                doubles = new double[doublesLength];
                fastStream.Read(doubles);
            }

            Console.Write("Shorts read: ");
            PrintArray(shorts);

            Console.Write("Doubles read: ");
            PrintArray(doubles);
        }

        static void Main()
        {
            const string fileName = "test.dat";

            FloatToBytesSample();
            StructToBytesSample();
            ManipulationSample();
            BytesToUIntSample1();
            MakeFromBytesSample();

            FastStreamWriteSample(fileName);
            FastStreamReadSample(fileName);

            Console.WriteLine("--- END ---");
        }
    }
}
