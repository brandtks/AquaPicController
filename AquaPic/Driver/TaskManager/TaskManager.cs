using System;
using Gtk;
using AquaPic.AlarmDriver;
using AquaPic.AnalogInputDriver;
using AquaPic.LightingDriver;
using AquaPic.PowerDriver;
using AquaPic.TemperatureDriver;

namespace AquaPic.TaskManagerDriver
{
    public class TaskManager
    {
        //public TaskManager () {
        //}

        static uint timer250;
        static uint timer1000;

        public static void Start () {
            timer250 = GLib.Timeout.Add (250, On250Tasks);
            timer1000 = GLib.Timeout.Add (1000, On1000Tasks);
        }

        protected static bool On250Tasks () {
            AnalogInput.Run ();
            Power.Run ();
            Temperature.Run ();
            return true; // restarts timer
        }

        protected static bool On1000Tasks () {
            Alarm.Run ();
            Lighting.Run ();
            return true; // restarts timer
        }
    }
}

