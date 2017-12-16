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
using System.Diagnostics;
using Cairo;
using Gtk;
using GoodtimeDevelopment.TouchWidget;

namespace AquaPic.UserInterface
{
    public class SettingsWidget : Fixed
    {
        public bool optionalSetting;
        public TouchLabel label;

        public virtual string text {
            get {
                return label.text;
            }
            set {
                label.text = value;
            }
        }

        public SettingsWidget () {
            SetSizeRequest (290, 30);

            optionalSetting = false;

            label = new TouchLabel ();
            label = new TouchLabel ();
            label.SetSizeRequest (115,30);
            label.textAlignment = TouchAlignment.Right;
            label.textColor = "white";
            label.textRender.textWrap = TouchTextWrap.Shrink;
            label.textHorizontallyCentered = true;
            label.text = "Error: No Title";
            Put (label, 0, 0);
            label.Show ();

            Show ();
        }
    }

    public class SettingsTextBox : SettingsWidget
    {
        public TouchTextBox textBox;

        public override string text {
            get {
                return label.text;
            }
            set {
                textBox.name = value;
                label.text = value;
            }
        }
        
        public SettingsTextBox () : base () {
            textBox = new TouchTextBox ();
            textBox.SetSizeRequest (170, 30);
            textBox.enableTouch = true;
            Put (textBox, 120, 0);
            textBox.Show ();
        }
    }

    public class SettingsComboBox : SettingsWidget
    {
        public TouchComboBox combo;

        public SettingsComboBox () : base () {
            combo = new TouchComboBox ();
            combo.SetSizeRequest (170, 30);
            combo.ButtonPressEvent += OnComboButtonPressed;
            Put (combo, 120, 0);
            combo.Show ();
        }

        protected void OnComboButtonPressed (object sender, ButtonPressEventArgs args) {
            Fixed p = Parent as Fixed;
            if (p != null) {
                int x = Allocation.Left;
                int y = Allocation.Top;
                p.Remove (this);
                p.Put (this, x, y);
            }
        }
    }

    public class SettingSelectorSwitch : SettingsWidget
    {
        public TouchSelectorSwitch selectorSwitch;
        public string[] labels;

        public SettingSelectorSwitch (string label1, string label2) : base () {
            selectorSwitch = new TouchSelectorSwitch (2);
            selectorSwitch.currentSelected = 0;
            selectorSwitch.sliderSize = MySliderSize.Large;
            selectorSwitch.sliderColorOptions [0] = "pri";
            selectorSwitch.sliderColorOptions [1] = "grey2";

            selectorSwitch.SetSizeRequest (170, 30);
            selectorSwitch.ExposeEvent += OnExpose;
            Put (selectorSwitch, 120, 0);
            selectorSwitch.Show ();

            labels = new string[2];
            labels [0] = label1;
            labels [1] = label2;
        }

        public SettingSelectorSwitch () : this ("True", "False") { }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            TouchSelectorSwitch ss = sender as TouchSelectorSwitch;
            int seperation = ss.Allocation.Width / ss.selectionCount;
            int x = ss.Allocation.Left;

            TouchText render = new TouchText ();
            render.textWrap = TouchTextWrap.Shrink;
            render.alignment = TouchAlignment.Center;
            render.font.color = "white";

            foreach (var l in labels) {
                render.text = l;
                render.Render (ss, x, ss.Allocation.Top, seperation, ss.Allocation.Height);
                x += seperation;
            }
        }
    }
}

