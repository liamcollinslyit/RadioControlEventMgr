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

        private void RefreshMessagesList()
        {
            try
            {
                lstMessageList.ItemsSource = messages;
                messages.Clear();
                foreach (var message in db.Messages)
                {
                    messages.Add(message);
                }
                messages = messages.OrderByDescending(t => t.CallSignID).ToList();
                lstMessageList.Items.Refresh();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        private void RefreshIncidentList()
        {
            try
            {
                lstIncidentList.ItemsSource = incidents;
                incidents.Clear();
                foreach (var incident in db.Incidents)
                {
                    incidents.Add(incident);
                }
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
            lstIncidentMessages.ItemsSource = incidentMessage;
            incidentMessage.Clear();
            foreach (var message in db.Messages.Where(t => t.IncidentID == selectedIncident.IncidentID))
            {
                incidentMessage.Add(message);
            }
            lstIncidentMessages.Items.Refresh();

            lblIncidentTitle.Content = selectedIncident.IncidentNo;
            txtIncidentAt.Text = selectedIncident.AtSceneTime.ToString();
            txtIncidentLeave.Text = selectedIncident.LeaveSceneTime.ToString();
            txtIncidentLocation.Text = selectedIncident.Location.LocationName;
            txtIncidentDescription.Text = selectedIncident.Description;

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
    }
}
