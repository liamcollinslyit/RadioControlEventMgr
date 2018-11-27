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
    /// <summary>
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin : Page
    {

        enum DBOperation
        {
            Add,
            Edit,
            Delete
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


        private void submenuAddNewUser_Click(object sender, RoutedEventArgs e)
        {
            ClearUserDetails();
            stkUserDetails.Visibility = Visibility.Visible;
            dbOperation = DBOperation.Add;
        }

        private void submenuEditUser_Click(object sender, RoutedEventArgs e)
        {
            stkUserDetails.Visibility = Visibility.Visible;
            UpdateUserDetails();
            dbOperation = DBOperation.Edit;
        }

        private void submenuDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            db.Users.RemoveRange(db.Users.Where(t => t.UserId == selectedUser.UserId));

            int saveSuccess = db.SaveChanges();
            if (saveSuccess == 1)
            {
                MessageBox.Show("User deleted successfully", "Save to database", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshUserList();
            }
            else
            {
                MessageBox.Show("Problem deleting user record", "Save to database", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ClearUserDetails();
            stkUserDetails.Visibility = Visibility.Collapsed;
        }


        private void btnEditUpdate_Click(object sender, RoutedEventArgs e)
        {
            string username = txtEditUsername.Text.Trim();
            string password = txtEditPassword.Text.Trim();
            string forename = txtEditForename.Text.Trim();
            string surname = txtEditSurname.Text.Trim();

            AccessLevel accessLevel = (AccessLevel)cboEditUserAccess.SelectedItem;


            if (username == "" || password == "")
            {
                MessageBox.Show("Valid Username and Password Required", "Save to database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (accessLevel == null)
            {
                MessageBox.Show("Please Select Access Level", "Save to database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
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

            bool isUnique = true;

            foreach (var dbUser in db.Users)
            {
                if (dbUser.Username == username)
                {
                    MessageBox.Show("Username must be unique, update not saved", "Save to database", MessageBoxButton.OK, MessageBoxImage.Error);
                    isUnique = false;
                    break;
                }
            }

            if (isUnique)
            {
                db.Entry(user).State = System.Data.Entity.EntityState.Added;
                int saveSuccess = db.SaveChanges();
                if (saveSuccess == 1)
                {
                    MessageBox.Show("User saved successfully", "Save to database", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshUserList();
                    ClearUserDetails();
                    stkUserDetails.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("Problem creating user record", "Save to database", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateUser(string username, string password, string forename, string surname, AccessLevel accessLevel)
        {
            bool isUnique = true;

            foreach (var user in db.Users.Where(t => t.UserId != selectedUser.UserId))
            {
                if (user.Username == username)
                {
                    MessageBox.Show("Username must be unique, update not saved", "Save to database", MessageBoxButton.OK, MessageBoxImage.Error);
                    isUnique = false;
                    break;
                }
            }

            foreach (var user in db.Users.Where(t => t.UserId == selectedUser.UserId))
            {
                if (user.Username == username && user.Password == password && user.Forename == forename && user.Surname == surname && user.LevelID == accessLevel.LevelID)
                {
                    ClearUserDetails();
                    stkUserDetails.Visibility = Visibility.Collapsed;
                    MessageBox.Show("User data change not required", "Save to database", MessageBoxButton.OK, MessageBoxImage.Information);
                    isUnique = false;
                    break;
                }

                user.Username = username;
                user.Password = password;
                user.Forename = forename;
                user.Surname = surname;
            }

            if (isUnique)
            {
                int saveSuccess = db.SaveChanges();
                if (saveSuccess == 1)
                {
                    MessageBox.Show("User updated successfully", "Save to database", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshUserList();
                    ClearUserDetails();
                    stkUserDetails.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("Problem updating user record", "Save to database", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void ClearUserDetails()
        {
            txtEditForename.Text = "";
            txtEditSurname.Text = "";
            txtEditUsername.Text = "";
            txtEditPassword.Text = "";
            cboEditUserAccess.SelectedIndex = -1;

        }

        private void UpdateUserDetails()
        {
            txtEditForename.Text = selectedUser.Forename;
            txtEditSurname.Text = selectedUser.Surname;
            txtEditUsername.Text = selectedUser.Username;
            txtEditPassword.Text = selectedUser.Password;
            cboEditUserAccess.SelectedItem = selectedUser.AccessLevel;
        }


        private void RefreshUserList()
        {
            lstUserList.ItemsSource = users;
            users.Clear();
            foreach (var user in db.Users)
            {
                users.Add(user);
            }
            lstUserList.Items.Refresh();
        }

        private void RefreshLogsList()
        {
            lstLogsList.ItemsSource = logs;
            logs.Clear();
            foreach (var log in db.Logs)
            {
                logs.Add(log);
            }
            lstUserList.Items.Refresh();
        }

        private void RefreshAccessLevels()
        {
            cboEditUserAccess.ItemsSource = accessLevels;
            accessLevels.Clear();
            foreach (var accessLevel in db.AccessLevels)
            {
                accessLevels.Add(accessLevel);
            }
            cboEditUserAccess.Items.Refresh();
        }


    }
}
