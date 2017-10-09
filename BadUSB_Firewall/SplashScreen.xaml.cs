/**
******************************************************************************
* @file	   SplashScreen.xaml.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Displays the load screen
******************************************************************************
*/
namespace BadUSB_Firewall
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen 
    {
        public SplashScreen()
        {
            DataContext = this;
            InitializeComponent();
        }

        //Returns the current program version
        public string VersionNumber
        {
            get
            {
                return "Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
        }
    }
}
