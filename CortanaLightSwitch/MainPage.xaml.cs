using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string && !string.IsNullOrEmpty(((string)e.Parameter)))
            {
                var args = ((string)e.Parameter).Split('|');                
            }
        }

        private void BtnSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            IsLightOn = !IsLightOn;

            if (IsLightOn)
            {
                imgOn.Visibility = Visibility.Visible;
                imgOff.Visibility = Visibility.Collapsed;
            }
            else
            {
                imgOn.Visibility = Visibility.Collapsed;
                imgOff.Visibility = Visibility.Visible;
            }
        }

        private void BtnSettings_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (SettingsPage));
        }
    }
}
