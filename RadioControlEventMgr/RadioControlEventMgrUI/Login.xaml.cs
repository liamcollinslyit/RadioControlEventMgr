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

       RadioDBEntities db = new RadioDBEntities();
        int loginCounter = 0;


        public Login()
        {
            InitializeComponent();
        }

        // Enter button - validate user and initilise correct dashboard.
        private void btnLoginEnter_Click(object sender, RoutedEventArgs e)
        {
            string currentUser = tbxUsername.Text;
            string currentPassword = tbxPassword.Password;

            foreach (var userRecord in db.Users)
            {
                if(currentUser == userRecord.Username)
                {
                    if (currentPassword == userRecord.Password)
                    {
                        Dashboard dashboard = new Dashboard();
                        dashboard.Show();
                        this.Close();
                    }
                }
            }

            loginCounter++;
            if (loginCounter<3)
            {
                lblLoginHeading.Content = "Login Failed - Please try again";
                lblLoginHeading.Foreground = Brushes.Red;
            }
            else
            {
                lblLoginHeading.Content = "Max number of attempts - try again in 5 minutes";
                lblLoginHeading.Foreground = Brushes.Red;
                System.Threading.Thread.Sleep(10000);
                loginCounter = 0;
            }

        }

        //Exit button - close application
        private void btnLoginExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
