using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;
using AquaPic.Modules;

namespace AquaPic
{
    public class TemperatureWindow : WindowBase
    {
        TouchComboBox combo;
        TouchLabelTextBox setpoint;
        TouchLabelTextBox deadband;
        TouchLabel heaterLabel;
        int heaterId;

        public TemperatureWindow (params object[] options) : base () {
            MyBox box1 = new MyBox (385, 395);
            Put (box1, 10, 30);
            box1.Show ();

            MyBox box2 = new MyBox (385, 395);
            Put (box2, 405, 30);
            box2.Show ();

            var label = new TouchLabel ();
            label.text = "General Temperature Information";
            label.WidthRequest = 370;
            label.textColor = "pri";
            label.textSize = 12;
            Put (label, 15, 40);
            label.Show ();

            var tempLabel = new TouchLabel ();
            tempLabel.text = "Temperature";
            tempLabel.textColor = "grey4"; 
            tempLabel.textSize = 14;
            tempLabel.WidthRequest = 170;
            tempLabel.render.alignment = MyAlignment.Right;
            Put (tempLabel, 15, 75);
            tempLabel.Show ();

            var temp = new TouchTextBox ();
            temp.WidthRequest = 200;
            temp.HeightRequest = 35;
            temp.textSize = 14;
            temp.text = Temperature.WaterTemperature.ToString ("F1");
            Put (temp, 190, 70);
            temp.Show ();

            var setpointlabel = new TouchLabel ();
            setpointlabel.text = "Setpoint";
            setpointlabel.textColor = "grey4"; 
            setpointlabel.WidthRequest = 170;
            setpointlabel.render.alignment = MyAlignment.Right;
            Put (setpointlabel, 15, 114);
            setpointlabel.Show ();

            var tempSetpoint = new TouchTextBox ();
            tempSetpoint.WidthRequest = 200;
            tempSetpoint.text = Temperature.temperatureSetpoint.ToString ("F1");
            Put (tempSetpoint, 190, 110);
            tempSetpoint.Show ();

            var tempDeadbandLabel = new TouchLabel ();
            tempDeadbandLabel.text = "Deadband";
            tempDeadbandLabel.textColor = "grey4";
            tempDeadbandLabel.WidthRequest = 170;
            tempDeadbandLabel.render.alignment = MyAlignment.Right;
            Put (tempDeadbandLabel, 15, 149);
            tempDeadbandLabel.Show ();

            var tempDeadband = new TouchTextBox ();
            tempDeadband.WidthRequest = 200;
            tempDeadband.text = (Temperature.temperatureDeadband * 2).ToString ("F1");
            Put (tempDeadband, 190, 145);
            tempDeadband.Show ();

            heaterId = 0;

            heaterLabel = new TouchLabel ();
            heaterLabel.textAlignment = MyAlignment.Center;
            heaterLabel.WidthRequest = 375;
            heaterLabel.textColor = "secb";
            Put (heaterLabel, 410, 77);
            heaterLabel.Show ();

            setpoint = new TouchLabelTextBox ();
            setpoint.label.text = "Setpoint";
            setpoint.textBox.enableTouch = false;
            Put (setpoint, 410, 100);

            deadband = new TouchLabelTextBox ();
            deadband.label.text = "Deadband";
            deadband.textBox.enableTouch = false;
            Put (deadband, 410, 135);

            var settingsBtn = new TouchButton ();
            settingsBtn.text = "Settings";
            settingsBtn.SetSizeRequest (100, 30);
            settingsBtn.ButtonReleaseEvent += (o, args) => {
                var s = new TemperatureSettings ();
                s.Run ();
                s.Destroy ();
                tempSetpoint.text = Temperature.temperatureSetpoint.ToString ("F1");
                tempDeadband.text = (Temperature.temperatureDeadband * 2).ToString ("F1");
            };
            Put (settingsBtn, 15, 390);
            settingsBtn.Show ();

            string[] names = Temperature.GetAllHeaterNames ();
            combo = new TouchComboBox (names);
            combo.Active = heaterId;
            combo.WidthRequest = 235;
            combo.ChangedEvent += OnComboChanged;
            Put (combo, 550, 35);
            combo.Show ();

            GetHeaterData ();

            Show ();
        }

        protected void OnComboChanged (object sender, ComboBoxChangedEventArgs e) {
            int id = Temperature.GetHeaterIndex (e.ActiveText);
            if (id != -1) {
                heaterId = id;
                GetHeaterData ();
            }
        }

        protected void GetHeaterData () {
            if (Temperature.ControlsTemperature (heaterId)) {
                heaterLabel.text = string.Format ("{0} control based upon global setpoints", Temperature.GetHeaterName (heaterId));
                setpoint.Visible = false;
                deadband.Visible = false;
            } else {
                heaterLabel.text = "Heater Control Setpoints";
                setpoint.Visible = true;
                deadband.Visible = true;

                setpoint.textBox.text = Temperature.GetHeaterSetpoint (heaterId).ToString ("F1");
                deadband.textBox.text = Temperature.GetHeaterDeadband (heaterId).ToString ("F1");
            }

            heaterLabel.QueueDraw ();
            setpoint.QueueDraw ();
            deadband.QueueDraw ();
        }
    }
}

