/**
******************************************************************************
* @file	   MainWindow.xaml.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Main logic of the software firewall
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
using Microsoft.Win32;  	// Required for changes of the registry settings
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
    /// Used error codes in the project
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
        ErrorSuccessRebootRequired = 3010,  //(0xBC2) The requested operation is successful. Changes will not be effective until the system is rebooted.
        ErrorNotDisableable = -536870351,   //unchecked((int)0xe0000231),decimal -536870351(on 32bit system) or 3758096945  
        ErrorNoSuchDevinst = -536870389     //0xe000020b
    }

    /// <summary>
    /// Device properties codes for registration
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
    //Required for WM_DEVICECHANGE
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
    /// Class, which is required for the firewall rules window (FirewallRules.xaml and FirewallRules.xaml.cs
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
    /// Object used for newly discovered and displayed USB-devices in the GUI
    /// </summary>
    public class USBDevice
    {
        public USBDevice(USBDeviceInfo device)
        {
            mDevice = device;   //Main-device
            mInterfaces = new ObservableCollection<USBDeviceInfo>();//possible interfaces
        }
        public string mPosition { get; set; }
        public USBDeviceInfo mDevice { get; set; }
        public ObservableCollection<USBDeviceInfo> mInterfaces { get; set; }
    }
    #endregion
    #region Class_ChangeStateDevice
    /// <summary>
    /// USB Object for the white and black list
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BadUSBFirewall : INotifyPropertyChanged
    {
        #region Guid_Codes
        //GUID codes, which are recognized in the application
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
        //Traylist-icon
        readonly System.Windows.Forms.NotifyIcon _appIcon = new System.Windows.Forms.NotifyIcon();

        volatile bool _isRunning = false;
        private bool _continueApp = false;
        private bool _stopApp = false;

        //Information window that is displayed at first application start
        private FirstStart _firstStartWindow;
        readonly bool _firstRun = false;

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
        //Changestate class
        private static readonly ChangeDeviceState CdsLib = new ChangeDeviceState();
        //Database class
        private DeviceLists _devLists;
        
        #endregion
        //Variables and structures required for WM_Devicechange
        #region Native_Functions
        private const int DBT_DEVICEARRIVAL = 0x8000;               //A device or piece of media has been inserted and is now available.        
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;        // A device or piece of media has been removed.
        //private const int DBT_CONFIGCHANGED = 0x0018;             //The current configuration has changed, due to a dock or undock.
        private const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;  //Class of devices. 
        public const int WM_DEVICECHANGE = 0x0219;                  // Notifies an application of a change to the hardware configuration of a device or the computer.
        private static IntPtr notificationHandle;                   //device notification handle

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
        /// Main class of application
        /// </summary>
        /// <param name="">Param Description</param>
        public BadUSBFirewall()
        {
            try
            {
                //loading screen
                var splash = new SplashScreen();
                splash.Show();
                //Increase process priority
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

                _firstRun = Settings.Default.FirstStart;
                //Initialization of application components
                InitializeComponent();

                UsbTreeView.ItemsSource = USBDevices;
                DataContext = this;
                //Call the initialization function
                ApplicationInit();
                //Close charge screen
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
        /// Application initialization function
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
            //Event handler for performed operations in the white or blacklist database
            _devLists.WhiteListAdded += BlackToWhiteList;
            _devLists.BlackListAdded += WhiteToBlackList;

            //Windows Logoff or Shut Down Detect
            SystemEvents.SessionEnding += OnSessionEnding;

            Settings.Default.Upgrade();

            build_NetList();

            // Number of keyboards, pointing and network devices at program start for
            // Determine the last termination and the current number
            // network devices
            SysNetworkAdapterStart = NetItems.Count;                        //starting amount
            SysNetworkAdapterNow = NetItems.Count;                          //Number now (At program start = Start value)
            SysNetworkAdapterExit = Settings.Default.AppExitNetworkAdapters;//Number on last program termination
            //Keyboards
            SysKeyboardStart = detect_KeyboardNow();
            SysKeyboardNow = SysKeyboardStart;
            SysKeyboardExit = Settings.Default.AppExitKeyboards;
            //Pointing devices
            SysPointingDevicesStart = detect_PointingDevicesNow();
            SysPointingDevicesNow = SysPointingDevicesStart;
            SysPointingDevicesExit = Settings.Default.AppExitPointingDevices;

            if (!Settings.Default.FirstStart && Settings.Default.ContinueApplication)
            {
                // Create the temporary device list, which contains all previous
                // connected devices and the initial connection date.
                _devLists.build_TemporaryDevicesList();
            }

            if (Settings.Default.FirstStart || Settings.Default.StopApplication)
            {
                // Called if it is the first application start
                FirstApplicationRun();
            }

            if (Settings.Default.ContinueApplication)
            {
                // Detect whether 32 or 64Bit OS is used
                detect_OSVersion();
                // Detect user rights
                detect_UserRights();
                // Set the markers in the menu
                set_Markers();
                // Create temporary lists using the databases
                // White, - Black, - Initial and DeviceRuleList.
                // to check the checksum more quickly
                _devLists.build_Lists();

                // Network devices Events for detecting
                // NetworkAvailabilityChanged and NetworkAddressChanged events
                EstablishNetworkEvents();


                if (!Settings.Default.FirstStart)
                {
                    // Detection of devices that are already blocked when the program is started
                    detect_Blocked();

                    if (Settings.Default.Detect_USBChanges)
                    {
                        // Display deviations of network adapters, keyboards and pointing devices at program start
                        detect_NetworkAdaptersChanges();
                        detect_KeyboardChanges();
                        detect_PointingDevicesChanges();

                        // Create a list of currently connected USB devices
                        create_DeviceState("NewStateListDB");
                        // Comparison between the current device list created at program end
                        compare_DeviceState();
                    }
                }

                SetImage("green.PNG");
                _imgName = "green.PNG";

                Settings.Default.Save();

                if (_addDifDevices)
                {
                    // Differences of the device list (program start and end) are added to the DifferenceList
                    add_DifferencesToList();
                }

                if (Settings.Default.FirstStart)
                {
                    Settings.Default.FirstStart = false;
                }
            }

            // Menu items are added to the traybar icon
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

            // Create the reduced display window, which can be opened via the traybar icon
            _noteWindow = new ReducedInfoWidget(_imgName, USBDevices.Count, UsbElementsInList,
                SysKeyboardNow, SysPointingDevicesNow, SysNetworkAdapterNow);
            _noteWindow.Top = SystemParameters.VirtualScreenHeight - _noteWindow.Height -
                (SystemParameters.VirtualScreenHeight - SystemParameters.WorkArea.Height);
            _noteWindow.Left = SystemParameters.VirtualScreenWidth - _noteWindow.Width -
                (SystemParameters.VirtualScreenWidth - SystemParameters.WorkArea.Width);
            _noteWindow.Visibility = Visibility.Collapsed;

            // Create an approved GUID list for the guid that is detected by a DBT_DEVICEARRIVAL event.
            GuidDeviceList.Add(Guid_USBDevice);
            GuidDeviceList.Add(Guid_HID);
        }

        /// <summary>
        /// Sets the settings in the menu entries, which where present at the last application termination
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
        /// Set the background color of GUI pointer boxes
        /// Displayed when calling the Connected_Devices function
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
        /// Set background color of GUI keyboard boxes
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
        /// Set background color from the network device boxes in the GUI
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
        /// Return the program version
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
        /// Opens the reduced display window
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
        /// Function for recording the log messages
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
        /// Detect network events
        /// </summary>
        /// <param name="">Param Description</param>
        private void EstablishNetworkEvents()
        {
            try
            {
                // Occurs when the availability of the network changes.
                NetworkChange.NetworkAvailabilityChanged += Network_AvailabilityChangedEvent;
                // Occurs when the IP address of a network interface changes.
                NetworkChange.NetworkAddressChanged += Network_ChangedEvent;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in adding Network Events: " + ex.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #region Device_Deactivation_Activation
        /// <summary>
        /// Function for deactivating a USB device
        /// </summary>
        /// <param name="">Param Description</param>
        private int disable_Device(string devName, string hardwareID, string classGuid, string devType, string firstLocation, string vendorID, string productID, bool autoMode, bool displayMessage)
        {

            bool removeDecision = false;
            int conditionResult = -1;

            try
            {
                // If "Port" is included in the device port, then it is the main device
                // and not an interface device
                if (firstLocation.Contains("Port"))
                {
                    // In these modes, automatic blocking is performed without user request
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
                        // Call the disable function
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
               
                return conditionResult;
            }
            return conditionResult;
        }

        /// <summary>
        /// Activate a blocked device
        /// </summary>
        /// <param name="">Param Description</param>
        private int activate_Device(string name, string hardwareID, string classGuid, string deviceType, string deviceLocation, string checksum, bool autoMode)
        {
            int result = 0;
            MessageBoxResult decision = MessageBoxResult.Yes;
            try
            {
                // User request for activation
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

                // Perform automatic device activation
                if (decision == MessageBoxResult.Yes)
                {
                    // Call the activation function with the "ENABLE" parameter
                    result = CdsLib.ChangeDevState(hardwareID, classGuid, deviceLocation, true, ChangeDeviceStateParams.Enable);
                    switch (result)
                    {
                        case (int)ErrorCode.ErrorSuccessRebootRequired:
                            break;
                        case (int)ErrorCode.ErrorInvalidData:
                            result = CdsLib.ChangeDevState(hardwareID, classGuid, deviceLocation, true, ChangeDeviceStateParams.Enable);
                            break;
                        case (int)ErrorCode.ErrorNoSuchDevinst:
                            // Force device regeneration
                            CdsLib.ForceReenumeration();

                            result = CdsLib.ChangeDevState(hardwareID, classGuid, deviceLocation, true, ChangeDeviceStateParams.Enable);

                            if (result != (int)ErrorCode.Success)
                            {
                                    if (deviceLocation.Contains("Port"))
                                    {
                                        ShowToastMessage("Device enable error", "Could not enable this device" + "\nPlease reconnect it again ", System.Windows.Forms.ToolTipIcon.Error);
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
        /// Enable all disabled devices displayed in the GUI. Is executed when the user in the GUI does not
        /// Single selection before calling activation.
        /// </summary>
        /// <param name="">Param Description</param>
        private void activate_AllDevices()
        {

            try
            {
                for (int i = 0; i < USBDevices.Count; i++)
                {
                    // Activate the main unit. In normal cases, the interfaces are activated as well
                    activate_Device(USBDevices[i].mDevice.Name, USBDevices[i].mDevice.HardwareID, USBDevices[i].mDevice.ClassGuid, USBDevices[i].mDevice.DeviceType, USBDevices[i].mDevice.FirstLocationInformation, USBDevices[i].mDevice.Checksum, true);

                    for (int j = 0; j < USBDevices[i].mInterfaces.Count; j++)
                    {
                        // Activate the interface of the device
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
        /// Class-specific device blocking function.
        /// Also suitable for deactivating interfaces.
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
        /// Check whether a device class lock has been activated in the menu
        /// </summary>
        /// <param name="">Param Description</param>
        private bool device_ClassBlocking()
        {
            bool result = Settings.Default.BlockNewKeyboards || Settings.Default.BlockNewNetworkAdapter || Settings.Default.BlockNewHID || Settings.Default.BlockNewMassStorage;
            return result;
        }

        /// <summary>
        /// Function for the treatment of a device detected at device connection.
        /// </summary>
        /// <param name="">Param Description</param>
        private void handle_NewDevice(USBDeviceInfo newDevice)
        {
            int result = 0;

            try
            {
                // Check if the device is in the blacklist. Blacklist has top priority
                if (DeviceLists.findIn_BlackList(newDevice.Checksum))
                {
                    // Is this a composite device?
                    if (DeviceClass.IsComposite(newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.Service))
                    {
                        // Only block the main device since all its interfaces are automatically blocked (deactivated).
                        if (newDevice.FirstLocationInformation.Contains("Port"))
                        {
                            result = CdsLib.ChangeDevState(newDevice.HardwareID, newDevice.ClassGuid, newDevice.FirstLocationInformation,
                                true, ChangeDeviceStateParams.DisableComposite);
                        }
                    }
                    // No composite device, therefore a device with one interface
                    else
                    {
                        result = CdsLib.disable_USBDevice(newDevice.HardwareID, newDevice.ClassGuid, newDevice.FirstLocationInformation, true);
                    }

                    // Query the number of device interfaces
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
                // Check if the device already exists in the Whitelist.
                else if (DeviceLists.findIn_WhiteList(newDevice.Checksum))
                {
                    WhiteListFunctions(newDevice);
                }

                // Check the start list
                else if (DeviceLists.findIn_InitialList(newDevice.Checksum))
                {
                    /*do nothing at the moment, could be adjusted like handling of the whitelist-devices*/
                }

                // Check whether device class blocking has been activated in the menu.
                else if (device_ClassBlocking())
                {
                    // Block network adapter
                    if (Settings.Default.BlockNewNetworkAdapter && DeviceClass.IsNetwork(newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.ClassGuid))
                    {
                        classSpecific_Blocking(newDevice);
                    }
                    // Block the mass storage
                    else if (Settings.Default.BlockNewMassStorage && newDevice.USB_Class == "08")
                    {
                        classSpecific_Blocking(newDevice);
                    }
                    // Block HID devices
                    else if (Settings.Default.BlockNewHID && DeviceClass.IsHid(newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.ClassGuid))
                    {
                        classSpecific_Blocking(newDevice);
                        // Determine the number of pointing devices
                        SysPointingDevicesNow = detect_PointingDevicesNow();
                        // Determine the number of keyboards
                        SysKeyboardNow = detect_KeyboardNow();
                    }
                    // Block keyboards
                    else if (Settings.Default.BlockNewKeyboards && DeviceClass.IsKeyboard(newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.ClassGuid, newDevice.Service))
                    {
                        classSpecific_Blocking(newDevice);
                        SysKeyboardNow = detect_KeyboardNow();
                    }
                }

                // Only allow mass storage. All other device classes are blocked.
                else if (Settings.Default.AllowOnlyMassStorage)
                {
                    if (newDevice.USB_Class != "08")
                    {
                        classSpecific_Blocking(newDevice);
                    }

                }

                // A device that has not yet been added to a list.
                // This is blocked by default
                else
                {
                    // Is safe mode activated? If yes, block the device immediately and add it to the blacklist.
                    if (Settings.Default.SafeMode)
                    {

                        TempList.Add(newDevice.Checksum);
                        result = disable_Device(newDevice.Name, newDevice.HardwareID, newDevice.ClassGuid, newDevice.DeviceType, newDevice.FirstLocationInformation, newDevice.VendorID, newDevice.ProductID, false, false);

                        if (result == (int)ErrorCode.Success)
                        {
                            newDevice.DateAdded = DateTime.Now.ToString();

                            // Add the device to the blacklist.
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
                        /// Standard blocking function for newly recognized and untreated USB devices
                        if (Settings.Default.BlockUSBDevices)
                        {
                            // Add the device to the Templiste. This means that the device will not be checked again after activation.
                            TempList.Add(newDevice.Checksum);

                            // Is it a compound device
                            if (DeviceClass.IsComposite(newDevice.USB_Class, newDevice.USB_SubClass, newDevice.USB_Protocol, newDevice.Service))
                            {
                                if (newDevice.FirstLocationInformation.Contains("Port"))
                                {
                                    // Call the compound deactivation function
                                    result = CdsLib.ChangeDevState(newDevice.HardwareID, newDevice.ClassGuid, newDevice.FirstLocationInformation, true, ChangeDeviceStateParams.DisableComposite);
                                }
                            }
                            else
                            {
                                // No compounding device. Call up the normal deactivation function.
                                result = disable_Device(newDevice.Name, newDevice.HardwareID, newDevice.ClassGuid, newDevice.DeviceType, newDevice.FirstLocationInformation, newDevice.VendorID, newDevice.ProductID, true, false);
                            }
                        }

                        // User request during blocking
                        else
                        {
                            var decision = MessageBox.Show("The device " + newDevice.DeviceType + "VendorID: " + newDevice.VendorID + " ProductID: " + newDevice.ProductID + "\nis not included in the Black- or Whitelist.\nBLOCK it now (YES-Button) or decide in the Application (NO-Button) ?", _appTitle + "New device detected", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (decision == MessageBoxResult.Yes)
                            {
                                TempList.Add(newDevice.Checksum);
                                result = disable_Device(newDevice.Name, newDevice.HardwareID, newDevice.ClassGuid, newDevice.DeviceType, newDevice.FirstLocationInformation, newDevice.VendorID, newDevice.ProductID, true, false);
                            }
                        }

                        // Add the device to the displayed and to be treated devices of the GUI.
                        Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => add_newDevice(newDevice)));
                        // Notifies the newly detected device
                        Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => show_newDeviceMessage(newDevice, result)));

                    }
                }

                // Start the automatic keyboard and pointing device looger application as soon as it is a keyboard or pointing device.
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
        /// Additional check functions of Whitelist
        /// </summary>
        /// <param name="">Param Description</param>
        private void WhiteListFunctions(USBDeviceInfo device)
        {
            bool connectedNow = false;
            bool updatePort = false;
            bool prohibitPort = false;
            string findPort = "";

            // Query device connection information from this device from the database.
            // connectedlocation [0] = First time connection,
            // connectedlocation [1] = Last terminal position stored in the database
            string[] connectedLocation = _devLists.get_Port(device, "WhiteListDB");

            if (connectedLocation[0] != null && connectedLocation[1] != null && connectedLocation.Length == 2)
            {
                string[] hwIDParts = device.HardwareID.Split(@" ".ToCharArray());

                if (hwIDParts.Length > 0)
                {
                    // Check if another device with the same device properties is currently connected to the system.
                    findPort = CdsLib.get_DevicePort(hwIDParts[0], device.FirstLocationInformation, device.ClassGuid);

                    if (!string.IsNullOrEmpty(findPort))
                    {
                        // Identical device found
                        if (findPort != device.FirstLocationInformation && findPort != "")
                        {
                            connectedNow = true;
                        }
                    }
                }
                // If port change is prohibited (only first-time connection allowed) is activated.
                if (Settings.Default.ProhibitPortChange)
                {
                    // Current connection position deviates from the first connected connection position,
                    // therefore the connection has been changed and the device will be blocked
                    if (connectedLocation[0] != device.FirstLocationInformation)
                    {
                        prohibitPort = true;
                        // Call the Identical device function
                        IdenticalDevice(findPort, connectedLocation, device, connectedNow, false, true, prohibitPort, false);
                    }
                }

                //last device connection differs from actual first connection
                if ((connectedLocation[1] != device.FirstLocationInformation) && !prohibitPort)
                {
                    updatePort = true;
                }

                // If Identical Whitelist Blocking is enabled.
                if (Settings.Default.BlockIdenticalWhitelist && !prohibitPort)
                {
                    //detect if identical device from whitelist is already connected
                    if (connectedNow)
                    {
                        //blockPort = true;
                        IdenticalDevice(findPort, connectedLocation, device, connectedNow, updatePort, true, false, false);
                    }
                }

                // If a device connection change of a whitelist device has been carried out and the user is interrogated
                // whether the device should be blocked.
                if (Settings.Default.NotifyWhitelist && !prohibitPort && Settings.Default.ProhibitPortChange == false &&
                    device.FirstLocationInformation != connectedLocation[0])
                {
                    IdenticalDevice(findPort, connectedLocation, device, connectedNow, updatePort, false, false, true);
                }

                // Update the last connection position of the device in the database
                if (updatePort && !prohibitPort)
                {
                    _devLists.UpdatePort(device, "WhiteListDB");
                }


                if (Settings.Default.DetectPortChange)
                {
                    // Only notify if a connection change has occurred or if the identical device is present
                    TestPort(device, "WhiteListDB", connectedNow, connectedLocation[1]);
                }
            }
        }

        /// <summary>
        /// View a notifications Toast an
        /// </summary>
        /// <param name="">Param Description</param>
        private void ShowToastMessage(string title, string msg, System.Windows.Forms.ToolTipIcon icon)
        {
            Thread t = new Thread(() => MyToastMessage(title, msg, icon));
            t.Start();
        }

        /// <summary>
        /// Notifications Toast message
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
        /// Reverts the application title along with the version
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
        /// Show a Messagebox notification window in its own thread -> No blocking of the GUI
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
        /// Displays device blocking and port switching of a device.
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
        /// Adds a new device to the list, which is used to handle the device
        /// GUI is required.
        /// </summary>
        /// <param name="">Param Description</param>
        private void add_newDevice(USBDeviceInfo item)
        {
            bool foundSlot = false;
            bool contains = false;

            try
            {

                // Initial connection of the device
                if (!DeviceLists.findIn_TemporaryDeviceList(item.Checksum))
                {
                    item.FirstUsage = "Yes";
                    _devLists.add_TempDevice(item);
                }
                //device was added before
                else
                {
                    item.FirstUsage = "No";
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
        /// Start the keyboard and pointing device logger application
        /// </summary>
        /// <param name="">Param Description</param>
        private void RunKeyLogger(string newGuid, string usbClass, string usbSubClass, string usbProtocol, string service)
        {
            //Guid tempGuid = new Guid(newGuid.ToUpper());
            bool runLogger = (DeviceClass.IsKeyboard(usbClass, usbSubClass, usbProtocol, newGuid, service) || (DeviceClass.IsPointingDevice(usbClass, usbSubClass, usbProtocol, newGuid, service)));

            if (runLogger)
            {
                // Allow only one instance
                if (_isRunning) return;
                _isRunning = true;
                Dispatcher.BeginInvoke(new Action(() => BtnKeylogger.IsEnabled = false));
                RunKeylogger();
            }
        }

        /// <summary>
        /// Call when the pointing device logger is terminated.
        /// </summary>
        /// <param name="">Param Description</param>
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // check an possible error or if a cancel occured
            if (e.Error != null)
            {
            }
            else if (e.Cancelled)
            {
            }

        }

        /// <summary>
        /// List of all existing network devices
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
        /// Network Device Initialization List. Only network adapters that are active.
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
        /// Device list, which is used to display the tree structure of the devices in the GUI.
        /// These devices are displayed in the lower view of the GUI (minimum device properties).
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
        /// Displays recognized devices in the GUI. Top display window
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
                    // Reduced display form Update devices
                    Dispatcher.BeginInvoke(new Action(() => _noteWindow.UsbDevices_Now(USBDevices.Count)));

                    ConnectedDevice_Box.Text = string.Empty;

                    for (int i = 0; i < USBDevices.Count; i++)
                    {
                        Brush textColour = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF92D050");
                        ConnectedDevice_Box.Inlines.Add(new Run(" [" + USBDevices[i].mPosition + "]: ") { FontSize = 12, FontWeight = FontWeights.Bold });
                        var foundItem = false;

                        // Are device information on a device of this class and type present in the device classes?
                        var index = DeviceClass.IndexClass(USBDevices[i].mDevice);

                        // Does the device belong to a device of the hazard level?
                        if (DeviceClass.ContainsThreatClass(USBDevices[i].mDevice))
                        {
                            containsThreat = true; foundThreat = true;
                        }
                        // Is the device a suspect device?
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

                        // Device has already been connected to this system while the software firewall is running
                        if (USBDevices[i].mDevice.FirstUsage == "No")
                        {
                            string changed = USBDevices[i].mDevice.FirstLocationInformation != USBDevices[i].mDevice.LastLocationInformation ? "Yes" : "No";
                            ConnectedDevice_Box.Inlines.Add(new Run(USBDevices[i].mDevice.FirstUsage) { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black });
                            ConnectedDevice_Box.Inlines.Add(seperator + "PORT changed: ");
                            ConnectedDevice_Box.Inlines.Add(new Run(changed + Environment.NewLine) { FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black });
                        }

                        // Device has not been connected to this system while the software firewall is running
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
                                //Interfaces of the device
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

                        // Change the displayed threat level icon to the Danger level
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => _noteWindow.SetImage(_imgName)));
                    }
                    else if (foundSuspect)
                    {
                        if (!Equals(ConnectedDeviceGroupBox.Background, Brushes.Red))
                        {
                            // Change the displayed threat level icon to the Suspicious level
                            ConnectedDeviceGroupBox.Background = Brushes.Yellow;
                            SetImage("yellow.PNG");
                            _imgName = "yellow.PNG";
                            Application.Current.Dispatcher.BeginInvoke(new Action(() => _noteWindow.SetImage(_imgName)));
                        }
                    }
                    else
                    {
                        // Change the symbol displayed to the threat level to the non-hazardous level
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
        /// Sets the graphic for the displayed symbol
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
        /// Number of USB devices to be handled.
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
        /// Number of network adapters at the last program termination.
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
        /// Number of network adapters currently.
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
        /// Number of network adapters at program start.
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
        /// Number of keyboards at the last program termination.
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
        /// Number of keyboards at program start.
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
        /// Number of keyboards currently.
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
        /// Number of pointers on the last program termination.
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
        /// Number of pointing devices at program start.
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
        /// Number of pointing devices current.
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
        /// Returns whether the user has administration rights
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
        /// Blocking function for identical whitelist units and port exchange.
        /// Opens the window which informs the user whether the device has changed its port-connection or
        /// is already connected. This device can also be blocked.
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
                                // Newly connected device, which has the connection information of the device listed as Whitelist. An additional device
                                // with the same properties is already connected, so this is blocked.
                                result = disable_Device(device.Name, device.HardwareID, device.ClassGuid, device.DeviceType, devicePort, device.VendorID, device.ProductID, true, true);//new
                            }
                            else
                            {
                                // Newly connected device next to already identical connected and whitelist device. Block new device.
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
                            // Show the notification window
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
        /// First start of the application
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
                // Generation of databases
                _devLists.CreateDatabase();
                build_DeviceTables();

                // Copy the initial devices to the temporary list
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
        /// Detects whether the number of network adapters differs between program end and start.
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
        /// Detection whether the number of pointers differs between program end and start.
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
        /// Capture the currently available pointing devices on the system
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
        /// Notification if the number of keyboards differs between program end and start.
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
        /// Capture the currently available keyboards
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
            // Capture the list of USB devices present on the system.
            var getDevices = collect_USBDevices();
            if (getDevices.Count > 0)
            {
                var querryInitial = "InitialListDB";

                foreach (var item in getDevices)
                {
                    if (!string.IsNullOrEmpty(item.Service))
                    {
                        item.DateAdded = DateTime.Now.ToString();
                        // Add devices to the initiallist.
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
        /// Opens a web page to check the VendorID and ProductID
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
        /// Detecting the user rights
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
        /// Detect whether 32 or 64-bit operating system is used
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
        /// Shows notifications in a messagebox
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
        /// Changes the connection change detection menu entry
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
        /// // Occurs when the IP address of a network interface changes.
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
        /// // Occurs when the availability of a network changes.
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
        /// Generation of networkdevices-list.
        /// </summary>
        /// <param name="InitNetworkAdapters">All existing network adapters</param>
        /// <param name="NetItems">All physically available network adapters</param>
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
        /// Displays the table with the device differences (recorded between program end and start).
        /// </summary>
        /// <param name="">Param Description</param>
        private void showTable(string table)
        {
            DeviceLists devLists = new DeviceLists();
            devLists.Show();

            devLists.fill_Grid(table);
        }

        /// <summary>
        /// Compares the created deviceists (OldStateListDB and NewStateListDB) to differences.
        /// </summary>
        /// <param name="OldStateListDB">Created at programs end</param>
        /// <param name="NewStateListDB">Created at programs start</param>
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
        /// Displays a list of all currently available USB devices on the system
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
        /// The NewStateListDB generates the OldStateListDB at program start or at program end
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
                            // Only add devices that have an accepted GUID and are not available in the Black or Whitelist.
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
        /// To detect connected devices in the blacklist.
        /// Executed at program start and after deactivation of the SAFE mode.
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
        /// Own treatment at program termination
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
                // Create the OldStateListDB
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
        /// Adds a newly discovered device to Whitelist
        /// </summary>
        /// <param name="">Param Description</param>
        private void add_DeviceWhitelist(USBDeviceInfo device, string actualTime, bool addWhiteList)
        {
            try
            {
                // Check if it already exists
                if (DeviceLists.findIn_WhiteList(device.Checksum))
                {
                    // Notify that device already exists.
                    Dispatcher.BeginInvoke(new Action(() => ShowToastMessage("Identical Whitelist device found", device.Name + "\n" + device.DeviceType + "\nVendorID:" + device.VendorID + " ProductID:" + device.ProductID + "\nDate added:" + device.DateAdded + "is already in the Whitelist", System.Windows.Forms.ToolTipIcon.Warning)));
                }

                else
                {
                    if (!addWhiteList) return;
                    // add date to the list = actual time
                    device.DateAdded = actualTime;
                    // Add to database
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
        /// Add all devices in the GUI to the Whitelist. Is executed when the user
        /// in the GUI does not make a single selection before calling the allow function.
        /// </summary>
        /// <param name="">Param Description</param>
        private void add_DevicesToWhitelist(string actualTime, bool addWhiteList)
        {
            try
            {
                for (int i = 0; i < USBDevices.Count; i++)
                {
                    // Add the main device
                    add_DeviceWhitelist(USBDevices[i].mDevice, actualTime, addWhiteList);

                    for (int j = 0; j < USBDevices[i].mInterfaces.Count; j++)
                    {
                        // Add the interface
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
        /// Add detected device deviations between program end and start to the difference list
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
                        // Do not add any whitelist
                        if (!DeviceLists.findIn_WhiteList(item.Checksum))
                        {
                            // Check if the device is currently connected
                            result = CdsLib.ChangeDevState(item.HardwareID, item.ClassGuid, item.FirstLocationInformation, true, ChangeDeviceStateParams.Available);
                            if (Settings.Default.BlockUSBDevices)
                            {
                                if (result == (int)ErrorCode.Success)
                                {
                                    // Disabling the device
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
        /// Removes a device from the Blacklist and adds it to Whitelist
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
        /// Removes a device from the Whiteliste and adds it to the Blacklist
        /// </summary>
        /// <param name="">Param Description</param>
        private void WhiteToBlackList(object sender, MyEventArgs e)
        {
            // Check whether the device is currently connected to the connected device connection
            var result = CdsLib.ChangeDevState(e.device.mHardwareID, e.device.mClassGuid, e.device.mLocationInformation,
                true, ChangeDeviceStateParams.Available);
            if (result == (int)ErrorCode.Success)
            {
                result = disable_Device(e.device.mName, e.device.mHardwareID, e.device.mClassGuid, e.device.mDeviceType,
                    e.device.mLocationInformation, e.device.mVendorID, e.device.mProductID, true, true);

                // Device could not be found on the connected port
                if (result == (int)ErrorCode.NotFound)
                {
                    // Recheck the device list again and block a device found on a match (also on another device port)
                    CdsLib.disable_USBDevice(e.device.mHardwareID, e.device.mClassGuid, e.device.mLocationInformation, false);
                }
            }
        }

        /// <summary>
        /// Adds a device to the Blacklist database
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
            {
                // Date of addition to the list
                device.DateAdded = actualTime;
                // Add the device to the database
                var result = _devLists.add_DataItem("BlackListDB", device);
                if (result == false)
                {
                    ShowToastMessage("Error adding in Blacklist", "Error in adding " + device.DeviceType + "\nVendorID: " + device.VendorID + " ProductID: " + device.ProductID, System.Windows.Forms.ToolTipIcon.Error);
                }
                // Device may not have been disabled automatically, so ask the user again
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
                    // Block the device
                    disable_Device(device.Name, device.HardwareID, device.ClassGuid, device.DeviceType, device.FirstLocationInformation, device.VendorID, device.ProductID, true, true);
                }

            }
            // Remove the device from the temporary list because it is no longer activated.
            TempList.Remove(device.Checksum);
        }
        #endregion
        #region GUI_Handling
        /// <summary>
        /// Closes the device example window
        /// </summary>
        /// <param name="">Param Description</param>
        private void closeExamplesPopup_Click(object sender, RoutedEventArgs e)
        {
            PopupExamplesBox.IsOpen = false;
        }

        /// <summary>
        /// Fills the window, which displays the information of the stored device examples.
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
        /// Lists the extended device information in the GUI
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
        /// Function to handle the decision selection made by the user in the GUI.
        /// It was either Block (add to Blacklist). Allow (Add to Whitelist)
        /// Or No decision (do not add to any list and activate device).
        /// If a specific device selection has been carried out, only these are treated. Otherwise
        /// all existing devices.
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
                        // Allow was elected. Devices are added to Whitelist.
                        if (Equals(button, SelectButton1))
                        {
                            // Device selection has been performed.
                            if (SelectedTreeItems.Count > 0)
                            {
                                handle_SelectedDevices(true);
                            }
                            // All devices shown to Whitelist.
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
                                    // Add all devices to Whitelist
                                    add_DevicesToWhitelist(actualTime, true);
                                    // Activate all devices
                                    activate_AllDevices();
                                    // Delete all devices displayed in the GUI
                                    USBDevices.Clear();
                                    // Update elements of the Reduced Representation form
                                    Dispatcher.BeginInvoke(new Action(() => _noteWindow.UsbDevices_Now(0)));
                                    UsbElementsInList = 0;
                                    Dispatcher.BeginInvoke(new Action(() => _noteWindow.InterfaceDevices_Now(0)));
                                    USBItems.Clear();
                                }
                            }
                        }
                        // Block was selected. Add all devices to the Blacklist
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
                        // Select 3 (No decision) has been selected. Remove all devices from the GUI, activate them
                        // and delete them from the temporary list.
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
                                // The devices should be activated. Currently set to Yes by default.
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
        /// Single device selection function de GUI. Adds the selected devices to the White or Blacklist.
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
                                // Add the devices to be treated to the temporary list.
                                // This does not recognize the devices detected by the activation again
                                // again handled by the software firewall
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
                                //Add the main-deviceto the whitelist
                                add_DeviceWhitelist(USBDevices[i].mDevice, actualTime, selected_WhiteList);
                                //Activate the main-device
                                activate_Device(USBDevices[i].mDevice.Name, USBDevices[i].mDevice.HardwareID, USBDevices[i].mDevice.ClassGuid, USBDevices[i].mDevice.DeviceType, USBDevices[i].mDevice.FirstLocationInformation, USBDevices[i].mDevice.Checksum, true);
                            }
                            else
                            {  
                                //Add the main-device to the blacklist
                                add_DevicesToBlackList(USBDevices[i].mDevice, actualTime);
                            }

                            if (USBDevices[i].mInterfaces.Count > 0)
                            {
                                int cnt = USBDevices[i].mInterfaces.Count;
                                for (int j = cnt; j > 0; j--)
                                {
                                    if (selected_WhiteList)
                                    {
                                        //Add the interface to the whitelist
                                        add_DeviceWhitelist(USBDevices[i].mInterfaces[j - 1], actualTime, selected_WhiteList);
                                        //Activate the interface
                                        activate_Device(USBDevices[i].mInterfaces[j - 1].Name, USBDevices[i].mInterfaces[j - 1].HardwareID, USBDevices[i].mInterfaces[j - 1].ClassGuid, USBDevices[i].mInterfaces[j - 1].DeviceType, USBDevices[i].mInterfaces[j - 1].FirstLocationInformation, USBDevices[i].mInterfaces[j - 1].Checksum, true);
                                    }
                                    else
                                    {
                                        //Add the interface o the blacklist
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
                                    // Remove the devices added to the datank from the USBDevices and USBItems list, which contain the
                                    // in the GUI
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
        /// Adds a device selected in the GUI to the SelectedTreeItems list.
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
        /// Removes a device selected in the GUI from the SelectedTreeItems.
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
        /// Creates and returns a list of all USB plug-and-play devices present on the system.
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
        /// Menu item SAFE-Mode has been selected
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
        /// Allow entry of mass storage only was selected.
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
        /// Block Mass storage menu entry was selected.
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
        //// Menu entry for blocking HID devices was selected.
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
        /// Menu entry for blocking network adapter was selected.
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
        /// Menu entry for blocking keyboards has been selected.
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
        /// Menu function to delete the white and blacklist
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
        /// Restore the initialization list. All currently connected
        /// devices are added to it and classified as irrelevant.
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
        /// Displays a parent named database
        /// </summary>
        /// <param name="myItem.CommandParameter">List which is to be displayed</param>
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
        /// Deletes the contents of a database
        /// </summary>
        /// <param name="myItem.CommandParameter">List which you want to delete</param>
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
        /// Menu item for activating USB device blocking for untreated devices
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
        /// Reset all lists and setting. When this is started again, this is displayed again as the first start.
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
                Settings.Default.Auto_StartKeylogger = false;
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
        /// Displays the administrative window for device management.
        /// </summary>
        /// <param name="">Param Description</param>
        private void configureRules_Click(object sender, RoutedEventArgs e)
        {
            _devLists.show_FirewallRules();
        }

        /// <summary>
        /// Opens the administration window for device management
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
        /// Show some tips
        /// </summary>
        /// <param name="">Param Description</param>
        private void btnShowHint_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hint-1:\nATTENTION: Vendor(ID and Name), Product(ID and Name), DeviceID, Manufacturer, Name and Serialnumber could be manipulated because of an BadUSB-Device" +
                            "\n\nHint-2:\nSet a BIOS password to prevent a bad USB device from accessing the BIOS." +
                            "\n\nHint-3:\nRemove unknown or not secure USB-devices (example USB flash drive) before startup of the system. Insert them first if the security applications are running.", Title, MessageBoxButton.OK, MessageBoxImage.Information);

        }

        /// <summary>
        /// Disable the protection of the software firewall
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
        /// Menu entry which is the monitoring function for the detection of the device differences
        /// between program end and start.
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
        /// Menu item for activating the automatic keyboard and pointing device logger
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
        /// /// Starts the keyboard and mouse logger.
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
        /// Menu item Keylogger has been selected.
        /// </summary>
        /// <param name="">Param Description</param>
        private void btnKeylogger_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage("Runs an simple Key and mousepress function in an seperate console window.", MessageBoxButton.OK, MessageBoxImage.Information);
            BtnKeylogger.IsEnabled = false;
            RunKeylogger();
        }

        /// <summary>
        /// Invoked when the keylogger has finished, because only one open instance
        /// is allowed.
        /// </summary>
        /// <param name="">Param Description</param>
        private void OnKeyloggerExited(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => BtnKeylogger.IsEnabled = true));
            _isRunning = false;
        }

        #endregion
        #region Device_Manager

        /// <summary>
        /// Menu entry for opening the device manager was selected.
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
        /// Device manager has been closed
        /// </summary>
        /// <param name="">Param Description</param>
        private void OnDeviceManagerExited(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => BtnOpenDeviceManager.IsEnabled = true));
        }
        #endregion
        #region Windows_Explorer
        /// <summary>
        /// Menu entry for opening a Windows Explorer window was selected
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
        /// Windows Explorer window has been closed.
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
        /// menu item for prohibiting a Whitelist device connection change was selected.
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
        //// Menu entry for blocking identical whitelist devices
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
        /// Menu item for notification and blocking when connecting a Whitelist device to another USB port
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
        /// Notified by a Whitelist device connection change or an existing identical device
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
        /// Opens a window with detailed network adapter device information. This is done in the GUI
        /// in the displayed network devices by selecting a network adapter.
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
        /// Register a HANDLE to receive WM_DEVICECHANGE notifications.
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

            // Register for USB and HID devices. These include the device classes required for this software firewall.
            foreach (Guid t in GuidDeviceList)
            {
                dbi.dbcc_classguid = t;
                //Allocate memory
                IntPtr buffer = Marshal.AllocHGlobal(dbi.dbcc_size);
                Marshal.StructureToPtr(dbi, buffer, true);
                //Carry out registration
                notificationHandle = RegisterDeviceNotification(windowHandle, buffer, 0);
                // Free allocated memory
                Marshal.FreeHGlobal(buffer);
                if (notificationHandle == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Error in registering of the device notification handle.");
                }
            }
        }

        /// <summary>
        /// Undo the notification registration on WM_DEVICECHANGE.
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
        /// After recognizing a device, this and the subsequent functions are executed as a separate thread.
        /// In the case of a compound device, the devices of the additional interfaces are interrogated.
        /// The "device_Connected" function is called for the main device and any interfaces.
        /// </summary>
        /// <param name="">Param Description</param>
        private void bgWorker_DoWork(USBDeviceInfo usbDevice)
        {
            Guid devGuid = new Guid(usbDevice.ClassGuid);
            // If the protection function of the software firewall is activated
            if (Settings.Default.ProtectiveFunction)
            {

                if (usbDevice.FirstLocationInformation.ToUpper().Contains("PORT"))
                {
                    // Is it a compound device?
                    if (DeviceClass.IsComposite(usbDevice.USB_Class, usbDevice.USB_SubClass, usbDevice.USB_Protocol, usbDevice.Service))
                    {
                        List<USBDeviceInfo> childDevices = new List<USBDeviceInfo>();
                        // Obtain additional device interfaces.
                        _cnt = CdsLib.FindChild(childDevices, usbDevice.DeviceID, usbDevice.HardwareID, usbDevice.DateConnected);

                        if (_cnt > 0)
                        {
                            string data = "";
                            for (int i = 0; i < _cnt; i++)
                            {
                                data += childDevices[i].ClassGuid;
                            }
                            // Create a new checksum
                            usbDevice.generate_HashCodeParentDevice(data);

                            // Call the device_Connected function for the main unit and interfaces
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
                        // No compound device -> only one interface.
                        device_Connected(usbDevice, devGuid);
                    }
                }
                // Is the specific device class blocking activated?
                // In this case, a connected device can also be connected via a connection position
                // which has no "port" identifier in the port position.
                else if (device_ClassBlocking())
                {
                    device_Connected(usbDevice, devGuid);
                }
            }
            // Even if the protection is deactivated, the connected USB devices are recorded
            else
            {
                // First time connection
                if (!DeviceLists.findIn_TemporaryDeviceList(usbDevice.Checksum))
                {
                    usbDevice.FirstUsage = "Yes";
                    _devLists.add_TempDevice(usbDevice);
                }
                // Device has already been connected once. Update the connection information
                else
                {
                    _devLists.get_TemporaryDevice(usbDevice);
                }
            }
        }

        /// <summary>
        /// Function that processes messages sent to a window. The WNDPROC type defines a pointer to this callback function.
        /// Only WM_DEVICECHANGE notifications are handled.
        /// </summary>
        /// <param name="DEV_BROADCAST_DEVICEINTERFACE">Contains information about a class of devices.</param>
        /// <param name="DBT_DEVICEARRIVAL">DBT_DEVICEARRIVAL Event is sent when a device or media is inserted and available.</param>
        /// <param name="WM_DEVICECHANGE">Notification of a change to the hardware configuration of a device or the computer.</param>
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
                            // Get device information
                            found = CdsLib.getDeviceDescription(devInterface, usbDevice);
                            if (found && !string.IsNullOrEmpty(usbDevice.HardwareID))
                            {
                                // Execute further processing in a separate thread in order to avoid a GUI blocking
                                BackgroundWorker bgWorker = new BackgroundWorker();
                                bgWorker.DoWork += (obj, e) => bgWorker_DoWork(usbDevice);
                                bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
                                bgWorker.RunWorkerAsync();
                            }
                        }
                        // DBT_DEVICEREMOVECOMPLETE events are sent when a device or medium is physically removed.
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
        /// Determines whether the number of devices (keyboards, pointing devices, or network adapters) when connecting a device has changed
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
                // No network adapter with Microsoft as manufacturer.
                // These are listed as virtual in the device manager and therefore not added to the network adapters
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

            // Device has been treated in the GUI by the user using "No decision".
            // Therefore, do not perform any further action and remove this device entry from the HandleNextTime
            // Remove list. This means that the device is recognized again when the device is connected again
            // and must be treated again. Only observe devices for which a driver (service) is loaded.
            if (!string.IsNullOrEmpty(newDevice.Service))
            {
                if (HandleNextTime.Contains(newDevice.Checksum))
                {
                    //Device is active
                    if (newDevice.Status == "0" || newDevice.Status == "OK")
                    {
                        HandleNextTime.Remove(newDevice.Checksum);
                    }
                }
                // device has not yet been handled by the user in the GUI
                else if (!TempList.Contains(newDevice.Checksum))
                {
                    handle_NewDevice(newDevice);
                }
                // Device has been previously added and activated by the user to the Whitelist.
                else
                {
                    if (TempList.Contains(newDevice.Checksum))
                    {
                        if (newDevice.Status == "0" || newDevice.Status == "OK")
                        {
                            TempList.Remove(newDevice.Checksum);
                            // Check the device
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
        /// Function for notification when changing the number of keys
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
        /// Function for notification when changing the number of ponting devices
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
        /// Function for notification when changing network adapters
        /// </summary>
        /// <param name="">Param Description</param>
        private void network_Changed()
        {
            Dispatcher.BeginInvoke(new Action(() => ShowToastMessage("New NETWORK-ADAPTER connected", DateTime.Now.ToString(), System.Windows.Forms.ToolTipIcon.Warning)));
        }

        #endregion

        /// <summary>
        /// The OnSourceInitialized (EventArgs e) method is overwritten to permanently set changes to the system menu.
        /// This method is called after the constructor, but it is still executed before the window of the application is displayed.
        /// Method for registering a device event notification.
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
