/**
*************************************************************************************
* @file	   ReducedInfoWidget.xaml.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Zeigt ein Fenster mit Informationen in einer reduzierten Darstellungsform
*************************************************************************************
*/
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BadUSB_Firewall
{
    /// <summary>
    /// Interaction logic for ToastWindow.xaml
    /// </summary>
    public partial class ReducedInfoWidget
    {
        private readonly int _mDevices;
        private readonly int _mInterfaces;
        private readonly int _mKeyboards;
        private readonly int _mPointing;
        private readonly int _mNetwork;
        //private string mTitle = "BadUSB-Software-Firewall";
        private Uri _uri;
        private ImageSource _imgSource;

        public ReducedInfoWidget(string imgPath, int devices, int interfaces, int keyboards, int pointing, int network)
        {
            if (File.Exists(Path.Combine(BadUSBFirewall.BaseDir, "Resources", imgPath)))
            {
                //Pfad für das dargestellte Symbol, welches auch in der Hauptanwendung verwendet wird
                _uri = new Uri(Path.Combine(BadUSBFirewall.BaseDir, "Resources", imgPath), UriKind.Absolute);
                _imgSource = new BitmapImage(_uri);
            }
            else
            {
                MessageBox.Show("Image " + imgPath + " not found in Resources directory!", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _mDevices = devices;
            _mInterfaces = interfaces;
            _mKeyboards = keyboards;
            _mPointing = pointing;
            _mNetwork = network;
            InitializeComponent();

            DisplayText();
        }

        /// <summary>
        /// Aktualisiert die Anzahl der dargestellten Zeigegeräte
        /// </summary>
        /// <param name="">Param Description</param>
        public void PointingDevices_Now(int value)
        {
            PointingNowBox.Text = value.ToString();
        }

        /// <summary>
        /// Aktualisiert die Anzahl der dargestellten Tastaturen
        /// </summary>
        /// <param name="">Param Description</param>
        public void Keyboards_Now(int value)
        {
            KeyboardsNowBox.Text = value.ToString();
        }

        /// <summary>
        /// Aktualisiert die Anzahl der dargestellten Netzwerkadapter
        /// </summary>
        /// <param name="">Param Description</param>
        public void NetworkDevices_Now(int value)
        {
            NetworkNowBox.Text = value.ToString();
        }

        /// <summary>
        /// Aktualisiert die Anzahl der zu behandelnden USB-Geräte innerhalb der GUI an.
        /// </summary>
        /// <param name="">Param Description</param>
        public void UsbDevices_Now(int value)
        {
            DevicesNowBox.Text = value.ToString();
        }

        /// <summary>
        /// Aktualisiert die Anzahl der zu behandelnden Schnittstellen in der GUI an.
        /// </summary>
        /// <param name="">Param Description</param>
        public void InterfaceDevices_Now(int value)
        {
            InterfacesNowBox.Text = value.ToString();
        }

        /// <summary>
        /// Ändert das aktuelle Symbol
        /// </summary>
        /// <param name="">Param Description</param>
        public void SetImage(string imgPath)
        {
            if (File.Exists(Path.Combine(BadUSBFirewall.BaseDir, "Resources", imgPath)))
            {
                _uri = new Uri(Path.Combine(BadUSBFirewall.BaseDir, "Resources", imgPath), UriKind.Absolute);
                _imgSource = new BitmapImage(_uri);
                ToastImage.Source = _imgSource;
            }
            else
            {
                MessageBox.Show("Image " + imgPath + " not found in Resources directory!", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void DisplayText()
        {
            ToastImage.Source = _imgSource;
            ToastTitle.Content = Title;
            DevicesNowBox.Text = _mDevices.ToString();
            InterfacesNowBox.Text = _mInterfaces.ToString();
            KeyboardsNowBox.Text = _mKeyboards.ToString();
            PointingNowBox.Text = _mPointing.ToString();
            NetworkNowBox.Text = _mNetwork.ToString();
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
