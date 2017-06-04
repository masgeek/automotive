using System.IO;
using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Widget;

namespace liquidtorque.Activities
{
    [Activity(Label = "Terms Of Service", NoHistory = true, AlwaysRetainTaskState = false)]
    public class TermsOfServiceActivity : BaseActivity
    {
        private string _filename = "tos_text.txt";
        private TextView _termsTextView;


        protected override int LayoutResource
        {
            get { return Resource.Layout.terms_text_layout; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _termsTextView = FindViewById<TextView>(Resource.Id.termsTextView);
            //read the text file
            AssetManager am = Assets;
            //open the text file
            var terms = am.Open(_filename);
            string content;
            //start the stream reader
            using (StreamReader sr = new StreamReader(terms))
            {
                content = sr.ReadToEnd();
            }
            // Set TextView.Text to our asset content
            _termsTextView.Text = content;
            _termsTextView.SetTypeface(Typeface.Monospace,TypefaceStyle.Normal);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            this.Finish(); //close the activity
        }
    }
}