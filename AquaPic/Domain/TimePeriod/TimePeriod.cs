#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2018 Goodtime Development

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/
*/

#endregion // License

using System;
using GoodtimeDevelopment.Utilites;

namespace AquaPic.Runtime
{
    public class TimePeriod
    {
        public string startTimeDescriptor;
        protected DateSpan _startTime;
        public DateSpan startTime {
            get {
                return _startTime;
            }
        }

        public string endTimeDescriptor;
        protected DateSpan _endTime;
        public DateSpan endTime {
            get {
                return _endTime;
            }
        }

        public TimePeriod (string startTimeDescriptor, string endTimeDescriptor) {
            this.startTimeDescriptor = startTimeDescriptor;
            this.endTimeDescriptor = endTimeDescriptor;
            ParseTimeDescriptors ();
        }

        public void ParseTimeDescriptors (bool forceTomorrow = false) {
            _startTime = ParseTimeDescriptor (startTimeDescriptor, forceTomorrow);
            _endTime = ParseTimeDescriptor (endTimeDescriptor, forceTomorrow);
            if (_startTime.After (_endTime)) {
                _endTime = ParseTimeDescriptor (endTimeDescriptor, true);
            }
        }

        protected DateSpan ParseTimeDescriptor (string timeDescriptor, bool forceTomorrow = false) {
            var time = DateSpan.Zero;
            var segments = timeDescriptor.Split (' ');
            if (segments.Length == 1) { // There is only a single time element to parse
                if (segments[0].StartsWith ("sun", StringComparison.InvariantCultureIgnoreCase)) { // The time to parse is abstruct based on sunrise or sunset
                    if (forceTomorrow) { // if forcing tomorrow's time, append tomorrow to the end of the abstruct time period
                        segments[0] = segments[0] + "tomorrow";
                    }
                    time = AbstractTimes.GetDateSpan (segments[0]);
                } else if (segments[0].Contains (":")) { // The time to parse is an absolute time
                    time = new DateSpan (Time.Parse (segments[0]));
                    if (forceTomorrow) {
                        time.AddDays (1);
                    }
                } else {
                    throw new Exception ("Can not parse time period");
                }
            } else if (segments.Length == 3) { // The time descriptor has some variation attacted, ie sunrise - 2:00 for sunrise minus 2 hours
                if (segments[0].StartsWith ("sun", StringComparison.InvariantCultureIgnoreCase)) { // The time to parse is abstruct based on sunrise or sunset
                    if (forceTomorrow) { // if forcing tomorrow's time, append tomorrow to the end of the abstruct time period
                        segments[0] = segments[0] + "tomorrow";
                    }
                    time = AbstractTimes.GetDateSpan (segments[0]);
                } else if (segments[0].Contains (":")) { // The time to parse is an absolute time
                    time = new DateSpan (Time.Parse (segments[0]));
                    if (forceTomorrow) {
                        time.AddDays (1);
                    }
                } else {
                    throw new Exception ("Can not parse time period");
                }

                var variation = Timer.Parse (segments[2]);

                if (segments[1] == "+") {
                    time.AddTime (variation);
                } else if (segments[1] == "-") {
                    time.AddTime (variation.Negate ());
                } else {
                    throw new Exception ("Can not parse time period");
                }
            }

            return time;
        }
    }
}
