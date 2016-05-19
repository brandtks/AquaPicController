using System;
using FileHelpers;

namespace AquaPic.Runtime
{
    [DelimitedRecord(",")]
    public class LogEntry
    {
        [FieldConverter(ConverterKind.Date, "MM/dd/yy-HH:mm:ss")]
        public DateTime dt;

        [FieldConverter(ConverterKind.Double)]
        public double value;
    }
}

