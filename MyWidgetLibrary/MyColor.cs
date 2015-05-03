using System;
using System.Collections.Generic;
using Cairo;

namespace MyWidgetLibrary
{
    public class MyColor
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        private float storedR;
        private float storedG;
        private float storedB;
        private float storedA;

        private string colorName;
        private string storedColorName;

        private static Dictionary<string, float[]> colorLookup = new Dictionary<string, float[]> () {
            { "red", new float [3] { 1.0f, 0.0f, 0.0f} },
            { "green", new float [3] { 0.0f, 1.0f, 0.0f} },
            { "blue", new float [3] { 0.0f, 0.0f, 1.0f} },
            { "yellow", new float [3] { 1.0f, 1.0f, 0.0f} },
            { "grey", new float [3] { 0.5f, 0.5f, 0.5f} },
            { "dgrey", new float [3] { 0.15f, 0.15f, 0.15f} },
            { "lgrey", new float [3] { 0.85f, 0.85f, 0.85f} },
            { "black", new float [3] { 0.0f, 0.0f, 0.0f} },
            { "white", new float [3] { 1.0f, 1.0f, 1.0f} },

            { "pri", new float [3] { 0.40392f, 0.8902f, 0f} }, // greenish
            { "seca", new float [3] { 0.00784f, 0.55686f, 0.60784f} }, // blueish
            { "secc", new float [3] { 1f, 0.99216f, 0.25098f} }, // yellowish
            { "secb", new float [3] { 1f, 0.60392f, 0.25098f} }, // orangish
            { "compl", new float [3] { 0.89412f, 0f, 0.27059f} }, // pinkish
            { "grey0", new float [3] { 0.15f, 0.15f, 0.15f} },
            { "grey1", new float [3] { 0.35f, 0.35f, 0.35f} },
            { "grey2", new float [3] { 0.50f, 0.50f, 0.50f} },
            { "grey3", new float [3] { 0.70f, 0.70f, 0.70f} },
            { "grey4", new float [3] { 0.85f, 0.85f, 0.85f} }
        };

        public MyColor (string color, double A = 1.0) {
            try {
                colorName = color.ToLower ();
                this.R = colorLookup [colorName] [0];
                this.G = colorLookup [colorName] [1];
                this.B = colorLookup [colorName] [2];
            } catch {
                colorName = string.Empty;
                this.R = 0.0f;
                this.G = 0.0f;
                this.B = 0.0f;
            }

            this.A = (float)A;

            storedR = R;
            storedG = G;
            storedB = B;
            storedA = this.A;
            storedColorName = colorName;
        }

        public MyColor (double R, double G, double B, double A = 1.0) {
            colorName = string.Empty;
            this.R = (float)R;
            this.G = (float)G;
            this.B = (float)B;
            this.A = (float)A;
            storedR = this.R;
            storedG = this.G;
            storedB = this.B;
            storedA = this.A;
            storedColorName = colorName;
        }

        public MyColor (byte R, byte G, byte B, double A = 1.0) {
            colorName = string.Empty;
            this.R = (float)(R/255);
            this.G = (float)(G/255);
            this.B = (float)(B/255);
            this.A = (float)A;
            storedR = this.R;
            storedG = this.G;
            storedB = this.B;
            storedA = this.A;
            storedColorName = colorName;
        }

        public void ChangeColor (string color, double a = 1.0) {
            storedR = R;
            storedG = G;
            storedB = B;
            storedColorName = colorName;

            try {
                colorName = color.ToLower ();
                R = colorLookup [colorName] [0];
                G = colorLookup [colorName] [1];
                B = colorLookup [colorName] [2];
            } catch {
                colorName = storedColorName;
                R = storedR;
                G = storedG;
                B = storedB;
            }
                
            A = (float)a;
        }

        public void ChangeColor (double r, double g, double b, double a = 1.0) {
            storedR = R;
            storedG = G;
            storedB = B;

            this.R = (float)r;
            this.G = (float)g;
            this.B = (float)b;
            this.A = (float)a;
        }

        public void ModifyAlpha (double a) {
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

        public string ToHTML () {
            byte red = (byte)(R * 255);
            byte green = (byte)(G * 255);
            byte blue = (byte)(B * 255);
            string html = string.Format ("#{0:X2}{1:X2}{2:X2}", red, green, blue);
            return html;
        }

        public static string ToHTML (string color) {
            byte r, g, b;

            try {
                r = (byte)(colorLookup [color] [0] * 255);
                g = (byte)(colorLookup [color] [1] * 255);
                b = (byte)(colorLookup [color] [2] * 255);
            } catch {
                r = 0;
                g = 0;
                b = 0;
            }

            string html = string.Format ("#{0:X2}{1:X2}{2:X2}", r, g, b);
            return html;
        }

        public void SetSource (Context cr) {
            cr.SetSourceRGBA (R, G, B, A);
        }

        public static void SetSource (Context cr, string color, double a = 1.0) {
            double r, g, b;

            try {
                r = colorLookup [color] [0];
                g = colorLookup [color] [1];
                b = colorLookup [color] [2];
            } catch {
                r = 0.0;
                g = 0.0;
                b = 0.0;
            }

            cr.SetSourceRGBA (r, g, b, a);
        }

        public static Color NewColor (string color, double a = 1.0) {
            try {
                double r = colorLookup [color] [0];
                double g = colorLookup [color] [1];
                double b = colorLookup [color] [2];
                return new Color (r, g, b, a);
            } catch {
                return new Color (0.0, 0.0, 0.0);
            }
        }

        public Color ToColor () {
            return new Color (R, G, B, A);
        }
    }
}

