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
        public DateTime dateTime;
        public double value;

        public DataLogEntryAddedEventArgs (DateTime dateTime, double value) {
            this.dateTime = dateTime;
            this.value = value;
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
        public event DataLogEntryAddedEventHandler DataLogEntryAddedEvent;

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
            var entry = new LogEntry (DateTime.Now, value);

            var filename = String.Format ("{0:MMddyyHH}.csv", DateTime.Now);
            var writeEngine = new FileHelperEngine<LogEntry> ();
            writeEngine.AppendToFile (Path.Combine (currentFilePath, filename), entry);

            if (DataLogEntryAddedEvent != null) {
                DataLogEntryAddedEvent (this, new DataLogEntryAddedEventArgs (entry.dateTime, entry.value));
            }
        }

        public LogEntry[] GetEntries (int maxEntries) {
            return GetEntries (maxEntries, 1);
        }

        public LogEntry[] GetEntries (int maxEntries, int secondTimeSpan) {
            List<LogEntry> entries = new List<LogEntry> ();
            FileHelperEngine<LogEntry> readEngine = new FileHelperEngine<LogEntry> ();
            int hourIndex = 0;
            var lastEntryTime = DateTime.Now;

            while ((entries.Count < maxEntries) && (hourIndex <= 0)) {
                string filename = string.Format ("{0:MMddyyHH}.csv", DateTime.Now.AddHours (hourIndex));
                string path = Path.Combine (currentFilePath, filename);
                --hourIndex;

                if (File.Exists (path)) {
                    var fileEntries = readEngine.ReadFile (path);
                    if (fileEntries.Length > 0) {
                        Array.Reverse (fileEntries);
                        
                        foreach (var entry in fileEntries) {
                            var difference = lastEntryTime.Subtract (entry.dateTime).TotalSeconds;

                            if (difference > 2.5) {
                                hourIndex = 1;
                                break;
                            }
                            lastEntryTime = entry.dateTime;

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
                        }
                    } else {
                        hourIndex = 1;
                    }
                } else {
                    hourIndex = 1;
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

