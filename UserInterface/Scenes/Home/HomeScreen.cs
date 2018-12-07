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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;
using AquaPic.Runtime;

namespace AquaPic.UserInterface
{
    public class HomeWindow : SceneBase
    {
        List<HomeWidget> widgets;

        public HomeWindow (params object[] options) {
            showTitle = false;

            widgets = new List<HomeWidget> ();

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "mainScreen.json");

            if (File.Exists (path)) {
                using (StreamReader reader = File.OpenText (path)) {
                    var ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                    foreach (var jt in ja) {
                        var jo = jt as JObject;

                        var name = (string)jo["name"];
                        var type = (string)jo["type"];

                        var column = -1;
                        var row = -1;
                        try {
                            column = Convert.ToInt32 (jo["column"]);
                            row = Convert.ToInt32 (jo["row"]);
                        } catch {
                            Logger.AddWarning (string.Format ("Invalid row or column for {0}", name));
                            continue;
                        }

                        HomeWidget widget = null;
                        switch (type) {
                        case "Timer": {
                                widget = new DeluxeTimerWidget (name, row, column);
                                break;
                            }
                        case "LinePlot": {
                                var group = (string)jo["group"];

                                if (HomeWindowWidgets.linePlots.ContainsKey (name)) {
                                    widget = HomeWindowWidgets.linePlots[name].CreateInstance (group, row, column);
                                } else {
                                    Logger.AddWarning (string.Format ("Unknown line plot for main window: {0}", name));
                                }

                                break;
                            }
                        case "BarPlot": {
                                var group = (string)jo["group"];

                                if (HomeWindowWidgets.barPlots.ContainsKey (name)) {
                                    widget = HomeWindowWidgets.barPlots[name].CreateInstance (group, row, column);
                                } else {
                                    Logger.AddWarning (string.Format ("Unknown bar plot for main window: {0}", name));
                                }

                                break;
                            }
                        case "CurvedBarPlot": {
                                var group = (string)jo["group"];

                                if (HomeWindowWidgets.curvedBarPlots.ContainsKey (name)) {
                                    widget = HomeWindowWidgets.curvedBarPlots[name].CreateInstance (group, row, column);
                                } else {
                                    Logger.AddWarning (string.Format ("Unknown bar plot for main window: {0}", name));
                                }

                                break;
                            }
                        case "Button": {
                                widget = new ButtonWidget (name, row, column);
                                break;
                            }
                        default:
                            Logger.AddWarning (string.Format ("Unknown widget for main window: {0}", type));
                            break;
                        }

                        if (widget != null) {
                            Put (widget, column * 105 + 50, row * 87 + 32);
                            widget.Show ();
                            widgets.Add (widget);
                        }
                    }
                }
            } else {
                Logger.Add ("Home screen file did not exist, created new home screen file");
                var file = File.Create (path);
                file.Close ();
                var ja = new JArray ();
                File.WriteAllText (path, ja.ToString ());
            }

            Update ();
            Show ();
        }

        protected override bool OnUpdateTimer () {
            Update ();
            return true;
        }

        protected void Update () {
            foreach (var widget in widgets) {
                var updateWidget = widget as IHomeWidgetUpdatable;
                if (updateWidget != null) {
                    updateWidget.Update ();
                }
            }
            QueueDraw ();
        }

    }
}

