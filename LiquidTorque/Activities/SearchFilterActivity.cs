using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using HockeyApp;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;

namespace liquidtorque.Activities
{
    [Activity(Label = "Search Vehicles")]
    public class SearchFilterActivity : BaseActivity
    {
        DataManager _dataManager = DataManager.GetInstance();

        private ArrayAdapter<string> _modelAdapter;
        private ArrayAdapter<string> _yearAdapter;
        private ArrayAdapter<string> _makesAdapter;

        private string _selectedMake;
        private string _selectedYear;
        private string _typedModel;

        private AutoCompleteTextView _modelSearch;
        private Spinner _makeSpinner;
        private Spinner _yearSpinner;

        private Button _cancelButton;
        private Button _resetButton;
        private Button _applyButton;

        protected override int LayoutResource
        {
            get { return Resource.Layout.search_filter; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            try
            {
                // Create your application here
                _modelSearch = FindViewById<AutoCompleteTextView>(Resource.Id.modelSearchText);
                _makeSpinner = FindViewById<Spinner>(Resource.Id.makeSpinner);
                _yearSpinner = FindViewById<Spinner>(Resource.Id.yearSpinner);

                _cancelButton = FindViewById<Button>(Resource.Id.cancelFilter);
                _resetButton = FindViewById<Button>(Resource.Id.resetFilter);
                _applyButton = FindViewById<Button>(Resource.Id.applyFilter);

                //Set the data source

                var makes = _dataManager.GetVehicleMake(isFilterScenario:true);
                var models = LoadModels();

                //create adapters for the spinners
                _yearAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                    HelperClass.GenerateYearRange(isFilterScenario:true));
                _makesAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                    makes);
                _modelAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                    models);

                //set the spinnder data 
                _makeSpinner.Adapter = _makesAdapter;
                _yearSpinner.Adapter = _yearAdapter;
                _modelSearch.Adapter = _modelAdapter;


                //set the click actions
                _cancelButton.Click += delegate { Finish(); /* End the filter activity */ };
                _resetButton.Click += delegate { ResetFilter(); /* clear the search values */ };
                _applyButton.Click += delegate { ApplyFilters(); };
                //set the spinner selection actions
                _makeSpinner.ItemSelected += MakeSelected;
                _yearSpinner.ItemSelected += YearSelected;

                //set autocomplete text actions
                _modelSearch.AfterTextChanged += ModelSearch_AfterTextChanged;
                _modelSearch.Click += ModelSearch_Click; ;
            }
            catch (Exception ex)
            {
                var message = "Error creating filter activity" + ex.StackTrace + ex.Message;
                Android.Util.Log.Info(message, ex.StackTrace);
                MetricsManager.TrackEvent(message);
            }
        }

        private void ModelSearch_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            _typedModel = _modelSearch.Text;//set the typed text
        }

        private void ApplyFilters()
        {
            //set the filters in the filter class

            VehicleFilterStrings.FiltersSet = true;
            VehicleFilterStrings.Make = _selectedMake;
            VehicleFilterStrings.Model = _typedModel;
            VehicleFilterStrings.Year = _selectedYear;


            Intent intent = new Intent();
            intent.PutExtra("model", _typedModel);
            intent.PutExtra("make", _selectedMake);
            intent.PutExtra("year", _selectedYear);
            //return this data to previous activity
            SetResult(Result.Ok,intent);

            //end the activity
            Finish();
        }

        private void ModelSearch_Click(object sender, EventArgs e)
        {
            _modelSearch.ShowDropDown(); //show the dropdown always
        }

        #region Spinner selections

        private void YearSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0) return;
                var selectedItem = _yearSpinner.GetItemAtPosition(e.Position);
                _selectedYear = selectedItem.ToString();
            }
            catch (Exception ex)
            {
                var message = "Error loading vehicle model years" + ex.StackTrace + ex.Message;
                Android.Util.Log.Info(message, ex.StackTrace);
                MetricsManager.TrackEvent(message);
            }
        }

        private void MakeSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //fetch the model based on the seleted make
            try
            {
                if (e.Position <= 0) return;
                //clear the text in the search
                _modelSearch.Text = null;
                var selectedItem = _makeSpinner.GetItemAtPosition(e.Position);

                _selectedMake = selectedItem.ToString();
                if (!string.IsNullOrEmpty(_selectedMake))
                {
                    //fetch the models under the selected make
          

                    _modelAdapter = new ArrayAdapter<string>(this,
                        Android.Resource.Layout.SimpleSpinnerDropDownItem, LoadModels(_selectedMake));
                    _modelSearch.Adapter = _modelAdapter;

                    //acticate the dropdown of the search
                    _modelSearch.ShowDropDown(); //show the dropdown always

                }
            }
            catch (Exception ex)
            {
                var message = "Error loading vehicle makes" + ex.StackTrace + ex.Message;
                Android.Util.Log.Info(message, ex.StackTrace);
                MetricsManager.TrackEvent(message);
            }
        }

        /// <summary>
        /// Load all models or filter by a specific make
        /// </summary>
        /// <param name="modelFilter"></param>
        /// <returns></returns>
        List<string> LoadModels(string modelFilter = null)
        {
            List<string> filteredModels = new List<string>();
            List<Tuple<string, string>> models = _dataManager.GetVehicleModels(_selectedMake);

            filteredModels.Add("All");
            foreach (var model in models)
            {
                filteredModels.Add(model.Item1);
            }

            return filteredModels; 
        }
        #endregion Spinner selections end
        /// <summary>
        /// Rest the filter values
        /// </summary>
        private void ResetFilter()
        {
            //clear the values and set them back to default;
            try
            {
                _selectedMake = null;
                _selectedYear = null;
                _typedModel = null;
                _modelSearch.Text = null;
            }
            catch (Exception ex)
            {
                var message = "Error loading vehicle profile" + ex.StackTrace + ex.Message;
                MetricsManager.TrackEvent(message);
            }
        }
    }
}