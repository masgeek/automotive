using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using UniversalImageLoader.Core.Display;
using UniversalImageLoader.Core.Listener;

namespace liquidtorque.Listeners
{
    public class AnimateFirstDisplayListener : SimpleImageLoadingListener
    {
        private static readonly List<string> DisplayedImages = new List<string>();

        public override void OnLoadingStarted(string imageUri, View view)
        {
            base.OnLoadingStarted(imageUri, view);
        }

        public override void OnLoadingComplete(string imageUri, View view, Bitmap loadedImage)
        {
            if (loadedImage != null)
            {
                lock (DisplayedImages)
                {
                    ImageView imageView = (ImageView)view;
                    bool firstDisplay = !DisplayedImages.Contains(imageUri);
                    if (firstDisplay)
                    {
                        FadeInBitmapDisplayer.Animate(imageView, 500);
                        DisplayedImages.Add(imageUri);
                    }
                }
            }
        }
    }
}