﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;

namespace HedgehogClient {
    /// <summary>
    /// Interaction logic for HedgehogClientWindow.xaml
    /// </summary>
    public partial class HedgehogClientWindow {
        private int _currentSpeed = 5;
        public readonly IPAddress Address;
        private TcpClient _client;
        private SocketStatus _status;
        private ControlKeys.MovementKey _currentKey = ControlKeys.MovementKey.Stop;

        private SocketStatus Status {
            get { return _status; }
            set {
                ChangeCursor(value == SocketStatus.Busy);
                _status = value;
            }
        }

        private static TaskCompletionSource<bool> _tcs;

        //Port for Hedgehog Server
        private const int Port = 3131;
        //Timeout in Milliseconds for Sends
        private const int SendTimeout = 3000;

        private enum SocketStatus {
            Disconnected,
            Connecting,
            Connected,
            Busy
        }

        private enum LogType {
            Debug,
            Info,
            Error
        }

        //Constructor
        public HedgehogClientWindow(IPAddress address) {
            InitializeComponent();
            Address = address;
            ipLabel.Content = Address + ":" + Port;

            Connect();
            ControlKeys.RegisterXboxInput(WindowKeyDown);
        }

        //Set Green or Red Hedgehog Icon
        private void SetHedgehogIcon(bool green) {
            try {
                if(green) {
                    using(Icon icon = Properties.Resources.Hedgehog_Green) {
                        using(Bitmap bitmap = icon.ToBitmap()) {
                            IntPtr hBitmap = bitmap.GetHbitmap();
                            ImageSource source =
                                Imaging.CreateBitmapSourceFromHBitmap(
                                    hBitmap, IntPtr.Zero, Int32Rect.Empty,
                                    BitmapSizeOptions.FromEmptyOptions());
                            statusImage.Source = source;
                            Icon = source;
                        }
                    }
                } else {
                    using(Icon icon = Properties.Resources.Hedgehog_Red) {
                        using(Bitmap bitmap = icon.ToBitmap()) {
                            IntPtr hBitmap = bitmap.GetHbitmap();
                            ImageSource source =
                                Imaging.CreateBitmapSourceFromHBitmap(
                                    hBitmap, IntPtr.Zero, Int32Rect.Empty,
                                    BitmapSizeOptions.FromEmptyOptions());
                            statusImage.Source = source;
                            Icon = source;
                        }
                    }
                }
            } catch {
                //ignored
            }
        }

        //Disconnected Callback
        private void Disconnected(bool byUser, string message = null) {
            //Run on Main Thread
            Dispatcher.BeginInvoke(new Action(delegate {
                DisconnectButton.IsEnabled = false;
                DisconnectButton.ToolTip = "Already disconnected";
                Status = SocketStatus.Disconnected;
                statusLabel.Content = "Disconnected";
                statusLabel.Foreground = Brushes.Red;

                string msgBoxText = "The connection to the Hedgehog has been lost!";
                if(message != null) {
                    msgBoxText += "\n\r" + message;
                }

                SetHedgehogIcon(false);

                if(!byUser)
                    MessageBox.Show(msgBoxText, "Disconnected", MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        //Log to Console
        private void Log(string message, LogType logType) {
            //invoke on main Thread
            Dispatcher.BeginInvoke(new Action(delegate {
                //Only show Debug Log when DEBUG mode is enabled
                if(logType == LogType.Debug) {
#if DEBUG
                    logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] [{logType}] > " + message + Environment.NewLine);
                    logBox.ScrollToEnd();
#endif
                } else {
                    logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] [{logType}] > " + message + Environment.NewLine);
                    logBox.ScrollToEnd();
                }
            }));
        }

        //Change Hedgehog Speed Indicator
        private void UpdateSpeed(bool increase) {
            Dispatcher.BeginInvoke(new Action(delegate {
                try {
                    if(increase) {
                        if(_currentSpeed < 10)
                            speedLabel.Content = (++_currentSpeed).ToString();
                    } else {
                        if(_currentSpeed > 0)
                            speedLabel.Content = (--_currentSpeed).ToString();
                    }
                } catch {
                    //ignored
                }
            }));
        }




        #region Socket
        //Connect the Server and give User Feedback
        private async void Connect() {
            Log("Connecting to Hedgehog...", LogType.Info);
            Cursor = Cursors.AppStarting;
            statusLabel.Content = "Connecting...";
            statusLabel.Foreground = Brushes.Orange;

            _client = new TcpClient();

            try {
                Status = SocketStatus.Connecting;

                await _client.ConnectAsync(Address, Port);

                if(_client.Connected) {
                    byte[] buffer = new byte[1];
                    _client.Client.BeginReceive(buffer, 0, 1, SocketFlags.None, Received, null);

                    Log("Connected!", LogType.Info);
                    Status = SocketStatus.Connected;
                    statusLabel.Content = "Connected";
                    statusLabel.Foreground = Brushes.Green;
                    SetHedgehogIcon(true);
                } else {
                    throw new Exception("Could not connect to Hedgehog, wrong or no response!");
                }
            } catch(Exception e) {
                Log("Error Connecting!", LogType.Error);
                Log(e.Message, LogType.Error);
                DisconnectButton.IsEnabled = false;
                DisconnectButton.ToolTip = "Not yet connected";
                Status = SocketStatus.Disconnected;
                statusLabel.Content = "Disconnected";
                statusLabel.Foreground = Brushes.Red;
                SetHedgehogIcon(false);
                MessageBox.Show($"Could not Connect!\n\r{e.Message}", "Error Connecting to Hedgehog!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Cursor = Cursors.Arrow;
        }

        //Send Key (Integer from 0 to 7) to Hedgehog
        private async Task SendKey(ControlKeys.MovementKey movementKey) {
            int i = 0;

            while(Status == SocketStatus.Busy) {
                await Task.Delay(10);
                i += 10;

                if(i >= SendTimeout) {
                    Log("Could not Send Message, Hedgehog Send Request Timed out!", LogType.Error);
                    throw new HedgehogException("Hedgehog Send Request Timed out!");
                }
            }

            if(Status == SocketStatus.Disconnected || Status == SocketStatus.Connecting) {
                throw new HedgehogException("Not connected to Hedgehog!");
            }

            Status = SocketStatus.Busy;

            byte key = (byte)movementKey;
            byte[] message = { key };

            //_currentKey = movementKey;
            _client.Client.BeginSend(message, 0, 1, SocketFlags.None, Sent, movementKey);
            Log($"Sending Key [{movementKey}]...", LogType.Debug);
        }

        //Mesage Sent Callback
        private void Sent(IAsyncResult result) {
            ControlKeys.MovementKey key = (ControlKeys.MovementKey)result.AsyncState;

            if(result.IsCompleted) {
                Log($"Key [{key} ({(byte)key})] sent!", LogType.Debug);
                _tcs?.TrySetResult(true);
            } else {
                Log($"Error sending [{key} ({(byte)key})] Key...", LogType.Error);
                Disconnected(false, "Tried to send Message to Hedgehog, failed");
                _tcs?.TrySetResult(false);
            }

            //Speed Indicator
            if(key == ControlKeys.MovementKey.Plus) {
                UpdateSpeed(true);
            } else if(key == ControlKeys.MovementKey.Minus) {
                UpdateSpeed(false);
            }

            Status = SocketStatus.Connected;
            _client.Client.EndSend(result);
        }

        //Message Received Callback       | On Receive from Server => Socket closed
        private void Received(IAsyncResult result) {
            try {
                _client.Client.EndReceive(result);
                Disconnected(false, "Server shut down connection!");
            } catch {
                // ignored
            }
        }

        //Disconnect the Client
        private async void Disconnect(bool byUser, string message = null) {
            while(Status == SocketStatus.Busy) {
                await Task.Delay(10);
            }

            if(Status == SocketStatus.Connected) {
                try {
                    _tcs = new TaskCompletionSource<bool>();
                    await SendKey(ControlKeys.MovementKey.NullByte);
                    await _tcs.Task;
                } catch {
                    //already disconnected
                }

                _client.Close();
                Disconnected(byUser, message);
            }
        }

        //Change to Loading or normal Cursor
        private void ChangeCursor(bool loading) {
            Dispatcher.BeginInvoke(new Action(delegate {
                Cursor = loading ? Cursors.Wait : Cursors.Arrow;
            }));
        }
        #endregion

        #region Action Listeners
        //KeyUp Unlocks a Key (e.g. W)
        private async void WindowKeyUp(object sender, KeyEventArgs e) {
            try {
                _currentKey = ControlKeys.MovementKey.Stop;
                await SendKey(ControlKeys.MovementKey.Stop);
            } catch {
                // ignored
            }

            keyLabel.Content = "/";
        }

        //KeyDown Locks a Key (e.g. W) and drives forward till KeyUp)
        private async void WindowKeyDown(object sender, KeyEventArgs e) {
            try {
                if(_currentKey != ControlKeys.MovementKey.Stop)
                    return;

                keyLabel.Content = "Sending...";

                Key key = e.Key;
                ControlKeys.MovementKey movementKey = ControlKeys.GetKey(key);
                _currentKey = movementKey;

                _tcs = new TaskCompletionSource<bool>();
                await SendKey(ControlKeys.MovementKey.Stop);
                await _tcs.Task;

                _tcs = new TaskCompletionSource<bool>();
                await SendKey(movementKey);
                await _tcs.Task;

                keyLabel.Content = ControlKeys.FriendlyStatus(movementKey);
            } catch {
                keyLabel.Content = "/";
                //Wrong input
            }
        }

        //Disconnect Button
        private void DisconnectClick(object sender, RoutedEventArgs e) {
            Disconnect(true, "Disconnected by User.");
        }

        //Clear Button
        private void ClearClick(object sender, RoutedEventArgs e) {
            logBox.Clear();
        }

        //Close Window
        private void WindowClosing(object sender, CancelEventArgs e) {
            Disconnect(true, "Window closed");

            new MainWindow().Show();
        }
        #endregion
    }
}
