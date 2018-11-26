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
        RadioDBEntities db = new RadioDBEntities("metadata=res://*/RadioModel.csdl|res://*/RadioModel.ssdl|res://*/RadioModel.msl;provider=System.Data.SqlClient;provider connection string='data source=192.168.60.132" +
                                         ";initial catalog=RadioDB;user id=radiouser;password=password;pooling=False;MultipleActiveResultSets=True;App=EntityFramework'");
        List<User> users = new List<User>();
        List<Log> logs = new List<Log>();
        List<AccessLevel> accessLevels = new List<AccessLevel>();

        public Admin()
        {
            InitializeComponent();
        }

        private void submenuAddNewUser_Click(object sender, RoutedEventArgs e)
        {
            stkUserDetails.Visibility = Visibility.Visible;

        }

        private void btnEditUpdate_Click(object sender, RoutedEventArgs e)
        {
            string username = txtEditUsername.Text;
            string password = txtEditPassword.Text;
            string forename = txtEditForename.Text;
            string surname = txtEditSurname.Text;

            AccessLevel accessLevel = new AccessLevel();
            accessLevel = (AccessLevel)cboEditUserAccess.SelectedItem;

            CreateUser(username, password, forename, surname, accessLevel);
            stkUserDetails.Visibility = Visibility.Collapsed;
        }

        private void CreateUser(string username, string password, string forename, string surname, AccessLevel accessLevel)
        {
            User user = new User();
            user.Username = username;
            user.Password = password;
            user.Forename = forename;
            user.Surname = surname;
            user.LevelID = accessLevel.LevelID;
            SaveUser(user);
        }

        private void SaveUser(User user)
        {
            db.Entry(user).State = System.Data.Entity.EntityState.Added;
            db.SaveChanges();
        }

        private void submenuEditUser_Click(object sender, RoutedEventArgs e)
        {

        }

        private void submenuDeleteUser_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            lstUserList.ItemsSource = users;
            lstLogsList.ItemsSource = logs;
            cboEditUserAccess.ItemsSource = accessLevels;

            foreach (var user in db.Users)
            {
                users.Add(user);
            }

            foreach (var log in db.Logs)
            {
                logs.Add(log);
            }

            foreach (var accessLevel in db.AccessLevels)
            {
                accessLevels.Add(accessLevel);
            }

        }
    }
}
