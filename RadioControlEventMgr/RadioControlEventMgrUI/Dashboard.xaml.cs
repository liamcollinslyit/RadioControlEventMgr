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
using System.Windows.Shapes;

namespace RadioControlEventMgrUI
{

    // C# code for dashboard

    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        // Situation button - Open Situation page in frame
        private void btnSituation_Click(object sender, RoutedEventArgs e)
        {
            Situation situation = new Situation();
            frmMain.Navigate(situation);
        }

        // Logs button - Open Logs page in frame
        private void btnLogs_Click(object sender, RoutedEventArgs e)
        {
            Logs logs = new Logs();
            frmMain.Navigate(logs);
        }

        // Map button - Open Map page in frame
        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            Map map = new Map();
            frmMain.Navigate(map);
        }

        // Admin button - Open Admin page in frame
        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            Admin admin = new Admin();
            frmMain.Navigate(admin);
        }

        //Exit button - close application
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
