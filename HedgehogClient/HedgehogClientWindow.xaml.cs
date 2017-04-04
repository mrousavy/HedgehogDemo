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

        private static TaskCompletionSource<bool> _tcs;

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
            ipLabel.Content = _address + ":" + _port;

            Connect();
        }

        private async void Connect() {
            Log("Connecting to Hedgehog...");
            Cursor = Cursors.AppStarting;
            statusLabel.Content = "Connecting...";
            statusLabel.Foreground = Brushes.Orange;

            _endPoint = new IPEndPoint(_address, _port);
            _client = new TcpClient();

            try {
                _status = SocketStatus.Connecting;

                await _client.ConnectAsync(_address, _port);

                if(_client.Connected) {
                    Log("Connected!");
                    _status = SocketStatus.Connected;
                    statusLabel.Content = "Connected";
                    statusLabel.Foreground = Brushes.Green;
                } else {
                    Log("Error Connecting!");
                    _status = SocketStatus.Disconnected;
                    statusLabel.Content = "Disconnected";
                    statusLabel.Foreground = Brushes.Red;
                }
            } catch(Exception e) {
                Log("Error Connecting!");
                Log(e.Message);
                DisconnectButton.IsEnabled = false;
                DisconnectButton.ToolTip = "Not yet connected";
                _status = SocketStatus.Disconnected;
                statusLabel.Content = "Disconnected";
                statusLabel.Foreground = Brushes.Red;
                MessageBox.Show($"Could not Connect!\n\r{e.Message}", "Error Connecting to Hedgehog!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Cursor = Cursors.Arrow;
        }


        //Log to Console
        private void Log(string message) {
            logBox.Text += $"({DateTime.Now:HH:mm:ss}) > " + message + Environment.NewLine;
            logBox.ScrollToLine(logBox.LineCount - 2);
        }

        //KeyDown Locks a Key (e.g. W) and drives forward till KeyUp)
        private async void WindowKeyDown(object sender, KeyEventArgs e) {
            try {
                keyLabel.Content = "Sending...";

                _tcs = new TaskCompletionSource<bool>();
                await SendKey(ControlKeys.MovementKey.Stop);
                await _tcs.Task;

                Key key = e.Key;

                ControlKeys.MovementKey movementKey = ControlKeys.GetKey(key);

                keyLabel.Content = ControlKeys.FriendlyStatus(movementKey);

                _tcs = new TaskCompletionSource<bool>();
                await SendKey(movementKey);
                await _tcs.Task;
            } catch {
                //Wrong input
            }
        }

        //KeyUp Unlocks a Key (e.g. W)
        private async void WindowKeyUp(object sender, KeyEventArgs e) {
            try {
                await SendKey(ControlKeys.MovementKey.Stop);
            } catch {
                // ignored
            }

            keyLabel.Content = "/";
        }

        private async Task SendKey(ControlKeys.MovementKey movementKey) {
            while(_status == SocketStatus.Busy) {
                await Task.Delay(10);
            }

            if(_status == SocketStatus.Disconnected || _status == SocketStatus.Connecting) {
                throw new Exception("Not connected to Hedgehog!");
            }

            _status = SocketStatus.Busy;

            byte key = (byte)movementKey;
            byte[] message = { key };

            _client.Client.BeginSend(message, 0, 1, SocketFlags.None, Sent, null);
        }


        private void Sent(IAsyncResult result) {
            _client.Client.EndSend(result);

            if(result.IsCompleted) {
                _tcs.SetResult(true);
            } else {
                Disconnected("Tried to send Message to Hedgehog, failed");
                _tcs.SetResult(false);
            }

            _status = SocketStatus.Connected;
        }

        private async void Disconnect(string message = null) {
            while(_status == SocketStatus.Busy) {
                await Task.Delay(10);
            }

            if(_status == SocketStatus.Connected) {
                _client.Close();
                Disconnected(message);
            }
        }

        private void Disconnected(string message = null) {
            DisconnectButton.IsEnabled = false;
            DisconnectButton.ToolTip = "Already disconnected";
            _status = SocketStatus.Disconnected;
            statusLabel.Content = "Disconnected";
            statusLabel.Foreground = Brushes.Red;

            string msgBoxText = "The connection to the Hedgehog has been lost!";
            if(message != null) {
                msgBoxText += "\n\r" + message;
            }

            MessageBox.Show(msgBoxText, "Disconnected", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DisconnectClick(object sender, RoutedEventArgs e) {
            Disconnect("Disconnected by User.");
        }
    }
}
