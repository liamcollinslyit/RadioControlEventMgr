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

    // C# code for Logs page

    public partial class Logs : Page
    {
        public User loggedInUser = new User();

        RadioDBEntities db = new RadioDBEntities("metadata=res://*/RadioModel.csdl|res://*/RadioModel.ssdl|res://*/RadioModel.msl;provider=System.Data.SqlClient;provider connection string='data source=192.168.60.132" +
                                         ";initial catalog=RadioDB;user id=radiouser;password=password;pooling=False;MultipleActiveResultSets=True;App=EntityFramework'");

        List<Message> messages = new List<Message>();
        List<Message> incidentMessage = new List<Message>();
        List<Incident> incidents = new List<Incident>();

        Incident selectedIncident = new Incident();

        public Logs()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshIncidentList();
            RefreshMessagesList();
        }

        // ---------------------------------------------------------------------------------------//
        // Incident Log Click Events
        // ---------------------------------------------------------------------------------------//

        // Context menu - Open incident button - show incident details stackpanel
        private void submenuOpenIncident_Click(object sender, RoutedEventArgs e)
        {
            stkIncident.Visibility = Visibility.Visible;
        }

        // ---------------------------------------------------------------------------------------//
        // Refreshing Data In Tables And Combo Boxes
        // ---------------------------------------------------------------------------------------//

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
                    foreach (var message in db.Messages.Where(t => t.Date.ToString().Contains(filter) || t.Crew.CallSign.Contains(filter) || t.Incident.IncidentNo.Contains(filter)
                                                             || t.Status.StatusName.Contains(filter) || t.MessageText.Contains(filter)))

                    {
                        messages.Add(message);
                        totalMessages++;
                    }
                    tbxMessgesStats.Text = $"Displaying {totalMessages} messages using filter \"{filter}\"";
                }
  
                messages = messages.OrderByDescending(t => t.Date).ToList();
                lstMessageList.ItemsSource = messages;

                lstMessageList.Items.Refresh();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

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

                        if (incident.LeaveSceneTime == null) open++;
                        else closed++;
                    }
                    tbxIncidentStats.Text = $"Displaying all {totalIncidents} incidents. Open Incidents: {open}. Closed Incidents: {closed}.";
                }
                else
                {
                    foreach (var incident in db.Incidents.Where(t => t.IncidentNo.Contains(filter) || t.Location.LocationName.Contains(filter) || t.ReportedTime.ToString().Contains(filter) 
                                                                || t.AtSceneTime.ToString().Contains(filter) || t.LeaveSceneTime.ToString().Contains(filter) || t.Description.Contains(filter)))
                    {
                        incidents.Add(incident);
                        totalIncidents++;

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

        // ---------------------------------------------------------------------------------------//
        // List box Selection Change and Incident Messages Panel
        // ---------------------------------------------------------------------------------------//

        private void lstIncidentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstIncidentList.SelectedIndex >= 0)
            {
                selectedIncident = incidents.ElementAt(lstIncidentList.SelectedIndex);
                submenuOpenIncident.IsEnabled = true;
                UpdateIncidentMessages();
            }
        }

        private void UpdateIncidentMessages()
        {
            try
            {
                incidentMessage.Clear();

                foreach (var message in db.Messages.Where(t => t.IncidentID == selectedIncident.IncidentID))
                {
                    incidentMessage.Add(message);
                }

                incidentMessage = incidentMessage.OrderBy(t => t.Date).ToList();

                lstIncidentMessages.ItemsSource = incidentMessage;

                lstIncidentMessages.Items.Refresh();

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
        // Log Messages And DB Updates With Error Control
        // ---------------------------------------------------------------------------------------//

        /// <summary>
        /// Create an entry in the log database
        /// </summary>
        /// <param name="eventDescription"></param>
        /// Description of the event
        /// <param name="userID"></param>
        /// User ID of event generator
        public void CreateLogEntry(string eventDescription, int userID)
        {
            Log log = new Log();
            log.Date = DateTime.Now;
            log.Event = eventDescription;
            log.UserID = userID;
            db.Entry(log).State = System.Data.Entity.EntityState.Added;
            SaveDBChanges();
        }

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

        public void DBConnectionError()
        {
            MessageBox.Show("Problem connecting to the SQL server, contact system administrator. Application will now close.", "Connection to Database", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(0);
        }

        private void btnFilterMessages_Click(object sender, RoutedEventArgs e)
        {
            RefreshMessagesList(txtFilterMessages.Text);

        }

        private void btnClearFilterMessages_Click(object sender, RoutedEventArgs e)
        {
            RefreshMessagesList();
            tbxMessgesStats.Text = "";
        }

        private void btnFilterIncidents_Click(object sender, RoutedEventArgs e)
        {
            RefreshIncidentList(txtFilterIncidents.Text);
        }

        private void btnClearFilterIncidents_Click(object sender, RoutedEventArgs e)
        {
            RefreshIncidentList();
            tbxIncidentStats.Text = "";
        }
    }
}
