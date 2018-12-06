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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GoodtimeDevelopment.Utilites;
using GoodtimeDevelopment.TouchWidget;

namespace AquaPic.UserInterface
{
    partial class HomeSettingsDialog : Dialog
    {
        Fixed fix;
        TouchButton saveBtn;
        TouchButton cancelButton;
		TouchButton deleteButton;
        TouchComboBox widgetCombo;
        TileBoard board;
        SettingsTextBox rowTextBox;
        SettingsTextBox columnTextBox;
        TouchUpDownButtons rowUpDownBtn;
        TouchUpDownButtons columnUpDownBtn;
        List<HomeSettingsWidget> widgets;

        public HomeSettingsDialog (Window parent) : base ("Home Screen Widget Placement", parent, DialogFlags.DestroyWithParent) {
            Name = "AquaPic.Home.Settings.Widget.Placement";
            WindowPosition = (WindowPosition)4;
            SetSizeRequest (600, 320);

#if RPI_BUILD
            Decorated = false;

            ExposeEvent += (o, args) => {
                using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                    cr.MoveTo (Allocation.Left, Allocation.Top);
                    cr.LineTo (Allocation.Right, Allocation.Top);
                    cr.LineTo (Allocation.Right, Allocation.Bottom);
                    cr.LineTo (Allocation.Left, Allocation.Bottom);
                    cr.ClosePath ();
                    cr.LineWidth = 1.8;
                    TouchColor.SetSource (cr, "grey4");
                    cr.Stroke ();
                }
            };
#endif

            ModifyBg (StateType.Normal, TouchColor.NewGtkColor ("grey0"));

            foreach (Widget w in Children) {
                Remove (w);
                w.Dispose ();
            }

            fix = new Fixed ();
            fix.SetSizeRequest (600, 320);

            saveBtn = new TouchButton ();
            saveBtn.SetSizeRequest (100, 30);
            saveBtn.text = "Save";
            saveBtn.ButtonReleaseEvent += OnSaveButtonReleased;
            fix.Put (saveBtn, 495, 285);
            saveBtn.Show ();

            cancelButton = new TouchButton ();
            cancelButton.SetSizeRequest (100, 30);
            cancelButton.text = "Cancel";
            cancelButton.ButtonReleaseEvent += (o, args) => {
                Destroy ();
            };
            fix.Put (cancelButton, 385, 285);
            cancelButton.Show ();

			deleteButton = new TouchButton ();
			deleteButton.SetSizeRequest (100, 30);
			deleteButton.text = "Delete";
			deleteButton.ButtonReleaseEvent += OnDeleteButtonRelease;
			deleteButton.buttonColor = "compl";
			fix.Put (deleteButton, 5, 285);

            var widgetLabel = new TouchLabel ();
            widgetLabel.text = "Home Screen Widgets";
            widgetLabel.textAlignment = TouchAlignment.Right;
            widgetLabel.WidthRequest = 185;
            fix.Put (widgetLabel, 5, 11);
            widgetLabel.Show ();

            rowTextBox = new SettingsTextBox ("Row Origin");
            rowTextBox.textBox.TextChangedEvent += (o, a) => {
                if (widgetCombo.activeIndex != -1) {
                    var newRow = widgets[widgetCombo.activeIndex].rowOrigin;
                    try {
                        newRow = Convert.ToInt32 (a.text);
                    } catch {
                        MessageBox.Show ("Improper integer number format");
                        a.keepText = false;
                    }

                    if ((newRow < 0) || ((newRow + widgets[widgetCombo.activeIndex].height - 1) > 4)) {
                        MessageBox.Show ("Row outside range");
                        a.keepText = false;
                    } else {
                        ChangeWidgetRow (newRow);
                        SetColorOfRowUpDownButtons ();
                    }
                    
                } else {
                    MessageBox.Show ("Please select a widget");
                    a.keepText = false;
                }
            };
            fix.Put (rowTextBox, 5, 40);
            rowTextBox.Show ();

            rowUpDownBtn = new TouchUpDownButtons ();
            rowUpDownBtn.SetSizeRequest (170, 40);

            rowUpDownBtn.up.buttonColor = "grey1";
            rowUpDownBtn.up.text = Convert.ToChar (0x2193).ToString ();
            rowUpDownBtn.up.ButtonReleaseEvent += OnRowUpDownButtonReleased;

            rowUpDownBtn.down.buttonColor = "grey1";
            rowUpDownBtn.down.text = Convert.ToChar (0x2191).ToString ();
            rowUpDownBtn.down.ButtonReleaseEvent += OnRowUpDownButtonReleased;

            fix.Put (rowUpDownBtn, 125, 72);
            rowUpDownBtn.Show ();

            columnTextBox = new SettingsTextBox ("Column Origin");
            columnTextBox.textBox.TextChangedEvent += (o, a) => {
                if (widgetCombo.activeIndex != -1) {
                    var newColumn = widgets[widgetCombo.activeIndex].columnOrigin;
                    try {
                        newColumn = Convert.ToInt32 (a.text);
                    } catch {
                        MessageBox.Show ("Improper integer number format");
                        a.keepText = false;
                    }

                    if ((newColumn < 0) || ((newColumn + widgets[widgetCombo.activeIndex].width - 1) > 6)) {
                        MessageBox.Show ("Column outside range");
                        a.keepText = false;
                    } else {
                        ChangeWidgetColumn (newColumn);
                        SetColorOfColumnUpDownButtons ();
                    }
                } else {
                    MessageBox.Show ("Please select a widget");
                    a.keepText = false;
                }
            };
            fix.Put (columnTextBox, 305, 40);
            columnTextBox.Show ();

            columnUpDownBtn = new TouchUpDownButtons ();
            columnUpDownBtn.SetSizeRequest (170, 40);

            columnUpDownBtn.up.buttonColor = "grey1";
            columnUpDownBtn.up.text = Convert.ToChar (0x2190).ToString ();
            columnUpDownBtn.up.ButtonReleaseEvent += OnColumnUpDownButtonReleased;

            columnUpDownBtn.down.buttonColor = "grey1";
            columnUpDownBtn.down.text = Convert.ToChar (0x2192).ToString ();
            columnUpDownBtn.down.ButtonReleaseEvent += OnColumnUpDownButtonReleased;
            
            fix.Put (columnUpDownBtn, 425, 72);
            columnUpDownBtn.Show ();

            widgetCombo = new TouchComboBox ();
            widgets = new List<HomeSettingsWidget> ();
            board = new TileBoard ();

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
            path = System.IO.Path.Combine (path, "mainScreen.json");

            using (StreamReader reader = File.OpenText (path)) {
                JArray ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                foreach (var jt in ja) {
                    var jo = jt as JObject;

                    string name = (string)jo["name"];
                    string group = (string)jo["group"];
                    string type = (string)jo["type"];
                    int column = Convert.ToInt32 (jo["column"]);
                    int row = Convert.ToInt32 (jo["row"]);
                    var widget = new HomeSettingsWidget (name, group, type, row, column);
                    widgets.Add (widget);
                    widgetCombo.comboList.Add (widget.fullName);
                    board.OccupyTile (widget);
                }
            }

            fix.Put (board, 189, 120);
            board.Show ();

            widgetCombo.WidthRequest = 400;
            widgetCombo.comboList.Add ("Add new");
            widgetCombo.ComboChangedEvent += OnComboChanged;
            fix.Put (widgetCombo, 195, 5);
            widgetCombo.Show ();

            ExposeEvent += (obj, args) => {
                widgetCombo.Visible = false;
                widgetCombo.Visible = true;
            };

            Add (fix);
            fix.Show ();
        }

        protected void OnSaveButtonReleased (object sender, ButtonReleaseEventArgs args) {
            if (board.containsNoConflictTiles) {
                var ja = new JArray ();

                foreach (var widget in widgets) {
                    var jo = new JObject ();
                    jo.Add (new JProperty ("type", widget.type));
                    jo.Add (new JProperty ("name", widget.name));
                    if (widget.group.IsNotEmpty ()) {
                        jo.Add (new JProperty ("group", widget.group));
                    }
                    jo.Add (new JProperty ("column", widget.columnOrigin.ToString ()));
                    jo.Add (new JProperty ("row", widget.rowOrigin.ToString ()));
                    ja.Add (jo);
                }

                string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "Settings");
                path = System.IO.Path.Combine (path, "mainScreen.json");

                File.WriteAllText (path, ja.ToString ());
                Destroy ();
            } else {
                MessageBox.Show ("Please fix conflict tiles");
            }
        }

		protected void OnDeleteButtonRelease (object sender, ButtonReleaseEventArgs args) {
			if (widgetCombo.activeIndex >= 0) {
				var parent = Toplevel as Window;
				var ms = new TouchDialog ("Are you sure you with to delete " + widgetCombo.activeText, parent);

				ms.Response += (o, a) => {
					if (a.ResponseId == ResponseType.Yes) {
						board.FreeTile (widgets[widgetCombo.activeIndex]);
						widgets.RemoveAt (widgetCombo.activeIndex);
						widgetCombo.activeIndex = -1;
						rowTextBox.textBox.text = string.Empty;
						columnTextBox.textBox.text = string.Empty;
						SetColorOfRowUpDownButtons (-1);
						SetColorOfColumnUpDownButtons (-1);

						rowTextBox.QueueDraw ();
						columnTextBox.QueueDraw ();
						board.QueueDraw ();
						widgetCombo.QueueDraw ();
					}
				};

				ms.Run ();
				ms.Destroy ();
			}
		}

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs args) {
            if (board.containsNoConflictTiles) {
                if (args.activeIndex != -1) {
                    if (args.activeText == "Add new") {
                        var parent = Toplevel as Window;
                        var s = new AddHomeWidgetDialog (parent);
                        s.Run ();
                        var newWidget = s.newWidget;
                        s.Destroy ();
                        s.Dispose ();

                        if (newWidget != null) {
                            widgets.Add (newWidget);
                            widgetCombo.comboList.Insert (widgetCombo.comboList.Count - 1, newWidget.fullName);
                            board.OccupyTile (newWidget);

                            int index = widgetCombo.comboList.Count - 2;
                            widgetCombo.activeIndex = index;
                            rowTextBox.textBox.text = widgets[index].rowOrigin.ToString ();
                            columnTextBox.textBox.text = widgets[index].columnOrigin.ToString ();
                            SetColorOfRowUpDownButtons (index);
                            SetColorOfColumnUpDownButtons (index);

                            rowTextBox.QueueDraw ();
                            columnTextBox.QueueDraw ();
                            board.QueueDraw ();
                            widgetCombo.QueueDraw ();
                        } else {
                            args.keepChange = false;
                        }
                    } else {
                        int index = args.activeIndex;
                        rowTextBox.textBox.text = widgets[index].rowOrigin.ToString ();
                        columnTextBox.textBox.text = widgets[index].columnOrigin.ToString ();
                        board.HighlightTile (widgets[index]);
                        SetColorOfRowUpDownButtons (index);
                        SetColorOfColumnUpDownButtons (index);

                        rowTextBox.QueueDraw ();
                        columnTextBox.QueueDraw ();
                        board.QueueDraw ();
                    }
                }
            } else {
                MessageBox.Show ("Please fix conflict tiles");
                args.keepChange = false;
            }
        }

        private void ChangeWidgetRow (int newRow) {
            int index = widgetCombo.activeIndex;
            if (index != -1) {
                board.FreeTile (widgets[index]);
                widgets[index].rowOrigin = newRow;
                board.OccupyTile (widgets[index]);
                board.HighlightTile (widgets[index]);
                board.QueueDraw ();
            }
        }

        protected void OnRowUpDownButtonReleased (object sender, ButtonReleaseEventArgs args) {
            if (widgetCombo.activeIndex != -1) {
                var btn = sender as TouchButton;
                var newRow = Convert.ToInt32 (rowTextBox.textBox.text);
                if (btn.Name == "Up") {
                    if (CanChangeRowUp ()) {
                        newRow++;
                        ChangeWidgetRow (newRow);
                    }
                } else if (btn.Name == "Down") {
                    if (CanChangeRowDown ()) {
                        newRow--;
                        ChangeWidgetRow (newRow);
                    }
                }
                SetColorOfRowUpDownButtons ();
                rowTextBox.textBox.text = newRow.ToString ();
                rowTextBox.QueueDraw ();
            }
        }

        private void SetColorOfRowUpDownButtons () {
            int index = widgetCombo.activeIndex;
            if (index != -1) {
                SetColorOfRowUpDownButtons (index);
            }
        }

        private void SetColorOfRowUpDownButtons (int index) {
            if (index != -1) {
                rowUpDownBtn.up.buttonColor = "pri";
                rowUpDownBtn.down.buttonColor = "pri";

                if (!CanChangeRowDown (index)) {
                    rowUpDownBtn.down.buttonColor = "grey1";
                }

                if (!CanChangeRowUp (index)) {
                    rowUpDownBtn.up.buttonColor = "grey1";
                }
            } else {
                rowUpDownBtn.up.buttonColor = "grey1";
                rowUpDownBtn.down.buttonColor = "grey1";
            }

            rowUpDownBtn.QueueDraw ();
        }

        private bool CanChangeRowDown () {
            int index = widgetCombo.activeIndex;
            if (index != -1) {
                return CanChangeRowDown (index);
            } else {
                return false;
            }
        }

        private bool CanChangeRowDown (int index) {
            if (index != -1) {
                if (widgets[index].rowOrigin <= 0) {
                    return false;
                }
                return true;
            } else {
                return false;
            }
        }

        private bool CanChangeRowUp () {
            int index = widgetCombo.activeIndex;
            if (index != -1) {
                return CanChangeRowUp (index);
            } else {
                return false;
            }
        }

        private bool CanChangeRowUp (int index) {
            if (index != -1) {
                if ((widgets[index].rowOrigin + widgets[index].height - 1) >= 4) {
                    return false;
                }
                return true;
            } else {
                return false;
            }
        }

        private void ChangeWidgetColumn (int newColumn) {
            int index = widgetCombo.activeIndex;
            if (index != -1) {
                board.FreeTile (widgets[index]);
                widgets[index].columnOrigin = newColumn;
                board.OccupyTile (widgets[index]);
                board.HighlightTile (widgets[index]);
                board.QueueDraw ();
            }
        }

        protected void OnColumnUpDownButtonReleased (object sender, ButtonReleaseEventArgs args) {
            if (widgetCombo.activeIndex != -1) {
                var btn = sender as TouchButton;
                var newColumn = Convert.ToInt32 (columnTextBox.textBox.text);
                if (btn.Name == "Down") {
                    if (CanChangeColumnUp ()) {
                        newColumn++;
                        ChangeWidgetColumn (newColumn);
                    }
                } else if (btn.Name == "Up") {
                    if (CanChangeColumnDown ()) {
                        newColumn--;
                        ChangeWidgetColumn (newColumn);
                    }
                }
                SetColorOfColumnUpDownButtons ();
                columnTextBox.textBox.text = newColumn.ToString ();
                columnTextBox.QueueDraw ();
            }
        }

        private void SetColorOfColumnUpDownButtons () {
            int index = widgetCombo.activeIndex;
            if (index != -1) {
                SetColorOfColumnUpDownButtons (index);
            }
        }

        private void SetColorOfColumnUpDownButtons (int index) {
            if (index != -1) {
                columnUpDownBtn.up.buttonColor = "pri";
                columnUpDownBtn.down.buttonColor = "pri";

                if (!CanChangeColumnDown (index)) {
                    columnUpDownBtn.up.buttonColor = "grey1";
                }

                if (!CanChangeColumnUp (index)) {
                    columnUpDownBtn.down.buttonColor = "grey1";
                }
            } else {
                columnUpDownBtn.up.buttonColor = "grey1";
                columnUpDownBtn.down.buttonColor = "grey1";
            }

            columnUpDownBtn.QueueDraw ();
        }

        private bool CanChangeColumnDown () {
            int index = widgetCombo.activeIndex;
            if (index != -1) {
                return CanChangeColumnDown (index);
            } else {
                return false;
            }
        }

        private bool CanChangeColumnDown (int index) {
            if (index != -1) {
                if (widgets[index].columnOrigin <= 0) {
                    return false;
                }
                return true;
            } else {
                return false;
            }
        }

        private bool CanChangeColumnUp () {
            int index = widgetCombo.activeIndex;
            if (index != -1) {
                return CanChangeColumnUp (index);
            } else {
                return false;
            }
        }

        private bool CanChangeColumnUp (int index) {
            if (index != -1) {
                if ((widgets[index].columnOrigin + widgets[index].width - 1) >= 6) {
                    return false;
                }
                return true;
            } else {
                return false;
            }
        }
    }
}

