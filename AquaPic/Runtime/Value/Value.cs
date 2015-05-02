using System;

namespace AquaPic.ValueRuntime
{
    public delegate float ValueGetterHandler ();
    public delegate void ValueSetterHandler (float value);

    public class Value
    {
        public ValueGetterHandler ValueGetter;
        public ValueSetterHandler ValueSetter;

        public Value () {
        }

        public void Execute () {
            float value = 0.0f;

            if (ValueGetter != null)
                value = ValueGetter ();

            if (ValueSetter != null)
                ValueSetter (value);
        }
    }
}

