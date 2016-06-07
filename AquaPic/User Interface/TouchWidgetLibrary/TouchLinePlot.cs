using System;
using Cairo;
using Gtk;
using AquaPic.Runtime;

namespace TouchWidgetLibrary
{
    public class TouchLinePlot : EventBox
    {
        DataLogger logger;

        public TouchLinePlot () {
            Visible = true;
            VisibleWindow = false;

            SetSizeRequest (296, 76);

            ExposeEvent += OnExpose;
            ButtonReleaseEvent += OnButtonRelease;
        }

        protected void OnExpose (object sender, ExposeEventArgs args) {

        }

        protected void OnButtonRelease (object sender, ButtonReleaseEventArgs args) {

        }

        public void LinkDataLogger (DataLogger logger) {
            this.logger = logger;
            this.logger.DataLogEntryAddedEvent += OnDataLogEntryAdded;
        }

        protected void OnDataLogEntryAdded (object sender, DataLogEntryAddedEventArgs args) {

        }
    }
}

