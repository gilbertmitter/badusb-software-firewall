# badusb-software-firewall
## BadUSB-Software-Firewall
A Software-Firewall for preventing attacks of manipulated USB-devices. Actually the application is in development and the GUI is only a prototype. The application is written in C# and is tested under Windows 10 but should also work under Windows 7 and 8.
## Usage
When a USB-device is connected the first time i will be blocked due the application and the the device class and product and vendor id of this device are listened in the GUI of the application. If the device features additional interfaces (composite device) they are also listened. The user can than select if he would allow this device (add to a whitelist), block this device(add o blacklist) or use it till the next connection without adding to any list. For example: if a manipulated USB-flash drive, which is emulating a keyboard is connected, the user will see that this device is a keyboard and not an mass-storage device. There are also more addiditional security functions (preventing port change, identical devices, showing a little windows in the taskbar, showing the amount of connected keyboards, pointing devices, network adapters and so on). Instructions for this will follow later. 
## Build
Build the solution in release mode for 64bit.
## License
This program is provided under an MIT open source license, read the LICENSE.txt file for details.
