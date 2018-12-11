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
    /// Interaction logic for map screen - Display local and online map
    /// </summary>
    public partial class Map : Page
    {
        // Currently logged in user, passed from dashboard
        public User loggedInUser = new User();

        public Map()
        {
            InitializeComponent();
        }
    }
}
