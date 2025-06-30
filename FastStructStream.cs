using System;
using System.IO;
using System.Collections.Generic;

namespace InfinityBit.IO
{
    public class FastStructStream
    {
        private Stream m_stream;
        private Dictionary<Type, object> m_dict = new Dictionary<Type, object>(8);

        public Stream Stream 
        {
            get { return m_stream;  }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Stream can not be null");

                m_stream = value;
            }
        }

        public FastStructStream()
        {
        }

        public FastStructStream(Stream stream)
        {
            Stream = stream;
        }

        #region Read

        public T Read<T>() where T : struct
        {
            ArrayReinterpreter<T> reinterpreter = GetReinterpreter<T>();

            return reinterpreter.AsByteArray(bytes => 
            { 
                if (m_stream.Read(bytes, 0, bytes.Length) < bytes.Length)
                    throw new EndOfStreamException();
            });
        }

        public void Read<T>(T[] array) where T : struct
        {
            ArrayReinterpreter<T> reinterpreter = GetReinterpreter<T>();

            reinterpreter.AsByteArray(array, bytes =>
            {
                if (m_stream.Read(bytes, 0, bytes.Length) < bytes.Length)
                    throw new EndOfStreamException();
            });
        }

        public void Read<T>(T[] array, int offset, int count) where T : struct
        {
            ArrayReinterpreter<T> reinterpreter = GetReinterpreter<T>();

            reinterpreter.AsByteArray(array, bytes =>
            {
                int byteCount = count * reinterpreter.SizeOfItem;

                if (m_stream.Read(bytes, offset * reinterpreter.SizeOfItem, byteCount) < byteCount)
                    throw new EndOfStreamException();
            });
        }

        #endregion

        #region Write

        public void Write<T>(T value) where T : struct
        {
            ArrayReinterpreter<T> reinterpreter = GetReinterpreter<T>();
            reinterpreter.AsByteArray(value, bytes => m_stream.Write(bytes, 0, bytes.Length));
        }

        public void Write<T>(T[] array) where T : struct
        {
            ArrayReinterpreter<T> reinterpreter = GetReinterpreter<T>();
            reinterpreter.AsByteArray(array, bytes => m_stream.Write(bytes, 0, bytes.Length));
        }

        public void Write<T>(T[] array, int offset, int count) where T : struct
        {
            ArrayReinterpreter<T> reinterpreter = GetReinterpreter<T>();

            reinterpreter.AsByteArray(array, bytes =>
                m_stream.Write(bytes, offset * reinterpreter.SizeOfItem, count * reinterpreter.SizeOfItem));
        }

        #endregion

        public ArrayReinterpreter<T> GetReinterpreter<T>() where T : struct
        {
            Type type = typeof(T);
            object obj;

            if (m_dict.TryGetValue(type, out obj))
            {
                return (ArrayReinterpreter<T>)obj;
            }

            ArrayReinterpreter<T> newReinterpreter = new ArrayReinterpreter<T>();
            m_dict.Add(type, newReinterpreter);

            return newReinterpreter;
        }
    }
}
