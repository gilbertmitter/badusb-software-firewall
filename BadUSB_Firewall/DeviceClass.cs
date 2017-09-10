/**
******************************************************************************
* @file	   DeviceClass.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Beinhaltet Informationen und Funktionen zu den USB-Geräteklassen
******************************************************************************
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BadUSB_Firewall
{
    public class DeviceClass
    {
        public DeviceClass()
        {
           Fill_ExampleList();
        }
        private static ObservableCollection<Tuple<string, string, string[], string[]>> _uSbDeviceExamples = new ObservableCollection<Tuple<string, string, string[], string[]>>();

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public static ObservableCollection<Tuple<string, string, string[], string[]>> UsbDeviceExamples
        {
            get { return _uSbDeviceExamples; }
            set
            {
                if (Equals(value, _uSbDeviceExamples))
                    return;
                _uSbDeviceExamples = value;
            }
        }

        //Vorab getestete Gerätebeispiele
        #region USBExample_Devices
        public void Fill_ExampleList()
        {
            //Smartphone 
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("Android-Smartphone USB-Internet Sharing (Wiko)", "Wiko Darknight", new string[] { "Remote NDIS Internet Sharing Deivice" }, new string[] { "Wireless Controller" }));

            //Mass storage
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("Single Card Reader (Transcend)", "Transcend Card Reader TS-RDS5 (SD/MMC) USB 2.0", new string[] { "USB-Massenspeichergerät" }, new string[] { "Mass Storage" }));
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("USB 2.0-Multi Card Reader (Easy Line)", "Genesys Logic Inc. 30 in 1 Multi USB 2.0 Card Reader", new string[] { "USB-Massenspeichergerät" }, new string[] { "Mass Storage" }));
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("USB 3.0-Multi Card Reader (Transcend)", "Transcend TS-RDF8K Multi USB 3.0 Card Reader", new string[] { "USB-Massenspeichergerät" }, new string[] { "Mass Storage" }));
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("Flash Drive 8GB (Transcend)", "Transcend 8GB", new string[] { "USB-Massenspeichergerät" }, new string[] { "Mass Storage" }));
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("Flash Drive 2GB (Kingston)", "Kingston Technology Company Inc. Flash Drive 2GB", new string[] { "USB-Massenspeichergerät" }, new string[] { "Mass Storage" }));
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("HD-Video Camera (Panasonic)", " Panasonic HDC-SD9", new string[] { "USB-Massenspeichergerät" }, new string[] { "Mass Storage" }));
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("External USB 3.0 Hard disk 5TB (Seagate)", "Seagate Expansion Desk USB Device 5TB external USB3.0 had disk", new string[] { "Per USB angeschlossenes SCSI (UAS)-Massenspeichergerät" }, new string[] { "Mass Storage" }));
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("Android-Smartphone(with sd-card) connected over USB (Wiko)", "Wiko Darknight", new string[] { "DARKNIGHT TMASS Storage USB Device", "USB-Massenspeichergerät" }, new string[] { "Mass Storage", "Mass Storage" }));

            //HID
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("HID-Gamecontroller (Quanta)", "Competition Pro SL-6602", new string[] { "USB-Eingabegerät" }, new string[] { "Human interface device(HID)" }));
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("Joystick (SpeedLink)", "SpeedLink Strike^2 SL-6535", new string[] { "USB-Eingabegerät" }, new string[] { "Human interface device(HID)" }));

            //Mouse
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("Wireless Mouse (Logitech)", "LOGITECH Wireless mini Mouse M187", new string[] { "USB-Verbundgerät", "USB-Eingabegerät", "USB-Eingabegerät" }, new string[] { "Composite device", "Mouse", "Human interface device(HID)" }));
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("Wireless Mouse (Patuoxun)", "Patuoxun Mazer 2500DPI wireless LED gaming mouse", new string[] { "USB-Verbundgerät", "USB-Eingabegerät", "USB-Eingabegerät" }, new string[] { "Composite device", "Keyboard", "Mouse" }));
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("Wired Mouse (Microsoft)", "Microsoft Wheel Mouse Optical", new string[] { "Microsoft Wheel Mouse Optical(USB)" }, new string[] { "Mouse" }));

            //Keyboard
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("Wired Keyboard (Sharkoon)", "Sharkoon Skiller Pro", new string[] { "USB-Verbundgerät", "USB-Eingabegerät", "USB-Eingabegerät", "USB-Eingabegerät" }, new string[] { "Composite Devive", "Human interface device (HID)", "Human interface device (HID)", "Keyboard" }));

            //Mouse Keyboard Combo
            UsbDeviceExamples.Add(new Tuple<string, string, string[], string[]>("Wireless Keyboard and Mouse Combo (Logitech)", "LOGITECH MX 330", new string[] { "USB-Verbundgerät", "USB-Eingabegerät", "USB-Eingabegerät" }, new string[] { "Composite device", "Keyboard", "Mouse" }));
        }

        #endregion

        #region USB Class Informations
        //USB-Geräteklasseninformationen mit Verwendungsbeispielen
        // http://www.usb.org/developers/defined_class
        public static readonly Tuple<string, string, string, string, string, string>[] ClassCodes =
       {
            Tuple.Create("00",  "", "", "Device",  "Unspecified",                      "Device class is unspecified. Usage of the class informations are in the interface descriptors"),
            Tuple.Create("00",  "00","00","",       "Composite Device",                 "USB device that uses multiple USB-classes."),

            Tuple.Create("01",  "", "", "Interface","Audio",                            "All devices and functions for generating or modify audio-signals. Speaker, microphone, sound card, MIDI"),
            Tuple.Create("02",  "", "", "Both",     "Communications and CDC Control",   "Devices that can perform telecommunications and networking functions. Telecommunications devices (analog phones & modems, ISDN terminal adapters, digital phones, COM-port devices). Networking devices (ADSL & cable modems, Ethernet adapters and hubs)"),
            Tuple.Create("03",  "", "", "Interface","Human interface device",           "Mainly used for devices that allow humans to control the operation of computer systems. Input devices like Keyboard, mouse, joystick, trackballs, remote controls and telephone keypads, drawing tablets, data gloves, Simulation devices (steering wheels, pedals, VR input devices), Front-panel controls (buttons, knobs, sliders & switches)"),
            Tuple.Create("03",  "01","01","",       "Keyboard",                         "Keyboard"),
            Tuple.Create("03",  "01","02","",       "Mouse",                            "Mouse"),
            Tuple.Create("05",  "", "", "Interface","Physical Interface Device",        "Physical Force feedback devices, force feedback joystick"),
            Tuple.Create("06",  "", "", "Interface","Still Imaging",                    "Webcam, Camera, scanner"),
            Tuple.Create("07",  "", "", "Interface","Printer",                          "General print-output (Laser printer, inkjet printer), plotter, CNC machine"),
            Tuple.Create("08",  "", "", "Interface","Mass storage",                     "Used for devices that allow access to their internal data storage. USB flash drive, flash memory card reader (CF-, SD), digital audio player, digital camera, external hard drives, external optical drives (CD or DVD), high end media player (MP3), Portable Flash memory devices, Solid-state drives (SSD), Card readers, Mobile phones"),
            Tuple.Create("09",  "", "", "Device",   "USB Hub",                          "Full bandwidth hub"),
            Tuple.Create("0A",  "", "", "Interface","CDC-Data",                         "Used together with class 02h (Communications and CDC control)"),
            Tuple.Create("0B",  "", "", "Interface","Smart-Cards (Chip-Cards)",         "USB smart card readers"),
            Tuple.Create("0D",  "", "", "Interface","Content security",                 "Interface to devices/functions for usage with protecded content (Fingerprint reader)"),
            Tuple.Create("0E",  "", "", "Interface","Video",                            "All devices/functions to generate or modify video signals (Webcam, Digital camcorders, digital still cameras that support video streaming)"),
            Tuple.Create("0E",  "03", "00", "",     "Webcam",                           "Webcam"),
            Tuple.Create("0F",  "", "", "Interface","Personal Healthcare",              "Pulse monitor (watch), Heart rate monitor, glucose meter"),
            Tuple.Create("10",  "", "", "Interface","Audio/Video",                      "Webcam, TV"),
            Tuple.Create("11",  "", "", "Device",   "Billboard Device",                 "Either a standalone USB device that adheres to this class specification or a Device Container that exposes other USB functionality but includes the Billboard Descriptors as part of its complete BOS descriptor"),
            Tuple.Create("12",  "", "", "Interface","USB Type-C Bridge Class",          "This base class is defined for devices that conform to the USB Type-C Bridge Device Class Specification"),
            Tuple.Create("DC",  "", "", "Both",     "Diagnostic Device",                "USB compliance testing device"),
            Tuple.Create("E0",  "", "", "Interface","Wireless Controller",              "Wireless USB-adapter, Bluetooth adapter, Microsoft RNDIS"),
             Tuple.Create("E0",  "01", "01", "",    "Bluetooth Device",                 "Bluetooth Device"),
            Tuple.Create("EF",  "", "", "Both",     "Miscellaneous",                    "ActiveSync- /Palm Sync-devices (PDA, handheld), test and measurement devices"),
            Tuple.Create("FE",  "", "", "Interface","Application-specific",             "USB-IrDA Bridge, Test & Measurement Class(USBTMC), USB DFU(Direct Firmware Update)"),
            Tuple.Create("FF",  "", "", "Both",     "Vendor-specific",                  "Indicates that a device needs vendor-specific drivers. Doesn’t fit into any other predefined class or one that doesn’t use the standard protocols for an existing class.")

        };
        #endregion

        #region ServiceCodes
        //Treiberbezeichnungen für bestimmte USB-Hubs
        private static readonly Tuple<string, string>[] ServiceCodes =
        {
            Tuple.Create("Enhanced USB-Controller" ,"usbehci"),
            Tuple.Create("USB-Hub","usbhub"),
            Tuple.Create("USB-Host Controller","usbxhci"),
            Tuple.Create("USB-Hub","usbhub3")
        };

        #endregion

        // USB-Geräte GUID-Codes und Gerätebeschreibung
        //Guid siehe https://msdn.microsoft.com/en-us/library/windows/hardware/ff553426(v=vs.85).aspx
        #region GUID Codes
        public static readonly Tuple<string, string, string>[] GuidCodes =
            {             //   Class    	GUID    	                                Device Description
            Tuple.Create("CDROM",           "{4D36E965-E325-11CE-BFC1-08002BE10318}",   "CD/DVD/Blu-ray drives, including SCSI CD-ROM drives. By default, the system's CD-ROM class installer also installs a system-supplied CD audio driver and CD-ROM changer driver as Plug and Play filters."),
            //Tuple.Create("Monitor",       "{4d36e96e-e325-11ce-bfc1-08002be10318}",   "This class includes display monitors. An INF for a device of this class installs no device driver(s), but instead specifies the features of a particular monitor to be stored in the registry for use by drivers of video adapters. (Monitors are enumerated as the child devices of display adapters"),
            Tuple.Create("Battery",         "{72631E54-78A4-11D0-BCF7-00AA00B7B32A}",   "Battery devices and UPS devices."),
            Tuple.Create("Biometric",       "{53D29EF7-377C-4D14-864B-EB3A85769359}",   "All biometric-based personal identification devices."),
            Tuple.Create("Bluetooth Devices","{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}",  "Bluetooth devices"),
            Tuple.Create("DiskDrive",       "{4D36E967-E325-11CE-BFC1-08002BE10318}",   "Hard drives, USB-Flash drive"),
            Tuple.Create("Display",         "{4D36E968-E325-11CE-BFC1-08002BE10318}",   "Video adapters"),
            Tuple.Create("FDC",             "{4D36E969-E325-11CE-BFC1-08002BE10318}",   "Floppy controllers"),
            Tuple.Create("FloppyDisk",      "{4D36E980-E325-11CE-BFC1-08002BE10318}",   "Floppy drives"),
            Tuple.Create("GPS",             "{6BDD1FC3-810F-11D0-BEC7-08002BE2092F}",   "GNSS devices that use the Universal Windows driver model introduced in Windows 10."),
            Tuple.Create("HDC",             "{4D36E96A-E325-11CE-BFC1-08002BE10318}",   "Hard drive controllers"),
            Tuple.Create("HIDClass",        "{745A17A0-74D3-11D0-B6FE-00A0C90F57DA}",   "Interactive input devices that are operated by the system-supplied HID class driver. This includes USB devices that comply with the USB HID Standard and non-USB devices that use a HID minidriver. It specifies a device class (a type of computer hardware) for human interface devices such as keyboards, mice, game controllers and alphanumeric display devices"),
            Tuple.Create("Dot4",            "{48721B56-6795-11D2-B1A8-0080C72E74A2}",   "That control the operation of multifunction IEEE 1284.4 peripheral devices."),
            Tuple.Create("Dot4Print",       "{49CE6AC8-6F86-11D2-B1E5-0080C72E74A2}",   "Dot4 print functions."),
            Tuple.Create("61883",           "{7EBEFBC0-3200-11D2-B4C2-00A0C9697D07}",   "IEEE 1394 devices that support the IEC-61883 protocol device class. The 61883 component includes the 61883.sys protocol driver that transmits various audio and video data streams over the 1394 bus."),
            Tuple.Create("1394",            "{6BDD1FC1-810F-11D0-BEC7-08002BE2092F}",   "IEEE 1394 host controller"),
            Tuple.Create("Smart Card Readers","{50DD5230-BA8A-11D1-BF5D-0000F805F530}","Smart card readers"),
            Tuple.Create("Image",          "{6BDD1FC6-810F-11D0-BEC7-08002BE2092F}",   "Still-image capture devices like digital cameras, and scanners"),
            Tuple.Create("IrDA Devices",   "{6BDD1FC5-810F-11D0-BEC7-08002BE2092F}",   "Infrared devices."),
            Tuple.Create("Modem ",         "{4D36E96D-E325-11CE-BFC1-08002BE10318}",   "Modem devices"),
            Tuple.Create("Multifunction Devices","{4D36E971-E325-11CE-BFC1-08002BE10318}","Combo cards, such as a PCMCIA modem and netcard adapter."),
            Tuple.Create("Keyboard",       "{4D36E96B-E325-11CE-BFC1-08002BE10318}",   "Keyboards"),
             Tuple.Create("Mouse",         "{4D36E96F-E325-11CE-BFC1-08002BE10318}",   "Mice and pointing devices"),
            Tuple.Create("Media",          "{4D36E96C-E325-11CE-BFC1-08002BE10318}",   "Audio and video devices"),
            Tuple.Create("Net",            "{4D36E972-E325-11CE-BFC1-08002BE10318}",   "Network adapters"),
            //Tuple.Create("Ports",	    "{4D36E978-E325-11CE-BFC1-08002BE10318}",	"Serial and parallel ports"),
            Tuple.Create("SCSIAdapter",    "{4D36E97B-E325-11CE-BFC1-08002BE10318}",   "SCSI and RAID controllers"),
            Tuple.Create("Storage",        "{71A27CDD-812A-11D0-BEC7-08002BE2092F}",   "Storage devices."),
            Tuple.Create("System",         "{4D36E97D-E325-11CE-BFC1-08002BE10318}",   "For System buses, bridges, etc."),
            Tuple.Create("Unknown",        "{4D36E97E-E325-11CE-BFC1-08002BE10318}",   "(Reserved for system use) For  Enumerated devices where the system cannot determine the type installed under this class."),
            Tuple.Create("USB",            "{36FC9E60-C465-11CF-8056-444553540000}",   "USB Drivers for this class are system-supplied."),
            Tuple.Create("USB Device",     "{88BAE032-5A81-49f0-BC3D-A4FF138216D6}",   "All the USB devices that don't belong to another USB-Class. Not used for the USB host controllers and the USB hubs."),
            Tuple.Create("USB Device",     "{A5DCBF10-6530-11D2-901F-00C04FB951ED}",   "Devices that are attached to a USB hub."),
            Tuple.Create("USB HC",         "{3ABF6F2D-71C4-462A-8A92-1E6861E6AF27}",   "USB Host controller devices."),
            Tuple.Create("USB HUB",        "{F18A0E88-C30C-11D0-8815-00A0C906BED8}",   "USB hub devices."),
            Tuple.Create("WDP",            "{EEC5AD98-8080-425F-922A-DABF3DE3F69A}",   "WDP = Windows Portable Devices.")
        };
        #endregion


        #region Accepted GUID List
        //Akzeptierte GUID-Codes
        public static readonly List<Guid> AcceptedGuid = new List<Guid>
            {
              //new Guid("{4D36E965-E325-11CE-BFC1-08002BE10318}"), // CD/DVD/Blu-ray drives 
              //new Guid("{E0CBF06C-CD8B-4647-BB8A-263B43F0f974}"), //"This class includes all Bluetooth devices"),
              //new Guid("{4D36E967-E325-11CE-BFC1-08002BE10318}"), //	"Hard drives"),//second device from usb mass storage
                new Guid("{745A17A0-74D3-11D0-B6FE-00A0C90F57DA}"),	//"The USB human interface device class (USB HID class) is a part of the USB specification for computer peripherals: it specifies a device class (a type of computer hardware) for human interface devices such as keyboards, mice, game controllers and alphanumeric display devices"),
              //new Guid("{50DD5230-BA8A-11D1-BF5D-0000F805F530}"), //"This class includes smart card readers"),
                new Guid("{6BDD1FC6-810F-11D0-BEC7-08002BE2092F}"),	//"This class includes still-image capture devices, digital cameras, and scanners"),
              //new Guid("{6BDD1FC5-810F-11D0-BEC7-08002BE2092F}"),//   "This class includes infrared devices."),
                new Guid("{4D36E96D-E325-11CE-BFC1-08002BE10318}"),//   "Modem devices"),
              //new Guid("{4D36E971-E325-11CE-BFC1-08002BE10318}"),//"This class includes combo cards, such as a PCMCIA modem and netcard adapter."),
                new Guid("{4D36E96B-E325-11CE-BFC1-08002BE10318}"),//	"Keyboards"),
                new Guid("{4D36E96F-E325-11CE-BFC1-08002BE10318}"),//	"Mice and pointing devices"),
              //new Guid("{4D36E96C-E325-11CE-BFC1-08002BE10318}"),//	"Audio and video devices"),
                new Guid("{4D36E972-E325-11CE-BFC1-08002BE10318}"),//	"Network adapters"),
              //new Guid("{71A27CDD-812A-11D0-BEC7-08002BE2092F}"),//   "Storage devices..."),
              //new Guid("{4D36E97D-E325-11CE-BFC1-08002BE10318}"),//	"System buses, bridges, etc."),
                new Guid("{36FC9E60-C465-11CF-8056-444553540000}"),//	"Drivers for this class are system-supplied."),
				new Guid("{4D36E97B-E325-11CE-BFC1-08002BE10318}"),//   "SCSI and RAID controllers"),
              //new Guid("{88BAE032-5A81-49f0-BC3D-A4FF138216D6}"),//   "USBDevice includes all USB devices that do not belong to another class. This class is not used for USB host controllers and hubs."),
                new Guid("{A5DCBF10-6530-11D2-901F-00C04FB951ED}"),//   "USB devices that are attached to a USB hub"),
              //new Guid("{3ABF6F2D-71C4-462A-8A92-1E6861E6AF27}"),//   "The GUID_DEVINTERFACE_USB_HOST_CONTROLLER device interface class is defined for USB host controller devices"),
              //new Guid("{F18A0E88-C30C-11D0-8815-00A0C906BED8}"),//   "The GUID_DEVINTERFACE_USB_HUB device interface class is defined for USB hub devices."),
                new Guid("{EEC5AD98-8080-425F-922A-DABF3DE3F69A}")       //wdp windows portable devices
            };
        #endregion

        /// <summary>
        /// Gefährlich eingestufte Geräteklassen als GUID-Codes
        /// </summary>
        public static readonly List<string> ThreatGuid = new List<string>{"{4D36E96B-E325-11CE-BFC1-08002BE10318}",
                                                                            "{4D36E96F-E325-11CE-BFC1-08002BE10318}",
                                                                            "{4D36E972-E325-11CE-BFC1-08002BE10318}",
                                                                            "{F18A0E88-C30C-11D0-8815-00A0C906BED8}",
                                                                            "{745A17A0-74D3-11D0-B6FE-00A0C90F57DA}"};

        /// <summary>
        /// Gefährlich eingestufte Geräteklassen als Klassen-Codes
        /// </summary>
        public static readonly Tuple<string, string, string, string>[] ThreatClass =
        {
            Tuple.Create("02",  "", "",  "Communications and CDC Control"),
            Tuple.Create("03","00","00","Human interface device (HID)"),
            Tuple.Create("03","01","01","Keyboard"),
            Tuple.Create("03","01","02","Mouse"),
            Tuple.Create("09",  "", "", "USB Hub"),
            Tuple.Create("E0",  "", "", "Wireless Controller")
        };

        /// <summary>
        /// Verdächtig eingestufte Geräteklasse
        /// </summary>
        public static readonly Tuple<string, string, string, string>[] SuspectedClass =
       {
            Tuple.Create("00","00","00","Composite Device")
        };

        /// <summary>
        /// Funktion zum Überprüfen, ob das zu überprüfende Gerät als
        /// verdächtig eingestuft wird.
        /// </summary>
        /// <param name="device">Zu überprüfendes USB-Gerät</param>
        public static bool ContainsSuspectedClass(USBDeviceInfo device)
        {
            bool result = false;

            foreach (var elem in SuspectedClass)
            {
                if (elem.Item1.Equals(device.USB_Class))
                {
                    result = true;
                    break;
                }
            }

            return result;

        }

        /// <summary>
        /// Überprüfung ob es sich um ein HID-Gerät handelt
        /// </summary>
        /// <param name="">Param Description</param>
        public static bool IsHid(string usbClass, string usbSubClass, string usbProtocol, string classGuid)
        {
            bool result = false;
            if (usbClass == "03") { result = true; }
            else if (classGuid.ToUpper() == "{745A17A0-74D3-11D0-B6FE-00A0C90F57DA}") { result = true; }
            return result;
        }

        /// <summary>
        /// Überprüfung ob es sich um ein Verbundgerät handelt
        /// </summary>
        /// <param name="">Param Description</param>
        public static bool IsComposite(string usbClass, string usbSubClass, string usbProtocol, string service)
        {
            bool result = false;
            if (usbClass == "00" && usbSubClass == "00" && usbProtocol == "00")
            {
                result = true;
            }
            else if (service.ToLower() == "usbccgp")
            {
                result = true;
            }
            return result;

        }

        /// <summary>
        /// Überprüfung ob es sich um ein Netzwerkgerät handelt
        /// </summary>
        /// <param name="">Param Description</param>
        public static bool IsNetwork(string usbClass, string usbSubClass, string usbProtocol, string classGuid)
        {
            bool result = false;
            if (usbClass == "E0")
            {
                result = true;
            }
            else if (classGuid.ToUpper() == "{4D36E972-E325-11CE-BFC1-08002BE10318}")
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Überprüfung ob es sich um eine Tastatur handelt
        /// </summary>
        /// <param name="">Param Description</param>
        public static bool IsKeyboard(string usbClass, string usbSubClass, string usbProtocol, string classGuid, string service)
        {
            bool result = false;
            if (usbClass == "03" && usbSubClass == "01" && usbProtocol == "01")
            {
                result = true;
            }
            else if (service.ToLower() == "kbdhid")
            {
                result = true;
            }
            else if (classGuid.ToUpper() == "{4D36E96B-E325-11CE-BFC1-08002BE10318}")
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Überprüfung ob es sich um ein Zeigegerät handelt
        /// </summary>
        /// <param name="">Param Description</param>
        public static bool IsPointingDevice(string usbClass, string usbSubClass, string usbProtocol, string classGuid, string service)
        {
            bool result = false;
            if (usbClass == "03" && usbSubClass == "01" && usbProtocol == "02")
            {
                result = true;
            }
            else if (service.ToLower() == "mouhid")
            {
                result = true;
            }
            else if (classGuid.ToUpper() == "{4D36E96F-E325-11CE-BFC1-08002BE10318}")
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Funktion zum Überprüfen, ob das zu überprüfende Gerät als
        /// gefährlich eingestuft wird.
        /// </summary>
        /// <param name="device">Zu überprüfendes USB-Gerät</param>
        public static bool ContainsThreatClass(USBDeviceInfo device)
        {
            bool result = false;

            foreach (var elem in ThreatClass)
            {
                if (elem.Item1.Equals(device.USB_Class))
                {
                    result = true;
                    break;
                }
            }
            if (!result)
            {
                foreach (var elem in ThreatGuid)
                {
                    if (elem.Equals(device.ClassGuid.ToUpper()))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;

        }

        /// <summary>
        /// Liefert den Index zu vorhandenen Geräteinformationen und Beispielen zurück
        /// </summary>
        /// <param name="">Param Description</param>
        public static int IndexClass(USBDeviceInfo device)
        {
            int result = -1;

            for (int i = 0; i < ClassCodes.Length; i++)
            {
                if (ClassCodes[i].Item1.Equals(device.USB_Class.ToUpper()) && ClassCodes[i].Item2.Equals(device.USB_SubClass.ToUpper()) && ClassCodes[i].Item3.Equals(device.USB_Protocol.ToUpper()))
                {
                    result = i;

                    break;
                }
            }

            return result;

        }

        /// <summary>
        /// Liefert den Gerätetyp zurück
        /// </summary>
        /// <param name="">Param Description</param>
        public static string GetClassDevice(USBDeviceInfo tempDevice)
        {
            string result = "";
            bool found = false;

            if (!string.IsNullOrEmpty(tempDevice.USB_Class) && !string.IsNullOrEmpty(tempDevice.USB_SubClass) && !string.IsNullOrEmpty(tempDevice.USB_Protocol))
            {
                var index = IndexClass(tempDevice);

                if (index > -1)
                {
                    result = ClassCodes[index].Item5;
                }
                else
                {
                    foreach (var elem in ClassCodes)
                    {
                        if (elem.Item1.Equals(tempDevice.USB_Class.ToUpper()))
                        {
                            result = elem.Item5;
                            found = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (var elem in ServiceCodes)
                {
                    if (elem.Item2.ToUpper().Equals(tempDevice.Service.ToUpper()))
                    {
                        result = elem.Item1;
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                foreach (var elem in GuidCodes)
                {
                    if (elem.Item2 == tempDevice.ClassGuid)
                    {
                        result = elem.Item1;
                        found = true;
                        break;
                    }
                }
            }
            return result;

        }
    }
}
