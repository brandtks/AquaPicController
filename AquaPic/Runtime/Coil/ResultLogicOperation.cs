using System;
using AquaPic.TimerRuntime;

namespace AquaPic.CoilRuntime
{
    public class ResultLogicOperation
    {
        private bool _result;
        public bool result {
            get { return _result; }
        }

        private Timer t;
        private bool timerSet;

        public ResultLogicOperation () {
            _result = true;
        }

        public void Start () {
            _result = true;
        }

        public void And (bool condition) {
            _result &= condition;
        }

        public void OnDelay (uint timeDelay, bool condition) {
            if (t == null) {// t hasn't been initialized
                t = new Timer ();
                t.autoReset = false;
                t.TimerElapsedEvent += (sender, args) => {
                    timerSet = true;
                };
            }

            if (!t.enabled && !timerSet && _result) {
                t.timerInterval = timeDelay;
                t.autoReset = false;

            }
        }
    }
}

