/**
******************************************************************************
* @file	   SplashScreen.xaml.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Zeigt den Ladebildschirm an
******************************************************************************
*/
namespace BadUSB_Firewall
{
    /// <summary>
    /// Interaktionslogik für SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen 
    {
        public SplashScreen()
        {
            DataContext = this;
            InitializeComponent();
        }

        //Liefert die aktuelle Programmversion
        public string VersionNumber
        {
            get
            {
                return "Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
        }
    }
}
