#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using Gtk;
using GoodtimeDevelopment.TouchWidget;

namespace AquaPic.Runtime
{
    public delegate void EventAddedHandler ();

    public class Logger
    {
        public static TextBuffer buffer;
        public static event EventAddedHandler EventAddedEvent;

        static Logger () {
            TextTagTable ttt = new TextTagTable ();
            buffer = new TextBuffer (ttt);
        }

        public static void Add (string message) {
            Add (message, new object[0]);
        }

        public static void Add (string message, params object[] args) {
            message = string.Format (message, args);
            AppendTime ();

            var ti = buffer.EndIter;
            buffer.Insert (ref ti, string.Format ("{0}\n", message));

            #if DEBUG
            Console.WriteLine ("{0:MM/dd HH:mm:ss}: {1}", DateTime.Now, message);
            #endif

            EventAddedEvent?.Invoke ();
        }

        public static void AddInfo (string message) {
            AddInfo (message, new object[0]);
        }

        public static void AddInfo (string message, params object[] args) {
            message = string.Format (message, args);
            AppendTime ();

            var tag = new TextTag (null);
            tag.ForegroundGdk = TouchColor.NewGtkColor ("pri");
            buffer.TagTable.Add (tag);

            var ti = buffer.EndIter;
            buffer.InsertWithTags (ref ti, string.Format ("{0}\n", message), tag);

            #if DEBUG
            Console.WriteLine ("{0:MM/dd HH:mm:ss}: {1}", DateTime.Now, message);
            #endif

            EventAddedEvent?.Invoke ();
        }

        public static void AddError (string message) {
            AddError (message, new object[0]);
        }

        public static void AddError (string message, params object[] args) {
            message = string.Format (message, args);
            AppendTime ();

            var tag = new TextTag (null);
            tag.ForegroundGdk = TouchColor.NewGtkColor ("compl");
            buffer.TagTable.Add (tag);

            var ti = buffer.EndIter;
            buffer.InsertWithTags (ref ti, string.Format ("{0}\n", message), tag);

            #if DEBUG
            Console.WriteLine ("{0:MM/dd HH:mm:ss}: {1}", DateTime.Now, message);
            #endif

            EventAddedEvent?.Invoke ();
        }

        public static void AddWarning (string message) {
            AddWarning (message, new object[0]);
        }

        public static void AddWarning (string message, params object[] args) {
            message = string.Format (message, args);
            AppendTime ();

            var tag = new TextTag (null);
            tag.ForegroundGdk = TouchColor.NewGtkColor ("secb");
            buffer.TagTable.Add (tag);

            var ti = buffer.EndIter;
            buffer.InsertWithTags (ref ti, string.Format ("{0}\n", message), tag);

            #if DEBUG
            Console.WriteLine ("{0:MM/dd HH:mm:ss}: {1}", DateTime.Now, message);
            #endif

            EventAddedEvent?.Invoke ();
        }

        protected static void AppendTime () {
            var tag = new TextTag (null);
            tag.ForegroundGdk = TouchColor.NewGtkColor ("seca");
            buffer.TagTable.Add (tag);

            var ti = buffer.EndIter;
            buffer.InsertWithTags (ref ti, string.Format ("{0:MM/dd HH:mm:ss}: ", DateTime.Now), tag);
        }
    }
}

