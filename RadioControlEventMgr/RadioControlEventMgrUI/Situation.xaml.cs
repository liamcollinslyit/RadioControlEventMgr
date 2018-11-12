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

    // C# code for Situation page

    public partial class Situation : Page
    {
        public Situation()
        {
            InitializeComponent();
        }

        // Context menu - Add incident button - show incident stackpanel
        private void submenuAddIncident_Click(object sender, RoutedEventArgs e)
        {
            stkSituationIncident.Visibility = Visibility.Visible;
        }

        // Context menu - New message button - show message stackpanel
        private void submenuNewMessage_Click(object sender, RoutedEventArgs e)
        {
            stkMessage.Visibility = Visibility.Visible;
        }

        // Incident panel - Cancel button - hide incident stackpanel
        private void btnSituationCancel_Click(object sender, RoutedEventArgs e)
        {
            stkSituationIncident.Visibility = Visibility.Collapsed;
        }

        // Incident panel - Ok button - hide message stackpanel
        private void btnSituationOk_Click(object sender, RoutedEventArgs e)
        {
            stkSituationIncident.Visibility = Visibility.Collapsed;
        }

        // Message Panel  - Cancel button - hide message stackpanel
        private void btnMessageCancel_Click(object sender, RoutedEventArgs e)
        {
            stkMessage.Visibility = Visibility.Collapsed;
        }

        // Message Panel  - Ok button - hide message stackpanel
        private void btnMessageOk_Click(object sender, RoutedEventArgs e)
        {
            stkMessage.Visibility = Visibility.Collapsed;
        }
    }
}
