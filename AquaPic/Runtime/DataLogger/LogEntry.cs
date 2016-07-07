using System;
using FileHelpers;

namespace AquaPic.Runtime
{
    [DelimitedRecord(",")]
    public class LogEntry
    {
        [FieldConverter(ConverterKind.Date, "MM/dd/yy-HH:mm:ss")]
        public DateTime dateTime;

        public string eventType;

        [FieldConverter(ConverterKind.Double)]
        public double value;

        public LogEntry () 
            : this (DateTime.MinValue, "value", 0.0) { }
        
        public LogEntry (DateTime dateTime, double value) 
            : this (dateTime, "value", value) { }
        
        public LogEntry (double value) 
            : this (DateTime.Now, "value", value) { }
        
        public LogEntry (DateTime dateTime, string eventType) 
            : this (dateTime, eventType, 0.0) { }
        
        public LogEntry (string eventType)
            : this (DateTime.Now, eventType, 0.0) { }

        public LogEntry (LogEntry entry)
            : this (entry.dateTime, entry.eventType, entry.value) { }
        
        public LogEntry (DateTime dateTime, string eventType, double value) {
            this.dateTime = dateTime;
            this.eventType = eventType;
            this.value = value;
        }
    }
}

