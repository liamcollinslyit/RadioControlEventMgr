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

        public Situation()
        {
            InitializeComponent();

            // Initialize time comboboxes Hours 00-23, Minutes 00-59
            for (int i = 0; i <= 23 ; i++)
            {
                if (i < 10) cboSituationTimeHour.Items.Add($"0{i}");
                else cboSituationTimeHour.Items.Add(i);
            }

            for (int i = 0; i <= 59; i++)
            {
                if (i<10) cboSituationTimeMin.Items.Add($"0{i}");
                else cboSituationTimeMin.Items.Add(i);  
            }

            for (int i = 0; i <= 23; i++)
            {
                if (i < 10) cboMessageTimeHour.Items.Add($"0{i}");
                else cboMessageTimeHour.Items.Add(i);
            }

            for (int i = 0; i <= 59; i++)
            {
                if (i < 10) cboMessageTimeMin.Items.Add($"0{i}");
                else cboMessageTimeMin.Items.Add(i);
            }
        }

        // Context menu - Add incident button - show incident stackpanel , and set time to now
        private void submenuAddIncident_Click(object sender, RoutedEventArgs e)
        {
            stkSituationIncident.Visibility = Visibility.Visible;

            if (DateTime.Now.Hour < 10) cboSituationTimeHour.SelectedValue = $"0{DateTime.Now.Hour}";
            else cboSituationTimeHour.SelectedValue = DateTime.Now.Hour;

            if (DateTime.Now.Minute < 10) cboSituationTimeMin.SelectedValue = $"0{DateTime.Now.Minute}";
            else cboSituationTimeMin.SelectedValue = DateTime.Now.Minute;

        }

        // Context menu - New message button - show message stackpanel, and set time to now
        private void submenuNewMessage_Click(object sender, RoutedEventArgs e)
        {
            stkMessage.Visibility = Visibility.Visible;

            if (DateTime.Now.Hour < 10) cboMessageTimeHour.SelectedValue = $"0{DateTime.Now.Hour}";
            else cboMessageTimeHour.SelectedValue = DateTime.Now.Hour;

            if (DateTime.Now.Minute < 10) cboMessageTimeMin.SelectedValue = $"0{DateTime.Now.Minute}";
            else cboMessageTimeMin.SelectedValue = DateTime.Now.Minute;
        }

        private void submenuAtScene_Click(object sender, RoutedEventArgs e)
        {
        }

        // Incident panel - Now button - Set time to now in combobox
        private void btnSituationNow_Click(object sender, RoutedEventArgs e)
        {
            if (DateTime.Now.Hour < 10) cboSituationTimeHour.SelectedValue = $"0{DateTime.Now.Hour}";
            else cboSituationTimeHour.SelectedValue = DateTime.Now.Hour;

            if (DateTime.Now.Minute < 10) cboSituationTimeMin.SelectedValue = $"0{DateTime.Now.Minute}";
            else cboSituationTimeMin.SelectedValue = DateTime.Now.Minute;
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
            stkSituationIncident.Visibility = Visibility.Collapsed;
        }

        // Message panel - Now button - Set time to now in combobox
        private void btnMessageNow_Click(object sender, RoutedEventArgs e)
        {
            if (DateTime.Now.Hour < 10) cboMessageTimeHour.SelectedValue = $"0{DateTime.Now.Hour}";
            else cboMessageTimeHour.SelectedValue = DateTime.Now.Hour;

            if (DateTime.Now.Minute < 10) cboMessageTimeMin.SelectedValue = $"0{DateTime.Now.Minute}";
            else cboMessageTimeMin.SelectedValue = DateTime.Now.Minute;
        }

        // Message Panel  - Cancel button - hide message stackpanel
        private void btnMessageCancel_Click(object sender, RoutedEventArgs e)
        {
            stkMessage.Visibility = Visibility.Collapsed;
        }

        // Message Panel  - Ok button - hide message stackpanel
        private void btnMessageOk_Click(object sender, RoutedEventArgs e)
        {
            stkMessage.Visibility = Visibility.Collapsed;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            lstSituationIncidentList.ItemsSource = incidents;
            lstCrewList.ItemsSource = crews;
            cboSituationLocation.ItemsSource = locations;

            foreach (var incident in db.Incidents)
            {
                incidents.Add(incident);
            }

            foreach (var crew in db.Crews)
            {
                crews.Add(crew);
            }

            foreach (var location in db.Locations)
            {
                locations.Add(location);
            }
        }

        private void CreateIncidentEntry(Location location, TimeSpan time ,string details)
        {
            Incident incident = new Incident();
            incident.IncidentNo = "INC";
            incident.LocationID = location.LocationID;
            incident.ReportedTime = time;
            incident.Description = details;
            SaveIncident(incident);
        }

        private void SaveIncident(Incident incident)
        {
            db.Entry(incident).State = System.Data.Entity.EntityState.Added;
            db.SaveChanges();
        }


    }
}
