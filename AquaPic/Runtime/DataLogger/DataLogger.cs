using System;
using System.IO;
using System.Collections.Generic;
using FileHelpers;

namespace AquaPic.Runtime
{
    public enum LoggerState {
        FileOpen,
        FileClosed
    }

    public class DataLogger
    {
        FileHelperAsyncEngine<LogEntry> engine;
        LoggerState state;
        string currentFileName, currentFilePath;

        public DataLogger (string name) {
            currentFilePath = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            currentFilePath = Path.Combine (currentFilePath, "DataLogging");
            currentFilePath = Path.Combine (currentFilePath, name);

            state = LoggerState.FileClosed;

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

            var e = new LogEntry ();
            e.dt = DateTime.Now;
            e.value = value;

            engine.WriteNext (e);
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

