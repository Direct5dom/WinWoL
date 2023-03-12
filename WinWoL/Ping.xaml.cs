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
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace WinWoL
{
    public sealed partial class Ping : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public List<string> ConfigSelector { get; } = new List<string>()
        {
            "0","1","2","3","4","5","6","7","8","9","10"
        };
        public Ping()
        {
            this.InitializeComponent();
            if (localSettings.Values["pingNum"] == null)
            {
                configNum.SelectedItem = ConfigSelector[0];
                localSettings.Values["pingNum"] = ConfigSelector[0];
                refresh("0");
            }
            else
            {
                configNum.SelectedItem = localSettings.Values["pingNum"];
                refresh(localSettings.Values["pingNum"].ToString());
            }
        }
        private void configNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            refresh(configNum.SelectedItem.ToString());
            localSettings.Values["pingNum"] = configNum.SelectedItem;
        }
        public void pingTest(string pingHostPort, string pingHostName)
        {
            // ��������ַ������ѳ� IP/���� �� �˿�
            string[] pingHostPortSplit = pingHostPort.Split(':');
            string pingHost = pingHostPortSplit[0];
            int port = int.Parse(pingHostPortSplit[1]);
            // Ping ʵ������
            System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
            // Ping ѡ��
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            string data = "ping test data";
            byte[] buf = Encoding.ASCII.GetBytes(data);
            // ����ͬ�� Send �����������ݣ�������� reply ����;
            PingReply reply = pingSender.Send(pingHost, 120, buf, options);
            // �ж� replay���Ƿ���ͨ
            if (reply.Status == IPStatus.Success)
            {
                List<Item> items = new List<Item>();
                items.Add(new Item(
                    "����������" + pingHostName,
                    "����IP��" + reply.Address.ToString(),
                    "����ʱ��RTT��" + reply.RoundtripTime.ToString() + " ms",
                    port + " �˿ڿ��������" + checkPortEnable(pingHost, port).ToString()
                    ));
                MyGridView.ItemsSource = items;
            }
            else
            {
                List<Item> items = new List<Item>();
                items.Add(new Item(
                    "����������" + pingHostName,
                    "����IP��" + reply.Address.ToString(),
                    "����ʱ��RTT����ʱ",
                    port + " �˿ڿ����������ʱ"
                    ));
                MyGridView.ItemsSource = items;
            }
        }
        private bool checkPortEnable(string _ip, int _port)
        {
            //��IP�Ͷ˿��滻��Ϊ��Ҫ����
            string ipAddress = _ip;
            int portNum = _port;

            // ��ȡIP��ַ
            IPAddress ip;
            if (IPAddress.TryParse(ipAddress, out ip))
            {
                // ��IP
                ip = IPAddress.Parse(ipAddress);
            }
            else
            {
                // ������
                ip = Dns.GetHostEntry(ipAddress).AddressList[0];
            }

            IPEndPoint point = new IPEndPoint(ip, portNum);

            bool _portEnable = false;
            try
            {
                using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    sock.Connect(point);
                    sock.Close();

                    _portEnable = true;
                }
            }
            catch
            {
                _portEnable = false;
            }
            return _portEnable;
        }
        private async void AddConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string pingNum = configNum.SelectedItem.ToString();
            localSettings.Values["pingConfigTemp"] = localSettings.Values["pingConfig" + pingNum];

            AddPingDialog configDialog = new AddPingDialog();

            configDialog.XamlRoot = this.XamlRoot;
            configDialog.Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            if (AddConfig.Content.ToString() == "�޸�����")
            {
                configDialog.PrimaryButtonText = "�޸�";
            }
            else
            {
                configDialog.PrimaryButtonText = "���";
            }
            configDialog.CloseButtonText = "�ر�";
            configDialog.DefaultButton = ContentDialogButton.Primary;

            var result = await configDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                localSettings.Values["pingConfig" + pingNum] = localSettings.Values["pingConfigTemp"];
                refresh(pingNum);
            }
        }
        private void refresh(string pingNum)
        {
            string configInner = localSettings.Values["pingConfig" + pingNum] as string;
            if (configInner != null)
            {
                string[] configInnerSplit = configInner.Split(',');
                // configName.Text + "," + ipAddress.Text + "," + ipPort.Text;
                string configName = configInnerSplit[0];
                string ipAddress = configInnerSplit[1];
                string ipPort = configInnerSplit[2];

                pingTest(ipAddress + ":" + ipPort, configName);

                AddConfig.Content = "�޸�����";
            }
            else
            {
                List<Item> items = new List<Item>();
                items.Add(new Item(
                    "����������",
                    "����IP��",
                    "����ʱ��RTT��",
                    "[�˿�] �˿ڿ��������"
                    ));
                MyGridView.ItemsSource = items;

                AddConfig.Content = "�������";
            }
        }
        private void delConfig(string PingNum)
        {
            localSettings.Values["pingConfig" + PingNum] = null;
        }
        private void DelConfigButton_Click(object sender, RoutedEventArgs e)
        {
            delConfig(configNum.SelectedItem.ToString());
            refresh(configNum.SelectedItem.ToString());
        }
    }



    public class Item
    {
        // ������
        public string HostName { get; set; }
        // ����IP
        public string HostIP { get; set; }
        // ����ʱ��
        public string RTT { get; set; }
        // �˿��Ƿ񿪷�
        public string PortIsOpen { get; set; }


        public Item(string hostName, string hostIP, string rTT, string portIsOpen)
        {
            HostName = hostName;
            HostIP = hostIP;
            RTT = rTT;
            PortIsOpen = portIsOpen;
        }
    }
}
