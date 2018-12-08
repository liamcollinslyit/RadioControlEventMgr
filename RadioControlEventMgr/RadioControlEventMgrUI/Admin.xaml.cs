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
        List<Crew> crews = new List<Crew>();
        List<CrewType> crewTypes = new List<CrewType>();
        List<Location> locations = new List<Location>();
        List<Log> logs = new List<Log>();
        List<AccessLevel> accessLevels = new List<AccessLevel>();

        User selectedUser = new User();
        Crew selectedCrew = new Crew();
        CrewType selectedCrewType = new CrewType();
        Location selectedLocation = new Location();
        DBOperation dbUserOperation = new DBOperation();
        DBOperation dbCrewOperation = new DBOperation();
        DBOperation dbCrewTypeOperation = new DBOperation();
        DBOperation dbLocationOperation = new DBOperation();

        public Admin()
        {
            InitializeComponent();
        }

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

        private void lstUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (lstUserList.SelectedIndex >= 0)
            {
                submenuEditUser.IsEnabled = true;
                submenuDeleteUser.IsEnabled = true;
                selectedUser = users.ElementAt(lstUserList.SelectedIndex);
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

        private void lstCrewList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCrewList.SelectedIndex >= 0)
            {
                submenuEditCrew.IsEnabled = true;
                submenuDeleteCrew.IsEnabled = true;
                selectedCrew = crews.ElementAt(lstCrewList.SelectedIndex);
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

        private void lstLocationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstLocationList.SelectedIndex >= 0)
            {
                submenuEditLocation.IsEnabled = true;
                submenuDeleteLocation.IsEnabled = true;
                selectedLocation = locations.ElementAt(lstLocationList.SelectedIndex);
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

        private void submenuAddNewUser_Click(object sender, RoutedEventArgs e)
        {
            dbUserOperation = DBOperation.Add;
            ClearUserDetails();
            stkUserDetails.Visibility = Visibility.Visible;
            CreateLogEntry("User opened add user panel", loggedInUser.UserId);
        }

        private void submenuEditUser_Click(object sender, RoutedEventArgs e)
        {
            dbUserOperation = DBOperation.Edit;
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
                dbUserOperation == DBOperation.Edit)
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

        private void RefreshCrew()
        {
            try
            {
                int crewNum = 0;
                crews.Clear();
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

        private void RefreshCrewTypes()
        {
            try
            {
                int crewTypeNum = 0;
                crewTypes.Clear();
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

        private void RefreshLocation()
        {
            try
            {
                int locationNum = 0;
                locations.Clear();
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


        private void btnWriteToCSV_Click(object sender, RoutedEventArgs e)
        {
            if (txtEventName.Text == "" || dateEventDate.SelectedDate == null)
            {
                MessageBox.Show("Eventname and date must be entered before saving", "Save to File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV files (*.csv)| *.csv|txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveDialog.FilterIndex = 1;
                saveDialog.ShowDialog();

                if (saveDialog.FileName != "")
                {
                    using (StreamWriter writetext = new StreamWriter(saveDialog.FileName))
                    {
                        writetext.WriteLine($"{txtEventName.Text.Trim()}: {dateEventDate.SelectedDate.Value.ToString("dd/MM/yyyy")}");
                        writetext.WriteLine();
                        writetext.WriteLine();
                        writetext.WriteLine("Event Incidents");
                        writetext.WriteLine("Incident No,Location,Reported Time,At Scene Time, Leave Scene Time,Description");
                        foreach (var incident in db.Incidents)
                        {
                            string description = incident.Description.Replace(",", ";");
                            writetext.WriteLine($"{incident.IncidentNo}, {incident.Location.LocationName}, {incident.ReportedTime}, {incident.AtSceneTime}, {incident.LeaveSceneTime}, {description}");
                        }
                        writetext.WriteLine();
                        writetext.WriteLine();
                        writetext.WriteLine("Event Messages");
                        writetext.WriteLine("Date,Call Sign,Incident,Status,Message Text");
                        foreach (var message in db.Messages)
                        {
                            string messageText = message.MessageText.Replace(",", ";");
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
                    MessageBox.Show("File Name must be entered", "Save to File", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnBuildEvent_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("This will reset event data and clear information from the database. Continue ?", "Build Event", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    CreateLogEntry($"User {loggedInUser.Username} built a new event", loggedInUser.UserId);

                    foreach (Crew crew in db.Crews)
                    {
                        crew.StatusID = 1;
                        crew.IncidentID = null;
                    }

                    SaveDBChanges();

                    db.Database.ExecuteSqlCommand("TRUNCATE TABLE [RadioDB].[dbo].[Message]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [RadioDB].[dbo].[Incident]");
                    db.Database.ExecuteSqlCommand("DBCC CHECKIDENT('[RadioDB].[dbo].[Incident]', RESEED, 0)");

                    MessageBox.Show("Event built successfully", "Build Event", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                catch (EntityException)
                {
                    DBConnectionError();
                }
            }
        }

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
                MessageBox.Show("Call Sign, Crew Type and Crew Location must be selected", "Save to Database", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Invalid crew details entered", loggedInUser.UserId);
            }

            txtCallSign.Text = "";
            cboCrewType.SelectedIndex = -1;
            cboCrewLocation.SelectedIndex = -1;
            dbCrewOperation = DBOperation.Add;

        }

        private void CreateCrew(string callSign, int statusID, CrewType crewType, Location location)
        {
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
                MessageBox.Show("Problem creating crew record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem creating crew {crew.CallSign}", loggedInUser.UserId);
            }

        }

        private void UpdateCrew(string callSign, CrewType crewType, Location location)
        {
            try
            {
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
                MessageBox.Show("Problem updating crew record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem updating crew {selectedCrew.CallSign}", loggedInUser.UserId);
            }
        }

        private void UpdateCrewDetails()
        {
            txtCallSign.Text = selectedCrew.CallSign;
            cboCrewType.SelectedItem = selectedCrew.CrewType;
            cboCrewLocation.SelectedItem = selectedCrew.Location;
        }

        private void submenuAddNewCrew_Click(object sender, RoutedEventArgs e)
        {
            dbCrewOperation = DBOperation.Add;
            btnCrewUpdate.IsEnabled = true;
        }

        private void submenuEditCrew_Click(object sender, RoutedEventArgs e)
        {
            dbCrewOperation = DBOperation.Edit;
            btnCrewUpdate.IsEnabled = false;
            UpdateCrewDetails();
        }

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
                MessageBox.Show($"Problem deleting crew {selectedCrew.CallSign}, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem deleting crew {selectedCrew.CallSign}", loggedInUser.UserId);
            }

            txtCallSign.Text = "";
            cboCrewType.SelectedIndex = -1;
            cboCrewLocation.SelectedIndex = -1;
            dbCrewOperation = DBOperation.Add;
        }



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
                MessageBox.Show("Crew type name must be entered", "Save to Database", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Invalid crew type details entered", loggedInUser.UserId);
            }

            txtTypeName.Text = "";
            dbCrewOperation = DBOperation.Add;
        }

        private void CreateCrewType(string typeName)
        {
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
                MessageBox.Show("Problem creating crew type record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem creating crew {crewType.CrewTypeName}", loggedInUser.UserId);
            }
        }

        private void UpdateCrewType(string typeName)
        {
            try
            {
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
                MessageBox.Show("Problem updating crew type record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem updating crew type {selectedCrewType.CrewTypeName}", loggedInUser.UserId);
            }
        }

        private void UpdateCrewTypeDetails()
        {
            txtTypeName.Text = selectedCrewType.CrewTypeName;
        }

        private void submenuAddCrewType_Click(object sender, RoutedEventArgs e)
        {
            dbCrewTypeOperation = DBOperation.Add;
            btnTypeUpdate.IsEnabled = true;
        }

        private void submenuEditCrewType_Click(object sender, RoutedEventArgs e)
        {
            dbCrewTypeOperation = DBOperation.Edit;
            btnTypeUpdate.IsEnabled = false;
            UpdateCrewTypeDetails();
        }

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
                MessageBox.Show($"Problem deleting crew type {selectedCrewType.CrewTypeName}, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem deleting crew {selectedCrewType.CrewTypeName}", loggedInUser.UserId);
            }

            txtTypeName.Text = "";
            dbCrewTypeOperation = DBOperation.Add;
        }

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
                MessageBox.Show("Location name must be entered", "Save to Database", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Invalid location details entered", loggedInUser.UserId);
            }

            txtLocationName.Text = "";          
            dbLocationOperation = DBOperation.Add;
        }

        private void CreateLocation(string locationName)
        {
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
                MessageBox.Show("Problem creating location record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem creating crew {location.LocationName}", loggedInUser.UserId);
            }

        }

        private void UpdateLocation(string locationName)
        {
            try
            {
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
                MessageBox.Show("Problem updating location record, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem updating location {selectedLocation.LocationName}", loggedInUser.UserId);
            }
        }

        private void UpdateLocationDetails()
        {
            txtLocationName.Text = selectedLocation.LocationName;
        }

        private void submenuAddNewLocation_Click(object sender, RoutedEventArgs e)
        {
            dbLocationOperation = DBOperation.Add;
            btnLocationUpdate.IsEnabled = true;
        }

        private void submenuEditLocation_Click(object sender, RoutedEventArgs e)
        {
            dbLocationOperation = DBOperation.Edit;
            btnLocationUpdate.IsEnabled = false;
            UpdateLocationDetails();
        }

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
                MessageBox.Show($"Problem deleting location {selectedLocation.LocationName}, please try again or contact system administrator", "User Administration", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateLogEntry($"Error: Problem deleting location {selectedLocation.LocationName}", loggedInUser.UserId);
            }

            txtLocationName.Text = "";
            dbLocationOperation = DBOperation.Add;
        }

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

        private void btnFilterLogs_Click(object sender, RoutedEventArgs e)
        {
            RefreshLogsList(txtFilterLogs.Text);
        }

        private void btnClearFilterLogs_Click(object sender, RoutedEventArgs e)
        {
            RefreshLogsList();
            tbxLogStats.Text = "";
        }

        private void btnClearAdminLogs_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("This will clear all system lof informaion from the database. Continue ?", "Clear Log", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    db.Database.ExecuteSqlCommand("TRUNCATE TABLE[RadioDB].[dbo].[Log]");         
                    RefreshLogsList();
                }
                catch (EntityException)
                {
                    DBConnectionError();
                }
            }
        }

        private void btnFilterUsers_Click(object sender, RoutedEventArgs e)
        {
            RefreshUserList(txtFilterUsers.Text);
        }

        private void btnClearFilterUsers_Click(object sender, RoutedEventArgs e)
        {
            RefreshUserList();
            tbxUsersStats.Text = "";
        }
    }
}
