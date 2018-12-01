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
    // C# code for login screen

    public partial class Login : Window
    {
        RadioDBEntities db = new RadioDBEntities("metadata=res://*/RadioModel.csdl|res://*/RadioModel.ssdl|res://*/RadioModel.msl;provider=System.Data.SqlClient;provider connection string='data source=192.168.60.132" +
                                                 ";initial catalog=RadioDB;user id=radiouser;password=password;pooling=False;MultipleActiveResultSets=True;App=EntityFramework'");
        int loginCounter = 1;


        public Login()
        {
            InitializeComponent();
        }

        // ---------------------------------------------------------------------------------------//
        // Log In Window Click Events
        // ---------------------------------------------------------------------------------------//

        // Enter button - validate user and initilise correct dashboard.
        private void btnLoginEnter_Click(object sender, RoutedEventArgs e)
        {
            string enteredUsername = tbxUsername.Text;
            string enteredPassword = tbxPassword.Password;

            User validatedUser = ValidateUser(enteredUsername, enteredPassword);

            if (validatedUser.UserId > 0) LoginSuccess(validatedUser);
            else LoginFailed(enteredUsername);
        }

        //Exit button - close application
        private void btnLoginExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            CreateLogEntry("Login screen closed by user using exit button", 0);
            Environment.Exit(0);
        }

        // ---------------------------------------------------------------------------------------//
        // User Validation And Login Events
        // ---------------------------------------------------------------------------------------//

        private User ValidateUser(string enteredUsername , string enteredPassword)
        {
            User validatedUser = new User();

            try
            {
                foreach (var user in db.Users.Where(t => t.Username == enteredUsername && t.Password == enteredPassword))
                {
                    validatedUser = user;
                }
            }
            catch (EntityException)
            {
                DBConnectionError();
            }

            return validatedUser;
        }

        private void LoginSuccess(User validatedUser)
        {
            Dashboard dashboard = new Dashboard();
            dashboard.Owner = this;
            dashboard.loggedInUser = validatedUser;
            this.Hide();
            CreateLogEntry($"User {validatedUser.Username} login successful: Access Level - {validatedUser.AccessLevel.AccessLevelName}", validatedUser.UserId);
            dashboard.ShowDialog();
        }

        private void LoginFailed(string enteredUsername)
        {
            CreateLogEntry($"User failed to login with username {enteredUsername}, invalid username/password: Attempt {loginCounter}", 0);

            if (loginCounter < 3)
            {
                lblLoginHeading.Content = $"Login attempt {loginCounter} of 3 failed - Please try again";
            }
            else
            {
                lblLoginHeading.Content = $"Login attempt 3 of 3 failed - relaunch application to try again";                
                lblLoginHeading.FontSize = 16;
                CreateLogEntry($"Max number of attempts, user login locked", 0);
            }

            lblLoginHeading.Foreground = Brushes.Red;
            tbxUsername.Background = Brushes.LightCoral;
            tbxPassword.Background = Brushes.LightCoral;
            btnLoginEnter.IsEnabled = false;
            loginCounter++;
        }

        // ---------------------------------------------------------------------------------------//
        // Reset Textboxes And Button After Failed Login
        // ---------------------------------------------------------------------------------------//

        private void tbxUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbxUsername.Background = Brushes.White;
            tbxPassword.Background = Brushes.White;
            btnLoginEnter.IsEnabled = true;
        }

        private void tbxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            tbxUsername.Background = Brushes.White;
            tbxPassword.Background = Brushes.White;
            btnLoginEnter.IsEnabled = true;
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
            this.Close();
            Environment.Exit(0);
        }

    }
}
