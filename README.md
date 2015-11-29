# Cortana  HomeMatic Light Switch
This is a demo project I created for a workshop on a conference to show how to interact with Windows 10's personal assistant Cortana. It also connects with a home automation systems called HomeMatic and is able to switch a selected light on and off. Either by clicking a button or asking Cortana to do that.

## Technology
The demo app is written in C# and is based on the Unicersal Windows Platform (UWP) framework which allows to deploy apps across all Windows 10 devices including mobile phones and even the Xbox One.

To connect with the HomeMatic system I used my [Rhome](https://github.com/Thepagedot/Rhome) project which provides a C# SDK for several home automation solutions.

## Was has been built
> Coming soon...

## Step-by-step
### 1. Connecting with HomeMatic
To connect with HomeMatic we need a fully functional HomeMatic home automation system to be set up, of course. This contains at least a Central Unit (CCU) and one Switcher that contols a lamp. Make sure that you have the [HomeMatic XML-API](https://github.com/hobbyquaker/XML-API) installed on your system, sothat your app can communicate with it.

To let the app communicate with the HomeMatic central we use the [Rhome framework](https://github.com/Thepagedot/Rhome) which we can install via [NuGet](https://www.nuget.org/packages/Thepagedot.Rhome.HomeMatic).
```
PM> Install-Package Thepagedot.Rhome.HomeMatic -Pre
```

Once we installed the package, we can create a new instance of the `HomeMaticXmlApi` inside the app's main file `App.xaml.cs`. We need to pass the constructor an instance of our central unit with its IP-Address and name. Additianlly we should add an `integet` variable that holds the ID of the selected light.
```c#
sealed partial class App : Application
{
  public static HomeMaticXmlApi HomeMatic;
  public static int SelectedLightId;
  
  public App()
  {
      this.InitializeComponent();
      this.Suspending += OnSuspending;

      // Initialize HomeMatic API
      var ccu = new Ccu("Demo", "192.168.0.14");
      HomeMatic = new HomeMaticXmlApi(ccu);
  }

  // ...
}
```
We can now test our connection by calling the `App.HomeMatic.CheckConnectionAsync()` method. It returns a boolean which indicates whether the connection could be established or not.

### 2. Retrieving information from HomeMatic
One the connection has been established correctly, we can ask the API for available rooms and devices. In our scenario we are just interested in devices that has a switcher because we only want to offer potential lamps to the user which he can select from.

For this, I would recommend to implement the dedicated mehtod `GetAllSwitchersAsync()` in a statc helper class called `HomeMaticTools`. This method loads all devices from the central unit and filters by devices that have a switcher channel.
```c#
public static class HomeMaticTools
{
    public static async Task<List<Switcher>> GetAllSwitchersAsync()
    {
        var devices = await App.HomeMatic.GetDevicesAsync();
        var switchers = new List<Switcher>();

        foreach (var device in devices)
            foreach (var channel in device.ChannelList)
                if (channel is Switcher)
                    switchers.Add((Switcher)channel);

        return switchers;
    }
}
```
Now we can use this mehtod to offer the user a list of devices to select from. To do so, we create a new `SettingsPage` page on that the user can navigate to selecte the lamp he wants to control. There we implement a `ListView` which we fill with the available devices.
```c#
protected override async void OnNavigatedTo(NavigationEventArgs e)
{
    base.OnNavigatedTo(e);

    // Connect to HomeMatic and load devices
    if (await App.HomeMatic.CheckConnectionAsync())
    {
        // Get suitable devices 
        var switchers = await HomeMaticTools.GetAllSwitchersAsync();
        lvDevices.ItemsSource = switchers;
    }
    else
    {
        await new MessageDialog("Connection failed.").ShowAsync();
    }
}
```
As soon as the user selects one of these devices we cath the selection by subscribing to the `ListView`'s `OnSelectionChanged` event. Here we can grab the selected device and set save its ID to the central `SelectedLightId` variable inside the `App` class.
```c#
private void LvDevices_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (e.AddedItems.Any())
    {
        App.SelectedLightId = ((Switcher)e.AddedItems.First()).IseId;
    }            
}
```

### 3. Switching on/off the light
Once the user has selected a light to control, we can tell the HomeMatic system to switch it on and off. For this, we should add a button to the `MainPage` that controls the light. When the user presses the button, we send the regarding command to the cental unit.
In this case, we assume that the light is always off when the app is started. Of course we could also check for the light's state whith the framework, by fetching all devices and checking the state of the selected lamp but this is not part of this sample.
```c#
public static bool IsLightOn;

private async void BtnSwitch_OnClick(object sender, RoutedEventArgs e)
{
    // Check if a light has been selected
    if (App.SelectedLightId == 0)
    {
        await new MessageDialog("You have not selected a light yet. Head over to settings to select one.", "No light selected").ShowAsync();
        return;
    }

    // Negate light state
    IsLightOn = !IsLightOn;

    // Send command to HomeMatic
    await App.HomeMatic.SendChannelUpdateAsync(App.SelectedLightId, IsLightOn);
}
```
### 4. Teaching Cortana new commands
To teach Cortana to work with our app, we need to create a voice commant file, where we define the senteces that Cortana listen to and set in context with our app. For this, the `CortanaLightSwitchCommand.xml`file has been created which defines the command to switch a light on and off. If you need detailled information about the syntax of these commands please head over the [Cortana for Windows developers page](https://dev.windows.com/en-us/cortana).
```xml
<VoiceCommands xmlns="http://schemas.microsoft.com/voicecommands/1.1">
  <CommandSet xml:lang="de" Name="CortanaLightSwitch_de">
    <CommandPrefix>Schalte das Licht</CommandPrefix>
    <Example>Schalte das Licht in der Küche ein.</Example>

    <Command Name="switchLightOn">
      <Example>in der Küche ein</Example>
      <ListenFor>[in][im] [der][dem] {room} {status}</ListenFor>
      <Feedback>Licht in {room} wird {status}geschaltet.</Feedback>
      <Navigate />
    </Command>
    
    <PhraseList Label="room">
      <Item>Küche</Item>
      <Item>Schlafzimmer</Item>
    </PhraseList>

    <PhraseList Label="status">
      <Item>ein</Item>
      <Item>aus</Item>
    </PhraseList>        
  </CommandSet>
</VoiceCommands>
```
Once we've created such a VCD file, we need to register it at Cortana. We do this everytime our app starts in `OnLaunched` event handler of our central `App` class.
```c#
// Register Cortana voice commands
var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///CortanaLightSwitchCommands.xml"));
await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(file);  
```
Now we can activate our app using Cortana. In the next step we need to react on these activations and extract the user's spoken infomation and proceed them.

### 5. Reacting on an app activation by Cortana
Whenever your apps get activated by Cortana she tolds you as a devloper that she did it and passes you some additional information about the activation context and the spoken commands. To access this, we need to override the `OnActivated` event handler inside the `App` class. Here we can differentiate between different `ActivationKind`s. If it is a `VoiceCommand`, we need to react.

We can access the `SemanticInterpretation` of the spoken text to extract the variables like `room` and `status` out of it, that we have defined in the VCD file before. We can wrap these information in a single seperated string and pass them to our MainPage while navigating to it.

```c#
protected override void OnActivated(IActivatedEventArgs args)
{
    base.OnActivated(args);
    string infos = string.Empty;

    // Get voice data
    if (args.Kind == ActivationKind.VoiceCommand)
    {                
        var commandArgs = args as VoiceCommandActivatedEventArgs;
        var speechRecognitionResult = commandArgs.Result;
        var commandName = speechRecognitionResult.RulePath[0];

        switch (commandName)
        {
            case "switchLightOn":
                var room = speechRecognitionResult.SemanticInterpretation.Properties["room"][0];
                var status = speechRecognitionResult.SemanticInterpretation.Properties["status"][0];
                infos =  room + "|" + status;
                break;
        }
    }

    // Prepare frame
    Frame rootFrame = Window.Current.Content as Frame;
    if (rootFrame == null)
    {
        rootFrame = new Frame();
        rootFrame.NavigationFailed += OnNavigationFailed;
        Window.Current.Content = rootFrame;
        RestoreSavedSettings();
    }            

    // Navigate and pass information
    rootFrame.Navigate(typeof(MainPage), infos);
    Window.Current.Activate();
}
```




## What is also included

- Saveing settings
- Automatic pre-selection of the last selected device in settings
- Loading indicators

## Credits
Light Bulb icons by Till Teenck from the Noun Project
