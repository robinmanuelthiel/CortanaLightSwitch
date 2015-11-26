using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Thepagedot.Rhome.HomeMatic.Models;
using Thepagedot.Rhome.HomeMatic.Services;

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

            // Enable back navigation
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
                var switchers = await HomeMaticTools.GetAllSwitchersAsync();
                lvDevices.ItemsSource = switchers;
                lvDevices.SelectedItem = switchers.FirstOrDefault(s => s.IseId == App.SelectedLightId);
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
                App.SelectedLightId = ((Switcher)e.AddedItems.First()).IseId;
            }            
        }

        private void tbxAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Check if entered text is IP Address
            App.HomeMatic = new HomeMaticXmlApi(new Ccu("Demo", (sender as TextBlock).Text));
        }
    }
}
