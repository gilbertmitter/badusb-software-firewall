/**
******************************************************************************
* @file	   NetAdapterItem.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Class which is a network adapter device.
******************************************************************************
*/
using System.Management;
using System.Collections.Generic;

namespace BadUSB_Firewall
{

    /// <summary>
    /// Description
    /// </summary>
    /// <param name="">Param Description</param>
    public class NetAdapterItem
    {
        //The different connection types of a network adapter to a network
        private readonly Dictionary<string, string> _netConList = new Dictionary<string, string>
            {
                {"0","Disconnected" },
                {"1","Connecting"},
                {"2","Connected" },
                {"3","Disconnecting" },
                {"4","Hardware Not Present" },
                {"5","Hardware Disabled" },
                {"6","Hardware Malfunction" },
                {"7","Media Disconnected" },
                {"8","Authenticating" },
                {"9","Authentication Succeeded" },
                {"10","Authentication Failed" },
                {"11","Invalid Address" },
                {"12","Credentials Required" }
            };

        /// <summary>
        /// Check network connection status to a device
        /// </summary>
        /// <param name="">Param Description</param>
        private string GetNetConStatus(string value)
        {
            string netConStatus;

            if (_netConList.ContainsKey(value))
            {
                netConStatus = _netConList[value];
            }
            else
            {
                netConStatus = "Other";
            }


            return netConStatus;

        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public NetAdapterItem(ManagementObject device)
        {
            AdapterAvailability = device.GetPropertyValue("Availability") == null ? "" : device.GetPropertyValue("Availability").ToString();
            AdapterCaption = device.GetPropertyValue("Caption") == null ? "" : device.GetPropertyValue("Caption").ToString();
            AdapterConfigManagerErrorCode = device.GetPropertyValue("ConfigManagerErrorCode") == null ? "" : device.GetPropertyValue("ConfigManagerErrorCode").ToString();
            AdapterDescription = device.GetPropertyValue("Description") == null ? "" : device.GetPropertyValue("Description").ToString();
            AdapterDeviceId = device.GetPropertyValue("DeviceID") == null ? "" : device.GetPropertyValue("DeviceID").ToString();
            AdapterGuid = device.GetPropertyValue("GUID") == null ? "" : device.GetPropertyValue("GUID").ToString();
            AdapterInstalled = device.GetPropertyValue("Installed") == null ? "" : device.GetPropertyValue("Installed").ToString();
            AdapterInterfaceIndex = device.GetPropertyValue("InterfaceIndex") == null ? "" : device.GetPropertyValue("InterfaceIndex").ToString();
            AdapterMacAddress = device.GetPropertyValue("MACAddress") == null ? "" : device.GetPropertyValue("MACAddress").ToString();
            AdapterManufacturer = device.GetPropertyValue("Manufacturer") == null ? "" : device.GetPropertyValue("Manufacturer").ToString();
            AdapterName = device.GetPropertyValue("Name") == null ? "" : device.GetPropertyValue("Name").ToString();
            AdapterNetConnectionId = device.GetPropertyValue("NetConnectionID") == null ? "" : device.GetPropertyValue("NetConnectionID").ToString();
            AdapterNetConnectionStatus = device.GetPropertyValue("NetConnectionStatus") == null ? "" : GetNetConStatus(device.GetPropertyValue("NetConnectionStatus").ToString());
            AdapterNetEnabled = device.GetPropertyValue("NetEnabled") == null ? "" : device.GetPropertyValue("NetEnabled").ToString();
            AdapterPhysicalAdapter = device.GetPropertyValue("PhysicalAdapter") == null ? "" : device.GetPropertyValue("PhysicalAdapter").ToString();
            AdapterPnpDeviceId = device.GetPropertyValue("PNPDeviceID") == null ? "" : device.GetPropertyValue("PNPDeviceID").ToString();
            AdapterProductName = device.GetPropertyValue("ProductName") == null ? "" : device.GetPropertyValue("ProductName").ToString();
            AdapterServiceName = device.GetPropertyValue("ServiceName") == null ? "" : device.GetPropertyValue("ServiceName").ToString();
            AdapterType = device.GetPropertyValue("AdapterType") == null ? "" : device.GetPropertyValue("AdapterType").ToString();
            AdapterTimeOfLastReset = device.GetPropertyValue("TimeOfLastReset") == null ? "" : device.GetPropertyValue("TimeOfLastReset").ToString();
            AdapterChecksum = USBDeviceInfo.GenerateHashCode(
                AdapterCaption + AdapterDescription + AdapterDeviceId + AdapterGuid +
                AdapterInstalled + AdapterInterfaceIndex + AdapterMacAddress + AdapterManufacturer +
                AdapterName + AdapterNetConnectionId + AdapterPnpDeviceId + AdapterPhysicalAdapter +
                AdapterProductName + AdapterServiceName + AdapterType);
        }

        ///// <summary>
        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public string AdapterAvailability { get; set; }
        public string AdapterCaption { get; set; }
        public string AdapterConfigManagerErrorCode { get; set; }
        public string AdapterDate { get; set; }
        public string AdapterDescription { get; set; }
        public string AdapterDeviceId { get; set; }
        public string AdapterGuid { get; set; }
        public string AdapterInstalled { get; set; }
        public string AdapterInterfaceIndex { get; set; }
        public string AdapterMacAddress { get; set; }
        public string AdapterManufacturer { get; set; }
        public string AdapterName { get; set; }
        public string AdapterNetConnectionId { get; set; }
        public string AdapterNetConnectionStatus { get; set; }
        public string AdapterNetEnabled { get; set; }
        public string AdapterPhysicalAdapter { get; set; }
        public string AdapterPnpDeviceId { get; set; }
        public string AdapterProductName { get; set; }
        public string AdapterServiceName { get; set; }
        public string AdapterTimeOfLastReset { get; set; }
        public string AdapterType { get; set; }
        public string AdapterChecksum { get; set; }

    }
}
