// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

// Copyright (C) 2023  SI Xiaolong (https://github.com/Direct5dom) (SIXiaolong_GitHub@outlook.com)

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Globalization;
using Windows.Storage;
using PInvoke;
using Windows.Services.Maps;
using Windows.Networking;
using System.Net.Mail;
using Validation;
using static System.Net.Mime.MediaTypeNames;

namespace WinWoL
{
    public sealed partial class WoL : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public List<string> ConfigSelector { get; } = new List<string>()
        {
            "0","1","2","3","4","5","6","7","8","9","10"
        };
        public WoL()
        {
            this.InitializeComponent();

            if (localSettings.Values["configNum"] == null)
            {
                configNum.SelectedItem = ConfigSelector[0];
                localSettings.Values["configNum"] = ConfigSelector[0];
                refresh("0");
            }
            else
            {
                configNum.SelectedItem = localSettings.Values["configNum"];
                refresh(localSettings.Values["configNum"].ToString());
            }
        }
        private void configNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            refresh(configNum.SelectedItem.ToString());
            localSettings.Values["configNum"] = configNum.SelectedItem;
        }
        private void WoLPC(string ConfigIDNum)
        {
            string configInner = localSettings.Values["ConfigID" + ConfigIDNum] as string;
            if (configInner != null)
            {
                string[] configInnerSplit = configInner.Split(',');
                // configName.Text + "," + macAddress.Text + "," + ipAddress.Text + ":" + ipPort.Text;
                string ConfigID = configInnerSplit[0];
                string macAddress = configInnerSplit[1];
                if (macAddress != null)
                {
                    macAddress = "AA:BB:CC:DD:EE:FF";
                }
                string ipAddress = configInnerSplit[2];
                if (ipAddress != null)
                {
                    ipAddress = "255.255.255.255";
                }
                string ipPort = configInnerSplit[3];
                if (ipPort != null)
                {
                    ipPort = "9";
                }
                sendMagicPacket(macAddress, ipAddress, int.Parse(ipPort));
                ToggleThemeTeachingTip2.IsOpen = true;
            }
        }
        private void refresh(string ConfigIDNum)
        {
            List<ConfigItem> items = new List<ConfigItem>();
            string configInner = localSettings.Values["ConfigID" + ConfigIDNum] as string;
            if (configInner != null)
            {
                string[] configInnerSplit = configInner.Split(',');
                // configName.Text + "," + macAddress.Text + "," + ipAddress.Text + ":" + ipPort.Text;
                string configName = configInnerSplit[0];
                string macAddress = configInnerSplit[1];
                string ipAddress = configInnerSplit[2];
                string ipPort = configInnerSplit[3];

                items.Add(new ConfigItem(
                    "??????????" + configName,
                    "???? Mac??" + macAddress,
                    "???? IP??" + ipAddress,
                    "??????????" + ipPort
                    ));

                AddConfig.Content = "????????";
                DelConfig.IsEnabled = true;
                WoLConfig.IsEnabled = true;
            }
            else
            {
                items.Add(new ConfigItem(
                    "??????????",
                    "???? Mac??",
                    "???? IP??",
                    "??????????"
                    ));
                AddConfig.Content = "????????";
                DelConfig.IsEnabled = false;
                WoLConfig.IsEnabled = false;
            }
            MyGridView.ItemsSource = items;
        }
        private void delConfig(string ConfigIDNum)
        {
            localSettings.Values["ConfigID" + ConfigIDNum] = null;
        }
        private async void AddConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configNum.SelectedItem.ToString();
            localSettings.Values["ConfigIDTemp"] = localSettings.Values["ConfigID" + ConfigIDNum];

            AddConfigDialog configDialog = new AddConfigDialog();

            configDialog.XamlRoot = this.XamlRoot;
            configDialog.Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            if (AddConfig.Content.ToString() == "????????")
            {
                configDialog.PrimaryButtonText = "????";
            }
            else
            {
                configDialog.PrimaryButtonText = "????";
            }
            configDialog.CloseButtonText = "????";
            configDialog.DefaultButton = ContentDialogButton.Primary;

            var result = await configDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                localSettings.Values["ConfigID" + ConfigIDNum] = localSettings.Values["ConfigIDTemp"];
                refresh(ConfigIDNum);
            }
        }
        private void DelConfigButton_Click(object sender, RoutedEventArgs e)
        {
            delConfig(configNum.SelectedItem.ToString());
            refresh(configNum.SelectedItem.ToString());
            if (this.DelConfig.Flyout is Flyout f)
            {
                f.Hide();
            }
        }
        private void WoLConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configNum.SelectedItem.ToString();
            WoLPC(ConfigIDNum);
        }
        // ??UDP????????MagicPacket
        public void sendMagicPacket(string macAddress, string domain, int port)
        {
            // ??string????????????????????????
            string s = macAddress;
            // hexStrings = {"11", "22", "33", "44", "55", "66"}
            string[] hexStrings = s.Split(':');
            // ????????byte????
            byte[] bytes = new byte[hexStrings.Length];
            // ??????????????????????????????????byte????????????byte??????
            for (int i = 0; i < hexStrings.Length; i++)
            {
                // ????16????????????????????????
                bytes[i] = Convert.ToByte(hexStrings[i], 16);
            }
            // ????????UDP Socket????
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // ????????????????
            socket.EnableBroadcast = true;
            // ??MAC????????????????????byte[] mac = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 };
            byte[] mac = bytes;
            // ??????????????
            byte[] packet = new byte[17 * 6];
            // ??????6????????0xFF
            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;
            // ????????16????????MAC????????
            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];
            // ????IP????
            IPAddress ip;
            if (IPAddress.TryParse(domain, out ip))
            {
                // ??IP
                ip = IPAddress.Parse(domain);
            }
            else
            {
                // ??????
                ip = Dns.GetHostEntry(domain).AddressList[0];
            }
            // ????????
            socket.SendTo(packet, new IPEndPoint(ip, port));
            // ????Socket????
            socket.Close();
        }
    }
    // ConfigItem??
    public class ConfigItem
    {
        // ????????ID
        public string ConfigName { get; set; }
        // ????MAC
        public string MacAddress { get; set; }
        // ????IP
        public string IpAddress { get; set; }
        // ????????
        public string IpPort { get; set; }
        public ConfigItem(string configName, string macAddress, string ipAddress, string ipPort)
        {
            ConfigName = configName;
            MacAddress = macAddress;
            IpAddress = ipAddress;
            IpPort = ipPort;
        }
    }
}
