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

namespace GameServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClsConnection m_objConnection;

        public MainWindow()
        {
            InitializeComponent();
            m_objConnection = new ClsConnection();
            m_objConnection.StartServer(rtbOutputText);
        }

        private void CloseConnections(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_objConnection.CloseConnections();
        }
    }
}
