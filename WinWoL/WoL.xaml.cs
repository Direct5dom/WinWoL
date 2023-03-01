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

namespace WinWoL
{
    public sealed partial class WoL : Page
    {
        public WoL()
        {
            this.InitializeComponent();
        }
        private void WakePCButton_Click(object sender, RoutedEventArgs e)
        {
            sendMagicPacket(macAddress.Text, ipAddress.Text, int.Parse(ipPort.Text));
        }
        public void sendMagicPacket(string macAddress, string domain, int port)
        {
            // ��string�ָ�Ϊʮ�������ַ�������
            string s = macAddress;

            // hexStrings = {"11", "22", "33", "44", "55", "66"}
            string[] hexStrings = s.Split(':');

            // ����һ��byte����
            byte[] bytes = new byte[hexStrings.Length];

            // �����ַ������飬��ÿ���ַ���ת��Ϊbyteֵ�����洢��byte������
            for (int i = 0; i < hexStrings.Length; i++)
            {
                // ʹ��16��Ϊ������ʾʮ�����Ƹ�ʽ
                bytes[i] = Convert.ToByte(hexStrings[i], 16);
            }

            // ����һ��UDP Socket����
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // ������Ҫ�㲥����
            socket.EnableBroadcast = true;

            // ��MAC��ַת��Ϊ�ֽ����飺byte[] mac = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 };
            byte[] mac = bytes;

            // ����һ��ħ����
            byte[] packet = new byte[17 * 6];

            // ���ǰ6���ֽ�Ϊ0xFF
            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;

            // ������16���ظ���MAC��ַ�ֽ�
            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];

            // ��ȡIP��ַ
            IPAddress ip;
            if (IPAddress.TryParse(domain, out ip))
            {
                // ��IP
                ip = IPAddress.Parse(domain);
            }
            else
            {
                // ������
                ip = Dns.GetHostEntry(domain).AddressList[0];
                // ����������Ӧ��IPд��ipAddress.Text
                ipAddress.Text = ip.ToString();
            }

            // ��������
            socket.SendTo(packet, new IPEndPoint(ip, port));

            // �ر�Socket����
            socket.Close();
        }
    }
}
