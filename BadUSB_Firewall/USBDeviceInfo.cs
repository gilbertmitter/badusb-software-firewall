/**
******************************************************************************
* @file	   USBDeviceInfo.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Represents a USB object.
******************************************************************************
*/
using System;
using System.Text;
using System.Management;
using Microsoft.Win32;
using System.Security.Cryptography;

namespace BadUSB_Firewall
{

    /// <summary>
    /// Description
    /// </summary>
    /// <param name="">Param Description</param>
    public class USBDeviceInfo : IEquatable<USBDeviceInfo>
    {
        //Creates an empty USBDeviceInfo object and assigns the values for device detection.
        public USBDeviceInfo()
        {
            Checksum = "";
            ClassGuid = "";                 //Guid for the device's setup class
            CompatibleID = "";              //ID string(s) containign the device's class and (optional) subclass and protocol
            Description = "";
            DateAdded = "";
            DateConnected = "";
            DeviceID = "";
            DeviceType = "";
            HardwareID = "";                //ID string containing the device's Vendor ID and Product ID
            FirstUsage = "Yes";
            FirstLocationInformation = "";  //USB device or iProduct string
            LastLocationInformation = "";   //USB device or iProduct string
            Manufacturer = "";              //device manufacturer
            Name = "";
            ProductID = "";
            ProductName = "";
            SerialNumber = "";
            Service = "";                   //Name of the device's Service key
            Status = "";
            USB_Class = "";
            USB_SubClass = "";
            USB_Protocol = "";
            VendorID = "";
            VendorName = "";
        }


        /// <summary>
        /// Obtain a USBDeviceInfo object from a ManaementBase object (obtained through a WMI query).
        /// </summary>
        /// <param name="">Param Description</param>
        public USBDeviceInfo(ManagementBaseObject device)
        {
            try
            {
                ClassGuid = device.Properties["ClassGuid"].Value == null ? "" : device.Properties["ClassGuid"].Value.ToString();
                if (device.Properties["CompatibleID"].Value != null)
                {
                    string[] value = (string[])device.Properties["CompatibleID"].Value;
                    if (value == null) { CompatibleID = ""; }
                    else
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            CompatibleID += value[i];
                            if (i < value.Length - 1)
                            {
                                CompatibleID += " ";
                            }
                        }
                    }

                }
                else
                {
                    CompatibleID = "";
                }

                DateConnected = DateTime.Now.ToString();
                DateAdded = "";
                Description = device.Properties["Description"].Value == null ? "" : device.Properties["Description"].Value.ToString();

                DeviceID = device.Properties["DeviceID"].Value == null ? "" : device.Properties["DeviceID"].Value.ToString();

                if (device.Properties["HardwareID"].Value != null)
                {
                    string[] value = (string[])device.Properties["HardwareID"].Value;
                    if (value == null)
                    {
                        HardwareID = "";
                    }
                    else
                    {
                        if (value.Length > 0)
                        {
                            HardwareID = value[0];
                        }
                        else
                        {
                            HardwareID = "";
                        }
                    }

                }
                else
                {
                    HardwareID = "";
                }

                Manufacturer = device.Properties["Manufacturer"].Value == null ? "" : device.Properties["Manufacturer"].Value.ToString();
                Name = device.Properties["Name"].Value == null ? "" : device.Properties["Name"].Value.ToString();

                Service = device.Properties["Service"].Value == null ? "" : device.Properties["Service"].Value.ToString();
                Status = device.Properties["Status"].Value == null ? "" : device.Properties["Status"].Value.ToString();
                FirstLocationInformation = GetDeviceLocation(DeviceID);
                FirstUsage = "Yes";
                LastLocationInformation = FirstLocationInformation;
                USB_Class = "";
                USB_SubClass = "";
                USB_Protocol = "";
                if (!string.IsNullOrEmpty(CompatibleID))
                {
                    int classStart = CompatibleID.IndexOf("Class_");
                    if (classStart > -1)
                    {
                        USB_Class = CompatibleID.Substring(classStart + 6, 2);

                        int subClassStart = CompatibleID.IndexOf("SubClass_");
                        if (subClassStart > -1)
                        {
                            USB_SubClass = CompatibleID.Substring(subClassStart + 9, 2);
                            int protocolStart = CompatibleID.IndexOf("Prot_");
                            if (protocolStart > -1)
                            {
                                USB_Protocol = CompatibleID.Substring(protocolStart + 5, 2);
                            }
                        }

                    }

                }

                VendorName = "";
                ProductName = "";
                ProductID = "";
                VendorID = "";

                int serialStart = 0;
                //get vendor id and product id
                if (!string.IsNullOrEmpty(DeviceID))
                {
                    int vidStart = DeviceID.IndexOf("VID_");
                    if (vidStart > -1)
                    {
                        //remove VID_ and get Vendor ID
                        VendorID = DeviceID.Substring(vidStart + 4, 4);
                        int pidStart = DeviceID.IndexOf("PID_");
                        if (pidStart > -1)
                        {
                            //remove PID_ and get Product ID
                            ProductID = DeviceID.Substring(pidStart + 4, 4);
                            serialStart = pidStart + 9;
                        }

                    }

                }
                if (DeviceID.Length - serialStart > 0)
                {
                    string sNum = DeviceID.Substring(serialStart, (DeviceID.Length - serialStart));
                    if (!sNum.Contains("&")) { SerialNumber = sNum; }
                    else { SerialNumber = ""; }
                }
                else { SerialNumber = ""; }

                //get the device type if possible
                DeviceType = DeviceClass.GetClassDevice(this);

                //generate the checksum
                Checksum = GenerateHashCode(ClassGuid + Description + DeviceType + HardwareID +
                                            ProductID + ProductName + SerialNumber + Service +
                                            USB_Class + USB_SubClass + USB_Protocol + VendorID + VendorName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);//change to log file
            }

        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public bool Equals(USBDeviceInfo other)
        {
            if (other != null && Checksum != other.Checksum) return false;

            return true;
        }

        public string Checksum { get; set; }
        public string ClassGuid { get; set; }
        public string CompatibleID { get; set; }
        public string Description { get; set; }
        public string DateAdded { get; set; }
        public string DateConnected { get; set; }
        public string DeviceID { get; set; }
        public string DeviceType { get; set; }
        public string HardwareID { get; set; }
        public string FirstLocationInformation { get; set; }
        public string FirstUsage { get; set; }
        public string LastLocationInformation { get; set; }
        public string Manufacturer { get; set; }
        public string Name { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string SerialNumber { get; set; }
        public string Service { get; set; }
        public string Status { get; set; }
        public string USB_Class { get; set; }
        public string USB_SubClass { get; set; }
        public string USB_Protocol { get; set; }
        public string VendorID { get; set; }
        public string VendorName { get; set; }

        /// <summary>
        /// Create checksum
        /// </summary>
        /// <param name="">Param Description</param>
        public static string GenerateHashCode(string data)
        {
            string generatedHash = "";

            try
            {
                MD5 md5Hash = MD5.Create();
                // Convert string to a byte array and compute the hash.
                byte[] hashData = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(data));

                StringBuilder sBuilder = new StringBuilder();

                // Loop through the hashed data and build a hexadecimal string 
                for (int i = 0; i < hashData.Length; i++)
                {
                    sBuilder.Append(hashData[i].ToString("x2"));
                }

                // Build string from hex-value
                generatedHash = sBuilder.ToString();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);//change to log file
                return string.Empty;
            }
            return generatedHash;
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        //Outsorce code generation to c++ dll ?
        public void generate_HashCodeParentDevice(string childData)
        {
            try
            {
                string data = GenerateHashCode(Checksum + childData);
                Checksum = data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);//change to log file
            }
        }

        /// <summary>
        /// Port connection of a device via the registry
        /// </summary>
        /// <param name="">Param Description</param>
        private string GetDeviceLocation(string deviceID)
        {
            string location = "";
            string[] parts = deviceID.Split(@"\".ToCharArray());

            if (parts.Length >= 3)
            {
                string devRegPath = @"SYSTEM\CurrentControlSet\Enum\" + parts[0] + "\\" + parts[1] + "\\" + parts[2];
                RegistryKey key = Registry.LocalMachine.OpenSubKey(devRegPath);
                if (key != null)
                {
                    object result2 = key.GetValue("LocationInformation");
                    if (result2 != null)
                    {
                        location = result2.ToString();
                    }
                    key.Close();
                }
            }
            return location;
        }

    }

}
