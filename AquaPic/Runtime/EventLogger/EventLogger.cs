﻿using System;
using Gtk;
using MyWidgetLibrary;

namespace AquaPic.Runtime
{
    public delegate void EventAddedHandler ();

    public class EventLogger
    {
        public static TextBuffer buffer;
        public static EventAddedHandler EventAddedEvent;

        static EventLogger () {
            TextTagTable ttt = new TextTagTable ();
            buffer = new TextBuffer (ttt);
        }

        public static void Add (string message) {
            var tag = new TextTag (null);
            tag.ForegroundGdk = MyColor.NewGtkColor ("seca");
            buffer.TagTable.Add (tag);

            var ti = buffer.EndIter;
            buffer.InsertWithTags (ref ti, string.Format ("({0:MM/dd hh:mm}): ", DateTime.Now), tag);

            ti = buffer.EndIter;
            buffer.Insert (ref ti, string.Format ("{0}\n", message));

            Console.WriteLine ("({0:MM/dd hh:mm}): {1}", DateTime.Now, message);

            if (EventAddedEvent != null)
                EventAddedEvent ();
        }
    }
}

