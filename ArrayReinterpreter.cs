/*
  Original idea: https://gist.github.com/OmerMor/1050703
*/
using System;
using System.Runtime.InteropServices;

namespace InfinityBit
{
    #region ArrayHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ArrayHeader
    {
        public UIntPtr Type;
        public UIntPtr Length;
    }

    #endregion

    #region ArrayUnion

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct ArrayUnion
    {
        [FieldOffset(0)] public byte[] Bytes;
        [FieldOffset(0)] public Array Structs;
    }

    #endregion

    public unsafe class ArrayReinterpreter<T> where T : struct
    {
        private readonly UIntPtr m_byteArrayTypePtr;
        private readonly UIntPtr m_structArrayTypePtr;
        private readonly byte[] m_byteStub = new byte[0];
        private readonly T[] m_structStub;
        private readonly T[] m_structBuffer;

        public int SizeOfItem { get; private set; }

        public ArrayReinterpreter()
        {
            m_structStub = new T[0];
            m_structBuffer = new T[1];
            SizeOfItem = Marshal.SizeOf(typeof(T));

            m_byteArrayTypePtr = GetArrayTypePtr(m_byteStub);
            m_structArrayTypePtr = GetArrayTypePtr(m_structStub);
        }

        #region AsByteArray

        public T GetFromByteArray(Action<byte[]> action)
        {
            AsByteArray(m_structBuffer, action);
            return m_structBuffer[0];
        }

        public void AsByteArray(T value, Action<byte[]> action)
        {
            m_structBuffer[0] = value;
            AsByteArray(m_structBuffer, action);
        }

        public void AsByteArray(ref T value, Action<byte[]> action)
        {
            m_structBuffer[0] = value;
            AsByteArray(m_structBuffer, action);

            value = m_structBuffer[0];
        }

        public void AsByteArray(T[] structArray, Action<byte[]> action)
        {
            if (structArray == null)
            {
                action(null);
                return;
            }

            if (structArray.Length == 0)
            {
                action(m_byteStub);
                return;
            }

            int length= structArray.Length;
            ArrayUnion union = new ArrayUnion { Structs = structArray };

            GCHandle gcHandle = GCHandle.Alloc(structArray, GCHandleType.Pinned);
            ArrayHeader* header = GetArrayHeader((void*)gcHandle.AddrOfPinnedObject());

            try
            {
                header->Type = m_byteArrayTypePtr;
                header->Length = (UIntPtr)(length * SizeOfItem);

                action(union.Bytes);
            }
            finally
            {
                header->Type = m_structArrayTypePtr;
                header->Length = (UIntPtr)length;

                gcHandle.Free();
            }
        }

        #endregion

        #region AsStructArray

        public void AsStructArray(byte[] byteArray, Action<T[]> action)
        {
            if (byteArray == null)
            {
                action(null);
                return;
            }

            if (byteArray.Length == 0)
            {
                action(m_structStub);
                return;
            }

            int length = byteArray.Length / SizeOfItem;

            if (length * SizeOfItem != byteArray.Length)
            {
                throw new Exception(string.Format("Length of the byte array must be a multiple of {0}", SizeOfItem));
            }

            ArrayUnion union = new ArrayUnion { Bytes = byteArray };

            GCHandle gcHandle = GCHandle.Alloc(byteArray, GCHandleType.Pinned);
            ArrayHeader* header = GetArrayHeader((void*)gcHandle.AddrOfPinnedObject());

            try
            {
                header->Type = m_structArrayTypePtr;
                header->Length = (UIntPtr)length;

                action((T[])union.Structs);
            }
            finally
            {
                header->Type = m_byteArrayTypePtr;
                header->Length = (UIntPtr)(length * SizeOfItem);

                gcHandle.Free();
            }
        }

        public T AsStruct(byte[] byteArray)
        {
            if (byteArray.Length != SizeOfItem)
            {
                throw new Exception(string.Format("Length of the byte array must be equal {0}", SizeOfItem));
            }

            T result = new T();
            AsStructArray(byteArray, structs =>
            {
                result = structs[0];
            });

            return result;
        }

        #endregion

        private static UIntPtr GetArrayTypePtr(Array array)
        {
            GCHandle gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            ArrayHeader* header = GetArrayHeader((void*)gcHandle.AddrOfPinnedObject());
            gcHandle.Free();

            return header->Type;
        }

        private static ArrayHeader* GetArrayHeader(void* p)
        {
            return (ArrayHeader*)p - 1;
        }
    }
}
