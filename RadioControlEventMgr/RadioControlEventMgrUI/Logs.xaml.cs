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

    // C# code for Logs page

    public partial class Logs : Page
    {

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

        // Context menu - Open incident button - show incident details stackpanel
        private void submenuOpenIncident_Click(object sender, RoutedEventArgs e)
        {
            stkIncident.Visibility = Visibility.Visible;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            lstMessageList.ItemsSource = messages;
            lstIncidentList.ItemsSource = incidents;
            foreach (var message in db.Messages)
            {
                messages.Add(message);
            }
            foreach (var incident in db.Incidents)
            {
                incidents.Add(incident);
            }
            messages = messages.OrderBy(t => t.Date).ToList();
            lstMessageList.Items.Refresh();
        }

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
    }
}
