using System;
using Gtk;
using Cairo;

namespace MyWidgetLibrary
{
    public class MyText
    {
        public MyFont font;
        public MyAlignment alignment;
        public MyOrientation orientation;
        public MyTextWrap textWrap;
        public string text;

        public MyText (string text) {
            this.text = text;
            font = new MyFont ();
            alignment = MyAlignment.Left;
            orientation = MyOrientation.Horizontal;
            textWrap = MyTextWrap.WordWrap;
        }

        public MyText () : this (string.Empty) { }

        public static implicit operator MyText (string name) {
            return new MyText (name);
        }

        public void Render (object sender, int x, int y, int width) {
            Render (sender, x, y, width, -1);
        }

        public void Render (object sender, int x, int y, int width, int height) {
            Bin widget = sender as Bin;
            Pango.Layout l = new Pango.Layout (widget.PangoContext);

            l.FontDescription = Pango.FontDescription.FromString (font.fontName + " " + font.size.ToString ());
            l.SetMarkup ("<span color=\"" + font.color.ToHTML () + "\">" + text + "</span>"); 

//            if (orientation == MyOrientation.Horizontal) {
////                l.SetMarkup ("<span color=\"" + font.color.ToHTML () + "\">" + text + "</span>"); 

            if (textWrap == MyTextWrap.WordWrap) {
                l.Wrap = Pango.WrapMode.Word;
                l.Width = Pango.Units.FromPixels (width);
            }

            if (alignment == MyAlignment.Left)
                l.Alignment = Pango.Alignment.Left;
            else if (alignment == MyAlignment.Right)
                l.Alignment = Pango.Alignment.Right;
            else // center
                l.Alignment = Pango.Alignment.Center;

            string displayedText = text;
            if ((l.LineCount > 1) && (textWrap == MyTextWrap.None)) {
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
                if (textWrap == MyTextWrap.None) {
                    while (w > width) {
                        displayedText = displayedText.Remove (displayedText.Length - 1);
                        l.SetText (displayedText);
                        l.GetPixelSize (out w, out h);
                    }
                } else if (textWrap == MyTextWrap.Shrink) {
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

            if (height != -1) {
                y = (y + (height / 2)) - (h / 2);
            }

//            } else {
//                //<TODO> cheesy work around to get somewhat vertical text. Gravity attribute does not seem to work
//                //l.SetMarkup ("<span gravity=\"east\" color=\"" + font.color.ToHTML () + "\">" + text + "</span>");
////                l.Wrap = Pango.WrapMode.Char;
////                l.Width = Pango.Units.FromPixels (5);
////                l.Spacing = Pango.Units.FromPixels (0);
//            }

            widget.GdkWindow.DrawLayout (widget.Style.TextGC (StateType.Normal), x, y, l);
            l.Dispose ();
        }
    }

    public class MyFont
    {
        public MyColor color;
        public int size;
        public string fontName;

        public MyFont () {
            color = "white";
            size = 11;
            fontName = "Courier New";
        }
    }

    public enum MyAlignment : byte {
        Right = 1,
        Left,
        Center
    }

    public enum MyTextWrap : byte {
        None = 1,
        WordWrap,
        Shrink
    }
}

