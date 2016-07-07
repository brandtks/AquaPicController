﻿using System;
using System.Collections.Generic;

namespace AquaPic.Utilites
{
    public class CircularBuffer<T>
    {
        private int _maxSize;
        public int maxSize {
            get {
                return _maxSize;
            }
            set {
                var oldMaxSize = _maxSize;
                _maxSize = value;
                //Shitily programmed. This might throw an exception, but probably never will
                if (oldMaxSize > _maxSize) {
                    _buffer.RemoveRange (0, oldMaxSize - _maxSize);
                }
            }
        }

        private List<T> _buffer;
        public List<T> buffer {
            get {
                return _buffer;
            }
        }

        public int count {
            get {
                return _buffer.Count;
            }
        }

        public CircularBuffer (int maxSize) {
            _maxSize = maxSize;
            _buffer = new List<T> ();
        }

        public void Add (T value) {
            _buffer.Add (value);
            while (_buffer.Count > _maxSize) {
                _buffer.RemoveAt (0);
            }
        }
    }
}

