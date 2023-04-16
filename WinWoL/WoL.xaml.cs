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
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace WinWoL
{
    public sealed partial class WoL : Page
    {
        // ����localSettings
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        // Selection��Ҫ��List
        public List<string> ConfigSelector { get; set; } = new List<string>()
        {
            "0","1","2","3","4","5","6","7","8","9","10"
        };

        // ҳ���ʼ��
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

        // ����ʵ��
        // ��ˢ�º���
        private void refresh(string ConfigIDNum)
        {
            // ��ȡlocalSettings�д洢���ַ���
            string configInner = localSettings.Values["ConfigID" + ConfigIDNum] as string;

            // ��ʼ������Ԫ��
            ConfigName.Text = "���ñ�����";
            MacAddress.Text = "���� Mac��";
            IpAddress.Text = "WoL ������ַ��";
            IpPort.Text = "WoL �˿ڣ�";
            RDPIpAddress.Text = "RDP ������ַ��";
            RDPIpPort.Text = "RDP �˿ڣ�";
            RDPPing.Text = "RDP �˿��ӳ٣�";
            AddConfig.Content = "�������";
            DelConfig.IsEnabled = false;
            RefConfig.IsEnabled = false;
            WoLConfig.IsEnabled = false;
            RDPConfig.IsEnabled = false;

            // ����ַ�����Ϊ��
            if (configInner != null)
            {
                // �޸Ľ���UI�����Ժ�������ʾ
                AddConfig.Content = "�޸�����";
                DelConfig.IsEnabled = true;
                WoLConfig.IsEnabled = true;

                // �ָ��ַ���
                string[] configInnerSplit = configInner.Split(',');
                // ������ַ����ṹ��
                // configName.Text + "," + macAddress.Text + ","
                // + ipAddress.Text + "," + ipPort.Text + ","
                // + rdpIsOpen.IsOn + "," + rdpIpAddress.Text + "," + rdpIpPort;
                string configName = configInnerSplit[0];
                string macAddress = configInnerSplit[1];
                string ipAddress = configInnerSplit[2];
                string ipPort = configInnerSplit[3];
                string rdpIsOpen = configInnerSplit[4];
                string rdpIpAddress = configInnerSplit[5];
                string rdpPort = configInnerSplit[6];

                // ����RDP��������
                localSettings.Values["mstscCMD"] = "mstsc /v:" + rdpIpAddress + ":" + rdpPort + ";";

                // ������ǹ㲥��ַ������ʾIP��������
                // ����ǹ㲥��ַ������ʾ���� LAN ����㲥��
                string ipAddressDisplay = ipAddress;
                if (ipAddressDisplay == "255.255.255.255")
                {
                    ipAddressDisplay = "�� LAN ����㲥";
                }

                // �������RDP
                if (rdpIsOpen == "True")
                {
                    ConfigName.Text = "���ñ�����" + configName;
                    MacAddress.Text = "���� Mac��" + macAddress;
                    IpAddress.Text = "WoL ������ַ��" + ipAddressDisplay;
                    IpPort.Text = "WoL �˿ڣ�" + ipPort;
                    RDPIpAddress.Text = "RDP ������ַ��" + rdpIpAddress;
                    RDPIpPort.Text = "RDP �˿ڣ�" + rdpPort;
                    RDPPing.Text = "RDP �˿��ӳ٣�δ����";

                    RefConfig.IsEnabled = true;
                    RDPConfig.IsEnabled = true;
                }
                // û�п���RDP
                else
                {
                    ConfigName.Text = "���ñ�����" + configName;
                    MacAddress.Text = "���� Mac��" + macAddress;
                    IpAddress.Text = "WoL ������ַ��" + ipAddressDisplay;
                    IpPort.Text = "WoL �˿ڣ�" + ipPort;
                    RDPIpAddress.Text = "RDP ������ַ��δ����";
                    RDPIpPort.Text = "RDP �˿ڣ�δ����";
                    RDPPing.Text = "RDP �˿��ӳ٣�δ����";

                    RefConfig.IsEnabled = false;
                    RDPConfig.IsEnabled = false;
                }
            }
        }
        // ��UDPЭ�鷢��MagicPacket
        public void sendMagicPacket(string macAddress, string domain, int port)
        {
            // �������Mac��ַ�ַ����ָ�Ϊʮ�������ַ�������
            // hexStrings = {"11", "22", "33", "44", "55", "66"}
            string s = macAddress;
            string[] hexStrings = s.Split(':');

            // ����һ��byte����
            byte[] bytes = new byte[hexStrings.Length];
            // �����ַ������飬��ÿ���ַ���ת��Ϊbyteֵ�����洢��byte������
            for (int i = 0; i < hexStrings.Length; i++)
            {
                // ʹ��16��Ϊ������ʾʮ�����Ƹ�ʽ
                bytes[i] = Convert.ToByte(hexStrings[i], 16);
            }
            // ��MAC��ַת��Ϊ�ֽ����飺byte[] mac = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 };
            byte[] mac = bytes;

            // ����һ��UDP Socket����
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // ������Ҫ�㲥����
            socket.EnableBroadcast = true;

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
            IPAddress ip = domain2ip(domain);

            // ��������
            socket.SendTo(packet, new IPEndPoint(ip, port));

            // �ر�Socket����
            socket.Close();
        }
        static IPAddress domain2ip(string domain)
        {
            // �˺���������Դ����ַǷ�IP�����磺266.266.266.266��
            // ��Щ�Ƿ�IP�ᱻ��������������
            IPAddress ipAddress;
            if (IPAddress.TryParse(domain, out ipAddress))
            {
                // ��IP
                return IPAddress.Parse(domain);
            }
            else
            {
                // ����������������
                return Dns.GetHostEntry(domain).AddressList[0];
            }
        }
        // Ping���Ժ���
        static string PingTest(string domain, int port)
        {
            // ��ȡIP��ַ
            // ������ִ��������������Դ���һЩ�Ƿ�IP���������⣨���磺�㲥��ַ 255.255.255.255��
            // �Ƿ�IP�ᱻ���ء�������ַ�޷���ͨ������������pingSender������Ӧ�ñ���
            IPAddress ipAddress = domain2ip(domain);

            // Pingʵ������
            System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
            // Pingѡ��
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            string data = "ping";
            byte[] buf = Encoding.ASCII.GetBytes(data);

            // ����ͬ��Send�����������ݣ��������reply����;
            PingReply reply = pingSender.Send(ipAddress, 500, buf, options);
            // �ж�replay���Ƿ���ͨ
            if (reply.Status == IPStatus.Success)
            {
                // �����ͨ��������ָ���˿�ͨ��
                var client = new TcpClient();
                if (!client.ConnectAsync(ipAddress, port).Wait(500))
                {
                    // ��ָ���˿�ͨ��ʧ��
                    return "�˿�����ʧ��";
                }
                else
                {
                    // ��ָ���˿�ͨ�ųɹ�������RTT������
                    return reply.RoundtripTime.ToString() + " ms";
                }
            }
            else
            {
                // �޷���ͨ
                return "�޷���ͨ";
            }

        }
        // ���������ļ������÷���MagicPacket
        private void WoLPC(string ConfigIDNum)
        {
            // ��ȡlocalSettings�е��ַ���
            string configInner = localSettings.Values["ConfigID" + ConfigIDNum] as string;
            // ����ַ����ǿ�
            if (configInner != null)
            {
                // �ָ��ַ���
                string[] configInnerSplit = configInner.Split(',');
                // configName.Text + "," + macAddress.Text + "," + ipAddress.Text + ":" + ipPort.Text;
                string macAddress = configInnerSplit[1];
                string ipAddress = configInnerSplit[2];
                string ipPort = configInnerSplit[3];

                // ���Է���Magic Packet���ɹ����ѷ��͵���
                try
                {
                    sendMagicPacket(macAddress, ipAddress, int.Parse(ipPort));
                    MagicPacketIsSendTips.IsOpen = true;
                }
                // ʧ�ܴ򿪷���ʧ�ܵ���
                catch
                {
                    MagicPacketNotSendTips.IsOpen = true;
                }
            }
        }
        // ����mstsc����
        private void RDPPCChildThread()
        {
            // ����һ���µĽ���
            Process process = new Process();
            // ָ������PowerShell
            process.StartInfo.FileName = "PowerShell.exe";
            // ����Ϊ����mstsc�Ĳ���
            // ��������localSettings�У�����ˢ�º���ˢ��
            process.StartInfo.Arguments = localSettings.Values["mstscCMD"] as string;
            //�Ƿ�ʹ�ò���ϵͳshell����
            process.StartInfo.UseShellExecute = false;
            //�Ƿ����´����������ý��̵�ֵ (����ʾ���򴰿�)
            process.StartInfo.CreateNoWindow = true;
            // ���̿�ʼ
            process.Start();
            // �ȴ�ִ�н���
            process.WaitForExit();
            // ���̹ر�
            process.Close();
        }
        // ɾ������
        private void delConfig(string ConfigIDNum)
        {
            // ���ָ��ConfigIDNum��localSettings
            localSettings.Values["ConfigID" + ConfigIDNum] = null;
        }
        // Ping RDP�����˿�
        private void PingRDPRef(string ConfigIDNum)
        {
            // ��localSettings�ж�ȡ�ַ���
            string configInner = localSettings.Values["ConfigID" + ConfigIDNum] as string;
            // ����ַ����ǿ�
            if (configInner != null)
            {
                // �ָ��ַ���
                string[] configInnerSplit = configInner.Split(',');
                // ������ַ����ṹ��
                // configName.Text + "," + macAddress.Text + ","
                // + ipAddress.Text + "," + ipPort.Text + ","
                // + rdpIsOpen.IsOn + "," + rdpIpAddress.Text + "," + rdpIpPort;
                string rdpIpAddress = configInnerSplit[5];
                string rdpPort = configInnerSplit[6];

                // ���RDP�����˿��Ƿ����Pingͨ
                try
                {
                    RDPPing.Text = "RDP �˿��ӳ٣�" + PingTest(rdpIpAddress, int.Parse(rdpPort)).ToString();
                }
                catch
                {
                    RDPPing.Text = "RDP �˿��ӳ٣��޷���ͨ";
                }
            }
        }

        // �¼�
        // Selection�ı�
        private void configNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            refresh(configNum.SelectedItem.ToString());
            localSettings.Values["configNum"] = configNum.SelectedItem;

            //�ر����е�������
            MagicPacketIsSendTips.IsOpen = false;
            MagicPacketNotSendTips.IsOpen = false;
        }
        // ���/�޸����ð�ť���
        private async void AddConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configNum.SelectedItem.ToString();
            // ��ConfigIDTemp���洢���ַ�������Ϊ��ǰ�������洢���ַ���
            // ��������ʵ�֡��޸ġ��Ĳ���
            localSettings.Values["ConfigIDTemp"] = localSettings.Values["ConfigID" + ConfigIDNum];

            // ����һ���µ�dialog����
            AddConfigDialog configDialog = new AddConfigDialog();

            // �Դ�dialog�����������
            configDialog.XamlRoot = this.XamlRoot;
            configDialog.Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            // ������������������PrimaryButton������
            if (AddConfig.Content.ToString() == "�޸�����")
            {
                configDialog.PrimaryButtonText = "�޸�";
            }
            else
            {
                configDialog.PrimaryButtonText = "���";
            }
            configDialog.CloseButtonText = "�ر�";
            // Ĭ�ϰ�ťΪPrimaryButton
            configDialog.DefaultButton = ContentDialogButton.Primary;

            // �첽��ȡ�����ĸ���ť
            var result = await configDialog.ShowAsync();

            // ���������Primary
            if (result == ContentDialogResult.Primary)
            {
                // ��ConfigIDTempд�뵽��ǰ����ID�µ�localSettings
                localSettings.Values["ConfigID" + ConfigIDNum] = localSettings.Values["ConfigIDTemp"];
                // ˢ��UI
                refresh(ConfigIDNum);
            }
        }
        // Ping���԰�ť���
        private void RefConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configNum.SelectedItem.ToString();
            // Ping����
            PingRDPRef(ConfigIDNum);
        }
        // ɾ�����ð�ť���
        private void DelConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configNum.SelectedItem.ToString();
            // ɾ������
            delConfig(ConfigIDNum);
            // ˢ��UI
            refresh(ConfigIDNum);
            // ������ʾFlyout
            if (this.DelConfig.Flyout is Flyout f)
            {
                f.Hide();
            }
        }
        // ���绽�Ѱ�ť���
        private void WoLConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configNum.SelectedItem.ToString();
            // WoL����ָ�����豸
            WoLPC(ConfigIDNum);
        }
        // Զ�����水ť���
        private void RDPConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configNum.SelectedItem.ToString();
            // ˢ��RDP�����˿ڵ�Ping״̬
            PingRDPRef(ConfigIDNum);

            // ����һ�����̣߳���ֹUI����
            ThreadStart childref = new ThreadStart(RDPPCChildThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
        }
    }
}
