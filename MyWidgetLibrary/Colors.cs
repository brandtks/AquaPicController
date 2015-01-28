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

        public Colors (string color, bool transparent = false, float alphaLevel = 0.8f) {
            try {
                this.R = colorLookup [color] [0];
                this.G = colorLookup [color] [1];
                this.B = colorLookup [color] [2];
            } catch {
                this.R = 0.0f;
                this.G = 0.0f;
                this.B = 0.0f;
            }
            if (transparent)
                this.A = alphaLevel;
            else
                this.A = 1.0f;
        }

        public void changeColor(string color, bool transparent = false, float alphaLevel = 0.8f) {
            R = colorLookup [color] [0];
            G = colorLookup [color] [1];
            B = colorLookup [color] [2];
            if (transparent)
                A = alphaLevel;
            else
                A = 1.0f;
        }

        public void setTemporaryNewAlpha (float a) {
            storedA = A;
            A = a;
        }

        public void restoreAlpha () {
            A = storedA;
        }

        public void ModifyColor(float ratio) {
            storedR = R;
            storedG = G;
            storedB = B;

            R *= ratio;
            G *= ratio;
            B *= ratio;
        }

        public void restoreColor () {
            R = storedR;
            G = storedG;
            B = storedB;
        }
    }
}

