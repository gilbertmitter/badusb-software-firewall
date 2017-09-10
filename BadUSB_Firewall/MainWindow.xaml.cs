/**
******************************************************************************
* @file	   MainWindow.xaml.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Hauptlogik der Software-Firewall
******************************************************************************
*/
#region using_directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Deployment.Application;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Management;
using Microsoft.Win32;  	// Wird für Änderungen der Registrierungseinstellungen benötigt
using System.Diagnostics;
using System.Security.Principal;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using BadUSB_Firewall.Properties;
#endregion

namespace BadUSB_Firewall
{
    #region Public_Enum
    /// <summary>
    /// Verwendete Fehlercodes
    /// </summary>
    public enum ErrorCode
    {
        ExceptionError = -1,
        InvalidHandleValue = -1,
        NotFound = -2,
        ReenumerationError = -2,
        Success = 0,
        ErrorInvalidData = 13,
        InvalidHandleError = 6,
        ErrorSuccessRebootRequired = 3010, // (0xBC2) The requested operation is successful. Changes will not be effective until the system is rebooted.
        ErrorNotDisableable = -536870351,//unchecked((int)0xe0000231),
        ErrorNoSuchDevinst = -536870389 //0xe000020b
    }

    /// <summary>
    /// Geräteeigenschaften-Codes für Registrierung
    /// </summary>
    public enum SpdrpCodes : uint
    {
        SpdrpDevicedesc = 0x00000000,
        SpdrpMfg = 0x0000000B,
        SpdrpHardwareid = 0x00000001,
        SpdrpCompatibleids = 0x00000002,
        SpdrpClassguid = 0x00000008,
        SpdrpLocationInformation = 0x0000000D,
        SpdrpDevtype = 0x00000019,
        SpdrpService = 0x00000004
    }

    #endregion
    #region Classdefinitions
    //Wird für WM_DEVICECHANGE benötigt
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DEV_BROADCAST_DEVICEINTERFACE
    {
        public int dbcc_size;
        public int dbcc_devicetype;
        public int dbcc_reserved;
        public Guid dbcc_classguid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string dbcc_name;
    }
    #region Class_DeviceEntry
    /// <summary>
    /// Klasse, welche für das Firewall-Regeln Fenster benötigt wird(FirewallRules.xaml and FirewallRules.xaml.cs
    /// </summary>
    public class DeviceEntry : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private string _mName;
        private string _mChecksum;
        private string _mClassGuid;
        private string _mDateAdded;
        private string _mDeviceType;
        private string _mDeviceID;
        private string _mLastLocation;
        private string _mHardwareID;
        private string _mProductID;
        private string _mProductName;
        private string _mSerialNumber;
        private string _mService;

        private string _mUSB_Class;
        private string _mUSB_SubClass;
        private string _mUSB_Protocol;
        private string _mVendorID;
        private string _mVendorName;
        private string _mFirstLocation;
        private bool _Added_BlackList;
        private bool _Added_WhiteList;

        private bool _Remove_Device;
        private bool _In_BlackList;
        private bool _In_WhiteList;
        private bool __CheckBox_Enabled;

        public string mName
        {
            get { return _mName; }
            set
            {
                _mName = value;
                OnPropertyChanged("mName");
            }
        }
        public string mChecksum
        {
            get { return _mChecksum; }
            set
            {
                _mChecksum = value;
                OnPropertyChanged("mChecksum");
            }
        }
        public string mClassGuid
        {
            get { return _mClassGuid; }
            set
            {
                _mClassGuid = value;
                OnPropertyChanged("mClassGuid");
            }
        }
        public string mDateAdded
        {
            get { return _mDateAdded; }
            set
            {
                _mDateAdded = value;
                OnPropertyChanged("mDateAdded");
            }
        }
        public string mDeviceType
        {
            get { return _mDeviceType; }
            set
            {
                _mDeviceType = value;
                OnPropertyChanged("mDeviceType");
            }
        }

        public string mDeviceID
        {
            get { return _mDeviceID; }
            set
            {
                _mDeviceID = value;
                OnPropertyChanged("mDeviceID");
            }
        }

        public string mHardwareID
        {
            get { return _mHardwareID; }
            set
            {
                _mHardwareID = value;
                OnPropertyChanged("mHardwareID");
            }
        }

        public string mLastLocation
        {
            get { return _mLastLocation; }
            set
            {
                _mLastLocation = value;
                OnPropertyChanged("mLastLocation");
            }
        }
        public string mProductID
        {
            get { return _mProductID; }
            set
            {
                _mProductID = value;
                OnPropertyChanged("mProductID");
            }
        }
        public string mProductName
        {
            get { return _mProductName; }
            set
            {
                _mProductName = value;
                OnPropertyChanged("mProductName");
            }
        }
        public string mSerialNumber
        {
            get { return _mSerialNumber; }
            set
            {
                _mSerialNumber = value;
                OnPropertyChanged("mSerialNumber");
            }
        }
        public string mService
        {
            get { return _mService; }
            set
            {
                _mService = value;
                OnPropertyChanged("mService");
            }
        }

        public string mUSB_Class
        {
            get { return _mUSB_Class; }
            set
            {
                _mUSB_Class = value;
                OnPropertyChanged("mUSB_Class");
            }
        }
        public string mUSB_SubClass
        {
            get { return _mUSB_SubClass; }
            set
            {
                _mUSB_SubClass = value;
                OnPropertyChanged("mUSB_Subclass");
            }
        }
        public string mUSB_Protocol
        {
            get { return _mUSB_Protocol; }
            set
            {
                _mUSB_Protocol = value;
                OnPropertyChanged("mUSB_Protocol");
            }
        }
        public string mVendorID
        {
            get { return _mVendorID; }
            set
            {
                _mVendorID = value;
                OnPropertyChanged("mVendorID");
            }
        }
        public string mVendorName
        {
            get { return _mVendorName; }
            set
            {
                _mVendorName = value;
                OnPropertyChanged("mVendorName");
            }
        }
        public string mFirstLocation
        {
            get { return _mFirstLocation; }
            set
            {
                _mFirstLocation = value;
                OnPropertyChanged("mFirstLocation");

            }
        }
        public bool Added_BlackList
        {
            get { return _Added_BlackList; }
            set
            {
                _Added_BlackList = value;
                OnPropertyChanged("Added_BlackList");
            }
        }
        public bool Added_WhiteList
        {
            get { return _Added_WhiteList; }
            set
            {
                _Added_WhiteList = value;
                OnPropertyChanged("Added_WhiteList");
            }
        }
        public bool Remove_Device
        {
            get { return _Remove_Device; }
            set
            {
                _Remove_Device = value;
                OnPropertyChanged("Remove_Device");
            }
        }
        public bool In_BlackList
        {
            get { return _In_BlackList; }
            set
            {
                _In_BlackList = value;
                OnPropertyChanged("In_BlackList");
            }
        }
        public bool In_WhiteList
        {
            get { return _In_WhiteList; }
            set
            {
                _In_WhiteList = value;
                OnPropertyChanged("In_WhiteList");
            }
        }
        public bool CheckBox_Enabled
        {
            get { return __CheckBox_Enabled; }
            set
            {
                __CheckBox_Enabled = value;
                OnPropertyChanged("CheckBox_Enabled");
            }
        }

        public DeviceEntry() { }
        public DeviceEntry(string name, string checksum, string classGuid, string dateAdded, string deviceType, string deviceID, string hardwareID, string lastLocation, string productID, string productName, string serialNumber, string service,
                            string uSB_Class, string uSB_Protocol, string uSB_SubClass, string vendorID, string vendorName, string firstLocation, bool inBlacklist, bool inWhiteList)
        {
            mName = name;
            Remove_Device = false;
            mChecksum = checksum;
            mClassGuid = classGuid;
            mDateAdded = dateAdded;
            mDeviceID = deviceID;
            string[] hwIDParts = hardwareID.Split(@" ".ToCharArray());
            mHardwareID = "";
            if (hwIDParts.Length > 0)
            {
                mHardwareID = hwIDParts[0];
            }
            mLastLocation = lastLocation;
            mProductID = productID;
            mProductName = productName;
            mSerialNumber = serialNumber;
            mService = service;
            mUSB_Class = uSB_Class;
            mUSB_Protocol = uSB_Protocol;
            mUSB_SubClass = uSB_SubClass;
            mVendorID = vendorID;
            mVendorName = vendorName;
            mFirstLocation = firstLocation;
            Added_BlackList = inBlacklist;
            Added_WhiteList = inWhiteList;
            In_BlackList = inBlacklist;
            In_WhiteList = inWhiteList;
            // Main device
            if (firstLocation.ToUpper().Contains("PORT"))
            {
                mDeviceType = deviceType;
                CheckBox_Enabled = true;
            }
            //interface of device
            else
            {
                mDeviceType = "---> " + deviceType;
                CheckBox_Enabled = false;
            }
        }
    }
    #endregion
    #region Class_USBDevice
    /// <summary>
    /// Objekt welches für neu erkannte Geräte verwendet und in der GUI angezeigt wird 
    /// </summary>
    public class USBDevice
    {
        public USBDevice(USBDeviceInfo device)
        {
            mDevice = device;//Hauptgerät
            mInterfaces = new ObservableCollection<USBDeviceInfo>();//eventuelle Schnittstellen
        }
        public string mPosition { get; set; }
        public USBDeviceInfo mDevice { get; set; }
        public ObservableCollection<USBDeviceInfo> mInterfaces { get; set; }
    }
    #endregion
    #region Class_ChangeStateDevice
    /// <summary>
    /// USB Objekt für die Weiß- und Blackliste
    /// </summary>
    public class ChangeStateDevice
    {
        public ChangeStateDevice(string name, string hardwareID, string classGuid, string deviceID, string deviceType, string location, string vendorID, string productID, string checksum)
        {
            mHardwareID = hardwareID;
            mClassGuid = classGuid;
            mDeviceID = deviceID;
            mDeviceType = deviceType;
            mLocationInformation = location;
            mVendorID = vendorID;
            mProductID = productID;
            mChecksum = checksum;
        }
        public ChangeStateDevice(USBDeviceInfo device)
        {
            mHardwareID = device.HardwareID;
            mClassGuid = device.ClassGuid;
            mDeviceID = device.DeviceID;
            mDeviceType = device.DeviceType;
            mLocationInformation = device.FirstLocationInformation;
            mVendorID = device.VendorID;
            mProductID = device.ProductID;
            mChecksum = device.Checksum;
            if (DeviceClass.IsComposite(device.USB_Class, device.USB_SubClass, device.USB_Protocol, device.Service))
            {
                mIsComposite = true;
            }
            else
            {
                mIsComposite = false;
            }

        }

        public string mName { get; set; }
        public string mHardwareID { get; set; }
        public string mClassGuid { get; set; }
        public string mDeviceID { get; set; }
        public string mDeviceType { get; set; }
        public string mLocationInformation { get; set; }
        public string mVendorID { get; set; }
        public string mProductID { get; set; }
        public string mChecksum { get; set; }
        public bool mIsComposite { get; set; }
    }
    #endregion
    #endregion
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class BadUSBFirewall : INotifyPropertyChanged
    {
        #region Guid_Codes
        //GUID Codes, welche in der Anwendung erkannt werden
        private static readonly Guid GUID_DEVCLASS_NETWORK = new Guid("{4D36E972-E325-11CE-BFC1-08002BE10318}");
        private static readonly Guid Guid_USBDevice = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED");
        private static readonly Guid Guid_HID = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030");
        #endregion
        #region DataTypes
        public static string BaseDir;
        private static ReducedInfoWidget _noteWindow;
        private readonly string _appTitle = Settings.Default.MyTitle + ": ";
        private static List<string> TempList = new List<string>();
        private string _imgName = "green.PNG";
        private string _appPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        private const int ERROR_NOT_DISABLEABLE = unchecked((int)0xe0000231);// decimal -536870351(on 32bit system) or 3758096945 ERROR_NOT_DISABLEABLE 
        volatile bool _isRunning = false;
        private bool _continueApp = false;
        private bool _stopApp = false;

        //Informationsfenster das beim ersten Start angezeigt wird
        private FirstStart _firstStartWindow;

        private static bool _isAdmin = false;
        private static int _uSbElementsInList = 0;
        private static int _sysKeyboardExit = 0;
        private static int _sysKeyboardStart = 0;
        private static int _sysKeyboardNow = 0;
        private static int _sysPointingDevicesExit = 0;
        private static int _sysPointingDevicesStart = 0;
        private static int _sysPointingDevicesNow = 0;
        private static int _sysNetworkAdapterExit = 0;
        private static int _sysNetworkAdapterStart = 0;
        private static int _sysNetworkAdapterNow = 0;

        readonly bool _firstRun = false;
        bool _addDifDevices = false;
        int _cnt = 0;

        private static readonly List<Guid> GuidDeviceList = new List<Guid>();
        private List<string> HandleNextTime = new List<string>();
        private List<string> LogMessages = new List<string>();
        private List<string> SelectedTreeItems = new List<string>();
        private List<NetAdapterItem> _initNetworkAdapters = new List<NetAdapterItem>();
        private ObservableCollection<NetAdapterItem> _netItems = new ObservableCollection<NetAdapterItem>();
        private ObservableCollection<USBDevice> _usbDevices = new ObservableCollection<USBDevice>();
        private ObservableCollection<USBDeviceInfo> _usbItems = new ObservableCollection<USBDeviceInfo>();
        //Changestate Klasse
        private static readonly ChangeDeviceState CdsLib = new ChangeDeviceState();
        //Datenbanken Klasse
        private DeviceLists _devLists;
        //Trayleisten-Symbol
        readonly System.Windows.Forms.NotifyIcon _appIcon = new System.Windows.Forms.NotifyIcon();
        #endregion
        //Für WM_Devicechange benötigte Variablen und Strukturen
        #region Native_Functions
        private const int DBT_DEVICEARRIVAL = 0x8000; //A device or piece of media has been inserted and is now available.        
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // A device or piece of media has been removed.
        private const int DBT_CONFIGCHANGED = 0x0018;//The current configuration has changed, due to a dock or undock.
        private const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;//Class of devices. 
        public const int WM_DEVICECHANGE = 0x0219; // Notifies an application of a change to the hardware configuration of a device or the computer.
        private static IntPtr notificationHandle;//device notification handle

        //Retrieved by handling WM_CHANGEDEVICE Message's LPARAM property value. 
        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);

        #endregion

        /// <summary>
        /// Hauptklasse der Anwendung
        /// </summary>
        /// <param name="">Param Description</param>
        public BadUSBFirewall()
        {
            try
            {
                //Ladebildschirm
                var splash = new SplashScreen();
                splash.Show();
                //Erhöhe die Prozesspriorität
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

                _firstRun = Settings.Default.FirstStart;
                //Initialisierung der Anwendungskomponenten
                InitializeComponent();

                UsbTreeView.ItemsSource = USBDevices;
                DataContext = this;
                //Aufruf der Initialisierungsfunktion
                ApplicationInit();
                //Schließe Ladebildschirm
                splash.Close();

                if (Settings.Default.StopApplication)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        //for the icon
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();

            base.OnStateChanged(e);
        }

        #region Application_Init
        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void YesPressed(object sender, YesPressedArgs e)
        {
            _continueApp = e.Pressed;
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void NoPressed(object sender, NoPressedArgs e)
        {
            _stopApp = e.Pressed;
        }

        /// <summary>
        /// Anwendungsinitialisierungs-Funktion
        /// </summary>
        /// <param name="">Param Description</param>
        private void ApplicationInit()
        {
            BaseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _appPath = BaseDir;

            if (BaseDir != null && File.Exists(Path.Combine(BaseDir, "Resources", "BadUSB-Firewall.ico")))
            {
                //for the icon in the traybar
                _appIcon.Icon = new System.Drawing.Icon(Path.Combine(BaseDir, "Resources", "BadUSB-Firewall.ico"));
            }
            else
            {
                MessageBox.Show("BadUSB_Logger.ico was not found in the Resource directory!");
            }

            _appIcon.Text = Settings.Default.MyTitle;
            _appIcon.Visible = true;
            _appIcon.Click +=
                    delegate
                    {
                        Show();
                        WindowState = WindowState.Normal;
                    };

            _devLists = new DeviceLists();
            //Eventhandler für durchgeführte Operationen in der White-oder Blacklistdatenbank
            _devLists.WhiteListAdded += BlackToWhiteList;
            _devLists.BlackListAdded += WhiteToBlackList;

            //Windows Logoff oder herunterfahren erkennen
            SystemEvents.SessionEnding += OnSessionEnding;

            Settings.Default.Upgrade();

            build_NetList();

            //Anzahl der Tastaturen, Zeige- und Netzwerkgeräte bei Programmstart für
            // letzte Beendigung und aktuelle Anzahl ermitteln
            //Netzwerkgeräte
            SysNetworkAdapterStart = NetItems.Count;                        //Startanzahl
            SysNetworkAdapterNow = NetItems.Count;                          //Anzahl jetzt gerade (Bei Programmstart = Startwert)
            SysNetworkAdapterExit = Settings.Default.AppExitNetworkAdapters;//Anzahl bei letzter Programmbeendigung
            //Tastaturen
            SysKeyboardStart = detect_KeyboardNow();
            SysKeyboardNow = SysKeyboardStart;
            SysKeyboardExit = Settings.Default.AppExitKeyboards;
            //Zeigegeräte
            SysPointingDevicesStart = detect_PointingDevicesNow();
            SysPointingDevicesNow = SysPointingDevicesStart;
            SysPointingDevicesExit = Settings.Default.AppExitPointingDevices;

            if (!Settings.Default.FirstStart && Settings.Default.ContinueApplication)
            {
                //Erstellen der temporären Geräteliste, welche alle bisherigen
                //angeschlossenen Geräte und erstmaliges Anschlussdatum beinhaltet.
                _devLists.build_TemporaryDevicesList();
            }

            if (Settings.Default.FirstStart || Settings.Default.StopApplication)
            {
                //Bei erstmaligen Start aufgerufene Funktion
                FirstApplicationRun();
            }

            if (Settings.Default.ContinueApplication)
            {
                //Erkennen ob 32 oder 64Bit-OS verwendet wird
                detect_OSVersion();
                //Benutzerrechte erkennen
                detect_UserRights();
                //Markierungen im Menü setzen
                set_Markers();
                //Temporäre Listen anhand der Datenbanken erstellen
                // White,- Black,- Initial- und DeviceRuleList.
                //zur schnelleren Überprüfung der Checksumme
                _devLists.build_Lists();

                //Netzwerkgeräte Events zur Erkennung von
                // NetworkAvailabilityChanged und NetworkAddressChanged Ereignissen
                EstablishNetworkEvents();


                if (!Settings.Default.FirstStart)
                {
                    //Erkennung ob bereits bei Programmstart blockierte Geräte vorhanden sind
                    detect_Blocked();

                    if (Settings.Default.Detect_USBChanges)
                    {
                        //Abweichungen der Netzwerkadapter, Tastaturen und Zeigegeräte bei Programmstart anzeigen
                        detect_NetworkAdaptersChanges();
                        detect_KeyboardChanges();
                        detect_PointingDevicesChanges();

                        //Erstellen einer Liste der aktuell angeschlossenen USB-Geräte
                        create_DeviceState("NewStateListDB");
                        //Vergleich zwischen der aktuellen und bei Programmende erstellten Geräteliste 
                        compare_DeviceState();
                    }
                }

                SetImage("green.PNG");
                _imgName = "green.PNG";

                Settings.Default.Save();

                if (_addDifDevices)
                {
                    //Unterschiede der Geräteliste (Programmstart und Ende) werden der DifferenceList hinzugefügt
                    add_DifferencesToList();
                }

                if (Settings.Default.FirstStart)
                {
                    Settings.Default.FirstStart = false;
                }
            }

            // Dem Taskleistensymbol werden Menüeinträge hinzugefügt
            System.Windows.Forms.MenuItem item = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem item1 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem item2 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.ContextMenu context = new System.Windows.Forms.ContextMenu();
            context.MenuItems.AddRange(new[] { item, item1, item2 });
            item.Index = 0;
            item.Text = "Show Status Widget";
            item.Click += ShowStatus;
            item1.Index = 1;
            item1.Text = "Configure Firewall Rules";
            item1.Click += Open_FirewallRules;
            item2.Index = 2;
            item2.Text = "Exit Application";
            item2.Click += ExitApplication;
            _appIcon.ContextMenu = context;

            //Erstellung des reduzierten Darstellungsfensters, welches über das Taskleistensymbol geöffnet werden kann
            _noteWindow = new ReducedInfoWidget(_imgName, USBDevices.Count, UsbElementsInList,
                SysKeyboardNow, SysPointingDevicesNow, SysNetworkAdapterNow);
            _noteWindow.Top = SystemParameters.VirtualScreenHeight - _noteWindow.Height -
                (SystemParameters.VirtualScreenHeight - SystemParameters.WorkArea.Height);
            _noteWindow.Left = SystemParameters.VirtualScreenWidth - _noteWindow.Width -
                (SystemParameters.VirtualScreenWidth - SystemParameters.WorkArea.Width);
            _noteWindow.Visibility = Visibility.Collapsed;

            //Akzeptierte GUID-Liste erstellen für die Guid die bei einem DBT_DEVICEARRIVAL Ereignis erkannt werden.
            GuidDeviceList.Add(Guid_USBDevice);
            GuidDeviceList.Add(Guid_HID);
        }

        /// <summary>
        /// Setzt die bei letztem Programmende vorhandenen Einstellungen in den Menüeinträgen 
        /// </summary>
        /// <param name="">Param Description</param>
        private void set_Markers()
        {
            MenuBlockUsb.IsChecked = Settings.Default.BlockUSBDevices == true ? true : false;
            MenuNotifyWhitelist.IsChecked = Settings.Default.NotifyWhitelist == true ? true : false;
            MenuAutoStartKeylogger.IsChecked = Settings.Default.Auto_StartKeylogger == true ? true : false;
            MenuSafeMode.IsChecked = Settings.Default.SafeMode == true ? true : false;
            MenuOnlymassStorage.IsChecked = Settings.Default.AllowOnlyMassStorage == true ? true : false;
            MenuBlockNewNetworkAdapters.IsChecked = Settings.Default.BlockNewNetworkAdapter == true ? true : false;
            MenuDetectUsbChanges.IsChecked = Settings.Default.Detect_USBChanges == true ? true : false;
            MenuDetectPortChange.IsChecked = Settings.Default.DetectPortChange == true ? true : false;
            MenuProhibitChange.IsChecked = Settings.Default.ProhibitPortChange == true ? true : false;
            MenuBlockNewKeyboards.IsChecked = Settings.Default.BlockNewKeyboards == true ? true : false;
            MenuBlockNewHidDevice.IsChecked = Settings.Default.BlockNewHID == true ? true : false;
            MenuBlockNewMassStorage.IsChecked = Settings.Default.BlockNewMassStorage == true ? true : false;
            MenuBlockIdenticalWhitelist.IsChecked = Settings.Default.BlockIdenticalWhitelist == true ? true : false;
            MenuProtectiveFunction.IsChecked = Settings.Default.ProtectiveFunction == true ? false : true;

            if (Settings.Default.ProtectiveFunction == false)
            {
                MenuOnlymassStorage.IsEnabled = false;
                MenuSafeMode.IsEnabled = false;
                MenuBlockUsb.IsChecked = false;
                MenuNotifyWhitelist.IsEnabled = false;
                MenuAutoStartKeylogger.IsEnabled = false;
                MenuDetectUsbChanges.IsEnabled = false;
                MenuDetectPortChange.IsEnabled = false;
                MenuProhibitChange.IsEnabled = false;
                MenuBlockNewKeyboards.IsEnabled = false;
                MenuBlockNewNetworkAdapters.IsEnabled = false;
                MenuBlockNewHidDevice.IsEnabled = false;
                MenuBlockNewMassStorage.IsEnabled = false;
                MenuBlockIdenticalWhitelist.IsEnabled = false;
            }

            if (Settings.Default.SafeMode && Settings.Default.ProtectiveFunction)
            {
                MenuBlockUsb.IsChecked = false;
                MenuOnlymassStorage.IsChecked = false;
                MenuBlockNewKeyboards.IsChecked = false;
                MenuBlockNewNetworkAdapters.IsChecked = false;
                MenuBlockNewHidDevice.IsChecked = false;
                MenuBlockNewMassStorage.IsChecked = false;

                Settings.Default.BlockUSBDevices = false;
                Settings.Default.Save();
            }


            if (Settings.Default.BlockNewKeyboards && Settings.Default.ProtectiveFunction)
            {
                MenuBlockUsb.IsChecked = false;
                MenuSafeMode.IsChecked = false;
                MenuOnlymassStorage.IsChecked = false;

                Settings.Default.BlockUSBDevices = false;
                Settings.Default.Save();
            }

            if (Settings.Default.BlockNewHID && Settings.Default.ProtectiveFunction)
            {
                MenuBlockUsb.IsChecked = false;
                MenuSafeMode.IsChecked = false;
                MenuOnlymassStorage.IsChecked = false;

                Settings.Default.BlockUSBDevices = false;
                Settings.Default.Save();
            }

            if (Settings.Default.BlockNewMassStorage && Settings.Default.ProtectiveFunction)
            {
                MenuBlockUsb.IsChecked = false;
                MenuSafeMode.IsChecked = false;
                MenuOnlymassStorage.IsChecked = false;

                Settings.Default.BlockUSBDevices = false;
                Settings.Default.Save();
            }

            if (Settings.Default.BlockNewNetworkAdapter && Settings.Default.ProtectiveFunction)
            {
                MenuBlockUsb.IsChecked = false;
                MenuSafeMode.IsChecked = false;
                MenuOnlymassStorage.IsChecked = false;

                Settings.Default.BlockUSBDevices = false;
                Settings.Default.Save();
            }

            if (Settings.Default.BlockUSBDevices && Settings.Default.ProtectiveFunction)
            {
                MenuSafeMode.IsChecked = false;
                MenuOnlymassStorage.IsChecked = false;
                MenuBlockNewKeyboards.IsChecked = false;
            }

            if (Settings.Default.AllowOnlyMassStorage && Settings.Default.ProtectiveFunction)
            {
                MenuBlockUsb.IsChecked = false;
                MenuSafeMode.IsChecked = false;
                MenuBlockNewKeyboards.IsChecked = false;
                MenuBlockNewNetworkAdapters.IsChecked = false;
                MenuBlockNewHidDevice.IsChecked = false;
                MenuBlockNewMassStorage.IsChecked = false;

                Settings.Default.BlockNewMassStorage = false;
                Settings.Default.BlockNewHID = false;
                Settings.Default.BlockNewKeyboards = false;
                Settings.Default.BlockNewNetworkAdapter = false;
                Settings.Default.BlockUSBDevices = false;
                Settings.Default.Save();
            }

            if (Settings.Default.AdvancedSettings && Settings.Default.ProtectiveFunction)
            {
                SelectButton1.IsEnabled = true;
                SelectButton2.IsEnabled = true;
                SelectButton3.IsEnabled = true;
            }
        }
        #endregion

    #region Set_TextboxBackgroundColour
        
        /// <summary>
        /// Hintergrundfarbe von Zeigegeräteboxen der GUI setzen
        /// Werden bei Aufruf der Connected_Devices Funktion angezeigt
        /// </summary>
        /// <param name="">Param Description</param>
        private void set_PointingBoxBackground(object sender, RoutedEventArgs e)
        {
            try
            {

                if (SysPointingDevicesNow != SysPointingDevicesStart)
                {
                    SysPointingDevicesNowBox.Background = Settings.Default.FirstStart ? (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050") : Brushes.Red;
                    if (SysPointingDevicesExit == SysPointingDevicesNow)
                    {
                        SysPointingDevicesNowBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                    }
                }

                else if (SysPointingDevicesStart != SysPointingDevicesExit)
                {
                    SysPointingDevicesStartBox.Background = _firstRun ? (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050") : Brushes.Red;
                    SysPointingDevicesNowBox.Background = _firstRun ? (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050") : Brushes.Red;
                }

                else
                {
                    SysPointingDevicesStartBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                    SysPointingDevicesNowBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Hintergrundfarbe von Tastaturboxen der GUI setzen
        /// </summary>
        /// <param name="">Param Description</param>
        private void set_KeyboardBoxBackground(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SysKeyboardNow != SysKeyboardStart)
                {
                    SysKeyboardNowBox.Background = Properties.Settings.Default.FirstStart ? (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050") : Brushes.Red;
                    if (SysKeyboardExit == SysKeyboardNow)
                    {
                        SysKeyboardNowBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                    }
                }
                else if (SysKeyboardStart != SysKeyboardExit)
                {
                    SysKeyboardStartBox.Background = _firstRun ? (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050") : Brushes.Red;
                    SysKeyboardNowBox.Background = _firstRun ? (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050") : Brushes.Red;
                }
                else
                {
                    SysKeyboardStartBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                    SysKeyboardNowBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Hintergrundfarbe von den Netzwerkgeräteboxen in der GUI setzen
        /// </summary>
        /// <param name="">Param Description</param>
        private void set_NetworkAdapterBoxBackground(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SysNetworkAdapterNow != SysNetworkAdapterStart)
                {
                    SysNetworkNowBox.Background = Properties.Settings.Default.FirstStart ? (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050") : Brushes.Red;
                    if (SysNetworkAdapterExit == SysNetworkAdapterNow)
                    {
                        SysNetworkNowBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                    }
                }
                else if (SysNetworkAdapterStart != SysNetworkAdapterExit)
                {
                    SysNetworkStartBox.Background = _firstRun ? (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050") : Brushes.Red;
                    SysNetworkNowBox.Background = _firstRun ? (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050") : Brushes.Red;
                }
                else
                {
                    SysNetworkStartBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                    SysNetworkNowBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
        /// <summary>
        /// Rückgabe der Programmversion
        /// </summary>
        /// <param name="">Param Description</param>
        private string AssemblyVersion
        {
            get
            {
                try
                {
                    return System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
                }
                catch (InvalidDeploymentException)
                {
                    return "0.0.0.0";
                }
            }
        }

        /// <summary>
        /// Öffnet das reduzierte Darstellungsfenster
        /// </summary>
        /// <param name="">Param Description</param>
        private void ShowStatus(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _noteWindow.Show();
            }));
        }

        /// <summary>
        /// Funktion zur Aufzeichnung der Log-Nachrichten
        /// </summary>
        /// <param name="">Param Description</param>
        private void LogWrite(string logMessage)
        {
            try
            {
                using (StreamWriter logWriter = File.AppendText(_appPath + "\\" + "log.txt"))
                {
                    logWriter.Write("\r\n" + DateTime.UtcNow.ToString() + " : " + logMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in writing to the logfile: " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Erkennen von Netzwerkereignissen einrichten
        /// </summary>
        /// <param name="">Param Description</param>
        private void EstablishNetworkEvents()
        {
            try
            {
                //Tritt auf, wenn sich die Verfügbarkeit des Netzwerks ändert.
                NetworkChange.NetworkAvailabilityChanged += Network_AvailabilityChangedEvent;
                //Tritt auf, wenn sich die IP - Adresse einer Netzwerkschnittstelle ändert.
                NetworkChange.NetworkAddressChanged += Network_ChangedEvent;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in adding Network Events: " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #region Device_Deactivation_Activation
        /// <summary>
        /// Funktion zur deaktivierung eines USB-Gerätes
        /// </summary>
        /// <param name="">Param Description</param>
        private int disable_Device(string devName, string hardwareID, string classGuid, string devType, string firstLocation, string vendorID, string productID, bool autoMode, bool displayMessage)
        {

            bool removeDecision = false;
            int conditionResult = -1;

            try
            {
                //Wenn "Port" enthalten ist im Geräteanschluss, dann handelt es sich um das Hauptgerät
                //und nicht um ein Schnittstellengerät
                if (firstLocation.Contains("Port"))
                {
                    //Bei diesen Modi wird eine automatische Blockierung ohne Benutzernachfrage durchgeführt
                    if (autoMode || Settings.Default.BlockUSBDevices || Settings.Default.SafeMode)
                    {
                        removeDecision = true;
                    }
                    else
                    {
                        var decision = MessageBox.Show("Disable the device \nName:" + devName + "\nDeviceType " + devType + "\nVendorID: " + vendorID + " ProductID: " + productID, _appTitle + "Disable " + devType + " device",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (decision == MessageBoxResult.Yes) { removeDecision = true; }
                    }

                    if (removeDecision)
                    {
                        //Rufe die Deaktivierungsfunktion auf 
                        conditionResult = CdsLib.disable_USBDevice(hardwareID, classGuid, firstLocation, true);

                        if (conditionResult != (int)ErrorCode.Success && displayMessage)
                        {
                            ShowToastMessage("Device deactivation error", "Could not deactivate " + devType + "\n" + devName + "\nVendorID:" + vendorID + " ProductID:" + productID, System.Windows.Forms.ToolTipIcon.Warning);
                        }
                        else if (conditionResult == (int)ErrorCode.Success && displayMessage)
                        {
                            ShowMessage("Succesfully blocked the device \n\nName: " + devName + "\nDeviceType: " + devType, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            if (displayMessage)
                            {
                                ShowToastMessage("Device deactivation error", "Could not deactivate " + devType + "\n" + devName + "\nVendorID:" + vendorID + " ProductID:" + productID, System.Windows.Forms.ToolTipIcon.Warning);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in device disable: " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                //logerror
                return conditionResult;
            }
            return conditionResult;
        }

        /// <summary>
        /// Aktvierung eines blockierten Gerätes
        /// </summary>
        /// <param name="">Param Description</param>
        private int activate_Device(string name, string hardwareID, string classGuid, string deviceType, string deviceLocation, string checksum, bool autoMode)
        {
            int result = 0;
            MessageBoxResult decision = MessageBoxResult.Yes;
            try
            {
                //Benutzernachfrage zur Aktivierung
                if (Settings.Default.BlockUSBDevices == false)
                {
                    //TODO detect if device was disabled -> than ask question
                    if (TempList.Contains(checksum) || (autoMode == false))
                    {
                        decision = MessageBox.Show("Should the device \n\nName: " + name + "\nDevice-Type: " + deviceType + "\n\nbe enabled?", _appTitle + "Enable the " + deviceType + " device", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    }
                    else
                    {
                        decision = MessageBoxResult.No;
                    }
                }

                //Automatische Geräteaktivierung durchführen
                if (decision == MessageBoxResult.Yes)
                {
                    //Aufruf der Aktivierungsfunktion mit "ENABLE" Parameter
                    result = CdsLib.ChangeDevState(hardwareID, classGuid, deviceLocation, true, ChangeDeviceStateParams.Enable);
                    switch (result)
                    {
                        case (int)ErrorCode.ErrorSuccessRebootRequired:
                            break;
                        case (int)ErrorCode.ErrorInvalidData:
                            result = CdsLib.ChangeDevState(hardwareID, classGuid, deviceLocation, true, ChangeDeviceStateParams.Enable);
                            break;
                        case (int)ErrorCode.ErrorNoSuchDevinst:
                            //Reenumeration der Geräte erzwingen
                            CdsLib.ForceReenumeration();

                            result = CdsLib.ChangeDevState(hardwareID, classGuid, deviceLocation, true, ChangeDeviceStateParams.Enable);

                            if (result != (int)ErrorCode.Success)
                            {
                                result = CdsLib.ChangeDevState(hardwareID, classGuid, deviceLocation, true, ChangeDeviceStateParams.Unremove);
                                CdsLib.ForceReenumeration();

                                if (result != (int)ErrorCode.Success)
                                {
                                    result = CdsLib.ChangeDevState(hardwareID, classGuid, deviceLocation, false, ChangeDeviceStateParams.Enable);

                                    if (result != (int)ErrorCode.Success)
                                    {
                                        if (deviceLocation.Contains("Port"))
                                        {
                                            ShowToastMessage("Device enable error", "Could not enable this device" + "\nPlease reconnect it again ", System.Windows.Forms.ToolTipIcon.Error);
                                        }
                                    }
                                }
                            }
                            break;
                        case (int)ErrorCode.Success:
                            ShowMessage("Succesfully activated the device \nName: " + name + "\nDevice-Type: " + deviceType + "\nHardwareID: " + hardwareID, MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case (int)ErrorCode.NotFound:
                            CdsLib.ForceReenumeration();

                            result = CdsLib.ChangeDevState(hardwareID, classGuid, deviceLocation, true, ChangeDeviceStateParams.Enable);
                            if (result != (int)ErrorCode.Success)
                            {
                                CdsLib.ForceReenumeration();
                                result = CdsLib.ChangeDevState(hardwareID, classGuid, deviceLocation, false, ChangeDeviceStateParams.Enable);
                            }
                            break;
                        default:
                            ShowToastMessage("Device enable error", "Could not enable " + deviceType + "\n" + name + "\nHardwareID: " + hardwareID, System.Windows.Forms.ToolTipIcon.Error);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in activating device. " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                //logerror
                return result;
            }
            return result;
        }

        /// <summary>
        /// Alle deaktivierten und in der GUI angezeigten Geräte aktivieren. Wird ausgeführt wenn der Benutzer in der GUI keine
        /// Einzelauswahl vor Aufruf der Aktivierung trifft.
        /// </summary>
        /// <param name="">Param Description</param>
        private void activate_AllDevices()
        {

            try
            {
                for (int i = 0; i < USBDevices.Count; i++)
                {
                    //Hauptgerät aktivieren. Im Normalfall werden die Schnittstellen hierdurch auch aktiviert
                    activate_Device(USBDevices[i].mDevice.Name, USBDevices[i].mDevice.HardwareID, USBDevices[i].mDevice.ClassGuid, USBDevices[i].mDevice.DeviceType, USBDevices[i].mDevice.FirstLocationInformation, USBDevices[i].mDevice.Checksum, true);

                    for (int j = 0; j < USBDevices[i].mInterfaces.Count; j++)
                    {
                        //Schnittstelle des Gerätes aktivieren
                        activate_Device(USBDevices[i].mInterfaces[j].Name, USBDevices[i].mInterfaces[j].HardwareID, USBDevices[i].mInterfaces[j].ClassGuid, USBDevices[i].mInterfaces[j].DeviceType, USBDevices[i].mInterfaces[j].FirstLocationInformation, USBDevices[i].mInterfaces[j].Checksum, true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in activating all devices: " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
       

        /// <summary>
        /// Klassenspezifische-Geräteblockierungsfunktion.
        /// Auch zur Deaktivierung von Schnittstellen geeignet. 
        /// </summary>
        /// <param name="">Param Description</param>
        private void classSpecific_Blocking(USBDeviceInfo newDevice)
        {
            try
            {
                if (newDevice != null)
                {
                    if (!string.IsNullOrEmpty(newDevice.FirstLocationInformation))
                    {
                        var result = CdsLib.disable_USBDevice(newDevice.HardwareID, newDevice.ClassGuid, newDevice.FirstLocationInformation, true);

                        if (result == (int)ErrorCode.Success)
                        {
                            ShowToastMessage("New " + newDevice.DeviceType + " detected and blocked", "VendorID: " + newDevice.VendorID + " ProductID: " + newDevice.ProductID, System.Windows.Forms.ToolTipIcon.Warning);
                        }
                        else
                        {
                            ShowToastMessage("New detected " + newDevice.DeviceType + " not blocked!", "VendorID: " + newDevice.VendorID + " ProductID: " + newDevice.ProductID, System.Windows.Forms.ToolTipIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() => ShowMessageBox(ex.Message, "Error in blocking class specific device function", MessageBoxButton.OK, MessageBoxImage.Error)));
            }

        }

        /// <summary>
        /// Überprüfen ob im Menü eine Geräteklassenblockierung aktiviert wurde
        /// </summary>
        /// <param name="">Param Description</param>
        private bool device_ClassBlocking()
        {
            bool result = Settings.Default.BlockNewKeyboards || Settings.Default.BlockNewNetworkAdapter || Settings.Default.BlockNewHID || Settings.Default.BlockNewMassStorage;
            return result;
        }

        /// <summary>
        /// Funktion zur Behandlung eines bei Geräteanschluss erkannten Gerätes.
        /// </summary>
        /// <param name="">Param Description</param>
        private void handle_NewDevice(USBDeviceInfo newDevice)
        {
            int result = 0;

            try
            {
                //Überprüfung ob Gerät in der Blacklist ist. Blacklist hat höchste Priorität
                if (DeviceLists.findIn_BlackList(newDevice.Checksum))
                {
                    //Handelt es sich hierbei um ein Verbundgerät?
                    if (DeviceClass.IsComposite(newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.Service))
                    {
                        //Nur Hauptgerät blockieren, da alle seine Schnittstellen automatisch mitblockiert(deaktiviert) werden.
                        if (newDevice.FirstLocationInformation.Contains("Port"))
                        {
                            result = CdsLib.ChangeDevState(newDevice.HardwareID, newDevice.ClassGuid, newDevice.FirstLocationInformation,
                                true, ChangeDeviceStateParams.DisableComposite);
                        }
                    }
                    //Kein Verbundgerät, daher insgesamt eine Schnittstelle
                    else
                    {
                        result = CdsLib.disable_USBDevice(newDevice.HardwareID, newDevice.ClassGuid, newDevice.FirstLocationInformation, true);
                    }

                    //Anzahl der Geräteschnittstellen abfragen
                    uint cnt = _devLists.get_Interfaces(newDevice.HardwareID, "BlackListDB");
                    if (result == (int)ErrorCode.Success)
                    {
                        if (newDevice.FirstLocationInformation.Contains("Port"))
                        {
                            if (cnt > 0)
                            {
                                ShowToastMessage("Device from Blacklist with\n" + cnt + " Interfaces was blocked", "Type:" + newDevice.DeviceType + "\nVendorID:" + newDevice.VendorID + " ProductID:" + newDevice.ProductID, System.Windows.Forms.ToolTipIcon.Warning);
                            }
                            else
                            {

                                ShowToastMessage("Device from Blacklist blocked", "Type:" + newDevice.DeviceType + "\nVendorID:" + newDevice.VendorID + " ProductID:" + newDevice.ProductID, System.Windows.Forms.ToolTipIcon.Warning);
                            }
                        }
                    }

                    else
                    {
                        if (newDevice.FirstLocationInformation.Contains("Port"))
                        {
                            if (cnt > 0)
                            {
                                ShowToastMessage("Device from Blacklist with\n" + cnt + " Interfaces not blocked", "Type:" + newDevice.DeviceType + "\nVendorID:" + newDevice.VendorID + " ProductID:" + newDevice.ProductID + "\nPlease disconnect device!", System.Windows.Forms.ToolTipIcon.Warning);
                            }

                            else
                            {
                                ShowToastMessage("Device from Blacklist not blocked", "Type:" + newDevice.DeviceType + "\nVendorID:" + newDevice.VendorID + " ProductID:" + newDevice.ProductID + "\nPlease disconnect device!", System.Windows.Forms.ToolTipIcon.Warning);
                            }
                        }

                    }
                }
                //Überprüfung ob Gerät bereits in der Whitelist vorhanden ist.
                else if (DeviceLists.findIn_WhiteList(newDevice.Checksum))
                {
                    WhiteListFunctions(newDevice);
                }

                //Überprüfung der Initialliste
                else if (DeviceLists.findIn_InitialList(newDevice.Checksum))
                {
                    /*do nothing*/
                }

                //Überprüfen ob Geräteklassenblockierung im Menü aktiviert wurde.
                else if (device_ClassBlocking())
                {
                    //Netzwerkadapter blockieren
                    if (Settings.Default.BlockNewNetworkAdapter && DeviceClass.IsNetwork(newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.ClassGuid))
                    {
                        classSpecific_Blocking(newDevice);
                    }
                    //Massenspeicher blockieren
                    else if (Settings.Default.BlockNewMassStorage && newDevice.USB_Class == "08")
                    {
                        classSpecific_Blocking(newDevice);
                    }
                    //HIDGeräte blockieren
                    else if (Settings.Default.BlockNewHID && DeviceClass.IsHid(newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.ClassGuid))
                    {
                        classSpecific_Blocking(newDevice);
                        //Anzahl der Zeigegeräte ermitteln
                        SysPointingDevicesNow = detect_PointingDevicesNow();
                        //Anzahl der Tastaturen ermitteln
                        SysKeyboardNow = detect_KeyboardNow();
                    }
                    //Tastaturen blockieren
                    else if (Settings.Default.BlockNewKeyboards && DeviceClass.IsKeyboard(newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.ClassGuid, newDevice.Service))
                    {
                        classSpecific_Blocking(newDevice);
                        SysKeyboardNow = detect_KeyboardNow();
                    }
                }

                //Nur Massenspeicher erlauben. Alle anderen Geräteklassen werden blockiert.
                else if (Settings.Default.AllowOnlyMassStorage)
                {
                    if (newDevice.USB_Class != "08")
                    {
                        classSpecific_Blocking(newDevice);
                    }

                }

                // Ein Gerät, welches noch keiner Liste hinzugefügt wurde.
                //Standardmäßig wird dieses blockiert
                else
                {
                    //Ist der sichere Modus aktiviert? Falls ja Gerät sofort blockieren und der Blacklist hinzufügen.
                    if (Settings.Default.SafeMode)
                    {

                        TempList.Add(newDevice.Checksum);
                        result = disable_Device(newDevice.Name, newDevice.HardwareID, newDevice.ClassGuid, newDevice.DeviceType, newDevice.FirstLocationInformation, newDevice.VendorID, newDevice.ProductID, false, false);

                        if (result == (int)ErrorCode.Success)
                        {
                            newDevice.DateAdded = DateTime.Now.ToString();

                            //Füge das Gerät der Blacklist hinzu.
                            var removeResult = Dispatcher.Invoke(() => _devLists.add_DataItem("BlackListDB", newDevice), DispatcherPriority.Normal);

                            Dispatcher.Invoke(() => _devLists.removeTempDevice(newDevice));
                            if (removeResult == false)
                            {
                                ShowToastMessage("Error adding device to Blacklist", newDevice.DeviceType + "\nVendorID: " + newDevice.VendorID + " ProductID: " + newDevice.ProductID + "\nCould not add device to the Blacklist!", System.Windows.Forms.ToolTipIcon.Error);
                            }
                            else
                            {
                                ShowToastMessage("Added " + newDevice.DeviceType + " to Blacklist", "Added the device " + newDevice.DeviceType + "\nat the Date: " + newDevice.DateAdded + "\nto the Blacklist.", System.Windows.Forms.ToolTipIcon.Info);
                            }
                        }
                    }

                    else
                    {
                        //Standardmäßige Blockierfunktion für neu erkannte und unbehandelte USB-Geräte
                        if (Settings.Default.BlockUSBDevices)
                        {   //Füge das Gerät zur Templiste hinzu. Hierdurch wird das Gerät nach der Aktivierung nicht erneut behandelt.
                            TempList.Add(newDevice.Checksum);

                            //Handelt es sich um ein Verbundgerät
                            if (DeviceClass.IsComposite(newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.Service))
                            {
                                if (newDevice.FirstLocationInformation.Contains("Port"))
                                {
                                    //Verbundgerätdeaktivierungsfunktion aufrufen
                                    result = CdsLib.ChangeDevState(newDevice.HardwareID, newDevice.ClassGuid, newDevice.FirstLocationInformation, true, ChangeDeviceStateParams.DisableComposite);
                                }
                            }
                            else
                            {
                                //Kein Verbundgerät. Normale Deaktivierungsfunktion aufrufen.
                                result = disable_Device(newDevice.Name, newDevice.HardwareID, newDevice.ClassGuid, newDevice.DeviceType, newDevice.FirstLocationInformation, newDevice.VendorID, newDevice.ProductID, true, false);
                            }
                        }

                        //Benutzernachfrage bei der Blockierung
                        else
                        {
                            var decision = MessageBox.Show("The device " + newDevice.DeviceType + "VendorID: " + newDevice.VendorID + " ProductID: " + newDevice.ProductID + "\nis not included in the Black- or Whitelist.\nBLOCK it now (YES-Button) or decide in the Application (NO-Button) ?", _appTitle + "New device detected", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (decision == MessageBoxResult.Yes)
                            {
                                TempList.Add(newDevice.Checksum);
                                result = disable_Device(newDevice.Name, newDevice.HardwareID, newDevice.ClassGuid, newDevice.DeviceType, newDevice.FirstLocationInformation, newDevice.VendorID, newDevice.ProductID, true, false);
                            }
                        }

                        //Füge das Gerät zu den angezeigten und zu behandelnden Geräten der GUI.
                        Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => add_newDevice(newDevice)));
                        //Benachrichtigung über das neu erkannte Gerät
                        Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => show_newDeviceMessage(newDevice, result)));

                    }
                }

                //Starte den automatischen Tastatur und Zeigegerätelogger, sobald es sich um eine Tastatur oder Zeigegerät handelt.
                if (Settings.Default.Auto_StartKeylogger)
                {
                    RunKeyLogger(newDevice.ClassGuid, newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.Service);
                }

            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() => ShowMessageBox(ex.Message, "Error in adding the device", MessageBoxButton.OK, MessageBoxImage.Error)));
            }
        }

        /// <summary>
        /// Zusätzliche Überprüfungsfunktionen der Whitelist
        /// </summary>
        /// <param name="">Param Description</param>
        private void WhiteListFunctions(USBDeviceInfo device)
        {
            bool connectedNow = false;
            bool updatePort = false;
            bool prohibitPort = false;
            string findPort = "";

            //Geräteanschlussinformationen von diesem Gerät aus der Datenbank abfragen.
            //connectedlocation[0]=Erstmaliger Anschluss, 
            //connectedlocation[1]=Letzte in Datenbank hinterlegte Anschlussposition
            string[] connectedLocation = _devLists.get_Port(device, "WhiteListDB");

            if (connectedLocation[0] != null && connectedLocation[1] != null && connectedLocation.Length == 2)
            {
                string[] hwIDParts = device.HardwareID.Split(@" ".ToCharArray());

                if (hwIDParts.Length > 0)
                {
                    //Überprüfen ob aktuell noch ein anderes Gerät mit den selben Geräteeigenschaften am System angeschlossen ist.
                    findPort = CdsLib.get_DevicePort(hwIDParts[0], device.FirstLocationInformation, device.ClassGuid);

                    if (!string.IsNullOrEmpty(findPort))
                    {
                        //Identisches Gerät gefunden
                        if (findPort != device.FirstLocationInformation && findPort != "")
                        {
                            connectedNow = true;
                        }
                    }
                }
                //Wenn Portwechsel verbieten (Nur erstmalige Anschlussposition erlaubt) aktiviert ist.
                if (Settings.Default.ProhibitPortChange)
                {
                    //Aktuelle Anschlussposition weicht von der erstmaligen hinterlegten Anschlussposition ab,
                    //daher wurde der Anschluss gewechselt und Gerät wird blockiert werden
                    if (connectedLocation[0] != device.FirstLocationInformation)
                    {
                        prohibitPort = true;
                        //Aufruf der Identischen-Geräte-Funktion
                        IdenticalDevice(findPort, connectedLocation, device, connectedNow, false, true, prohibitPort, false);
                    }
                }

                //last device connection differs from actual first connection
                if ((connectedLocation[1] != device.FirstLocationInformation) && !prohibitPort)
                {
                    updatePort = true;
                }

                //Wenn Identische Whitelistgeräte-Blockierung aktiviert ist.
                if (Settings.Default.BlockIdenticalWhitelist && !prohibitPort)
                {
                    //detect if identical device from whitelist is already connected
                    if (connectedNow)
                    {
                        //blockPort = true;
                        IdenticalDevice(findPort, connectedLocation, device, connectedNow, updatePort, true, false, false);
                    }
                }

                //Wenn ein Geräteanschlusswechsel eines Whitelistgerätes durchgeführt wurde und der Benutzer befragt wird
                //ob das Gerät blockiert werden soll.
                if (Settings.Default.NotifyWhitelist && !prohibitPort && Settings.Default.ProhibitPortChange == false &&
                    device.FirstLocationInformation != connectedLocation[0])
                {
                    IdenticalDevice(findPort, connectedLocation, device, connectedNow, updatePort, false, false, true);
                }

                //Aktualisierung der letzten Anschlussposition des Gerätes in der Datenbank
                if (updatePort && !prohibitPort)
                {
                    _devLists.UpdatePort(device, "WhiteListDB");
                }


                if (Settings.Default.DetectPortChange)
                {
                    //Nur benachrichtigen, wenn ein Anschlusswechsel erfolgte oder identisches Gerät vorhanden ist
                    TestPort(device, "WhiteListDB", connectedNow, connectedLocation[1]);
                }
            }
        }

        /// <summary>
        /// Zeige eine Benachrichtigungen Toast an
        /// </summary>
        /// <param name="">Param Description</param>
        private void ShowToastMessage(string title, string msg, System.Windows.Forms.ToolTipIcon icon)
        {
            Thread t = new Thread(() => MyToastMessage(title, msg, icon));
            t.Start();
        }

        /// <summary>
        /// Benachrichtigungen Toast
        /// </summary>
        /// <param name="">Param Description</param>
        private void MyToastMessage(object title, object msg, object icon)
        {
            try
            {
                string msgContent = "\n";
                if (!string.IsNullOrEmpty((string)msg)) { msgContent = (string)msg; }
                Dispatcher.BeginInvoke(new Action(() => _appIcon.ShowBalloonTip(10000, (string)title, msgContent, (System.Windows.Forms.ToolTipIcon)icon)));

            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in MyToastMessage: " + ex.Message);
            }
        }

        /// <summary>
        /// Retouniert den Anwendungstitel mitsamt der Version
        /// </summary>
        /// <param name="">Param Description</param>
        public string MyTitle
        {
            get { return Settings.Default.MyTitle + " Version: " + AssemblyVersion; }
            set
            {
                Settings.Default.MyTitle = value;
                OnPropertyChanged("MyTitle");
            }
        }

        /// <summary>
        /// Zeige ein Messagebox-Benachrichtigungsfenster in einem eigenen Thread -> keine Blockierung der GUI
        /// </summary>
        /// <param name="">Param Description</param>
        private void ShowMessageBox(string text, string title, MessageBoxButton button, MessageBoxImage image)
        {
            Thread t = new Thread(() => MyMessageBox(text, title, button, image));
            t.Start();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="">Param Description</param>
        private void MyMessageBox(object text, object title, object button, object image)
        {
            Dispatcher.BeginInvoke(new Action(() => MessageBox.Show((string)text, _appTitle + title, (MessageBoxButton)button, (MessageBoxImage)image)));
        }

        /// <summary>
        /// Zeigt eine Geräteblockierung und Portwechsel eines Gerätes an.
        /// </summary>
        /// <param name="">Param Description</param>
        private void show_newDeviceMessage(USBDeviceInfo item, int result)
        {
            string msgPort = "";
            string msgDevice = "";
            string msgDisplay = "";

            if (item.LastLocationInformation != item.FirstLocationInformation)
            {
                msgPort = "Last: " + item.LastLocationInformation + "\nNow: " + item.FirstLocationInformation;
            }

            if (Settings.Default.BlockUSBDevices)
            {
                if (DeviceClass.IsPointingDevice(item.USB_Class, item.USB_SubClass, item.USB_Protocol, item.ClassGuid, item.Service))
                {
                    if (SysPointingDevicesNow > Settings.Default.InitialPointingDevices)
                    {
                        msgDevice = "Additional Pointing device connected " + DateTime.Now.ToString();
                    }
                }
                else if (DeviceClass.IsKeyboard(item.USB_Class, item.USB_SubClass, item.USB_Protocol, item.ClassGuid, item.Service))
                {
                    if (SysKeyboardNow > Settings.Default.InitialKeyboardConnected)
                    {
                        msgDevice = "Additional Keyboard connected " + DateTime.Now.ToString();
                    }
                }
                else if (item.ClassGuid.ToLower() == GUID_DEVCLASS_NETWORK.ToString().ToLower())
                {
                    if (SysNetworkAdapterNow > Settings.Default.InitialNetworkAdapters)
                    {
                        msgDevice = "Additional Network adapter connected " + DateTime.Now.ToString();
                    }
                }
            }
            msgDisplay = msgDevice;
            if (item.FirstLocationInformation.Contains("Port_"))
            {
                msgDisplay += "\nDevice ";
            }
            else
            {
                msgDisplay += "\nInterface ";
            }

            msgDisplay += "First usage: ";

            msgDisplay += item.FirstUsage + "\n(" + item.DateConnected + ")";
            if (!string.IsNullOrEmpty(msgPort))
            {
                if (msgPort.Contains("Port"))
                {
                    ShowToastMessage(item.DeviceType + " Port change", msgPort, System.Windows.Forms.ToolTipIcon.Info);
                }
                else
                {
                    ShowToastMessage(item.DeviceType + " Port change", "", System.Windows.Forms.ToolTipIcon.Info);
                }
            }
            if (Settings.Default.BlockUSBDevices)
            {
                if (result == (int)ErrorCode.Success)
                {
                    Dispatcher.BeginInvoke(new Action(() => ShowToastMessage(item.DeviceType + " blocked", msgDisplay, System.Windows.Forms.ToolTipIcon.Info)));
                }

            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => ShowToastMessage(item.DeviceType + " added", msgDisplay, System.Windows.Forms.ToolTipIcon.Info)));
            }
        }

        /// <summary>
        /// Fügt ein neues Gerät zu der Liste hinzu, welche zur Gerätebehandlung der zur
        /// GUI hinzugefügten Geräte benötigt wird.
        /// </summary>
        /// <param name="">Param Description</param>
        private void add_newDevice(USBDeviceInfo item)
        {
            bool foundSlot = false;
            bool contains = false;

            try
            {

                //Erstanschluss des Gerätes
                if (!DeviceLists.findIn_TemporaryDeviceList(item.Checksum))
                {
                    item.FirstUsage = "Yes";
                    _devLists.add_TempDevice(item);
                }
                //device was added before
                else
                {
                    item.FirstUsage = "No";//?
                    _devLists.get_TemporaryDevice(item);

                }

                if (USBDevices.Count > 0)
                {
                    for (int i = 0; i < USBDevices.Count; i++)
                    {
                        contains = false;
                        if (USBDevices[i].mDevice.DeviceID == item.DeviceID && USBDevices[i].mDevice.FirstLocationInformation == item.FirstLocationInformation)
                        {
                            contains = true;
                        }


                        if (!contains)
                        {
                            foreach (USBDeviceInfo device in USBDevices[i].mInterfaces)
                            {
                                if (device.DeviceID == item.DeviceID) { contains = true; break; }
                            }
                            if (!contains && USBDevices[i].mDevice.VendorID == item.VendorID && USBDevices[i].mDevice.ProductID == item.ProductID)
                            {
                                string portLocation = item.FirstLocationInformation;

                                if (portLocation.Contains("Port_") && portLocation != USBDevices[i].mDevice.FirstLocationInformation)
                                {
                                    USBDeviceInfo data = USBDevices[i].mDevice;
                                    USBDevices[i].mDevice = item;
                                    USBDevices[i].mInterfaces.Add(data);
                                    foundSlot = true;
                                }

                                else
                                {
                                    //root device at pos 0
                                    if (USBDevices[i].mDevice.FirstLocationInformation.Contains("Port_"))
                                    {
                                        USBDevices[i].mInterfaces.Add(item);
                                        foundSlot = true;
                                    }

                                    else
                                    {
                                        if (item.FirstLocationInformation.Contains("Port_"))
                                        {
                                            USBDeviceInfo data = USBDevices[i].mDevice;
                                            USBDevices[i].mDevice = item;
                                            USBDevices[i].mInterfaces.Add(data);
                                            foundSlot = true; ;
                                        }
                                        else
                                        {
                                            USBDevices[i].mInterfaces.Add(item);
                                            foundSlot = true;
                                        }
                                    }
                                }
                            }


                            if (foundSlot) { break; }
                        }
                    }


                    if (foundSlot == false && contains == false)
                    {
                        USBDevices.Add(new USBDevice(item));
                        USBDevices[USBDevices.Count - 1].mPosition = USBDevices.Count.ToString();
                    }
                }
                else
                {
                    USBDevices.Add(new USBDevice(item));
                    USBDevices[USBDevices.Count - 1].mPosition = USBDevices.Count.ToString();

                }
                if (contains == false)
                {
                    USBItems.Add(item);
                    UsbElementsInList++;
                    Dispatcher.BeginInvoke(new Action(() => _noteWindow.InterfaceDevices_Now(UsbElementsInList)));
                }
            }
            catch (Exception ex)
            {
                ShowToastMessage(item.DeviceType, "Error in adding this device\n" + ex.Message, System.Windows.Forms.ToolTipIcon.Error);
            }
        }


        /// <summary>
        /// Starte den Tastatur und Zeigegeräte-Logger
        /// </summary>
        /// <param name="">Param Description</param>
        private void RunKeyLogger(string newGuid, string usbClass, string usbSubClass, string usbProtocol, string service)
        {
            //Guid tempGuid = new Guid(newGuid.ToUpper());
            bool runLogger = (DeviceClass.IsKeyboard(usbClass, usbSubClass, usbProtocol, newGuid, service) || (DeviceClass.IsPointingDevice(usbClass, usbSubClass, usbProtocol, newGuid, service)));

            if (runLogger)
            {
                //Nur eine Instanz zulassen
                if (_isRunning) return;
                _isRunning = true;
                Dispatcher.BeginInvoke(new Action(() => BtnKeylogger.IsEnabled = false));
                RunKeylogger();
            }
        }

        /// <summary>
        /// Aufruf, sobald der Zeigegeräte-Logger beendet wird.
        /// </summary>
        /// <param name="">Param Description</param>
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // check error, check cancel, then use result
            if (e.Error != null)
            {
                // handle the error
            }
            else if (e.Cancelled)
            {
                // handle cancellation
            }

        }

        /// <summary>
        /// Liste aller vorhandenen Netzwerkgeräte
        /// </summary>
        /// <param name="">Param Description</param>
        public ObservableCollection<NetAdapterItem> NetItems
        {
            get { return _netItems; }
            set
            {
                if (Equals(value, _netItems))
                    return;
                _netItems = value;
                OnPropertyChanged("NetItems");
            }
        }

        /// <summary>
        /// Netzwerkgeräte-Initialisierungsliste. Nur Netzwerkadapter die aktiv sind.
        /// </summary>
        /// <param name="">Param Description</param>
        public List<NetAdapterItem> InitNetworkAdapters
        {
            get { return _initNetworkAdapters; }
            set
            {
                if (Equals(value, _initNetworkAdapters))
                    return;
                _initNetworkAdapters = value;
                OnPropertyChanged("InitNetworkAdapters");
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public ObservableCollection<USBDevice> USBDevices
        {
            get { return _usbDevices; }
            set
            {
                if (Equals(value, _usbDevices))
                    return;
                _usbDevices = value;
                OnPropertyChanged("USBDevices");
            }
        }

        ///// <summary>
        /// Geräteliste, welche für die Anzeige der Baumstruktur der Geräte in der GUI verwendet wird.
        /// Diese Geräte werden in der unteren Ansicht der GUI (minimale Geräteeigenschaften) angezeigt. 
        /// </summary>
        /// <param name="">Param Description</param>
        public ObservableCollection<USBDeviceInfo> USBItems
        {
            get { return _usbItems; }
            set
            {
                if (Equals(value, _usbItems))
                    return;
                _usbItems = value;
                OnPropertyChanged("USBItems");
            }
        }

        /// <summary>
        /// Stellt erkannte Geräte in der GUI dar. Oberes Anzeigefenster
        /// </summary>
        /// <param name="">Param Description</param>
        public void fill_DeviceBox(object sender, RoutedEventArgs e)
        {
            bool containsThreat = false;
            bool containsSuspect = false;
            bool foundThreat = false;
            bool foundSuspect = false;
            string deviceSep = "\t";
            string interfaceSep = "\t\t";

            try
            {
                if (USBDevices.Count > 0)
                {
                    //Reduzierte Darstellungsform Devices aktualisieren
                    Dispatcher.BeginInvoke(new Action(() => _noteWindow.UsbDevices_Now(USBDevices.Count)));

                    ConnectedDevice_Box.Text = string.Empty;

                    for (int i = 0; i < USBDevices.Count; i++)
                    {
                        Brush textColour = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                        ConnectedDevice_Box.Inlines.Add(new Run(" [" + USBDevices[i].mPosition + "]: ") { FontSize = 12, FontWeight = FontWeights.Bold });
                        var foundItem = false;

                        //Sind Geräteinformationen über ein Gerät dieser Klasse und Typ in den Gerätenklassen vorhanden ?
                        var index = DeviceClass.IndexClass(USBDevices[i].mDevice);

                        //Gehört das Gerät zu einem Gerät der Gefährungsstufe ?
                        if (DeviceClass.ContainsThreatClass(USBDevices[i].mDevice))
                        {
                            containsThreat = true; foundThreat = true;
                        }
                        //Gehört das Gerät zu einem Gerät der Verdächtigstufe ?
                        else if (USBDevices[i].mInterfaces.Count > 0 || DeviceClass.ContainsSuspectedClass(USBDevices[i].mDevice))
                        {
                            foundSuspect = true; containsSuspect = true;
                        }


                        if (containsThreat)
                        {
                            textColour = Brushes.Red;
                        }
                        else if (containsSuspect)
                        {
                            textColour = Brushes.Gold;
                        }

                        var seperator = deviceSep;

                        if (index != -1)
                        {
                            foundItem = true;
                            ConnectedDevice_Box.Inlines.Add(new Run(DeviceClass.ClassCodes[index].Item5 + " (" + USBDevices[i].mDevice.Service.ToLower() + ")" + Environment.NewLine) { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = textColour });
                        }
                        else
                        {
                            foreach (var elem in DeviceClass.ClassCodes)
                            {
                                if (elem.Item1 == USBDevices[i].mDevice.USB_Class)
                                {
                                    foundItem = true;

                                    ConnectedDevice_Box.Inlines.Add(new Run(elem.Item5 + " (" + USBDevices[i].mDevice.Service.ToLower() + ")" + Environment.NewLine) { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = textColour });
                                    break;
                                }
                            }
                            if (!foundItem)
                            {
                                bool searchDeviceGuid = false;
                                foreach (var elem in DeviceClass.GuidCodes)
                                {
                                    if (elem.Item2.ToUpper().Equals(USBDevices[i].mDevice.ClassGuid.ToUpper()))
                                    {

                                        ConnectedDevice_Box.Inlines.Add(new Run(deviceSep + "-> INTERFACE: " + elem.Item1 + " (" + USBDevices[i].mDevice.Service.ToLower() + ")" + Environment.NewLine) { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = textColour });
                                        searchDeviceGuid = true;
                                        break;
                                    }
                                }
                                if (!searchDeviceGuid)
                                {
                                    ConnectedDevice_Box.Inlines.Add(Environment.NewLine);
                                    ConnectedDevice_Box.Inlines.Add(seperator + "No description found for Device: " + USBDevices[i].mDevice.Name + Environment.NewLine);
                                }
                                searchDeviceGuid = false;

                            }
                        }
                        ConnectedDevice_Box.Inlines.Add(seperator + "First usage: ");

                        //Gerät wurde bereits zuvor schon einmal an diesem System bei laufender Software-Firewall angeschlossen
                        if (USBDevices[i].mDevice.FirstUsage == "No")
                        {
                            string changed = USBDevices[i].mDevice.FirstLocationInformation != USBDevices[i].mDevice.LastLocationInformation ? "Yes" : "No";
                            ConnectedDevice_Box.Inlines.Add(new Run(USBDevices[i].mDevice.FirstUsage) { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black });
                            ConnectedDevice_Box.Inlines.Add(seperator + "PORT changed: ");
                            ConnectedDevice_Box.Inlines.Add(new Run(changed + Environment.NewLine) { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black });
                        }

                        //Gerät wurde zuvor nicht an diesem System bei laufender Software-Firewall angeschlossen
                        else
                        {
                            ConnectedDevice_Box.Inlines.Add(new Run(USBDevices[i].mDevice.FirstUsage + Environment.NewLine) { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black });
                        }
                        ConnectedDevice_Box.Inlines.Add(seperator + "First connection: ");
                        ConnectedDevice_Box.Inlines.Add(new Run(USBDevices[i].mDevice.DateConnected + Environment.NewLine) { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black });


                        ConnectedDevice_Box.Inlines.Add(seperator + "VendorID: ");
                        ConnectedDevice_Box.Inlines.Add(USBDevices[i].mDevice.VendorID);
                        ConnectedDevice_Box.Inlines.Add("  ProductID: ");
                        ConnectedDevice_Box.Inlines.Add(USBDevices[i].mDevice.ProductID + Environment.NewLine);

                        for (int j = 0; j < USBDevices[i].mInterfaces.Count; j++)
                        {
                            seperator = interfaceSep;
                            foundItem = false;

                            index = DeviceClass.IndexClass(USBDevices[i].mInterfaces[j]);
                            if (DeviceClass.ContainsThreatClass(USBDevices[i].mInterfaces[j]))
                            {
                                containsThreat = true; foundThreat = true;
                            }
                            else if (USBDevices[i].mInterfaces.Count > 0 || DeviceClass.ContainsSuspectedClass(USBDevices[i].mInterfaces[j]))
                            {
                                foundSuspect = true; containsSuspect = true;
                            }


                            if (containsThreat)
                            {
                                textColour = Brushes.Red;
                            }
                            else if (containsSuspect)
                            {
                                textColour = Brushes.Gold;
                            }
                            else
                            {
                                textColour = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                            }
                            if (index != -1)
                            {
                                foundItem = true;
                                //Schnittstelle des Gerätes
                                ConnectedDevice_Box.Inlines.Add(new Run(deviceSep + "-> INTERFACE: " + DeviceClass.ClassCodes[index].Item5 + " (" + USBDevices[i].mInterfaces[j].Service.ToLower() + ")" + System.Environment.NewLine) { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = textColour });
                            }
                            else
                            {
                                foreach (var elem in DeviceClass.ClassCodes)
                                {
                                    if (elem.Item1 == USBDevices[i].mInterfaces[j].USB_Class)
                                    {
                                        foundItem = true;
                                        ConnectedDevice_Box.Inlines.Add(new Run(deviceSep + "-> INTERFACE: " + elem.Item5 + " (" + USBDevices[i].mInterfaces[j].Service.ToLower() + ")" + System.Environment.NewLine) { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = textColour });
                                        break;
                                    }
                                }
                                if (!foundItem)
                                {
                                    bool searchInterfaceGuid = false;
                                    foreach (var elem in DeviceClass.GuidCodes)
                                    {
                                        if (elem.Item2.ToUpper().Equals(USBDevices[i].mInterfaces[j].ClassGuid.ToUpper()))
                                        {

                                            ConnectedDevice_Box.Inlines.Add(new Run(deviceSep + "-> INTERFACE: " + elem.Item1 + " (" + USBDevices[i].mInterfaces[j].Service.ToLower() + ")" + System.Environment.NewLine) { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = textColour });
                                            searchInterfaceGuid = true;
                                            break;
                                        }
                                    }
                                    if (!searchInterfaceGuid)
                                    {
                                        ConnectedDevice_Box.Inlines.Add(Environment.NewLine);
                                        ConnectedDevice_Box.Inlines.Add(seperator + "No description found for Device: " + USBDevices[i].mInterfaces[j].Name + System.Environment.NewLine);
                                    }
                                    searchInterfaceGuid = false;
                                }
                            }
                        }
                        containsSuspect = false;
                        containsThreat = false;
                    }

                    if (foundThreat)
                    {
                        ConnectedDeviceGroupBox.Background = Brushes.Red;
                        SetImage("red.PNG");
                        _imgName = "red.PNG";

                        //Ändere das angezeigte Symbol der Gefährdungsstufe auf die Stufe Gefährdung
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => _noteWindow.SetImage(_imgName)));
                    }
                    else if (foundSuspect)
                    {
                        if (!Equals(ConnectedDeviceGroupBox.Background, Brushes.Red))
                        {
                            //Ändere das angezeigte Symbol der Gefährdungsstufe auf die Stufe Verdächtig
                            ConnectedDeviceGroupBox.Background = Brushes.Yellow;
                            SetImage("yellow.PNG");
                            _imgName = "yellow.PNG";
                            Application.Current.Dispatcher.BeginInvoke(new Action(() => _noteWindow.SetImage(_imgName)));
                        }
                    }
                    else
                    {
                        //Ändere das angezeigte Symbol der Gefährdungsstufe auf die Stufe Ungefährlich
                        ConnectedDeviceGroupBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                        SetImage("green.PNG");
                        _imgName = "green.PNG";
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => _noteWindow.SetImage(_imgName)));
                    }
                }
                else
                {
                    ConnectedDevice_Box.Text = string.Empty;
                    ConnectedDeviceGroupBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                    SetImage("green.PNG");
                    _imgName = "green.PNG";
                    if (_noteWindow != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => _noteWindow.SetImage(_imgName)));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in fill_DeviceBox. " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Setzt die Grafik für das angezeigte Symbol
        /// </summary>
        /// <param name="">Param Description</param>
        private void SetImage(string imagePath)
        {
            if (File.Exists(Path.Combine(BaseDir, "Resources", imagePath)))
            {
                Uri uri = new Uri(Path.Combine(BaseDir, "Resources", imagePath), UriKind.Absolute);
                ImageSource imgSource = new BitmapImage(uri);
                StateImage.Source = imgSource;
            }
            else
            {
                MessageBox.Show("Image " + imagePath + " not found in Resources directory!", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// Anzahl der USB-Geräte, welche aktuell zu behandeln sind.
        /// </summary>
        /// <param name="">Param Description</param>
        public int UsbElementsInList
        {
            get { return _uSbElementsInList; }
            set
            {
                if (Equals(value, _uSbElementsInList))
                    return;
                _uSbElementsInList = value;

                OnPropertyChanged("UsbElementsInList");
            }
        }

        /// <summary>
        /// Anzahl der Netzwerkadapter bei der letzten Programmbeendigung.
        /// </summary>
        /// <param name="">Param Description</param>
        public int SysNetworkAdapterExit
        {
            get { return _sysNetworkAdapterExit; }
            set
            {
                if (Equals(value, _sysNetworkAdapterExit))
                    return;
                _sysNetworkAdapterExit = value;

                OnPropertyChanged("SysNetworkAdapterExit");
            }
        }
        /// <summary>
        /// Anzahl der Netzwerkadapter aktuell.
        /// </summary>
        /// <param name="">Param Description</param>
        public int SysNetworkAdapterNow
        {
            get { return _sysNetworkAdapterNow; }
            set
            {
                if (Equals(value, _sysNetworkAdapterNow))
                    return;
                _sysNetworkAdapterNow = value;

                OnPropertyChanged("SysNetworkAdapterNow");
            }
        }

        /// <summary>
        /// Anzahl der Netzwerkadapter bei Programmstart.
        /// </summary>
        /// <param name="">Param Description</param>
        public int SysNetworkAdapterStart
        {
            get { return _sysNetworkAdapterStart; }
            set
            {
                if (Equals(value, _sysNetworkAdapterStart))
                    return;
                _sysNetworkAdapterStart = value;

                OnPropertyChanged("SysNetworkAdapterStart");
            }
        }

        /// <summary>
        /// Anzahl der Tastaturen bei der letzten Programmbeendigung.
        /// </summary>
        /// <param name="">Param Description</param>
        public int SysKeyboardExit
        {
            get { return _sysKeyboardExit; }
            set
            {
                if (Equals(value, _sysKeyboardExit))
                    return;
                _sysKeyboardExit = value;

                OnPropertyChanged("SysKeyboardExit");
            }
        }

        /// <summary>
        /// Anzahl der Tastaturen bei Programmstart.
        /// </summary>
        /// <param name="">Param Description</param>
        public int SysKeyboardStart
        {
            get { return _sysKeyboardStart; }
            set
            {
                if (Equals(value, _sysKeyboardStart))
                    return;
                _sysKeyboardStart = value;

                OnPropertyChanged("SysKeyboardStart");
            }
        }
        /// <summary>
        /// Anzahl der Tastaturen aktuell.
        /// </summary>
        /// <param name="">Param Description</param>
        public int SysKeyboardNow
        {
            get { return _sysKeyboardNow; }
            set
            {
                if (Equals(value, _sysKeyboardNow))
                    return;
                _sysKeyboardNow = value;

                OnPropertyChanged("SysKeyboardNow");
            }
        }

        /// <summary>
        /// Anzahl der Zeigegeräte bei der letzten Programmbeendigung.
        /// </summary>
        /// <param name="">Param Description</param>
        public int SysPointingDevicesExit
        {
            get { return _sysPointingDevicesExit; }
            set
            {
                if (Equals(value, _sysPointingDevicesExit))
                    return;
                _sysPointingDevicesExit = value;
                OnPropertyChanged("SysPointingDevicesExit");
            }
        }

        /// <summary>
        /// Anzahl der Zeigegeräte bei Programmstart.
        /// </summary>
        /// <param name="">Param Description</param>
        public int SysPointingDevicesStart
        {
            get { return _sysPointingDevicesStart; }
            set
            {
                if (Equals(value, _sysPointingDevicesStart))
                    return;
                _sysPointingDevicesStart = value;
                OnPropertyChanged("SysPointingDevicesStart");
            }
        }
        /// <summary>
        /// Anzahl der Zeigegeräte aktuell.
        /// </summary>
        /// <param name="">Param Description</param>
        public int SysPointingDevicesNow
        {
            get { return _sysPointingDevicesNow; }
            set
            {
                if (Equals(value, _sysPointingDevicesNow))
                    return;
                _sysPointingDevicesNow = value;

                OnPropertyChanged("SysPointingDevicesNow");
            }
        }

        /// <summary>
        /// Rückgabe ob der Benutzer über Administartionsrechte verfügt
        /// </summary>
        /// <param name="">Param Description</param>
        private bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                if (Equals(value, _isAdmin))
                    return;
                _isAdmin = value;
                OnPropertyChanged("IsAdmin");
            }
        }

        /// <summary>
        /// Blockierungsfunktion für Identische Whitelistgeräte und Anschlussechsel. 
        /// Öffnet das Fenster welchen den Benutzer mitteilt ob Gerät den Anschluss gewechselt hat oder
        /// bereits verbunden ist bzw. dieses auch blockieren kann. 
        /// </summary>
        /// <param name="">Param Description</param>
        private void IdenticalDevice(string devicePort, string[] whitelistDevicePorts, USBDeviceInfo device, bool connectedNow, bool updatePort, bool blockPortChange, bool prohibitport, bool notifyPortChange)
        {
            try
            {
                bool connected = false;
                string dateAdded;

                List<ChangeStateDevice> identicalList;
                if (connectedNow ) { connected = true; }

                if (connected && Settings.Default.BlockUSBDevices || blockPortChange || prohibitport)
                {
                    if (notifyPortChange == false)
                    {
                        TempList.Add(device.Checksum);
                        if (device.FirstLocationInformation.ToUpper().Contains("PORT"))
                        {
                            int result;
                            if (connected && (devicePort != whitelistDevicePorts[0]) && (device.FirstLocationInformation == whitelistDevicePorts[0]))
                            {
                                //Neu angeschlossenes Gerät, welches über die Anschlussinformation des als Whitelist gelisteten Gerätes verfügt. Ein zusätzliches Gerät
                                //mit den selben Eigenschaften ist bereits angeschlossen, daher wird dieses blockiert. 
                                result = disable_Device(device.Name, device.HardwareID, device.ClassGuid, device.DeviceType, devicePort, device.VendorID, device.ProductID, true, true);//new
                            }
                            else
                            {
                                //Neu angeschlossenes Gerät neben bereits identischen angeschlossenenund Whitelistgerät. Neues Gerät blockieren.
                                result = disable_Device(device.Name, device.HardwareID, device.ClassGuid, device.DeviceType, device.FirstLocationInformation, device.VendorID, device.ProductID, true, true);//new
                            }

                            if (result != (int)ErrorCode.Success) { ShowToastMessage("Device deactivation failed", "Please replug the " + device.DeviceType + "\nfor a succesfully deactivation!", System.Windows.Forms.ToolTipIcon.Error); }
                        }
                    }
                }
                if (device.FirstLocationInformation.Contains("Port"))
                {

                    dateAdded = _devLists.get_DateAdded(device.Checksum, "WhiteListDB");

                    identicalList = _devLists.get_ChangeStateDevice(device, dateAdded, updatePort, connected);

                    if (identicalList.Count > 0)
                    {

                        Thread viewerThread = new Thread(() =>
                        {
                            SynchronizationContext.SetSynchronizationContext(
                                new DispatcherSynchronizationContext(
                                    Dispatcher.CurrentDispatcher));


                            NotifyWindow notifyWindow = new NotifyWindow(identicalList, dateAdded, connected, Settings.Default.BlockUSBDevices, blockPortChange, prohibitport, notifyPortChange);

                            notifyWindow.Closed += (sender2, e2) =>
                                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                            //Zeige das Benachrichtigungsfenster
                            notifyWindow.Show();
                            Dispatcher.Run();
                        });

                        viewerThread.SetApartmentState(ApartmentState.STA);
                        viewerThread.IsBackground = true;
                        viewerThread.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Erstmaliger Start der Anwendung
        /// </summary>
        /// <param name="">Param Description</param>
        private void FirstApplicationRun()
        {
            Settings.Default.InitialNetworkAdapters = NetItems.Count;
            Settings.Default.InitialKeyboardConnected = SysKeyboardStart;
            Settings.Default.InitialPointingDevices = SysPointingDevicesStart;

            _firstStartWindow = new FirstStart(SysKeyboardStart.ToString(), SysPointingDevicesStart.ToString());
            _firstStartWindow.EventYesPressed += YesPressed;
            _firstStartWindow.EventNoPressed += NoPressed;

            Dispatcher.Invoke(() => _firstStartWindow.ShowDialog());

            while (_stopApp == false && _continueApp == false) { }

            if (_continueApp)
            {
                //Erzeugung der Datenbanken
                _devLists.CreateDatabase();
                build_DeviceTables();

                //Initialgeräte in die temporäre Liste kopieren
                _devLists.copy_DevicesToTemp();
                _devLists.build_TemporaryDevicesList();
                Settings.Default.ContinueApplication = true;
                Settings.Default.StopApplication = false;
            }
            else
            {
                Settings.Default.StopApplication = true;
                Settings.Default.ContinueApplication = false;
            }

            Settings.Default.Save();

        }

        /// <summary>
        /// Erkennung ob Anzahl der Netzwerkadapter zwischen Programmende und Start abweicht.
        /// </summary>
        /// <param name="">Param Description</param>
        private void detect_NetworkAdaptersChanges()
        {
            if (SysNetworkAdapterExit != SysNetworkAdapterStart)
            {
                ShowToastMessage("Nr. of Network-adapters changed", "Between last application-exit and restart.\nLast exit:" + SysNetworkAdapterExit + " Now:" + SysNetworkAdapterStart, System.Windows.Forms.ToolTipIcon.Warning);
            }
        }

        /// <summary>
        /// Erkennung ob Anzahl der Zeigegeräte zwischen Programmende und Start abweicht.
        /// </summary>
        /// <param name="">Param Description</param>
        private void detect_PointingDevicesChanges()
        {
            if (SysPointingDevicesExit != SysPointingDevicesStart)
            {
                ShowToastMessage("Nr. of Pointing devices changed", "Between last application-exit and restart.\nLast exit:" + SysPointingDevicesExit + " Now:" + SysPointingDevicesStart, System.Windows.Forms.ToolTipIcon.Warning);
            }

        }

        /// <summary>
        /// Erfassung der aktuell am System vorhandenen Zeigegeräte
        /// </summary>
        /// <param name="">Param Description</param>
        private int detect_PointingDevicesNow()
        {
            var count = 0;
            using (ManagementObjectSearcher mos = new ManagementObjectSearcher("Select Name from Win32_PointingDevice"))
            {
                foreach (var pDev in mos.Get())
                {
                    var pointingItem = (ManagementObject)pDev;
                    using (pointingItem)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Benachrichtigung wenn die Anzahl der Tastaturen zwischen Programmende und Start abweicht.
        /// </summary>
        /// <param name="">Param Description</param>
        private void detect_KeyboardChanges()
        {
            if (SysKeyboardExit != SysKeyboardStart)
            {
                ShowToastMessage("Nr. of Keyboards changed", "Between last application-exit and restart.\nLast exit:" + SysKeyboardExit + " Now:" + SysKeyboardStart, System.Windows.Forms.ToolTipIcon.Warning);
            }

        }

        /// <summary>
        /// Erfassung der aktuell am System vorhandenen Tastaturen
        /// </summary>
        /// <param name="">Param Description</param>
        private int detect_KeyboardNow()
        {
            int count = 0;
            using (ManagementObjectSearcher mos = new ManagementObjectSearcher("Select Name from Win32_Keyboard"))
            {
                foreach (var kDev in mos.Get())
                {
                    var keyboardItem = (ManagementObject)kDev;
                    using (keyboardItem)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void build_DeviceTables()
        {
            //Liste der aktuell am System vorhandenen USB-Geräte erfassen.
            var getDevices = collect_USBDevices();
            if (getDevices.Count > 0)
            {
                var querryInitial = "InitialListDB";

                foreach (var item in getDevices)
                {
                    if (!string.IsNullOrEmpty(item.Service))
                    {
                        item.DateAdded = DateTime.Now.ToString();
                        //Geräte zur Initialliste hinzufügen.
                        var result = _devLists.add_DataItem(querryInitial, item);

                        if (result == false)
                        {
                            ShowToastMessage("Could not add " + item.DeviceType, "\nVendorID: " + item.VendorID + "\nProductID: " + item.ProductID + "\nto the " + querryInitial, System.Windows.Forms.ToolTipIcon.Error);
                        }
                    }
                }
            }
        }

       

        /// <summary>
        /// Öffnet eine Webseite um die VendorID und ProductID überprüfen zu können
        /// </summary>
        /// <param name="">Param Description</param>
        private void btnVerifyVendorProductID_click(object sender, RoutedEventArgs e)
        {
            //SZ development USB ID Database
            const string target = "http://www.the-sz.com/products/usbid/";

            try
            {
                Process.Start(target);
            }
            catch (Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259) //The system cannot find the file specified
                    ShowToastMessage("Internet browser was found", noBrowser.Message, System.Windows.Forms.ToolTipIcon.Error);
            }
            catch (Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

     

        /// <summary>
        /// Erkennen der Benutzerrechte
        /// </summary>
        /// <param name="">Param Description</param>
        private void detect_UserRights()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            IsAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!IsAdmin)
            {
                ShowToastMessage("Administrative rights needed!", "Programm will not have full functionality", System.Windows.Forms.ToolTipIcon.Info);
                Settings.Default.AdminRights = false;
            }
            else
            {
                Settings.Default.AdminRights = true;
            }
            Settings.Default.Save();
        }

        /// <summary>
        /// Erkennen ob 32 oder 64Bit Betriebssystem verwendet wird
        /// </summary>
        /// <param name="">Param Description</param>
        private void detect_OSVersion()
        {
            //64bit OS
            if (Environment.Is64BitOperatingSystem)
            {
                Settings.Default.Is64BitOS = true;
            }
            //32bit OS
            else
            {
                Settings.Default.Is64BitOS = false;
            }
        }

        /// <summary>
        /// Zeigt Benachrichtigungen in einer Messagebox
        /// </summary>
        /// <param name="">Param Description</param>
        private void ShowMessage(string message, MessageBoxButton button, MessageBoxImage image)
        {
            if (Settings.Default.AllowPopupMessages)
            {
                MessageBox.Show(message, Title, button, image);
            }
        }

        /// <summary>
        /// Ändert den Anschlusswechselerkennungs Menüeintrag
        /// </summary>
        /// <param name="">Param Description</param>
        private void detect_PortChange_Click(object sender, RoutedEventArgs e)
        {
            if (MenuDetectPortChange.IsChecked)
            {
                Settings.Default.DetectPortChange = true;
            }
            else
            {
                Settings.Default.DetectPortChange = false;
            }
            Settings.Default.Save();

        }



        /// <summary>
        ///  //Tritt auf, wenn sich die IP - Adresse einer Netzwerkschnittstelle ändert.
        /// </summary>
        /// <param name = "" > Param Description</param>
        private void Network_ChangedEvent(object sender, EventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(build_NetList));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.GetType());//change to log file
            }
        }

        /// <summary>
        /// //Tritt auf, wenn sich die Verfügbarkeit eines Netzwerks ändert.
        /// </summary>
        /// <param name="">Param Description</param>
        private void Network_AvailabilityChangedEvent(object sender, NetworkAvailabilityEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(build_NetList));
                if (e.IsAvailable)
                {

                    Application.Current.Dispatcher.BeginInvoke(new Action(() => ShowMessage("Network Availability has changed", MessageBoxButton.OK, MessageBoxImage.Information)));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => ShowMessage("No Network connected", MessageBoxButton.OK, MessageBoxImage.Information)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.GetType());//change to log file
            }
        }

        /// <summary>
        /// Érzeugung der Netzwerkgerätelisten. 
        /// </summary>
        /// <param name="InitNetworkAdapters">Alle vorhandenen Netzwerkadapter</param>
        /// <param name="NetItems">Alle physikalisch vorhandenen Netzwerkadapter</param>
        private void build_NetList()
        {
            NetItems.Clear();
            InitNetworkAdapters.Clear();

            NetworkInterface[] netInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            if (netInterfaces.Length > 0)
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher(@"SELECT * FROM  Win32_NetworkAdapter ");

                ManagementObjectCollection queryCollection = mos.Get();
                try
                {
                    foreach (var nDev in queryCollection)
                    {
                        var mObj = (ManagementObject)nDev;
                        using (mObj)
                        {
                            if (mObj != null)
                            {
                                InitNetworkAdapters.Add(new NetAdapterItem(mObj));
                                // Get only the physical adapters
                                if (InitNetworkAdapters.Last().AdapterPhysicalAdapter.Contains("True"))
                                {
                                    NetItems.Add(new NetAdapterItem(mObj));
                                }

                            }
                        }
                    }
                    SysNetworkAdapterNow = NetItems.Count;

                    if (_noteWindow != null)
                    {
                        _noteWindow.NetworkDevices_Now(SysNetworkAdapterNow);
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }

            }
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
        /// Zeigt die Tabelle mit den Gerätedifferenzen (erfasst zwischen Programmende und Start) an.
        /// </summary>
        /// <param name="">Param Description</param>
        private void showTable(string table)
        {
            DeviceLists devLists = new DeviceLists();
            devLists.Show();

            devLists.fill_Grid(table);
        }

        /// <summary>
        /// Vergleicht die erstellten Gerätelisten (OldStateListDB und NewStateListDB) auf Unterschiede.
        /// </summary>
        /// <param name="OldStateListDB">Wird bei Programmende erstellt</param>
        /// <param name="NewStateListDB">Wird bei Programmstart erstellt</param>
        private void compare_DeviceState()
        {
            if (Settings.Default.ProtectiveFunction)
            {
                if (Settings.Default.Detect_USBChanges)
                {
                    try
                    {
                        //dataTable operations
                        int result = _devLists.CreateDifferenceList("OldStateListDB", "NewStateListDB");
                        //there where found some devices that changed since last programm restart or system restart                   
                        if (result > 0)
                        {
                            _devLists.add_DiffToTemp();
                            var decision = MessageBox.Show("A anomaly in the USB-device list since the last shutdown was observed.\nAdd found devices into the BadUSB-Software-Firewall?\n(The devices will be blocked if blocking of devices is enabled)", _appTitle + "Anomaly in USB-devices found", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                            if (decision == MessageBoxResult.Yes)
                            {
                                _addDifDevices = true;
                            }
                            else
                            {
                                decision = MessageBox.Show("Show the USB-devices which differ since the last shutdown?", _appTitle + "Show USB-differences", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (decision == MessageBoxResult.Yes)
                                {
                                    Dispatcher.BeginInvoke(new Action(() => showTable("DifferenceListDB")));
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);//change to log file
                    }
                }
            }
        }

        /// <summary>
        /// Zeigt eine Liste aller aktuell am System vorhandenen USB-Geräte an
        /// </summary>
        /// <param name="">Param Description</param>
        private void createActualStateList_Click(object sender, RoutedEventArgs e)
        {
            MenuItem myItem = (MenuItem)sender;
            string table = myItem.CommandParameter.ToString();
            if (!string.IsNullOrEmpty(table))
            {
                _devLists.delete_Table(table);

                List<USBDeviceInfo> getDevices = collect_USBDevices();

                if (getDevices.Count > 0)
                {
                    foreach (USBDeviceInfo item in getDevices)
                    {
                        var result = _devLists.add_DataItem(table, item);
                        if (result == false)
                        {
                            ShowMessage("Could not create the " + table, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                }
                _devLists.Show();
                _devLists.fill_Grid(table);
            }
        }

        /// <summary>
        /// Erzeugt die NewStateListDB bei Programmstart oder bei Programmende die OldStateListDB
        /// </summary>
        /// <param name="">Param Description</param>
        private void create_DeviceState(string table)
        {
            try
            {
                if (Settings.Default.Detect_USBChanges && Settings.Default.ProtectiveFunction)
                {
                    List<USBDeviceInfo> getDevices = collect_USBDevices();
                    //delete the content of the table
                    _devLists.delete_Table(table);
                    if (getDevices.Count > 0)
                    {
                        for (int i = 0; i < getDevices.Count; i++)
                        {
                            if (DeviceLists.findIn_TemporaryDeviceList(getDevices[i].Checksum))
                            {
                                _devLists.get_TemporaryDevice(getDevices[i]);

                            }
                            else
                            {
                                if (DeviceClass.AcceptedGuid.Contains(new Guid(getDevices[i].ClassGuid)) && !string.IsNullOrEmpty(getDevices[i].USB_Class) && !DeviceLists.findIn_InitialList(getDevices[i].Checksum) && getDevices[i].Status != "Error")
                                {
                                    _devLists.add_TempDevice(getDevices[i]);
                                }
                            }
                        }
                        foreach (USBDeviceInfo item in getDevices)
                        {
                            //Nur Geräte hinzugügen die über eine akzeptierte GUID verfügen und nicht in der Black- oder Whitelist vorhanden sind.
                            if (DeviceClass.AcceptedGuid.Contains(new Guid(item.ClassGuid)) && !string.IsNullOrEmpty(item.USB_Class) && !string.IsNullOrEmpty(item.USB_SubClass) && !string.IsNullOrEmpty(item.USB_Protocol))
                            {
                                if (item.Status != "Error" && !DeviceLists.findIn_BlackList(item.Checksum) && !DeviceLists.findIn_WhiteList(item.Checksum) && !DeviceLists.findIn_InitialList(item.Checksum))
                                {
                                    var result = _devLists.add_DataItem(table, item);
                                    if (result == false)
                                    {
                                        ShowMessage("Could not create the " + table, MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Zur Erkennung angeschlossenerer in der Blacklist befindlichen Geräte.
        /// Wird bei Programmstart und nach der Deaktivierung des SAFE-Modus ausgeführt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void detect_Blocked()
        {
            try
            {
                var count = _devLists.get_BlacklistCount();
                if (count > 0)
                {
                    List<USBDeviceInfo> getDevices = collect_USBDevices();

                    if (getDevices.Count > 0)
                    {
                        List<string> blacklisHwid = _devLists.get_BlacklistHWID();
                        for (int i = 0; i < getDevices.Count; i++)
                        {
                            if (DeviceLists.findIn_TemporaryDeviceList(getDevices[i].Checksum))
                            {
                                _devLists.get_TemporaryDevice(getDevices[i]);

                            }
                        }
                        foreach (USBDeviceInfo item in getDevices)
                        {
                            //only add devices for comparsion, which are in the accepted guid list and not in the white and blacklist
                            if (DeviceClass.AcceptedGuid.Contains(new Guid(item.ClassGuid)) && !string.IsNullOrEmpty(item.USB_Class) && !string.IsNullOrEmpty(item.USB_SubClass) && !string.IsNullOrEmpty(item.USB_Protocol))
                            {
                                if (blacklisHwid.Contains(item.HardwareID))
                                {
                                    int result = disable_Device(item.Name, item.HardwareID, item.ClassGuid, item.DeviceType, item.FirstLocationInformation, item.VendorID, item.ProductID, true, true);

                                    uint cnt = _devLists.get_Interfaces(item.HardwareID, "BlackListDB");
                                    if (result == (int)ErrorCode.Success)
                                    {
                                        if (item.FirstLocationInformation.Contains("Port"))
                                        {
                                            if (cnt > 0)
                                            {
                                                ShowToastMessage("Found and blocked Blacklist device", item.DeviceType + "\n" + cnt + " Interfaces." + "\nVendorID:" + item.VendorID + " ProductID:" + item.ProductID, System.Windows.Forms.ToolTipIcon.Warning);
                                            }
                                            else
                                            {
                                                ShowToastMessage("Found and blocked Blacklist device", item.DeviceType + "\nVendorID:" + item.VendorID + " ProductID:" + item.ProductID, System.Windows.Forms.ToolTipIcon.Warning);
                                            }
                                        }
                                    }

                                    else
                                    {
                                        if (item.FirstLocationInformation.Contains("Port"))
                                        {
                                            if (cnt > 0)
                                            {
                                                ShowToastMessage("Found device from Blacklist with " + cnt + " Interfaces not blocked", item.DeviceType + "\nPort: " + item.VendorID + " ProductID:" + item.ProductID + "\nPlease disconnect device!", System.Windows.Forms.ToolTipIcon.Warning);
                                            }

                                            else
                                            {
                                                ShowToastMessage("Found device from Blacklist not blocked", item.DeviceType + "\nVendorID:" + item.VendorID + " ProductID:" + item.ProductID + "\nPlease disconnect device!", System.Windows.Forms.ToolTipIcon.Warning);
                                            }
                                        }

                                    }

                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void ExitApplication(object sender, EventArgs e)
        {
            Close();
        }
               

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void OnSessionEnding(object sender, SessionEndingEventArgs e)
        {
            Unregister_DeviceNotification();
            if (Settings.Default.Detect_USBChanges)
            {
                create_DeviceState("OldStateListDB");
            }
            // writeLogEntry("Shutdown or Logoff Success");

            Application.Current.Shutdown();
        }

        /// <summary>
        /// Eigene Behandlung bei Programmbeendigung
        /// </summary>
        /// <param name="">Param Description</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;

            foreach (string item in LogMessages)
            {
                LogWrite(item);
            }

            _noteWindow.Close();

            Unregister_DeviceNotification();

            if (Settings.Default.Detect_USBChanges && Settings.Default.StopApplication == false)
            {
                //Erzeugung der OldStateListDB
                if (Settings.Default.ContinueApplication)
                {
                    _devLists.addTempDevice_ToTable();
                    create_DeviceState("OldStateListDB");

                }
            }
            if (Settings.Default.ContinueApplication)
            {
                Settings.Default.AppExitKeyboards = SysKeyboardNow;
                Settings.Default.AppExitPointingDevices = SysPointingDevicesNow;
                Settings.Default.AppExitNetworkAdapters = SysNetworkAdapterNow;
                Settings.Default.Save();
            }
            _appIcon.Visible = false;
            _appIcon.Icon = null;
            _appIcon.Dispose();
            base.OnClosing(e);
            Application.Current.Shutdown();

        }

        #region Database_Functions
        /// <summary>
        /// Fügt ein neu erkanntes Gerät der Whitelist hinzu
        /// </summary>
        /// <param name="">Param Description</param>
        private void add_DeviceWhitelist(USBDeviceInfo device, string actualTime, bool addWhiteList)
        {
            try
            {
                //Überprüfen ob bereits vorhanden
                if (DeviceLists.findIn_WhiteList(device.Checksum))
                {
                    //Benachrichtigen, dass Gerät bereits vorhanden ist.
                    Dispatcher.BeginInvoke(new Action(() => ShowToastMessage("Identical Whitelist device found", device.Name + "\n" + device.DeviceType + "\nVendorID:" + device.VendorID + " ProductID:" + device.ProductID + "\nDate added:" + device.DateAdded + "is already in the Whitelist", System.Windows.Forms.ToolTipIcon.Warning)));
                }

                else
                {
                    if (!addWhiteList) return;
                    //Hinzufügungsdatum zur Liste = actualTime
                    device.DateAdded = actualTime;
                    //Zur Datenbank hinzufügen
                    var res = _devLists.add_DataItem("WhiteListDB", device);

                    if (res == false)
                    {
                        MessageBox.Show("Error in adding a device to the Whitelist-Database.\n\nDEVICE:\nName: " + device.Name + "\nDevice-Type: " + device.DeviceType + "\nVendorID: " + device.VendorID + "\nProductID: " + device.ProductID, _appTitle + "Error in adding device to Whitelist Database", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in adding the device to the whitelist: " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        /// <summary>
        /// Alle in der GUI angezeigten Geräte zur Whitelist hinzufügen. Wird ausgeführt wenn der Benutzer 
        /// in der GUI keine Einzelauswahl vor Aufruf der Erlauben-Funktion trifft.
        /// </summary>
        /// <param name="">Param Description</param>
        private void add_DevicesToWhitelist(string actualTime, bool addWhiteList)
        {
            try
            {
                for (int i = 0; i < USBDevices.Count; i++)
                {
                    //Hinzufügen des Hauptgerätes
                    add_DeviceWhitelist(USBDevices[i].mDevice, actualTime, addWhiteList);

                    for (int j = 0; j < USBDevices[i].mInterfaces.Count; j++)
                    {
                        //Hinzufügen der Schnittstelle
                        add_DeviceWhitelist(USBDevices[i].mInterfaces[j], actualTime, addWhiteList);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in adding the devices to the whitelist: " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Fügt erkannte Geräteabweichungen zwischen Programmende und Start zur Differenzenliste hinzu
        /// </summary>
        /// <param name="">Param Description</param>
        private void add_DifferencesToList()
        {
            int result = 0;
            try
            {
                List<USBDeviceInfo> tempDevices = new List<USBDeviceInfo>();
                _devLists.add_Differences(tempDevices);

                foreach (USBDeviceInfo item in tempDevices)
                {
                    add_newDevice(item);
                    if (Settings.Default.BlockUSBDevices)
                    {
                        //Keine Geräe der Whitelist hinzufügen
                        if (!DeviceLists.findIn_WhiteList(item.Checksum))
                        {
                            // Überprüfung ob Gerät aktuell angeschlossen ist
                            result = CdsLib.ChangeDevState(item.HardwareID, item.ClassGuid, item.FirstLocationInformation, true, ChangeDeviceStateParams.Available);
                            if (Settings.Default.BlockUSBDevices)
                            {
                                if (result == (int)ErrorCode.Success)
                                {
                                    //Deaktivierung des Gerätes
                                    result = disable_Device(item.Name, item.HardwareID, item.ClassGuid, item.DeviceType, item.FirstLocationInformation, item.VendorID, item.ProductID, true, false);

                                }
                            }
                        }
                    }
                    show_newDeviceMessage(item, result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception :" + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Entfernt ein Gerät aus der Blackliste und fügt es zur Whitelist hinzu
        /// </summary>
        /// <param name="">Param Description</param>
        private void BlackToWhiteList(object sender, MyEventArgs e)
        {
            // int result = -1;
            if (!TempList.Contains(e.device.mChecksum))
            {
                TempList.Add(e.device.mChecksum);
            }
            activate_Device(e.device.mName, e.device.mHardwareID, e.device.mClassGuid, e.device.mDeviceType,
                e.device.mLocationInformation, e.device.mChecksum, false);
        }

        /// <summary>
        /// Entfernt ein Gerät aus der Whiteliste und fügt es zur Blacklist hinzu
        /// </summary>
        /// <param name="">Param Description</param>
        private void WhiteToBlackList(object sender, MyEventArgs e)
        {
            //Überprüfung ob Gerät aktuell am hinterlegten Geräteanschluss angeschlossen ist
            var result = CdsLib.ChangeDevState(e.device.mHardwareID, e.device.mClassGuid, e.device.mLocationInformation,
                true, ChangeDeviceStateParams.Available);
            if (result == (int)ErrorCode.Success)
            {
                result = disable_Device(e.device.mName, e.device.mHardwareID, e.device.mClassGuid, e.device.mDeviceType,
                    e.device.mLocationInformation, e.device.mVendorID, e.device.mProductID, true, true);

                //Gerät wurde am hinterlegten Anschluss nicht gefunden
                if (result == (int)ErrorCode.NotFound)
                {
                    //Durchsuche Geräteliste erneut und blockiere bei einer Übereinstimmung gefundenes Gerät (auch an anderen Geräteanschluss)
                    CdsLib.disable_USBDevice(e.device.mHardwareID, e.device.mClassGuid, e.device.mLocationInformation, false);
                }
            }
        }

        /// <summary>
        /// Fügt ein Gerät zu der Blacklist-Datenbank hinzu
        /// </summary>
        /// <param name="">Param Description</param>
        private void add_DevicesToBlackList(USBDeviceInfo device, string actualTime)
        {
            bool changeDeviceState = false;
            MessageBoxResult foundDecision = MessageBoxResult.Yes;

            if (DeviceLists.findIn_BlackList(device.Checksum))
            {
                ShowToastMessage("Device already in the Blacklist", device.DeviceType + "\nVendorID: " + device.VendorID + " ProductID: " + device.ProductID + "\nDate added: " + device.DateAdded + "is already in Blacklist", System.Windows.Forms.ToolTipIcon.Warning);
            }
            if (foundDecision == MessageBoxResult.Yes)
            {   //Datum der hinzufügung zur Liste
                device.DateAdded = actualTime;
                //Gerät der Datenbank hinzufügen
                var result = _devLists.add_DataItem("BlackListDB", device);
                if (result == false)
                {
                    ShowToastMessage("Error adding in Blacklist", "Error in adding " + device.DeviceType + "\nVendorID: " + device.VendorID + " ProductID: " + device.ProductID, System.Windows.Forms.ToolTipIcon.Error);
                }
                //Gerät wurde eventuell bisher nicht automatisch deaktiviert, deshalb Benutzer nochmal befragen
                if (Settings.Default.BlockUSBDevices == false)
                {
                    MessageBoxResult changeState = MessageBoxResult.No;
                    changeState = MessageBox.Show("Disable device\nNOW (YES-Button) or on NEXT connection (NO-Button) ?\n\nDEVICE:\nName: " + device.Name + "\nDeviceType: " + device.DeviceType + "\nVendorID: " + device.VendorID + "\nProductID: " + device.ProductID, _appTitle + "Disable " + device.DeviceType + " device NOW?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (changeState == MessageBoxResult.Yes)
                    {
                        changeDeviceState = true;
                    }
                }
                if (changeDeviceState)
                {
                    //Gerät blockieren
                    disable_Device(device.Name, device.HardwareID, device.ClassGuid, device.DeviceType, device.FirstLocationInformation, device.VendorID, device.ProductID, true, true);
                }

            }
            //Gerät aus der temporären Liste entfernen, da es nicht mehr aktiviert wird.
            TempList.Remove(device.Checksum);
        }
        #endregion
        #region GUI_Handling
        /// <summary>
        /// Schließt das Gerätebeispielfenster
        /// </summary>
        /// <param name="">Param Description</param>
        private void closeExamplesPopup_Click(object sender, RoutedEventArgs e)
        {
            PopupExamplesBox.IsOpen = false;
        }

        /// <summary>
        /// Füllt das Fenster, welches die Informationen der hinterlegten Gerätebeispiele anzeigt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void fill_USBExamplesBox(object sender, RoutedEventArgs e)
        {
            PopupExamplesBox.IsOpen = false;
            Tuple<string, string, string[], string[]> data = DeviceExamplesBox.SelectedItem as Tuple<string, string, string[], string[]>;
            if (DeviceExamplesBox.SelectedItem != null)
            {
                OverviewExamplesTextBlock.Text = string.Empty;
                OverviewExamplesTextBlock.Inlines.Add(new Run("Device: ") { FontSize = 12, FontWeight = FontWeights.Bold });
                if (data != null)
                {
                    OverviewExamplesTextBlock.Inlines.Add(data.Item1 + Environment.NewLine);
                    OverviewExamplesTextBlock.Inlines.Add(new Run("Manufacturer and Product Name: ") { FontSize = 12, FontWeight = FontWeights.Bold });
                    OverviewExamplesTextBlock.Inlines.Add(data.Item2 + Environment.NewLine);
                    OverviewExamplesTextBlock.Inlines.Add(new Run("Interfaces: ") { FontSize = 12, FontWeight = FontWeights.Bold });
                    OverviewExamplesTextBlock.Inlines.Add(data.Item3.Count() + Environment.NewLine);
                    OverviewExamplesTextBlock.Inlines.Add(new Run("Deviceclass:") { FontSize = 12, FontWeight = FontWeights.Bold });
                    OverviewExamplesTextBlock.Inlines.Add(Environment.NewLine);
                    for (int i = 0; i < data.Item3.Count(); i++)
                    {
                        OverviewExamplesTextBlock.Inlines.Add("\t" + data.Item4[i]);
                        OverviewExamplesTextBlock.Inlines.Add(Environment.NewLine);
                    }
                }
                PopupExamplesBox.IsOpen = true;
            }
        }

        /// <summary>
        /// Auflistung der erweiterten Geräteinformationen in der GUI
        /// </summary>
        /// <param name="">Param Description</param>
        private void fill_DescriptionBox(object sender, RoutedEventArgs e)
        {
            if (UsbDescriptionBox != null)
            {
                UsbDescriptionBox.Text = string.Empty;
                bool found = false;
                if (USBDevices.Count > 0)
                {
                    if (SelectNewDeviceBox.SelectedItem != null)
                    {
                        USBDeviceInfo data = SelectNewDeviceBox.SelectedItem as USBDeviceInfo;
                        if (data != null)
                        {
                            UsbDescriptionBox.Inlines.Add("Connection Date/Time: ");
                            UsbDescriptionBox.Inlines.Add(new Run(data.DateConnected + Environment.NewLine) { FontSize = 13, FontWeight = FontWeights.Bold });
                            UsbDescriptionBox.Inlines.Add("Name: ");
                            UsbDescriptionBox.Inlines.Add(data.Name + Environment.NewLine);
                            UsbDescriptionBox.Inlines.Add("Device-Type: ");
                            UsbDescriptionBox.Inlines.Add(new Run(data.DeviceType + Environment.NewLine) { FontSize = 13, FontWeight = FontWeights.Bold });
                            UsbDescriptionBox.Inlines.Add("DeviceID: ");
                            UsbDescriptionBox.Inlines.Add(data.DeviceID + Environment.NewLine);
                            UsbDescriptionBox.Inlines.Add("HardwareID: ");
                            UsbDescriptionBox.Inlines.Add(data.HardwareID + Environment.NewLine);
                            UsbDescriptionBox.Inlines.Add("ServiceName: ");
                            UsbDescriptionBox.Inlines.Add(new Run(data.Service + Environment.NewLine) { FontSize = 13, FontWeight = FontWeights.Bold });
                            UsbDescriptionBox.Inlines.Add("GUID: ");
                            UsbDescriptionBox.Inlines.Add(data.ClassGuid + Environment.NewLine);
                            UsbDescriptionBox.Inlines.Add("Serial Number: ");
                            UsbDescriptionBox.Inlines.Add(data.SerialNumber + Environment.NewLine);
                            UsbDescriptionBox.Inlines.Add("VendorID: ");
                            UsbDescriptionBox.Inlines.Add(data.VendorID);
                            UsbDescriptionBox.Inlines.Add(" ProductID: ");
                            UsbDescriptionBox.Inlines.Add(data.ProductID + Environment.NewLine);
                            UsbDescriptionBox.Inlines.Add("USBClass: ");
                            UsbDescriptionBox.Inlines.Add(data.USB_Class + "    ");
                            UsbDescriptionBox.Inlines.Add("USBSubClass: ");
                            UsbDescriptionBox.Inlines.Add(data.USB_SubClass + "     ");
                            UsbDescriptionBox.Inlines.Add("USBProtocol: ");
                            UsbDescriptionBox.Inlines.Add(data.USB_Protocol + Environment.NewLine);


                            UsbDescriptionBox.Inlines.Add("First usage: ");

                            if (data.FirstUsage == "No") { found = true; }

                            if (found)
                            {
                                UsbDescriptionBox.Inlines.Add(new Run("NO") { FontSize = 13, FontWeight = FontWeights.Bold });
                                UsbDescriptionBox.Inlines.Add(" - first connection was on the ");
                                UsbDescriptionBox.Inlines.Add(new Run(data.DateConnected + Environment.NewLine) { FontSize = 13, FontWeight = FontWeights.Bold });
                                if (data.FirstLocationInformation.Contains("Port_") && data.LastLocationInformation.Contains("Port_"))
                                {
                                    UsbDescriptionBox.Inlines.Add("The device is connected to: ");
                                    UsbDescriptionBox.Inlines.Add(new Run(data.FirstLocationInformation + Environment.NewLine) { FontSize = 13, FontWeight = FontWeights.Bold });
                                    UsbDescriptionBox.Inlines.Add("Last connection was: ");
                                    UsbDescriptionBox.Inlines.Add(new Run(data.LastLocationInformation + Environment.NewLine) { FontSize = 13, FontWeight = FontWeights.Bold });
                                }
                            }
                            else
                            {
                                UsbDescriptionBox.Inlines.Add(new Run("YES") { FontSize = 13, FontWeight = FontWeights.Bold });
                                UsbDescriptionBox.Inlines.Add(" this is the first usage of this device." + Environment.NewLine);
                                if (data.FirstLocationInformation.Contains("Port_"))
                                {
                                    UsbDescriptionBox.Inlines.Add("The device is connected to: ");
                                    UsbDescriptionBox.Inlines.Add(new Run(data.FirstLocationInformation + Environment.NewLine) { FontSize = 13, FontWeight = FontWeights.Bold });

                                }
                            }
                            UsbDescriptionBox.Inlines.Add("\nDevice DESCRIPTION:");
                            UsbDescriptionBox.Inlines.Add(Environment.NewLine);

                            var foundItem = false;
                            foreach (var elem in DeviceClass.GuidCodes)
                            {
                                if (elem.Item2 == data.ClassGuid.ToUpper())
                                {
                                    foundItem = true;
                                    UsbDescriptionBox.Inlines.Add("This device belongs to the ");
                                    UsbDescriptionBox.Inlines.Add(new Run(elem.Item1) { FontSize = 13, FontWeight = FontWeights.Bold });
                                    UsbDescriptionBox.Inlines.Add(" Class. In such an Class there are typically devices like: ");
                                    UsbDescriptionBox.Inlines.Add(Environment.NewLine + elem.Item3);
                                    break;
                                }

                            }
                            if (!foundItem)
                            {

                                UsbDescriptionBox.Inlines.Add("No description was found for the Device: " + data.Name);
                                UsbDescriptionBox.Inlines.Add(Environment.NewLine);
                            }
                            foundItem = false;
                            int index = DeviceClass.IndexClass(data);
                            if (index != -1)
                            {
                                foundItem = true;
                                UsbDescriptionBox.Inlines.Add("\nClass-Examples: ");
                                UsbDescriptionBox.Inlines.Add(DeviceClass.ClassCodes[index].Item6 + Environment.NewLine);
                            }
                            else
                            {
                                foreach (var elem in DeviceClass.ClassCodes)
                                {
                                    if (elem.Item1 == data.USB_Class)
                                    {
                                        foundItem = true;
                                        UsbDescriptionBox.Inlines.Add("\nClass-Examples: ");
                                        UsbDescriptionBox.Inlines.Add(elem.Item6 + Environment.NewLine);
                                        break;
                                    }
                                }
                            }
                            if (!foundItem)
                            {
                                UsbDescriptionBox.Inlines.Add(Environment.NewLine);
                                UsbDescriptionBox.Inlines.Add("No USB-Class description was found for the Device: " + data.Name);

                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Funktion zur Behandlung der durch den Benutzer in der GUI getroffenen Entscheidungsauswahl.
        /// Es wurde entweder Blockieren (zur Blacklist hinzufügen). Allow (Zur Whitelist hinzufügen)
        /// Oder Keine Entscheidung (zu keiner Liste hinzufügen und Gerät aktivieren) gewählt.
        /// Wurde eine spezifische Geräteauswahl durchgeführt, so werden nur diese behandelt. Sonst
        /// alle vorhandenen Geräte.
        /// </summary>
        /// <param name="">Param Description</param>
        private void userDecision_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //Get radio button decision
                var button = sender as RadioButton;

                if (SelectNewDeviceBox != null && button != null)
                {
                    if (SelectNewDeviceBox.SelectedItem != null)
                    {
                        MessageBoxResult decision;
                        var actualTime = "";
                        //Allow wurde gewählt. Geräte werden zur Whitelist hinzugefügt.
                        if (Equals(button, SelectButton1))
                        {
                            //Geräteselektion wurde durchgeführt. 
                            if (SelectedTreeItems.Count > 0)
                            {
                                handle_SelectedDevices(true);
                            }
                            //Alle dargestellten Geräte zur Whitelist.
                            else
                            {
                                decision = Settings.Default.BlockUSBDevices == false ? MessageBoxResult.Yes : MessageBox.Show("Add all devices to the WhiteList?", _appTitle + "Add devices to Whitelist",
                                     MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (decision == MessageBoxResult.Yes)
                                {
                                    if (_addDifDevices && Settings.Default.BlockUSBDevices)
                                    {

                                        for (int i = 0; i < USBDevices.Count; i++)
                                        {
                                            if (_devLists.Find_InDiffList(USBDevices[i].mDevice.Checksum))
                                            {
                                                if (!TempList.Contains(USBDevices[i].mDevice.Checksum))
                                                {
                                                    TempList.Add(USBDevices[i].mDevice.Checksum);
                                                }
                                                for (int j = 0; j > USBDevices[i].mInterfaces.Count; j++)
                                                {
                                                    if (_devLists.Find_InDiffList(USBDevices[i].mInterfaces[j].Checksum))
                                                    {
                                                        if (!TempList.Contains(USBDevices[i].mInterfaces[j].Checksum))
                                                        {
                                                            TempList.Add(USBDevices[i].mInterfaces[j].Checksum);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }


                                    actualTime = DateTime.Now.ToString();
                                    //Füge alle Geräte zur Whitelist hinzu
                                    add_DevicesToWhitelist(actualTime, true);
                                    //Aktiviere alle Geräte
                                    activate_AllDevices();
                                    //Lösche alle in der GUI dargestellten Geräte.
                                    USBDevices.Clear();
                                    //Aktualisiere Elemente der Reduzierten Darstellungsform
                                    Dispatcher.BeginInvoke(new Action(() => _noteWindow.UsbDevices_Now(0)));
                                    UsbElementsInList = 0;
                                    Dispatcher.BeginInvoke(new Action(() => _noteWindow.InterfaceDevices_Now(0)));
                                    USBItems.Clear();
                                }
                            }
                        }
                        //Blockieren wurde gewählt. Alle Geräte zur Blacklist hinzufügen
                        else if (Equals(button, SelectButton2))
                        {
                            if (SelectedTreeItems.Count > 0)
                            {
                                handle_SelectedDevices(false);
                            }
                            else
                            {
                                decision = MessageBox.Show("Add all devices to the BlackList?", _appTitle + "Add devices to Blacklist",
                                                     MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (decision == MessageBoxResult.Yes)
                                {
                                    actualTime = DateTime.Now.ToString();

                                    for (int i = 0; i < USBDevices.Count; i++)
                                    {
                                        add_DevicesToBlackList(USBDevices[i].mDevice, actualTime);
                                        for (int j = 0; j < USBDevices[i].mInterfaces.Count; j++)
                                        {
                                            add_DevicesToBlackList(USBDevices[i].mInterfaces[j], actualTime);
                                        }
                                    }
                                    USBDevices.Clear();
                                    Dispatcher.BeginInvoke(new Action(() => _noteWindow.UsbDevices_Now(0)));
                                    UsbElementsInList = 0;
                                    Dispatcher.BeginInvoke(new Action(() => _noteWindow.InterfaceDevices_Now(0)));
                                    USBItems.Clear();

                                }

                            }
                        }
                        //Auswahl 3 (Keine Entscheidung) wurde gewählt. Entferne alle Geräte aus der GUI, aktiviere sie
                        //und lösche sie aus der temporären Liste.
                        else if (Equals(button, SelectButton3))
                        {
                            if (SelectedTreeItems.Count > 0)
                            {
                                List<int> removeIndex = new List<int>();
                                decision = MessageBox.Show("Use selected device(s) without\nadding them to the White or BlackList?", _appTitle + "Add selected devices to no list",
                                                MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (decision == MessageBoxResult.Yes)
                                {

                                    if (Settings.Default.BlockUSBDevices)
                                    {

                                        decision = MessageBoxResult.Yes;
                                        if (decision == MessageBoxResult.Yes)
                                        {
                                            if (_addDifDevices && Settings.Default.BlockUSBDevices)
                                            {

                                                for (int i = 0; i < SelectedTreeItems.Count; i++)
                                                {
                                                    if (_devLists.Find_InDiffList(SelectedTreeItems[i]))
                                                    {
                                                        if (!TempList.Contains(SelectedTreeItems[i]))
                                                        {
                                                            TempList.Add(SelectedTreeItems[i]);
                                                        }
                                                    }
                                                }
                                            }

                                            actualTime = DateTime.Now.ToString();
                                            for (int i = 0; i < USBDevices.Count; i++)
                                            {
                                                if (SelectedTreeItems.Contains(USBDevices[i].mDevice.Checksum))
                                                {
                                                    HandleNextTime.Add(USBDevices[i].mDevice.Checksum);
                                                    for (int j = 0; j < USBDevices[i].mInterfaces.Count; j++)
                                                    {
                                                        HandleNextTime.Add(USBDevices[i].mInterfaces[j].Checksum);
                                                    }
                                                }
                                            }
                                            for (int i = 0; i < USBDevices.Count; i++)
                                            {
                                                if (SelectedTreeItems.Contains(USBDevices[i].mDevice.Checksum))
                                                {
                                                    USBDevices[i].mDevice.DateAdded = actualTime;
                                                    activate_Device(USBDevices[i].mDevice.Name, USBDevices[i].mDevice.HardwareID, USBDevices[i].mDevice.ClassGuid, USBDevices[i].mDevice.DeviceType, USBDevices[i].mDevice.FirstLocationInformation, USBDevices[i].mDevice.Checksum, false);


                                                    for (int j = 0; j < USBDevices[i].mInterfaces.Count; j++)
                                                    {
                                                        USBDevices[i].mInterfaces[j].DateAdded = actualTime;
                                                        activate_Device(USBDevices[i].mInterfaces[j].Name, USBDevices[i].mInterfaces[j].HardwareID, USBDevices[i].mInterfaces[j].ClassGuid, USBDevices[i].mInterfaces[j].DeviceType, USBDevices[i].mInterfaces[j].FirstLocationInformation, USBDevices[i].mInterfaces[j].Checksum, false);
                                                    }
                                                }

                                            }
                                        }
                                    }

                                    for (int i = 0; i < USBDevices.Count; i++)
                                    {
                                        if (SelectedTreeItems.Contains(USBDevices[i].mDevice.Checksum))
                                        {
                                            for (int j = USBDevices[i].mInterfaces.Count; j > 0; j--)
                                            {
                                                UsbElementsInList--;
                                                Dispatcher.BeginInvoke(new Action(() => _noteWindow.InterfaceDevices_Now(UsbElementsInList)));
                                                USBItems.Remove(USBItems.Single(s => s.Checksum == USBDevices[i].mInterfaces[j - 1].Checksum));
                                                if (TempList.Contains(USBDevices[i].mInterfaces[j - 1].Checksum))
                                                {
                                                    TempList.Remove(USBDevices[i].mInterfaces[j - 1].Checksum);
                                                }
                                                USBDevices[i].mInterfaces.Remove(USBDevices[i].mInterfaces[j - 1]);
                                            }

                                            removeIndex.Add(i);
                                            UsbElementsInList--;
                                            Dispatcher.BeginInvoke(new Action(() => _noteWindow.InterfaceDevices_Now(UsbElementsInList)));
                                            USBItems.Remove(USBItems.Single(s => s.Checksum == USBDevices[i].mDevice.Checksum));
                                            if (TempList.Contains(USBDevices[i].mDevice.Checksum))
                                            {
                                                TempList.Remove(USBDevices[i].mDevice.Checksum);
                                            }
                                        }
                                    }
                                    for (int i = removeIndex.Count; i > 0; i--)
                                    {
                                        USBDevices.Remove(USBDevices[i - 1]);
                                    }
                                    SelectedTreeItems.Clear();
                                }
                            }
                            else
                            {
                                //Sollen die Geräte aktiviert werden. Aktuell standardmäßig auf ja eingestellt.
                                decision = MessageBox.Show("Use all device(s) without\nadding them to the White- or BlackList ?", _appTitle + "Dont include device(s) to Black- or Whitelist",
                                MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (decision == MessageBoxResult.Yes)
                                {

                                    if (Settings.Default.BlockUSBDevices)
                                    {
                                        decision = MessageBoxResult.Yes;
                                        if (decision == MessageBoxResult.Yes)
                                        {
                                            if (_addDifDevices && Settings.Default.BlockUSBDevices)
                                            {

                                                for (int i = 0; i < USBDevices.Count; i++)
                                                {
                                                    if (_devLists.Find_InDiffList(USBDevices[i].mDevice.Checksum))
                                                    {
                                                        if (!TempList.Contains(USBDevices[i].mDevice.Checksum))
                                                        {
                                                            TempList.Add(USBDevices[i].mDevice.Checksum);
                                                        }
                                                        for (int j = 0; j > USBDevices[i].mInterfaces.Count; j++)
                                                        {
                                                            if (_devLists.Find_InDiffList(USBDevices[i].mInterfaces[j].Checksum))
                                                            {
                                                                if (!TempList.Contains(USBDevices[i].mInterfaces[j].Checksum))
                                                                {
                                                                    TempList.Add(USBDevices[i].mInterfaces[j].Checksum);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            for (int i = 0; i < USBDevices.Count; i++)
                                            {
                                                HandleNextTime.Add(USBDevices[i].mDevice.Checksum);
                                                for (int j = 0; j < USBDevices[i].mInterfaces.Count; j++)
                                                {
                                                    HandleNextTime.Add(USBDevices[i].mInterfaces[j].Checksum);
                                                }
                                            }
                                            activate_AllDevices();

                                            UsbElementsInList = 0;
                                            Dispatcher.BeginInvoke(new Action(() => _noteWindow.InterfaceDevices_Now(0)));

                                            Dispatcher.BeginInvoke(new Action(() => _noteWindow.UsbDevices_Now(0)));
                                            USBItems.Clear();
                                            USBDevices.Clear();
                                        }

                                    }
                                    else
                                    {
                                        UsbElementsInList = 0;
                                        Dispatcher.BeginInvoke(new Action(() => _noteWindow.InterfaceDevices_Now(0)));

                                        Dispatcher.BeginInvoke(new Action(() => _noteWindow.UsbDevices_Now(0)));
                                        USBItems.Clear();
                                        USBDevices.Clear();
                                    }

                                    TempList.Clear();
                                }
                            }
                        }
                    }
                    SelectNewDeviceBox.SelectedIndex = 0;
                    button.IsChecked = false;
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        /// <summary>
        /// Einzelgeräteauswahl-Funktion de GUI. Fügt die selektierten Geräte in die White-oder Blacklist.
        /// </summary>
        /// <param name="">Param Description</param>
        private int handle_SelectedDevices(bool selected_WhiteList)
        {
            string actualTime = DateTime.Now.ToString();
            int result = 0;
            string listName = selected_WhiteList ? "Whitelist" : "Blacklist";
            var decision = MessageBox.Show("Add selected devices to the " + listName + " ?", _appTitle + "Add selected devices to the " + listName,
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (decision == MessageBoxResult.Yes)
            {

                if (selected_WhiteList && _addDifDevices && Settings.Default.BlockUSBDevices)
                {
                    foreach (string elem in SelectedTreeItems)
                    {
                        if (_devLists.Find_InDiffList(elem))
                        {
                            if (!TempList.Contains(elem))
                            {
                                //Füge die zu behandelnden Geräte zur Temporären Liste hinzu.
                                //Hierdurch werden die durch die Aktivierung erneut erkannten Geräte nicht 
                                //erneut durch die Software-Firewall behandelt
                                TempList.Add(elem);
                            }
                        }
                    }
                }


                foreach (string devToHandle in SelectedTreeItems)
                {
                    for (int i = 0; i < USBDevices.Count; i++)
                    {
                        //Activate (device first than interfaces)
                        if (USBDevices[i].mDevice.Checksum == devToHandle)
                        {
                            if (selected_WhiteList)
                            {
                                //Füge das Hauptgerät zur Whitelist
                                add_DeviceWhitelist(USBDevices[i].mDevice, actualTime, selected_WhiteList);
                                //Aktiviert das Hauptgerät
                                activate_Device(USBDevices[i].mDevice.Name, USBDevices[i].mDevice.HardwareID, USBDevices[i].mDevice.ClassGuid, USBDevices[i].mDevice.DeviceType, USBDevices[i].mDevice.FirstLocationInformation, USBDevices[i].mDevice.Checksum, true);
                            }
                            else
                            {   //Füge das Hauptgerät zur Blacklist
                                add_DevicesToBlackList(USBDevices[i].mDevice, actualTime);
                            }

                            if (USBDevices[i].mInterfaces.Count > 0)
                            {
                                int cnt = USBDevices[i].mInterfaces.Count;
                                for (int j = cnt; j > 0; j--)
                                {
                                    if (selected_WhiteList)
                                    {
                                        //Füge die Schnittstelle zur Whitelist
                                        add_DeviceWhitelist(USBDevices[i].mInterfaces[j - 1], actualTime, selected_WhiteList);
                                        //Aktiviert die Schnitstelle
                                        activate_Device(USBDevices[i].mInterfaces[j - 1].Name, USBDevices[i].mInterfaces[j - 1].HardwareID, USBDevices[i].mInterfaces[j - 1].ClassGuid, USBDevices[i].mInterfaces[j - 1].DeviceType, USBDevices[i].mInterfaces[j - 1].FirstLocationInformation, USBDevices[i].mInterfaces[j - 1].Checksum, true);
                                    }
                                    else
                                    {
                                        //Füge die Schnittstelle zur Blacklist
                                        add_DevicesToBlackList(USBDevices[i].mInterfaces[j - 1], actualTime);
                                    }

                                }
                            }
                        }
                    }
                }

                foreach (string devToHandle in SelectedTreeItems)
                {
                    for (int i = USBDevices.Count; i > 0; i--)
                    {
                        if (USBDevices[i - 1].mDevice.Checksum == devToHandle)
                        {
                            if (USBDevices[i - 1].mInterfaces.Count > 0)
                            {
                                int cnt = USBDevices[i - 1].mInterfaces.Count;
                                for (int j = cnt; j > 0; j--)
                                {
                                    //Entferne die zur Datenank hinzugefügten Geräte aus der USBDevices und USBItems Liste, welche die 
                                    //in der GUI dargestellten Geräte beinhalten
                                    UsbElementsInList--;
                                    Dispatcher.BeginInvoke(new Action(() => _noteWindow.InterfaceDevices_Now(UsbElementsInList)));
                                    USBItems.Remove(USBItems.Single(s => s.Checksum == USBDevices[i - 1].mInterfaces[j - 1].Checksum));

                                    USBDevices[i - 1].mInterfaces.Remove(USBDevices[i - 1].mInterfaces[j - 1]);
                                }
                            }

                            UsbElementsInList--;
                            Dispatcher.BeginInvoke(new Action(() => _noteWindow.InterfaceDevices_Now(UsbElementsInList)));
                            USBItems.Remove(USBItems.Single(s => s.Checksum == USBDevices[i - 1].mDevice.Checksum));
                            USBDevices.Remove(USBDevices[i - 1]);

                        }
                    }
                }
            }

            SelectedTreeItems.Clear();

            return result;
        }

        /// <summary>
        /// Fügt ein in der GUI selektiertes Gerät zu der SelectedTreeItems Liste hinzu.
        /// </summary>
        /// <param name="">Param Description</param>
        public void TreeView_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.OriginalSource as CheckBox;
            if (cb != null)
            {
                USBDevice device = cb.DataContext as USBDevice;
                if (device != null)
                {
                    SelectedTreeItems.Add(device.mDevice.Checksum);
                }
            }
        }

        /// <summary>
        /// Entfernt ein zuvor in der GUI selektiertes Gerät aus der SelectedTreeItems. 
        /// </summary>
        /// <param name="">Param Description</param>
        public void TreeView_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.OriginalSource as CheckBox;
            if (cb != null)
            {
                USBDevice device = cb.DataContext as USBDevice;
                if (device != null)
                {
                    SelectedTreeItems.Remove(device.mDevice.Checksum);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="">Param Description</param>
        public void TreeView_SelectedItemChanged(object sender,
            RoutedPropertyChangedEventArgs<object> e)
        {
            e.Handled = true;
        }
        #endregion


        /// <summary>
        /// Erstellt eine Liste aller am System vorhandenen USB Plug-and-Play Geräte und retourniert diese.
        /// </summary>
        /// <param name="">Param Description</param>
        static List<USBDeviceInfo> collect_USBDevices()
        {
            List<USBDeviceInfo> usbItems = new List<USBDeviceInfo>();
            List<string> deviceId = new List<string>();
            try
            {
                using (var usbSearcher = new ManagementObjectSearcher(@"Select * From Win32_USBControllerDevice"))
                {
                    string lastAntecedent = "";

                    foreach (var device in usbSearcher.Get())
                    {
                        string antecedent = device["Antecedent"].ToString();
                        string fileAntecedent = antecedent.Replace(@"""", "").Replace("\\\\", "\\");
                        var deviceAntecedent = fileAntecedent.Split('=');

                        string dependent = device["Dependent"].ToString();

                        string fileDependent = dependent.Replace(@"""", "").Replace("\\\\", "\\");
                        var deviceDependent = fileDependent.Split('=');

                        if (!fileAntecedent.Equals(lastAntecedent))
                        {
                            deviceId.Add(deviceAntecedent[1]);
                        }
                        deviceId.Add(deviceDependent[1]);
                    }

                }
                using (var idSearcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity"))
                {
                    foreach (var item in idSearcher.Get())
                    {
                        if (deviceId.Contains(item["DeviceID"].ToString()))
                        {
                            string status = item.Properties["Status"].Value == null ? "" : item.Properties["Status"].Value.ToString();
                            if (!string.IsNullOrEmpty(status))
                            {
                                if (status.ToUpper().Equals("OK"))
                                {
                                    usbItems.Add(new USBDeviceInfo(item));
                                }
                            }

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return usbItems;
        }

        #region Menuentrys
        #region Menuentry_Protection
        /// <summary>
        /// Menüeintrag SAFE-Mode wurde gewählt
        /// </summary>
        /// <param name="">Param Description</param>
        private void safeMode_Click(object sender, RoutedEventArgs e)
        {
            if (MenuSafeMode.IsChecked)
            {
                var decision = MessageBox.Show("Enable SAFE-mode?\nAll devices (which are not in the White- or Initiallist)\nwill be automatically blocked and added to the blacklist.", _appTitle + "Enable Safe-mode", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (decision == MessageBoxResult.Yes)
                {
                    Settings.Default.SafeMode = true;

                    MenuBlockUsb.IsChecked = false;
                    MenuOnlymassStorage.IsChecked = false;
                    Settings.Default.AllowOnlyMassStorage = false;
                    Settings.Default.BlockUSBDevices = false;
                }
                else
                {
                    MenuSafeMode.IsChecked = false;
                }
            }
            else
            {
                MenuBlockUsb.IsChecked = true;
                Settings.Default.SafeMode = false;
                Settings.Default.BlockUSBDevices = true;
            }
            Settings.Default.Save();
        }


        /// <summary>
        /// Menüeintrag nur Massenspeicher zulassen wurde gewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void onlyMassStorage_Click(object sender, RoutedEventArgs e)
        {
            if (MenuOnlymassStorage.IsChecked)
            {
                Settings.Default.AllowOnlyMassStorage = true;

                MenuBlockUsb.IsChecked = false;
                MenuSafeMode.IsChecked = false;
                MenuBlockNewHidDevice.IsChecked = false;
                MenuBlockNewKeyboards.IsChecked = false;
                MenuBlockNewNetworkAdapters.IsChecked = false;
                MenuBlockNewMassStorage.IsChecked = false;

                Settings.Default.SafeMode = false;
                Settings.Default.BlockUSBDevices = false;
                Settings.Default.BlockNewMassStorage = false;
                Settings.Default.BlockNewHID = false;
                Settings.Default.BlockNewKeyboards = false;
                Settings.Default.BlockNewNetworkAdapter = false;
            }
            else
            {
                Settings.Default.BlockUSBDevices = true;
                Settings.Default.AllowOnlyMassStorage = false;
                MenuBlockUsb.IsChecked = true;
            }
            Settings.Default.Save();
        }


        /// <summary>
        /// Menüeintrag Massenspeicher blockieren wurde gewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void block_NewMassStorage_Click(object sender, RoutedEventArgs e)
        {
            if (MenuBlockNewMassStorage.IsChecked)
            {
                Settings.Default.BlockNewMassStorage = true;

                MenuBlockUsb.IsChecked = false;
                MenuSafeMode.IsChecked = false;

                MenuOnlymassStorage.IsChecked = false;

                Settings.Default.SafeMode = false;
                Settings.Default.BlockUSBDevices = false;

                Settings.Default.AllowOnlyMassStorage = false;

            }
            else
            {

                Settings.Default.BlockNewMassStorage = false;

                if (Settings.Default.BlockNewNetworkAdapter == false && Settings.Default.BlockNewHID == false && Settings.Default.BlockNewKeyboards == false)
                {
                    Settings.Default.BlockUSBDevices = true;
                    MenuBlockUsb.IsChecked = true;
                }
            }
            Settings.Default.Save();
        }

        /// <summary>
        /// Menüeintrag zur Blockierung von HID-Geräten wurde gewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void block_NewHID_Click(object sender, RoutedEventArgs e)
        {
            if (MenuBlockNewHidDevice.IsChecked)
            {
                Settings.Default.BlockNewHID = true;

                MenuBlockUsb.IsChecked = false;
                MenuSafeMode.IsChecked = false;
                MenuOnlymassStorage.IsChecked = false;

                Settings.Default.SafeMode = false;
                Settings.Default.AllowOnlyMassStorage = false;
                Settings.Default.BlockUSBDevices = false;
            }
            else
            {
                if (Settings.Default.BlockNewNetworkAdapter == false && Settings.Default.BlockNewKeyboards == false && Settings.Default.BlockNewMassStorage == false)
                {
                    Settings.Default.BlockUSBDevices = true;
                    MenuBlockUsb.IsChecked = true;
                }
                Settings.Default.BlockNewHID = false;
            }
            Settings.Default.Save();
        }

        /// <summary>
        /// Menüeintrag zum Blockieren von Netzwerkadapter wurde gewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void block_NewNetwork_Click(object sender, RoutedEventArgs e)
        {
            if (MenuBlockNewNetworkAdapters.IsChecked)
            {
                Settings.Default.BlockNewNetworkAdapter = true;

                MenuBlockUsb.IsChecked = false;
                MenuSafeMode.IsChecked = false;
                MenuOnlymassStorage.IsChecked = false;

                Settings.Default.SafeMode = false;
                Settings.Default.AllowOnlyMassStorage = false;
                Settings.Default.BlockUSBDevices = false;
            }
            else
            {
                if (Settings.Default.BlockNewHID == false && Settings.Default.BlockNewKeyboards == false && Settings.Default.BlockNewMassStorage == false)
                {
                    MenuBlockUsb.IsChecked = true;
                    Settings.Default.BlockUSBDevices = true;
                }
                Settings.Default.BlockNewNetworkAdapter = false;
            }
            Settings.Default.Save();
        }

        /// <summary>
        /// Menüeintrag zum Blockieren von Tastaturen wurde gewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void block_NewKeyboard_Click(object sender, RoutedEventArgs e)
        {
            if (MenuBlockNewKeyboards.IsChecked)
            {
                Settings.Default.BlockNewKeyboards = true;

                MenuBlockUsb.IsChecked = false;
                MenuSafeMode.IsChecked = false;
                MenuOnlymassStorage.IsChecked = false;

                Settings.Default.SafeMode = false;
                Settings.Default.AllowOnlyMassStorage = false;
                Settings.Default.BlockUSBDevices = false;
            }
            else
            {
                if (Settings.Default.BlockNewNetworkAdapter == false && Settings.Default.BlockNewHID == false && Settings.Default.BlockNewMassStorage == false)
                {
                    MenuBlockUsb.IsChecked = true;
                    Settings.Default.BlockUSBDevices = true;
                }
                Settings.Default.BlockNewKeyboards = false;

            }
            Settings.Default.Save();
        }


        #endregion
        /// <summary>
        /// Menüfunktion zum Löschen der White- und Blacklist
        /// </summary>
        /// <param name="">Param Description</param>
        private void deleteTables_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var decision = MessageBox.Show("Delete the Black- and Whitelist ?\n(All devices in this lists are removed)", _appTitle + "Delete Black-and Whitelist", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (decision != MessageBoxResult.Yes) return;
                _devLists.delete_Table("WhiteListDB");
                _devLists.delete_Table("BlackListDB");
                _devLists.DeviceRuleList.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// Neuerstellung der Initialisierungsliste. Alle aktuell angeschlossenen
        /// Geräte werden dieser hinzugefügt und als unbedrohlich eingestuft.
        /// </summary>
        /// <param name="">Param Description</param>
        private void rebuildInitialTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var decision = MessageBox.Show("Rebuild the Initiallist ?", _appTitle + "Rebuild Initiallist", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (decision != MessageBoxResult.Yes) return;
                //build init table
                _devLists.delete_Table("InitialListDB");
                build_DeviceTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Zeigt eine über sender angegebene Datenbank an
        /// </summary>
        /// <param name="myItem.CommandParameter">Liste welche angezeigt werden soll</param>
        private void showList_Click(object sender, RoutedEventArgs e)
        {
            MenuItem myItem = (MenuItem)sender;
            string table = myItem.CommandParameter.ToString();
            if (!string.IsNullOrEmpty(table))
            {
                _devLists.Show();
                _devLists.fill_Grid(table);
            }
        }

        /// <summary>
        /// Löscht den Inhalt einer Datenbank
        /// </summary>
        /// <param name="myItem.CommandParameter">Liste welche gelöscht werden soll</param>
        private void deleteList_Click(object sender, RoutedEventArgs e)
        {
            DeviceLists devLists = new DeviceLists();
            MenuItem myItem = (MenuItem)sender;
            string value = myItem.CommandParameter.ToString();

            var decision = MessageBox.Show("Delete " + value + " List?", _appTitle + "Delete " + value + " List", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (decision == MessageBoxResult.Yes)
            {

                devLists.delete_Table(value);
            }

        }

        /// <summary>
        /// Menüeintrag zur Aktivierung der USB-Geräteblockierung bei unbehandelten Geräten
        /// </summary>
        /// <param name="">Param Description</param>
        private void block_NewDevices(object sender, RoutedEventArgs e)
        {
            if (MenuBlockUsb.IsChecked)
            {
                ShowMessage("Blocks new devices which are not in the whitelist, without asking the user", MessageBoxButton.OK, MessageBoxImage.Information);
                Settings.Default.BlockUSBDevices = true;
            }
            else
            {
                ShowMessage("New devices will only be blocked depending on user decision", MessageBoxButton.OK, MessageBoxImage.Information);
                Settings.Default.BlockUSBDevices = false;
            }
            Settings.Default.Save();

        }

        /// <summary>
        /// Rücksetzen aller Listen und Einstellung. Bei erneutem Start wird dies erneut als erster Start angezeigt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void resetSettings_Click(object sender, RoutedEventArgs e)
        {
            var decision = MessageBox.Show("Reset settings and delete all lists?\nYES = all settings will be reset and all lists will be deleted.\nThe Application will than exit and needs a new restart.", _appTitle + "Reset all settings and lists", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (decision == MessageBoxResult.Yes)
            {
                _devLists.delete_Table("InitialListDB");
                _devLists.delete_Table("WhiteListDB");
                _devLists.delete_Table("BlackListDB");
                _devLists.delete_Table("NewStateListDB");
                _devLists.delete_Table("OldStateListDB");
                _devLists.delete_Table("DifferenceListDB");
                _devLists.delete_Table("ActualStateListDB");
                _devLists.delete_Table("TemporaryDeviceListDB");

                Settings.Default.AdminRights = false;
                Settings.Default.AllowPopupMessages = false;
                Settings.Default.BlockUSBDevices = true;
                Settings.Default.TerminateProcess = false;
                Settings.Default.Is64BitOS = false;
                Settings.Default.InitialNetworkAdapters = 0;
                Settings.Default.InitialKeyboardConnected = 0;
                Settings.Default.InitialPointingDevices = 0;
                Settings.Default.Tweak_BlockAll = false;
                Settings.Default.Tweak_BlockStorage = false;
                Settings.Default.Tweak_BlockMice = false;
                Settings.Default.Tweak_BlockKeyboards = false;
                Settings.Default.Detect_USBChanges = true;
                Settings.Default.AutomaticRemoveofDriver = true;
                Settings.Default.AdvancedSettings = false;
                Settings.Default.FirstStart = true;
                Settings.Default.ProtectiveFunction = true;
                Settings.Default.AppExitKeyboards = 0;
                Settings.Default.AppExitPointingDevices = 0;
                Settings.Default.AppExitNetworkAdapters = 0;
                Settings.Default.SafeMode = false;
                Settings.Default.Auto_StartKeylogger = false;// true;
                Settings.Default.StopApplication = true;
                Settings.Default.ContinueApplication = false;
                Settings.Default.NotifyWhitelist = true;
                Settings.Default.AllowOnlyMassStorage = false;
                Settings.Default.DetectPortChange = true;
                Settings.Default.ProhibitPortChange = true;
                Settings.Default.BlockNewKeyboards = false;
                Settings.Default.BlockNewHID = false;
                Settings.Default.BlockNewNetworkAdapter = false;
                Settings.Default.BlockNewMassStorage = false;
                Settings.Default.BlockIdenticalWhitelist = true;
                Settings.Default.Save();
                //exit the application
                _devLists.delete_TempList();
                Close();
            }
        }

        /// <summary>
        /// Zeigt das Administrative Fenster zur Geräteregelverwaltung an.
        /// </summary>
        /// <param name="">Param Description</param>
        private void configureRules_Click(object sender, RoutedEventArgs e)
        {
            _devLists.show_FirewallRules();
        }

        /// <summary>
        /// Öffnet das Administrationsfenster zur Geräteregelnverwaltung
        /// </summary>
        /// <param name="">Param Description</param>
        private void Open_FirewallRules(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _devLists.show_FirewallRules();
            }));
        }

        /// <summary>
        /// Zeige einige Hinweis-Tips
        /// </summary>
        /// <param name="">Param Description</param>
        private void btnShowHint_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hint-1:\nATTENTION: Vendor(ID and Name), Product(ID and Name), DeviceID, Manufacturer, Name and Serialnumber could be manipulated because of an BadUSB-Device" +
                            "\n\nHint-2:\nSet a BIOS password to prevent a bad USB device from accessing the BIOS." +
                            "\n\nHint-3:\nRemove unknown or not secure USB-devices (example USB flash drive) before startup of the system. Insert them first if the security applications are running.", Title, MessageBoxButton.OK, MessageBoxImage.Information);

        }

        /// <summary>
        /// Deaktivierung der Schutzfunktion der Software-Firewall
        /// </summary>
        /// <param name="">Param Description</param>
        private void disable_ProtectiveFunction_Click(object sender, RoutedEventArgs e)
        {
            if (MenuProtectiveFunction.IsChecked)
            {
                var decision = MessageBox.Show("Disable protective functions of the BadUSB-Software Firewall?", _appTitle + "Disbale protective functions", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (decision == MessageBoxResult.Yes)
                {
                    MenuOnlymassStorage.IsEnabled = false;
                    MenuSafeMode.IsEnabled = false;
                    MenuBlockUsb.IsChecked = false;
                    MenuNotifyWhitelist.IsEnabled = false;
                    MenuAutoStartKeylogger.IsEnabled = false;
                    MenuDetectUsbChanges.IsEnabled = false;
                    MenuDetectPortChange.IsEnabled = false;
                    MenuProhibitChange.IsEnabled = false;
                    MenuBlockNewKeyboards.IsEnabled = false;
                    MenuBlockIdenticalWhitelist.IsEnabled = false;

                    Settings.Default.ProtectiveFunction = false;
                    Settings.Default.Save();
                }
                else
                {
                    MenuProtectiveFunction.IsChecked = false;
                }
            }
            else
            {
                MenuOnlymassStorage.IsEnabled = true;
                MenuSafeMode.IsEnabled = true;
                MenuBlockUsb.IsChecked = true;
                MenuNotifyWhitelist.IsEnabled = true;
                MenuAutoStartKeylogger.IsEnabled = true;
                MenuDetectUsbChanges.IsEnabled = true;
                MenuDetectPortChange.IsEnabled = true;
                MenuProhibitChange.IsEnabled = true;
                MenuBlockNewKeyboards.IsEnabled = true;
                MenuBlockIdenticalWhitelist.IsEnabled = true;
                Settings.Default.ProtectiveFunction = true;
                Settings.Default.Save();
                //detect if a blocked device was connectedt through disabling of the software-firewall
                detect_Blocked();
            }
        }

        /// <summary>
        /// Menüeintrag welcher die Überwachungsfunktion für die Erfassung der Gerätedifferenzen
        /// zwischen Programmende und Start aktiviert.
        /// </summary>
        /// <param name="">Param Description</param>
        private void detect_USBChanges_Click(object sender, RoutedEventArgs e)
        {

            if (MenuDetectUsbChanges.IsChecked)
            {
                ShowMessage("A restart of the Programm or a reboot of the Operatingsystem is needed to show the changes(to create the different device-list)", MessageBoxButton.OK, MessageBoxImage.Information);
                Settings.Default.Detect_USBChanges = true;
            }
            else
            {
                Settings.Default.Detect_USBChanges = false;
            }
            Settings.Default.Save();
        }
        /// <summary>
        /// Menüeintrag zur Aktivierung des automatischen Tastatur- und Zeigegerätes-Logger
        /// </summary>
        /// <param name="">Param Description</param>
        private void auto_StartKeylogger_Click(object sender, RoutedEventArgs e)
        {

            if (MenuAutoStartKeylogger.IsChecked)
            {
                Settings.Default.Auto_StartKeylogger = true;
            }
            else
            {
                Settings.Default.Auto_StartKeylogger = false;
            }
            Settings.Default.Save();
        }





        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #region Menu_Applications
        #region Keylogger
        /// <summary>
        /// Startet den Tastatur- und Mauslogger.
        /// </summary>
        /// <param name="">Param Description</param>
        private void RunKeylogger()
        {

            if (File.Exists(Environment.CurrentDirectory + "/Resources/BadUSB_Logger.exe"))
            {
                Process proc = new Process();
                proc.StartInfo.FileName = Environment.CurrentDirectory + "/Resources/BadUSB_Logger.exe";
                proc.EnableRaisingEvents = true;
                proc.Exited += OnKeyloggerExited;
                _isRunning = true;
                proc.Start();
            }
            else
            {
                MessageBox.Show("BadUSB_Logger.exe was not found in the Resource directory!", _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Menüeintrag Keylogger wurde gewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void btnKeylogger_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Runs an simple Key and mousepress function in an seperate console window.", MessageBoxButton.OK, MessageBoxImage.Information);
            BtnKeylogger.IsEnabled = false;
            RunKeylogger();
        }

        /// <summary>
        /// Wird aufgerufen wenn der Keylogger beendet wurde, da nur eine geöffnete Instanz
        /// zugelassen wird.
        /// </summary>
        /// <param name="">Param Description</param>
        //Listen to exiting of the keylogger console application
        private void OnKeyloggerExited(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => BtnKeylogger.IsEnabled = true));
            _isRunning = false;
        }

        #endregion
        #region Device_Manager

        /// <summary>
        /// Menüeintrag zum Öffnen des Gerätemanagers wurde gewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void btnOpenDeviceManager_Click(object sender, RoutedEventArgs e)
        {
            Process proc = new Process
            {
                StartInfo = { FileName = "devmgmt.msc" },
                EnableRaisingEvents = true
            };
            proc.Exited += OnDeviceManagerExited;
            BtnOpenDeviceManager.IsEnabled = false;
            proc.Start();
        }

        /// <summary>
        /// Geräemanager wurde geschlossen
        /// </summary>
        /// <param name="">Param Description</param>
        private void OnDeviceManagerExited(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => BtnOpenDeviceManager.IsEnabled = true));
        }
        #endregion
        #region Windows_Explorer
        /// <summary>
        /// Menüeintrag zum Öffnen eines Windows-Explorer Fenster wurde gewählt
        /// </summary>
        /// <param name="">Param Description</param>
        private void btnOpenWindowsExplorer_Click(object sender, RoutedEventArgs e)
        {
            Process proc = new Process
            {
                StartInfo = { FileName = "explorer.exe" },
                EnableRaisingEvents = true
            };
            proc.Exited += OnWindowsExplorerExited;
            BtnOpenWindowsExplorer.IsEnabled = false;
            proc.Start();
        }

        /// <summary>
        /// Windows-Explorer Fenster wurde geschlossen.
        /// </summary>
        /// <param name="">Param Description</param>
        private void OnWindowsExplorerExited(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => BtnOpenWindowsExplorer.IsEnabled = true));
        }
        #endregion
        #endregion
        #endregion
        #region Whitelist_Functions
        /// <summary>
        /// Menüeintrag zum verbieten eines Whitelist-Geräteanschlusswechsel wurde gewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void prohibit_PortChange_Click(object sender, RoutedEventArgs e)
        {
            if (MenuProhibitChange.IsChecked)
            {

                Settings.Default.ProhibitPortChange = true;
            }
            else
            {
                Settings.Default.ProhibitPortChange = false;
            }
            Settings.Default.Save();
        }

        /// <summary>
        /// Menüeintrag zur Blockierung identischer Whitelistgeräte
        /// </summary>
        /// <param name="">Param Description</param>
        private void block_IdenticalWhitelist_Click(object sender, RoutedEventArgs e)
        {
            if (MenuBlockIdenticalWhitelist.IsChecked)
            {
                Settings.Default.BlockIdenticalWhitelist = true;
            }
            else
            {
                Settings.Default.BlockIdenticalWhitelist = false;
            }
            Settings.Default.Save();
        }

        /// <summary>
        /// Menüentrag zur Benachrichtigung und Blockiermöglichkeit bei Anschluss eines Whitelist-Gerätes an einen anderen USB-Anschluss
        /// </summary>
        /// <param name="">Param Description</param>
        private void notify_Whitelist_Click(object sender, RoutedEventArgs e)
        {

            if (MenuNotifyWhitelist.IsChecked)
            {
                Settings.Default.NotifyWhitelist = true;
            }
            else
            {
                Settings.Default.NotifyWhitelist = false;
            }
            Settings.Default.Save();
        }

        /// <summary>
        /// Benachrichtet über einen Whitelist-Geräteanschlusswechsel oder ein bereits vorhandenes identisches Gerät
        /// </summary>
        /// <param name="">Param Description</param>
        private void TestPort(USBDeviceInfo device, string devTable, bool connectedNow, string connectedLocation)
        {
            try
            {
                if (device != null && !string.IsNullOrEmpty(devTable))
                {
                    if (device.FirstLocationInformation != connectedLocation)
                    {
                        if (device.FirstLocationInformation.Contains("Port_"))
                        {
                            if (connectedNow)
                            {
                                ShowToastMessage("Device is already connected!", device.DeviceType + "\nDate conected:" + device.DateConnected + "\nis already connected on your System at: " + connectedLocation, System.Windows.Forms.ToolTipIcon.Warning);
                            }
                            else
                            {
                                ShowToastMessage(devTable + " device changed Port", device.DeviceType + "\nDate connected:" + device.DateConnected + "\nPort now: " + device.FirstLocationInformation + "\nPort last time: " + connectedLocation, System.Windows.Forms.ToolTipIcon.Warning);
                            }
                        }
                        else
                        {
                            if (connectedNow)
                            {
                                ShowToastMessage("Device is already connected!", device.DeviceType + "\nDate connected:" + device.DateConnected + "\nis already connected on your System", System.Windows.Forms.ToolTipIcon.Warning);
                            }
                            else
                            {
                                ShowToastMessage(devTable + " interface changed Port", device.DeviceType + "\nDate connected:" + device.DateConnected, System.Windows.Forms.ToolTipIcon.Warning);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        #endregion

        /// <summary>
        /// Öffnet ein Fenster mit einer detaillierten Netzwerkadapter-Geräteinformation. Diese wird in der GUI
        /// bei den angezeigten Netzwerkgeräten durch Auswahl eines Netzwerkadapters geöffnet. 
        /// </summary>
        /// <param name="">Param Description</param>
        private void btnShowSelectedNetworkItem_Click(object sender, SelectionChangedEventArgs e)
        {
            foreach (var obj in NetAdaptersList.SelectedItems)
            {
                var netAdapterItem = obj as NetAdapterItem;
                if (netAdapterItem != null)
                    MessageBox.Show(
                        "Adapter Name: " + netAdapterItem.AdapterName + "\n" +
                        "Availability: " + netAdapterItem.AdapterAvailability + "\n" +
                        "Caption: " + netAdapterItem.AdapterCaption + "\n" +
                        "ConfigManagerErrorCode: " + netAdapterItem.AdapterConfigManagerErrorCode + "\n" +
                        "Description: " + netAdapterItem.AdapterDescription + "\n" +
                        "Device ID: " + netAdapterItem.AdapterDeviceId + "\n" +
                        "GUID: " + netAdapterItem.AdapterGuid + "\n" +
                        "Installed: " + netAdapterItem.AdapterInstalled + "\n" +
                        "Interface Index: " + netAdapterItem.AdapterInterfaceIndex + "\n" +
                        "MAC Address: " + netAdapterItem.AdapterMacAddress + "\n" +
                        "Manufacturer: " + netAdapterItem.AdapterManufacturer + "\n" +
                        "NetConnectionID: " + netAdapterItem.AdapterNetConnectionId + "\n" +
                        "NetConnectionStatus: " + netAdapterItem.AdapterNetConnectionStatus + "\n" +
                        "NetEnabled: " + netAdapterItem.AdapterNetEnabled + "\n" +
                        "Pyhsical Adapter: " + netAdapterItem.AdapterPhysicalAdapter + "\n" +
                        "PNP Device ID: " + netAdapterItem.AdapterPnpDeviceId + "\n" +
                        "Product Name: " + netAdapterItem.AdapterProductName + "\n" +
                        "Service Name: " + netAdapterItem.AdapterServiceName + "\n" +
                        "Adapter Type: " + netAdapterItem.AdapterType + "\n" +
                        "Time of last reset: " + netAdapterItem.AdapterTimeOfLastReset
                        , _appTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
            }
        }

        #region Device_Detection_and_Handling
        /// <summary>
        /// Registriert ein HANDLE zum Empfang von WM_DEVICECHANGE Benachrichtigungen.
        /// </summary>
        /// <param name="windowHandle">Handle to the window receiving notifications.</param>
        private static void Register_DeviceNotification(IntPtr windowHandle)
        {
            DEV_BROADCAST_DEVICEINTERFACE dbi = new DEV_BROADCAST_DEVICEINTERFACE
            {
                dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE,
                dbcc_reserved = 0,
                dbcc_name = ""
            };

            dbi.dbcc_size = Marshal.SizeOf(dbi);

            //Für USB und HID Geräte registrieren. Diese beinhalten die für dieses Software-Firewall benötigten Geräteklassen.
            foreach (Guid t in GuidDeviceList)
            {
                dbi.dbcc_classguid = t;
                //Speicher allokieren
                IntPtr buffer = Marshal.AllocHGlobal(dbi.dbcc_size);
                Marshal.StructureToPtr(dbi, buffer, true);
                //Registrierung durchführen
                notificationHandle = RegisterDeviceNotification(windowHandle, buffer, 0);
                //Alokierten Speicher wieder freigeben
                Marshal.FreeHGlobal(buffer);
                if (notificationHandle == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Error in register for the device notifications.");
                }
            }
        }

        /// <summary>
        /// Wiederrufen der Benachrichtigungsregistrierung auf WM_DEVICHANGE.
        /// </summary>
        private static void Unregister_DeviceNotification()
        {
            if (notificationHandle != IntPtr.Zero)
            {
                UnregisterDeviceNotification(notificationHandle);
            }
        }

        /// <summary>
        /// Intercepts Windows messages and handles WM_DEVICECHANGE message.
        /// </summary>
        /// <param name="">Param Description</param>
        private void check_DeviceRemove()
        {
            bool changeDetected = false;

            int countNow = detect_PointingDevicesNow();
            int beforeCount = 0;
            if (countNow != SysPointingDevicesNow)
            {
                changeDetected = true;
                beforeCount = SysPointingDevicesNow;
                SysPointingDevicesNow = countNow;
                pointing_Changed(false, beforeCount);
            }

            if (!changeDetected)
            {
                countNow = detect_KeyboardNow();

                if (countNow != SysKeyboardNow)
                {
                    beforeCount = SysKeyboardNow;
                    SysKeyboardNow = countNow;
                    keyboard_Changed(false, beforeCount);
                }
            }
        }

        /// <summary>
        /// Nach der Erkennung eines Gerätes wird diese und die nachfolgenden Funktionen als eigener Thread abgearbeitet.
		/// Handelt es sich um ein Verbundgerät, so werden die Geräte der zusätzlichen Schnittstellen abgefragt.
		/// Für das Hauptgerät und eventuellen Schnittstellen wird jeweils die "device_Connected"-Funktion aufgerufen.
        /// </summary>
        /// <param name="">Param Description</param>
        private void bgWorker_DoWork(USBDeviceInfo usbDevice)
        {
            Guid devGuid = new Guid(usbDevice.ClassGuid);
            //Wenn die Schutzfunktion der Software-Firewall aktiviert ist
            if (Settings.Default.ProtectiveFunction)
            {

                if (usbDevice.FirstLocationInformation.ToUpper().Contains("PORT"))
                {
                    //Handelt es sich um ein Verbundgerät?
                    if (DeviceClass.IsComposite(usbDevice.USB_Class, usbDevice.USB_SubClass, usbDevice.USB_Protocol, usbDevice.Service))
                    {
                        List<USBDeviceInfo> childDevices = new List<USBDeviceInfo>();
                        //Zusätzliche Geräteschnittstellen einholen.
                        _cnt = CdsLib.FindChild(childDevices, usbDevice.DeviceID, usbDevice.HardwareID, usbDevice.DateConnected);

                        if (_cnt > 0)
                        {
                            string data = "";
                            for (int i = 0; i < _cnt; i++)
                            {
                                data += childDevices[i].ClassGuid;
                            }
                            //Erzeuge neue Prüfsumme
                            usbDevice.generate_HashCodeParentDevice(data);

                            //Rufe die device_Connected Funktion für Hauptgerät und Schnittstellen auf
                            device_Connected(usbDevice, devGuid);
                            foreach (var elem in childDevices)
                            {
                                Guid childGuid = new Guid(elem.ClassGuid);
                                device_Connected(elem, childGuid);
                            }
                        }
                    }
                    else
                    {
                        //Kein Verbundgerät -> nur eine Schnittstelle.
                        device_Connected(usbDevice, devGuid);
                    }
                }
                //Wurde die Spezifische Geräteklassenblockierung aktiviert?
                //Hierbei kann ein verbundenes Gerät dann auch über eine Anschlussposition
                //verfügen, welche keinen "Port"-Bezeichner in der Anschlussposition hat. 
                else if (device_ClassBlocking())
                {
                    device_Connected(usbDevice, devGuid);
                }
            }
            //Auch bei deaktiviertem Schutz werden die angeschlossenen USB-Geräte erfasst
            else
            {
                //Erstmaliger Anschluss
                if (!DeviceLists.findIn_TemporaryDeviceList(usbDevice.Checksum))
                {
                    usbDevice.FirstUsage = "Yes";
                    _devLists.add_TempDevice(usbDevice);
                }
                //Gerät wurde bereits zuvor einmal angeschlossen. Aktualisiere die Anschlussinformation
                else
                {
                    _devLists.get_TemporaryDevice(usbDevice);
                }
            }
        }

        /// <summary>
        /// Funktion, die Nachrichten verarbeitet, die an ein Fenster gesendet werden. Der WNDPROC-Typ definiert einen Zeiger auf diese Callback-Funktion.
        /// Behandelt werden hierbei nur WM_DEVICECHANGE Benachrichtigungen.
        /// </summary>
        /// <param name="DEV_BROADCAST_DEVICEINTERFACE">Enthält Informationen über eine Klasse von Geräten.</param>
        /// <param name="DBT_DEVICEARRIVAL">DBT_DEVICEARRIVAL Ereignis wird gesendet, wenn ein Gerät oder ein Medium eingelegt und verfügbar ist.</param>
        /// <param name="WM_DEVICECHANGE">                //Benachrichtigung über eine Änderung an der Hardwarekonfiguration eines Gerätes oder des Computers.</param>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {

                if (msg == WM_DEVICECHANGE)
                {
                    if (lParam != IntPtr.Zero && wParam != IntPtr.Zero)
                    {
                        DEV_BROADCAST_DEVICEINTERFACE devInterface = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));

                        var wmType = wParam.ToInt32();

                        if (wmType == DBT_DEVICEARRIVAL)
                        {
                            bool found = false;
                            USBDeviceInfo usbDevice = new USBDeviceInfo();
                            //Geräteinformationen einholen
                            found = CdsLib.getDeviceDescription(devInterface, usbDevice/*, false*/);
                            if (found && !string.IsNullOrEmpty(usbDevice.HardwareID))
                            {
                                //Weitere Bearbeitung in seperaten Thread abarbeiten um keine GUI-Blockierung herbeizuführen
                                BackgroundWorker bgWorker = new BackgroundWorker();
                                bgWorker.DoWork += (obj, e) => bgWorker_DoWork(usbDevice);
                                bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
                                bgWorker.RunWorkerAsync();
                            }
                        }
                        //DBT_DEVICEREMOVECOMPLETE-Ereignise werden gesendet, wenn ein Gerät oder Medium physisch entfernt wurde.
                        else if (wmType == DBT_DEVICEREMOVECOMPLETE)
                        {
                            if (devInterface.dbcc_classguid == Guid_HID)
                            {
                                Dispatcher.BeginInvoke(new Action(() => check_DeviceRemove()));
                            }
                        }
                    }
                }

                return IntPtr.Zero;
            }
            catch (Exception ex)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Error in receiving device notifications." + ex.Message);
            }
        }
        #endregion
        #region Device_AmountChangeDetection
        /// <summary>
        /// Erkennt ob sich bei einem HID-Geräteanschluss die Anzahl der
        /// Tastaturen, Zeigegeräte oder Netzwerkadapter geändert hat.
        /// </summary>
        /// <param name="">Param Description</param>
        private void device_Connected(USBDeviceInfo newDevice, Guid devGuid)
        {
            bool networkChanged = false;
            bool keyboardConnected = false;
            bool pointingConnected = false;

            if (DeviceClass.IsKeyboard(newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.ClassGuid, newDevice.Service))
            {
                keyboardConnected = true;
            }
            else if (DeviceClass.IsPointingDevice(newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.ClassGuid, newDevice.Service))
            {
                pointingConnected = true;
            }

            else if (devGuid == GUID_DEVCLASS_NETWORK)
            {
                //Keine Netzwerkadapter mit Microsoft als Hersteller, da 
                //diese im Gerätemanager als virtuell und daher nicht unter dern
                //Netzwerkadaptern angeführt werden
                if (!newDevice.Manufacturer.ToUpper().Contains("MICROSOFT"))
                {
                    networkChanged = true;
                    for (int i = 0; i < NetItems.Count; i++)
                    {
                        if (newDevice.DeviceID == NetItems[i].AdapterDeviceId)
                        {
                            networkChanged = false;
                            break;
                        }
                    }
                }
            }

            // Gerät wurde in der GUI durch den Benutzer mittels "No decision" behandelt. 
            //Daher keine weitere Aktion ausführen und diesen Geräteeintrag aus der HandleNextTime 
            //Liste entfernen. Hierdurch wird das Gerät bei einem erneuten Geräteanschluss wieder erkannt
            //und muss erneut behandelt werden. Nur Geräte beachten, für die ein Treiber (Service) geladen wird.
            if (!string.IsNullOrEmpty(newDevice.Service))
            {
                if (HandleNextTime.Contains(newDevice.Checksum))
                {
                    //Gerät ist aktiv
                    if (newDevice.Status == "0" || newDevice.Status == "OK")
                    {
                        HandleNextTime.Remove(newDevice.Checksum);
                    }
                }
                //Gerät wurde bisher nicht durch den Benutzer in der GUI behandelt
                else if (!TempList.Contains(newDevice.Checksum))
                {
                    handle_NewDevice(newDevice);
                }
                //Gerät wurde zuvor durch den Benutzer zu der Whitelist hinzugefügt und aktiviert.
                else
                {
                    if (TempList.Contains(newDevice.Checksum))
                    {
                        if (newDevice.Status == "0" || newDevice.Status == "OK")
                        {
                            TempList.Remove(newDevice.Checksum);
                            //Gerät überprufen
                            handle_NewDevice(newDevice);
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(newDevice.Service) && (newDevice.Status == "0" || newDevice.Status == "OK"))
                {
                    if (device_ClassBlocking())
                    {
                        handle_NewDevice(newDevice);
                    }
                }
            }
            if (keyboardConnected)
            {
                keyboard_Changed(true, SysKeyboardNow);
            }
            else if (pointingConnected)
            {
                pointing_Changed(true, SysPointingDevicesNow);
            }

            else if (networkChanged)
            {
                network_Changed();
            }
        }

        /// <summary>
        /// Funktion für die Benachrichtigung bei Änderung der Tastaturanzahl
        /// </summary>
        /// <param name="">Param Description</param>
        private void keyboard_Changed(bool checkCount, int countBefore)
        {
            if (checkCount)
            {
                SysKeyboardNow = detect_KeyboardNow();
            }
            if (!Settings.Default.BlockNewKeyboards)
            {
                Dispatcher.BeginInvoke(new Action(() => { _noteWindow.Keyboards_Now(SysKeyboardNow); }));
                if (SysKeyboardStart != SysKeyboardNow)
                {
                    int startCount = countBefore;
                    int nowCount = SysKeyboardNow;
                    Dispatcher.BeginInvoke(new Action(() => ShowToastMessage("Nr. of KEYBOARDS changed", DateTime.Now.ToString() + "\nBefore: " + startCount + " Now: " + nowCount, System.Windows.Forms.ToolTipIcon.Warning)));
                }
            }
        }

        /// <summary>
        /// Funktion für die Benachrichtigung bei Änderung der Zeiegeräteanzahl
        /// </summary>
        /// <param name="">Param Description</param>
        private void pointing_Changed(bool checkCount, int countBefore)
        {
            if (checkCount)
            {
                SysPointingDevicesNow = detect_PointingDevicesNow();
            }
            Dispatcher.BeginInvoke(new Action(() => { _noteWindow.PointingDevices_Now(SysPointingDevicesNow); }));

            int startCount = countBefore;
            int nowCount = SysPointingDevicesNow;

            if (startCount != nowCount)
            {

                Dispatcher.BeginInvoke(new Action(() => ShowToastMessage("Nr. of POINTING-DEVICES changed", DateTime.Now.ToString() + "\nBefore: " + startCount + " Now: " + nowCount, System.Windows.Forms.ToolTipIcon.Warning)));
            }
        }

        /// <summary>
        /// Funktion für die Benachrichtigung bei Änderung der Netzwerkadapter
        /// </summary>
        /// <param name="">Param Description</param>
        private void network_Changed()
        {
            Dispatcher.BeginInvoke(new Action(() => ShowToastMessage("New NETWORK-ADAPTER connected", DateTime.Now.ToString(), System.Windows.Forms.ToolTipIcon.Warning)));
        }

        #endregion

        /// <summary>
        /// Um Änderungen am Systemmenu dauerhaft einzurichten wird die Methode OnSourceInitialized(EventArgs e) überschrieben. 
        /// Aufruf dieser Methode erfolgt nach dem Konstruktor, aber sie wird noch ausgeführt bevor das Fenster der Anwendung dargestellt wird.
        /// Methode zur Registrierung einer Geräteereignis-Benachrichtigung.
        /// </summary>
        /// <param name="">Param Description</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            if (source != null)
            {
                notificationHandle = source.Handle;
                source.AddHook(WndProc);
                Register_DeviceNotification(notificationHandle);
            }

        }

    }

}
