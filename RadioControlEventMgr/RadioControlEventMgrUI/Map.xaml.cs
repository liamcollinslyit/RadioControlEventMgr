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

    // C# code for map page

    public partial class Map : Page
    {
        public Map()
        {
            InitializeComponent();
        }

        // Context menu - Offline map button - show offline map and hide online map
        private void submenuOfflineMap_Click(object sender, RoutedEventArgs e)
        {
            webMap.Visibility = Visibility.Collapsed;
            imgMap.Visibility = Visibility.Visible;
        }

        // Context menu - Online map button - show online map and hide offline map
        private void submenuOnlineMap_Click(object sender, RoutedEventArgs e)
        {
            imgMap.Visibility = Visibility.Collapsed;
            webMap.Visibility = Visibility.Visible;
        }
    }
}
