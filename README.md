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
### 4. Let Cortana do the work
> Coming soon...


## What is also included

- Save settings
- Auto selection of selected device in settings
- Loading indicators

## Credits
Light Bulb icons by Till Teenck from the Noun Project
