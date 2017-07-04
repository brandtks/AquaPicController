#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller. 

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

using System;
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

        public T this[int index] {
            get {
                return _buffer[index];
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

        public void AddRange (IEnumerable<T> collection) {
            _buffer.AddRange (collection);
            while (_buffer.Count > _maxSize) {
                _buffer.RemoveAt (0);
            }
        }

        public T[] ToArray () {
            return _buffer.ToArray ();
        }
    }
}

