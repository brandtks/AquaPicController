using System;
using AquaPic.Utilites;

namespace AquaPic.Runtime
{
    public partial class Bit
    {
        private class BitState
        {
            public MyState state;

            public BitState () {
                state = MyState.Off;
            }

            public BitState (MyState initState) {
                state = initState;
            }
        }
    }
}

