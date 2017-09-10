/**
******************************************************************************
* @file	   FirewallRules.xaml.cs
* @author  Mitter Gilbert
* @version V1.0.0
* @date    26.04.2017
* @brief   Administrative Geräteregelverwaltung über eigenes Fenster
******************************************************************************
*/
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BadUSB_Firewall
{
    /// <summary>
    /// Description
    /// </summary>
    /// <param name="">Param Description</param>
    public class ApplyChangesPressedArgs : System.EventArgs
    {
        public bool Pressed { get; set; }

        public ApplyChangesPressedArgs(bool pressed)
        {
            Pressed = pressed;
        }
    }

    /// <summary>
    /// Interaction logic for FirewallRules.xaml
    /// </summary>
    public partial class FirewallRules : INotifyPropertyChanged
    {
        public event System.EventHandler<ApplyChangesPressedArgs> EventApplyChangesPressed;
        private static readonly DeviceLists MyDeviceClass = new DeviceLists();
        private ObservableCollection<DeviceEntry> _myDeviceList = new ObservableCollection<DeviceEntry>();

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="">Param Description</param>
        public FirewallRules()
        {
            if (_myDeviceList != null)
            {
                //Vorhandene Geräteliste einholen
                MyDeviceList = MyDeviceClass.GetDeviceList();
                InitializeComponent();
                DataContext = this;
                ApplyButton.IsEnabled = false;
            }
            else
            {
                //error
                MessageBox.Show("Could not open the BadUSB-Firewall Rule window.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public ObservableCollection<DeviceEntry> MyDeviceList
        {
            get
            {
                return _myDeviceList;
            }
            set
            {
                if (Equals(value, _myDeviceList))
                    return;
                _myDeviceList = value;
                OnPropertyChanged("MyDeviceList");
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
        /// "Apply changes" Knopf wurde gedrückt
        /// </summary>
        /// <param name="">Param Description</param>
        public void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (EventApplyChangesPressed != null) EventApplyChangesPressed(this, new ApplyChangesPressedArgs(true));
            Close();
        }

        /// <summary>
        /// Schließen wurde gedrückt. Geänderte Werte zurücksetzen.
        /// </summary>
        /// <param name="">Param Description</param>
        public void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < MyDeviceList.Count; i++)
            {
                if (MyDeviceList[i].Added_BlackList && MyDeviceList[i].In_BlackList == false) { MyDeviceList[i].In_BlackList = true; }
                if (MyDeviceList[i].Added_WhiteList && MyDeviceList[i].In_WhiteList == false) { MyDeviceList[i].In_WhiteList = true; }
                if (MyDeviceList[i].Remove_Device) { MyDeviceList[i].Remove_Device = false; }
            }
            Close();
        }

        /// <summary>
        /// Description
        /// </summary>
        /// <param name="">Param Description</param>
        public static T FindAncestorOrSelf<T>(DependencyObject obj)
        where T : DependencyObject
        {
            while (obj != null)
            {
                T objTemp = obj as T;

                if (objTemp != null)
                    return objTemp;

                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }

        #region CheckBox_Selections
        /// <summary>
        /// Checkbox zum Blockieren (in die Blacklist hinzufügen) wurde gewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void BlockBox_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem lvItem = FindAncestorOrSelf<ListViewItem>(sender as CheckBox);
            ListView listView = ItemsControl.ItemsControlFromItemContainer(lvItem) as ListView;

            if (listView != null)
            {
                int index = listView.ItemContainerGenerator.IndexFromContainer(lvItem);
                
                if (index > -1 && index < MyDeviceList.Count)
                {
                    MyDeviceList[index].In_WhiteList = false;
                    MyDeviceList[index].Remove_Device = false;
                    if (!MyDeviceList[index].Added_BlackList && !MyDeviceList[index].Remove_Device)
                    {
                        MyDeviceList[index].In_BlackList = true;
                        if (!ApplyButton.IsEnabled) { ApplyButton.IsEnabled = true; }
                    }

                    var results = MyDeviceList.Where(p => p != MyDeviceList[index] && p.mHardwareID.Contains(MyDeviceList[index].mHardwareID));
                    foreach (DeviceEntry device in results)
                    {
                        device.In_WhiteList = MyDeviceList[index].In_WhiteList;
                        device.Remove_Device = MyDeviceList[index].Remove_Device;
                        device.In_BlackList = MyDeviceList[index].In_BlackList;
                    }
                }
            }
        }


        /// <summary>
        /// Checkbox zum Blockieren (in die Blacklist hinzufügen) wurde abgewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void BlockBoxUncheck_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem lvItem = FindAncestorOrSelf<ListViewItem>(sender as CheckBox);
            ListView listView = ItemsControl.ItemsControlFromItemContainer(lvItem) as ListView;
            bool changed = false;
            if (listView != null)
            {
                int index = listView.ItemContainerGenerator.IndexFromContainer(lvItem);
                if (index > -1 && index < MyDeviceList.Count)
                {
                    if (MyDeviceList[index].Added_BlackList && !MyDeviceList[index].In_WhiteList && !MyDeviceList[index].Remove_Device)
                    {
                        MyDeviceList[index].In_BlackList = true;
                        MyDeviceList[index].In_WhiteList = false;
                        MyDeviceList[index].Remove_Device = false;
                        changed = true;
                        if (!ApplyButton.IsEnabled) { ApplyButton.IsEnabled = false; }

                    }
                    else if (MyDeviceList[index].In_WhiteList)
                    {
                        changed = true;
                        MyDeviceList[index].In_BlackList = false;
                    }
                    else if (!MyDeviceList[index].In_WhiteList && !MyDeviceList[index].Remove_Device)
                    {
                        if (MyDeviceList[index].Added_BlackList)
                        {
                            changed = true;
                            MyDeviceList[index].In_BlackList = true;
                        }
                        else if (MyDeviceList[index].Added_WhiteList)
                        {
                            changed = true;
                            MyDeviceList[index].In_WhiteList = true;
                        }
                    }

                    if (changed)
                    {
                        var results = MyDeviceList.Where(p => p != MyDeviceList[index] && p.mHardwareID.Contains(MyDeviceList[index].mHardwareID));
                        foreach (DeviceEntry device in results)
                        {
                            device.In_WhiteList = MyDeviceList[index].In_WhiteList;
                            device.Remove_Device = MyDeviceList[index].Remove_Device;
                            device.In_BlackList = MyDeviceList[index].In_BlackList;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checkbox zum Erlauben (in die Whitelist hinzufügen) wurde gewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void AllowBox_Click(object sender, RoutedEventArgs e)
        {

            ListViewItem lvItem = FindAncestorOrSelf<ListViewItem>(sender as CheckBox);
            ListView listView = ItemsControl.ItemsControlFromItemContainer(lvItem) as ListView;
            if (listView != null)
            {
                int index = listView.ItemContainerGenerator.IndexFromContainer(lvItem);

                if (index > -1 && index < MyDeviceList.Count)
                {
                    MyDeviceList[index].In_BlackList = false;
                    MyDeviceList[index].Remove_Device = false;
                    if (!MyDeviceList[index].Added_WhiteList && !MyDeviceList[index].Remove_Device)
                    {
                        MyDeviceList[index].In_WhiteList = true;
                        if (!ApplyButton.IsEnabled) { ApplyButton.IsEnabled = true; }
                    }

                    var results = MyDeviceList.Where(p => p != MyDeviceList[index] && p.mHardwareID.Contains(MyDeviceList[index].mHardwareID));
                    foreach (DeviceEntry device in results)
                    {
                        device.In_WhiteList = MyDeviceList[index].In_WhiteList;
                        device.Remove_Device = MyDeviceList[index].Remove_Device;
                        device.In_BlackList = MyDeviceList[index].In_BlackList;
                    }
                }
            }
        }

        /// <summary>
        /// Checkbox zum Erlauben (in die Whitelist hinzufügen) wurde abgewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void AllowBoxUncheck_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem lvItem = FindAncestorOrSelf<ListViewItem>(sender as CheckBox);
            ListView listView = ItemsControl.ItemsControlFromItemContainer(lvItem) as ListView;
            bool changed = false;
            if (listView != null)
            {
                int index = listView.ItemContainerGenerator.IndexFromContainer(lvItem);
                if (index > -1 && index < MyDeviceList.Count)
                {
                    if (MyDeviceList[index].Added_WhiteList && !MyDeviceList[index].In_BlackList && !MyDeviceList[index].Remove_Device)
                    {
                        MyDeviceList[index].In_WhiteList = true;
                        MyDeviceList[index].In_BlackList = false;
                        MyDeviceList[index].Remove_Device = false;
                        changed = true;
                        if (!ApplyButton.IsEnabled) { ApplyButton.IsEnabled = false; }

                    }
                    else if (MyDeviceList[index].In_BlackList)
                    {
                        changed = true;
                        MyDeviceList[index].In_WhiteList = false;
                    }
                    else if (!MyDeviceList[index].In_BlackList && !MyDeviceList[index].Remove_Device)
                    {
                        if (MyDeviceList[index].Added_BlackList)
                        {
                            changed = true;
                            MyDeviceList[index].In_BlackList = true;
                        }
                        else if (MyDeviceList[index].Added_WhiteList)
                        {
                            changed = true;
                            MyDeviceList[index].In_WhiteList = true;
                        }
                    }
                    if (changed)
                    {
                        var results = MyDeviceList.Where(p => p != MyDeviceList[index] && p.mHardwareID.Contains(MyDeviceList[index].mHardwareID));
                        foreach (DeviceEntry device in results)
                        {
                            device.In_WhiteList = MyDeviceList[index].In_WhiteList;
                            device.Remove_Device = MyDeviceList[index].Remove_Device;
                            device.In_BlackList = MyDeviceList[index].In_BlackList;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checkbox zum Entfernen wurde gewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void RemoveBox_Click(object sender, RoutedEventArgs e)
        {

            if (!ApplyButton.IsEnabled) { ApplyButton.IsEnabled = true; }

            ListViewItem lvItem = FindAncestorOrSelf<ListViewItem>(sender as CheckBox);
            ListView listView = ItemsControl.ItemsControlFromItemContainer(lvItem) as ListView;
            if (listView != null)
            {
                int index = listView.ItemContainerGenerator.IndexFromContainer(lvItem);
                if (index > -1 && index < MyDeviceList.Count)
                {
                    MyDeviceList[index].Remove_Device = true;
                    MyDeviceList[index].In_BlackList = false;
                    MyDeviceList[index].In_WhiteList = false;

                    var results = MyDeviceList.Where(p => p != MyDeviceList[index] && p.mHardwareID.Contains(MyDeviceList[index].mHardwareID));
                    foreach (DeviceEntry device in results)
                    {
                        device.In_WhiteList = MyDeviceList[index].In_WhiteList;
                        device.Remove_Device = MyDeviceList[index].Remove_Device;
                        device.In_BlackList = MyDeviceList[index].In_BlackList;
                    }
                }
            }
        }

        /// <summary>
        /// Checkbox zum Entfernen wurde abgewählt.
        /// </summary>
        /// <param name="">Param Description</param>
        private void RemoveBoxUncheck_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem lvItem = FindAncestorOrSelf<ListViewItem>(sender as CheckBox);
            ListView listView = ItemsControl.ItemsControlFromItemContainer(lvItem) as ListView;

            if (listView != null)
            {
                int index = listView.ItemContainerGenerator.IndexFromContainer(lvItem);
                if (index > -1 && index < MyDeviceList.Count)
                {
                    if (MyDeviceList[index].Added_BlackList && MyDeviceList[index].In_BlackList)
                    {
                        MyDeviceList[index].In_WhiteList = false;
                        MyDeviceList[index].Remove_Device = false;
                    }
                    else if (MyDeviceList[index].Added_BlackList && MyDeviceList[index].In_WhiteList)
                    {
                        MyDeviceList[index].In_BlackList = false;
                        MyDeviceList[index].Remove_Device = false;
                    }
                    else if (MyDeviceList[index].Added_WhiteList && MyDeviceList[index].In_WhiteList)
                    {
                        MyDeviceList[index].In_BlackList = false;
                        MyDeviceList[index].Remove_Device = false;
                    }
                    else if (MyDeviceList[index].Added_WhiteList && MyDeviceList[index].In_BlackList)
                    {
                        MyDeviceList[index].In_WhiteList = false;
                        MyDeviceList[index].Remove_Device = false;
                    }
                    else
                    {

                        MyDeviceList[index].Remove_Device = false;
                        if (MyDeviceList[index].Added_WhiteList)
                        {
                            MyDeviceList[index].In_WhiteList = true;
                        }
                        else if (MyDeviceList[index].Added_BlackList)
                        {
                            MyDeviceList[index].In_BlackList = true;
                        }
                    }

                    var results = MyDeviceList.Where(p => p != MyDeviceList[index] && p.mHardwareID.Contains(MyDeviceList[index].mHardwareID));
                    foreach (DeviceEntry device in results)
                    {
                        device.In_WhiteList = MyDeviceList[index].In_WhiteList;
                        device.Remove_Device = MyDeviceList[index].Remove_Device;
                        device.In_BlackList = MyDeviceList[index].In_BlackList;
                    }
                }
            }
        }
        #endregion
    }
}
