using System;
using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Widget;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using liquidtorque.OffLineData;
using DataAccessLayer;
using HockeyApp;
using AndroidHUD;

namespace liquidtorque.Activities
{
    [Activity(Label = "New Request")]
    public class NewRequestActivity : BaseActivity
    {
        private string _selectedMake;
        private string _selectedModel;
        private string _selectedYear;
        private string _username;
        private Spinner _make;
        private Spinner _model;
        private Spinner _year;
        private Button _saverequest;
        private Button _cancelrequest;

        private DataManager _dataManager;
        private readonly List<string> _filteredModels = new List<string> { "Select Model" };
        private ArrayAdapter<string> _makesAdapter;
        private ArrayAdapter<string> _yearAdapter;
        private ArrayAdapter<string> _modelsAdapter;

        protected override int LayoutResource
        {
            get { return Resource.Layout.newrequest; }
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _dataManager = DataManager.GetInstance();
            SqlLiteDataStore.GetInstance();
            _username = Intent.GetStringExtra("username");
            var makes = _dataManager.GetVehicleMake();

            //create adapters for the spinners
            _yearAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                HelperClass.ListYears);
             _makesAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                makes);
            _modelsAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                _filteredModels);
            // Create your application here
            // btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
            _make = FindViewById<Spinner>(Resource.Id.makeSpinner);
            _model = FindViewById<Spinner>(Resource.Id.modelSpinner);
            _year = FindViewById<Spinner>(Resource.Id.yearSpinner);

            _saverequest = FindViewById<Button>(Resource.Id.saveRequestButton);
            _cancelrequest = FindViewById<Button>(Resource.Id.cancelRequestButton);

            //attach data source to the relevant spinners
            _year.Adapter = _yearAdapter;
            _make.Adapter = _makesAdapter;
            _model.Adapter = _modelsAdapter;

            // Button click activity
            _saverequest.Click += SaveRequestClick;
            _cancelrequest.Click += CancelRequestClick;

            //spinner action
            _year.ItemSelected += YearItemSelected;
            _model.ItemSelected += ModelItemSelected;
            _make.ItemSelected += MakeItemSelected;
        }

        private void CancelRequestClick(object sender, EventArgs e)
        {
            Finish(); //end the activity and return to previous window
        }

        private async void SaveRequestClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedMake)|| string.IsNullOrEmpty(_selectedModel)||string.IsNullOrEmpty(_selectedYear))
                {
                    Toast.MakeText(this, string.Format("Please select all values"), ToastLength.Short).Show();
                }
                else
                {
                    UserRequest request = new UserRequest
                    {
                        make = _selectedMake,
                        model = _selectedModel,
                        year = _selectedYear,
                        userName = _username
                    };

                    //show the HUD here
                    AndHUD.Shared.Show(this, "Saving your request...");
                    await _dataManager.insertUserRequest(request);
                    Toast.MakeText(this, string.Format("Request saved successfully"), ToastLength.Short).Show();
                    //dismiss the hud after getting a response
                    AndHUD.Shared.Dismiss(this);
                    Finish(); //end the activity
                }
            }

            catch (Exception ex)
            {
                var message = "Error saving car request " + ex.Message + ex.StackTrace;
                Toast.MakeText(this, string.Format("Request not saved, please try again"), ToastLength.Short).Show();
                MetricsManager.TrackEvent(message);
                Console.WriteLine(message);
                AndHUD.Shared.Dismiss(this);
            }
        }

        private void MakeItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0)
                {
                    _selectedMake = null;
                    return;
                }

                if(_filteredModels.Count > 0) { _filteredModels.Clear();}

                object selecteditem = _make.GetItemAtPosition(e.Position);
                _selectedMake = selecteditem.ToString();
                _filteredModels.Add("Select Model");
                if (!string.IsNullOrEmpty(_selectedMake))
                {
                    var modellist = _dataManager.GetVehicleModels(_selectedMake);
                    foreach (var modelName in modellist)
                    {
                        _filteredModels.Add(modelName.Item1);
                    }
                    _modelsAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                        _filteredModels);
                    _model.Adapter = _modelsAdapter;

                }
            }
            catch (Exception ex)
            {

                var message = "Error selecting vehicle make " + ex.Message + ex.StackTrace;
                MetricsManager.TrackEvent(message);
                Console.WriteLine(message);
            }
        }

        private void ModelItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0)
                {
                    _selectedModel = null;
                    return;
                }
                object selecteditem = _model.GetItemAtPosition(e.Position);
                _selectedModel = selecteditem.ToString();
            }
            catch (Exception ex)
            {
                var message = "Error selecting vehicle model " + ex.Message + ex.StackTrace;
                MetricsManager.TrackEvent(message);
                Console.WriteLine(message);

            }
        }

        private void YearItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0)
                {
                    _selectedYear = null;
                    return;
                }
                object selecteditem = _year.GetItemAtPosition(e.Position);
                _selectedYear = selecteditem.ToString();
            }
            catch (Exception ex)
            {

                var message = "Error selecting vehicle year " + ex.Message + ex.StackTrace;
                MetricsManager.TrackEvent(message);
                Console.WriteLine(message);

            }
        }
    }
}