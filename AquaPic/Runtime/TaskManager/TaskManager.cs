using System;
using Gtk;
using AquaPic.AlarmRuntime;
using AquaPic.AnalogInputDriver;
using AquaPic.AnalogOutputDriver;
using AquaPic.DigitalInputDriver;
using AquaPic.LightingModule;
using AquaPic.PluginRuntime;
using AquaPic.PowerDriver;
using AquaPic.TemperatureModule;

namespace AquaPic.TaskManagerRuntime
{
    public class TaskManager
    {
        //public TaskManager () {
        //}

        static uint timer250;
        static uint timer1000;

        public static void Start () {
            #if SIMULATION
            timer250 = GLib.Timeout.Add (500, On250Tasks);
            timer1000 = GLib.Timeout.Add (4000, On1000Tasks);
            #else
            timer250 = GLib.Timeout.Add (250, On250Tasks);
            timer1000 = GLib.Timeout.Add (1000, On1000Tasks);
            #endif
        }

        protected static bool On250Tasks () {
            Power.Run ();
            return true; // restarts timer
        }

        protected static bool On1000Tasks () {
            AnalogInput.Run ();
            DigitalInput.Run ();
            AnalogOutput.Run ();

            Temperature.Run ();
            Alarm.Run ();
            Plugin.Run ();
            return true; // restarts timer
        }
    }
}

