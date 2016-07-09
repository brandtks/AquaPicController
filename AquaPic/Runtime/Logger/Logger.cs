using System;
using Gtk;
using TouchWidgetLibrary;

namespace AquaPic.Runtime
{
    public delegate void EventAddedHandler ();

    public class Logger
    {
        public static TextBuffer buffer;
        public static event EventAddedHandler EventAddedEvent;

        static Logger () {
            TextTagTable ttt = new TextTagTable ();
            buffer = new TextBuffer (ttt);
        }

        public static void Add (string message, params object[] args) {
            message = string.Format (message, args);
            AppendTime ();

            var ti = buffer.EndIter;
            buffer.Insert (ref ti, string.Format ("{0}\n", message));

            #if DEBUG
            Console.WriteLine ("{0:MM/dd HH:mm:ss}: {1}", DateTime.Now, message);
            #endif

            if (EventAddedEvent != null)
                EventAddedEvent ();
        }

        public static void AddInfo (string message, params object[] args) {
            message = string.Format (message, args);
            AppendTime ();

            var tag = new TextTag (null);
            tag.ForegroundGdk = TouchColor.NewGtkColor ("pri");
            buffer.TagTable.Add (tag);

            var ti = buffer.EndIter;
            buffer.InsertWithTags (ref ti, string.Format ("{0}\n", message), tag);

            #if DEBUG
            Console.WriteLine ("{0:MM/dd HH:mm:ss}: {1}", DateTime.Now, message);
            #endif

            if (EventAddedEvent != null)
                EventAddedEvent ();
        }

        public static void AddError (string message, params object[] args) {
            message = string.Format (message, args);
            AppendTime ();

            var tag = new TextTag (null);
            tag.ForegroundGdk = TouchColor.NewGtkColor ("compl");
            buffer.TagTable.Add (tag);

            var ti = buffer.EndIter;
            buffer.InsertWithTags (ref ti, string.Format ("{0}\n", message), tag);

            #if DEBUG
            Console.WriteLine ("{0:MM/dd HH:mm:ss}: {1}", DateTime.Now, message);
            #endif

            if (EventAddedEvent != null)
                EventAddedEvent ();
        }

        public static void AddWarning (string message, params object[] args) {
            message = string.Format (message, args);
            AppendTime ();

            var tag = new TextTag (null);
            tag.ForegroundGdk = TouchColor.NewGtkColor ("secb");
            buffer.TagTable.Add (tag);

            var ti = buffer.EndIter;
            buffer.InsertWithTags (ref ti, string.Format ("{0}\n", message), tag);

            #if DEBUG
            Console.WriteLine ("{0:MM/dd HH:mm:ss}: {1}", DateTime.Now, message);
            #endif

            if (EventAddedEvent != null)
                EventAddedEvent ();
        }

        protected static void AppendTime () {
            var tag = new TextTag (null);
            tag.ForegroundGdk = TouchColor.NewGtkColor ("seca");
            buffer.TagTable.Add (tag);

            var ti = buffer.EndIter;
            buffer.InsertWithTags (ref ti, string.Format ("{0:MM/dd HH:mm:ss}: ", DateTime.Now), tag);
        }
    }
}

