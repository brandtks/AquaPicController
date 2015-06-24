using System;
using Gtk;
using Cairo;
using MyWidgetLibrary;

namespace AquaPic
{
    public class AnalogChannelDisplay : Fixed
    {
        public TouchLabel label;
        public TouchTextBox textBox;
        public TouchProgressBar progressBar;
        public TouchLabel typeLabel;

        public float currentValue {
            set {
                int v = Convert.ToInt32 (value);
                textBox.text = v.ToString ("D");

                progressBar.currentProgress = value / 1024.0f;
            }
        }

        public AnalogChannelDisplay () {
            SetSizeRequest (760, 50);

            label = new TouchLabel ();
            Put (label, 5, 0);
            label.Show ();

            textBox = new TouchTextBox ();
            textBox.WidthRequest = 200; 
            Put (textBox, 0, 20);
            textBox.Show ();

            progressBar = new TouchProgressBar (MyOrientation.Horizontal);
            progressBar.WidthRequest = 540;
            Put (progressBar, 220, 20);
            progressBar.Show ();

            typeLabel = new TouchLabel ();
            typeLabel.Visible = false;
            typeLabel.WidthRequest = 200;
            typeLabel.textAlignment = MyAlignment.Right;
            Put (typeLabel, 550, 0);

            Show ();
        }
    }
}

