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
    /// Interaction logic for Logs screen - Message and Incident Logs
    /// </summary>
    public partial class Logs : Page
    {
        // Currently logged in user, passed from dashboard
        public User loggedInUser = new User();

        // DB Connection string
        RadioDBEntities db = new RadioDBEntities("metadata=res://*/RadioModel.csdl|res://*/RadioModel.ssdl|res://*/RadioModel.msl;provider=System.Data.SqlClient;provider connection string='data source=192.168.60.132" +
                                         ";initial catalog=RadioDB;user id=radiouser;password=password;pooling=False;MultipleActiveResultSets=True;App=EntityFramework'");

        // List for storing information read from DB
        List<Message> messages = new List<Message>();
        List<Message> incidentMessage = new List<Message>();
        List<Incident> incidents = new List<Incident>();

        // Variables for selected list item 
        Incident selectedIncident = new Incident();

        public Logs()
        {
            InitializeComponent();
        }

        // When page is loaded, get info from DB
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshIncidentList();
            RefreshMessagesList();
        }

        // ---------------------------------------------------------------------------------------//
        // List box Selection Change and Incident Messages Panel
        // ---------------------------------------------------------------------------------------//

        // User selection changed, set selected user and enable context menu options
        private void lstIncidentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstIncidentList.SelectedIndex >= 0)
            {
                selectedIncident = incidents.ElementAt(lstIncidentList.SelectedIndex);
                submenuOpenIncident.IsEnabled = true;
                RefreshIncidentMessages();
            }
        }

        // ---------------------------------------------------------------------------------------//
        // Incident Log Context Click Events
        // ---------------------------------------------------------------------------------------//

        // Open incident button - show incident details stackpanel
        private void submenuOpenIncident_Click(object sender, RoutedEventArgs e)
        {
            stkIncident.Visibility = Visibility.Visible;
        }

        // ---------------------------------------------------------------------------------------//
        // Refreshing Data In Tables And Combo Boxes
        // ---------------------------------------------------------------------------------------//

        // Refresh Message details list from DB, filter if required and set information message in filter panel
        private void RefreshMessagesList(string filter = null)
        {
            try
            {
                int totalMessages = 0;

                messages.Clear();
                if (filter == null)
                {
                    foreach (var message in db.Messages)
                    {
                        messages.Add(message);
                        totalMessages++;
                    }
                    tbxMessgesStats.Text = $"Displaying all {totalMessages} messages";
                }
                else
                {
                    // return list of user that have filter text in any field
                    foreach (var message in db.Messages.Where(t => t.Date.ToString().Contains(filter) || t.Crew.CallSign.Contains(filter) || t.Incident.IncidentNo.Contains(filter)
                                                             || t.Status.StatusName.Contains(filter) || t.MessageText.Contains(filter)))
                    {
                        messages.Add(message);
                        totalMessages++;
                    }
                    tbxMessgesStats.Text = $"Displaying {totalMessages} messages using filter \"{filter}\"";
                }
                // Order by date descending
                messages = messages.OrderByDescending(t => t.Date).ToList();
                lstMessageList.ItemsSource = messages;
                lstMessageList.Items.Refresh();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        // Refresh Incident details list from DB, filter if required and set information message in filter panel
        private void RefreshIncidentList(string filter = null)
        {
            try
            {
                int totalIncidents = 0;
                int open = 0;
                int closed = 0;

                incidents.Clear();
                if (filter == null)
                {
                    foreach (var incident in db.Incidents)
                    {
                        incidents.Add(incident);
                        totalIncidents++;

                        // increment open/closed if incident has a leave scene time
                        if (incident.LeaveSceneTime == null) open++;
                        else closed++;
                    }
                    tbxIncidentStats.Text = $"Displaying all {totalIncidents} incidents. Open Incidents: {open}. Closed Incidents: {closed}.";
                }
                else
                {
                    // return list of user that have filter text in any field
                    foreach (var incident in db.Incidents.Where(t => t.IncidentNo.Contains(filter) || t.Location.LocationName.Contains(filter) || t.ReportedTime.ToString().Contains(filter) 
                                                                || t.AtSceneTime.ToString().Contains(filter) || t.LeaveSceneTime.ToString().Contains(filter) || t.Description.Contains(filter)))
                    {
                        incidents.Add(incident);
                        totalIncidents++;

                        // increment open/closed if incident has a leave scene time
                        if (incident.LeaveSceneTime == null) open++;
                        else closed++;
                    }
                    tbxIncidentStats.Text = $"Displaying {totalIncidents} messages using filter \"{filter}\". Open Incidents: {open}. Closed Incidents: {closed}.";
                }

                lstIncidentList.ItemsSource = incidents;
                lstIncidentList.Items.Refresh();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        // Refresh Incident message details list from DB
        private void RefreshIncidentMessages()
        {
            try
            {
                incidentMessage.Clear();

                foreach (var message in db.Messages.Where(t => t.IncidentID == selectedIncident.IncidentID))
                {
                    incidentMessage.Add(message);
                }

                // Order by date descending
                incidentMessage = incidentMessage.OrderBy(t => t.Date).ToList();
                lstIncidentMessages.ItemsSource = incidentMessage;
                lstIncidentMessages.Items.Refresh();

                // Set Incident panel textboxes with incident data
                lblIncidentTitle.Content = selectedIncident.IncidentNo;
                txtIncidentReported.Text = selectedIncident.ReportedTime.ToString();
                txtIncidentAt.Text = selectedIncident.AtSceneTime.ToString();
                txtIncidentLeave.Text = selectedIncident.LeaveSceneTime.ToString();
                txtIncidentLocation.Text = selectedIncident.Location.LocationName;
                txtIncidentDescription.Text = selectedIncident.Description;
            }
            catch (EntityException)
            {

                DBConnectionError();
            }
        }

        // ---------------------------------------------------------------------------------------//
        // Filter + Clear Button Click Events
        // ---------------------------------------------------------------------------------------//

        // Applied filter to messages list using entered text
        private void btnFilterMessages_Click(object sender, RoutedEventArgs e)
        {
            RefreshMessagesList(txtFilterMessages.Text);

        }

        // Remove filter from messages list and clear text box
        private void btnClearFilterMessages_Click(object sender, RoutedEventArgs e)
        {
            RefreshMessagesList();
            tbxMessgesStats.Text = "";
        }

        // Applied filter to incidents list using entered text
        private void btnFilterIncidents_Click(object sender, RoutedEventArgs e)
        {
            RefreshIncidentList(txtFilterIncidents.Text);
        }

        // Remove filter from incidents list and clear text box
        private void btnClearFilterIncidents_Click(object sender, RoutedEventArgs e)
        {
            RefreshIncidentList();
            tbxIncidentStats.Text = "";
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
