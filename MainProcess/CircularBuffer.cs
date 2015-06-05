using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainProcess
{
    //circular buffer implementation 
    public class CircularBuffer
    {
        private readonly byte[] _buffer;
        private int _startIndex, _endIndex;

        public CircularBuffer(int capacity)
        {
            _buffer = new byte[capacity];
        }

        private int GetCount()
        {
            if (_endIndex > _startIndex)
                return _endIndex - _startIndex;
            if (_endIndex < _startIndex)
                return (_buffer.Length - _startIndex) + _endIndex;
            return 0;
        }
        //enqueue the data 
        public void Enqueue(byte[] data)
        {
            Write(data);
        }

        public byte[] Dequeue()
        {
            byte[] dataHeader = Read(20);
            short dataLength = BitConverter.ToInt16(dataHeader, 0);
            byte[] data = Read(dataLength);

            return dataHeader.Concat(data).ToArray();
        }

        //write the data
        private void Write(byte[] data)
        {
            if (_endIndex + data.Length >= _buffer.Length)
            {
                var endLen = _buffer.Length - _endIndex;
                var remainingLen = data.Length - endLen;

                Array.Copy(data, 0, _buffer, _endIndex, endLen);
                Array.Copy(data, endLen, _buffer, 0, remainingLen);
                _endIndex = remainingLen;
            }
            else
            {
                Array.Copy(data, 0, _buffer, _endIndex, data.Length);
                _endIndex += data.Length;
            }
        }

        //read the data with its length
        private byte[] Read(int len)
        {
            var result = new byte[len];

            if (_startIndex + len < _buffer.Length)
            {
                Array.Copy(_buffer, _startIndex, result, 0, len);
                _startIndex += len;
                return result;
            }
            else
            {
                var endLen = _buffer.Length - _startIndex;
                var remainingLen = len - endLen;
                Array.Copy(_buffer, _startIndex, result, 0, endLen);
                Array.Copy(_buffer, 0, result, endLen, remainingLen);
                _startIndex = remainingLen;
                return result;
            }
        }

        public byte this[int index]
        {
            get
            {
                if (index >= GetCount()) throw new ArgumentOutOfRangeException();
                return _buffer[(_startIndex + index) % _buffer.Length];
            }
        }

        public IEnumerable Bytes
        {
            get
            {
                for (var i = _startIndex; i < GetCount(); i++)
                    yield return _buffer[(_startIndex + i) % _buffer.Length];
            }
        }
    }
}
