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
    /// Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : Page
    {
        public Map()
        {
            InitializeComponent();
        }

        private void submenuOfflineMap_Click(object sender, RoutedEventArgs e)
        {
            webMap.Visibility = Visibility.Collapsed;
            imgMap.Visibility = Visibility.Visible;
        }

        private void submenuOnlineMap_Click(object sender, RoutedEventArgs e)
        {
            imgMap.Visibility = Visibility.Collapsed;
            webMap.Visibility = Visibility.Visible;
        }
    }
}
