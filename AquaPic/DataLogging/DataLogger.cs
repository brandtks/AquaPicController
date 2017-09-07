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

﻿using System;
using System.IO;
using System.Collections.Generic;
using FileHelpers;
using AquaPic.Utilites;

namespace AquaPic.DataLogging
{
    public delegate void DataLogEntryAddedEventHandler (object sender, DataLogEntryAddedEventArgs args);

    public class DataLogEntryAddedEventArgs : EventArgs
    {
        public LogEntry entry;

        public DataLogEntryAddedEventArgs (LogEntry entry) {
            this.entry = entry;
        }
    }

    public class DataLogger
    {
        string  currentFilePath, _name;
        public string name {
            get {
                return _name;
            }
        }

        public event DataLogEntryAddedEventHandler ValueLogEntryAddedEvent;
        public event DataLogEntryAddedEventHandler EventLogEntryAddedEvent;

        public DataLogger (string name) {
            _name = name;
            currentFilePath = Path.Combine (Utils.AquaPicEnvironment, "DataLogging");
            currentFilePath = Path.Combine (currentFilePath, _name.RemoveWhitespace ());

            if (!Directory.Exists (currentFilePath)) {
                Directory.CreateDirectory (currentFilePath);
            }
        }

        public void AddEntry (double value) {
            var entry = new LogEntry (value);

            var filename = String.Format ("{0:MMddyyHH}.csv", DateTime.Now);
            var writeEngine = new FileHelperEngine<LogEntry> ();
            writeEngine.AppendToFile (Path.Combine (currentFilePath, filename), entry);

            if (ValueLogEntryAddedEvent != null) {
                ValueLogEntryAddedEvent (this, new DataLogEntryAddedEventArgs (entry));
            }
        }

        public void AddEntry (string eventType) {
            var entry = new LogEntry (eventType);

            var filename = String.Format ("{0:MMddyyHH}.csv", DateTime.Now);
            var writeEngine = new FileHelperEngine<LogEntry> ();
            writeEngine.AppendToFile (Path.Combine (currentFilePath, filename), entry);

            if (EventLogEntryAddedEvent != null) {
                EventLogEntryAddedEvent (this, new DataLogEntryAddedEventArgs (entry));
            }
        }

        public LogEntry[] GetValueEntries (int maxEntries, DateTime endSearchTime) {
            return GetValueEntries (maxEntries, 1, endSearchTime);
        }

        public LogEntry[] GetValueEntries (int maxEntries, int secondTimeSpan, DateTime endSearchTime) {
            List<LogEntry> entries = new List<LogEntry> ();
            FileHelperEngine<LogEntry> readEngine = new FileHelperEngine<LogEntry> ();
            int hourIndex = 0;

            if (Directory.GetFiles (currentFilePath).Length > 0) {
                while (entries.Count < maxEntries) {
                    string filename = string.Format ("{0:MMddyyHH}.csv", DateTime.Now.AddHours (hourIndex));
                    string path = Path.Combine (currentFilePath, filename);
                    --hourIndex;

                    if (File.Exists (path)) {
                        var fileEntries = readEngine.ReadFile (path);
                        if (fileEntries.Length > 0) {
                            Array.Reverse (fileEntries);

                            foreach (var entry in fileEntries) {
                                if (entry.eventType == "value") {
                                    if (entry.dateTime.CompareTo (endSearchTime) > -1) {
                                        if (entries.Count > 0) {
                                            var previous = entries[entries.Count - 1].dateTime;
                                            var totalSeconds = previous.Subtract (entry.dateTime).TotalSeconds.ToInt ();

                                            if (totalSeconds >= secondTimeSpan) {
                                                entries.Add (entry);
                                            }
                                        } else {
                                            entries.Add (entry);
                                        }

                                        if (entries.Count == maxEntries) {
                                            break;
                                        }
                                    } else {
                                        maxEntries = entries.Count;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (hourIndex < -24) {
                        break;
                    }
                }
            }

            var returnArray = entries.ToArray ();
            Array.Reverse (returnArray);
            return returnArray;
        }

        public LogEntry[] GetEventEntries (int maxEntries, DateTime endSearchTime) {
            List<LogEntry> entries = new List<LogEntry> ();
            FileHelperEngine<LogEntry> readEngine = new FileHelperEngine<LogEntry> ();
            int hourIndex = 0;

            if (Directory.GetFiles (currentFilePath).Length > 0) {
                while (entries.Count < maxEntries) {
                    string filename = string.Format ("{0:MMddyyHH}.csv", DateTime.Now.AddHours (hourIndex));
                    string path = Path.Combine (currentFilePath, filename);
                    --hourIndex;

                    if (File.Exists (path)) {
                        var fileEntries = readEngine.ReadFile (path);
                        if (fileEntries.Length > 0) {
                            Array.Reverse (fileEntries);

                            foreach (var entry in fileEntries) {
                                if (entry.eventType != "value") {
                                    if (entry.dateTime.CompareTo (endSearchTime) > -1) {
                                        entries.Add (entry);

                                        if (entries.Count == maxEntries) {
                                            break;
                                        }
                                    } else {
                                        maxEntries = entries.Count;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (hourIndex < -24) {
                        break;
                    }
                }
            }

            var returnArray = entries.ToArray ();
            Array.Reverse (returnArray);
            return returnArray;
        }

        #if DEBUG
        public void DeleteAllLogFiles () {           
            var files = Directory.GetFiles (currentFilePath);
            foreach (var file in files) {
                Console.WriteLine ("Deleting {0} for {1}", file, _name);
                File.Delete (file);
            }
        }
        #endif
    }
}

