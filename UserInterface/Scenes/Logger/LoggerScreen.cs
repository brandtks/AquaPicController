#region License

/*
    AquaPic Main Control - Handles all functionality for the AquaPic aquarium controller.

    Copyright (c) 2017 Goodtime Development

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
using System.Collections.Generic;
using System.IO;
using Gtk;
using GoodtimeDevelopment.TouchWidget;
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class LoggerWindow : SceneBase
    {
        public TextView tv;
        public TextBuffer buffer;

        public LoggerWindow (params object[] options) : base () {
            sceneTitle = "Logger";

            TextTagTable ttt = new TextTagTable ();
            buffer = new TextBuffer (ttt);

            var tag = new TextTag ("DateTimeTag");
            tag.ForegroundGdk = TouchColor.NewGtkColor ("seca");
            buffer.TagTable.Add (tag);

            tag = new TextTag ("InfoTag");
            tag.ForegroundGdk = TouchColor.NewGtkColor ("pri");
            buffer.TagTable.Add (tag);

            tag = new TextTag ("WarningTag");
            tag.ForegroundGdk = TouchColor.NewGtkColor ("secb");
            buffer.TagTable.Add (tag);

            tag = new TextTag ("ErrorTag");
            tag.ForegroundGdk = TouchColor.NewGtkColor ("compl");
            buffer.TagTable.Add (tag);

            foreach (var log in Logger.logs) {
                AddLogToBuffer (log);
            }

            tv = new TextView (buffer);
            tv.ModifyFont (Pango.FontDescription.FromString ("Sans 11"));
            tv.ModifyBase (StateType.Normal, TouchColor.NewGtkColor ("grey4"));
            tv.ModifyText (StateType.Normal, TouchColor.NewGtkColor ("black"));
            tv.CanFocus = false;
            tv.Editable = false;

            ScrolledWindow sw = new ScrolledWindow ();
            sw.SetSizeRequest (720, 340);
            sw.VScrollbar.WidthRequest = 30;
            sw.HScrollbar.HeightRequest = 30;
            sw.Add (tv);
            Put (sw, 65, 60);
            sw.Show ();
            tv.Show ();

            var b = new TouchButton ();
            b.SetSizeRequest (100, 60);
            b.text = "Clear Logger";
            b.ButtonReleaseEvent += OnClearButtonRelease;
            Put (b, 685, 405);

            b = new TouchButton ();
            b.SetSizeRequest (100, 60);
            b.text = "Save Logger";
            b.ButtonReleaseEvent += (o, args) => SaveEvents ();
            Put (b, 575, 405);

            Logger.EventAddedEvent += OnEventAdded;
            Show ();
        }

        public override void Dispose () {
            Logger.EventAddedEvent -= OnEventAdded;
            base.Dispose ();
        }

        protected void OnEventAdded (LogItem log) {
            AddLogToBuffer (log);
            tv.QueueDraw ();
        }

        protected void AddLogToBuffer (LogItem log) {
            // Get the text tag for the time stamp
            var tag = buffer.TagTable.Lookup ("DateTimeTag");
            var ti = buffer.EndIter;
            // Add the time stamp to the text buffer with the appropriate tag
            buffer.InsertWithTags (ref ti, string.Format ("{0:MM/dd HH:mm:ss}: ", log.timeStamp), tag);

            switch (log.type) {
            case LogType.General:
                ti = buffer.EndIter;
                buffer.Insert (ref ti, string.Format ("{0}\n", log.message));
                break;
            case LogType.Info:
                tag = buffer.TagTable.Lookup ("InfoTag");
                ti = buffer.EndIter;
                buffer.InsertWithTags (ref ti, string.Format ("{0}\n", log.message), tag);
                break;
            case LogType.Warning:
                tag = buffer.TagTable.Lookup ("WarningTag");
                ti = buffer.EndIter;
                buffer.InsertWithTags (ref ti, string.Format ("{0}\n", log.message), tag);
                break;
            case LogType.Error:
                tag = buffer.TagTable.Lookup ("ErrorTag");
                ti = buffer.EndIter;
                buffer.InsertWithTags (ref ti, string.Format ("{0}\n", log.message), tag);
                break;
            default:
                break;
            }
        }

        protected void OnClearButtonRelease (object sender, ButtonReleaseEventArgs args) {
            var parent = Toplevel as Window;
            var ms = new TouchDialog ("Save events before clearing", parent);

            ms.Response += (o, a) => {
                if (a.ResponseId == ResponseType.Yes) {
                    SaveEvents ();
                    buffer.Clear ();
                } else if (a.ResponseId == ResponseType.No) {
                    ms.Destroy ();

                    var parent2 = Toplevel as Window;
                    var d = new TouchDialog ("Are you sure you want to clear all the contents of the event logger", parent2);

                    d.Response += (obj, arg) => {
                        if (arg.ResponseId == ResponseType.Yes)
                            buffer.Clear ();
                    };

                    d.Run ();
                    d.Destroy ();
                }
            };

            ms.Run ();
            ms.Destroy ();
        }

        protected void SaveEvents () {
            if (!string.IsNullOrWhiteSpace (buffer.Text)) {
                var path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Logs");
                path = System.IO.Path.Combine (path, DateTime.Now.ToString ("yy-MM-dd-HH-mm-ss") + ".txt");

                List<string> lines = new List<string> ();
                for (int i = 0; i < buffer.LineCount; ++i) {
                    TextIter tis = buffer.GetIterAtLine (i);
                    TextIter tie = buffer.GetIterAtLine (i + 1);
                    lines.Add (buffer.GetText (tis, tie, true));
                }

                string[] l = lines.ToArray ();

                File.WriteAllLines (path, l);
            }
        }
    }
}

