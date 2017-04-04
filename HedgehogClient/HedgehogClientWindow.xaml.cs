using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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
        private SocketStatus _status;

        private const int _port = 8000;


        private enum SocketStatus {
            Disconnected,
            Connecting,
            Connected,
            Busy
        }


        public HedgehogClientWindow(IPAddress address) {
            InitializeComponent();
            _address = address;

            Connect();
        }

        private async void Connect() {
            Cursor = Cursors.AppStarting;
            statusLabel.Content = "Connecting...";
            statusLabel.Foreground = Brushes.Orange;

            _endPoint = new IPEndPoint(_address, _port);
            _client = new TcpClient();

            try {
                _status = SocketStatus.Connecting;

                await _client.ConnectAsync(_address, _port);

                if(_client.Connected) {
                    _status = SocketStatus.Connected;
                    statusLabel.Content = "Connected";
                    statusLabel.Foreground = Brushes.Green;
                } else {
                    _status = SocketStatus.Disconnected;
                    statusLabel.Content = "Disconnected";
                    statusLabel.Foreground = Brushes.Red;
                }
            } catch(Exception e) {
                _status = SocketStatus.Disconnected;
                statusLabel.Content = "Disconnected";
                statusLabel.Foreground = Brushes.Red;
                MessageBox.Show($"Could not Connect!\n\r{e.Message}", "Error Connecting to Hedgehog!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Cursor = Cursors.Arrow;
        }





        private void Log(string message) {
            logBox.Text += $"({DateTime.Now}) >" + message + Environment.NewLine;
        }

        //KeyDown Locks a Key (e.g. W) and drives forward till KeyUp)
        private void WindowKeyDown(object sender, KeyEventArgs e) {
            try {
                Key key = e.Key;

                ControlKeys.MovementKey movementKey = ControlKeys.GetKey(key);

                SendKey(movementKey);
            } catch {
                //Wrong input
            }
        }

        //KeyUp Unlocks a Key (e.g. W)
        private void WindowKeyUp(object sender, KeyEventArgs e) {
            SendKey(ControlKeys.MovementKey.Stop);
        }

        private async void SendKey(ControlKeys.MovementKey movementKey) {
            while(_status == SocketStatus.Busy) {
                await Task.Delay(10);
            }

            if(_status == SocketStatus.Disconnected || _status == SocketStatus.Connecting) {
                return;
            }

            byte key = (byte)movementKey;
            byte[] message = { key };

            _client.Client.BeginSend(message, 0, 1, SocketFlags.None, Sent, null);
        }


        private void Sent(IAsyncResult result) {
            if(result.IsCompleted) {
                Console.WriteLine("Sent!");
            } else {
                Disconnected("Tried to send Message to Hedgehog, failed");
            }
        }

        private void Disconnected(string message = null) {
            _status = SocketStatus.Disconnected;
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
