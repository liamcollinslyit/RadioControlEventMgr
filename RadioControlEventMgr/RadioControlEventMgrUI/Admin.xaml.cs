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
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin : Page
    {
        public User loggedInUser = new User();

        enum DBOperation
        {
            Add,
            Edit
        }

        RadioDBEntities db = new RadioDBEntities("metadata=res://*/RadioModel.csdl|res://*/RadioModel.ssdl|res://*/RadioModel.msl;provider=System.Data.SqlClient;provider connection string='data source=192.168.60.132" +
                                                 ";initial catalog=RadioDB;user id=radiouser;password=password;pooling=False;MultipleActiveResultSets=True;App=EntityFramework'");

        List<User> users = new List<User>();
        List<Log> logs = new List<Log>();
        List<AccessLevel> accessLevels = new List<AccessLevel>();

        User selectedUser = new User();
        DBOperation dbOperation = new DBOperation();

        public Admin()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUserList();
            RefreshLogsList();
            RefreshAccessLevels();
        }

        // ---------------------------------------------------------------------------------------//
        // List box Selection Change
        // ---------------------------------------------------------------------------------------//

        private void lstUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (lstUserList.SelectedIndex >= 0)
            {
                submenuEditUser.IsEnabled = true;
                submenuDeleteUser.IsEnabled = true;
                selectedUser = users.ElementAt(lstUserList.SelectedIndex);
                if (dbOperation == DBOperation.Edit)
                {
                    UpdateUserDetails();
                }
            }
            else
            {
                submenuEditUser.IsEnabled = false;
                submenuDeleteUser.IsEnabled = false;
            }
        }

        // ---------------------------------------------------------------------------------------//
        // User Context Click Events
        // ---------------------------------------------------------------------------------------//

        private void submenuAddNewUser_Click(object sender, RoutedEventArgs e)
        {
            dbOperation = DBOperation.Add;
            ClearUserDetails();
            stkUserDetails.Visibility = Visibility.Visible;
            CreateLogEntry("User opened add user panel", loggedInUser.UserId);
        }

        private void submenuEditUser_Click(object sender, RoutedEventArgs e)
        {
            dbOperation = DBOperation.Edit;
            UpdateUserDetails();
            stkUserDetails.Visibility = Visibility.Visible;
            CreateLogEntry("User opened edit user panel", loggedInUser.UserId);
        }

        private void submenuDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            db.Users.RemoveRange(db.Users.Where(t => t.UserId == selectedUser.UserId));

            int saveSuccess = SaveDBChanges();
            if (saveSuccess == 1)
            {
                MessageBox.Show($"User {selectedUser.Username} successfully deleted from system", "User Administration", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshUserList();
                CreateLogEntry($"User {selectedUser.Username} successfully deleted from system ", loggedInUser.UserId);
            }
            else
            {
                MessageBox.Show($"Problem deleting user {selectedUser.Username}, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem deleting user {selectedUser.Username}", loggedInUser.UserId);
            }

            ClearUserDetails();
        }

        // ---------------------------------------------------------------------------------------//
        // User Details Panel Click Events
        // ---------------------------------------------------------------------------------------//

        private void btnEditUpdate_Click(object sender, RoutedEventArgs e)
        {
            string username = txtEditUsername.Text.Trim();
            string password = txtEditPassword.Text.Trim();
            string forename = txtEditForename.Text.Trim();
            string surname = txtEditSurname.Text.Trim();
            AccessLevel accessLevel = (AccessLevel)cboEditUserAccess.SelectedItem;

            if (ValidateUserDetails())
            {
                if (dbOperation == DBOperation.Add)
                {
                    CreateUser(username, password, forename, surname, accessLevel);
                }
                if (dbOperation == DBOperation.Edit)
                {
                    UpdateUser(username, password, forename, surname, accessLevel);
                }
            }            
        }


        private void CreateUser(string username, string password, string forename, string surname, AccessLevel accessLevel)
        {
            User user = new User();
            user.Username = username;
            user.Password = password;
            user.Forename = forename;
            user.Surname = surname;
            user.LevelID = accessLevel.LevelID;
            db.Entry(user).State = System.Data.Entity.EntityState.Added;

            int saveSuccess = SaveDBChanges();
            if (saveSuccess == 1)
            {
                MessageBox.Show("User successfully added to system", "User Administration", MessageBoxButton.OK, MessageBoxImage.Information);
                CreateLogEntry($"User {user.Username} successfully added to system", loggedInUser.UserId);
                RefreshUserList();
                ClearUserDetails();
            }
            else
            {
                MessageBox.Show("Problem creating user record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error:  Problem creating user {user.Username}", loggedInUser.UserId);

            }

        }

        private void UpdateUser(string username, string password, string forename, string surname, AccessLevel accessLevel)
        {
            try
            {
                foreach (var user in db.Users.Where(t => t.UserId == selectedUser.UserId))
                {
                    user.Username = username;
                    user.Password = password;
                    user.Forename = forename;
                    user.Surname = surname;
                }

            }
            catch (EntityException)
            {

                DBConnectionError();
            }

            int saveSuccess = SaveDBChanges();
            if (saveSuccess == 1)
            {
                MessageBox.Show("User successfully updated in system", "User Administration", MessageBoxButton.OK, MessageBoxImage.Information);
                CreateLogEntry($"User {selectedUser.Username} successfully updated in system", loggedInUser.UserId);
                RefreshUserList();
                ClearUserDetails();
            }
            else
            {
                MessageBox.Show("Problem updating user record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error:  Problem updating user {selectedUser.Username}", loggedInUser.UserId);
            }
        }

        // ---------------------------------------------------------------------------------------//
        // User Details Validations And Changed Validation
        // ---------------------------------------------------------------------------------------//

        private bool ValidateUserDetails()
        {
            bool valid = true;
            string errorMessage = "User Detail's Error:";

            if (txtEditUsername.Text.Length < 1 || txtEditUsername.Text.Length > 30)
            {
                txtEditUsername.Background = Brushes.LightCoral;
                errorMessage += Environment.NewLine + "Username must be between 1 and 30 charatcers.";
                valid = false;
            }
            if (txtEditPassword.Text.Length < 1 || txtEditPassword.Text.Length > 30)
            {
                txtEditPassword.Background = Brushes.LightCoral;
                errorMessage += Environment.NewLine + "Password must be between 1 and 30 charatcers.";
                valid = false;
            }
            if (txtEditForename.Text.Length < 1 || txtEditForename.Text.Length > 30)
            {
                txtEditForename.Background = Brushes.LightCoral;
                errorMessage += Environment.NewLine + "Forename must be between 1 and 30 charatcers.";
                valid = false;
            }
            if (txtEditSurname.Text.Length < 1 || txtEditSurname.Text.Length > 30)
            {
                txtEditSurname.Background = Brushes.LightCoral;
                errorMessage += Environment.NewLine + "Surname must be between 1 and 30 charatcers.";
                valid = false;
            }
            if (cboEditUserAccess.SelectedIndex < 0)
            {
                errorMessage += Environment.NewLine + "User's access level must be selected.";
                valid = false;
            }

            try
            {
                foreach (var user in db.Users.Where(t => t.Username == txtEditUsername.Text))
                {
                    if (dbOperation == DBOperation.Edit && selectedUser.UserId == user.UserId)
                    {
                        break;
                    }
                    txtEditUsername.Background = Brushes.LightCoral;
                    errorMessage += Environment.NewLine + "Username already exists in system, it must be unique.";
                    valid = false;
                }
            }
            catch (EntityException)
            {
                DBConnectionError();
            }

            if (!valid)
            {
                MessageBox.Show(errorMessage, "User Detail's Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                CreateLogEntry($"Error: Invalid user details entered - {errorMessage}", loggedInUser.UserId);
            }

            return valid;
        }

        private void txtBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Background = Brushes.White;

            if (
                selectedUser.Username == txtEditUsername.Text &&
                selectedUser.Password == txtEditPassword.Text &&
                selectedUser.Forename == txtEditForename.Text &&
                selectedUser.Surname == txtEditSurname.Text &&
                dbOperation == DBOperation.Edit)
            {
                btnEditUpdate.IsEnabled = false;
            }
            else
            {
                btnEditUpdate.IsEnabled = true;
            }
        }

        private void cboEditUserAccess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccessLevel accessLevel = (AccessLevel)cboEditUserAccess.SelectedItem;

            if (selectedUser.AccessLevel == accessLevel && dbOperation == DBOperation.Edit)
            {
                btnEditUpdate.IsEnabled = false;
            }
            else
            {
                btnEditUpdate.IsEnabled = true;
            }
        }

        // ---------------------------------------------------------------------------------------//
        // Clearing and Setting User details
        // ---------------------------------------------------------------------------------------//

        private void ClearUserDetails()
        {
            txtEditUsername.Text = "";
            txtEditPassword.Text = "";
            txtEditForename.Text = "";
            txtEditSurname.Text = "";
            cboEditUserAccess.SelectedIndex = -1;
            txtEditUsername.Background = Brushes.White;
            txtEditPassword.Background = Brushes.White;
            txtEditForename.Background = Brushes.White;
            txtEditSurname.Background = Brushes.White;
            stkUserDetails.Visibility = Visibility.Collapsed;
        }

        private void UpdateUserDetails()
        {
            txtEditUsername.Text = selectedUser.Username;
            txtEditPassword.Text = selectedUser.Password;
            txtEditForename.Text = selectedUser.Forename;
            txtEditSurname.Text = selectedUser.Surname;
            cboEditUserAccess.SelectedItem = selectedUser.AccessLevel;
        }

        // ---------------------------------------------------------------------------------------//
        // Refreshing Data In Tables And Combo Boxes
        // ---------------------------------------------------------------------------------------//

        private void RefreshUserList()
        {
            try
            {
                lstUserList.ItemsSource = users;
                users.Clear();
                foreach (var user in db.Users.Where(t => t.UserId != 0))
                {
                    users.Add(user);
                }
                lstUserList.Items.Refresh();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        private void RefreshLogsList()
        {
            try
            {
                lstLogsList.ItemsSource = logs;
                logs.Clear();
                foreach (var log in db.Logs)
                {
                    logs.Add(log);
                }
                lstUserList.Items.Refresh();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        private void RefreshAccessLevels()
        {
            try
            {
                cboEditUserAccess.ItemsSource = accessLevels;
                accessLevels.Clear();
                foreach (var accessLevel in db.AccessLevels)
                {
                    accessLevels.Add(accessLevel);
                }
                cboEditUserAccess.Items.Refresh();
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
            RefreshLogsList();
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
