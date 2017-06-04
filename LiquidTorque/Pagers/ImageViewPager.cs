using System;
using Android.Content;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using HockeyApp;

namespace liquidtorque.Pagers
{
    /// <summary>
    /// This will implement the swipe action for the view page
    /// to allow swiping across several image
    /// </summary>
    class ImageViewPager : ViewPager
    {
        public bool IsLocked { get; set; }

        #region required constructors

        public ImageViewPager(Context context)
            : base(context)
        {
            IsLocked = false;
        }

        public ImageViewPager(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            IsLocked = false;
        }

        #endregion End of mandatory constructors

        #region Time for my code now...awesome

        /// <summary>
        /// Override the touch event
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            try
            {
                return !IsLocked && base.OnInterceptTouchEvent(ev);
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("A general exception has occurred {0} {1}", ex.Message, ex.StackTrace));
                MetricsManager.TrackEvent(String.Format("A general exception has occurred {0} {1}", ex.Message,
                    ex.StackTrace));
            }

            return false;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            try
            {
                return !IsLocked && base.OnTouchEvent(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("A general exception has occurred {0} {1}", ex.Message, ex.StackTrace));
                MetricsManager.TrackEvent(String.Format("A general exception has occurred {0} {1}", ex.Message,
                    ex.StackTrace));
            }

            return false;
        }

        public void ToggleLock()
        {
            //toggle the lock state of the view page
            IsLocked = !IsLocked; //will always be teh inverse...opposite day nayo
        }

        #endregion End of my custom code :-(
    }
}