using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace HedgehogClient {
    /// <summary>
    /// Interaction logic for HedgehogClientWindow.xaml
    /// </summary>
    public partial class HedgehogClientWindow : Window {
        private IPAddress _address;
        private IPEndPoint _endPoint;
        private TcpClient _client;

        private const int _port = 8000;


        public HedgehogClientWindow(IPAddress address) {
            InitializeComponent();
            _address = address;

            Connect();
        }

        private async void Connect() {
            statusLabel.Content = "Connecting...";
            statusLabel.Foreground = Brushes.Orange;

            _endPoint = new IPEndPoint(_address, _port);
            _client = new TcpClient();

            try {
                Cursor = Cursors.AppStarting;

                await _client.ConnectAsync(_address, _port);

                if(_client.Connected) {
                    statusLabel.Content = "Connected";
                    statusLabel.Foreground = Brushes.Green;
                } else {
                    statusLabel.Content = "Disconnected";
                    statusLabel.Foreground = Brushes.Red;
                }


            } catch(Exception e) {
                statusLabel.Content = "Disconnected";
                statusLabel.Foreground = Brushes.Red;
                MessageBox.Show($"Could not Connect!\n\r{e.Message}", "Error Connecting to Hedgehog!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WindowKeyDown(object sender, KeyEventArgs e) {
            Key key = e.Key;

        }



        private void Disconnected(string message = null) {
            statusLabel.Content = "Disconnected";
            statusLabel.Foreground = Brushes.Red;

            string msgBoxText = "The connection to the Hedgehog has been lost!";
            if(message != null) {
                msgBoxText += "\n\r" + message;
            }

            MessageBox.Show(msgBoxText, "Disconnected", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
