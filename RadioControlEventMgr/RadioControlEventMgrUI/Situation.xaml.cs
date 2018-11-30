using RadioLibrary;
using System;
using System.Collections.Generic;
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

    // C# code for Situation page

    public partial class Situation : Page
    {
        RadioDBEntities db = new RadioDBEntities("metadata=res://*/RadioModel.csdl|res://*/RadioModel.ssdl|res://*/RadioModel.msl;provider=System.Data.SqlClient;provider connection string='data source=192.168.60.132" +
                                 ";initial catalog=RadioDB;user id=radiouser;password=password;pooling=False;MultipleActiveResultSets=True;App=EntityFramework'");

        List<Incident> incidents = new List<Incident>();
        List<Crew> crews = new List<Crew>();
        List<Location> locations = new List<Location>();
        List<Status> statuses = new List<Status>();

        Incident selectedIncident = new Incident();
        Crew selectedCrew = new Crew();

        Style incidentStyle = new Style();
        Style crewStyle = new Style();

        public Situation()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshIncidentList();
            RefreshCrewList();
            RefreshLocations();
            RefreshStatus();

            // Initialize time comboboxes Hours 00-23, Minutes 00-59
            for (int i = 0; i <= 23; i++)
            {
                if (i < 10)
                {
                    cboSituationTimeHour.Items.Add($"0{i}");
                    cboMessageTimeHour.Items.Add($"0{i}");
                }
                else
                {
                    cboSituationTimeHour.Items.Add(i);
                    cboMessageTimeHour.Items.Add(i);
                }
            }
            for (int i = 0; i <= 59; i++)
            {
                if (i < 10)
                {
                    cboSituationTimeMin.Items.Add($"0{i}");
                    cboMessageTimeMin.Items.Add($"0{i}");
                }
                else
                {
                    cboSituationTimeMin.Items.Add(i);
                    cboMessageTimeMin.Items.Add(i);
                }
            }

            incidentStyle.Setters.Add(new Setter() {Property = Control.BackgroundProperty,Value = Brushes.LightGray});
            AddStyleTrigger(incidentStyle, "LeaveSceneTime", null, Brushes.LightGreen);
            AddStyleTrigger(incidentStyle, "AtSceneTime", null, Brushes.LightCoral);
            lstSituationIncidentList.Resources.Add(typeof(ListViewItem), incidentStyle);

            AddStyleTrigger(crewStyle, "Status.StatusName", "Available", Brushes.LightGreen);
            AddStyleTrigger(crewStyle, "Status.StatusName", "Unavailable", Brushes.LightCoral);
            lstCrewList.Resources.Add(typeof(ListViewItem), crewStyle);

        }

        // ---------------------------------------------------------------------------------------//
        // Incident context menu Click Events
        // ---------------------------------------------------------------------------------------//

        // Context menu - Add incident button - show incident stackpanel , and set time to now
        private void submenuAddIncident_Click(object sender, RoutedEventArgs e)
        {
            stkSituationIncident.Visibility = Visibility.Visible;
            ClearSituationDetails();
        }

        private void submenuAtScene_Click(object sender, RoutedEventArgs e)
        {
            foreach (var incident in db.Incidents.Where(t => t.IncidentID == selectedIncident.IncidentID))
            {
                incident.AtSceneTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            }

            db.SaveChanges();
            RefreshIncidentList();
            submenuAtScene.IsEnabled = false;
            submenuLeaveScene.IsEnabled = true;
        }

        private void submenuLeaveScene_Click(object sender, RoutedEventArgs e)
        {
            foreach (var incident in db.Incidents.Where(t => t.IncidentID == selectedIncident.IncidentID))
            {
                incident.LeaveSceneTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            }

            db.SaveChanges();
            RefreshIncidentList();
            submenuLeaveScene.IsEnabled = false;
        }

        private void submenuDeleteIncident_Click(object sender, RoutedEventArgs e)
        {
            db.Incidents.RemoveRange(db.Incidents.Where(t => t.IncidentID == selectedIncident.IncidentID));
            db.SaveChanges();
            RefreshIncidentList();
        }

        // ---------------------------------------------------------------------------------------//
        // Incident panel Click Events
        // ---------------------------------------------------------------------------------------//

        // Incident panel - Now button - Set time to now in combobox
        private void btnSituationNow_Click(object sender, RoutedEventArgs e)
        {
            SetIncidentTime();
        }

        // Incident panel - Cancel button - hide incident stackpanel
        private void btnSituationCancel_Click(object sender, RoutedEventArgs e)
        {
            stkSituationIncident.Visibility = Visibility.Collapsed;
        }

        // Incident panel - Ok button - hide message stackpanel
        private void btnSituationOk_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan time = new TimeSpan(Convert.ToInt32(cboSituationTimeHour.SelectedValue), Convert.ToInt32(cboSituationTimeMin.SelectedValue), 0);
            string details = txtSituationDetails.Text;

            Location location = new Location();
            location = (Location)cboSituationLocation.SelectedItem;

            CreateIncidentEntry(location, time, details);
            RefreshIncidentList();
            stkSituationIncident.Visibility = Visibility.Collapsed;
        }

        // ---------------------------------------------------------------------------------------//
        // Crew Context Click Events
        // ---------------------------------------------------------------------------------------//

        // Context menu - New message button - show message stackpanel, and set time to now
        private void submenuNewMessage_Click(object sender, RoutedEventArgs e)
        {
            stkMessage.Visibility = Visibility.Visible;
            SetMessageTime();
        }

        private void submenuIncident_Click(object sender, RoutedEventArgs e)
        {
            MenuItem selectedItem = e.OriginalSource as MenuItem;
            Incident menuIncident = selectedItem.DataContext as Incident;
            UpdateCrew(selectedCrew.Status, selectedCrew.Location, menuIncident);
        }

        private void submenuLocation_Click(object sender, RoutedEventArgs e)
        {
            MenuItem selectedItem = e.OriginalSource as MenuItem;
            Location menuLocation = selectedItem.DataContext as Location;
            UpdateCrew(selectedCrew.Status, menuLocation, selectedCrew.Incident);
        }

        private void submenuStatus_Click(object sender, RoutedEventArgs e)
        {
            MenuItem selectedItem = e.OriginalSource as MenuItem;
            Status menuStatus = selectedItem.DataContext as Status;
            UpdateCrew(menuStatus, selectedCrew.Location, selectedCrew.Incident);
        }

        // ---------------------------------------------------------------------------------------//
        // Message panel Click Events
        // ---------------------------------------------------------------------------------------//

        // Message panel - Now button - Set time to now in combobox
        private void btnMessageNow_Click(object sender, RoutedEventArgs e)
        {
            SetMessageTime();
        }

        // Message Panel  - Cancel button - hide message stackpanel
        private void btnMessageCancel_Click(object sender, RoutedEventArgs e)
        {
            stkMessage.Visibility = Visibility.Collapsed;
        }

        // Message Panel  - Ok button - hide message stackpanel
        private void btnMessageOk_Click(object sender, RoutedEventArgs e)
        {
            DateTime dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Convert.ToInt32(cboMessageTimeHour.SelectedValue), Convert.ToInt32(cboMessageTimeMin.SelectedValue), 0);
            Crew crew = new Crew();
            crew.CallSignID = selectedCrew.CallSignID;
            Location location = (Location)cboMessageLocation.SelectedItem;
            Status status = (Status)cboMessageStatus.SelectedItem;
            Incident incident = (Incident)cboMessageIncident.SelectedItem;
            string text = txtMessageText.Text;

            CreateMessageEntry(dateTime, crew, incident, status, text);
            UpdateCrew(status, location, incident);
            txtMessageText.Text = "";
            stkMessage.Visibility = Visibility.Collapsed;
            RefreshCrewList();
        }

        // ---------------------------------------------------------------------------------------//
        // Refreshing Data in tables and combo boxes
        // ---------------------------------------------------------------------------------------//

        private void RefreshIncidentList()
        {
            lstSituationIncidentList.ItemsSource = incidents;
            cboMessageIncident.ItemsSource = incidents;
            submenuIncident.ItemsSource = incidents;
            incidents.Clear();
            foreach (var incident in db.Incidents)
            {
                incidents.Add(incident);
            }
            lstSituationIncidentList.Items.Refresh();
            cboMessageIncident.Items.Refresh();
            submenuIncident.Items.Refresh();
        }
        private void RefreshCrewList()
        {
            lstCrewList.ItemsSource = crews;
            crews.Clear();
            foreach (var crew in db.Crews)
            {
                crews.Add(crew);
            }
            lstCrewList.Items.Refresh();
        }

        private void RefreshLocations()
        {
            cboSituationLocation.ItemsSource = locations;
            cboMessageLocation.ItemsSource = locations;
            submenuLocation.ItemsSource = locations;
            locations.Clear();
            foreach (var location in db.Locations)
            {
                locations.Add(location);
            }
            cboSituationLocation.Items.Refresh();
            cboMessageLocation.Items.Refresh();
            submenuLocation.Items.Refresh();
        }

        private void RefreshStatus()
        {
            cboMessageStatus.ItemsSource = statuses;
            submenuStatus.ItemsSource = statuses;
            statuses.Clear();
            foreach (var status in db.Status)
            {
                statuses.Add(status);
            }
            cboMessageStatus.Items.Refresh();
            submenuStatus.Items.Refresh();
        }

        private void SetIncidentTime()
        {
            if (DateTime.Now.Hour < 10) cboSituationTimeHour.SelectedValue = $"0{DateTime.Now.Hour}";
            else cboSituationTimeHour.SelectedValue = DateTime.Now.Hour;

            if (DateTime.Now.Minute < 10) cboSituationTimeMin.SelectedValue = $"0{DateTime.Now.Minute}";
            else cboSituationTimeMin.SelectedValue = DateTime.Now.Minute;
        }

        private void SetMessageTime()
        {
            if (DateTime.Now.Hour < 10) cboMessageTimeHour.SelectedValue = $"0{DateTime.Now.Hour}";
            else cboMessageTimeHour.SelectedValue = DateTime.Now.Hour;

            if (DateTime.Now.Minute < 10) cboMessageTimeMin.SelectedValue = $"0{DateTime.Now.Minute}";
            else cboMessageTimeMin.SelectedValue = DateTime.Now.Minute;
        }

        // ---------------------------------------------------------------------------------------//
        // List box selection changes + style setter
        // ---------------------------------------------------------------------------------------//

        private void lstSituationIncidentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstSituationIncidentList.SelectedIndex >= 0)
            {

                submenuAtScene.IsEnabled = false;
                submenuLeaveScene.IsEnabled = false;

                selectedIncident = incidents.ElementAt(lstSituationIncidentList.SelectedIndex);
                if (selectedIncident.AtSceneTime == null)
                {
                    submenuAtScene.IsEnabled = true;
                }
                else if (selectedIncident.LeaveSceneTime == null)
                {
                    submenuLeaveScene.IsEnabled = true;
                }

                submenuDeleteIncident.IsEnabled = true;
            }
        }

        private void lstCrewList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCrewList.SelectedIndex >= 0)
            {
                submenuNewMessage.IsEnabled = true;
                selectedCrew = crews.ElementAt(lstCrewList.SelectedIndex);
                UpdateCrewDetails();
            }

        }

        private void AddStyleTrigger(Style style, string binding, string value, Brush brush)
        {

            DataTrigger dataTrigger = new DataTrigger()
            {
                Binding = new Binding($"{binding}"),
                Value = value
            };
            dataTrigger.Setters.Add(new Setter()
            {
                Property = Control.BackgroundProperty,
                Value = brush
            });
            style.Triggers.Add(dataTrigger);
        }

        // ---------------------------------------------------------------------------------------//
        // Panels update and clear
        // ---------------------------------------------------------------------------------------//

        private void UpdateCrewDetails()
        {
            SetMessageTime();
            lblMessageTitle.Content = selectedCrew.CallSign;
            cboMessageStatus.SelectedItem = selectedCrew.Status;
            cboMessageLocation.SelectedItem = selectedCrew.Location;
            cboMessageIncident.SelectedItem = selectedCrew.Incident;
            txtMessageText.Text = "";
        }

        private void ClearSituationDetails()
        {
            SetIncidentTime();
            cboSituationLocation.SelectedIndex = -1;
            txtSituationDetails.Text = "";
        }

        // ---------------------------------------------------------------------------------------//
        // Database Updates 
        // ---------------------------------------------------------------------------------------//

        private void CreateIncidentEntry(Location location, TimeSpan time, string details)
        {
            Incident incident = new Incident();

            incident.IncidentNo = "NEWINC";
            incident.LocationID = location.LocationID;
            incident.ReportedTime = time;
            incident.Description = details;
            db.Entry(incident).State = System.Data.Entity.EntityState.Added;
            db.SaveChanges();

            incident.IncidentNo = $"INC {incident.IncidentID}";
            db.SaveChanges();

        }

        private void CreateMessageEntry(DateTime dateTime, Crew crew, Incident incident, Status status, string text)
        {
            Message message = new Message();
            message.Date = dateTime;
            message.CallSignID = crew.CallSignID;

            if (incident != null)
            {
                message.IncidentID = incident.IncidentID;
            }

            message.StatusID = status.StatusID;
            message.MessageText = text;
            db.Entry(message).State = System.Data.Entity.EntityState.Added;
            db.SaveChanges();
        }
        private void UpdateCrew(Status status, Location location, Incident incident)
        {
            foreach (var crew in db.Crews.Where(t => t.CallSignID == selectedCrew.CallSignID))
            {
                crew.Status = status;
                crew.Location = location;
                crew.Incident = incident;
            }
            db.SaveChanges();
            RefreshCrewList();
        }

    }
}
// Add save error/confirm messages hadling
// add message updating to context menus
