using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using liquidtorque.Adapters;
using liquidtorque.DataAccessLayer;
using liquidtorque.Pagers;

namespace liquidtorque.Activities
{
    [Activity(Label = "View Images", Icon = "@drawable/icon", Theme = "@style/Theme.AppCompat")]
    public class ImagePagerActivity : AppCompatActivity
    {
        private static string _isLocked = "isLocked";
        private ImageViewPager _viewPager;
        private IMenuItem _menuLockItem;
        private string _imageObjectId;
        private string _vehicleObjectId;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Intent != null)
            {
                _imageObjectId = Intent.GetStringExtra("imageObjectId");
                _vehicleObjectId = Intent.GetStringExtra("vehicleObjectId");
            }

            var cars = await VehicleData.FetchAllCarImage(_vehicleObjectId);

            // Create your application here
            _viewPager = new ImageViewPager(this);
            SetContentView(_viewPager); //set content for the view
            _viewPager.SetBackgroundResource(Resource.Color.transparent);
            _viewPager.Adapter = new ImageViewPageAdapter(cars);

            if (savedInstanceState != null)
            {
                _viewPager.IsLocked = savedInstanceState.GetBoolean(_isLocked, false);
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            if (IsViewPagerActive())
            {
                outState.PutBoolean(_isLocked, _viewPager.IsLocked);
            }
            base.OnSaveInstanceState(outState);
        }

        private bool IsViewPagerActive()
        {
            return (_viewPager != null && _viewPager is ImageViewPager);
        }

        #region menu item stuff
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.image_pager_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            _menuLockItem = menu.FindItem(Resource.Id.menu_delete_image);

            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_delete_image)
            {
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
        #endregion
    }
}