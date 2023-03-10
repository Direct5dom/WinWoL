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

namespace WinWoL
{
    public sealed partial class Ping : Page
    {
        public Ping()
        {
            this.InitializeComponent();
        }
        public void pingTest(string pingHostPort)
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
                // ����ģ��
                List<Item> items = new List<Item>();
                items.Add(new Item(
                    "��������" + pingHost,
                    "����IP��" + reply.Address.ToString(),
                    "����ʱ��RTT��" + reply.RoundtripTime.ToString() + " ms",
                    port + " �˿ڿ��������" + checkPortEnable(pingHost, port).ToString()
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

        private void pingTestButton_Click(object sender, RoutedEventArgs e)
        {
            pingTest(ipAddress.Text + ":" + ipPort.Text);
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
