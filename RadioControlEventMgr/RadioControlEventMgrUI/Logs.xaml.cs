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
        List<Incident> incidents = new List<Incident>();

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
        }
    }
}
