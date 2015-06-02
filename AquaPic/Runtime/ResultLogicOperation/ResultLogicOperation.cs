using System;

namespace AquaPic
{
    public class ResultLogicOperation
    {
        private bool _result;
        public bool result {
            get { return _result; }
        }

        public ResultLogicOperation () {
            _result = true;
        }

        public void And (bool condition) {

        }
    }
}

