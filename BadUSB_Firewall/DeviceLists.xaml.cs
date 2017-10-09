/**
************************************************************************************************
* @file	   DeviceLists.xaml.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Functions, which are used for the databases for display and administration
************************************************************************************************
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Data.SQLite;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace BadUSB_Firewall
{
    //Device class used for administrative device management (FirewallRules.xaml FirewallRules.xaml.cs)
    public class RemoveRuleDevice
    {
        public RemoveRuleDevice(string checksum, string dateAdded, string vendorID, string productID, string hardwareID, string removeTable)
        {
            Checksum = checksum;
            DateAdded = dateAdded;
            HardwareID = hardwareID;
            ProductID = productID;
            RemoveTable = removeTable;
            VendorID = vendorID;
        }

        public string Checksum { get; set; }
        public string DateAdded { get; set; }
        public string VendorID { get; set; }
        public string ProductID { get; set; }
        public string HardwareID { get; set; }
        public string RemoveTable { get; set; }
    }

    public class ChangePortDevice
    {
        public ChangePortDevice(string location, string id, string checksum)
        {
            mLocation = location;
            mID = id;
            mChecksum = checksum;
        }
        public string mLocation { get; set; }
        public string mID { get; set; }
        public string mChecksum { get; set; }
    }
    public class MyEventArgs : EventArgs
    {
        public ChangeStateDevice device { get; set; }

        public MyEventArgs(ChangeStateDevice changeDevice)
        {
            device = changeDevice;//
        }

    }

    /// <summary>
    /// Interaction logic for DeviceLists.xaml
    /// </summary>
    public partial class DeviceLists
    {
        //Temporary lists for better performance
        private static List<string> BlackList = new List<string>();
        private static List<string> WhiteList = new List<string>();
        private static List<string> InitialList = new List<string>();
        private static List<string> DiffList = new List<string>();
        private static ObservableCollection<DeviceEntry> _deviceRuleList = new ObservableCollection<DeviceEntry>();


        private static List<USBDeviceInfo> TemporaryDevicesList = new List<USBDeviceInfo>();

        public event EventHandler<MyEventArgs> BlackListAdded;
        public event EventHandler<MyEventArgs> WhiteListAdded;

        private readonly string addItem = "(ID,Added__to__list,First__connection, Name, DeviceType, USB_Class,USB_SubClass,USB_Protocol,VendorID,Vendor_Name,ProductID, ProductName,DeviceID,ClassGUID, HardwareID,First__connection__location,Last__connection__location, Serial_Number,Service,Checksum) VALUES (@ID,@Added__to__list,@First__connection,  @Name, @DeviceType,@USB_Class, @USB_SubClass, @USB_Protocol, @VendorID, @Vendor_Name,@ProductID, @ProductName,@DeviceID, @ClassGUID,   @HardwareID,@First__connection__location, @Last__connection__location, @Serial_Number, @Service,  @Checksum)";
        private readonly string databaseParameters = "ID ,Added__to__list,First__connection, Name, DeviceType, USB_Class,USB_SubClass,USB_Protocol,VendorID,Vendor_Name,ProductID, ProductName,DeviceID,ClassGUID, HardwareID,First__connection__location,Last__connection__location, Serial_Number,Service,Checksum";
        private readonly string compareParameters = "Null,Added__to__list,First__connection, Name, DeviceType,USB_Class,USB_SubClass,USB_Protocol,VendorID,Vendor_Name,ProductID, ProductName,DeviceID,ClassGUID, HardwareID,First__connection__location,Last__connection__location, Serial_Number,Service,Checksum";
        private string _activeTable;
        private readonly string addItemParameters = "NULL,Added__to__list,First__connection, Name, DeviceType, USB_Class,USB_SubClass,USB_Protocol,VendorID,Vendor_Name,ProductID, ProductName,DeviceID, ClassGUID, HardwareID,First__connection__location,Last__connection__location, Serial_Number,Service,Checksum";
        private readonly string createNewTable = "CREATE TABLE IF NOT EXISTS temp (ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT ,Added__to__list VARCHAR DEFAULT (null) ,First__connection VARCHAR DEFAULT (null),Name VARCHAR DEFAULT(null),DeviceType VARCHAR DEFAULT(null),USB_Class VARCHAR DEFAULT (null) ,USB_SubClass VARCHAR DEFAULT (null) ,USB_Protocol VARCHAR DEFAULT (null) ,VendorID VARCHAR DEFAULT (null) ,Vendor_Name VARCHAR DEFAULT (null) ,ProductID VARCHAR DEFAULT (null) ,ProductName VARCHAR DEFAULT (null) ,DeviceID VARCHAR DEFAULT (null) ,ClassGUID VARCHAR DEFAULT (null) ,HardwareID VARCHAR DEFAULT (null) ,First__connection__location VARCHAR DEFAULT (null),Last__connection__location VARCHAR DEFAULT (null) ,Serial_Number VARCHAR DEFAULT (null) ,Service VARCHAR DEFAULT (null),Checksum VARCHAR DEFAULT(null) )";
        private readonly string newTable = "(ID INTEGER PRIMARY KEY AUTOINCREMENT ,Added__to__list VARCHAR DEFAULT (null) ,First__connection VARCHAR DEFAULT (null) ,Name VARCHAR DEFAULT(null),DeviceType VARCHAR DEFAULT(null),USB_Class VARCHAR DEFAULT (null) ,USB_SubClass VARCHAR DEFAULT (null) ,USB_Protocol VARCHAR DEFAULT (null) ,VendorID VARCHAR DEFAULT (null) ,Vendor_Name VARCHAR DEFAULT (null) ,ProductID VARCHAR DEFAULT (null) ,ProductName VARCHAR DEFAULT (null) ,DeviceID VARCHAR DEFAULT (null) ,ClassGUID VARCHAR DEFAULT (null) ,HardwareID VARCHAR DEFAULT (null) ,First__connection__location VARCHAR DEFAULT (null),Last__connection__location VARCHAR DEFAULT (null) ,Serial_Number VARCHAR DEFAULT (null) ,Service VARCHAR DEFAULT (null),Checksum VARCHAR DEFAULT(null) )";

        //The different databases used
        private readonly List<string> _dbTables = new List<string>
        {
            "WhiteListDB",
            "BlackListDB",
            "InitialListDB",
            "NewStateListDB",
            "OldStateListDB",
            "DifferenceListDB",
            "ActualStateListDB",
            "TemporaryDeviceListDB"
        };

        private DataTable _sqlDataTable = new DataTable();
        private static readonly string SqLiteVersion = ";Version=3;";
        private string _databaseName = "badUSBDevicesDB.sqlite";
        private readonly string _connectionString;

        public DeviceLists()
        {
            InitializeComponent();
            _connectionString = "data source=" + Path.Combine(BadUSBFirewall.BaseDir, DatabaseName + SqLiteVersion);
        }

        public ObservableCollection<DeviceEntry> DeviceRuleList
        {
            get { return _deviceRuleList; }
            set
            {
                if (Equals(value, _deviceRuleList))
                    return;
                _deviceRuleList = value;
                OnPropertyChanged("DeviceRuleList");
            }
        }

        public ObservableCollection<DeviceEntry> GetDeviceList()
        {
            return DeviceRuleList;
        }

        /// <summary>
        /// Return the name of the SQlite-Database
        /// </summary>
        /// <param name="">Param Description</param>
        public string DatabaseName
        {
            get { return _databaseName; }
            set { _databaseName = value; }
        }

        /// <summary>
        /// Create all databases
        /// </summary>
        /// <param name="">Param Description</param>
        public void CreateDatabase()
        {
            try
            {
                //create a new sqlite database
                if (File.Exists(DatabaseName)) return;
                SQLiteConnection.CreateFile(DatabaseName);

                string querry = "CREATE TABLE IF NOT EXISTS ";
                foreach (string tableName in _dbTables)
                {
                    ExecuteQuerry(querry + tableName + newTable);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// Returns the currently activated database
        /// </summary>
        /// <param name="">Param Description</param>
        public string ActiveTable
        {
            get { return _activeTable; }
            set
            {
                if (Equals(value, _activeTable))
                    return;
                _activeTable = value;
                OnPropertyChanged("ActiveTable");
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
        /// Executes the SQL statement specified in cmdQuerry
        /// </summary>
        /// <param name="">Param Description</param>
        private void ExecuteQuerry(string cmdQuerry)
        {
            try
            {

                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                {
                    c.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(cmdQuerry, c))
                    {
                        using (SQLiteTransaction transaction = c.BeginTransaction())
                        {
                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // change to log file
            }
        }

        /// <summary>
        /// Executes the SQL statement specified by cmdQuerry for all in sqlParameters values.
        /// </summary>
        /// <param name="">Param Description</param>
        private bool ExecuteQuerry(string cmdQuerry, List<SQLiteParameter> sqlParameters)
        {
            bool result = false;
            try
            {
                if (sqlParameters.Count > 0)
                {
                    using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(c))
                        {
                            cmd.CommandText = cmdQuerry;
                            foreach (SQLiteParameter param in sqlParameters)
                            {
                                cmd.Parameters.Add(param);
                            }
                            c.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // change to log file
                return result;
            }
            finally
            {
                result = true;
            }
            return result;

        }

        /// <summary>
        /// Fill a database window with the list specified in cmdQuerry
        /// </summary>
        /// <param name="">Param Description</param>
        private void LoadData(string cmdQuerry)
        {
            using (SQLiteConnection c = new SQLiteConnection(_connectionString))
            {
                c.Open();
                using (SQLiteDataAdapter sqlAdapter = new SQLiteDataAdapter(cmdQuerry, c))
                {
                    _sqlDataTable = new DataTable(ActiveTable);
                    sqlAdapter.Fill(_sqlDataTable);

                    UsbDeviceGrid.ItemsSource = _sqlDataTable.DefaultView;

                    sqlAdapter.Update(_sqlDataTable);
                }
            }
        }

        /// <summary>
        /// Opens the database defined by srcTable and sets the
        /// corresponding switches according to the list.
        /// </summary>
        /// <param name="">Param Description</param>
        public void fill_Grid(string srcTable)
        {

            try
            {
                if (!string.IsNullOrEmpty(srcTable))
                {
                    ActiveTable = srcTable;
                    this.Title = "BadUSB-Firewall: " + ActiveTable;
                    if (srcTable == "WhiteListDB")
                    {
                        BtnAddBlackList.IsEnabled = true;
                        BtnAddWhiteList.IsEnabled = false;
                        BtnDeleteEntry.IsEnabled = true;

                    }
                    else if (srcTable == "BlackListDB")
                    {
                        BtnAddWhiteList.IsEnabled = true;
                        BtnAddBlackList.IsEnabled = false;
                        BtnDeleteEntry.IsEnabled = true;

                    }
                    else if (srcTable == "InitialListDB")
                    {
                        BtnAddBlackList.IsEnabled = false;
                        BtnAddWhiteList.IsEnabled = false;
                        BtnDeleteEntry.IsEnabled = false;
                    }
                    else if (srcTable == "DifferenceListDB")
                    {
                        BtnAddBlackList.IsEnabled = true;
                        BtnAddWhiteList.IsEnabled = true;
                        BtnDeleteEntry.IsEnabled = false;
                    }
                    else
                    {
                        BtnAddBlackList.IsEnabled = false;
                        BtnAddWhiteList.IsEnabled = false;
                        BtnDeleteEntry.IsEnabled = false;
                    }

                    string query = "select " + databaseParameters + " from " + srcTable;
                    LoadData(query);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Returns to the device corresponding to the given checksum,
        /// the date of the addition to the database stored in the database.
        /// </summary>
        /// <param name="">Param Description</param>
        public string get_DateAdded(string checksum, string srcTable)
        {
            string result = "";
            if (!string.IsNullOrEmpty(checksum) && !string.IsNullOrEmpty(srcTable))
            {
                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                {
                    c.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + srcTable, c))
                    {
                        using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                // Match found
                                if (dataReader["Checksum"].ToString() == checksum)
                                {
                                    result = dataReader["Added__to__list"].ToString();
                                    break;
                                }
                            }
                            dataReader.Close();
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks whether a device with the given checksum in the Whitelist is included.
        /// </summary>
        /// <param name="">Param Description</param>
        public static bool findIn_WhiteList(string checksum)
        {
            bool found = false;

            foreach (string item in WhiteList)
            {
                if (item == checksum)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Checks whether a device with the given checksum in the Initiallist is included.
        /// </summary>
        /// <param name="">Param Description</param>
        public static bool findIn_InitialList(string checksum)
        {
            bool found = false;

            foreach (string item in InitialList)
            {
                if (item == checksum)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Checks whether a device with the given checksum in the Blacklist is included. 
        /// </summary>
        /// <param name="">Param Description</param>
        public static bool findIn_BlackList(string checksum)
        {
            bool found = false;
            foreach (string item in BlackList)
            {
                if (item == checksum)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        /// <summary>
        /// Checks whether a device with the given checksum in the Temporary list is included.
        /// </summary>
        /// <param name="">Param Description</param>
        public static bool findIn_TemporaryDeviceList(string checksum)
        {
            bool found = false;
            foreach (USBDeviceInfo item in TemporaryDevicesList)
            {
                if (item.Checksum == checksum)
                {
                    item.FirstUsage = "No";
                    found = true;
                    break;
                }
            }
            return found;
        }

        /// <summary>
        /// Add temporary device
        /// </summary>
        /// <param name="">Param Description</param>
        public void add_TempDevice(USBDeviceInfo device)
        {
            try
            {
                TemporaryDevicesList.Add(device);
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
        public List<ChangeStateDevice> get_ChangeStateDevice(USBDeviceInfo device, string added, bool portUpdate, bool connected)
        {
            List<ChangeStateDevice> devices = new List<ChangeStateDevice>();
            List<ChangePortDevice> changePort = new List<ChangePortDevice>();
            ChangeDeviceState cdsLib = new ChangeDeviceState();

            string[] parts = device.HardwareID.Split(@" ".ToCharArray());

            if (parts.Length > 0)
            {
                var deviceHwId = parts[0];

                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                {
                    c.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM WhiteListDB", c))
                    {
                        using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                if (dataReader["Checksum"].ToString() == device.Checksum)
                                {
                                    changePort.Add(new ChangePortDevice(device.FirstLocationInformation, dataReader["ID"].ToString(), dataReader["Checksum"].ToString()));
                                    devices.Add(new ChangeStateDevice(device));
                                    break;
                                }
                            }
                            dataReader.Close();
                        }
                    }
                }
                if (changePort.Count > 0)
                {
                    using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                    {
                        c.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM WhiteListDB", c))
                        {
                            using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                            {
                                while (dataReader.Read())
                                {
                                    if ((dataReader["Checksum"].ToString() != device.Checksum) && dataReader["HardwareID"].ToString().Contains(deviceHwId) && (dataReader["Added__to__list"].ToString() == added))
                                    {
                                        string[] hwIdParts = dataReader["HardwareID"].ToString().Split(@" ".ToCharArray());
                                        if (hwIdParts.Length > 0)
                                        {
                                            string lastLocation = dataReader["Last__connection__location"].ToString();
                                            string classGuid = dataReader["ClassGUID"].ToString();
                                            string tempLocation = cdsLib.get_DevicePort(hwIdParts[0], lastLocation, classGuid);

                                            if (tempLocation != lastLocation && !string.IsNullOrEmpty(tempLocation))
                                            {
                                                changePort.Add(new ChangePortDevice(tempLocation, dataReader["ID"].ToString(), dataReader["Checksum"].ToString()));
                                                devices.Add(new ChangeStateDevice(dataReader["Name"].ToString(), hwIdParts[0], dataReader["ClassGUID"].ToString(), dataReader["DeviceID"].ToString(), dataReader["DeviceType"].ToString(), lastLocation, dataReader["VendorID"].ToString(), dataReader["ProductID"].ToString(), dataReader["Checksum"].ToString()));// device);// new ChangeStateDevice(/*dataReader["Name"].ToString(), */hwIDParts[0], dataReader["ClassGUID"].ToString(), dataReader["DeviceID"].ToString(), dataReader["DeviceType"].ToString(), tempLocation, dataReader["VendorID"].ToString(), dataReader["ProductID"].ToString(), dataReader["Checksum"].ToString()));
                                            }
                                            else
                                            {
                                                devices.Add(new ChangeStateDevice(dataReader["Name"].ToString(), hwIdParts[0], dataReader["ClassGUID"].ToString(), dataReader["DeviceID"].ToString(), dataReader["DeviceType"].ToString(), lastLocation, dataReader["VendorID"].ToString(), dataReader["ProductID"].ToString(), dataReader["Checksum"].ToString()));//  device);//new ChangeStateDevice(/*dataReader["Name"].ToString(), */hwIDParts[0], dataReader["ClassGUID"].ToString(), dataReader["DeviceID"].ToString(), dataReader["DeviceType"].ToString(), lastLocation, dataReader["VendorID"].ToString(), dataReader["ProductID"].ToString(), dataReader["Checksum"].ToString()));
                                            }
                                        }
                                    }
                                }
                                dataReader.Close();
                            }
                        }
                    }
                }

                if (portUpdate && !connected)
                {
                    foreach (ChangePortDevice item in changePort)
                    {
                        string sqlQuery = "Update WhiteListDB set Last__connection__location='" + item.mLocation + "' where ID='" + item.mID + "'";

                        ExecuteQuerry(sqlQuery);

                        var changeDevice = DeviceRuleList.SingleOrDefault(dev => dev.mChecksum == item.mChecksum);
                        if (changeDevice != null) // check item isn't null
                        {
                            changeDevice.mLastLocation = item.mLocation;
                        }
                    }
                }
            }
            int position = -1;
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].mLocationInformation.Contains("Port"))
                {
                    position = i;
                    break;
                }
            }
            // bring Device with port information in front of the list
            if (position > -1 && position != 0 && devices.Count > 1)
            {
                ChangeStateDevice tempDevice = devices[0];
                devices[0] = devices[position];
                devices[position] = tempDevice;
            }
            return devices;
        }

        /// <summary>
        /// Returns a list of the hardware id's of the Blocked Devices.
        /// Required for the detect_Blocked function in MainWindow.xaml.cs.
        /// </summary>
        /// <param name="">Param Description</param>
        public List<string> get_BlacklistHWID()
        {
            List<string> blackList = new List<string>();
            try
            {

                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                {
                    c.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM BlackListDB", c))
                    {
                        using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                blackList.Add(dataReader["HardwareID"].ToString());
                            }
                            dataReader.Close();
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return blackList;
            }
            return blackList;
        }

        /// <summary>
        /// Changes some values for a newly connected device
        ///  the values already stored in the database.
        /// </summary>
        /// <param name="">Param Description</param>
        public void get_TemporaryDevice(USBDeviceInfo device)
        {
            if (device != null)
            {
                foreach (USBDeviceInfo item in TemporaryDevicesList)
                {
                    if (item.Checksum == device.Checksum)
                    {
                        //Adjust last terminal position
                        device.LastLocationInformation = item.LastLocationInformation;
                        //Adjust the connection date
                        device.DateConnected = item.DateConnected;
                        //First use (Yes, NO)
                        device.FirstUsage = item.FirstUsage;
                        //The last port position in the database is displayed
                        //Adjust the terminal position of the device
                        item.LastLocationInformation = device.FirstLocationInformation;
                        //Device has already been used
                        item.FirstUsage = "No";

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the number of items in a list that corresponds to the specified checksum.
        /// </summary>
        /// <param name="">Param Description</param>
        private bool find_DeviceMatch(string srcTable, string checksum)
        {
            bool result = false;

            try
            {
                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                {
                    c.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + srcTable + " WHERE Checksum= '" + checksum + "' LIMIT 0,1", c))
                    {
                        cmd.ExecuteNonQuery();
                        Int32 count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count > 0)
                        {
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
            return result;

        }

        /// <summary>
        /// Deletes the contents of the database specified by srcTable.
        /// </summary>
        /// <param name="">Param Description</param>
        public void delete_Table(string srcTable)
        {
            try
            {
                if (!string.IsNullOrEmpty(srcTable))
                {
                    string sqlQuerry = "DELETE FROM " + srcTable;
                    ExecuteQuerry(sqlQuerry);
                    sqlQuerry = "DELETE FROM SQLITE_SEQUENCE WHERE NAME = '" + srcTable + "'";
                    ExecuteQuerry(sqlQuerry);
                    if (srcTable == "WhiteListDB")
                    {
                        WhiteList.Clear();

                    }
                    else if (srcTable == "BlackListDB")
                    {
                        BlackList.Clear();
                    }
                    Remove_RuleDevices(srcTable);
                }

            }
            catch (Exception ex)
            {
               // Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Displays the device information of a device selected in the database
        /// in a MessageBox.
        /// </summary>
        /// <param name="">Param Description</param>
        private void showDevice_Click(object sender, RoutedEventArgs e)
        {
            if (UsbDeviceGrid.SelectedItem == null)
                return;

            DataRowView data = (DataRowView)UsbDeviceGrid.SelectedItem;


            MessageBox.Show(
                                 "ID:" + data["ID"] + "\n" +
                                 "Name: " + data["Name"] + "\n" +
                                 "ClassGuid: " + data["ClassGuid"] + "\n" +
                                 "Date added to this list: " + data["Added__to__list"] + "\n" +
                                 "Date of first connection to this system: " + data["First__connection"] + "\n" +
                                 "DeviceID: " + data["DeviceID"] + "\n" +
                                 "DeviceType: " + data["DeviceType"] + "\n" +
                                 "HardwareIDs: " + data["HardwareID"] + "\n" +
                                 "First Location Information: " + data["First__connection__location"] + "\n" +
                                 "Last Location Information: " + data["Last__connection__location"] + "\n" +
                                 "ProductID: " + data["ProductID"] + "\n" +
                                 "Product Name: " + data["ProductName"] + "\n" +
                                 "Serial Number: " + data["Serial_Number"] + "\n" +
                                 "Service: " + data["Service"] + "\n" +
                                 "USB Class: " + data["USB_Class"] + "\n" +
                                 "USB Sub Class: " + data["USB_SubClass"] + "\n" +
                                 "USB Protocol: " + data["USB_Protocol"] + "\n" +
                                 "VendorID: " + data["VendorID"] + "\n" +
                                 "Vendor Name: " + data["Vendor_Name"] + "\n" +
                                 "Checksum: " + data["Checksum"],
                                 Title,
                                 MessageBoxButton.OK,
                                 MessageBoxImage.Information);
        }

        /// <summary>
        /// Returns the first and last port position, which is in the database
        ///is returned to a connected device.
        /// </summary>
        /// <param name="">Param Description</param>
        /// //true = firstPort, false = lastPort
        public string[] get_Port(USBDeviceInfo device, string devTable)
        {
            string[] ports = new string[2];

            try
            {
                if (device != null && (!string.IsNullOrEmpty(devTable)))
                {
                    using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                    {
                        c.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + devTable, c))
                        {
                            using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                            {
                                while (dataReader.Read())
                                {
                                    if (dataReader["Checksum"].ToString() == device.Checksum)
                                    {
                                        ports[0] = dataReader["First__connection__location"].ToString();
                                        ports[1] = dataReader["Last__connection__location"].ToString();

                                        break;
                                    }
                                }
                                dataReader.Close();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
               // Console.WriteLine(ex.Message);
                return ports;
            }

            return ports;
        }

        /// <summary>
        /// Updates the last connection point of a database device using the 
        /// first connection of the connected device.
        /// </summary>
        /// <param name="">Param Description</param>
        public void UpdatePort(USBDeviceInfo device, string devTable)
        {
            string port = "";
            string id = "";

            try
            {

                if (device != null && (!string.IsNullOrEmpty(devTable)))
                {
                    using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                    {
                        c.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + devTable, c))
                        {
                            using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                            {
                                while (dataReader.Read())
                                {
                                    if (dataReader["Checksum"].ToString() == device.Checksum)
                                    {

                                        port = dataReader["Last__connection__location"].ToString();
                                        id = dataReader["ID"].ToString();
                                        device.DateAdded = dataReader["Added__to__list"].ToString();//TODO??
                                        break;
                                    }
                                }
                                dataReader.Close();
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(port))
                    {
                        if (port != device.FirstLocationInformation)
                        {
                            string sqlQuery = "Update " + devTable + " set Last__connection__location='" + device.FirstLocationInformation + "' where ID='" + id + "'";

                            ExecuteQuerry(sqlQuery);

                            var changeDevice = DeviceRuleList.SingleOrDefault(dev => dev.mChecksum == device.Checksum);
                            if (changeDevice != null)
                            {
                                changeDevice.mLastLocation = device.FirstLocationInformation;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               // Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a selected device from the database when the "delete" button is pressed.
        /// </summary>
        /// <param name="">Param Description</param>
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {

            if (UsbDeviceGrid.SelectedItem != null)
            {
                DataRowView data = (DataRowView)UsbDeviceGrid.SelectedItem;
                if (data != null)
                {
                    string dateAdded = data["Added__to__list"].ToString();
                    string vid = data["VendorID"].ToString();
                    string pid = data["ProductID"].ToString();

                    RemoveItem(false);

                    Delete_RuleDevice(ActiveTable, vid, pid, dateAdded);
                }
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void RemoveAtPosition(string dateAdded, string vid, string pid, string dbTable)
        {
            string removeTable = !string.IsNullOrEmpty(dbTable) ? dbTable : ActiveTable;
            using (SQLiteConnection c = new SQLiteConnection(_connectionString))
            {
                c.Open();
                using (SQLiteDataAdapter sqlAdapter = new SQLiteDataAdapter("select " + databaseParameters + " from " + removeTable, c))
                {
                    _sqlDataTable = new DataTable(removeTable);
                    sqlAdapter.Fill(_sqlDataTable);
                }
            }
            MessageBoxResult decision = MessageBoxResult.Yes;

            var rowsToBeRemoved = new List<string>();

            foreach (DataRow row in _sqlDataTable.Rows)
            {
                if ((row["Added__to__list"].ToString() == dateAdded) && (row["VendorID"].ToString() == vid) && (row["ProductID"].ToString() == pid))
                {
                    if (decision == MessageBoxResult.Yes)
                    {
                        var txtSqlQuery = "";
                        if (removeTable == "WhiteListDB")
                        {
                            txtSqlQuery = "INSERT INTO BlackListDB SELECT " + addItemParameters + " FROM " + removeTable + " WHERE ID = " + row["ID"].ToString();
                            ExecuteQuerry(txtSqlQuery);
                        }
                        else if (removeTable == "BlackListDB")
                        {
                            txtSqlQuery = "INSERT INTO WhiteListDB SELECT " + addItemParameters + " FROM " + removeTable + " WHERE ID = " + row["ID"].ToString();
                            ExecuteQuerry(txtSqlQuery);
                        }
                        rowsToBeRemoved.Add(row["ID"].ToString());
                    }

                }
            }

            foreach (string deleteId in rowsToBeRemoved)
            {
                string querry = "DELETE FROM " + removeTable + " WHERE ID=" + deleteId;
                ExecuteQuerry(querry);
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void RemoveItem(bool autoMode)
        {
            try
            {
                if (UsbDeviceGrid.SelectedItem != null)
                {
                    DataRowView data = (DataRowView)UsbDeviceGrid.SelectedItem;
                    if (data != null)
                    {
                        MessageBoxResult deleteDecision = MessageBoxResult.No;
                        string checksum = data["Checksum"].ToString();
                        string dateAdded = data["Added__to__list"].ToString();
                        string vid = data["VendorID"].ToString();
                        string pid = data["ProductID"].ToString();
                        string msg = "\nInterfaces: \n";
                        bool found = false;
                        List<string> removeIndex = new List<string>();
                        MessageBoxResult decision = MessageBoxResult.Yes;


                        if ((ActiveTable != "DifferenceListDB") && (ActiveTable != "InitialListDB"))
                        {
                            using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                            {
                                c.Open();
                                using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + ActiveTable, c))
                                {
                                    using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                                    {
                                        while (dataReader.Read())
                                        {
                                            if ((dataReader["Added__to__list"].ToString() == dateAdded) && (dataReader["VendorID"].ToString() == vid) && (dataReader["ProductID"].ToString() == pid) && (dataReader["Checksum"].ToString() != checksum))
                                            {
                                                msg += "    " + dataReader["DeviceType"].ToString() + "\n";
                                                found = true;
                                            }
                                        }
                                        dataReader.Close();
                                    }
                                }
                            }
                            if (autoMode) { deleteDecision = MessageBoxResult.Yes; }
                            else
                            {
                                if (found)
                                {
                                    deleteDecision = MessageBox.Show("Delete this device inclusive the" + msg + "from this list?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                                }
                                else
                                {
                                    deleteDecision = MessageBox.Show("Delete this device from this list?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                                }
                            }

                        }
                        if (deleteDecision == MessageBoxResult.Yes)
                        {

                            //delete all items with the same date time stamp to the whitelist
                            if (data != null)
                            {
                                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                                {
                                    c.Open();
                                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + ActiveTable, c))
                                    {
                                        using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                                        {
                                            while (dataReader.Read())
                                            {

                                                if ((dataReader["Added__to__list"].ToString() == dateAdded) && (dataReader["VendorID"].ToString() == vid) && (dataReader["productID"].ToString() == pid))
                                                {
                                                    if (decision == MessageBoxResult.Yes)
                                                    {

                                                        removeIndex.Add(dataReader["ID"].ToString());
                                                        if (ActiveTable == "WhiteListDB")
                                                        {
                                                            WhiteList.Remove(dataReader["Checksum"].ToString());
                                                        }
                                                        else if (ActiveTable == "BlackListDB")
                                                        {
                                                            BlackList.Remove(dataReader["Checksum"].ToString());
                                                        }


                                                    }
                                                }
                                            }
                                            dataReader.Close();
                                        }
                                    }
                                }

                                var txtSqlQuery = "";
                                for (int i = 0; i < removeIndex.Count; i++)
                                {
                                    txtSqlQuery = "DELETE FROM " + ActiveTable + " WHERE ID = " + removeIndex[i];
                                    ExecuteQuerry(txtSqlQuery);
                                }
                                UsbDeviceGrid.ItemsSource = null;
                                txtSqlQuery = "select " + databaseParameters + " from " + ActiveTable;
                                LoadData(txtSqlQuery);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// Adds a device to Whitelist. 
        /// </summary>
        /// <param name="">Param Description</param>
        public void add_WhiteList(ChangeStateDevice device)
        {
            if (WhiteListAdded != null)
            {
                WhiteListAdded(this, new MyEventArgs(device));
            }
        }

        /// <summary>
        /// Adds a device to Blacklist.
        /// </summary>
        /// <param name="">Param Description</param>
        public void add_BlackList(ChangeStateDevice device)
        {
            if (BlackListAdded != null)
            {
                BlackListAdded(this, new MyEventArgs(device));
            }
        }

        /// <summary>
        /// The "Add to Whitelist" button has been pressed.
        /// Adds a device from the Blacklist / DifferenceList to the Whitelist.
        /// </summary>
        /// <param name="">Param Description</param>
        private void addWhiteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult decision = MessageBoxResult.Yes;
                DataRowView data = (DataRowView)UsbDeviceGrid.SelectedItem;
                if (data != null)
                {
                    string txtSQLQuery = "";

                    string dateAdded = data["Added__to__list"].ToString();
                    string vid = data["VendorID"].ToString();
                    string pid = data["ProductID"].ToString();
                    string msg = "\nInterfaces: \n";
                    string checksum = data["Checksum"].ToString();
                    bool found = false;
                    if (find_DeviceMatch("WhiteListDB", checksum))
                    {
                        MessageBox.Show("The device \nName: " + data["Name"].ToString() + "\nHardwareID: " + data["HardwareID"].ToString() + "\nDeviceType: " + data["DeviceType"].ToString() + "\nis already in the Whitelist!", Title, MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    else
                    {
                        using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                        {
                            c.Open();
                            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM BlackListDB", c))
                            {
                                using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                                {
                                    while (dataReader.Read())
                                    {
                                        if ((dataReader["Added__to__list"].ToString() == dateAdded) && (dataReader["VendorID"].ToString() == vid) && (dataReader["ProductID"].ToString() == pid) && (dataReader["Checksum"].ToString() != checksum))
                                        {
                                            msg += "    " + dataReader["DeviceType"].ToString() + "\n";
                                            found = true;
                                        }
                                    }
                                    dataReader.Close();
                                }
                            }
                        }
                        if (found)
                        {
                            decision = MessageBox.Show("Add this device inclusive the" + msg + "to the Whitelist ?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        }
                        else
                        {
                            decision = MessageBox.Show("Add this device to the Whitelist ?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        }

                        if (decision == MessageBoxResult.Yes)
                        {


                            //add all items with the same date time stamp to the whitelist
                            if ((Properties.Settings.Default.AdvancedSettings == false) && (ActiveTable != "DifferenceListDB") && (!string.IsNullOrEmpty(dateAdded)))
                            {
                                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                                {
                                    c.Open();
                                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM BlackListDB", c))
                                    {
                                        using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                                        {
                                            while (dataReader.Read())
                                            {
                                                if ((dataReader["Added__to__list"].ToString() == dateAdded) && (dataReader["VendorID"].ToString() == vid) && (dataReader["productID"].ToString() == pid))
                                                {
                                                    //Removes device from the blacklist
                                                    BlackList.Remove(dataReader["Checksum"].ToString());
                                                    //Adds the device to the whitelist
                                                    WhiteList.Add(dataReader["Checksum"].ToString());
                                                    //Adjust the administrative device management
                                                    Black_ToWhite_RuleDevice(dataReader["Checksum"].ToString());

                                                    ChangeStateDevice tempDevice = new ChangeStateDevice(dataReader["Name"].ToString(), dataReader["HardwareID"].ToString(), dataReader["ClassGUID"].ToString(), data["DeviceID"].ToString(), data["DeviceType"].ToString(), data["Last__connection__location"].ToString(), data["VendorID"].ToString(), data["ProductID"].ToString(), data["Checksum"].ToString());

                                                    add_WhiteList(tempDevice);
                                                }
                                            }
                                            dataReader.Close();
                                        }
                                    }
                                }

                                RemoveAtPosition(dateAdded, vid, pid, "");
                            }

                            // user decision what gets added to the whitelist or an item from the difference list gets added to the whitelist
                            else
                            {
                                checksum = data["Checksum"].ToString();
                                BlackList.Remove(checksum);
                                WhiteList.Add(checksum);
                                Black_ToWhite_RuleDevice(checksum);

                                ChangeStateDevice tempDevice = new ChangeStateDevice(data["Name"].ToString(), data["HardwareID"].ToString(), data["ClassGUID"].ToString(), data["DeviceID"].ToString(), data["DeviceType"].ToString(), data["Last__connection__location"].ToString(), data["VendorID"].ToString(), data["ProductID"].ToString(), data["Checksum"].ToString());

                                add_WhiteList(tempDevice);

                                if (ActiveTable == "DifferenceListDB")
                                {
                                    string actualDateTime = DateTime.Now.ToString();
                                    txtSQLQuery = "Update DifferenceListDB set Added__to__list='" + actualDateTime + "' where ID='" + data["ID"] + "'";
                                    ExecuteQuerry(txtSQLQuery);
                                }

                                txtSQLQuery = "INSERT INTO WhiteListDB SELECT " + addItemParameters + " FROM " + ActiveTable + " WHERE ID = " + data["ID"];
                                ExecuteQuerry(txtSQLQuery);
                                RemoveItem(true);

                            }

                            //Rebuild list
                            string sqlQuerry = createNewTable;
                            ExecuteQuerry(sqlQuerry);
                            string querry = "INSERT INTO temp SELECT " + addItemParameters + " FROM " + ActiveTable + " Order by ID";
                            ExecuteQuerry(querry);
                            ExecuteQuerry("drop table if exists " + ActiveTable);
                            querry = " ALTER TABLE temp RENAME TO " + ActiveTable;
                            ExecuteQuerry(querry);

                            UsbDeviceGrid.ItemsSource = null;
                            string query = "select " + databaseParameters + " from " + ActiveTable;
                            LoadData(query);
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
        /// The "Add to Blacklist" button has been pressed.
        /// Adds a device from the Whitelist to the Blacklist.
        /// </summary>
        /// <param name="">Param Description</param>
        private void addBlackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult decision = MessageBoxResult.Yes;
                DataRowView data = (DataRowView)UsbDeviceGrid.SelectedItem;
                bool found = false;

                if (data != null)
                {
                    if (find_DeviceMatch("InitialListDB", data["Checksum"].ToString()))
                    {
                        decision = MessageBoxResult.No;
                        MessageBox.Show("The device \nName: " + data["Name"].ToString() + "\nHardwareID: " + data["HardwareID"].ToString() + "\nDeviceType: " + data["DeviceType"].ToString() + "\nis also in the Initiallist(Device setup at first programm start). You cant't remove this device!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        found = true;
                    }
                    if (find_DeviceMatch("BlackListDB", data["Checksum"].ToString()) && !found)
                    {
                        MessageBox.Show("The device \nName: " + data["Name"].ToString() + "\nDeviceType: " + data["DeviceType"].ToString() + "\nis already in the Blacklist!", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                        found = true;
                    }

                    if (!found)
                    {
                        string checksum = data["Checksum"].ToString();
                        string dateAdded = data["Added__to__list"].ToString();
                        string vid = data["VendorID"].ToString();
                        string pid = data["ProductID"].ToString();
                        string msg = "\nInterfaces: \n";
                        bool ifaceFound = false;
                        using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                        {
                            c.Open();
                            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM WhiteListDB", c))
                            {
                                using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                                {
                                    while (dataReader.Read())
                                    {
                                        if ((dataReader["Added__to__list"].ToString() == dateAdded) && (dataReader["VendorID"].ToString() == vid) && (dataReader["ProductID"].ToString() == pid) && (dataReader["Checksum"].ToString() != checksum))
                                        {
                                            msg += "    " + dataReader["DeviceType"].ToString() + "\n";
                                            ifaceFound = true;
                                        }
                                    }
                                    dataReader.Close();
                                }
                            }
                        }
                        MessageBoxResult listDecision;
                        if (ifaceFound)
                        {
                            listDecision = MessageBox.Show("Add this device inclusive the" + msg + "to the Blacklist ?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        }
                        else
                        {
                            listDecision = MessageBox.Show("Add this device to the Blacklist ?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        }

                        if (listDecision == MessageBoxResult.Yes)
                        {
                            if (decision == MessageBoxResult.Yes)
                            {
                                if ((Properties.Settings.Default.AdvancedSettings == false) && (ActiveTable != "DifferenceListDB") && (!string.IsNullOrEmpty(dateAdded)))
                                {
                                    using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                                    {
                                        c.Open();
                                        using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM WhiteListDB", c))
                                        {
                                            using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                                            {
                                                while (dataReader.Read())
                                                {
                                                    if ((dataReader["Added__to__list"].ToString() == dateAdded) && (dataReader["VendorID"].ToString() == vid) && (dataReader["ProductID"].ToString() == pid))
                                                    {
                                                        ChangeStateDevice tempDevice = new ChangeStateDevice(dataReader["Name"].ToString(), dataReader["HardwareID"].ToString(), dataReader["ClassGUID"].ToString(), data["DeviceID"].ToString(), data["DeviceType"].ToString(), data["Last__connection__location"].ToString(), data["VendorID"].ToString(), data["ProductID"].ToString(), data["Checksum"].ToString());


                                                        add_BlackList(tempDevice);
                                                        White_ToBlack_RuleDevice(data["Checksum"].ToString());
                                                        WhiteList.Remove(dataReader["Checksum"].ToString());
                                                        BlackList.Add(dataReader["Checksum"].ToString());
                                                    }
                                                }
                                                dataReader.Close();
                                            }
                                        }
                                    }

                                    RemoveAtPosition(dateAdded, vid, pid, "");
                                }
                                //user decision what gets added to the blacklist or an item from the differencelist gets added to the blacklist
                                else
                                {
                                    //
                                    checksum = data["Checksum"].ToString();
                                    BlackList.Add(checksum);

                                    ChangeStateDevice tempDevice = new ChangeStateDevice(data["Name"].ToString(), data["HardwareID"].ToString(), data["ClassGUID"].ToString(), data["DeviceID"].ToString(), data["DeviceType"].ToString(), data["Last__connection__location"].ToString(), data["VendorID"].ToString(), data["ProductID"].ToString(), data["Checksum"].ToString());
                                    add_BlackList(tempDevice);

                                    var txtSqlQuery = "";
                                    if (ActiveTable == "DifferenceListDB")
                                    {
                                        string actualDateTime = DateTime.Now.ToString();
                                        txtSqlQuery = "Update DifferenceListDB set Added__to__list='" + actualDateTime + "' where ID='" + data["ID"] + "'";
                                        ExecuteQuerry(txtSqlQuery);
                                    }


                                    txtSqlQuery = "INSERT INTO BlackListDB SELECT " + addItemParameters + " FROM " + ActiveTable + " WHERE ID = " + data["ID"];
                                    ExecuteQuerry(txtSqlQuery);
                                    RemoveItem(true);
                                }

                                //rebuild table because of id problems
                                string sqlQuerry = createNewTable;
                                ExecuteQuerry(sqlQuerry);
                                string querry = "INSERT INTO temp SELECT " + addItemParameters + " FROM " + ActiveTable + " Order by ID";
                                ExecuteQuerry(querry);
                                ExecuteQuerry("drop table if exists " + ActiveTable);
                                querry = " ALTER TABLE temp RENAME TO " + ActiveTable;
                                ExecuteQuerry(querry);
                                LoadData("select " + databaseParameters + " from " + ActiveTable);
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
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //disable closing, to reuse the same window in the main class
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Creates the device difference lists using the
        /// Oldsatate and Newstate lists.
        /// </summary>
        /// <param name="">Param Description</param>
        public int CreateDifferenceList(string oldState, string newState)
        {
            int count = 0;
            List<USBDeviceInfo> oldList = new List<USBDeviceInfo>();
            List<USBDeviceInfo> newList = new List<USBDeviceInfo>();

            try
            {
                delete_Table("DifferenceListDB");

                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                {
                    c.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + oldState + " ", c))
                    {
                        using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                USBDeviceInfo temp = new USBDeviceInfo();
                                temp.DateAdded = dataReader["Added__to__list"].ToString();
                                temp.DateConnected = dataReader["First__connection"].ToString();
                                temp.Name = dataReader["Name"].ToString();
                                temp.DeviceType = dataReader["DeviceType"].ToString();
                                temp.Description = "";
                                temp.USB_Class = dataReader["USB_Class"].ToString();
                                temp.USB_SubClass = dataReader["USB_SubClass"].ToString();
                                temp.USB_Protocol = dataReader["USB_Protocol"].ToString();
                                temp.VendorID = dataReader["VendorID"].ToString();
                                temp.VendorName = dataReader["Vendor_Name"].ToString();
                                temp.ProductID = dataReader["ProductID"].ToString();
                                temp.ProductName = dataReader["ProductName"].ToString();
                                temp.DeviceID = dataReader["DeviceID"].ToString();
                                temp.ClassGuid = dataReader["ClassGUID"].ToString();
                                temp.CompatibleID = "";
                                temp.HardwareID = dataReader["HardwareID"].ToString();
                                temp.FirstLocationInformation = dataReader["First__connection__location"].ToString();

                                temp.LastLocationInformation = dataReader["Last__connection__location"].ToString();
                                temp.Manufacturer = "";
                                temp.SerialNumber = dataReader["Serial_Number"].ToString();
                                temp.Service = dataReader["Service"].ToString();
                                temp.Status = "";
                                temp.Checksum = dataReader["Checksum"].ToString();

                                oldList.Add(temp);
                            }
                            dataReader.Close();
                        }
                    }
                }
                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                {
                    c.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + newState + " ", c))
                    {
                        using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                USBDeviceInfo temp = new USBDeviceInfo();
                                temp.DateAdded = dataReader["Added__to__list"].ToString();
                                temp.DateConnected = dataReader["First__connection"].ToString();
                                temp.Name = dataReader["Name"].ToString();
                                temp.DeviceType = dataReader["DeviceType"].ToString();
                                temp.Description = "";
                                temp.USB_Class = dataReader["USB_Class"].ToString();
                                temp.USB_SubClass = dataReader["USB_SubClass"].ToString();
                                temp.USB_Protocol = dataReader["USB_Protocol"].ToString();
                                temp.VendorID = dataReader["VendorID"].ToString();
                                temp.VendorName = dataReader["Vendor_Name"].ToString();
                                temp.ProductID = dataReader["ProductID"].ToString();
                                temp.ProductName = dataReader["ProductName"].ToString();
                                temp.DeviceID = dataReader["DeviceID"].ToString();
                                temp.ClassGuid = dataReader["ClassGUID"].ToString();
                                temp.CompatibleID = "";
                                temp.HardwareID = dataReader["HardwareID"].ToString();
                                temp.FirstLocationInformation = dataReader["First__connection__location"].ToString();

                                temp.LastLocationInformation = dataReader["Last__connection__location"].ToString();
                                temp.Manufacturer = "";
                                temp.SerialNumber = dataReader["Serial_Number"].ToString();
                                temp.Service = dataReader["Service"].ToString();
                                temp.Status = "";
                                temp.Checksum = dataReader["Checksum"].ToString();

                                newList.Add(temp);
                            }
                            dataReader.Close();
                        }
                    }
                }

                //Only add discrepancies between the two lists of the differencelist
                var diffList = newList.Where(o => !oldList.Contains(o)).ToList();

                foreach (var item in diffList)
                {
                    string querry = "INSERT INTO DifferenceListDB " + addItem;
                    List<SQLiteParameter> sqlParameters = new List<SQLiteParameter>();

                    sqlParameters.Add(new SQLiteParameter("@ID", null));
                    sqlParameters.Add(new SQLiteParameter("@Name", item.Name));
                    sqlParameters.Add(new SQLiteParameter("@DeviceType", item.DeviceType));
                    sqlParameters.Add(new SQLiteParameter("@ClassGUID", item.ClassGuid));
                    sqlParameters.Add(new SQLiteParameter("@Added__to__list", item.DateAdded));
                    sqlParameters.Add(new SQLiteParameter("@First__connection", item.DateConnected));
                    sqlParameters.Add(new SQLiteParameter("@DeviceID", item.DeviceID));
                    sqlParameters.Add(new SQLiteParameter("@HardwareID", item.HardwareID));
                    sqlParameters.Add(new SQLiteParameter("@First__connection__location", item.FirstLocationInformation));
                    sqlParameters.Add(new SQLiteParameter("@Last__connection__location", item.LastLocationInformation));
                    sqlParameters.Add(new SQLiteParameter("@ProductID", item.ProductID));
                    sqlParameters.Add(new SQLiteParameter("@ProductName", item.ProductName));
                    sqlParameters.Add(new SQLiteParameter("@Serial_Number", item.SerialNumber));
                    sqlParameters.Add(new SQLiteParameter("@Service", item.Service));
                    sqlParameters.Add(new SQLiteParameter("@USB_Class", item.USB_Class));
                    sqlParameters.Add(new SQLiteParameter("@USB_SubClass", item.USB_SubClass));
                    sqlParameters.Add(new SQLiteParameter("@USB_Protocol", item.USB_Protocol));
                    sqlParameters.Add(new SQLiteParameter("@VendorID", item.VendorID));
                    sqlParameters.Add(new SQLiteParameter("@Vendor_Name", item.VendorName));
                    sqlParameters.Add(new SQLiteParameter("@Checksum", item.Checksum));

                    var result = ExecuteQuerry(querry, sqlParameters);
                    DiffList.Add(item.Checksum);
                    if (result == false) { break; }
                }


                count = diffList.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return count;
            }
            return count;
        }

        /// <summary>
        /// Check whether a device searched by checksum is included in the difference list.
        /// </summary>
        /// <param name="">Param Description</param>
        public bool Find_InDiffList(string checksum)
        {
            bool result = DiffList.Contains(checksum);
            return result;
        }

        /// <summary>
        /// Creation of th whitelist
        /// </summary>
        /// <param name="">Param Description</param>
        private void build_WhiteList()
        {
            try
            {
                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                {
                    c.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM WhiteListDB", c))
                    {
                        using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                WhiteList.Add(dataReader["Checksum"].ToString());
                            }
                            dataReader.Close();
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
        /// Creation of the blacklist
        /// </summary>
        /// <param name="">Param Description</param>
        private void build_BlackList()
        {
            try
            {
                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                {
                    c.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM BlackListDB", c))
                    {
                        using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                BlackList.Add(dataReader["Checksum"].ToString());
                            }
                            dataReader.Close();
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
        /// Creation of the initiallist
        /// </summary>
        /// <param name="">Param Description</param>
        private void build_InitialList()
        {
            try
            {
                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                {
                    c.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM InitialListDB", c))
                    {
                        using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                InitialList.Add(dataReader["Checksum"].ToString());
                            }
                            dataReader.Close();
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
        /// Add detected deviating devices from the difference list of the temporary list.
        /// </summary>
        /// <param name="">Param Description</param>
        public void add_DiffToTemp()
        {
            try
            {
                string querry = "INSERT INTO TemporaryDeviceListDB SELECT " + compareParameters + " FROM DifferenceListDB EXCEPT SELECT " + compareParameters + " FROM TemporaryDeviceListDB";

                ExecuteQuerry(querry);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Creation of the temporarylist
        /// </summary>
        /// <param name="">Param Description</param>
        public void build_TemporaryDevicesList()
        {
            try
            {
                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                {
                    c.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM TemporaryDeviceListDB", c))
                    {
                        using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                USBDeviceInfo temp =
                                    new USBDeviceInfo
                                    {
                                        DateAdded = dataReader["Added__to__list"].ToString(),
                                        DateConnected = dataReader["First__connection"].ToString(),
                                        Name = dataReader["Name"].ToString(),
                                        DeviceType = dataReader["DeviceType"].ToString(),
                                        Description = "",
                                        USB_Class = dataReader["USB_Class"].ToString(),
                                        USB_SubClass = dataReader["USB_SubClass"].ToString(),
                                        USB_Protocol = dataReader["USB_Protocol"].ToString(),
                                        VendorID = dataReader["VendorID"].ToString(),
                                        VendorName = dataReader["Vendor_Name"].ToString(),
                                        ProductID = dataReader["ProductID"].ToString(),
                                        ProductName = dataReader["ProductName"].ToString(),
                                        DeviceID = dataReader["DeviceID"].ToString(),
                                        ClassGuid = dataReader["ClassGUID"].ToString(),
                                        CompatibleID = "",
                                        HardwareID = dataReader["HardwareID"].ToString(),
                                        FirstLocationInformation = dataReader["First__connection__location"].ToString(),
                                        LastLocationInformation = dataReader["Last__connection__location"].ToString(),
                                        Manufacturer = "",
                                        SerialNumber = dataReader["Serial_Number"].ToString(),
                                        Service = dataReader["Service"].ToString(),
                                        Status = "",
                                        Checksum = dataReader["Checksum"].ToString()
                                    };


                                TemporaryDevicesList.Add(temp);
                            }
                            dataReader.Close();
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
        /// Adds devices from the difference list to the temporary list
        /// </summary>
        /// <param name="">Param Description</param>
        public void add_Differences(List<USBDeviceInfo> tempDevices)
        {
            try
            {
                if (tempDevices != null)
                {
                    using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                    {
                        c.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM DifferenceListDB", c))
                        {
                            using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                            {
                                while (dataReader.Read())
                                {
                                    USBDeviceInfo temp =
                                        new USBDeviceInfo
                                        {
                                            DateAdded = dataReader["Added__to__list"].ToString(),
                                            DateConnected = dataReader["First__connection"].ToString(),
                                            Name = dataReader["Name"].ToString(),
                                            DeviceType = dataReader["DeviceType"].ToString(),
                                            Description = "",
                                            USB_Class = dataReader["USB_Class"].ToString(),
                                            USB_SubClass = dataReader["USB_SubClass"].ToString(),
                                            USB_Protocol = dataReader["USB_Protocol"].ToString(),
                                            VendorID = dataReader["VendorID"].ToString(),
                                            VendorName = dataReader["Vendor_Name"].ToString(),
                                            ProductID = dataReader["ProductID"].ToString(),
                                            ProductName = dataReader["ProductName"].ToString(),
                                            DeviceID = dataReader["DeviceID"].ToString(),
                                            ClassGuid = dataReader["ClassGUID"].ToString(),
                                            CompatibleID = "",
                                            HardwareID = dataReader["HardwareID"].ToString(),
                                            FirstLocationInformation =
                                                dataReader["First__connection__location"].ToString(),
                                            LastLocationInformation =
                                                dataReader["Last__connection__location"].ToString(),
                                            Manufacturer = "",
                                            SerialNumber = dataReader["Serial_Number"].ToString(),
                                            Service = dataReader["Service"].ToString(),
                                            Status = "",
                                            Checksum = dataReader["Checksum"].ToString()
                                        };

                                    tempDevices.Add(temp);
                                }
                                dataReader.Close();
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
        /// Returns the number of devices stored in the Blacklist
        /// </summary>
        /// <param name="">Param Description</param>
        public int get_BlacklistCount()
        {
            return BlackList.Count;
        }

        /// <summary>
        /// Copies the stored SQLite databases to the temporary lists at program startup.
        /// </summary>
        /// <param name="">Param Description</param>
        public void build_Lists()
        {
            build_WhiteList();
            build_BlackList();
            build_InitialList();
            create_DeviceRuleList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="">Param Description</param>
        public void delete_TempList()
        {
            TemporaryDevicesList.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="">Param Description</param>
        public void copy_DevicesToTemp()
        {
            try
            {
                string sqlQuerry = "INSERT INTO TemporaryDeviceListDB SELECT " + addItemParameters + " FROM InitialListDB Order by ID";
                ExecuteQuerry(sqlQuerry);

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
        public void addTempDevice_ToTable()
        {
            try
            {
                delete_Table("TemporaryDeviceListDB");
                foreach (USBDeviceInfo item in TemporaryDevicesList)
                {
                    add_DataItem("TemporaryDeviceListDB", item);
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
        public void removeTempDevice(USBDeviceInfo device)
        {
            try
            {
                TemporaryDevicesList.Remove(device);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// Adds a USB device object handled by the user to a destination device specified
        /// by destinationTable database.
        /// </summary>
        /// <param name="">Param Description</param>
        public bool add_DataItem(string destinationTable, USBDeviceInfo item)
        {
            bool result = false;
            try
            {
                string querry = "INSERT INTO " + destinationTable + " " + addItem;
                List<SQLiteParameter> sqlParameters = new List<SQLiteParameter>
                {
                    new SQLiteParameter("@ID", null),
                    new SQLiteParameter("@Name", item.Name),
                    new SQLiteParameter("@DeviceType", item.DeviceType),
                    new SQLiteParameter("@ClassGUID", item.ClassGuid),
                    new SQLiteParameter("@Added__to__list", item.DateAdded),
                    new SQLiteParameter("@First__connection", item.DateConnected),
                    new SQLiteParameter("@DeviceID", item.DeviceID),
                    new SQLiteParameter("@HardwareID", item.HardwareID),
                    new SQLiteParameter("@First__connection__location", item.FirstLocationInformation),
                    new SQLiteParameter("@Last__connection__location", item.LastLocationInformation),
                    new SQLiteParameter("@ProductID", item.ProductID),
                    new SQLiteParameter("@ProductName", item.ProductName),
                    new SQLiteParameter("@Serial_Number", item.SerialNumber),
                    new SQLiteParameter("@Service", item.Service),
                    new SQLiteParameter("@USB_Class", item.USB_Class),
                    new SQLiteParameter("@USB_SubClass", item.USB_SubClass),
                    new SQLiteParameter("@USB_Protocol", item.USB_Protocol),
                    new SQLiteParameter("@VendorID", item.VendorID),
                    new SQLiteParameter("@Vendor_Name", item.VendorName),
                    new SQLiteParameter("@Checksum", item.Checksum)
                };


                result = ExecuteQuerry(querry, sqlParameters);

                if (result)
                {
                    if (destinationTable == "WhiteListDB")
                    {
                        DeviceRuleList.Add(new DeviceEntry(item.Name, item.Checksum, item.ClassGuid, item.DateAdded, item.DeviceType, item.DeviceID, item.HardwareID, item.LastLocationInformation, item.ProductID, item.ProductName, item.SerialNumber, item.Service,
                            item.USB_Class, item.USB_Protocol, item.USB_SubClass, item.VendorID, item.VendorName, item.FirstLocationInformation, false, true));
                        WhiteList.Add(item.Checksum);
                    }
                    else if (destinationTable == "BlackListDB")
                    {
                        DeviceRuleList.Add(new DeviceEntry(item.Name, item.Checksum, item.ClassGuid, item.DateAdded, item.DeviceType, item.DeviceID, item.HardwareID, item.LastLocationInformation, item.ProductID, item.ProductName, item.SerialNumber, item.Service,
                      item.USB_Class, item.USB_Protocol, item.USB_SubClass, item.VendorID, item.VendorName, item.FirstLocationInformation, true, false));
                        BlackList.Add(item.Checksum);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return result;
            }
            return result;
        }


        /// <summary>
        /// Returns the number of interfaces of a device using the
        /// transferred HardwareID.
        /// </summary>
        /// <param name="">Param Description</param>
        public uint get_Interfaces(string HardwareID, string dbList)
        {
            uint cnt = 0;
            try
            {
                using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                {
                    c.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + dbList, c))
                    {
                        using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                string hwId = dataReader["HardwareID"].ToString();

                                if (hwId.Contains(HardwareID) && hwId != HardwareID)
                                {
                                    cnt++;
                                }

                            }
                            dataReader.Close();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;

            }
            return cnt;
        }


        #region FirewallRules
        /// <summary>
        /// "Apply_changes" button in Administrative Manager Management has been pressed.
        /// </summary>
        /// <param name="">Param Description</param>
        private void apply_ChangePressed(object sender, ApplyChangesPressedArgs e)
        {
            if (e.Pressed)
            {
                List<RemoveRuleDevice> listDeviceRemove = new List<RemoveRuleDevice>();
                var listRemoveposition = new List<KeyValuePair<string, int>>();

                //Go through all devices of the administrative device management and check if there
                //is a change in this entry was carried out.
                for (int i = DeviceRuleList.Count - 1; i > -1; i--)
                {
                    //Device has been blocked (added to Blacklist) and is currently still authorized (in the Whitelist)
                    if (DeviceRuleList[i].Added_BlackList && DeviceRuleList[i].In_WhiteList)
                    {
                        //emove the device from the Blacklist and add it to the Whitelist
                        using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                        {
                            c.Open();
                            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM BlackListDB", c))
                            {
                                using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                                {
                                    while (dataReader.Read())
                                    {
                                        if ((dataReader["Added__to__list"].ToString() == DeviceRuleList[i].mDateAdded) && (dataReader["VendorID"].ToString() == DeviceRuleList[i].mVendorID) && (dataReader["ProductID"].ToString() == DeviceRuleList[i].mProductID) && (dataReader["Checksum"].ToString() == DeviceRuleList[i].mChecksum))
                                        {
                                            BlackList.Remove(dataReader["Checksum"].ToString());
                                            WhiteList.Add(dataReader["Checksum"].ToString());
                                            Black_ToWhite_RuleDevice(dataReader["Checksum"].ToString());
                                            ChangeStateDevice tempDevice = new ChangeStateDevice(dataReader["Name"].ToString(), dataReader["HardwareID"].ToString(), dataReader["ClassGUID"].ToString(), dataReader["DeviceID"].ToString(), dataReader["DeviceType"].ToString(), dataReader["Last__connection__location"].ToString(), dataReader["VendorID"].ToString(), dataReader["ProductID"].ToString(), dataReader["Checksum"].ToString());

                                            add_WhiteList(tempDevice);
                                        }
                                    }
                                    dataReader.Close();
                                }
                            }
                        }
                        listRemoveposition.Add(new KeyValuePair<string, int>("BlackListDB", i));
                    }
                    //Device has been removed and is currently still in the Blacklist
                    else if (DeviceRuleList[i].Added_BlackList && DeviceRuleList[i].Remove_Device)
                    {
                        //Remove device from the blacklist
                        listDeviceRemove.Add(new RemoveRuleDevice(DeviceRuleList[i].mChecksum, DeviceRuleList[i].mDateAdded, DeviceRuleList[i].mVendorID, DeviceRuleList[i].mProductID, DeviceRuleList[i].mHardwareID, "BlackListDB"));
                    }
                    //Remove device from the blacklist and add it into the whitelist
                    else if (DeviceRuleList[i].Added_WhiteList && DeviceRuleList[i].In_BlackList)
                    {
                        using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                        {
                            c.Open();
                            using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM WhiteListDB", c))
                            {
                                using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                                {
                                    while (dataReader.Read())
                                    {
                                        if ((dataReader["Added__to__list"].ToString() == DeviceRuleList[i].mDateAdded) && (dataReader["VendorID"].ToString() == DeviceRuleList[i].mVendorID) && (dataReader["ProductID"].ToString() == DeviceRuleList[i].mProductID) && (dataReader["Checksum"].ToString() == DeviceRuleList[i].mChecksum))
                                        {
                                            ChangeStateDevice tempDevice = new ChangeStateDevice(dataReader["Name"].ToString(), dataReader["HardwareID"].ToString(), dataReader["ClassGUID"].ToString(), dataReader["DeviceID"].ToString(), dataReader["DeviceType"].ToString(), dataReader["Last__connection__location"].ToString(), dataReader["VendorID"].ToString(), dataReader["ProductID"].ToString(), dataReader["Checksum"].ToString());

                                            add_BlackList(tempDevice);
                                            White_ToBlack_RuleDevice(DeviceRuleList[i].mChecksum);
                                            WhiteList.Remove(dataReader["Checksum"].ToString());
                                            BlackList.Add(dataReader["Checksum"].ToString());
                                        }
                                    }
                                    dataReader.Close();
                                }
                            }
                        }
                        listRemoveposition.Add(new KeyValuePair<string, int>("WhiteListDB", i));
                    }
                    //Device from the whitelist was removed
                    else if (DeviceRuleList[i].Added_WhiteList && DeviceRuleList[i].Remove_Device)
                    {
                        //remove device from the whitelist (delete button click)
                        listDeviceRemove.Add(new RemoveRuleDevice(DeviceRuleList[i].mChecksum, DeviceRuleList[i].mDateAdded, DeviceRuleList[i].mVendorID, DeviceRuleList[i].mProductID, DeviceRuleList[i].mHardwareID, "WhiteListDB"));
                    }
                }

                foreach (var item in listRemoveposition)
                {

                    RemoveAtPosition(DeviceRuleList[item.Value].mDateAdded, DeviceRuleList[item.Value].mVendorID, DeviceRuleList[item.Value].mProductID, item.Key);
                }
                foreach (RemoveRuleDevice item in listDeviceRemove)
                {
                    Remove_RuleDevice(item.RemoveTable, item.Checksum);
                    Delete_RuleTable(item);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="">Param Description</param>
        private void Delete_RuleTable(RemoveRuleDevice device)
        {
            List<string> removeIndex = new List<string>();

            using (SQLiteConnection c = new SQLiteConnection(_connectionString))
            {
                c.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + device.RemoveTable, c))
                {
                    using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {

                            if ((dataReader["Added__to__list"].ToString() == device.DateAdded) && (dataReader["VendorID"].ToString() == device.VendorID) && (dataReader["productID"].ToString() == device.ProductID))
                            {
                                removeIndex.Add(dataReader["ID"].ToString());
                                if (device.RemoveTable == "WhiteListDB")
                                {
                                    WhiteList.Remove(dataReader["Checksum"].ToString());
                                }
                                else if (device.RemoveTable == "BlackListDB")
                                {
                                    BlackList.Remove(dataReader["Checksum"].ToString());
                                }
                            }
                        }
                        dataReader.Close();
                    }
                }
            }

            for (int i = 0; i < removeIndex.Count; i++)
            {
                var txtSqlQuery = "DELETE FROM " + device.RemoveTable + " WHERE ID = " + removeIndex[i];
                ExecuteQuerry(txtSqlQuery);
            }
        }
        /// <summary>
        /// The device from Blacklist was changed to the device of the Whitelist
        /// </summary>
        /// <param name="">Param Description</param>
        private void Black_ToWhite_RuleDevice(string checksum)
        {
            for (int i = 0; i < DeviceRuleList.Count; i++)
            {
                if (DeviceRuleList[i].mChecksum == checksum)
                {
                    DeviceRuleList[i].In_BlackList = false;
                    DeviceRuleList[i].Added_BlackList = false;
                    DeviceRuleList[i].In_WhiteList = true;
                    DeviceRuleList[i].Added_WhiteList = true;
                    break;
                }
            }
        }
        /// <summary>
        /// The device from Whitelist has been changed to Blacklist
        /// </summary>
        /// <param name="">Param Description</param>
        private void White_ToBlack_RuleDevice(string checksum)
        {
            for (int i = 0; i < DeviceRuleList.Count; i++)
            {
                if (DeviceRuleList[i].mChecksum == checksum)
                {
                    DeviceRuleList[i].In_BlackList = true;
                    DeviceRuleList[i].Added_BlackList = true;
                    DeviceRuleList[i].In_WhiteList = false;
                    DeviceRuleList[i].Added_WhiteList = false;
                    break;
                }
            }
        }

        /// <summary>
        /// Remove a device from the administration
        /// </summary>
        /// <param name="">Param Description</param>
        private void Delete_RuleDevice(string srcTable, string vID, string pID, string dateAdded)
        {

            for (int i = DeviceRuleList.Count - 1; i > -1; i--)
            {
                if (DeviceRuleList[i].mDateAdded == dateAdded && DeviceRuleList[i].mVendorID == vID && DeviceRuleList[i].mProductID == pID)
                {

                    if (srcTable == "BlackListDB")
                    {
                        if (DeviceRuleList[i].Added_BlackList)
                        {
                            DeviceRuleList.RemoveAt(i);
                        }
                    }
                    else if (srcTable == "WhiteListDB")
                    {
                        if (DeviceRuleList[i].Added_WhiteList)
                        {
                            DeviceRuleList.RemoveAt(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="">Param Description</param>
        private void Remove_RuleDevice(string srcTable, string checksum)
        {
            if (srcTable == "BlackListDB")
            {
                DeviceRuleList.Remove(DeviceRuleList.Single(s => s.Added_BlackList && s.mChecksum == checksum));
            }
            else if (srcTable == "WhiteListDB")
            {
                DeviceRuleList.Remove(DeviceRuleList.Single(s => s.Added_WhiteList && s.mChecksum == checksum));
            }
        }

        /// <summary>
        /// Delete the administrative device list
        /// </summary>
        /// <param name="">Param Description</param>
        private void Remove_RuleDevices(string srcTable)
        {

            if (DeviceRuleList.Count > 0)
            {
                for (int i = DeviceRuleList.Count - 1; i > -1; i--)
                {
                    if (srcTable == "BlackListDB")
                    {
                        if (DeviceRuleList[i].Added_BlackList)
                        {
                            DeviceRuleList.RemoveAt(i);
                        }
                    }
                    else if (srcTable == "WhiteListDB")
                    {
                        if (DeviceRuleList[i].Added_WhiteList)
                        {
                            DeviceRuleList.RemoveAt(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Open the Device Management window
        /// </summary>
        /// <param name="">Param Description</param>
        public void show_FirewallRules()
        {
            FirewallRules rulesWindow = new FirewallRules();
            rulesWindow.EventApplyChangesPressed += apply_ChangePressed;


            Dispatcher.Invoke(() => rulesWindow.ShowDialog());
        }

        /// <summary>
        /// Generate the administrative device list
        /// the Blacklist and Whitelist
        /// </summary>
        /// <param name="">Param Description</param>
        public void create_DeviceRuleList()
        {
            try
            {
                //blacklist
                string listDb = "BlackListDB";
                for (int i = 0; i < 2; i++)
                {
                    using (SQLiteConnection c = new SQLiteConnection(_connectionString))
                    {
                        c.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM " + listDb, c))
                        {
                            using (SQLiteDataReader dataReader = cmd.ExecuteReader())
                            {
                                while (dataReader.Read())
                                {
                                    DeviceEntry tempDevice = new DeviceEntry();
                                    if (listDb == "BlackListDB")
                                    {
                                        tempDevice.Added_BlackList = true;
                                        tempDevice.Added_WhiteList = false;
                                        tempDevice.In_BlackList = true;
                                        tempDevice.In_WhiteList = false;
                                    }
                                    else if (listDb == "WhiteListDB")
                                    {
                                        tempDevice.Added_BlackList = false;
                                        tempDevice.Added_WhiteList = true;
                                        tempDevice.In_BlackList = false;
                                        tempDevice.In_WhiteList = true;
                                    }

                                    tempDevice.Remove_Device = false;
                                    tempDevice.mName = dataReader["Name"].ToString();
                                    tempDevice.mChecksum = dataReader["Checksum"].ToString();
                                    tempDevice.mClassGuid = dataReader["ClassGUID"].ToString();
                                    tempDevice.mDateAdded = dataReader["Added__to__list"].ToString();
                                    tempDevice.mFirstLocation = dataReader["First__connection__location"].ToString();

                                    tempDevice.mHardwareID = "";

                                    string[] hwIdParts = dataReader["HardwareID"].ToString().Split(@" ".ToCharArray());

                                    if (hwIdParts.Length > 0)
                                    {
                                        tempDevice.mHardwareID = hwIdParts[0];
                                    }

                                    if (tempDevice.mFirstLocation.ToUpper().Contains("PORT"))
                                    {
                                        tempDevice.mDeviceType = dataReader["DeviceType"].ToString();
                                        tempDevice.CheckBox_Enabled = true;
                                    }
                                    else
                                    {
                                        tempDevice.mDeviceType = "    -> " + dataReader["DeviceType"].ToString();
                                        tempDevice.CheckBox_Enabled = false;
                                    }

                                    tempDevice.mProductID = dataReader["ProductID"].ToString();
                                    tempDevice.mProductName = dataReader["ProductName"].ToString();
                                    tempDevice.mSerialNumber = dataReader["Serial_Number"].ToString();
                                    tempDevice.mService = dataReader["Service"].ToString();
                                    tempDevice.mUSB_Class = dataReader["USB_Class"].ToString();
                                    tempDevice.mUSB_Protocol = dataReader["USB_Protocol"].ToString();
                                    tempDevice.mUSB_SubClass = dataReader["USB_SubClass"].ToString();
                                    tempDevice.mVendorID = dataReader["VendorID"].ToString();
                                    tempDevice.mVendorName = dataReader["Vendor_Name"].ToString();

                                    DeviceRuleList.Add(tempDevice);
                                }
                                dataReader.Close();
                            }
                        }
                    }
                    listDb = "WhiteListDB";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion


    }
}


