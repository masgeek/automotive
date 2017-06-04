using Android.App;
using Android.Graphics;

namespace liquidtorque.ComponentClasses
{
    public class FontClass
    {

        public static Typeface LightTypeface
        {
            get { return SetFontTypeLight(); }
        }

        public static Typeface BoldTypeface
        {
            get { return SetFontTypeBold(); }
        }

        private static Typeface SetFontTypeLight(string path = "fonts/bgothl.ttf")
        {
            Typeface tf = Typeface.CreateFromAsset(Application.Context.Assets, path);
            return tf;
        }

        private static Typeface SetFontTypeBold(string path = "fonts/bgothm.ttf")
        {
            Typeface tf = Typeface.CreateFromAsset(Application.Context.Assets, path);
            return tf;
        }
    }
}