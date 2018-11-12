﻿using System;
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
        public Login()
        {
            InitializeComponent();
        }

        // Enter button - validate user and initilise correct dashboard.
        private void btnLoginEnter_Click(object sender, RoutedEventArgs e)
        {
            Dashboard dashboard = new Dashboard();
            dashboard.Show();
            this.Close();
        }

        //Exit button - close application
        private void btnLoginExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
