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
using Gtk;
using Cairo;
using GoodtimeDevelopment.TouchWidget;

namespace AquaPic.UserInterface
{
    public class SensorsWindow : SceneBase
    {
        AnalogSensorWidget topWidget;
        AnalogSensorWidget bottomWidget;
        TouchComboBox topCombo;
        TouchComboBox bottomCombo;

        readonly string[] analogSensorNames = { "Water Level Sensor", "Temperature Probe", "pH Probe" };

        public SensorsWindow (params object[] options) {
            sceneTitle = "Sensors";
            ExposeEvent += OnExpose;

            topWidget = new PhProbeWidget ();
            Put (topWidget, 415, 77);
            topWidget.Show ();

            bottomWidget = new WaterLevelSensorWidget ();
            Put (bottomWidget, 415, 277);
            bottomWidget.Show ();

            topCombo = new TouchComboBox (analogSensorNames);
            topCombo.WidthRequest = 235;
            topCombo.activeText = analogSensorNames[2];
            topCombo.ComboChangedEvent += OnComboChange;
            Put (topCombo, 153, 77);
            topCombo.Show ();

            bottomCombo = new TouchComboBox (analogSensorNames);
            bottomCombo.WidthRequest = 235;
            bottomCombo.activeText = analogSensorNames[0];
            bottomCombo.ComboChangedEvent += OnComboChange;
            Put (bottomCombo, 153, 277);
            bottomCombo.Show ();

            topWidget.GetSensorData ();
            bottomWidget.GetSensorData ();

            Show ();
        }

        protected void OnComboChange (object sender, ComboBoxChangedEventArgs args) {
            if (topCombo.Equals (sender)) {
                topWidget.Destroy ();
                topWidget = AnalogSensorWidgetCreater (args.activeText);
                Put (topWidget, 415, 77);
                topWidget.Show ();
                topWidget.GetSensorData ();
            } else {
                bottomWidget.Destroy ();
                bottomWidget = AnalogSensorWidgetCreater (args.activeText);
                Put (bottomWidget, 415, 277);
                bottomWidget.Show ();
                bottomWidget.GetSensorData ();
            }
        }

        protected AnalogSensorWidget AnalogSensorWidgetCreater (string name) {
            AnalogSensorWidget widget = null;

            switch (name) {
            case "Water Level Sensor":
                widget = new WaterLevelSensorWidget ();
                break;
            case "Temperature Probe":
                widget = new TemperatureProbeWidget ();
                break;
            case "pH Probe":
                widget = new PhProbeWidget ();
                break;
            }

            return widget;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {
            using (Context cr = Gdk.CairoHelper.Create (GdkWindow)) {
                TouchColor.SetSource (cr, "grey3", 0.75);
                cr.LineWidth = 3;

                cr.MoveTo (402.5, 70);
                cr.LineTo (402.5, 252.5);
                cr.ClosePath ();
                cr.Stroke ();

                cr.MoveTo (402.5, 282.5);
                cr.LineTo (402.5, 460);
                cr.ClosePath ();
                cr.Stroke ();

                cr.MoveTo (40, 267.5);
                cr.LineTo (780, 267.5);
                cr.ClosePath ();
                cr.Stroke ();
            }
        }
    }
}
