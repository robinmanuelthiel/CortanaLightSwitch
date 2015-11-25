using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CortanaLightSwitch
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static bool IsLightOn;

        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!string.IsNullOrEmpty(e.Parameter as string))
            {
                // Check if all paramters for accessing the light are set
                if (App.HomeMatic != null && App.SelectedLight != null)
                {
                    var args = ((string)e.Parameter).Split('|');
                    if (args[1] == "ein")
                        await App.SelectedLight.OnAsync(App.HomeMatic);
                    else
                        await App.SelectedLight.OffAsync(App.HomeMatic);
                }                
            }
        }

        private async void BtnSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            // Check if a light has been selected
            if (App.SelectedLight == null)
            {
                await new MessageDialog("You have not selected a light yet. Head over to settings to select one.", "No light selected").ShowAsync();
                return;
            }

            ((Button)sender).IsEnabled = false;
            IsLightOn = !IsLightOn;

            if (IsLightOn)
            {
                // Send command to HomeMatic
                await App.SelectedLight.OnAsync(App.HomeMatic);

                // Switch images
                imgOn.Visibility = Visibility.Visible;
                imgOff.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Send command to HomeMatic
                await App.SelectedLight.OffAsync(App.HomeMatic);

                // Switch images
                imgOn.Visibility = Visibility.Collapsed;
                imgOff.Visibility = Visibility.Visible;
            }

            ((Button)sender).IsEnabled = true;
        }

        private void BtnSettings_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (SettingsPage));
        }
    }
}
