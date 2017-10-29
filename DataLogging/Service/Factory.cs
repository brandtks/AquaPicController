using System;

namespace AquaPic.DataLogging
{
    public class Factory
    {
        public static IDataLogger GetDataLogger (string name) {
            return new DataLoggerIoImplementation (name);
        }
    }
}
