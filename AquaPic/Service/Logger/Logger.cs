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
using System.Collections.Generic;

namespace AquaPic.Service
{
    public delegate void EventAddedHandler (LogItem log);

    public class Logger
    {
        public static event EventAddedHandler EventAddedEvent;
        public static List<LogItem> logs;

        static Logger () {
            logs = new List<LogItem> ();
        }

        public static void Add (string message) {
            Add (message, new object[0]);
        }

        public static void Add (string message, params object[] args) {
            message = string.Format (message, args);
            var log = new LogItem (DateTime.Now, LogType.General, message);
            logs.Add (log);

#if DEBUG
            Console.WriteLine ("{0:MM/dd HH:mm:ss}: {1}", DateTime.Now, message);
#endif

            EventAddedEvent?.Invoke (log);
        }

        public static void AddInfo (string message) {
            AddInfo (message, new object[0]);
        }

        public static void AddInfo (string message, params object[] args) {
            message = string.Format (message, args);
            var log = new LogItem (DateTime.Now, LogType.Info, message);
            logs.Add (log);

#if DEBUG
            Console.WriteLine ("{0:MM/dd HH:mm:ss}: {1}", DateTime.Now, message);
#endif

            EventAddedEvent?.Invoke (log);
        }

        public static void AddWarning (string message) {
            AddWarning (message, new object[0]);
        }

        public static void AddWarning (string message, params object[] args) {
            message = string.Format (message, args);
            var log = new LogItem (DateTime.Now, LogType.Warning, message);
            logs.Add (log);

#if DEBUG
            Console.WriteLine ("{0:MM/dd HH:mm:ss}: {1}", DateTime.Now, message);
#endif

            EventAddedEvent?.Invoke (log);
        }

        public static void AddError (string message) {
            AddError (message, new object[0]);
        }

        public static void AddError (string message, params object[] args) {
            message = string.Format (message, args);
            var log = new LogItem (DateTime.Now, LogType.Error, message);
            logs.Add (log);

#if DEBUG
            Console.WriteLine ("{0:MM/dd HH:mm:ss}: {1}", DateTime.Now, message);
#endif

            EventAddedEvent?.Invoke (log);
        }
    }
}

