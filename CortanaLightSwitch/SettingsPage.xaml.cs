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
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender, args) => { if (Frame.CanGoBack) Frame.GoBack(); };
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Fill TextBox with IP-Address
            tbxAddress.Text = App.HomeMatic.Ccu.Address;

            // Connect to HomeMatic and load devices
            prgProgress.Visibility = Visibility.Visible;
            if (await App.HomeMatic.CheckConnectionAsync())
            {
                lvDevices.ItemsSource = await HomeMaticTools.GetAllSwitchersAsync();
            }
            else
            {
                await new MessageDialog("Connection failed.").ShowAsync();
            }
            prgProgress.Visibility = Visibility.Collapsed;
        }

        private async void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            prgProgress.Visibility = Visibility.Visible;
            if (await App.HomeMatic.CheckConnectionAsync())
            {
                // Load devices for selection
                lvDevices.ItemsSource = await HomeMaticTools.GetAllSwitchersAsync();
            }
            else
            {
                await new MessageDialog("Connection failed.").ShowAsync();
            }
            prgProgress.Visibility = Visibility.Collapsed;
        }

        private void LvDevices_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Any())
            {
                App.SelectedLight = (Switcher)e.AddedItems.First();
            }            
        }

        private void tbxAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Check if entered text is IP Address
            App.HomeMatic = new HomeMaticXmlApi(new Ccu("Demo", (sender as TextBlock).Text));
        }
    }
}
