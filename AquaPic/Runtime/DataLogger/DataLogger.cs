using System;
using System.IO;
using System.Collections.Generic;
using FileHelpers;
using AquaPic.Utilites;

namespace AquaPic.Runtime
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
            currentFilePath = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            currentFilePath = Path.Combine (currentFilePath, "DataLogging");
            currentFilePath = Path.Combine (currentFilePath, _name);

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
                                    break;
                                }
                            }
                        }
                    }
                }

                if (hourIndex < endSearchTime.Hour) {
                    break;
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
                                    break;
                                }
                            }
                        }
                    }
                }

                if (hourIndex < endSearchTime.Hour) {
                    break;
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

