using Microsoft.Win32;
using RadioLibrary;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.IO;
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
    /// Interaction logic for admin screen - Event Data, System Users and System Logs
    /// </summary>
    public partial class Admin : Page
    {
        // Currently logged in user, passed from dashboard
        public User loggedInUser = new User();

        // Enum for current operation of stackpanel's, controlled by context menu
        enum DBOperation
        {
            Add,
            Edit
        }

        //Db Connection string
        RadioDBEntities db = new RadioDBEntities("metadata=res://*/RadioModel.csdl|res://*/RadioModel.ssdl|res://*/RadioModel.msl;provider=System.Data.SqlClient;provider connection string='data source=192.168.60.132" +
                                                 ";initial catalog=RadioDB;user id=radiouser;password=password;pooling=False;MultipleActiveResultSets=True;App=EntityFramework'");
        
        // List for storing information read from DB
        List<User> users = new List<User>();
        List<Crew> crews = new List<Crew>();
        List<CrewType> crewTypes = new List<CrewType>();
        List<Location> locations = new List<Location>();
        List<Log> logs = new List<Log>();
        List<AccessLevel> accessLevels = new List<AccessLevel>();

        // Variables for selected list item 
        User selectedUser = new User();
        Crew selectedCrew = new Crew();
        CrewType selectedCrewType = new CrewType();
        Location selectedLocation = new Location();

        // DB Operation for each type of stackpanel
        DBOperation dbUserOperation = new DBOperation();
        DBOperation dbCrewOperation = new DBOperation();
        DBOperation dbCrewTypeOperation = new DBOperation();
        DBOperation dbLocationOperation = new DBOperation();

        public Admin()
        {
            InitializeComponent();
        }

        // When page is loaded, get info from DB
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUserList();
            RefreshLogsList();
            RefreshAccessLevels();
            RefreshCrew();
            RefreshCrewTypes();
            RefreshLocation();
        }

        // ---------------------------------------------------------------------------------------//
        // List box Selection Change
        // ---------------------------------------------------------------------------------------//

        // User selection changed, set selected user and enable context menu options
        private void lstUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (lstUserList.SelectedIndex >= 0)
            {
                submenuEditUser.IsEnabled = true;
                submenuDeleteUser.IsEnabled = true;
                selectedUser = users.ElementAt(lstUserList.SelectedIndex);

                // If current operation is edit then show user details in panel
                if (dbUserOperation == DBOperation.Edit)
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

        // Crew selection changed, set selected Crew and enable context menu options
        private void lstCrewList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCrewList.SelectedIndex >= 0)
            {
                submenuEditCrew.IsEnabled = true;
                submenuDeleteCrew.IsEnabled = true;
                selectedCrew = crews.ElementAt(lstCrewList.SelectedIndex);

                // If current operation is edit then show crew details in panel
                if (dbCrewOperation == DBOperation.Edit)
                {
                    UpdateCrewDetails();
                }
            }
            else
            {
                submenuEditCrew.IsEnabled = false;
                submenuDeleteCrew.IsEnabled = false;
            }
        }

        // Crew Type selection changed, set selected Crew Type and enable context menu options
        private void lstTypesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstTypesList.SelectedIndex >= 0)
            {
                submenuEditCrewType.IsEnabled = true;
                submenuDeleteCrewType.IsEnabled = true;
                selectedCrewType = crewTypes.ElementAt(lstTypesList.SelectedIndex);


                if (dbCrewTypeOperation == DBOperation.Edit)
                {
                    UpdateCrewTypeDetails();
                }
            }
            else
            {
                submenuEditCrewType.IsEnabled = false;
                submenuDeleteCrewType.IsEnabled = false;
            }
        }

        // Location selection changed, set selected location and enable context menu options
        private void lstLocationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstLocationList.SelectedIndex >= 0)
            {
                submenuEditLocation.IsEnabled = true;
                submenuDeleteLocation.IsEnabled = true;
                selectedLocation = locations.ElementAt(lstLocationList.SelectedIndex);

                // If current operation is edit then show location details in panel
                if (dbLocationOperation == DBOperation.Edit)
                {
                    UpdateLocationDetails();
                }
            }
            else
            {
                submenuEditLocation.IsEnabled = false;
                submenuDeleteLocation.IsEnabled = false;
            }
        }

        // ---------------------------------------------------------------------------------------//
        // User Context Click Events
        // ---------------------------------------------------------------------------------------//

        // Add user click event - add user to DB, clear + hide panel and note in system log
        private void submenuAddNewUser_Click(object sender, RoutedEventArgs e)
        {
            dbUserOperation = DBOperation.Add;
            ClearUserDetails();
            stkUserDetails.Visibility = Visibility.Visible;
            CreateLogEntry("User opened add user panel", loggedInUser.UserId);
        }

        // Edit user click event - Edit user in DB, clear + hide panel and note in system log
        private void submenuEditUser_Click(object sender, RoutedEventArgs e)
        {
            dbUserOperation = DBOperation.Edit;
            UpdateUserDetails();
            stkUserDetails.Visibility = Visibility.Visible;
            CreateLogEntry("User opened edit user panel", loggedInUser.UserId);
        }

        // Delecte user click event - Delete user from DB, clear panel, notify user of success/failure and note in system log
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

        // Update button click event - validate details then perform the selected operation
        private void btnEditUpdate_Click(object sender, RoutedEventArgs e)
        {
            string username = txtEditUsername.Text.Trim();
            string password = txtEditPassword.Text.Trim();
            string forename = txtEditForename.Text.Trim();
            string surname = txtEditSurname.Text.Trim();
            AccessLevel accessLevel = (AccessLevel)cboEditUserAccess.SelectedItem;

            if (ValidateUserDetails())
            {
                if (dbUserOperation == DBOperation.Add)
                {
                    CreateUser(username, password, forename, surname, accessLevel);
                }
                if (dbUserOperation == DBOperation.Edit)
                {
                    UpdateUser(username, password, forename, surname, accessLevel);
                }
            }            
        }

        // Function to create user in the DB with given data and note in system logs.
        private void CreateUser(string username, string password, string forename, string surname, AccessLevel accessLevel)
        {
            // Create new user with supplied details and add to DB
            User user = new User();
            user.Username = username;
            user.Password = password;
            user.Forename = forename;
            user.Surname = surname;
            user.LevelID = accessLevel.LevelID;
            db.Entry(user).State = System.Data.Entity.EntityState.Added;

            // If save is successful/failed show message and note to system log
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

        // Function to update the selected user with supplied details in DB and note in system logs.
        private void UpdateUser(string username, string password, string forename, string surname, AccessLevel accessLevel)
        {
            try
            {
                // Find the selected user in the DB and change to supplied details
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

            // If save is successful/failed show message and note to system log
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

        // Validate user details supplied in the panel, if invlaid show usful error message for each item and color text box red.
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
                // Check if username already exist in DB (If editing, break if matches selected user)
                foreach (var user in db.Users.Where(t => t.Username == txtEditUsername.Text))
                {
                    if (dbUserOperation == DBOperation.Edit && selectedUser.UserId == user.UserId)
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

            // If invlaid show the cumulative error message
            if (!valid)
            {
                MessageBox.Show(errorMessage, "User Detail's Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                CreateLogEntry($"Error: Invalid user details entered - {errorMessage}", loggedInUser.UserId);
            }

            return valid;
        }

        // User panel textbox change - reset color to white
        // Check entered details are different then selected user details in the DB and if so enable update button
        private void txtBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Background = Brushes.White;

            if (
                selectedUser.Username == txtEditUsername.Text &&
                selectedUser.Password == txtEditPassword.Text &&
                selectedUser.Forename == txtEditForename.Text &&
                selectedUser.Surname == txtEditSurname.Text &&
                dbUserOperation == DBOperation.Edit)
            {
                btnEditUpdate.IsEnabled = false;
            }
            else
            {
                btnEditUpdate.IsEnabled = true;
            }
        }

        // Check selected details are different then selected user details in the DB and if so enable update button
        private void cboEditUserAccess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccessLevel accessLevel = (AccessLevel)cboEditUserAccess.SelectedItem;

            if (selectedUser.AccessLevel == accessLevel && dbUserOperation == DBOperation.Edit)
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

        // Clear User details in panel
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

        // Update user details in panel
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

        // Refresh User details list from DB, filter if required and set information message in filter panel
        private void RefreshUserList(string filter = null)
        {
            try
            {
                int totalUsers = 0;
                int level1 = 0;
                int level2 = 0;
                int level3 = 0;

                users.Clear();
                if (filter == null)
                {
                    // Dont read INVLAID_USER at ID 0
                    foreach (var user in db.Users.Where(t => t.UserId != 0))
                    {
                        users.Add(user);
                        totalUsers++;
                        if (user.LevelID == 1) level1++;
                        if (user.LevelID == 2) level2++;
                        if (user.LevelID == 3) level3++;
                    }
                    tbxUsersStats.Text = $"Displaying all {totalUsers} users. View-Only: {level1}, Controller: {level2}, Superuser: {level3}";
                }
                else
                {
                    // return list of user that have filter text in any field
                    foreach (var user in db.Users.Where(t => t.UserId != 0 && ( t.Username.Contains(filter) || t.Password.Contains(filter) || t.Forename.Contains(filter) || t.Surname.Contains(filter) )))
                    {
                        users.Add(user);
                        totalUsers++;
                        if (user.LevelID == 1) level1++;
                        if (user.LevelID == 2) level2++;
                        if (user.LevelID == 3) level3++;
                    }
                    tbxUsersStats.Text = $"Displaying {totalUsers} users using filter \"{filter}\". View-Only: {level1}, Controller: {level2}, Superuser: {level3}";
                }
                lstUserList.ItemsSource = users;
                lstUserList.Items.Refresh();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        // Refresh Log details list from DB, filter if required and set information message in filter panel
        private void RefreshLogsList(string filter = null)
        {
            try
            {
                int totalLogs = 0;
                logs.Clear();
                if (filter == null)
                {
                    foreach (var log in db.Logs)
                    {
                        logs.Add(log);
                        totalLogs++;
                    }
                    tbxLogStats.Text = $"Displaying all {totalLogs} log messages.";
                }
                else
                {
                    // return list of log messages that have filter text in any field
                    foreach (var log in db.Logs.Where(t=> t.Date.ToString().Contains(filter) || t.Event.Contains(filter) || t.User.Username.Contains(filter)))
                    {
                        logs.Add(log);
                        totalLogs++;
                    }
                    tbxLogStats.Text = $"Displaying {totalLogs} log messages using filter \"{filter}\".";
                }
                logs = logs.OrderByDescending(t => t.Date).ToList();
                lstLogsList.ItemsSource = logs;
                lstUserList.Items.Refresh();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        // Refresh access level details list from DB
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

        // Refresh crew details list from DB and set crew label with number
        private void RefreshCrew()
        {
            try
            {
                int crewNum = 0;
                crews.Clear();
                // Dont read Control at ID 0
                foreach (var crew in db.Crews.Where(t=> t.CallSignID > 0))
                {
                    crews.Add(crew);
                    crewNum++;
                }
                lblCrew.Content = $"Crew - {crewNum}";
                lstCrewList.ItemsSource = crews;
                lstCrewList.Items.Refresh();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        // Refresh crew types details list from DB and set crew types label with number
        private void RefreshCrewTypes()
        {
            try
            {
                int crewTypeNum = 0;
                crewTypes.Clear();
                // Dont read Control at ID 0
                foreach (var crewType in db.CrewTypes.Where(t => t.CrewTypeID > 0))
                {
                    crewTypes.Add(crewType);
                    crewTypeNum++;
                }
                lblCrewType.Content = $"Crew Types - {crewTypeNum}";
                cboCrewType.ItemsSource = crewTypes;
                lstTypesList.ItemsSource = crewTypes;
                lstTypesList.Items.Refresh();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        // Refresh location details list from DB and set location label with number
        private void RefreshLocation()
        {
            try
            {
                int locationNum = 0;
                locations.Clear();
                // Dont read Control at ID 0
                foreach (var location in db.Locations.Where(t=> t.LocationID>0))
                {
                    locations.Add(location);
                    locationNum++;
                }
                lblLocation.Content = $"Locations - {locationNum}";
                cboCrewLocation.ItemsSource = locations;
                lstLocationList.ItemsSource = locations;
                lstLocationList.Items.Refresh();
            }
            catch (EntityException)
            {
                DBConnectionError();
            }
        }

        // ---------------------------------------------------------------------------------------//
        // Filter + Clear  and Clear Logs Button Click Events
        // ---------------------------------------------------------------------------------------//

        // Applied filter to users list using entered text
        private void btnFilterUsers_Click(object sender, RoutedEventArgs e)
        {
            RefreshUserList(txtFilterUsers.Text);
        }

        // Remove filter from users list and clear text box
        private void btnClearFilterUsers_Click(object sender, RoutedEventArgs e)
        {
            RefreshUserList();
            tbxUsersStats.Text = "";
        }

        // Applied filter to system logs list using entered text
        private void btnFilterLogs_Click(object sender, RoutedEventArgs e)
        {
            RefreshLogsList(txtFilterLogs.Text);
        }

        // Remove filter from system logs list and clear text box
        private void btnClearFilterLogs_Click(object sender, RoutedEventArgs e)
        {
            RefreshLogsList();
            tbxLogStats.Text = "";
        }

        // Clear log message from database for clean start
        private void btnClearAdminLogs_Click(object sender, RoutedEventArgs e)
        {
            // User confirm proceed
            MessageBoxResult result = MessageBox.Show("This will clear all system lof informaion from the database. Continue ?", "Clear Log", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Truncate table using SQL command
                    db.Database.ExecuteSqlCommand("TRUNCATE TABLE[RadioDB].[dbo].[Log]");
                    RefreshLogsList();
                }
                catch (EntityException)
                {
                    DBConnectionError();
                }
            }
        }

        // ---------------------------------------------------------------------------------------//
        // Event Tab Context Click Events
        // ---------------------------------------------------------------------------------------//

        // Add crew click event - set DB operation to add and enable update
        private void submenuAddNewCrew_Click(object sender, RoutedEventArgs e)
        {
            dbCrewOperation = DBOperation.Add;
            btnCrewUpdate.IsEnabled = true;
        }

        // Edit crew click event - set DB operation to add and enable update
        private void submenuEditCrew_Click(object sender, RoutedEventArgs e)
        {
            dbCrewOperation = DBOperation.Edit;
            btnCrewUpdate.IsEnabled = false;
            UpdateCrewDetails();
        }

        // Delete crew click event - Delete crew from DB, clear panel and note in system log
        private void submenuDeleteCrew_Click(object sender, RoutedEventArgs e)
        {
            db.Crews.RemoveRange(db.Crews.Where(t => t.CallSignID == selectedCrew.CallSignID));

            int saveSuccess = SaveDBChanges();
            if (saveSuccess == 1)
            {
                RefreshCrew();
                CreateLogEntry($"Crew {selectedCrew.CallSign} successfully deleted from system ", loggedInUser.UserId);
            }
            else
            {
                // If error saving display error message
                MessageBox.Show($"Problem deleting crew {selectedCrew.CallSign}, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem deleting crew {selectedCrew.CallSign}", loggedInUser.UserId);
            }

            txtCallSign.Text = "";
            cboCrewType.SelectedIndex = -1;
            cboCrewLocation.SelectedIndex = -1;
            dbCrewOperation = DBOperation.Add;
        }

        // Add crew type click event - set DB operation to add and enable update
        private void submenuAddCrewType_Click(object sender, RoutedEventArgs e)
        {
            dbCrewTypeOperation = DBOperation.Add;
            btnTypeUpdate.IsEnabled = true;
        }

        // Edit crew type click event - set DB operation to add and enable update
        private void submenuEditCrewType_Click(object sender, RoutedEventArgs e)
        {
            dbCrewTypeOperation = DBOperation.Edit;
            btnTypeUpdate.IsEnabled = false;
            UpdateCrewTypeDetails();
        }

        // Delete crew type click event - Delete crew from DB, clear panel and note in system log
        private void submenuDeleteCrewType_Click(object sender, RoutedEventArgs e)
        {
            db.CrewTypes.RemoveRange(db.CrewTypes.Where(t => t.CrewTypeID == selectedCrewType.CrewTypeID));

            int saveSuccess = SaveDBChanges();
            if (saveSuccess == 1)
            {
                RefreshCrewTypes();
                CreateLogEntry($"Crew type {selectedCrewType.CrewTypeName} successfully deleted from system ", loggedInUser.UserId);
            }
            else
            {
                // If error saving display error message
                MessageBox.Show($"Problem deleting crew type {selectedCrewType.CrewTypeName}, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem deleting crew {selectedCrewType.CrewTypeName}", loggedInUser.UserId);
            }

            txtTypeName.Text = "";
            dbCrewTypeOperation = DBOperation.Add;
        }

        // Add location click event - set DB operation to add and enable update
        private void submenuAddNewLocation_Click(object sender, RoutedEventArgs e)
        {
            dbLocationOperation = DBOperation.Add;
            btnLocationUpdate.IsEnabled = true;
        }

        // Edit location click event - set DB operation to add and enable update
        private void submenuEditLocation_Click(object sender, RoutedEventArgs e)
        {
            dbLocationOperation = DBOperation.Edit;
            btnLocationUpdate.IsEnabled = false;
            UpdateLocationDetails();
        }

        // Delete location click event - Delete crew from DB, clear panel and note in system log
        private void submenuDeleteLocation_Click(object sender, RoutedEventArgs e)
        {
            db.Locations.RemoveRange(db.Locations.Where(t => t.LocationID == selectedLocation.LocationID));

            int saveSuccess = SaveDBChanges();
            if (saveSuccess == 1)
            {
                RefreshLocation();
                CreateLogEntry($"Location {selectedLocation.LocationName} successfully deleted from system ", loggedInUser.UserId);
            }
            else
            {
                // If error saving display error message
                MessageBox.Show($"Problem deleting location {selectedLocation.LocationName}, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem deleting location {selectedLocation.LocationName}", loggedInUser.UserId);
            }

            txtLocationName.Text = "";
            dbLocationOperation = DBOperation.Add;
        }

        // ---------------------------------------------------------------------------------------//
        // Event Tab Panel Click Events
        // ---------------------------------------------------------------------------------------//

        // Update crew button click event - validate details are entered, perform the selected operation and clear panel
        private void btnCrewUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (txtCallSign.Text.Length > 0 || cboCrewType.SelectedIndex >= 0 || cboCrewLocation.SelectedIndex >= 0)
            {
                string callSign = txtCallSign.Text.Trim();
                CrewType crewType = (CrewType)cboCrewType.SelectedItem;
                Location location = (Location)cboCrewLocation.SelectedItem;
                if (dbCrewOperation == DBOperation.Add)
                {
                    CreateCrew(callSign, 1, crewType, location);
                }
                if (dbCrewOperation == DBOperation.Edit)
                {
                    UpdateCrew(callSign, crewType, location);
                }
            }
            else
            {
                // If details are missing then show error message and note in system log
                MessageBox.Show("Call Sign, Crew Type and Crew Location must be selected", "Save to Database", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Invalid crew details entered", loggedInUser.UserId);
            }

            txtCallSign.Text = "";
            cboCrewType.SelectedIndex = -1;
            cboCrewLocation.SelectedIndex = -1;
            dbCrewOperation = DBOperation.Add;

        }

        // Function to create crew in the DB with given data and note in system logs.
        private void CreateCrew(string callSign, int statusID, CrewType crewType, Location location)
        {
            // Create new crew with supplied details and add to DB          
            Crew crew = new Crew();
            crew.CallSign = callSign;
            crew.StatusID = statusID;
            crew.CrewTypeID = crewType.CrewTypeID;
            crew.LocationID = location.LocationID;
            db.Entry(crew).State = System.Data.Entity.EntityState.Added;

            int saveSuccess = SaveDBChanges();
            if (saveSuccess == 1)
            {
                CreateLogEntry($"Crew {crew.CallSign} successfully added to database", loggedInUser.UserId);
                RefreshCrew();
            }
            else
            { 
                // If save is successful/failed show message and note to system log
                MessageBox.Show("Problem creating crew record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem creating crew {crew.CallSign}", loggedInUser.UserId);
            }
        }

        // Function to update the selected crew with supplied details in DB and note in system logs.
        private void UpdateCrew(string callSign, CrewType crewType, Location location)
        {
            try
            {
                // Find the selected crew in the DB and change to supplied details
                foreach (var crew in db.Crews.Where(t => t.CallSignID == selectedCrew.CallSignID))
                {
                    crew.CallSign = callSign;
                    crew.CrewTypeID = crewType.CrewTypeID;
                    crew.LocationID = location.LocationID;
                }
            }
            catch (EntityException)
            {

                DBConnectionError();
            }

            int saveSuccess = SaveDBChanges();
            if (saveSuccess == 1)
            {
                CreateLogEntry($"User updated crew details: {selectedCrew.CallSign} with crew type {selectedCrew.CrewType.CrewTypeName} and Location {selectedCrew.Location.LocationName}", loggedInUser.UserId);
                RefreshCrew();
            }
            else
            {
                // If save is successful/failed show message and note to system log
                MessageBox.Show("Problem updating crew record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem updating crew {selectedCrew.CallSign}", loggedInUser.UserId);
            }
        }

        // Update crew type button click event - validate details are entered, perform the selected operation and clear panel
        private void btnTypeUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (txtTypeName.Text.Length > 0)
            {
                string typeName = txtTypeName.Text.Trim();
                if (dbCrewOperation == DBOperation.Add)
                {
                    CreateCrewType(typeName);
                }
                if (dbCrewOperation == DBOperation.Edit)
                {
                    UpdateCrewType(typeName);
                }
            }
            else
            {
                // If details are missing then show error message and note in system log
                MessageBox.Show("Crew type name must be entered", "Save to Database", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Invalid crew type details entered", loggedInUser.UserId);
            }

            txtTypeName.Text = "";
            dbCrewOperation = DBOperation.Add;
        }

        // Function to create crew type in the DB with given data and note in system logs.
        private void CreateCrewType(string typeName)
        {
            // Create new crew type with supplied details and add to DB          
            CrewType crewType = new CrewType();
            crewType.CrewTypeName = typeName;
            db.Entry(crewType).State = System.Data.Entity.EntityState.Added;

            int saveSuccess = SaveDBChanges();
            if (saveSuccess == 1)
            {
                CreateLogEntry($"Crew type {crewType.CrewTypeName} successfully added to database", loggedInUser.UserId);
                RefreshCrewTypes();
            }
            else
            {
                // If save is successful/failed show message and note to system log
                MessageBox.Show("Problem creating crew type record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem creating crew {crewType.CrewTypeName}", loggedInUser.UserId);
            }
        }

        // Function to update the selected crew type with supplied details in DB and note in system logs.
        private void UpdateCrewType(string typeName)
        {
            try
            {
                // Find the selected crew type in the DB and change to supplied details
                foreach (var crewType in db.CrewTypes.Where(t => t.CrewTypeID == selectedCrewType.CrewTypeID))
                {
                    crewType.CrewTypeName = typeName;
                }
            }
            catch (EntityException)
            {

                DBConnectionError();
            }

            int saveSuccess = SaveDBChanges();
            if (saveSuccess == 1)
            {
                CreateLogEntry($"User updated crew type details: {selectedCrewType.CrewTypeName} ", loggedInUser.UserId);
                RefreshCrewTypes();
            }
            else
            {
                // If save is successful/failed show message and note to system log
                MessageBox.Show("Problem updating crew type record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem updating crew type {selectedCrewType.CrewTypeName}", loggedInUser.UserId);
            }
        }

        // Update location click event - validate details are entered, perform the selected operation and clear panel
        private void btnLocationUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (txtLocationName.Text.Length > 0)
            {
                string locationName = txtLocationName.Text.Trim();
                if (dbLocationOperation == DBOperation.Add)
                {
                    CreateLocation(locationName);
                }
                if (dbLocationOperation == DBOperation.Edit)
                {
                    UpdateLocation(locationName);
                }
            }
            else
            {
                // If details are missing then show error message and note in system log
                MessageBox.Show("Location name must be entered", "Save to Database", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Invalid location details entered", loggedInUser.UserId);
            }

            txtLocationName.Text = "";
            dbLocationOperation = DBOperation.Add;
        }

        // Function to create location in the DB with given data and note in system logs.
        private void CreateLocation(string locationName)
        {
            // Create new location with supplied details and add to DB     
            Location location = new Location();
            location.LocationName = locationName;
            db.Entry(location).State = System.Data.Entity.EntityState.Added;

            int saveSuccess = SaveDBChanges();
            if (saveSuccess == 1)
            {
                CreateLogEntry($"Location {location.LocationName} successfully added to database", loggedInUser.UserId);
                RefreshLocation();
            }
            else
            {
                // If save is successful/failed show message and note to system log
                MessageBox.Show("Problem creating location record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem creating crew {location.LocationName}", loggedInUser.UserId);
            }
        }

        // Function to update the selected location with supplied details in DB and note in system logs.
        private void UpdateLocation(string locationName)
        {
            try
            {
                // Find the selected location in the DB and change to supplied details
                foreach (var location in db.Locations.Where(t => t.LocationID == selectedLocation.LocationID))
                {
                    location.LocationName = locationName;
                }
            }
            catch (EntityException)
            {

                DBConnectionError();
            }

            int saveSuccess = SaveDBChanges();
            if (saveSuccess == 1)
            {
                CreateLogEntry($"User updated location name: {selectedLocation.LocationName} ", loggedInUser.UserId);
                RefreshLocation();
            }
            else
            {
                // If save is successful/failed show message and note to system log
                MessageBox.Show("Problem updating location record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem updating location {selectedLocation.LocationName}", loggedInUser.UserId);
            }
        }

        // ---------------------------------------------------------------------------------------//
        // Event Tab Panel Update Events
        // ---------------------------------------------------------------------------------------//

        // Update crew details in panel
        private void UpdateCrewDetails()
        {
            txtCallSign.Text = selectedCrew.CallSign;
            cboCrewType.SelectedItem = selectedCrew.CrewType;
            cboCrewLocation.SelectedItem = selectedCrew.Location;
        }

        // Update crew type details in panel
        private void UpdateCrewTypeDetails()
        {
            txtTypeName.Text = selectedCrewType.CrewTypeName;
        }

        // Update location details in panel
        private void UpdateLocationDetails()
        {
            txtLocationName.Text = selectedLocation.LocationName;
        }

        // ---------------------------------------------------------------------------------------//
        // Event Tab Panel Selection Changed Events (Only enable update button if change required)
        // ---------------------------------------------------------------------------------------//

        // Crew panel call sign changed - If edit & different then selected crew's call sign in the DB then enable update button
        private void txtCallSign_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (txtCallSign.Text == selectedCrew.CallSign && dbCrewOperation == DBOperation.Edit)
            {
                btnCrewUpdate.IsEnabled = false;
            }
            else
            {
                btnCrewUpdate.IsEnabled = true;
            }
        }

        // Crew panel crew type changed - If edit & different then selected crew's type in the DB then enable update button
        private void cboCrewType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboCrewType.SelectedItem == selectedCrew.CrewType && dbCrewOperation == DBOperation.Edit)
            {
                btnCrewUpdate.IsEnabled = false;
            }
            else
            {
                btnCrewUpdate.IsEnabled = true;
            }
        }

        // Crew panel location changed - If edit & different then selected crew's location in the DB then enable update button
        private void cboCrewLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboCrewLocation.SelectedItem == selectedCrew.Location && dbCrewOperation == DBOperation.Edit)
            {
                btnCrewUpdate.IsEnabled = false;
            }
            else
            {
                btnCrewUpdate.IsEnabled = true;
            }
        }

        // Crew Type panel type name changed - If edit & different then selected crew type name in the DB then enable update button
        private void txtTypeName_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (txtTypeName.Text == selectedCrewType.CrewTypeName && dbCrewTypeOperation == DBOperation.Edit)
            {
                btnTypeUpdate.IsEnabled = false;
            }
            else
            {
                btnTypeUpdate.IsEnabled = true;
            }
        }

        // Location panel location name changed - If edit & different then selected location name in the DB then enable update button
        private void txtLocationName_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (txtLocationName.Text == selectedLocation.LocationName && dbLocationOperation == DBOperation.Edit)
            {
                btnLocationUpdate.IsEnabled = false;
            }
            else
            {
                btnLocationUpdate.IsEnabled = true;
            }
        }

        // ---------------------------------------------------------------------------------------//
        // Event Tab Build and Save Click Events
        // ---------------------------------------------------------------------------------------//

        // Build Event Click - Erase Message and Incident tables, and set all crew status to "unknown"
        private void btnBuildEvent_Click(object sender, RoutedEventArgs e)
        {
            // User confirm proceed
            MessageBoxResult result = MessageBox.Show("This will reset event data and clear information from the database. Continue ?", "Build Event", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Create system Log entry
                    CreateLogEntry($"User {loggedInUser.Username} built a new event", loggedInUser.UserId);

                    // Chnage all crew status to "unknown" -> ID = 1
                    foreach (Crew crew in db.Crews)
                    {
                        crew.StatusID = 1;
                        crew.IncidentID = null;
                    }
                    SaveDBChanges();

                    // Truncate messages table and Delete + RESEED Incident table using sql commands 
                    db.Database.ExecuteSqlCommand("TRUNCATE TABLE [RadioDB].[dbo].[Message]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [RadioDB].[dbo].[Incident]");
                    db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('[RadioDB].[dbo].[Incident]', RESEED, 0)");

                    // Display success message to user
                    MessageBox.Show("Event built successfully", "Build Event", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                catch (EntityException)
                {
                    DBConnectionError();
                }
            }
        }

        // Write button click - Using save dialog write formatted event data to file.
        private void btnWriteToCSV_Click(object sender, RoutedEventArgs e)
        {
            // Check that a user has supplied an event name and date
            if (txtEventName.Text == "" || dateEventDate.SelectedDate == null)
            {
                // If not entered display error
                MessageBox.Show("Eventname and date must be entered before saving", "Save to File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                // create new save dialog with types CSV, TXT and all, show to user.
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV files (*.csv)| *.csv|txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveDialog.FilterIndex = 1;
                saveDialog.ShowDialog();

                // If user enters a save location and name, write to file
                if (saveDialog.FileName != "")
                {
                    using (StreamWriter writetext = new StreamWriter(saveDialog.FileName))
                    {
                        writetext.WriteLine($"{txtEventName.Text.Trim()}: {dateEventDate.SelectedDate.Value.ToString("dd/MM/yyyy")}"); // Event Name: dd/MM/yyyy
                        writetext.WriteLine();
                        writetext.WriteLine();

                        writetext.WriteLine("Event Incidents"); // Event Incidents
                        writetext.WriteLine("Incident No,Location,Reported Time,At Scene Time,Leave Scene Time,Description"); //Header: Incident No,Location,Reported Time,At Scene Time,Leave Scene Time,Description

                        // For each incident in DB write in format - IncidentNo, Location, ReportedTime, AtSceneTime, LeaveSceneTime, Description ("," replaced with ";" to avoid CSV issues)
                        foreach (var incident in db.Incidents)
                        {
                            string description = incident.Description.Replace(",", ";");
                            writetext.WriteLine($"{incident.IncidentNo}, {incident.Location.LocationName}, {incident.ReportedTime}, {incident.AtSceneTime}, {incident.LeaveSceneTime}, {description}");
                        }
                        writetext.WriteLine();
                        writetext.WriteLine();

                        writetext.WriteLine("Event Messages"); // Event Messages
                        writetext.WriteLine("Date,Call Sign,Incident,Status,Message Text"); //Header: Date,Call Sign,Incident,Status,Message Text

                        // For each message in DB write in format - Date, CallSign, Incident, Status, MessageText ("," replaced with ";" to avoid CSV issues)
                        foreach (var message in db.Messages)
                        {
                            string messageText = message.MessageText.Replace(",", ";");

                            //If message has an incident print otherwise blank
                            if (message.Incident == null)
                            {
                                writetext.WriteLine($"{message.Date}, {message.Crew.CallSign},, {message.Status.StatusName}, {messageText}");
                            }
                            else
                            {
                                writetext.WriteLine($"{message.Date}, {message.Crew.CallSign}, {message.Incident.IncidentNo}, {message.Status.StatusName}, {messageText}");
                            }
                        }
                    }
                }
                else
                {
                    // If Event name and date not entered show error
                    MessageBox.Show("File Name must be entered", "Save to File", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
            RefreshLogsList();
        }

        // Error controled function to save to database. Return success/failure.
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
