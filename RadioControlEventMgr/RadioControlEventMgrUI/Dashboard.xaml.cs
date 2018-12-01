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
using System.Windows.Shapes;

namespace RadioControlEventMgrUI
{

    // C# code for dashboard

    public partial class Dashboard : Window
    {
        public User loggedInUser = new User();

        RadioDBEntities db = new RadioDBEntities("metadata=res://*/RadioModel.csdl|res://*/RadioModel.ssdl|res://*/RadioModel.msl;provider=System.Data.SqlClient;provider connection string='data source=192.168.60.132" +
                                         ";initial catalog=RadioDB;user id=radiouser;password=password;pooling=False;MultipleActiveResultSets=True;App=EntityFramework'");

        public Dashboard()
        {
            InitializeComponent();
        }

        // Situation button - Open Situation page in frame
        private void btnSituation_Click(object sender, RoutedEventArgs e)
        {
            Situation situation = new Situation();
            frmMain.Navigate(situation);
        }

        // Logs button - Open Logs page in frame
        private void btnLogs_Click(object sender, RoutedEventArgs e)
        {
            Logs logs = new Logs();
            frmMain.Navigate(logs);
        }

        // Map button - Open Map page in frame
        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            Map map = new Map();
            frmMain.Navigate(map);
        }

        // Admin button - Open Admin page in frame
        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            Admin admin = new Admin();
            admin.loggedInUser = loggedInUser;
            frmMain.Navigate(admin);
            CreateLogEntry("User opened Admin screen", loggedInUser.UserId);
        }

        //Exit button - close application
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(0);
        }

        private void CheckUserAccess(User user)
        {
            if (user.LevelID == 3)
            {
                btnSituation.Visibility = Visibility.Visible;
                btnMap.Visibility = Visibility.Visible;
                btnAdmin.Visibility = Visibility.Visible;
            }
            if (user.LevelID == 2)
            {
                btnSituation.Visibility = Visibility.Visible;
                btnMap.Visibility = Visibility.Visible;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckUserAccess(loggedInUser);
        }


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
