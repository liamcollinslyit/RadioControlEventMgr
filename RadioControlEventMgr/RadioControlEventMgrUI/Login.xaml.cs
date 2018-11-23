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

        // Enter button - validate user and initilise correct dashboard.
        private void btnLoginEnter_Click(object sender, RoutedEventArgs e)
        {
            string currentUser = tbxUsername.Text;
            string currentPassword = tbxPassword.Password;
            bool login = false;
            User validatedUser = new User();

            foreach (var user in db.Users)
            {
                if (currentUser == user.Username && currentPassword == user.Password)
                {
                    login = true;
                    validatedUser = user;
                }
            }

            if (login)
            {
                CreateLogsEntry("User log in successfully", validatedUser);
                Dashboard dashboard = new Dashboard();
                dashboard.Owner = this;
                dashboard.user = validatedUser;
                this.Hide();
                dashboard.ShowDialog();
            }
            else
            {
                lblLoginHeading.Content = $"Login attempt {loginCounter} of 3 failed - Please try again";
                lblLoginHeading.Foreground = Brushes.Red;
                loginCounter++;
            }

            if (loginCounter > 3)
            {
                lblLoginHeading.Content = $"Login attempt 3 of 3 failed - relaunch application to try again";
                lblLoginHeading.Foreground = Brushes.Red;
                lblLoginHeading.FontSize = 16;
                btnLoginEnter.IsEnabled = false;
            }

        }

        private void CreateLogsEntry(string eventDescription, User user)
        {
            Log log = new Log();
            log.Event = eventDescription;
            log.UserID = user.UserId;
            log.Date = DateTime.Now;
            SaveLog(log);
        }

        private void SaveLog(Log log)
        {
            db.Entry(log).State = System.Data.Entity.EntityState.Added;
            db.SaveChanges();
        }



        //Exit button - close application
        private void btnLoginExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(0);
        }
    }
}
