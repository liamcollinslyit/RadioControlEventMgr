using RadioLibrary;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RadioControlEventMgrUI
{
    /// <summary>
    /// Interaction logic for Situation screen - Entering/Viewing crew, message and incident information
    /// </summary>
    public partial class Situation : Page
    {
        // Currently logged in user, passed from dashboard
        public User loggedInUser = new User();

        // Enum for current operation of stackpanel's, controlled by context menu
        enum DBOperation
        {
            Add,
            Edit
        }

        // DB Connection string
        RadioDBEntities db = new RadioDBEntities("metadata=res://*/RadioModel.csdl|res://*/RadioModel.ssdl|res://*/RadioModel.msl;provider=System.Data.SqlClient;provider connection string='data source=192.168.60.132" +
                                 ";initial catalog=RadioDB;user id=radiouser;password=password;pooling=False;MultipleActiveResultSets=True;App=EntityFramework'");

        // List for storing information read from DB
        List<Incident> incidents = new List<Incident>();
        List<Incident> activeIncidents = new List<Incident>();
        List<Crew> crews = new List<Crew>();
        List<Location> locations = new List<Location>();
        List<Status> statuses = new List<Status>();
        List<Message> messages = new List<Message>();

        // Variables for selected list item 
        Incident selectedIncident = new Incident();
        Crew selectedCrew = new Crew();

        // DB Operation for incident
        DBOperation dbOperation = new DBOperation();

        // Styles for incident and crew styles
        Style incidentStyle = new Style();
        Style crewStyle = new Style();

        public Situation()
        {
            InitializeComponent();
        }

        // When page is loaded - get info from DB, set time combooxes and list view styles
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshIncidentList();
            RefreshCrewList();
            RefreshLocations();
            RefreshStatus();
            RefreshMessagesList();

            loadTimeBoxes(cboSituationTimeHour, cboSituationTimeMin);
            loadTimeBoxes(cboAtSceneTimeHour, cboAtSceneTimeMin);
            loadTimeBoxes(cboLeaveSceneTimeHour, cboLeaveSceneTimeMin);
            loadTimeBoxes(cboMessageTimeHour, cboMessageTimeMin);

            // Incident Styles - No arrive time RED, No Leave Time GREEN, Else default GRAY
            incidentStyle.Setters.Add(new Setter() {Property = Control.BackgroundProperty,Value = Brushes.LightGray});
            AddStyleTrigger(incidentStyle, "LeaveSceneTime", null, Brushes.LightGreen);
            AddStyleTrigger(incidentStyle, "AtSceneTime", null, Brushes.LightCoral);
            lstSituationIncidentList.Resources.Add(typeof(ListViewItem), incidentStyle);

            // Crew Style - Available GREEN, Unavailable RED, Radio check Failed ORANGE
            AddStyleTrigger(crewStyle, "Status.StatusName", "Radio Check Failed", Brushes.LightSalmon);
            AddStyleTrigger(crewStyle, "Status.StatusName", "Available", Brushes.LightGreen);
            AddStyleTrigger(crewStyle, "Status.StatusName", "Unavailable", Brushes.LightCoral);
            lstCrewList.Resources.Add(typeof(ListViewItem), crewStyle);
        }

        // ---------------------------------------------------------------------------------------//
        // List box Selection Change
        // ---------------------------------------------------------------------------------------//

        // Incident selection changed, set selected user and enable context menu options
        private void lstSituationIncidentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstSituationIncidentList.SelectedIndex >= 0)
            {
                submenuAtScene.IsEnabled = false;
                submenuLeaveScene.IsEnabled = false;

                selectedIncident = incidents.ElementAt(lstSituationIncidentList.SelectedIndex);

                // if no at scene time enable context option, else if no leave scene time enable context option
                if (selectedIncident.AtSceneTime == null)
                {
                    submenuAtScene.IsEnabled = true;
                }
                else if (selectedIncident.LeaveSceneTime == null)
                {
                    submenuLeaveScene.IsEnabled = true;
                }

                // If current operation is edit then show incident details in panel
                if (dbOperation == DBOperation.Edit)
                {
                    ClearSituationDetails();
                    UpdateSituationDetails();
                }
               
                submenuEditIncident.IsEnabled = true;
                submenuDeleteIncident.IsEnabled = true;
            }
            else
            {
                submenuDeleteIncident.IsEnabled = false;
                submenuEditIncident.IsEnabled = false;
            }
        }

        // Crew selection changed, set selected Crew and enable context menu options
        private void lstCrewList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            submenuNewMessage.IsEnabled = false;
            submenuLocation.IsEnabled = false;
            submenuStatus.IsEnabled = false;
            submenuIncident.IsEnabled = false;
            submenuClearIncident.IsEnabled = false;

            // Only allow New message and status change for control user
            if (lstCrewList.SelectedIndex == 0)
            {
                submenuNewMessage.IsEnabled = true;
                submenuStatus.IsEnabled = true;

            }
            if (lstCrewList.SelectedIndex >= 1)
            {
                submenuNewMessage.IsEnabled = true;
                submenuLocation.IsEnabled = true;
                submenuStatus.IsEnabled = true;
                submenuIncident.IsEnabled = true;
                submenuClearIncident.IsEnabled = true;
            }
            selectedCrew = crews.ElementAt(lstCrewList.SelectedIndex);
            UpdateCrewDetails();
        }

        // ---------------------------------------------------------------------------------------//
        // Incident context menu Click Events
        // ---------------------------------------------------------------------------------------//

        // Add incident click event - set DBOperation, show clear incident stackpanel , and set time to now
        private void submenuAddIncident_Click(object sender, RoutedEventArgs e)
        {
            dbOperation = DBOperation.Add;
            lblArrivedScene.Visibility = Visibility.Collapsed;
            lblLeaveScene.Visibility = Visibility.Collapsed;
            stkAtSceneTime.Visibility = Visibility.Collapsed;
            stkLeaveSceneTime.Visibility = Visibility.Collapsed;
            lstSituationMessages.Visibility = Visibility.Collapsed;
            stkSituationIncident.Visibility = Visibility.Visible;
            ClearSituationDetails();
            CreateLogEntry("User opened add incident panel", loggedInUser.UserId);
        }

        // Edit incident click event - set DBOperation, show updated panel and note in system log
        private void submenuEditIncident_Click(object sender, RoutedEventArgs e)
        {
            dbOperation = DBOperation.Edit;
            lblArrivedScene.Visibility = Visibility.Visible;
            lblLeaveScene.Visibility = Visibility.Visible;
            stkAtSceneTime.Visibility = Visibility.Visible;
            stkLeaveSceneTime.Visibility = Visibility.Visible;
            stkSituationIncident.Visibility = Visibility.Visible;
            lstSituationMessages.Visibility = Visibility.Collapsed;
            ClearSituationDetails();
            UpdateSituationDetails();
            CreateLogEntry("User opened edit incident panel", loggedInUser.UserId);
        }

        // At scene  click event - Set selected incident at scene time to now, enable leave scene option and note in system log
        private void submenuAtScene_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set selcted incident at scene time to now
                foreach (var incident in db.Incidents.Where(t => t.IncidentID == selectedIncident.IncidentID))
                {
                    incident.AtSceneTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                }
            }
            catch (EntityException)
            {
                DBConnectionError();
            }

            SaveDBChanges();
            CreateLogEntry($"User set arrived scene time for {selectedIncident.IncidentNo} to {selectedIncident.AtSceneTime}", loggedInUser.UserId);
            RefreshIncidentList();
            submenuAtScene.IsEnabled = false;
            submenuLeaveScene.IsEnabled = true;
        }

        // At scene  click event - Set selected incident at scene time to now, remove from all crew and note in system log
        private void submenuLeaveScene_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set selcted incident at scene time to now
                foreach (var incident in db.Incidents.Where(t => t.IncidentID == selectedIncident.IncidentID))
                {
                    incident.LeaveSceneTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                }

                // Remove incident from all crew
                foreach (var crew in db.Crews.Where(t => t.IncidentID == selectedIncident.IncidentID))
                {
                    crew.IncidentID = null;
                }
            }
            catch (EntityException)
            {
                DBConnectionError();
            }

            SaveDBChanges();
            CreateLogEntry($"User set leave scene time for {selectedIncident.IncidentNo} to {selectedIncident.LeaveSceneTime} (all crew removed from incident)", loggedInUser.UserId);
            RefreshCrewList();
            RefreshIncidentList();
            submenuLeaveScene.IsEnabled = false;
        }

        // Delecte incident click event - Confirm with user, Delete user from DB and note in system log
        private void submenuDeleteIncident_Click(object sender, RoutedEventArgs e)
        {
            // Confirm proceede ?
            MessageBoxResult confirm = MessageBox.Show($"Are you sure you want to delete incident: {selectedIncident.IncidentNo}", "Delete Incident", MessageBoxButton.YesNo,MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                db.Incidents.RemoveRange(db.Incidents.Where(t => t.IncidentID == selectedIncident.IncidentID));
                SaveDBChanges();
                CreateLogEntry($"User removed {selectedIncident.IncidentNo}", loggedInUser.UserId);
                RefreshIncidentList();
            }
        }

        // ---------------------------------------------------------------------------------------//
        // Incident panel Click Events and V
        // ---------------------------------------------------------------------------------------//

        // Reported Now button - Set reported time to now in combobox
        private void btnSituationNow_Click(object sender, RoutedEventArgs e)
        {
            SetTimeBoxNow(cboSituationTimeHour,cboSituationTimeMin);
        }

        // At Scene Now button - Set At Scene time to now in combobox
        private void btnAtSceneNow_Click(object sender, RoutedEventArgs e)
        {
            SetTimeBoxNow(cboAtSceneTimeHour, cboAtSceneTimeMin);
        }

        // Leave Scene Now button - Set Leave Scene time to now in combobox
        private void btnLeaveSceneNow_Click(object sender, RoutedEventArgs e)
        {
            SetTimeBoxNow(cboLeaveSceneTimeHour, cboLeaveSceneTimeMin);
        }

        // Cancel button - Close panel without saving and note in system log
        private void btnSituationCancel_Click(object sender, RoutedEventArgs e)
        {
            stkSituationIncident.Visibility = Visibility.Collapsed;
            lstSituationMessages.Visibility = Visibility.Visible;
            CreateLogEntry("User closed add incident panel", loggedInUser.UserId);
        }

        // Ok button - validate details then perform the selected operation and hide panel
        private void btnSituationOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidateIncidentDetails())
                {
                    TimeSpan reportTime = new TimeSpan(Convert.ToInt32(cboSituationTimeHour.SelectedValue), Convert.ToInt32(cboSituationTimeMin.SelectedValue), 0);
                    Location location = (Location)cboSituationLocation.SelectedItem;
                    string details = txtSituationDetails.Text.Trim();

                    if (dbOperation == DBOperation.Add)
                    {
                        CreateIncidentEntry(reportTime, location, details);
                    }
                    if (dbOperation == DBOperation.Edit)
                    {
                        UpdateIncident(reportTime, location, details);

                        if (cboAtSceneTimeHour.SelectedIndex >= 0)
                        {
                            // if At Scene Time selected then update time in DB
                            foreach (var incident in db.Incidents.Where(t => t.IncidentID == selectedIncident.IncidentID))
                            {
                                incident.AtSceneTime = new TimeSpan(Convert.ToInt32(cboAtSceneTimeHour.SelectedValue), Convert.ToInt32(cboAtSceneTimeMin.SelectedValue), 0);
                                submenuAtScene.IsEnabled = false;
                                submenuLeaveScene.IsEnabled = true;
                            }
                            SaveDBChanges();
                        }
                        if (cboLeaveSceneTimeHour.SelectedIndex >= 0)
                        {

                            // if leave Scene Time selected then update time in DB
                            foreach (var incident in db.Incidents.Where(t => t.IncidentID == selectedIncident.IncidentID))
                            {
                                incident.LeaveSceneTime = new TimeSpan(Convert.ToInt32(cboLeaveSceneTimeHour.SelectedValue), Convert.ToInt32(cboLeaveSceneTimeMin.SelectedValue), 0);
                                submenuLeaveScene.IsEnabled = false;
                            }
                            SaveDBChanges();
                        }
                    }

                    RefreshIncidentList();
                    stkSituationIncident.Visibility = Visibility.Collapsed;
                    lstSituationMessages.Visibility = Visibility.Visible;
                }
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        // ---------------------------------------------------------------------------------------//
        // Incident Details Validations 
        // ---------------------------------------------------------------------------------------//
        
        // Validate Incident details supplied in the panel, if invlaid show usful error message for each item.
        private bool ValidateIncidentDetails()
        {
            bool valid = true;
            string errorMessage = "Incident Detail's Error:";

            if ((cboAtSceneTimeHour.SelectedIndex >= 0 && cboAtSceneTimeMin.SelectedIndex <0) || (cboAtSceneTimeHour.SelectedIndex <0 && cboAtSceneTimeMin.SelectedIndex >= 0))
            {
                errorMessage += Environment.NewLine + "Both hours and minutes must be selected for at scene time";
                valid = false;
            }
            if ((cboLeaveSceneTimeHour.SelectedIndex >= 0 && cboLeaveSceneTimeHour.SelectedIndex < 0) || (cboLeaveSceneTimeHour.SelectedIndex < 0 && cboLeaveSceneTimeHour.SelectedIndex >= 0))
            {
                errorMessage += Environment.NewLine + "Both hours and minutes must be selected for leave scene time";
                valid = false;
            }
            if (cboSituationLocation.SelectedIndex < 0)
            {
                errorMessage += Environment.NewLine + "Location must be selected";
                valid = false;
            }

            // If invlaid show the cumulative error message
            if (!valid)
            {
                MessageBox.Show(errorMessage, "Incident Detail's Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                CreateLogEntry($"Error: Invalid incident details entered - {errorMessage}", loggedInUser.UserId);
            }

            return valid;
        }

        // ---------------------------------------------------------------------------------------//
        // Crew Context Click Events
        // ---------------------------------------------------------------------------------------//

        // New message click event - show message stackpanel, set time to now and note in system log
        private void submenuNewMessage_Click(object sender, RoutedEventArgs e)
        {
            stkMessage.Visibility = Visibility.Visible;
            lstSituationMessages.Visibility = Visibility.Collapsed;
            SetTimeBoxNow(cboMessageTimeHour,cboMessageTimeMin);
            CreateLogEntry("User opened new message panel", loggedInUser.UserId);
        }

        // Incident click event - Get the selected item, update selected crew with change and note to system log
        private void submenuIncident_Click(object sender, RoutedEventArgs e)
        {
            MenuItem selectedItem = e.OriginalSource as MenuItem;
            Incident menuIncident = selectedItem.DataContext as Incident;
            UpdateCrew(selectedCrew.Status, selectedCrew.Location, menuIncident);
            CreateMessageEntry(DateTime.Now, selectedCrew, selectedCrew.Incident, selectedCrew.Status, $"Assigned to incident - {selectedCrew.Incident.IncidentNo}");
        }

        // Location click event - Get the selected item, update selected crew with change and note to system log
        private void submenuLocation_Click(object sender, RoutedEventArgs e)
        {
            MenuItem selectedItem = e.OriginalSource as MenuItem;
            Location menuLocation = selectedItem.DataContext as Location;
            UpdateCrew(selectedCrew.Status, menuLocation, selectedCrew.Incident);
            CreateMessageEntry(DateTime.Now, selectedCrew, selectedCrew.Incident, selectedCrew.Status, $"Changed to location - {selectedCrew.Location.LocationName}");
        }

        // Status click event - Get the selected item, update selected crew with change and note to system log
        private void submenuStatus_Click(object sender, RoutedEventArgs e)
        {
            MenuItem selectedItem = e.OriginalSource as MenuItem;
            Status menuStatus = selectedItem.DataContext as Status;
            UpdateCrew(menuStatus, selectedCrew.Location, selectedCrew.Incident);
            CreateMessageEntry(DateTime.Now, selectedCrew, selectedCrew.Incident, selectedCrew.Status, $"Changed status to - {selectedCrew.Status.StatusName}");
        }

        // Clear Incident click  event - clear incident from selected crew and note to system log 
        private void submenuClearIncident_Click(object sender, RoutedEventArgs e)
        {
            CreateMessageEntry(DateTime.Now, selectedCrew, selectedCrew.Incident, selectedCrew.Status, $"Unassigned from incident - {selectedCrew.Incident.IncidentNo}");
            UpdateCrew(selectedCrew.Status, selectedCrew.Location, null);          
        }

        // ---------------------------------------------------------------------------------------//
        // Message panel Click Events
        // ---------------------------------------------------------------------------------------//

        // Now button - Set  message time to now in combobox
        private void btnMessageNow_Click(object sender, RoutedEventArgs e)
        {
            SetTimeBoxNow(cboMessageTimeHour, cboMessageTimeMin);
        }

        // Cancel button - Close panel without saving and note in system log
        private void btnMessageCancel_Click(object sender, RoutedEventArgs e)
        {
            stkMessage.Visibility = Visibility.Collapsed;
            lstSituationMessages.Visibility = Visibility.Visible;
            CreateLogEntry("User closed new message panel", loggedInUser.UserId);
        }

        // Ok button - Update crew, Add Message and clear + hide panel
        private void btnMessageOk_Click(object sender, RoutedEventArgs e)
        {
            DateTime dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Convert.ToInt32(cboMessageTimeHour.SelectedValue), Convert.ToInt32(cboMessageTimeMin.SelectedValue), 0);
            Crew crew = new Crew();
            crew.CallSignID = selectedCrew.CallSignID;
            Location location = (Location)cboMessageLocation.SelectedItem;
            Status status = (Status)cboMessageStatus.SelectedItem;
            Incident incident = (Incident)cboMessageIncident.SelectedItem;
            string text = txtMessageText.Text.Trim();

            CreateMessageEntry(dateTime, crew, incident, status, text);
            UpdateCrew(status, location, incident);
            txtMessageText.Text = "";
            stkMessage.Visibility = Visibility.Collapsed;
            lstSituationMessages.Visibility = Visibility.Visible;
            RefreshCrewList();
        }

        // ---------------------------------------------------------------------------------------//
        // Refreshing Data in tables and combo boxes, Add Style Trigger
        // ---------------------------------------------------------------------------------------//

        // Refresh Incident details list from DB and set information message in label panel
        private void RefreshIncidentList()
        {
            int incidentCount = 0;
            int activeCount = 0;

            incidents.Clear();
            activeIncidents.Clear();

            try
            {
                foreach (var incident in db.Incidents)
                {
                    incidents.Add(incident);
                    incidentCount++;
                }
                foreach (var incident in db.Incidents.Where(t=> t.LeaveSceneTime == null))
                {
                    activeIncidents.Add(incident);
                    activeCount++;
                }
            }
            catch (EntityException)
            {

                DBConnectionError();
            }

            txtNoIncidents.Text = incidentCount.ToString();
            txtOpenIncidents.Text = activeCount.ToString();

            // Order with Leave Scene to end 
            incidents = incidents.OrderBy(t => t.LeaveSceneTime.HasValue).ToList();

            lstSituationIncidentList.ItemsSource = incidents;
            cboMessageIncident.ItemsSource = activeIncidents;
            submenuIncident.ItemsSource = activeIncidents;
            lstSituationIncidentList.Items.Refresh();
            cboMessageIncident.Items.Refresh();
            submenuIncident.Items.Refresh();
        }

        // Refresh crew details list from DB and set information message in label panel
        private void RefreshCrewList()
        {
            int crewNum = 0;
            int availableCrew = 0;
            crews.Clear();

            try
            {
                foreach (var crew in db.Crews)
                {
                    crews.Add(crew);

                    // Ignore control in count
                    if (crew.CallSignID > 0)
                    {
                        crewNum++;
                    }   
                    // If crew still in radio check failed/unknow then show radio check button
                    if (crew.StatusID < 3)
                    {
                        btnRadioCheck.Visibility = Visibility.Visible;
                    }
                    if (crew.StatusID == 3 && crew.CallSignID > 0)
                    {
                        availableCrew++;                   
                    }
                }
            }
            catch (EntityException)
            {

                DBConnectionError();
            }

            txtCrewNo.Text = crewNum.ToString();
            txtAvailableCrew.Text = availableCrew.ToString();

            lstCrewList.ItemsSource = crews;
            lstCrewList.Items.Refresh();
        }

        // Refresh Locations details list from DB and set information message in label panel
        private void RefreshLocations()
        {
            locations.Clear();

            try
            {
                // Ignore control location
                foreach (var location in db.Locations.Where(t=>t.LocationID > 0))
                {
                    locations.Add(location);
                }
            }
            catch (EntityException)
            {

                DBConnectionError();
            }

            cboSituationLocation.ItemsSource = locations;
            cboMessageLocation.ItemsSource = locations;
            submenuLocation.ItemsSource = locations;
            cboSituationLocation.Items.Refresh();
            cboMessageLocation.Items.Refresh();
            submenuLocation.Items.Refresh();
        }

        // Refresh Status details list from DB and set information message in label panel
        private void RefreshStatus()
        {
            statuses.Clear();

            try
            {
                // Only allow chnage to Available/Unavailble
                foreach (var status in db.Status.Where(t=> t.StatusID >= 3))
                {
                    statuses.Add(status);
                }
            }
            catch (EntityException)
            {

                DBConnectionError();
            }

            cboMessageStatus.ItemsSource = statuses;
            submenuStatus.ItemsSource = statuses;

            cboMessageStatus.Items.Refresh();
            submenuStatus.Items.Refresh();
        }

        // Refresh Message details list from DB
        private void RefreshMessagesList()
        {
            try
            {
                messages.Clear();
                foreach (var message in db.Messages)
                {
                    messages.Add(message);
                }

                // Order by date descending
                messages = messages.OrderByDescending(t => t.Date).ToList();
                lstSituationMessages.ItemsSource = messages;

                lstSituationMessages.Items.Refresh();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        // Initialise given hour boxs to 00-23 and minutes box to 00-59
        private void loadTimeBoxes(ComboBox hour, ComboBox min)
        {
            for (int i = 0; i <= 23; i++)
            {
                if (i < 10) hour.Items.Add($"0{i}");
                else hour.Items.Add(i);
            }
            for (int i = 0; i <= 59; i++)
            {
                if (i < 10) min.Items.Add($"0{i}");
                else min.Items.Add(i);
            }
        }

        // Used to create the correct for List style using given values
        private void AddStyleTrigger(Style style, string binding, string value, Brush brush)
        {
            // create a  new data trigger with binding and value
            DataTrigger dataTrigger = new DataTrigger()
            {
                Binding = new Binding($"{binding}"),
                Value = value
            };
            // Add a setter to the trigger with brush
            dataTrigger.Setters.Add(new Setter()
            {
                Property = Control.BackgroundProperty,
                Value = brush
            });
            // Add this trigger to the style
            style.Triggers.Add(dataTrigger);
        }

        // ---------------------------------------------------------------------------------------//
        // Panels update and clear
        // ---------------------------------------------------------------------------------------//

        // Update incident details in panel
        private void UpdateSituationDetails()
        {
            TimeSpan reportedTime = (TimeSpan) selectedIncident.ReportedTime;
            cboSituationTimeHour.SelectedIndex = Convert.ToInt32(Math.Floor(reportedTime.TotalHours));
            cboSituationTimeMin.SelectedIndex = Convert.ToInt32((reportedTime.TotalHours - Math.Floor(reportedTime.TotalHours)) * 60);

            // If incident has at scene time update comboboxes
            if (selectedIncident.AtSceneTime != null)
            {
                TimeSpan atSceneTime = (TimeSpan)selectedIncident.AtSceneTime;
                cboAtSceneTimeHour.SelectedIndex = Convert.ToInt32(Math.Floor(atSceneTime.TotalHours));
                cboAtSceneTimeMin.SelectedIndex = Convert.ToInt32((atSceneTime.TotalHours - Math.Floor(atSceneTime.TotalHours)) * 60);
            }

            // If incident has at leave time update comboboxes
            if (selectedIncident.LeaveSceneTime != null)
            {
                TimeSpan leaveSceneTime = (TimeSpan)selectedIncident.LeaveSceneTime;
                cboLeaveSceneTimeHour.SelectedIndex = Convert.ToInt32(Math.Floor(leaveSceneTime.TotalHours));
                cboLeaveSceneTimeMin.SelectedIndex = Convert.ToInt32((leaveSceneTime.TotalHours - Math.Floor(leaveSceneTime.TotalHours)) * 60);
            }
           
            lblSituationIncidentTitle.Content = selectedIncident.IncidentNo;
            cboSituationLocation.SelectedItem = selectedIncident.Location;
            txtSituationDetails.Text = selectedIncident.Description;
        }

        // Clear incident details in panel
        private void ClearSituationDetails()
        {
            SetTimeBoxNow(cboSituationTimeHour, cboSituationTimeMin);
            cboAtSceneTimeHour.SelectedIndex = -1;
            cboAtSceneTimeMin.SelectedIndex = -1;
            cboLeaveSceneTimeHour.SelectedIndex = -1;
            cboLeaveSceneTimeMin.SelectedIndex = -1;
            cboSituationLocation.SelectedIndex = -1;
            txtSituationDetails.Text = "";
        }

        // Update crew details in panel and clear message box
        private void UpdateCrewDetails()
        {
            SetTimeBoxNow(cboMessageTimeHour, cboMessageTimeMin);
            lblMessageTitle.Content = selectedCrew.CallSign;
            cboMessageStatus.SelectedItem = selectedCrew.Status;
            cboMessageLocation.SelectedItem = selectedCrew.Location;
            cboMessageIncident.SelectedItem = selectedCrew.Incident;
            txtMessageText.Text = "";
        }

        // Given Hour and Minutes comboboxes - set hours and minutes to now
        private void SetTimeBoxNow(ComboBox hour, ComboBox min)
        {
            if (DateTime.Now.Hour < 10) hour.SelectedValue = $"0{DateTime.Now.Hour}";
            else hour.SelectedValue = DateTime.Now.Hour;

            if (DateTime.Now.Minute < 10) min.SelectedValue = $"0{DateTime.Now.Minute}";
            else min.SelectedValue = DateTime.Now.Minute;
        }

        // ---------------------------------------------------------------------------------------//
        // Database Updates 
        // ---------------------------------------------------------------------------------------//

        // Function to create incident in the DB with given data and note in system logs
        private void CreateIncidentEntry(TimeSpan reportedTime, Location location, string details)
        {
            // Create new incident with supplied details and add to DB
            Incident incident = new Incident();
            incident.IncidentNo = "NEWINC";
            incident.LocationID = location.LocationID;
            incident.ReportedTime = reportedTime;
            incident.Description = details;
            db.Entry(incident).State = System.Data.Entity.EntityState.Added;
            SaveDBChanges();

            // Create Incident No in formation INC InicdntID
            incident.IncidentNo = $"INC {incident.IncidentID}";
            SaveDBChanges();

            CreateLogEntry($"User created a new incident: {incident.IncidentNo} reported at {incident.ReportedTime} at {incident.Location.LocationName} ", loggedInUser.UserId);
        }

        // Function to update the selected incident with supplied details in DB and note in system logs
        private void UpdateIncident(TimeSpan reportedTime, Location location, string details)
        {
            if (selectedIncident.ReportedTime != reportedTime || selectedIncident.Location != location || selectedIncident.Description != details)
            {
                try
                {
                    // Find the selected incident in the DB and change to supplied details
                    foreach (var incident in db.Incidents.Where(t => t.IncidentID == selectedIncident.IncidentID))
                    {
                        incident.ReportedTime = reportedTime;
                        incident.Location = location;
                        incident.Description = details;
                    }
                }
                catch (EntityException)
                {

                    DBConnectionError();
                }

                int saveSuccess = SaveDBChanges();
                if (saveSuccess == 1)
                {
                    CreateLogEntry($"Incident {selectedIncident.IncidentNo} successfully updated in system: ", loggedInUser.UserId);
                    RefreshIncidentList();
                }
                else
                {
                    // If save is failed show message and note to system log
                    MessageBox.Show("Problem updating incident record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                    CreateLogEntry($"Error:  Problem updating user {selectedIncident.IncidentNo}", loggedInUser.UserId);
                }
            }
        }

        // Function to create message in the DB with given data and note in system logs
        private void CreateMessageEntry(DateTime dateTime, Crew crew, Incident incident, Status status, string text)
        {
            // Create new message with supplied details and add to DB
            Message message = new Message();
            message.Date = dateTime;
            message.CallSignID = crew.CallSignID;
            message.StatusID = status.StatusID;
            message.MessageText = text;

            // if incident supplied  update this also
            if (incident != null)
            {
                message.IncidentID = incident.IncidentID;
            }

            db.Entry(message).State = System.Data.Entity.EntityState.Added;
            SaveDBChanges();

            RefreshMessagesList();
            CreateLogEntry($"User created a new message: {crew.CallSignID} at {message.Date} ", loggedInUser.UserId);
        }

        // Function to update the selected crew with supplied details in DB and note in system logs
        private void UpdateCrew(Status status, Location location, Incident incident)
        {
            try
            {
                // Find the selected crew in the DB and change to supplied details
                foreach (var crew in db.Crews.Where(t => t.CallSignID == selectedCrew.CallSignID))
                {
                    crew.Status = status;
                    crew.Location = location;
                    crew.Incident = incident;
                }
            }
            catch (EntityException)
            {

                DBConnectionError();
            }

            SaveDBChanges();
            RefreshCrewList();

            // create log entry depening on if incident attached or not
            if (selectedCrew.Incident != null)
            {
               CreateLogEntry($"User updated crew details: {selectedCrew.CallSign} with status {selectedCrew.Status.StatusName}, Location {selectedCrew.Location.LocationName} and Incident {selectedCrew.Incident.IncidentNo} ", loggedInUser.UserId);
            }
            else
            {
               CreateLogEntry($"User updated crew details: {selectedCrew.CallSign} with status {selectedCrew.Status.StatusName} and Location {selectedCrew.Location.LocationName}", loggedInUser.UserId);
            }
           
        }

        // ---------------------------------------------------------------------------------------//
        // Radio Check Click Event
        // ---------------------------------------------------------------------------------------//

        // Radio Check Click Event - Step though all uncheck crew and ask if radio check successful
        private void btnRadioCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // step though all crew not availble/unavailable
                foreach (var crew in db.Crews.Where(t => t.StatusID < 3))
                {
                    // Ask if radio check successful yes/no/canel
                    MessageBoxResult result = MessageBox.Show($"Radio Check with {crew.CallSign}, OK ?", "Radio Check", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

                    // Yes change to available
                    if (result == MessageBoxResult.Yes)
                    {
                        crew.StatusID = 3;
                    }
                    // No change to radio check failed
                    if (result == MessageBoxResult.No)
                    {
                        crew.StatusID = 2;
                    }
                    // Cancel break loop
                    if (result == MessageBoxResult.Cancel)
                    {
                        break;
                    }  
                }

                btnRadioCheck.Visibility = Visibility.Collapsed;
                SaveDBChanges();
                RefreshCrewList();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        // ---------------------------------------------------------------------------------------//
        // Log Messages And DB Updates With Error Control
        // ---------------------------------------------------------------------------------------//

        // Funtion to create new log and write to DB with supplied Description and userID
        public void CreateLogEntry(string eventDescription, int userID)
        {
            Log log = new Log();
            log.Date = DateTime.Now;
            log.Event = eventDescription;
            log.UserID = userID;
            db.Entry(log).State = System.Data.Entity.EntityState.Added;
            SaveDBChanges();
        }

        // Error controled function to save to database. Return success/failure
        public int SaveDBChanges()
        {
            int success = 0;
            try
            {
                success = db.SaveChanges();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
            return success;
        }

        // Display error message and close application should DB connection fail
        public void DBConnectionError()
        {
            MessageBox.Show("Problem connecting to the SQL server, contact system administrator. Application will now close.", "Connection to Database", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(0);
        }
    }
}

