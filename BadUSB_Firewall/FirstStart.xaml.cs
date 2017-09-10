/**
******************************************************************************
* @file	   FirstStart.xaml.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Fenster wird beim allerersten Programmstart angezeigt
******************************************************************************
*/
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace BadUSB_Firewall
{
    /// <summary>
    /// Description
    /// </summary>
    /// <param name="">Param Description</param>
    public class YesPressedArgs : EventArgs
    {
        public bool Pressed { get; set; }

        public YesPressedArgs(bool pressed)
        {
            Pressed = pressed;
        }
    }

    /// <summary>
    /// Description
    /// </summary>
    /// <param name="">Param Description</param>
    public class NoPressedArgs : EventArgs
    {
        public bool Pressed { get; set; }

        public NoPressedArgs(bool pressed)
        {
           Pressed = pressed;
        }
    }

    /// <summary>
    /// Interaction logic for FirstStart.xaml
    /// </summary>
    public partial class FirstStart : INotifyPropertyChanged
    {
        public event EventHandler<YesPressedArgs> EventYesPressed;
        public event EventHandler<NoPressedArgs> EventNoPressed;

        private string _numOfKeyboards;
        private string _numOfPointingDevices;
        private bool _yesPressed = false;
        private bool _noPressed = false;

        /// <summary>
        /// Anzahl der vorhandenene Tastaturen
        /// </summary>
        /// <param name="">Param Description</param>
        private string NumOfKeyboards
        {
            get { return _numOfKeyboards; }
            set
            {
                if (Equals(value, _numOfKeyboards))
                    return;
                _numOfKeyboards = value;
                OnPropertyChanged("NumOfKeyboards");
            }
        }

        /// <summary>
        /// Anzahl der vorhandenen Zeigegeräte
        /// </summary>
        /// <param name="">Param Description</param>
        private string NumOfPointingDevices
        {
            get { return _numOfPointingDevices; }
            set
            {
                if (Equals(value, _numOfPointingDevices))
                    return;
                _numOfPointingDevices = value;
                OnPropertyChanged("NumOfPointingDevices");
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public FirstStart(string keyboards, string pointingDevices)
        {
            NumOfKeyboards = keyboards;
            NumOfPointingDevices = pointingDevices;
            InitializeComponent();
            DataContext = this;
            DisplayText();
        }

        /// <summary>
        /// Öffnet ein Fenster und zeigt den Starttext an.
        /// </summary>
        /// <param name="">Param Description</param>
        private void DisplayText()
        {
            TextWindow.Inlines.Add(new Run("        FIRST START OF THE BADUSB-SOFTWARE-FIREWALL" + Environment.NewLine) { FontSize = 14, FontWeight = FontWeights.DemiBold, Foreground = Brushes.Black });
            TextWindow.Inlines.Add("\nFollowing devices where found.");
            TextWindow.Inlines.Add(new Run("\n " + NumOfKeyboards + " - Keyboard(s)\n " + NumOfPointingDevices + " - Pointing device(s)") { FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brushes.Black });
            if (NumOfPointingDevices == "0")
            {
                TextWindow.Inlines.Add("\nNo Pointing device/s where found. This Application will not work without any pointing devices!!");
            }
            TextWindow.Inlines.Add(new Run("\n\nPlease DISCONNECT all USB-devices that you don't want to have in the\nINITIALLIST before you click the CONTINUE button.") { FontSize = 11 });
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
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public bool YesPressed
        {
            get { return _yesPressed; }
            set
            {
                if (bool.Equals(value, _yesPressed))
                    return;
                _yesPressed = value;
                OnPropertyChanged("YesPressed");
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public bool NoPressed
        {
            get { return _noPressed; }
            set
            {
                if (bool.Equals(value, _noPressed))
                    return;
                _noPressed = value;
                OnPropertyChanged("NoPressed");
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public void yesButton_Click(object sender, RoutedEventArgs e)
        {
            if (EventYesPressed != null) EventYesPressed(this, new YesPressedArgs(true));
            Close();
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public void noButton_Click(object sender, RoutedEventArgs e)
        {
            if (EventNoPressed != null) EventNoPressed(this, new NoPressedArgs(true));
            Close();
        }
    }
}
