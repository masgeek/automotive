using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using HockeyApp;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using liquidtorque.OffLineData;
using WizarDroid.NET;
using WizarDroid.NET.Persistence;
using Exception = Java.Lang.Exception;

namespace liquidtorque.Wizards.SellMyCarWizard
{
    public class CarInfoFragment : WizardStep
    {
        [WizardState] public Car CarProfile;
        [WizardState] public List<Bitmap> CarsList;

        private DataManager _dataManager;
        //Spinners
        private Spinner _makesSpinner;
        private Spinner _modelSpinner;
        private Spinner _yearSpinner;
        private Spinner _distanceTypeSpinner;
        private Spinner _engineTypeSpinner;
        private Spinner _driveTrainSpinner;
        private Spinner _transmissionSpinner;
        private Spinner _carTypeSpinner;
        private Spinner _intColorSpinner;
        private Spinner _extColorSpinner;

        //checkboxes
        private CheckBox _vatCheckBox;
        private CheckBox _porCheckBox;

        //edit texts
        private EditText _txtModelVariation;
        private EditText _txtMileage;
        private EditText _txtVin;
        private EditText _txtStockNo;
        private EditText _txtListPrice;
        private EditText _txtDescription;
        private int _vehicleMileage;
        //string for holding the variables than change dynamically
        private string _selectedMake;
        private string _selectedModel;
        private string _selectedDistanceUnit;
        private string _selectedEngine;
        private string _selectedDriveTrain;
        private string _selectedTransmission;
        private string _selectedCarType;
        private string _selectedExtColor;
        private string _selectedIntColor;
        private string _selectedYear;

        //Adapters
        private ArrayAdapter<string> _modelAdapter;

        //boolean validator
        bool _isPriceOnRequest;
        private bool _hasVat;
        bool _isDataValid;

        public CarInfoFragment()
        {
            StepExited += OnStepExited;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _dataManager = DataManager.GetInstance();
            SqlLiteDataStore.GetInstance();
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            var makes = _dataManager.GetVehicleMake();

            //create adapters for the spinners
            var yearAdapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                HelperClass.GenerateYearRange());
            var makesAdapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                makes);


            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.car_listing, container, false);

            //attach the its to the various elements in the layout

            //Spinners
            _makesSpinner = view.FindViewById<Spinner>(Resource.Id.make_spinner);
            _modelSpinner = view.FindViewById<Spinner>(Resource.Id.model_spinner);
            _yearSpinner = view.FindViewById<Spinner>(Resource.Id.year_spinner);
            _distanceTypeSpinner = view.FindViewById<Spinner>(Resource.Id.distance_spinner);
            _engineTypeSpinner = view.FindViewById<Spinner>(Resource.Id.engine_spinner);
            _driveTrainSpinner = view.FindViewById<Spinner>(Resource.Id.drivetrain_spinner);
            _transmissionSpinner = view.FindViewById<Spinner>(Resource.Id.transmission_spinner);
            _carTypeSpinner = view.FindViewById<Spinner>(Resource.Id.cartype_spinner);
            _intColorSpinner = view.FindViewById<Spinner>(Resource.Id.intcolor_spinner);
            _extColorSpinner = view.FindViewById<Spinner>(Resource.Id.extclor_spinner);

            //checkboxes
            _vatCheckBox = view.FindViewById<CheckBox>(Resource.Id.chkVat);
            _porCheckBox = view.FindViewById<CheckBox>(Resource.Id.chkPor);

            //edit texts
            _txtModelVariation = view.FindViewById<EditText>(Resource.Id.txtModelVariation);
            _txtMileage = view.FindViewById<EditText>(Resource.Id.txtMileage);
            _txtVin = view.FindViewById<EditText>(Resource.Id.txtVin);
            _txtStockNo = view.FindViewById<EditText>(Resource.Id.txtStockNo);
            _txtListPrice = view.FindViewById<EditText>(Resource.Id.txtListPrice);
            _txtDescription = view.FindViewById<EditText>(Resource.Id.txtDescription);

            //attach data source to the relevant spinners
            _yearSpinner.Adapter = yearAdapter;
            _makesSpinner.Adapter = makesAdapter;
            //modelSpinner.Adapter = makesAdapter;
            //populate the spinners accordingly

            //attach actions to the relevant elements

            //spinner actions
            _makesSpinner.ItemSelected += VehicleMakeSelected;
            _modelSpinner.ItemSelected += VehicleModelSelected;
            _yearSpinner.ItemSelected += YearSelected;
            _distanceTypeSpinner.ItemSelected += DistanceTypeSelected;
            _engineTypeSpinner.ItemSelected += EngineTypeSelected;
            _driveTrainSpinner.ItemSelected += DriveTrainSelected;
            _transmissionSpinner.ItemSelected += TransmissionTypeSelected;
            _carTypeSpinner.ItemSelected += CarTypeSelected;
            _intColorSpinner.ItemSelected += IntColorSelected;
            _extColorSpinner.ItemSelected += ExtColorSelected;

            //checkboxes
            _porCheckBox.CheckedChange += PriceOnRequestChecked;
            _vatCheckBox.CheckedChange += VatChecked;
            //attach validation delegates accordinlgy

            //txtModelVariation.AfterTextChanged += delegate { ValidateCarInfromation(); };
            _txtMileage.AfterTextChanged += delegate { ValidateCarInfromation(); };
            _txtVin.AfterTextChanged += delegate { ValidateCarInfromation(); };
            _txtStockNo.AfterTextChanged += delegate { ValidateCarInfromation(); };
            _txtListPrice.AfterTextChanged += delegate { ValidateCarInfromation(); };
            _txtDescription.AfterTextChanged += delegate { ValidateCarInfromation(); };

            return view;
        }

        

        private void VatChecked(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            _hasVat = e.IsChecked;
        }

        private void PriceOnRequestChecked(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                _isPriceOnRequest = e.IsChecked;
                _txtListPrice.Text = null;
                _txtListPrice.Enabled = false;
            }
            else
            { 
                _isPriceOnRequest = false;
                _txtListPrice.Enabled = true;
                
            }

            ValidateCarInfromation();
        }
        
        #region "Spinner selection"

        private void VehicleMakeSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //fetch the model based on the seleted make
            try
            {
                List<string> filteredModels = new List<string> {"Select Model"};
                if (e.Position > 0)
                {


                    var selectedItem = _makesSpinner.GetItemAtPosition(e.Position);


                    _selectedMake = selectedItem.ToString();
                    if (!string.IsNullOrEmpty(_selectedMake))
                    {
                        //fetch the models under the selected make
                        List<Tuple<string, string>> models = _dataManager.GetVehicleModels(_selectedMake);

                        foreach (var model in models)
                        {
                            filteredModels.Add(model.Item1);
                        }


                    }
                }
                _modelAdapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                    filteredModels);

                _modelSpinner.Adapter = _modelAdapter;
            }
            catch (Exception ex)
            {
                Android.Util.Log.Info(string.Format(string.Format("Error getting model {0}", ex.Message)), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting model {0}{1}", ex.Message, ex.StackTrace));
            }

        }

        private void VehicleModelSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0) return;
                var selectedItem = _modelSpinner.GetItemAtPosition(e.Position);
                _selectedModel = selectedItem.ToString();
                _isDataValid = !string.IsNullOrEmpty(_selectedModel);
            }
            catch (Exception ex)
            {
                var message = string.Format("Error getting make {0}{1}", ex.Message, ex.StackTrace);
                Android.Util.Log.Info(string.Format("Error getting make {0}", ex.Message), ex.StackTrace);
                MetricsManager.TrackEvent(message);
                
            }
        }

        private void ExtColorSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0) return;
                var selectedItem = _extColorSpinner.GetItemAtPosition(e.Position);
                _selectedExtColor = selectedItem.ToString();
                _isDataValid = !string.IsNullOrEmpty(_selectedExtColor);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Info(string.Format("Error getting exterior color {0}", ex.Message), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting exterior color {0}{1}", ex.Message, ex.StackTrace));
            }
        }

        private void IntColorSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0) return;
                var selectedItem = _intColorSpinner.GetItemAtPosition(e.Position);
                _selectedIntColor = selectedItem.ToString();
                _isDataValid = !string.IsNullOrEmpty(_selectedIntColor);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Info(string.Format("Error getting interior color {0}", ex.Message), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting interior color {0}{1}", ex.Message, ex.StackTrace));
            }
        }

        private void CarTypeSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0) return;
                var selectedItem = _carTypeSpinner.GetItemAtPosition(e.Position);
                _selectedCarType = selectedItem.ToString();
                _isDataValid = !string.IsNullOrEmpty(_selectedCarType);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Info(string.Format(string.Format("Error getting car type {0}", ex.Message)), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting car type {0}{1}", ex.Message, ex.StackTrace));
            }
        }

        private void TransmissionTypeSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                var selectedItem = _transmissionSpinner.GetItemAtPosition(e.Position);
                _selectedTransmission = selectedItem.ToString();
                _isDataValid = !string.IsNullOrEmpty(_selectedTransmission);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Info(string.Format(string.Format("Error getting tranmission {0}", ex.Message)), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting transmission {0}{1}", ex.Message, ex.StackTrace));
            }
        }

        private void DriveTrainSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0) return;
                var selectedItem = _driveTrainSpinner.GetItemAtPosition(e.Position);
                _selectedDriveTrain = selectedItem.ToString();
                _isDataValid = !string.IsNullOrEmpty(_selectedDriveTrain);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Info(string.Format("Error getting drivetrain {0}", ex.Message), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting drivetrain {0}{1}", ex.Message, ex.StackTrace));
            }
        }

        private void EngineTypeSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0) return;
                var selectedItem = _engineTypeSpinner.GetItemAtPosition(e.Position);
                _selectedEngine = selectedItem.ToString();
                _isDataValid = !string.IsNullOrEmpty(_selectedEngine);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Info(string.Format("Error getting engine type {0}", ex.Message), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting engine type {0}{1}", ex.Message, ex.StackTrace));
            }
        }

        private void DistanceTypeSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0) return;
                var selectedItem = _distanceTypeSpinner.GetItemAtPosition(e.Position);
                _selectedDistanceUnit = selectedItem.ToString();
                _isDataValid = !string.IsNullOrEmpty(_selectedDistanceUnit);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Info(string.Format("Error getting distance type {0}", ex.Message), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting distance type {0}{1}", ex.Message, ex.StackTrace));
            }
        }

        private void YearSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0) return;
                var selectedItem = _yearSpinner.GetItemAtPosition(e.Position);
                _selectedYear = selectedItem.ToString();
                _isDataValid = !string.IsNullOrEmpty(_selectedYear);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Info(string.Format(string.Format("Error getting year {0}", ex.Message)), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting year {0}{1}", ex.Message, ex.StackTrace));
            }
        }

        #endregion
        /// <summary>
        /// This function will validate the data provided by the users
        /// we can only proceed to the next level after all car info is correct and veriied
        /// </summary>
        private void ValidateCarInfromation()
        {
            _isDataValid = true; //always reset to false

            //validate the spinners here
            if (string.IsNullOrEmpty(_selectedMake))
            {
                _isDataValid = false;
            }
            //End of spinner validation


            if (string.IsNullOrWhiteSpace(_txtMileage.Text))
            {

                _txtMileage.Error = "Please enter a valid mileage";
                _isDataValid = false;
            }
            else
            {
                //validate if mileage is number
                var isNumeric = int.TryParse(_txtMileage.Text, out _vehicleMileage);

                if (!isNumeric)
                {
                    _isDataValid = false;
                    _txtMileage.Error = "Mileage should be a number";
                }
                else
                {
                    _txtMileage.Error = null;
                }
            }

            if (string.IsNullOrWhiteSpace(_txtVin.Text) || _txtVin.Text.Length < 1)
            {
                _txtVin.Error = "Specify a vheicle VIN";
                _isDataValid = false;
            }
            else
            {
                _txtVin.Error = null;
            }


            if (string.IsNullOrWhiteSpace(_txtStockNo.Text) || _txtStockNo.Text.Length < 1)
            {
                _txtStockNo.Error = "Specify a valid stock number";
                _isDataValid = false;
            }
            else
            {
                _txtStockNo.Error = null;
            }

            if (string.IsNullOrWhiteSpace(_txtModelVariation.Text) || _txtModelVariation.Text.Length < 1)
            {
                _txtModelVariation.Error = "Specify a model variaition";
                _isDataValid = false;
            }
            else
            {
                _txtModelVariation.Error = null;
            }

            if (!_isPriceOnRequest) //if its not price on request
            {
                if (string.IsNullOrWhiteSpace(_txtListPrice.Text))
                {
                    _txtListPrice.Error = "Please provide a list price";
                    _isDataValid = false;
                }
                else
                {
                    float parsedListPrice;
                    var isFloat = float.TryParse(_txtListPrice.Text, out parsedListPrice);

                    if (!isFloat)
                    {
                        _isDataValid = false;
                        _txtListPrice.Error = "Please provide a valid list price";
                    }
                    else
                    {
                        _txtListPrice.Error = null;
                    }
                }
            }

            //verify validation flag
            if (_isDataValid)
            {
                NotifyCompleted(); // All the input is valid.. Set the step as completed
            }
            else
            {
                NotifyIncomplete(); //Input is invalid do not proceed to next step
            }
        }


        //after all validation when next  button is pressed
        private void OnStepExited(StepExitCode exitCode)
        {
            if (exitCode == StepExitCode.ExitPrevious) return;


            if (CarProfile == null) { CarProfile = new Car();}
            //assign the car values now
            CarProfile.mileage = _vehicleMileage;
            CarProfile.milesData = _selectedDistanceUnit;
            CarProfile.model = _selectedModel;
            CarProfile.make = _selectedMake;
            CarProfile.driveTrain = _selectedDriveTrain;
            CarProfile.engine = _selectedEngine;
            CarProfile.transmission = _selectedTransmission;
            CarProfile.carType = _selectedCarType;
            CarProfile.interiorColor = _selectedIntColor;
            CarProfile.exteriorColor = _selectedExtColor;
            CarProfile.year = _selectedYear;
            CarProfile.listPrice = _txtListPrice.Text;
            CarProfile.vin = _txtVin.Text;
            CarProfile.stockNumber = _txtStockNo.Text;
            CarProfile.priceOnRequest = _isPriceOnRequest;
            CarProfile.vat = _hasVat;
            CarProfile.description = _txtDescription.Text;

        }
    }
}