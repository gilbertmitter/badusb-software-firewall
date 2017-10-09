/**
******************************************************************************
* @file	   ChangeDeviceState.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Functions that make the change of the device state
******************************************************************************
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace BadUSB_Firewall
{
    //Parameter für die changeDeviceState Funktion
    public enum ChangeDeviceStateParams
    {
        Enable,             //Activate
        Disable,            //Deactivate
        DisableComposite,   //Deactivate compiund-devices
        DetectReboot,       //Detect if a reboot is needed
        Reset,              //Device-Reset
        Remove,             //Delete a device
        Available,          //Detect if the device is actually connected
        Scanhardwarechanges //Reenumeration of the devicetree
    }
    #region Native_Functions
    public class ChangeDeviceState
    {
        #region DataTypes
        // private const uint CmDrpDriver = 0xA;
        // private const uint CmDrpHardwareid = 0x00000002;
        // private const uint CmDrpFriendlyname = 0x0000000D;
        // private const uint CmDrpPhysicalDeviceObjectName = 0x0000000F;
        // private const uint CmDrpInstallState = 0x00000023;
        // private const uint CmDrpClass = 0x00000008;
        // private const uint CmDrpCapabilities = 0x00000010;
        // private const int  CmLocateDevnodePhantom = 0x00000001;
        // private const uint CmDrpEnumeratorName = 0x00000017;
        private const uint CmDrpDevicedesc = 0x00000001;
        private const uint CmDrpCompatibleids = 0x00000003;
        private const uint CmDrpMfg = 0x0000000C;
        private const uint CmDrpDevtype = 0x0000001A;
        private const uint CmDrpLocationInformation = 0x0000000E;
        private const int CmLocateDevnodeNormal = 0x00000000;      
        private const int CmReenumerateNormal = 0x00000000;
        private const uint CmDrpClassguid = 0x00000009;
        private const uint CmDrpService = 0x00000005;
        private const uint CmRemoveNoRestart = 0x00000002;        
        private const uint DnRemovable = 0x00004000; 
        //All device interface classes
        private const uint DigcfAllclasses = 0x00000004;
        //Devices thar are currently present
        private const uint DigcfPresent = 0x00000002;
        //Device interfaces only for the specified device interface class
        private const uint DigcfDeviceinterface = 0x00000010;
        private const int BufferSize = 256;
        private const int MaxPath = 260;
        private const int DiNeedrestart = 0x00000080;
        private const int DiNeedreboot = 0x00000100;

        //PNP_VETO_TYPE-typed value indicating the reason for the failure
        public enum PnpVetoType
        {
            PnpVetoTypeUnknown = 0//,
            //PnpVetoLegacyDevice = 1,
            //PnpVetoPendingClose = 2,
            //PnpVetoWindowsApp = 3,
            //PnpVetoWindowsService = 4,
            //PnpVetoOutstandingOpen = 5,
            //PnpVetoDevice = 6,
            //PnpVetoDriver = 7,
            //PnpVetoIllegalDeviceRequest = 8,
            //PnpVetoInsufficientPower = 9,
            //PnpVetoNonDisableable = 10,
            //PnpVetoLegacyDriver = 11,
            //PnpVetoInsufficientRights = 12
        }

       public enum DicsFunction
        {
            DicsEnable = 0x00000001,    //Loads the drivers for the device and starts the device, if possible. 
                                        //If the function cannot start the device, it sets the DI_NEEDREBOOT flag for the device which indicates to the 
                                        //initiator of the property change request that they must prompt the user to restart the computer.
            DicsDisable = 0x00000002,   //Disables the device. If the device can be disabled but this function cannot disable the device 
                                        //dynamically, this function marks the device to be disabled the next time that the computer restarts.
            DicsPropchange = 0x00000003 //Removes and reconfigures the device so that the new properties can take effect. This flag typically 
                                        //indicates that a user has changed a property on a device manager property page for the device. The PnP manager 
                                        //directs the drivers for the device to remove their device objects and then the PnP manager reconfigures and restarts the device.
        }

        public enum DifFunction
        {
            DifRemove = 0x00000005,     //device installation function - remove
            DifPropertychange = 0x00000012
        }

        public enum Scopes
        {
            DicsFlagGlobal = 1,
            //DicsFlagConfigspecific = 2,
            DiRemovedeviceGlobal = 0x00000001,
            // DiRemovedeviceConfigspecific = 0x00000002,
            DiUnremovedeviceConfigspecific = 0x00000002
        }
        #endregion

        /*Import of the external SetupAPI and CfgMgr32 DLL.
         *  Descriptions for native functions are mainly taken from the Microsoft Hardware Dev center Website.
         https://developer.microsoft.com/en-us/windows/hardware
         */

        //Checks whether a device instance and its children can be removed and, if so, it removes them.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint CM_Query_And_Remove_SubTree(
                                                                UInt32 devinst, 
                                                                out PnpVetoType pVetoType, 
                                                                StringBuilder pszVetoName, 
                                                                uint ulNameLength, 
                                                                uint ulFlags);                                                               

        //Retrieves a specified device property from the registry
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int CM_Get_DevNode_Registry_Property(
                                                                    uint dnDevInst,
                                                                    uint ulProperty,
                                                                    IntPtr pulRegDataType,
                                                                    StringBuilder buffer,
                                                                    ref uint pulLength,
                                                                    uint ulFlags);

        //Obtains a device instance handle to the parent node of a specified device node (devnode) in the local machine's device tree.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint CM_Get_Parent(
                                        out UInt32 pdnDevInst,
                                        UInt32 dnDevInst,
                                        UInt32 ulFlags);

        //Enumerates the devices identified by a specified device node and all of its children.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern UInt32 CM_Reenumerate_DevNode(
                                                            UInt32 devInst,
                                                            UInt32 flags);

        //Obtains a device instance handle to the device node that is associated with a specified 
        //device instance ID on the local machine.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern UInt32 CM_Locate_DevNode(
                                                        ref UInt32 devInst,
                                                        string pDeviceID,
                                                        UInt32 ulFlags);

        //Obtains the status of a device instance from its device node (devnode) in the local machine's device tree.
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int CM_Get_DevNode_Status(
                                                        out UInt32 status,
                                                        out UInt32 probNum,
                                                        UInt32 devInst,
                                                        int flags);

        //Retrieves the device instance ID for a specified device instance on the local machine.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int CM_Get_Device_ID(
                                                    UInt32 dnDevInst,
                                                    StringBuilder buffer,
                                                    int bufferLen,
                                                    int flags);

        //Returns a handle to a device information set that contains requested device information elements for a local computer.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(
                                                        ref Guid classGuid,
                                                        [MarshalAs(UnmanagedType.LPWStr)] string enumerator,
                                                        IntPtr hwndParent,
                                                        uint flags);

        //Retrieves a specified Plug and Play device property.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupDiGetDeviceRegistryProperty(
                                                                    IntPtr deviceInfoSet,
                                                                    SP_DEVINFO_DATA deviceInfoData,
                                                                    UInt32 property,
                                                                    out UInt32 propertyRegDataType,
                                                                    StringBuilder propertyBuffer,
                                                                    UInt32 propertyBufferSize,
                                                                    out UInt32 requiredSize);


        //Retrieve a device information set for the devices in a specified class.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(
                                                        [In] ref Guid classGuid,
                                                        IntPtr enumerator,
                                                        IntPtr hwndParent,
                                                        UInt32 flags);

        //Free resources used by SetupDiGetClassDevs
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        //Returns a SP_DEVINFO_DATA structure that specifies a device information element in a device information set.
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(
                                                        IntPtr deviceInfoSet,
                                                        UInt32 memberIndex,
                                                        SP_DEVINFO_DATA deviceInfoData);

        //Sets or clears class install parameters for a device information set or a particular device information element
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupDiSetClassInstallParams(
                                                        IntPtr deviceInfoSet,
                                                        SP_DEVINFO_DATA deviceInfoData,
                                                        SP_PROPCHANGE_PARAMS classInstallParams,
                                                        UInt32 classInstallParamsSize);

        //Sets or clears class install parameters for a device information set or a particular device information element
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupDiSetClassInstallParams(
                                                        IntPtr deviceInfoSet,
                                                        SP_DEVINFO_DATA deviceInfoData,
                                                        SP_REMOVEDEVICE_PARAMS classInstallParams,
                                                        UInt32 classInstallParamsSize);

        //For driver remove function
        //Calls the appropriate class installer, and any registered co-installers, with the specified installation request (DIF code).
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupDiCallClassInstaller(
                                                        DifFunction installFunction,
                                                        IntPtr deviceInfoSet,
                                                        SP_DEVINFO_DATA deviceInfoData);

        //For the remove function
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupDiGetDeviceInstallParams(
                                                        IntPtr hDevInfo,
                                                        SP_DEVINFO_DATA deviceInfoData,
                                                        ref SP_DEVINSTALL_PARAMS deviceInstallParams);
 

        //Defines a device instance that is a member of the device information set
        [StructLayout(LayoutKind.Sequential)]
        public class SP_DEVINFO_DATA
        {
            public UInt32 cbSize;
            public Guid classGuid;
            public UInt32 devInst;
            public IntPtr reserved;
        }

        //Contains device installation parameters associated with a particular device information 
        //element or associated globally with a device information set.
        [StructLayout(LayoutKind.Sequential)]
        public class SP_DEVINSTALL_PARAMS
        {
            public Int32 cbSize;
            public Int32 Flags;
            public Int32 FlagsEx;
            public IntPtr hwndParent;
            public IntPtr InstallMsgHandler;
            public IntPtr InstallMsgHandlerContext;
            public IntPtr FileQueue;
            public IntPtr ClassInstallReserved;
            public int Reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxPath)]
            public string DriverPath;
        }

        //Corresponds to a DIF_PROPERTYCHANGE installation request.
        [StructLayout(LayoutKind.Sequential)]
        public class SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader = new SP_CLASSINSTALL_HEADER();
            public DicsFunction StateChange;
            public Scopes Scope;
            public UInt32 HwProfile;
        }

        //SP_REMOVEDEVICE_PARAMS https://msdn.microsoft.com/en-us/library/windows/hardware/ff553323(v=vs.85).aspx
        //Needed for remove and unremove function
        [StructLayout(LayoutKind.Sequential)]
        public class SP_REMOVEDEVICE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader = new SP_CLASSINSTALL_HEADER();
            public Scopes Scope;//uint32?
            public UInt32 HwProfile;//int
        }

        //Contains the device installation request code that defines the format of the rest of the install parameters structure.
        [StructLayout(LayoutKind.Sequential)]
        public class SP_CLASSINSTALL_HEADER
        {
            public UInt32 cbSize;
            public DifFunction InstallFunction;
        };

        #endregion

        /// <summary>
        /// Returns a device property specified by devDescription from the registry for a given device instance.
        /// </summary>
        /// <param name="">Param Description</param>
        private string GetDescription(IntPtr devHandle, SP_DEVINFO_DATA deviceInfoData, SpdrpCodes spdrpCode)
        {
            StringBuilder deviceDescription = new StringBuilder(MaxPath);
            string description = "";
            uint propertyRegDataType;
            uint requiredSize;

            bool found = SetupDiGetDeviceRegistryProperty(
                                                            devHandle,
                                                            deviceInfoData,
                                                            (uint)spdrpCode,
                                                            out propertyRegDataType,
                                                            deviceDescription,
                                                            BufferSize,
                                                            out requiredSize);
            if (found)
            {
                description = deviceDescription.ToString();
            }

            return description;
        }

        /// <summary>
        /// Returns the Stringbuilder length for a searched device-property.
        /// </summary>
        /// <param name="">Param Description</param>
        private uint GetRequiredSize(IntPtr devHandle, SP_DEVINFO_DATA devInfoData, SpdrpCodes spdrpCode)
        {

            uint requiredSize;
            uint propertyRegDataType;
            SetupDiGetDeviceRegistryProperty(
                                                 devHandle,
                                                 devInfoData,
                                                 (uint)spdrpCode,
                                                 out propertyRegDataType,
                                                 null,
                                                 0,
                                                 out requiredSize);
            return requiredSize;
        }

        /// <summary>
        /// Stores a searched device-property in the passed stringbuilder.
        /// </summary>
        /// <param name="deviceProperty">Stores the device-information</param>
        private uint GetDeviceProperty(IntPtr devHandle, SP_DEVINFO_DATA devInfoData, SpdrpCodes spdrpCode, uint propertyBufferSize, StringBuilder deviceProperty)
        {
            uint requiredSize;
            uint propertyRegDataType;
            SetupDiGetDeviceRegistryProperty(
                                                                                        devHandle,
                                                                                        devInfoData,
                                                                                        (uint)spdrpCode,
                                                                                        out propertyRegDataType,
                                                                                        deviceProperty,
                                                                                        propertyBufferSize,
                                                                                        out requiredSize);
            return requiredSize;
        }

        /// <summary>
        /// Checks whether a device with the same hardwareID is currently actively connected to the system.
        /// If a device is found on a different USB port port, this port position is returned.
        /// </summary>
        /// <param name="">Param Description</param>
        public string get_DevicePort(string hardwareID, string deviceLocation, string deviceClassGUID)
        {
            string devLocation = "";
            uint index = 0;

            Guid deviceGuid = new Guid(deviceClassGUID.ToUpper());

            try
            {
                var devHandle = SetupDiGetClassDevs(ref deviceGuid,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    DigcfPresent);

                //check if return value is INVALID_HANDLE_VALUE
                if (devHandle != (IntPtr)ErrorCode.InvalidHandleValue)
                {
                    SP_DEVINFO_DATA devInfoData =
                        new SP_DEVINFO_DATA
                        {
                            cbSize = (uint)Marshal.SizeOf(typeof(SP_DEVINFO_DATA)),
                            classGuid = deviceGuid,
                            devInst = 0,
                            reserved = IntPtr.Zero
                        };

                    while (true)
                    {
                        //Function returns a SP_DEVINFO_DATA structure, which specifies a device information element.
                        var found = SetupDiEnumDeviceInfo(devHandle, index, devInfoData);
                        if (!found)
                        {
                            break;//no more devices
                        }
                        //check hardwareID
                        var devHwId = GetDescription(devHandle, devInfoData, SpdrpCodes.SpdrpHardwareid);
                        //Found device equals the device to handle.
                        if (devHwId.ToLower().Equals(hardwareID.ToLower()))
                        {
                            devLocation = GetDescription(devHandle, devInfoData, SpdrpCodes.SpdrpLocationInformation);
                            //found location differs from last connection location of the device
                            if (devLocation != deviceLocation) { break; }
                            devLocation = "";
                        }
                        index++;
                    }
                    //Delete device information set and free the associated memory.
                    SetupDiDestroyDeviceInfoList(devHandle);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "Error";
            }
            return devLocation;

        }

        /// <summary>
        /// Function for deactivation of a USB-device.
        /// </summary>
        /// <param name="">Param Description</param>
        public int disable_USBDevice(string hardwareID, string classGuid, string firstLocation, bool usePort)
        {
            //Change device-state (deaktivate)
            var conditionResult = ChangeDevState(hardwareID, classGuid, firstLocation, usePort, ChangeDeviceStateParams.Disable);

            switch (conditionResult)
            {
                //Device can't be disabled => delete driver for it
                case (int)ErrorCode.ErrorNotDisableable:
                    conditionResult = ChangeDevState(hardwareID, classGuid, firstLocation, usePort, ChangeDeviceStateParams.Remove);
                    break;
                case (int)ErrorCode.ErrorInvalidData:
                    conditionResult = ChangeDevState(hardwareID, classGuid, firstLocation, usePort, ChangeDeviceStateParams.Disable);
                    if (conditionResult != (int)ErrorCode.Success)
                    {
                        conditionResult = ChangeDevState(hardwareID, classGuid, firstLocation, usePort, ChangeDeviceStateParams.Remove);
                    }
                    break;
                case (int)ErrorCode.NotFound:
                    conditionResult = ChangeDevState(hardwareID, classGuid, firstLocation, false, ChangeDeviceStateParams.Disable);
                    if (conditionResult == (int)ErrorCode.ErrorNotDisableable)
                    {
                        conditionResult = ChangeDevState(hardwareID, classGuid, firstLocation, false, ChangeDeviceStateParams.Remove);
                    }
                    break;
                case (int)ErrorCode.ErrorNoSuchDevinst:
                    conditionResult = ChangeDevState(hardwareID, classGuid, firstLocation, usePort, ChangeDeviceStateParams.Disable);
                    break;
            }

            return conditionResult;
        }


        /// <summary>
        /// Function which finds a device in the existing device tree corresponding to the hardware ID and connection position
        /// and then call the function specified by "mode" (eg enable, disable).
        /// </summary>
        /// <param name="">Param Description</param>
        /// <param name="hardwareID">HardwareID of the device</param>
        /// <param name="deviceClassGUID">Device-class-GUID of the device</param>
        /// <param name="deviceLocation">Connection-position</param>
        /// <param name="usePort">Defines if the connection-position of the device should be used</param>
        /// <param name="mode">ChangeDeviceStateParams</param>
        public int ChangeDevState(string hardwareID, string deviceClassGUID, string deviceLocation, bool usePort, ChangeDeviceStateParams mode)
        {
            int result = 0;
            string deviceHwId = "";
            IntPtr devHandle = IntPtr.Zero;
            uint index = 0;
            Guid deviceGuid = new Guid(deviceClassGUID);
            Guid devGuid = Guid.Empty;

            try
            {
                //Divide existing Hardwarid's. Only use HardwareID in the form
                //USB\VID_v(4)&PID_d(4)&REV_r(4) for the search.
                string[] parts = hardwareID.Split(@" ".ToCharArray());
                if (parts.Length > 0)
                {
                    deviceHwId = parts[0];
                }

                //Device-activation -> search in all available devices
                if (mode == ChangeDeviceStateParams.Enable)
                {
                    //get Get device interface info handle for guid of the searched device
                    devHandle = SetupDiGetClassDevs(ref deviceGuid,
                                                       IntPtr.Zero,
                                                       IntPtr.Zero,
                                                       DigcfAllclasses | DigcfPresent);//All Devices 
                }
                //Only search for activated devices
                else
                {
                    //get Get device interface info handle for guid of the searched device
                    devHandle = SetupDiGetClassDevs(ref deviceGuid,//classguid
                                                       IntPtr.Zero,//no enumerator
                                                       IntPtr.Zero,
                                                       DigcfPresent);//Only Devices present
                }

                //check for a return value of INVALID_HANDLE_VALUE
                if (devHandle == (IntPtr)ErrorCode.InvalidHandleValue)
                {
                    return (int)ErrorCode.InvalidHandleError;
                }

                SP_DEVINFO_DATA devInfoData =
                    new SP_DEVINFO_DATA
                    {
                        cbSize = (uint)Marshal.SizeOf(typeof(SP_DEVINFO_DATA)),
                        classGuid = mode == ChangeDeviceStateParams.Enable ? devGuid : deviceGuid,
                        devInst = 0,
                        reserved = IntPtr.Zero
                    };

                bool foundDevice;
                while (true)
                {
                    foundDevice = false;
                    //Returns an SP_DEVINFO_DATA structure that specifies a device information item in a device information set.
                    var found = SetupDiEnumDeviceInfo(devHandle, index, devInfoData);
                    if (!found)
                    {
                        break;//No more devices
                    }
                    //Check the HardwareID
                    var devHwId = GetDescription(devHandle, devInfoData, SpdrpCodes.SpdrpHardwareid);

                    if (devHwId.ToLower().Equals(deviceHwId.ToLower()))
                    {
                        //Use the port connection
                        if (usePort)
                        {
                            var devLocation = GetDescription(devHandle, devInfoData, SpdrpCodes.SpdrpLocationInformation);
                            //Returned Port matches the searched port
                            if (devLocation == deviceLocation)
                            {
                                foundDevice = true;
                                break;
                            }
                        }
                        else
                        {
                            foundDevice = true;
                            break;
                        }
                    }
                    index++;
                }
                //Searched device was found
                if (foundDevice)
                {
                    var res = false;
                    switch (mode)
                    {
                        //Deactivate a compound-device (Use the CM_Query_And_Remove_SubTree function).
                        case ChangeDeviceStateParams.DisableComposite:
                            result = Remove_DeviceNew(devInfoData);
                            if (result != (int)ErrorCode.Success)
                            {
                                res = SetupDiCallClassInstaller(DifFunction.DifRemove, devHandle, devInfoData);
                                if (res) { result = (int)ErrorCode.Success; } else { result = Marshal.GetLastWin32Error(); }
                            }
                            break;
                        //Deactivate the device
                        case ChangeDeviceStateParams.Disable:
                            result = Change_DeviceState(devHandle, devInfoData, DicsFunction.DicsDisable);
                            // ERROR_NOT_DISABLEABLE)// 0xe0000231 =  decimal -536870351(on 32bit system),
                            if (result == (int)ErrorCode.ErrorNotDisableable)
                            {
                                //Device is not suitable for disabling in normal way
                                result = Remove_DeviceNew(devInfoData);
                                if (result != (int)ErrorCode.Success)
                                {
                                    res = SetupDiCallClassInstaller(DifFunction.DifRemove, devHandle, devInfoData);
                                    if (res) { result = (int)ErrorCode.Success; } else { result = Marshal.GetLastWin32Error(); }
                                }
                            }
                            break;
                        //Activate device
                        case ChangeDeviceStateParams.Enable:
                            result = Change_DeviceState(devHandle, devInfoData, DicsFunction.DicsEnable);
                            if (result == (int)ErrorCode.Success)
                            {
                                uint status;
                                uint deviceProblemCode;
                                //get status information for a device
                                CM_Get_DevNode_Status(out status, out deviceProblemCode, devInfoData.devInst, 0);

                                //Common Device manager Error codes
                                //21 - Windows is removing this device.
                                //22 - This device is disabled.
                                //45 - Currently, this hardware device is not connected to the computer.
                                //47 - Windows cannot use this hardware device because it has been prepared for safe removal, but it has not been removed from the computer.
                                
                                //Reset device-node
                                if (deviceProblemCode > 0)
                                {
                                    result = Change_DeviceState(devHandle, devInfoData, DicsFunction.DicsPropchange);
                                }

                            }
                            break;
                        
                        case ChangeDeviceStateParams.Reset: result = Change_DeviceState(devHandle, devInfoData, DicsFunction.DicsPropchange); break;

                        //Delete device
                        case ChangeDeviceStateParams.Remove:
                            result = Remove_DeviceNew(devInfoData);
                            if (result != (int)ErrorCode.Success)
                            {
                                //try to uninstall device
                                res = SetupDiCallClassInstaller(DifFunction.DifRemove, devHandle, devInfoData);
                                if (res) { result = (int)ErrorCode.Success; } else { result = Marshal.GetLastWin32Error(); }
                            }
                            break;
                        case ChangeDeviceStateParams.Available: result = (int)ErrorCode.Success; break;

                        //Check if there are any changes to the hardware
                        case ChangeDeviceStateParams.Scanhardwarechanges: result = ForceReenumeration(); break;

                        default: break;

                    }
                }

                else
                {
                    result = (int)ErrorCode.NotFound;
                }
                //delete the device information set and free associated memory
                SetupDiDestroyDeviceInfoList(devHandle);
            }
            catch (Exception ex)
            {
                if (devHandle != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(devHandle);
                }
                return (int)ErrorCode.ExceptionError;
            }

            return result;

        }

        /// <summary>
        /// Searches the interfaces for a specified-device and return them as a list
        /// </summary>
        /// <param name="">Param Description</param>
        public int FindChild(List<USBDeviceInfo> childDevices, string instanceID, string hardwareID, string dateConnected)
        {

            IntPtr devHandle = IntPtr.Zero;
            uint index = 0;
            Guid devGuid = Guid.Empty;
            int findPos = 0;
            string trimHwId = "";

            try
            {
                if (childDevices != null)
                {
                    for (int i = 0; i < hardwareID.Length; i++)
                    {
                        if (hardwareID[i] == '\\')
                        {
                            findPos = i + 1;
                            break;
                        }
                    }
                    if (findPos > 0 && findPos < hardwareID.Length)
                    {
                        trimHwId = hardwareID.Substring(findPos);
                    }


                    //get Get device interface info handle for Guid of the searched device
                    devHandle = SetupDiGetClassDevs(
                                                    ref devGuid,
                                                    IntPtr.Zero,                    //no enumerator
                                                    IntPtr.Zero,
                                                    DigcfAllclasses | DigcfPresent);//Only Devices present


                    //check for a return value of INVALID_HANDLE_VALUE
                    if (devHandle == (IntPtr)ErrorCode.InvalidHandleValue)
                    {
                        return (int)ErrorCode.InvalidHandleValue;
                    }

                    SP_DEVINFO_DATA devInfoData =
                        new SP_DEVINFO_DATA
                        {
                            cbSize = (uint)Marshal.SizeOf(typeof(SP_DEVINFO_DATA)),
                            classGuid = devGuid,
                            devInst = 0,
                            reserved = IntPtr.Zero
                        };
                    //enumerate all devices in the set
                    while (true) 
                    {
                        var found = SetupDiEnumDeviceInfo(devHandle, index, devInfoData);
                        if (!found)
                        {
                            break;//no more devices
                        }
                        //check for hardwareID
                        var devHwId = GetDescription(devHandle, devInfoData, SpdrpCodes.SpdrpHardwareid);

                        if (devHwId.ToLower().Contains(trimHwId.ToLower()) && devHwId != hardwareID)
                        {
                            uint devNext;
                            //get parent device -> usb level
                            uint result = CM_Get_Parent(out devNext,
                                devInfoData.devInst,
                                0);
                            if (result == (int)ErrorCode.Success)
                            {
                                StringBuilder devId = new StringBuilder(MaxPath);
                                //get deviceId from parent device and compare with the given deviceId
                                if (CM_Get_Device_ID(devNext, devId, MaxPath, 0) == (int)ErrorCode.Success)
                                {
                                    if (devId.ToString().Equals(instanceID))
                                    {
                                        StringBuilder deviceId = new StringBuilder(MaxPath);
                                        //get child device Id
                                        if (CM_Get_Device_ID(devInfoData.devInst, deviceId, MaxPath, 0) == (int)ErrorCode.Success)
                                        {
                                            USBDeviceInfo childData = get_ChildData(devInfoData.devInst, devHwId, dateConnected, deviceId.ToString());

                                            if (childData != null)
                                            {
                                                // add to child list
                                                childDevices.Add(childData);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        index++;

                    }
                    //delete the device information set and free associated memory
                    SetupDiDestroyDeviceInfoList(devHandle);
                }
            }
            catch (Exception ex)
            {
                if (devHandle != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(devHandle);
                }

                return (int)ErrorCode.ExceptionError;
            }

            if (childDevices != null)
            {
                return childDevices.Count;
            }
            return 0;
        }

        /// <summary>
        /// Reenumeration of a device tree from an application
        /// and scan for Hardware-changes.
        /// corresponds to "Search fór changed hardware" in the device-manager of windows.
        /// https://support.microsoft.com/en-gb/kb/259697
        /// </summary>
        /// <param name="">Param Description</param>
        public int ForceReenumeration()
        {
            uint devInst = 0;
            //Device-node
            var result = CM_Locate_DevNode(ref devInst, null, CmLocateDevnodeNormal);

            if (result != (int)ErrorCode.Success)
            {
                return (int)ErrorCode.InvalidHandleValue;
            }

            result = CM_Reenumerate_DevNode(devInst, CmReenumerateNormal);

            if (result != (int)ErrorCode.Success)
            {
                return (int)ErrorCode.ReenumerationError;
            }
            return (int)ErrorCode.Success;
        }

        /// <summary>
        /// Checks if a device device and their children can be removed, and if so, removes them.
        /// </summary>
        /// <param name="">Param Description</param>
        //https://msdn.microsoft.com/en-gb/library/ff539806.aspx
        //https://msdn.microsoft.com/en-gb/library/ff539722.aspx
        private int Remove_DeviceNew(SP_DEVINFO_DATA devInfoData)
        {
            int lastError = 0;

            try
            {
                if (devInfoData.devInst > 0)
                {
                    StringBuilder pszVetoName = new StringBuilder(MaxPath);

                    uint deviceProblemCode;
                    uint status;
                    uint result;
                    PnpVetoType vetoType;// PNP_VETO_TYPE To identify the reason for the rejection of the function

                    //Get the status-information of a device
                    lastError = CM_Get_DevNode_Status(out status, out deviceProblemCode, devInfoData.devInst, 0);

                    if (lastError == (int)ErrorCode.Success)
                    {
                        result = CM_Query_And_Remove_SubTree(devInfoData.devInst, out vetoType, pszVetoName, MaxPath, CmRemoveNoRestart);

                        lastError = Marshal.GetLastWin32Error();
                        //PnpVetoTypeUnknown = 0 (The specified operation was rejected for an unknown reason)
                        if (vetoType == PnpVetoType.PnpVetoTypeUnknown && result == (int)ErrorCode.Success)
                        {
                            lastError = (int)ErrorCode.Success;
                        }
                    }
                }
                return lastError;
            }
            catch
            {
                return lastError;
            }
        }

        /// <summary>
        /// Switches device state to enabled or disabled
        /// </summary>
        /// <param name="">Param Description</param>
        private int Change_DeviceState(IntPtr devInfo, SP_DEVINFO_DATA devInfoData, DicsFunction stateParam)
        {
            int lastError = 0;
            try
            {
                SP_PROPCHANGE_PARAMS pcpParams = new SP_PROPCHANGE_PARAMS
                {
                    ClassInstallHeader =
                    {
                        cbSize = (uint) Marshal.SizeOf(typeof(SP_CLASSINSTALL_HEADER)),
                        InstallFunction = DifFunction.DifPropertychange
                    },
                    Scope = Scopes.DicsFlagGlobal,
                    StateChange = stateParam,
                    HwProfile = 0
                };


                var result = SetupDiSetClassInstallParams(devInfo,
                    devInfoData,
                    pcpParams,
                    (uint)Marshal.SizeOf(typeof(SP_PROPCHANGE_PARAMS)));
                if (result == false)
                {
                    lastError = Marshal.GetLastWin32Error();
                    return lastError;
                }

                result = SetupDiCallClassInstaller(DifFunction.DifPropertychange,
                                                   devInfo,
                                                   devInfoData);

                if (result == false)
                {
                    //Get the last error
                    lastError = Marshal.GetLastWin32Error();
                    return lastError;
                }
            }
            catch (Exception ex)
            {
                return lastError;
            }
            return lastError;
        }

        /// <summary>
        /// Return a device-onformation for a searched device-property
        /// </summary>
        /// <param name="">Param Description</param>
        private static string GetDeviceData(uint devInst, uint devProperty)
        {
            uint len = 0;
         
            CM_Get_DevNode_Registry_Property(devInst, devProperty, IntPtr.Zero, null, ref len, 0);
            StringBuilder data = new StringBuilder((int)len);
            var result = CM_Get_DevNode_Registry_Property(devInst, devProperty, IntPtr.Zero, data, ref len, 0);

            if (result == (int)ErrorCode.Success)
            {
                return data.ToString();
            }
            //The device driver for this hardware can not be initialized
            else if (result == 37) { return ""; }
            else
            {
                return "-1";
            }

        }

        /// <summary>
        /// Request information from a found USB device
        /// </summary>
        /// <param name="">Param Description</param>
        private static USBDeviceInfo get_ChildData(uint devInst, string hwID, string dateConnected, string devID)
        {

            USBDeviceInfo childDevice = new USBDeviceInfo();
            uint deviceProblemCode;//status
            uint status;

            childDevice.DeviceID = devID;
            childDevice.HardwareID = hwID;
            childDevice.FirstLocationInformation = GetDeviceData(devInst, CmDrpLocationInformation);
            childDevice.LastLocationInformation = childDevice.FirstLocationInformation;
            childDevice.ClassGuid = GetDeviceData(devInst, CmDrpClassguid);
            childDevice.Description = GetDeviceData(devInst, CmDrpDevicedesc);
            childDevice.DeviceType = GetDeviceData(devInst, CmDrpDevtype);
            childDevice.Manufacturer = GetDeviceData(devInst, CmDrpMfg);

            //get status information for a device
            CM_Get_DevNode_Status(out status, out deviceProblemCode, devInst, 0);

            childDevice.Status = deviceProblemCode.ToString();
            childDevice.Service = GetDeviceData(devInst, CmDrpService);
            childDevice.CompatibleID = GetDeviceData(devInst, CmDrpCompatibleids);
            childDevice.Name = childDevice.Description;
            childDevice.DateConnected = dateConnected;

            string compatibleId = childDevice.CompatibleID;
            if (!string.IsNullOrEmpty(compatibleId))
            {
                int classStart = compatibleId.IndexOf("Class_");
                if (classStart > -1)
                {
                    childDevice.USB_Class = compatibleId.Substring(classStart + 6, 2);

                    int subClassStart = compatibleId.IndexOf("SubClass_");
                    if (subClassStart > -1)
                    {
                        childDevice.USB_SubClass = compatibleId.Substring(subClassStart + 9, 2);
                        int protocolStart = compatibleId.IndexOf("Prot_");
                        if (protocolStart > -1)
                        {
                            childDevice.USB_Protocol = compatibleId.Substring(protocolStart + 5, 2);
                        }
                    }
                }
            }

            int serialStart = 0;
            if (!string.IsNullOrEmpty(childDevice.HardwareID))
            {
                int vidStart = childDevice.HardwareID.IndexOf("VID_");
                if (vidStart > -1)
                {
                    //remove VID_ and get Vendor ID
                    childDevice.VendorID = childDevice.HardwareID.Substring(vidStart + 4, 4);
                    int pidStart = childDevice.HardwareID.IndexOf("PID_");
                    if (pidStart > -1)
                    {
                        //remove PID_ and get Product ID
                        childDevice.ProductID = childDevice.HardwareID.Substring(pidStart + 4, 4);
                        serialStart = pidStart + 9;
                    }

                }

            }
            if (childDevice.HardwareID.Length - serialStart > 0)
            {
                string sNum = childDevice.HardwareID.Substring(serialStart, (childDevice.HardwareID.Length - serialStart));
                if (!sNum.Contains("&")) { childDevice.SerialNumber = sNum; }
                else { childDevice.SerialNumber = ""; }
            }
            else { childDevice.SerialNumber = ""; }

            string tempType = DeviceClass.GetClassDevice(childDevice);

            if (string.IsNullOrEmpty(tempType))
            {
                childDevice.DeviceType = childDevice.Description;
            }
            else { childDevice.DeviceType = tempType; }


            childDevice.Checksum = USBDeviceInfo.GenerateHashCode(childDevice.ClassGuid + childDevice.Description + childDevice.DeviceType + childDevice.HardwareID +
                                                                childDevice.ProductID + childDevice.ProductName + childDevice.SerialNumber + childDevice.Service +
                                                                childDevice.USB_Class + childDevice.USB_SubClass + childDevice.USB_Protocol + childDevice.VendorID + childDevice.VendorName);
            return childDevice;
        }



        /// <summary>
        /// Check the device information of the main unit
        /// </summary>
        /// <param name="">Param Description</param>
        public bool getDeviceDescription(DEV_BROADCAST_DEVICEINTERFACE dev, USBDeviceInfo device)
        {
            bool found = false;
            if (dev.dbcc_name.Length > 4)
            {
                string devId = dev.dbcc_name.Substring(4);
                int index = devId.LastIndexOf("#");
                if (index != -1)
                {
                    string trimDevId = devId.Remove(index);

                    devId = trimDevId.Replace('#', '\\').ToUpper();

                    int strLength = devId.Length;
                    index = -1;
                    for (int i = 0; i < strLength; i++)
                    {
                        if (devId[i] == '\\')
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index > -1)
                    {
                        Guid devGuid = Guid.Empty;
                        Guid tempdevGuid = dev.dbcc_classguid;

                        var devHandle = SetupDiGetClassDevs(ref tempdevGuid, null, IntPtr.Zero, DigcfDeviceinterface | DigcfPresent);

                        if (devHandle.ToInt64() != (int)ErrorCode.InvalidHandleValue)
                        {

                            SP_DEVINFO_DATA devInfoData =
                                new SP_DEVINFO_DATA
                                {
                                    cbSize = (UInt32)Marshal.SizeOf(typeof(SP_DEVINFO_DATA)),
                                    classGuid = devGuid,
                                    devInst = 0,
                                    reserved = IntPtr.Zero
                                };
                            StringBuilder deviceId = new StringBuilder(MaxPath);
                            device.DateConnected = DateTime.Now.ToString();

                            for (uint i = 0; SetupDiEnumDeviceInfo(devHandle, i, devInfoData); i++)
                            {

                                CM_Get_Device_ID(devInfoData.devInst, deviceId, MaxPath, 0);


                                if (devId == deviceId.ToString())
                                {
                                    found = true;

                                    uint requiredSize = 0;
                                    
                                    requiredSize = GetRequiredSize(devHandle,devInfoData,SpdrpCodes.SpdrpService);
                                    StringBuilder deviceService = new StringBuilder((int)requiredSize);
                                    GetDeviceProperty(devHandle, devInfoData, SpdrpCodes.SpdrpService, requiredSize, deviceService);

                                    if (deviceService.ToString().Length > 0)
                                    {
                                        requiredSize = GetRequiredSize(devHandle, devInfoData, SpdrpCodes.SpdrpClassguid);
                                        StringBuilder deviceClassGuid = new StringBuilder((int)requiredSize);
                                        GetDeviceProperty(devHandle, devInfoData, SpdrpCodes.SpdrpClassguid, requiredSize, deviceClassGuid);

                                        Guid classGuid = new Guid(deviceClassGuid.ToString().ToUpper());

                                        if (DeviceClass.AcceptedGuid.Contains(classGuid))
                                        {
                                            requiredSize = GetRequiredSize(devHandle, devInfoData, SpdrpCodes.SpdrpDevicedesc);
                                            StringBuilder deviceDescription = new StringBuilder((int)requiredSize);
                                            GetDeviceProperty(devHandle, devInfoData, SpdrpCodes.SpdrpDevicedesc, requiredSize, deviceDescription);

                                            requiredSize = GetRequiredSize(devHandle, devInfoData, SpdrpCodes.SpdrpHardwareid);
                                            StringBuilder deviceHwId = new StringBuilder((int)requiredSize);
                                            GetDeviceProperty(devHandle, devInfoData, SpdrpCodes.SpdrpHardwareid, requiredSize, deviceHwId);

                                            requiredSize = GetRequiredSize(devHandle, devInfoData, SpdrpCodes.SpdrpLocationInformation);
                                            StringBuilder devicePath = new StringBuilder((int)requiredSize);
                                            GetDeviceProperty(devHandle, devInfoData, SpdrpCodes.SpdrpLocationInformation, requiredSize, devicePath);

                                            requiredSize = GetRequiredSize(devHandle, devInfoData, SpdrpCodes.SpdrpCompatibleids);
                                            StringBuilder deviceCompatibleId = new StringBuilder((int)requiredSize);
                                            GetDeviceProperty(devHandle, devInfoData, SpdrpCodes.SpdrpCompatibleids, requiredSize, deviceCompatibleId);

                                            requiredSize = GetRequiredSize(devHandle, devInfoData, SpdrpCodes.SpdrpMfg);
                                            StringBuilder deviceMfg = new StringBuilder((int)requiredSize);
                                            GetDeviceProperty(devHandle, devInfoData, SpdrpCodes.SpdrpMfg, requiredSize, deviceMfg);

                                            //get status information on a device
                                            uint deviceProblemCode;//status
                                            uint status;
                                            CM_Get_DevNode_Status(out status, out deviceProblemCode, devInfoData.devInst, 0);

                                            device.Status = deviceProblemCode.ToString();
                                            device.Manufacturer = deviceMfg.ToString();
                                            device.Service = deviceService.ToString();
                                            device.ClassGuid = deviceClassGuid.ToString();
                                            device.CompatibleID = deviceCompatibleId.ToString();
                                            device.Description = deviceDescription.ToString();
                                            device.DeviceID = devId;
                                            device.FirstLocationInformation = devicePath.ToString();
                                            device.LastLocationInformation = device.FirstLocationInformation;
                                            device.HardwareID = deviceHwId.ToString();
                                            device.Name = device.Description;
                                            string compatibleId = deviceCompatibleId.ToString();

                                            if (!string.IsNullOrEmpty(compatibleId))
                                            {
                                                int classStart = compatibleId.IndexOf("Class_");
                                                if (classStart > -1)
                                                {
                                                    device.USB_Class = compatibleId.Substring(classStart + 6, 2);

                                                    int subClassStart = compatibleId.IndexOf("SubClass_");
                                                    if (subClassStart > -1)
                                                    {
                                                        device.USB_SubClass = compatibleId.Substring(subClassStart + 9, 2);
                                                        int protocolStart = compatibleId.IndexOf("Prot_");
                                                        if (protocolStart > -1)
                                                        {
                                                            device.USB_Protocol = compatibleId.Substring(protocolStart + 5, 2);
                                                        }
                                                    }
                                                }
                                            }

                                            int serialStart = 0;
                                            if (!string.IsNullOrEmpty(devId))
                                            {
                                                int vidStart = devId.IndexOf("VID_");
                                                if (vidStart > -1)
                                                {
                                                    //remove VID_ and get Vendor ID
                                                    device.VendorID = devId.Substring(vidStart + 4, 4);
                                                    int pidStart = devId.IndexOf("PID_");
                                                    if (pidStart > -1)
                                                    {
                                                        //remove PID_ and get Product ID
                                                        device.ProductID = devId.Substring(pidStart + 4, 4);
                                                        serialStart = pidStart + 9;
                                                    }

                                                }

                                            }
                                            if (devId.Length - serialStart > 0)
                                            {
                                                string sNum = devId.Substring(serialStart, (devId.Length - serialStart));
                                                if (!sNum.Contains("&")) { device.SerialNumber = sNum; }
                                                else { device.SerialNumber = ""; }
                                            }
                                            else { device.SerialNumber = ""; }

                                            string tempType = DeviceClass.GetClassDevice(device);

                                            if (string.IsNullOrEmpty(tempType))
                                            {
                                                device.DeviceType = device.Description;
                                            }
                                            else { device.DeviceType = tempType; }

                                            device.Checksum = USBDeviceInfo.GenerateHashCode(device.ClassGuid + device.Description + device.DeviceType + device.HardwareID +
                                                                                       device.ProductID + device.ProductName + device.SerialNumber + device.Service +
                                                                                       device.USB_Class + device.USB_SubClass + device.USB_Protocol + device.VendorID + device.VendorName);
                                        }
                                        else { found = false; break; }
                                    }
                                    else { found = false; break; }
                                }
                                if (found)
                                {
                                    break;
                                }
                            }
                        }
                        SetupDiDestroyDeviceInfoList(devHandle);
                    }
                }
            }
            return found;
        }
    }

}


