using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Thepagedot.Rhome.HomeMatic.Models;
using Thepagedot.Rhome.HomeMatic.Services;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CortanaLightSwitch
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, args) => Frame.GoBack();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Fill TextBox with IP-Address
            tbxAddress.Text = App.HomeMatic.Ccu.Address;

            // Demo
            lvDevices.ItemsSource = new List<HomeMaticDevice> { new HomeMaticDevice("Device 1", 0, ""), new HomeMaticDevice("Device 2", 0, "") };

            // Connect to HomeMatic and load devices
            //var isConnectionValid = await App.HomeMatic.CheckConnectionAsync();
            //if (isConnectionValid)
            //{
            //    var devices = await App.HomeMatic.GetDevicesAsync();                

            //    // Only show switchers in devices list
            //    //var switchers = devices.Where(d => d.)

            //    lvDevices.ItemsSource = devices;
            //}
        }

        private async void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            var isConnectionValid = await App.HomeMatic.CheckConnectionAsync();
            if (isConnectionValid)
            {
                // Load devices for selection   
            }
            else
            {
                // Notify user that the connection failed
                var xml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                var stringElements = xml.GetElementsByTagName("text");
                stringElements[0].AppendChild(xml.CreateTextNode("Connection failed."));
                var toast = new ToastNotification(xml);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
        }

        private async void LvDevices_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!e.AddedItems.Any()) return;

            var selectedDevice  = (HomeMaticDevice)e.AddedItems.First();

            // Check if selected device contains Switchers
            var switcher = selectedDevice.ChannelList.FirstOrDefault(c => c is Switcher);
            if (switcher != null)
            {
                // If so, set first switcher as selected light
                App.SelectedLight = (Switcher)e.AddedItems.First();
            }
            else
            {
                await new MessageDialog("The selected device is no light switcher. Please select another device.", "Wrong device selected").ShowAsync();
                (sender as ListView).SelectedIndex = -1;
            }
        }

        private void tbxAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Check if entered text is IP Address
            App.HomeMatic = new HomeMaticXmlApi(new Ccu("Demo", (sender as TextBlock).Text));

        }
    }
}
