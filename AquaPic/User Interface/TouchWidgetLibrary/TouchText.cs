using System;
using Gtk;
using Cairo;

namespace TouchWidgetLibrary
{
    public class TouchText
    {
        public MyFont font;
        public TouchAlignment alignment;
        public TouchOrientation orientation;
        public TouchTextWrap textWrap;
        public string text;

        public TouchText (string text) {
            this.text = text;
            font = new MyFont ();
            alignment = TouchAlignment.Left;
            orientation = TouchOrientation.Horizontal;
            textWrap = TouchTextWrap.WordWrap;
        }

        public TouchText () : this (string.Empty) { }

        public static implicit operator TouchText (string name) {
            return new TouchText (name);
        }

        public void Render (object sender, int x, int y, int width) {
            Render (sender, x, y, width, -1);
        }

        public void Render (object sender, int x, int y, int width, int height) {
            Bin widget = sender as Bin;
            Pango.Layout l = new Pango.Layout (widget.PangoContext);

            l.FontDescription = Pango.FontDescription.FromString (font.fontName + " " + font.size.ToString ());
            l.SetMarkup ("<span color=\"" + font.color.ToHTML () + "\">" + text + "</span>"); 

            if (orientation == TouchOrientation.Horizontal) {
//                l.SetMarkup ("<span color=\"" + font.color.ToHTML () + "\">" + text + "</span>"); 

                if (textWrap == TouchTextWrap.WordWrap) {
                    l.Wrap = Pango.WrapMode.Word;
                    l.Width = Pango.Units.FromPixels (width);
                }

                if (alignment == TouchAlignment.Left)
                    l.Alignment = Pango.Alignment.Left;
                else if (alignment == TouchAlignment.Right)
                    l.Alignment = Pango.Alignment.Right;
                else // center
                    l.Alignment = Pango.Alignment.Center;

                string displayedText = text;
                if ((l.LineCount > 1) && (textWrap == TouchTextWrap.None)) {
                    Pango.LayoutLine[] ll = l.Lines;
                    displayedText = text.Substring (0, ll [1].StartIndex - 1);
                    int lastSpace = displayedText.LastIndexOf (' ');
                    if (lastSpace != -1)
                        displayedText = displayedText.Substring (0, lastSpace);
                    displayedText = displayedText + "...";
                    l.SetText (displayedText);
                }

                int w, h;
                l.GetPixelSize (out w, out h);

                if (w > width) {
                    if (textWrap == TouchTextWrap.None) {
                        while (w > width) {
                            displayedText = displayedText.Remove (displayedText.Length - 1);
                            l.SetText (displayedText);
                            l.GetPixelSize (out w, out h);
                        }
                    } else if (textWrap == TouchTextWrap.Shrink) {
                        int K = l.FontDescription.Size / font.size;
                        int fs = font.size;
                        while ((w > width) && (fs > 0)) {
                            --fs;
                            l.FontDescription.Size = fs * K;
                            l.SetText (displayedText);
                            l.GetPixelSize (out w, out h);
                        }
                    }
                } else {
                    l.Width = Pango.Units.FromPixels (width);
                }

                if (height != -1)
                    y = (y + (height / 2)) - (h / 2);

            } else {
                var matrix = Pango.Matrix.Identity;
                matrix.Rotate(270);
                l.Context.Matrix = matrix;
            }

            widget.GdkWindow.DrawLayout (widget.Style.TextGC (StateType.Normal), x, y, l);
            l.Dispose ();
        }
    }

    public class MyFont
    {
        public TouchColor color;
        public int size;
        public string fontName;

        public MyFont () {
            color = "white";
            size = 11;
            fontName = "Sans";
        }
    }
}

