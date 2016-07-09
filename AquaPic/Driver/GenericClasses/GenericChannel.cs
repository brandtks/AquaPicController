using System;
using AquaPic.Utilites;

namespace AquaPic.Drivers
{
    public class GenericChannel<T>
    {
        public string name;
        public T value;
        public Mode mode;

        protected GenericChannel (string name, T value = default(T)) {
            this.name = name;
            this.value = value;
            mode = Mode.Auto;
        }

        public virtual void SetValue (object value) {
            try {
                this.value = (T)Convert.ChangeType (value, typeof(T));
            } catch {
                this.value = default(T);
            }
        }
    }
}

