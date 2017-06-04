using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Widget;
using AndroidHUD;
using HockeyApp;
using liquidtorque.ComponentClasses;
using liquidtorque.DataAccessLayer;
using liquidtorque.OffLineData;

namespace liquidtorque.Activities
{
    [Activity(Label = "Edit Vehicle Profile")]
    public class EditVehicleActivity : BaseActivity
    {
        private DataManager _dataManager = DataManager.GetInstance();
        private SqlLiteDataStore _offlineSqlLiteDataStore = SqlLiteDataStore.GetInstance();
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
        private EditText _editModelVariation;
        private EditText _editMileage;
        private EditText _editVin;
        private EditText _editStockNo;
        private EditText _editListPrice;
        private EditText _editDescription;
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
        private string _vehicleObjectId;

        //button
        private Button _btnUpdateVehicle;
        //boolean validator
        private bool _isPriceOnRequest;
        private bool _hasVat;
        private bool _isDataValid;

        //Adapters
        private ArrayAdapter<string> _yearAdapter;
        private ArrayAdapter<string> _makesAdapter;
        private ArrayAdapter<string> _modelAdapter;
        private ArrayAdapter _engineAdapter;
        private ArrayAdapter _driveTrainAdapter;
        private ArrayAdapter _transmissionAdapter;
        private ArrayAdapter _carTypeAdapter;
        private ArrayAdapter _colorAdapter;
        private ArrayAdapter _distanceAdapter;

        protected override int LayoutResource
        {
            get { return Resource.Layout.vehicle_details_edit; }
        }
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            try
            {

                AndHUD.Shared.Show(this, "Loading vehicle details...", -1, MaskType.Black);

                _vehicleObjectId = Intent.GetStringExtra("VehicleID") ?? "NA";

                //load spinner data
                var makes = _dataManager.GetVehicleMake();

                //create adapters for the spinners
               _yearAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                    HelperClass.GenerateYearRange());
               _makesAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                    makes);

                //ArrayAdapter<string> adapter = ArrayAdapter.createFromResource(this, R.array.select_state, android.R.layout.simple_spinner_item);
                _engineAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.engine, Android.Resource.Layout.SimpleSpinnerDropDownItem);
                 _driveTrainAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.drivetrain, Android.Resource.Layout.SimpleSpinnerDropDownItem);
                 _transmissionAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.transmission, Android.Resource.Layout.SimpleSpinnerDropDownItem);
                 _carTypeAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.car_type, Android.Resource.Layout.SimpleSpinnerDropDownItem);
                 _colorAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.car_color, Android.Resource.Layout.SimpleSpinnerDropDownItem);
                 _distanceAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.distance, Android.Resource.Layout.SimpleSpinnerDropDownItem);
                //Spinners
                _makesSpinner = FindViewById<Spinner>(Resource.Id.make_spinner);
                _modelSpinner =FindViewById<Spinner>(Resource.Id.model_spinner);
                _yearSpinner =FindViewById<Spinner>(Resource.Id.year_spinner);
                _distanceTypeSpinner =FindViewById<Spinner>(Resource.Id.distance_spinner);
                _engineTypeSpinner = FindViewById<Spinner>(Resource.Id.engine_spinner);
                _driveTrainSpinner = FindViewById<Spinner>(Resource.Id.drivetrain_spinner);
                _transmissionSpinner = FindViewById<Spinner>(Resource.Id.transmission_spinner);
                _carTypeSpinner = FindViewById<Spinner>(Resource.Id.cartype_spinner);
                _intColorSpinner = FindViewById<Spinner>(Resource.Id.intcolor_spinner);
                _extColorSpinner = FindViewById<Spinner>(Resource.Id.extclor_spinner);

                //checkboxes
                _vatCheckBox = FindViewById<CheckBox>(Resource.Id.chkVat);
                _porCheckBox = FindViewById<CheckBox>(Resource.Id.chkPor);

                //edit texts
                _editModelVariation = FindViewById<EditText>(Resource.Id.editModelVariation);
                _editMileage = FindViewById<EditText>(Resource.Id.editMileage);
                _editVin = FindViewById<EditText>(Resource.Id.editVin);
                _editStockNo = FindViewById<EditText>(Resource.Id.editStockNo);
                _editListPrice = FindViewById<EditText>(Resource.Id.editListPrice);
                _editDescription = FindViewById<EditText>(Resource.Id.editDescription);

                //buttons
                _btnUpdateVehicle = FindViewById<Button>(Resource.Id.btnUpdateVehicle);
                //attach data source to the relevant spinners
                _yearSpinner.Adapter = _yearAdapter;
                _makesSpinner.Adapter = _makesAdapter;
                _distanceTypeSpinner.Adapter = _distanceAdapter;
                _engineTypeSpinner.Adapter = _engineAdapter;
                _driveTrainSpinner.Adapter = _driveTrainAdapter;
                _transmissionSpinner.Adapter = _transmissionAdapter;
                _carTypeSpinner.Adapter = _carTypeAdapter;
                _intColorSpinner.Adapter = _colorAdapter;
                _extColorSpinner.Adapter = _colorAdapter;
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

                //editModelVariation.AfterTextChanged += delegate { ValidateCarInfromation(); };
                _editMileage.AfterTextChanged += delegate { ValidateCarInfromation(); };
                _editVin.AfterTextChanged += delegate { ValidateCarInfromation(); };
                _editStockNo.AfterTextChanged += delegate { ValidateCarInfromation(); };
                _editListPrice.AfterTextChanged += delegate { ValidateCarInfromation(); };
                _editDescription.AfterTextChanged += delegate { ValidateCarInfromation(); };
                
                //set spinner item
                //mySpinner.setSelection(arrayAdapter.getPosition("Category 2"));

                _btnUpdateVehicle.Click += BtnUpdateVehicleProfile;
                var resp = await PopulateFields();
            }
            catch (Exception ex)
            {
                var message = "Error loading car editing view " + ex.Message + ex.StackTrace;
                MetricsManager.TrackEvent(message);
                Console.WriteLine(message);
                AndHUD.Shared.Dismiss(this);
            }
        }


        #region Checkboxes region
        private void VatChecked(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            _hasVat = e.IsChecked;
        }

        private void PriceOnRequestChecked(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                _isPriceOnRequest = e.IsChecked;
                _editListPrice.Text = null;
                _editListPrice.Enabled = false;
            }
            else
            {
                _isPriceOnRequest = false;
                _editListPrice.Enabled = true;
                
            }
            ValidateCarInfromation();
        }
        #endregion
        #region "Spinner selection"
        private void VehicleMakeSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //fetch the model based on the seleted make
            try
            {
                List<string> filteredMake = new List<string>() { "Select Make" };
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
                            filteredMake.Add(model.Item1);
                        }

                    }
                }
                _modelAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, filteredMake);

                _modelSpinner.Adapter = _modelAdapter;
            }
            catch (Exception ex)
            {
                Android.Util.Log.Info(String.Format(string.Format("Error getting make {0}", ex.Message)), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting make {0} {1}", ex.Message, ex.StackTrace));
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
                Android.Util.Log.Info(String.Format("Error getting make {0}", ex.Message), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting make {0}{1}", ex.Message, ex.StackTrace));
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
                Android.Util.Log.Info(String.Format("Error getting exterior color {0}", ex.Message), ex.StackTrace);
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
                Android.Util.Log.Info(String.Format("Error getting interior color {0}", ex.Message), ex.StackTrace);
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
                Android.Util.Log.Info(String.Format(string.Format("Error getting car type {0}", ex.Message)), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting car type {0}{1}", ex.Message, ex.StackTrace));
            }
        }

        private void TransmissionTypeSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (e.Position <= 0) return;
                var selectedItem = _transmissionSpinner.GetItemAtPosition(e.Position);
                _selectedTransmission = selectedItem.ToString();
                _isDataValid = !string.IsNullOrEmpty(_selectedTransmission);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Info(String.Format(string.Format("Error getting tranmission {0}", ex.Message)), ex.StackTrace);
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
                Android.Util.Log.Info(String.Format("Error getting drivetrain {0}", ex.Message), ex.StackTrace);
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
                Android.Util.Log.Info(String.Format(string.Format("Error getting year {0}", ex.Message)), ex.StackTrace);
                MetricsManager.TrackEvent(string.Format("Error getting year {0}{1}", ex.Message, ex.StackTrace));
            }
        }

        #endregion

        /// <summary>
        /// This function will validate the data provided by the users
        /// we can only proceed to the next level after all car info is correct and verified
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

            /*if (string.IsNullOrWhiteSpace(editModelVariation.Text) || editModelVariation.Text.Length < 1)
            {
                editModelVariation.Error = "Specify a model variation";
                _isDataValid = false;
            }
            else
            {
                editModelVariation.Error = null;
            }*/


            if (string.IsNullOrWhiteSpace(_editMileage.Text))
            {

                _editMileage.Error = "Please enter a valid mileage";
                _isDataValid = false;
            }
            else
            {
                //validate if mileage is number
                var isNumeric = int.TryParse(_editMileage.Text, out _vehicleMileage);

                if (!isNumeric)
                {
                    _isDataValid = false;
                    _editMileage.Error = "Mileage should be a number";
                }
                else
                {
                    _editMileage.Error = null;
                }
            }

            if (string.IsNullOrWhiteSpace(_editVin.Text) || _editVin.Text.Length < 1)
            {
                _editVin.Error = "Specify a vheicle VIN";
                _isDataValid = false;
            }
            else
            {
                _editVin.Error = null;
            }


            if (string.IsNullOrWhiteSpace(_editStockNo.Text) || _editStockNo.Text.Length < 1)
            {
                _editStockNo.Error = "Specify a valid stock number";
                _isDataValid = false;
            }
            else
            {
                _editStockNo.Error = null;
            }

            if (string.IsNullOrWhiteSpace(_editModelVariation.Text) || _editModelVariation.Text.Length < 1)
            {
                _editModelVariation.Error = "Specify a model variaition";
                _isDataValid = false;
            }
            else
            {
                _editModelVariation.Error = null;
            }

            if (!_isPriceOnRequest) //if its not price on request
            {
                if (string.IsNullOrWhiteSpace(_editListPrice.Text))
                {
                    _editListPrice.Error = "Please provide a list price";
                    _isDataValid = false;
                }
                else
                {
                    float parsedListPrice;
                    var isFloat = float.TryParse(_editListPrice.Text, out parsedListPrice);

                    if (!isFloat)
                    {
                        _isDataValid = false;
                        _editListPrice.Error = "Please provide a valid list price";
                    }
                    else
                    {
                        _editListPrice.Error = null;
                    }
                }
            }

            _btnUpdateVehicle.Enabled = _isDataValid;
        }

        private async Task<bool> PopulateFields()
        {
            bool resp = false;
            try
            {
                if (!_vehicleObjectId.Equals("NA"))
                {
                    //load the details
                    await _dataManager.LoadSingleVehicleProfile(_vehicleObjectId);

                    if (CarProfile.description != null || !string.IsNullOrEmpty(CarProfile.description))
                    {
                        var description = CarProfile.description.Trim();
                        if (description.Length > 0)
                        {
                            _editDescription.Text = CarProfile.description;
                        }
                    }

                    _yearSpinner.SetSelection(_yearAdapter.GetPosition(CarProfile.year));
                    _makesSpinner.SetSelection(_makesAdapter.GetPosition(CarProfile.make));
                    _modelSpinner.SetSelection(_modelAdapter.GetPosition(CarProfile.model));
                    _distanceTypeSpinner.SetSelection(_distanceAdapter.GetPosition(CarProfile.milesData));
                    _engineTypeSpinner.SetSelection(_engineAdapter.GetPosition(CarProfile.engine));
                    _driveTrainSpinner.SetSelection(_driveTrainAdapter.GetPosition(CarProfile.driveTrain));
                    _transmissionSpinner.SetSelection(_transmissionAdapter.GetPosition(CarProfile.transmission));
                    _carTypeSpinner.SetSelection(_carTypeAdapter.GetPosition(CarProfile.carType));
                    _intColorSpinner.SetSelection(_colorAdapter.GetPosition(CarProfile.interiorColor));
                    _extColorSpinner.SetSelection(_colorAdapter.GetPosition(CarProfile.exteriorColor));
                    _distanceTypeSpinner.SetSelection(_colorAdapter.GetPosition(CarProfile.milesData));
                    //set the default string values in case the user does not change the spinner values
                    _selectedMake = CarProfile.make;
                    _selectedModel = CarProfile.model;
                    _selectedDistanceUnit = CarProfile.milesData;
                    _selectedEngine = CarProfile.engine;
                    _selectedDriveTrain = CarProfile.driveTrain;
                    _selectedTransmission = CarProfile.transmission;
                    _selectedCarType = CarProfile.carType;
                    _selectedExtColor = CarProfile.exteriorColor;
                    _selectedIntColor = CarProfile.interiorColor;
                    _selectedYear = CarProfile.year;
                    _isPriceOnRequest = CarProfile.priceOnRequest;
                    _hasVat = CarProfile.vat;
                    //end of string presets

                    _vatCheckBox.Checked = CarProfile.vat;
                    _porCheckBox.Checked = CarProfile.priceOnRequest;
                    _editModelVariation.Text = CarProfile.modelVariant ?? "N/A";
                    _editListPrice.Text = CarProfile.listPrice;
                    //lblYear.Text = CarProfile.year ?? "N/A";
                    //lblDriveTrain.Text = CarProfile.driveTrain ?? "N/A";
                    //lblEngine.Text = CarProfile.engine ?? "N/A";
                    //lblTransmission.Text = CarProfile.transmission ?? "N/A";
                    _editMileage.Text = CarProfile.mileage.ToString();
                    _editVin.Text = CarProfile.vin ?? "N/A";
                    _editStockNo.Text = CarProfile.status ?? "N/A";
                    //lblIntColor.Text = CarProfile.interiorColor ?? "N/A";
                    //lblExtColor.Text = CarProfile.exteriorColor ?? "N/A";
                    resp = true;
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("Error loading vehicle details {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
            }

            AndHUD.Shared.Dismiss(this);
            return resp;
        }

        /// <summary>
        /// let us save the changes done by the user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnUpdateVehicleProfile(object sender, EventArgs e)
        {

            AndHUD.Shared.Show(this, "Updating vehicle details...", -1, MaskType.Black);
            try
            {
                var updateCarObject = new Car
                {
                    make = _selectedMake,
                    model = _selectedModel,
                    modelVariant = _editModelVariation.Text,
                    year = _selectedYear,
                    mileage = _vehicleMileage,
                    milesData = _selectedDistanceUnit,
                    listPrice = _editListPrice.Text,
                    priceOnRequest = _isPriceOnRequest,
                    vat = _hasVat,
                    driveTrain = _selectedDriveTrain,
                    transmission = _selectedTransmission,
                    carType = _selectedCarType,
                    exteriorColor = _selectedExtColor,
                    interiorColor = _selectedIntColor,
                    description = _editDescription.Text,
                    objectId = _vehicleObjectId,
                    engine = _selectedEngine,
                    stockNumber = _editStockNo.Text
                };

                //save to parse
                var resp = await _dataManager.updateCar(updateCarObject);
                //end the activity
                Toast.MakeText(this, "Vehicle details updated successfully", ToastLength.Long).Show();
                Finish();
            }
            catch (Exception ex)
            {
                var message = string.Format("Error updating vehicle details {0} {1}", ex.Message, ex.StackTrace);
                Console.WriteLine(message);
                MetricsManager.TrackEvent(message);
                Toast.MakeText(this, "Vehicle not details updated successfully "+ex.Message, ToastLength.Short).Show();
            }
            AndHUD.Shared.Dismiss(this);
        }
    }
}
 