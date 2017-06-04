using Android.Support.V4.App;
using Java.Lang;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;


namespace liquidtorque.Adapters
{
    public class TabsFragmentPagerAdapter : FragmentPagerAdapter
    {
        private readonly Fragment[] _fragments;

        private readonly ICharSequence[] _titles;

        public TabsFragmentPagerAdapter(FragmentManager fm, Fragment[] fragments, ICharSequence[] titles) : base(fm)
        {
            this._fragments = fragments;
            this._titles = titles;
        }
        public override int Count
        {
            get
            {
                return _fragments.Length;
            }
        }

        public override Fragment GetItem(int position)
        {
            return _fragments[position];
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            return _titles[position];
        }
    }
}