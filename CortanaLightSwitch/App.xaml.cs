using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Thepagedot.Rhome.HomeMatic.Models;
using Thepagedot.Rhome.HomeMatic.Services;

namespace CortanaLightSwitch
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static HomeMaticXmlApi HomeMatic;
        public static Switcher SelectedLight;

        private static readonly ApplicationDataContainer Settings = ApplicationData.Current.LocalSettings;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            // Initialize HomeMatic API
            var ccu = new Ccu("Demo", "192.168.0.14");
            HomeMatic = new HomeMaticXmlApi(ccu);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                    // Restore IP-Address
                    var ipAddress = Settings.Values["ipAddress"];
                    if (ipAddress != null)
                    {
                        HomeMatic = new HomeMaticXmlApi(new Ccu("Demo", (string)ipAddress));
                    }

                    // Restore selected light
                    var selectedLightId = Settings.Values["selectedLightId"];
                    if (selectedLightId != null)
                    {
                        var isConnectionValid = await HomeMatic.CheckConnectionAsync();
                        if (isConnectionValid)
                        {
                            var devices = await HomeMatic.GetDevicesAsync();
                            foreach (var device in devices)
                            {
                                SelectedLight = (Switcher)((HomeMaticDevice)device).ChannelList.FirstOrDefault(c => c.IseId == (int)selectedLightId);
                                if (SelectedLight != null)
                                    break;
                            }                            
                        }
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();

            //var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///CortanaLightSwitchCommands_2.xml"));
            //await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(file);
            var commands = Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstalledCommandDefinitions["de"];
            
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            //TODO: Save application state and stop any background activity
            if (HomeMatic != null)
                Settings.Values["ipAddress"] = HomeMatic.Ccu.Address;
            if (SelectedLight != null)
                Settings.Values["selectedLightId"] = SelectedLight.IseId;            

            deferral.Complete();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
            string infos = string.Empty;

            if (args.Kind == ActivationKind.VoiceCommand)
            {                
                var commandArgs = args as VoiceCommandActivatedEventArgs;
                var speechRecognitionResult = commandArgs.Result;

                var commandName = speechRecognitionResult.RulePath[0];
                var textSpoken = speechRecognitionResult.Text;
                //var commandMode = this.SemanticInterpreation("commandMode", speechRecognitionResult);

                switch (commandName)
                {
                    case "switchLightOn":
                        var pronoun = speechRecognitionResult.SemanticInterpretation.Properties["pronoun"][0];
                        var room = speechRecognitionResult.SemanticInterpretation.Properties["room"][0];
                        var status = speechRecognitionResult.SemanticInterpretation.Properties["status"][0];
                        infos =  pronoun + "|" + room + "|" + status;
                        break;
                }
            }

            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }
            rootFrame.Navigate(typeof(MainPage), infos);
            Window.Current.Activate();
        }
    }
}
