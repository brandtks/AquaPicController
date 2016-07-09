using System;
using System.Collections.Generic;
using Cairo;

namespace TouchWidgetLibrary
{
    public class TouchColor
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

        private static Dictionary<string, float[]> colorLookup = new Dictionary<string, float[]> () {
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

        public TouchColor (string color, double A = 1.0) {
            bool colorFound;

            try {
                colorName = color.ToLower ();
                R = colorLookup [colorName] [0];
                G = colorLookup [colorName] [1];
                B = colorLookup [colorName] [2];
                colorFound = true;
            } catch {
                colorFound = false;
            }

            if (!colorFound) {
                Gdk.Color c = new Gdk.Color ();
                colorFound = Gdk.Color.Parse (color, ref c);
                if (colorFound) {
                    R = (float)(c.Red / 65535);
                    G = (float)(c.Green / 65535);
                    B = (float)(c.Blue / 65535);
                } else
                    throw new Exception ("No color could be found matching that description");
            }

            this.A = (float)A;

            storedR = R;
            storedG = G;
            storedB = B;
            storedA = this.A;
        }

        public TouchColor (double R, double G, double B, double A = 1.0) {
            colorName = string.Empty;
            this.R = (float)R;
            this.G = (float)G;
            this.B = (float)B;
            this.A = (float)A;
            storedR = this.R;
            storedG = this.G;
            storedB = this.B;
            storedA = this.A;
        }

        public TouchColor (byte R, byte G, byte B, double A = 1.0) {
            colorName = string.Empty;
            this.R = (float)(R/255);
            this.G = (float)(G/255);
            this.B = (float)(B/255);
            this.A = (float)A;
            storedR = this.R;
            storedG = this.G;
            storedB = this.B;
            storedA = this.A;
        }

        public static implicit operator TouchColor (string name) {
            return new TouchColor (name);
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

        public TouchColor Blend (TouchColor otherColor, float amount) {
            float red = (R * (1 - amount)) + (otherColor.R * amount);
            float green = (G * (1 - amount)) + (otherColor.G * amount);
            float blue = (B * (1 - amount)) + (otherColor.B * amount);
            return new TouchColor (red, green, blue);
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
                TouchColor c = new TouchColor (color);
                r = (byte)(c.R * 255);
                g = (byte)(c.G * 255);
                b = (byte)(c.B * 255);
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
            TouchColor c;

            try {
                c = new TouchColor (color);
                cr.SetSourceRGBA (c.R, c.G, c.B, a);
            } catch {
                cr.SetSourceRGBA (0.0, 0.0, 0.0, a);
            }
        }

        public static Color NewCairoColor (string color, double a = 1.0) {
            TouchColor c;

            try {
                c = new TouchColor (color);
                return new Color (c.R, c.G, c.B, a);
            } catch {
                return new Color (0.0, 0.0, 0.0);
            }
        }

        public Color NewCairoColor () {
            return new Color (R, G, B, A);
        }

        public static Gdk.Color NewGtkColor (string color) {
            TouchColor c;

            try {
                c = new TouchColor (color);
                return new Gdk.Color ((byte)(c.R * 255), (byte)(c.G * 255), (byte)(c.B * 255));
            } catch {
                return new Gdk.Color ();
            }
        }
    }
}

