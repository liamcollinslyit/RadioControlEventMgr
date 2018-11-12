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

    // C# code for Logs page

    public partial class Logs : Page
    {
        public Logs()
        {
            InitializeComponent();
        }

        // Context menu - Open incident button - show incident details stackpanel
        private void submenuOpenIncident_Click(object sender, RoutedEventArgs e)
        {
            stkIncident.Visibility = Visibility.Visible;
        }
    }
}
