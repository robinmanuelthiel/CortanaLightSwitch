using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            SystemNavigationManager.GetForCurrentView().BackRequested += delegate(object sender, BackRequestedEventArgs args)
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            };
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
    }
}
