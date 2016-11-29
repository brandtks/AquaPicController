using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cairo;
using Gtk;
using AquaPic.Utilites;
using TouchWidgetLibrary;

namespace AquaPic.UserInterface
{
    partial class HomeSettings : Dialog
    {
        private Fixed fix;
        private TouchButton saveBtn;
        private TouchButton cancelButton;
        private TouchComboBox widgetCombo;
        private CheckerBoard board;
        private SettingsTextBox rowTextBox;
        private SettingsTextBox columnTextBox;
        private TouchUpDownButtons rowUpDownBtn;
        private TouchUpDownButtons columnUpDownBtn;
        private List<HomeSettingsWidget> widgets;

        public HomeSettings () {
            Name = "Home Screen Widget Placement";
            Title = "Home Screen Widget Placement";
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
            saveBtn.ButtonReleaseEvent += (o, args) => {
                Destroy ();
            };
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

            var widgetLabel = new TouchLabel ();
            widgetLabel.text = "Home Screen Widgets";
            widgetLabel.textAlignment = TouchAlignment.Right;
            widgetLabel.WidthRequest = 185;
            fix.Put (widgetLabel, 5, 11);
            widgetLabel.Show ();

            rowTextBox = new SettingsTextBox ();
            rowTextBox.text = "Row Origin";
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
            rowUpDownBtn.up.ButtonReleaseEvent += OnRowUpDownButtonReleased;
            rowUpDownBtn.down.buttonColor = "grey1";
            rowUpDownBtn.down.ButtonReleaseEvent += OnRowUpDownButtonReleased;
            fix.Put (rowUpDownBtn, 125, 72);
            rowUpDownBtn.Show ();

            columnTextBox = new SettingsTextBox ();
            columnTextBox.text = "Column Origin";
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
            columnUpDownBtn.up.ButtonReleaseEvent += OnColumnUpDownButtonReleased;
            columnUpDownBtn.down.buttonColor = "grey1";
            columnUpDownBtn.down.ButtonReleaseEvent += OnColumnUpDownButtonReleased;
            fix.Put (columnUpDownBtn, 425, 72);
            columnUpDownBtn.Show ();

            widgetCombo = new TouchComboBox ();
            widgets = new List<HomeSettingsWidget> ();
            board = new CheckerBoard ();

            string path = System.IO.Path.Combine (Utils.AquaPicEnvironment, "AquaPicRuntimeProject");
            path = System.IO.Path.Combine (path, "Settings");
            path = System.IO.Path.Combine (path, "mainScreen.json");

            using (StreamReader reader = File.OpenText (path)) {
                JArray ja = (JArray)JToken.ReadFrom (new JsonTextReader (reader));

                foreach (var jt in ja) {
                    var jo = jt as JObject;

                    string name = (string)jo["name"];
                    string group = (string)jo["group"];
                    string type = (string)jo["type"];
                    int column = Convert.ToInt32 (jo["column"]) - 1;
                    int row = Convert.ToInt32 (jo["row"]) - 1;
                    widgetCombo.comboList.Add (string.Format ("{0} {1} ({2})", name, group, type));
                    var widget = new HomeSettingsWidget (name, group, type, row, column);
                    widgets.Add (widget);
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

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs args) {
            if (board.containsNoConflictTiles) {
                if (args.activeIndex != -1) {
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
                if (btn.Name == "Up") {
                    if (CanChangeColumnUp ()) {
                        newColumn++;
                        ChangeWidgetColumn (newColumn);
                    }
                } else if (btn.Name == "Down") {
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
                    columnUpDownBtn.down.buttonColor = "grey1";
                }

                if (!CanChangeColumnUp (index)) {
                    columnUpDownBtn.up.buttonColor = "grey1";
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
