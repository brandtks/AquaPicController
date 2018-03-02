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
using System.IO;
using System.Collections.Generic;
using FileHelpers;
using GoodtimeDevelopment.Utilites;

namespace AquaPic.DataLogging
{
    public class DataLoggerIoImplementation : IDataLogger
    {
        readonly string _currentFilePath;

        public DataLoggerIoImplementation (string name) {
            _name = name;
            _currentFilePath = Path.Combine (Utils.AquaPicEnvironment, "DataLogging");
            _currentFilePath = Path.Combine (_currentFilePath, _name.RemoveWhitespace ());

            if (!Directory.Exists (_currentFilePath)) {
                Directory.CreateDirectory (_currentFilePath);
            }
        }

        public override void AddEntry (double value) {
            var entry = new LogEntry (value);

            var filename = string.Format ("{0:MMddyyHH}.csv", DateTime.Now);
            var writeEngine = new FileHelperEngine<LogEntry> ();
            writeEngine.AppendToFile (Path.Combine (_currentFilePath, filename), entry);

            CallValueLogEntryAddedHandlers (entry);
        }

        public override void AddEntry (string eventType) {
            var entry = new LogEntry (eventType);

            var filename = string.Format ("{0:MMddyyHH}.csv", DateTime.Now);
            var writeEngine = new FileHelperEngine<LogEntry> ();
            writeEngine.AppendToFile (Path.Combine (_currentFilePath, filename), entry);

            CallEventLogEntryAddedHandlers (entry);
        }

        public override LogEntry[] GetValueEntries (int maxEntries, DateTime endSearchTime) {
            return GetValueEntries (maxEntries, 1, endSearchTime);
        }

        public override LogEntry[] GetValueEntries (int maxEntries, int secondTimeSpan, DateTime endSearchTime) {
            var entries = new List<LogEntry> ();
            var readEngine = new FileHelperEngine<LogEntry> ();
            int hourIndex = 0;

            if (Directory.GetFiles (_currentFilePath).Length > 0) {
                while (entries.Count < maxEntries) {
                    var filename = string.Format ("{0:MMddyyHH}.csv", DateTime.Now.AddHours (hourIndex));
                    var path = Path.Combine (_currentFilePath, filename);
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

        public override LogEntry[] GetEventEntries (int maxEntries, DateTime endSearchTime) {
            var entries = new List<LogEntry> ();
            var readEngine = new FileHelperEngine<LogEntry> ();
            int hourIndex = 0;

            if (Directory.GetFiles (_currentFilePath).Length > 0) {
                while (entries.Count < maxEntries) {
                    var filename = string.Format ("{0:MMddyyHH}.csv", DateTime.Now.AddHours (hourIndex));
                    var path = Path.Combine (_currentFilePath, filename);
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
            var files = Directory.GetFiles (_currentFilePath);
            foreach (var file in files) {
                Console.WriteLine ("Deleting {0} for {1}", file, _name);
                File.Delete (file);
            }
        }
        #endif
    }
}

