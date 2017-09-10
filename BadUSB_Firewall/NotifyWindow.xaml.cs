/**
**************************************************************************************************
* @file	   NotifyWindow.xaml.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Benachrichtigungsfenster für die erweiterten Whitelist-Überwachungs u. Schutzfunktionen
**************************************************************************************************
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace BadUSB_Firewall
{
    /// <summary>
    /// Description
    /// </summary>
    /// <param name="">Param Description</param>
    public partial class NotifyWindow : INotifyPropertyChanged
    {
        private string _counter;
        private readonly int timeToClose = 60;  // Das jeweilige Fenster schließt sich nach 60 Sekunden automatisch
        private System.Timers.Timer _countTimer;
        Thread _newWindowThread;
        private readonly string _dateAdded;
        private int _count;
        private readonly bool _connected;
        private readonly ChangeDeviceState _cdsLib = new ChangeDeviceState();
        private readonly List<ChangeStateDevice> _devices;
        private readonly bool _blockedDevice;
        private readonly bool _blockedPort;
        private readonly bool _prohibitPortChange;
        private readonly bool _portChange;

        public string ActualCounter
        {
            get { return _counter; }
            set
            {
                if (bool.Equals(value, _counter))
                    return;
                _counter = value;
                OnPropertyChanged("ActualCounter");
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public NotifyWindow(List<ChangeStateDevice> usbDevices, string added, bool actualConnected, bool blocked, bool blockPortChange, bool prohibitPort, bool notifyPortChange)
        {
            _dateAdded = added;
            _devices = usbDevices;
            _blockedDevice = blocked;
            _blockedPort = blockPortChange;
            _connected = actualConnected;
            _prohibitPortChange = prohibitPort;
            _portChange = notifyPortChange;

            InitializeComponent();
            DataContext = this;
            if (prohibitPort)
            {
                Title = "Whitelist device connected to other USB-Port";
            }
            else if (_blockedDevice && _connected)
            {
                Title = "Already connected Whitelist device notification";
            }
            else if (_blockedPort)
            {
                Title = "Whitelist device was connected to false USB-Port";
            }
            else if (_portChange)
            {
                Title = "Whitelist device was connected to another USB-Port";
            }
            else
            {
                Title = "Whitelist device notification";
            }
            ActualCounter = timeToClose.ToString();
            _count = timeToClose;

            DisplayText();
            StartCloseTimer();
            StartWindowTimer();
        }

        /// <summary>
        /// Startet den automatischen Timer. Sollte innerhalb 60 Sekunden bei geöffneten Fenster
        /// keine Benutzeraktion erfolgen, so wird das Fenster automatisch geschlossen.
        /// </summary>
        /// <param name="">Param Description</param>
        private void StartWindowTimer()
        {
            _newWindowThread = new Thread(ThreadStartingPoint);
            _newWindowThread.SetApartmentState(ApartmentState.STA);
            _newWindowThread.IsBackground = true;
            _newWindowThread.Start();
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void ThreadStartingPoint()
        {
            _countTimer = new System.Timers.Timer {Interval = 1000};
            //1 second
            _countTimer.Elapsed += timer_Elapsed;
            _countTimer.Start();

            Dispatcher.Run();
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void timer_Elapsed(object sender, EventArgs e)
        {
            if (_count == 0)
            {
                if (_countTimer != null)
                {
                    _countTimer.Dispose();
                    _countTimer.Stop();
                    _countTimer = null;
                }
            }
            else
            {
                _count--;
                ActualCounter = _count.ToString();
            }
        }

        /// <summary>
        /// Zeigt den Text abhängig der gewählten Whitelist-Überwachungsfunktionen an.
        /// </summary>
        /// <param name="">Param Description</param>
        private void DisplayText()
        {
            int foundPos = -1;

            for (int i = 0; i < _devices.Count; i++)
            {
                if (_devices[i].mLocationInformation.Contains("Port")) { foundPos = i; }
            }

            if ((_devices.Count > 0) && (foundPos > -1))
            {


                var msg = "Device: " + _devices[foundPos].mDeviceType.ToUpper();
                if (_devices.Count > 1)
                {
                    msg += "\nInterfaces: ";
                    for (int i = 0; i < _devices.Count; i++)
                    {
                        if (i != foundPos)
                        {
                            msg += "\n  ->" + _devices[i].mDeviceType.ToUpper();
                        }
                    }
                }
                TextWindow.Inlines.Add(new Run(msg + Environment.NewLine) { FontWeight = FontWeights.Bold, FontSize = 12, Foreground = Brushes.Black });
                TextWindow.Inlines.Add("VendorID: " + _devices[foundPos].mVendorID + " ProductID: " + _devices[foundPos].mProductID);

                //Portwechsel verbieten. An richtigen Geräteanschluss könnte das Gerät durch drücken 
                //des "Enable" Knopf wieder aktiviert werden
                if (_prohibitPortChange)
                {
                    EnableButton.IsEnabled = true;
                    DisableButton.IsEnabled = false;

                }
                //Gerät hat Anschluss gewechselt. Möglichkeit zum Blockieren geben.
                else if (_portChange)
                {
                    EnableButton.IsEnabled = false;
                    DisableButton.IsEnabled = true;

                }
                //Gerät wurde blockiert
                else if (_blockedPort && !_portChange)
                {
                    DisableButton.IsEnabled = false;
                    EnableButton.IsEnabled = false;
                }
                else
                {
                    DisableButton.IsEnabled = true;
                    EnableButton.IsEnabled = false;
                }

                if (_prohibitPortChange)
                {
                    TextWindow.Inlines.Add("\nAdded to whitelist: " + _dateAdded + "\nwas connected to another USB-port than the first connection.\nand was therefore ");
                    TextWindow.Inlines.Add(new Run("BLOCKED !") { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black });
                    TextWindow.Inlines.Add("\n\nWan't to use the device on this Port?\n");
                    TextWindow.Inlines.Add(new Run("-> than disable theProhibit Port Entry in the Configuration menu!") { FontStyle = FontStyles.Italic });
                    TextWindow.Inlines.Add(new Run("\n\nSKIP this message? => Press the (CLOSE WINDOW) button.") { FontSize = 12, FontWeight = FontWeights.Bold });
                    TextWindow.Inlines.Add(new Run("\nENABLE device? => Change connection & press (ENABLE DEVICE) button.") { FontSize = 12, FontWeight = FontWeights.Bold });
                }
                else
                {
                    //Ein anderes Gerät mit den selben Eigenschaften ist aktuell bereits verbunden.
                    if (_connected)
                    {

                        TextWindow.Inlines.Add("\nAdded to whitelist: " + _dateAdded + "\nis actual already connected on your system and also in the Whitelist");
                        if (_blockedDevice && _connected && !_portChange)
                        {
                            DisableButton.IsEnabled = false;

                            TextWindow.Inlines.Add("\nand was therefore ");
                            TextWindow.Inlines.Add(new Run("BLOCKED !") { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black });
                        }
                        else if (_connected)
                        {
                            TextWindow.Inlines.Add(".");

                        }
                    }
                    else if (_portChange)
                    {
                        TextWindow.Inlines.Add("\nAdded to whitelist: " + _dateAdded + "\nwas now connected to a different usb port as it was allowed.\n");
                    }
                    else
                    {
                        TextWindow.Inlines.Add("\nAdded to whitelist: " + _dateAdded + "\nis actual already in the Whitelist !");
                    }

                    if (_portChange)
                    {
                        TextWindow.Inlines.Add("\nIf you don't want to use this device on this USB-port then\n");
                    }
                    else
                    {
                        TextWindow.Inlines.Add("\nIf this is the first time you have connected this device or you don't have\nidentical devices then ");
                    }
                    if (_blockedDevice && _connected && !_portChange)
                    {
                        TextWindow.Inlines.Add(new Run("DISCONNECT") { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black });
                        TextWindow.Inlines.Add(" it");

                        TextWindow.Inlines.Add(new Run("\n\nSKIP this message? => Press the CLOSE WINDOW button.") { FontSize = 12, FontWeight = FontWeights.Bold });
                    }
                    else
                    {
                        TextWindow.Inlines.Add(new Run("DISCONNECT or BLOCK") { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black });
                        TextWindow.Inlines.Add(" it");
                        TextWindow.Inlines.Add(new Run("\n\nSKIP this message? => Press the CLOSE WINDOW button.") { FontSize = 12, FontWeight = FontWeights.Bold });
                        TextWindow.Inlines.Add(new Run("\nBLOCK this device? => Press the DISABLE DEVICE button.") { FontSize = 12, FontWeight = FontWeights.Bold });
                    }
                }
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void StartCloseTimer()
        {
            DispatcherTimer timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(timeToClose)};
            timer.Tick += TimerTick;
            timer.Start();
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void TimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
            timer.Tick -= TimerTick;
            Close();
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Funktion zur Behandlung des drückens des "Enable"-Knopfs.
        /// Gerät wird wieder aktiviert.
        /// </summary>
        /// <param name="">Param Description</param>
        public void EnableButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = _devices.Count; i > 0; i--)
            {
                var result = _cdsLib.ChangeDevState(_devices[i - 1].mHardwareID, _devices[i - 1].mClassGuid, _devices[i - 1].mLocationInformation, true, ChangeDeviceStateParams.Enable);

                if (result == (int)ErrorCode.ErrorInvalidData)
                {
                    result = _cdsLib.ChangeDevState(_devices[i - 1].mHardwareID, _devices[i - 1].mClassGuid, _devices[i - 1].mLocationInformation, true, ChangeDeviceStateParams.Enable);
                }
                else if (result == (int)ErrorCode.ErrorNoSuchDevinst)
                {
                    _cdsLib.ForceReenumeration();
                    result = _cdsLib.ChangeDevState(_devices[i - 1].mHardwareID, _devices[i - 1].mClassGuid, _devices[i - 1].mLocationInformation, true, ChangeDeviceStateParams.Enable);
                }
                else if (result == (int)ErrorCode.Success)
                {
                    //  showMessage("Succesfully activated the device \nName: " + name + "\nDevice-Type: " + deviceType + "\nDeviceID: " + deviceID, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (result == (int)ErrorCode.NotFound)
                {
                    _cdsLib.ForceReenumeration();
                    result = _cdsLib.ChangeDevState(_devices[i - 1].mHardwareID, _devices[i - 1].mClassGuid, _devices[i - 1].mLocationInformation, true, ChangeDeviceStateParams.Enable);
                    if (result == (int)ErrorCode.NotFound)
                    {
                        string actualPort = _cdsLib.get_DevicePort(_devices[i - 1].mHardwareID, _devices[i - 1].mLocationInformation, _devices[i - 1].mClassGuid);
                        if (actualPort != "" && actualPort != _devices[i - 1].mLocationInformation)
                        {
                            //device was found on another port of the system
                            result = _cdsLib.ChangeDevState(_devices[i - 1].mHardwareID, _devices[i - 1].mClassGuid, actualPort, true, ChangeDeviceStateParams.Enable);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(_devices[i - 1].mDeviceType + " \ncould not be enabled. Please replug the device for activation!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            _count = 0;
            Close();
        }

        /// <summary>
        /// Geräteblockierung wurde ausgewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        public void DisableButton_Click(object sender, RoutedEventArgs e)
        {
            if (_blockedDevice && _connected && !_portChange)
            {
            }
            else
            {

                if (_devices.Count > 0)
                {
                    int result;
                    if (_devices[0].mIsComposite)
                    {
                        result =  _cdsLib.ChangeDevState(_devices[0].mHardwareID, _devices[0].mClassGuid, _devices[0].mLocationInformation, true, ChangeDeviceStateParams.DisableComposite);
                    }
                    else
                    {
                        result = _cdsLib.disable_USBDevice(_devices[0].mHardwareID, _devices[0].mClassGuid, _devices[0].mLocationInformation, true);
                    }
                   if (result == (int)ErrorCode.Success)
                    {
                        if (_devices.Count > 1)
                        {
                            string msg = "\nwith the Interfaces\n";
                            for (int i = 1; i < _devices.Count; i++)
                            {
                                msg +="  ->"+ _devices[i].mDeviceType + "\n";
                            }
                                MessageBox.Show("The device "+_devices[0].mDeviceType + msg+"\nwas succesfully disabled.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("The device "+_devices[0].mDeviceType + " \nwas succesfully disabled.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                    }
                    else
                    {
                        if (_devices.Count > 1)
                        {
                            string msg = "\nwith the Interfaces\n";
                            for (int i = 1; i < _devices.Count; i++)
                            {
                                msg += "  ->" + _devices[i].mDeviceType + "\n";
                            }
                            MessageBox.Show("The device " + _devices[0].mDeviceType + msg + "\ncould not be disabled. Please replug the device!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            MessageBox.Show("The device " + _devices[0].mDeviceType + " \ncould not be disabled. Please replug the device!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }

            _count = 0;
            Close();
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _count = 0;
            Close();
        }
    }
}
