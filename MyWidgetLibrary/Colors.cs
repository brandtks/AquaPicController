using System;
using System.Collections.Generic;
using Cairo;

namespace MyWidgetLibrary
{
    public class Colors
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        private float storedR;
        private float storedG;
        private float storedB;
        private float storedA;

        private static Dictionary<string, float[]> colorLookup = new Dictionary<string, float[]> () {
            { "red", new float [3] { 1.0f, 0.0f, 0.0f} },
            { "green", new float [3] { 0.0f, 1.0f, 0.0f} },
            { "blue", new float [3] { 0.0f, 0.0f, 1.0f} },
            { "yellow", new float [3] { 1.0f, 1.0f, 0.0f} },
            { "grey", new float [3] { 0.5f, 0.5f, 0.5f} },
            { "dark gray", new float [3] { 0.2f, 0.2f, 0.2f} },
            { "light gray", new float [3] { 0.75f, 0.75f, 0.75f} },
            { "black", new float [3] { 0.0f, 0.0f, 0.0f} },
            { "white", new float [3] { 1.0f, 1.0f, 1.0f} }
        };

        public Colors (string color, double A = 1.0) {
            try {
                string c = color.ToLower ();
                this.R = colorLookup [c] [0];
                this.G = colorLookup [c] [1];
                this.B = colorLookup [c] [2];
            } catch {
                this.R = 0.0f;
                this.G = 0.0f;
                this.B = 0.0f;
            }

            this.A = (float)A;
        }

        public Colors (double R, double G, double B, double A = 1.0) {
            this.R = (float)R;
            this.G = (float)G;
            this.B = (float)B;
            this.A = (float)A;
        }

        public void ChangeColor(string color, double a = 1.0) {
            storedR = R;
            storedG = G;
            storedB = B;

            try {
                string c = color.ToLower ();
                R = colorLookup [c] [0];
                G = colorLookup [c] [1];
                B = colorLookup [c] [2];
            } catch {
                R = storedR;
                G = storedG;
                B = storedB;
            }
                
            A = (float)a;
        }

        public void SetTemporaryAlpha (double a) {
            storedA = A;
            A = (float)a;
        }

        public void RestoreAlpha () {
            A = storedA;
        }

        public void ModifyColor(double ratio) {
            storedR = R;
            storedG = G;
            storedB = B;

            R *= (float)ratio;
            if (R > 1.0f)
                R = 1.0f;
            G *= (float)ratio;
            if (G > 1.0f)
                G = 1.0f;
            B *= (float)ratio;
            if (B > 1.0f)
                B = 1.0f;
        }

        public void RestoreColor () {
            R = storedR;
            G = storedG;
            B = storedB;
        }
    }
}

