using System;
using FileHelpers;

namespace AquaPic.Runtime
{
    [DelimitedRecord(",")]
    public class LogEntry
    {
        [FieldConverter(ConverterKind.Date, "MM/dd/yy-HH:mm:ss")]
        public DateTime dateTime;

        [FieldConverter(ConverterKind.Double)]
        public double value;

        public LogEntry (DateTime dateTime, double value) {
            this.dateTime = dateTime;
            this.value = value;
        }

        public LogEntry () {
            dateTime = DateTime.MinValue;
            value = 0.0;
        }
    }
}

