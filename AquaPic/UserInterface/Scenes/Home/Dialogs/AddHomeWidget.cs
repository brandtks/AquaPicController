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
using System.Text.RegularExpressions;
using Gtk;
using GoodtimeDevelopment.Utilites;
using GoodtimeDevelopment.TouchWidget;
using AquaPic.Modules;

namespace AquaPic.UserInterface
{
    partial class HomeSettings
    {
        private class AddHomeWidget : Dialog
        {
            private Fixed fix;
            private TouchButton addBtn;
            private TouchButton cancelButton;
            private TouchComboBox typeCombo;
            private SettingsTextBox nameTextBox;

			private SettingsComboBox moduleComboBox;
			private SettingsComboBox groupComboBox;

			private SettingsComboBox nameComboBox;

            public HomeSettingsWidget newWidget;

            public AddHomeWidget () {
                Name = "Add Home Screen Widget";
                Title = "Add Home Screen Widget";
                WindowPosition = (WindowPosition)4;
                SetSizeRequest (600, 160);

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
                fix.SetSizeRequest (600, 150);

                addBtn = new TouchButton ();
                addBtn.SetSizeRequest (100, 30);
                addBtn.text = "Add";
                addBtn.ButtonReleaseEvent += OnAddButtonReleased;
                fix.Put (addBtn, 495, 125);
                addBtn.Show ();

                cancelButton = new TouchButton ();
                cancelButton.SetSizeRequest (100, 30);
                cancelButton.text = "Cancel";
                cancelButton.ButtonReleaseEvent += (o, args) => {
                    Destroy ();
                };
                fix.Put (cancelButton, 385, 125);
                cancelButton.Show ();

                var widgetLabel = new TouchLabel ();
                widgetLabel.text = "Home Screen Widgets";
                widgetLabel.textAlignment = TouchAlignment.Right;
                widgetLabel.WidthRequest = 185;
                fix.Put (widgetLabel, 5, 11);
                widgetLabel.Show ();

                nameTextBox = new SettingsTextBox ("Name");
				nameTextBox.Visible = false;
                fix.Put (nameTextBox, 5, 40);
                
				moduleComboBox = new SettingsComboBox ("Module");
				moduleComboBox.combo.nonActiveMessage = "Please select module";
				moduleComboBox.combo.ComboChangedEvent += OnModuleComboChanged;
				moduleComboBox.Visible = false;
				fix.Put (moduleComboBox, 5, 40);

				nameComboBox = new SettingsComboBox ("Name");
				nameComboBox.combo.nonActiveMessage = "Please select name";
				nameComboBox.combo.ComboChangedEvent += OnModuleComboChanged;
				nameComboBox.Visible = false;
				fix.Put (nameComboBox, 5, 40);

				groupComboBox = new SettingsComboBox ("Group");
				groupComboBox.combo.nonActiveMessage = "Please select group";
				groupComboBox.Visible = false;
				fix.Put (groupComboBox, 305, 40);

                typeCombo = new TouchComboBox ();
                typeCombo.comboList.Add ("Line Plot");
                typeCombo.comboList.Add ("Curved Bar Plot");
                typeCombo.comboList.Add ("Button");
                typeCombo.comboList.Add ("Timer");
                typeCombo.comboList.Add ("Bar Plot");
                typeCombo.nonActiveMessage = "Select type";
                typeCombo.WidthRequest = 400;
                typeCombo.maxListHeight = 3;
				typeCombo.ComboChangedEvent += OnTypeComboChanged;
                fix.Put (typeCombo, 195, 5);
                typeCombo.Show ();

                ExposeEvent += (obj, args) => {
                    typeCombo.Visible = false;
                    typeCombo.Visible = true;
                };

                Add (fix);
                fix.Show ();
            }

			protected void OnTypeComboChanged (object sender, ComboBoxChangedEventArgs args) {
				switch (args.activeText) {
				case "Line Plot":
					nameTextBox.Visible = false;
                    moduleComboBox.Visible = true;
					groupComboBox.Visible = false;
                    nameComboBox.Visible = false;

					moduleComboBox.combo.comboList.Clear ();
					moduleComboBox.combo.comboList.AddRange (HomeWindowWidgets.linePlots.Keys);
					moduleComboBox.combo.activeIndex = -1;

					break;
				case "Bar Plot":
					nameTextBox.Visible = false;
                    moduleComboBox.Visible = true;
					groupComboBox.Visible = false;
                    nameComboBox.Visible = false;

					moduleComboBox.combo.comboList.Clear ();
                    moduleComboBox.combo.comboList.AddRange (HomeWindowWidgets.barPlots.Keys);
                    moduleComboBox.combo.activeIndex = -1;

					break;
				case "Curved Bar Plot":
                    nameTextBox.Visible = false;
					moduleComboBox.Visible = false;
					groupComboBox.Visible = false;
					nameComboBox.Visible = true;

					nameComboBox.combo.comboList.Clear ();
					nameComboBox.combo.comboList.AddRange (HomeWindowWidgets.curvedBarPlots.Keys);
					moduleComboBox.combo.activeIndex = -1;

                    break;
				default:
					nameTextBox.Visible = true;
                    moduleComboBox.Visible = false;
                    groupComboBox.Visible = false;
					nameComboBox.Visible = false;
					break;
				}
			}

			protected void OnModuleComboChanged (object sender, ComboBoxChangedEventArgs args) {
				if (args.activeIndex != -1) {
                    if (args.activeText == "Temperature") {
						groupComboBox.Visible = true;
						groupComboBox.combo.comboList.Clear ();
						groupComboBox.combo.comboList.AddRange (Temperature.GetAllTemperatureGroupNames ());
						groupComboBox.combo.activeIndex = -1;
					} else if (args.activeText == "Water Level") {
						groupComboBox.Visible = true;
						groupComboBox.combo.comboList.Clear ();
						groupComboBox.combo.comboList.AddRange (WaterLevel.GetAllWaterLevelGroupNames ());
						groupComboBox.combo.activeIndex = -1;
					} else {
						groupComboBox.Visible = false;
					}
				}
			}

            protected void OnAddButtonReleased (object sender, ButtonReleaseEventArgs args) {
                if (typeCombo.activeIndex == -1) {
                    MessageBox.Show ("Please select a widget type");
                    return;
                }

				string name, group;

                var type = Regex.Replace (typeCombo.activeText, @"\s+", "");
				switch (type) {
                case "LinePlot":
				case "BarPlot":
					if (moduleComboBox.combo.activeIndex < 0) {
                        MessageBox.Show ("Please select a module");
                        return;
                    }
					name = (string)moduleComboBox.setting;

					if (groupComboBox.combo.activeIndex < 0) 
					if (groupComboBox.Visible) {
                        MessageBox.Show ("Please select a group");
                        return;
                    }
					group = (string)groupComboBox.setting;

                    break;
                case "CurvedBarPlot":
                    if (nameComboBox.combo.activeIndex < 0) {
                        MessageBox.Show ("Please select a name");
                        return;
                    }
					name = (string)nameComboBox.setting;
					group = string.Empty;
                    break;
                default:
					name = nameTextBox.textBox.text;
					group = string.Empty;
                    break;
                }

                newWidget = new HomeSettingsWidget (name, group, type, 0, 0);

                Destroy ();
            }


        }
    }
}

