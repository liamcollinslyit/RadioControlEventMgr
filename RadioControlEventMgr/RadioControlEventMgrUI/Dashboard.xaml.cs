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
    /// <summary>
    /// Interaction logic for dashboard screen - Situation, Logs, Map, Admin and Exit buttons
    /// </summary>
    public partial class Dashboard : Window
    {
        // Currently logged in user, passed from login screen
        public User loggedInUser = new User();

        // DB Connection string
        RadioDBEntities db = new RadioDBEntities("metadata=res://*/RadioModel.csdl|res://*/RadioModel.ssdl|res://*/RadioModel.msl;provider=System.Data.SqlClient;provider connection string='data source=192.168.60.132" +
                                         ";initial catalog=RadioDB;user id=radiouser;password=password;pooling=False;MultipleActiveResultSets=True;App=EntityFramework'");

        public Dashboard()
        {
            InitializeComponent();
        }

        // When window is loaded, check user's access level to display appropiate buttons
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckUserAccess(loggedInUser);
        }

        // ---------------------------------------------------------------------------------------//
        // Dashboard Button Click Events
        // ---------------------------------------------------------------------------------------//

        // Situation button - Open Situation page in frame, pass logged in user and note in log
        private void btnSituation_Click(object sender, RoutedEventArgs e)
        {
            Situation situation = new Situation();
            frmMain.Navigate(situation);
            situation.loggedInUser = loggedInUser;
            CreateLogEntry("User opened Situation screen", loggedInUser.UserId);
        }

        // Logs button - Open Logs page in frame
        private void btnLogs_Click(object sender, RoutedEventArgs e)
        {
            Logs logs = new Logs();
            frmMain.Navigate(logs);
            logs.loggedInUser = loggedInUser;
            CreateLogEntry("User opened Logs screen", loggedInUser.UserId);
        }

        // Map button - Open Map page in frame, pass logged in user and note in log
        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            Map map = new Map();
            frmMain.Navigate(map);
            map.loggedInUser = loggedInUser;
            CreateLogEntry("User opened Map screen", loggedInUser.UserId);
        }

        // Admin button - Open Admin page in frame, pass logged in user and note in log
        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            Admin admin = new Admin();
            frmMain.Navigate(admin);
            admin.loggedInUser = loggedInUser;
            CreateLogEntry("User opened Admin screen", loggedInUser.UserId);
        }

        //Exit button - close application and note in log
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(0);
            CreateLogEntry("User closed application", loggedInUser.UserId);
        }

        // ---------------------------------------------------------------------------------------//
        // Check User Access
        // ---------------------------------------------------------------------------------------//

        // Check logged in user's access level and display appropaite buttons
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
