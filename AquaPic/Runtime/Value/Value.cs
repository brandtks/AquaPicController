using System;

namespace AquaPic.Runtime
{
    public delegate float ValueGetterHandler ();
    public delegate void ValueSetterHandler (float value);

    public class Value
    {
        public ValueGetterHandler ValueGetter;
        public ValueSetterHandler ValueSetter;
        public float value;

        public void Execute () {
            float newValue = 0.0f;

            if (ValueGetter != null)
                newValue = ValueGetter ();

            if (value != newValue) {
                value = newValue;

                if (ValueSetter != null)
                    ValueSetter (value);
            }
        }
    }
}

