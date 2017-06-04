using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using liquidtorque.ComponentClasses;
using liquidtorque.Listeners;
using UniversalImageLoader.Core;
using UniversalImageLoader.Core.Listener;
using Object = Java.Lang.Object;

namespace liquidtorque.Adapters
{
    public class ImageAdapter : BaseAdapter
    {
        private readonly List<Tuple<string, string, string, string, Uri, string>> _carList;
        //get the list of vehilces

        private readonly LayoutInflater _inflater;
        private readonly IImageLoadingListener _animateFirstListener = new AnimateFirstDisplayListener();

        public ImageAdapter(Context context, List<Tuple<string, string, string, string, Uri, string>> cars)
        {
            _inflater = LayoutInflater.From(context);
            _carList = cars;
        }

        public override int Count
        {
            get { return _carList.Count; }
        }

        public override Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            View view = convertView;
            ViewHolder holder;
            if (convertView == null)
            {
                view = _inflater.Inflate(Resource.Layout.list_item_row, parent, false);
                holder = new ViewHolder
                {
                    mainText = view.FindViewById<TextView>(Resource.Id.Make),
                    subText = view.FindViewById<TextView>(Resource.Id.Model),
                    subtext2 = view.FindViewById<TextView>(Resource.Id.Price),
                    image = view.FindViewById<ImageView>(Resource.Id.Thumbnail),
                    progressBar = view.FindViewById<ProgressBar>(Resource.Id.loading)
                };
                view.Tag = holder;
            }
            else
            {
                holder = (ViewHolder) view.Tag;
            }

            var item = _carList.ElementAt(position);

            var makeName = !string.IsNullOrEmpty(item.Item2) ? item.Item2 : "N/A";
            var price = !string.IsNullOrEmpty(item.Item3) ? item.Item3 : "POR";
            var modelName = !string.IsNullOrEmpty(item.Item4) ? item.Item4 : "N/A";

            var imageBaseUrl = item.Item5;
            holder.mainText.Text = makeName;
            holder.subText.Text = modelName;
            holder.subtext2.Text = price;

            if (imageBaseUrl != null)
            {
                var url = imageBaseUrl.AbsoluteUri;
                /*ImageLoader.Instance.DisplayImage(url, holder.image, HelperClass.ImageDownloaderOptions,
                    _animateFirstListener);*/
                try
                {
                    ImageLoader.Instance.DisplayImage(
                        url,
                        holder.image,
                        HelperClass.ImageDownloaderOptions,
                        new ImageLoadingListener(
                            loadingStarted: delegate
                            {
                                holder.progressBar.Progress = 0;
                                holder.progressBar.Visibility = ViewStates.Visible;
                            },
                            loadingComplete: delegate
                            {
                                holder.progressBar.Visibility = ViewStates.Gone;
                            },
                            loadingFailed: delegate
                            {
                                holder.progressBar.Visibility = ViewStates.Gone;
                            }),
                        new ImageLoadingProgressListener(
                            progressUpdate: (imageUri, _view, current, total) =>
                            {
                                holder.progressBar.Progress = (int) (100.0f*current/total);
                            }));
                }
                catch (Exception ex)
                {
                    ImageLoader.Instance.ClearMemoryCache();
                }
            }
            return view;
        }
    }
}