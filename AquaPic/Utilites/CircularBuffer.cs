using System;
using System.Collections.Generic;

namespace AquaPic.Utilites
{
    public class CircularBuffer<T>
    {
        private int _size;
        public int size {
            get {
                return _size;
            }
        }

        public List<T> buffer;

        public CircularBuffer (int size) {
            _size = size;
            buffer = new List<T> ();
        }

        public void Add (T value) {
            buffer.Add (value);
            while (buffer.Count > _size) {
                buffer.RemoveAt (0);
            }
        }
    }
}

