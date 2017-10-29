#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Skyler Brandt

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion // License

﻿using System;
using FileHelpers;

namespace AquaPic.DataLogging
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

