using System;
using System.IO;
using System.Collections.Generic;
using FileHelpers;
using AquaPic.Utilites;

namespace AquaPic.Runtime
{
    public enum LoggerState {
        FileOpen,
        FileClosed
    }

    public delegate void DataLogEntryAddedEventHandler (object sender, DataLogEntryAddedEventArgs args);

    public class DataLogEntryAddedEventArgs : EventArgs
    {
        public DateTime time;
        public double value;

        public DataLogEntryAddedEventArgs (DateTime time, double value) {
            this.time = time;
            this.value = value;
        }
    }

    public class DataLogger
    {
        FileHelperAsyncEngine<LogEntry> engine;
        LoggerState state;
        string currentFileName, currentFilePath;

        public event DataLogEntryAddedEventHandler DataLogEntryAddedEvent;

        CircularBuffer<LogEntry> rollingStorage;
        public LogEntry[] locallyStoredData {
            get {
                return rollingStorage.buffer.ToArray ();
            }
        }

        public DataLogger (string name) {
            currentFilePath = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            currentFilePath = Path.Combine (currentFilePath, "DataLogging");
            currentFilePath = Path.Combine (currentFilePath, name);

            state = LoggerState.FileClosed;

            rollingStorage = new CircularBuffer<LogEntry> (288);

            if (!Directory.Exists (currentFilePath)) {
                Directory.CreateDirectory (currentFilePath);
            }
        }

        public void AddEntry (double value) {
            if (state == LoggerState.FileClosed) {
                StartLogging ();
            } else {
                string intendedFileName = String.Format ("{0:MMddyyHH}.csv", DateTime.Now);
                if (currentFileName != intendedFileName) {
                    currentFileName = intendedFileName;
                    OpenFile ();
                }
            }

            var entry = new LogEntry ();
            entry.dateTime = DateTime.Now;
            entry.value = value;

            var previous = rollingStorage.buffer [rollingStorage.buffer.Count - 1].dateTime;
            if (entry.dateTime.Subtract (previous).TotalMinutes >= 5) {
                rollingStorage.Add (entry);
            }

            if (DataLogEntryAddedEvent != null) {
                DataLogEntryAddedEvent (this, new DataLogEntryAddedEventArgs (entry.dateTime, entry.value));
            }

            engine.WriteNext (entry);
            engine.Flush ();
        }

        protected void OpenFile () {
            engine.Close ();
            engine.BeginAppendToFile (Path.Combine (currentFilePath, currentFileName));
            state = LoggerState.FileOpen;
        }

        public void StartLogging () {
            if (engine == null) {
                engine = new FileHelperAsyncEngine<LogEntry> ();
            }

            currentFileName = String.Format ("{0:MMddyyHH}.csv", DateTime.Now);
            OpenFile ();
        }

        public void StopLogging () {
            engine.Close ();
            state = LoggerState.FileClosed;
        }
    }
}

