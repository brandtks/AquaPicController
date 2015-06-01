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

        public static implicit operator MyText (string name) {
            return new MyText (name);
        }

//        public void Render (Context cr, int x, int y, int width, int height) {
//            cr.SelectFontFace("Courier New", FontSlant.Normal, FontWeight.Normal);
//            cr.SetFontSize (font.size * 1.4);
//
//            TextExtents te = cr.TextExtents (text);
//            FontExtents fe = cr.FontExtents;
//
//            y += ((int)fe.Height - (int)fe.Descent); 
//            if (alignment == MyAlignment.Right)
//                x += (width - (int)te.Width);
//            else if (alignment == MyAlignment.Center)
//                x += ((width / 2) - ((int)te.Width / 2));
//
//            string t;
//            if (te.Width > width) {
//                double difference = te.Width - (double)width;
//                int glyphs = (int)Math.Floor ((difference / fe.MaxXAdvance) + 0.5);
//                t = text.Substring (0, text.Length - glyphs);
//                t = string.Format ("{0}...", t);
//            } else
//                t = text;
//            
//            font.color.SetSource (cr);
//            if (orientation == MyOrientation.Horizontal) {
//                cr.MoveTo (x, y);
//                cr.ShowText (t);
//            } else { // orientation is vertical
//                for (int i = 0; i < t.Length; ++i) {
//                    int y2 = y + ((int)fe.Height * i);
//                    cr.MoveTo (x, y2);
//                    cr.ShowText (string.Format("{0}", t [i]));
//                }
//            }
//        }

        public void Render (object sender, int x, int y, int width) {
            Bin widget = sender as Bin;
            Pango.Layout l = new Pango.Layout (widget.PangoContext);

            l.FontDescription = Pango.FontDescription.FromString (font.fontName + " " + font.size.ToString ());
            l.SetMarkup ("<span color=\"" + font.color.ToHTML () + "\">" + text + "</span>"); 

            if (orientation == MyOrientation.Horizontal) {
                //l.SetMarkup ("<span color=\"" + font.color.ToHTML () + "\">" + text + "</span>"); 

                if (alignment == MyAlignment.Left)
                    l.Alignment = Pango.Alignment.Left;
                else if (alignment == MyAlignment.Right)
                    l.Alignment = Pango.Alignment.Right;
                else // center
                    l.Alignment = Pango.Alignment.Center;

                l.Wrap = Pango.WrapMode.Word;
                l.Width = Pango.Units.FromPixels (width);

                if ((l.LineCount > 1) && (textWrap == MyTextWrap.None)) {
                    Pango.LayoutLine[] ll = l.Lines;
                    string firstLine = text.Substring (0, ll [1].StartIndex - 1);
                    int lastSpace = firstLine.LastIndexOf (' ');
                    if (lastSpace != -1)
                        firstLine = firstLine.Substring (0, lastSpace);
                    l.SetText (firstLine + "...");
                }
            } else {
                //<TODO> cheesy work around to get somewhat vertical text. Gravity attribute does not seem to work
                l.Wrap = Pango.WrapMode.Char;
                l.Width = Pango.Units.FromPixels (5);
                l.Spacing = Pango.Units.FromPixels (0);
                //l.SetMarkup ("<span gravity=\"east\" color=\"" + font.color.ToHTML () + "\">" + text + "</span>");
            }

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
            color = "black";
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
        WordWrap
    }
}

