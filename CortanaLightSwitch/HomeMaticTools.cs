using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thepagedot.Rhome.HomeMatic.Models;

namespace CortanaLightSwitch
{
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
}
