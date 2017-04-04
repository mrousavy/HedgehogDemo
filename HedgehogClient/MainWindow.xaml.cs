using System.Net;
using System.Windows;
using System.Windows.Input;

namespace HedgehogClient {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
            ipBox.Focus();
            ipBox.Text = IpStore.Load();
            ipBox.SelectAll();
        }

        private void ConnectClick(object sender, RoutedEventArgs e) {
            IpSubmit();
        }

        private void IpBox_OnKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                IpSubmit();
            }
        }


        private void IpSubmit() {
            if(IPAddress.TryParse(ipBox.Text, out IPAddress address)) {
                //Save IP to file
                IpStore.Save(ipBox.Text);

                //Open Hedgehog Controller and close this window
                new HedgehogClientWindow(address).Show();
                Close();
            } else {
                //Error on IP
                MessageBox.Show("Please enter a valid IP Address!", "Wrong IP Format!", MessageBoxButton.OK, MessageBoxImage.Error);
                ipBox.Focus();
                ipBox.SelectAll();
            }
        }
    }
}
