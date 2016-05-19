using System;
using System.IO;
using System.Collections.Generic;
using FileHelpers;

namespace AquaPic.Runtime
{
    public class DataLogger
    {
        FileHelperAsyncEngine<LogEntry> engine;
        string currentFileName, currentFilePath;

        public DataLogger (string name) {
            currentFilePath = Path.Combine (Environment.GetEnvironmentVariable ("AquaPic"), "AquaPicRuntimeProject");
            currentFilePath = Path.Combine (currentFilePath, "DataLogging");
            currentFilePath = Path.Combine (currentFilePath, name);

            if (!Directory.Exists (currentFilePath)) {
                Directory.CreateDirectory (currentFilePath);
            }
        }

        public void AddEntry (double value) {
            if (engine == null) {
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
        }

        public void StartLogging () {
            if (engine == null) {
                engine = new FileHelperAsyncEngine<LogEntry> ();
            }

            currentFileName = String.Format ("{0:MMddyyHH}.csv", DateTime.Now);
            OpenFile ();
        }
    }
}

