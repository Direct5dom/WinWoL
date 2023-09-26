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
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Microsoft.UI.Dispatching;
using Windows.Storage.Streams;

namespace WinWoL
{
    public sealed partial class WoL : Page
    {
        // ����localSettings
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private DispatcherQueue _dispatcherQueue;

        // Selection��Ҫ��List
        public List<string> ConfigSelector { get; set; } = new List<string>()
        {
            "0","1","2","3","4","5","6","7","8","9","10"
        };

        // ҳ���ʼ��
        public WoL()
        {
            this.InitializeComponent();

            // ��ȡUI�̵߳�DispatcherQueue
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

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

            // ���ظ��� ��ʾ����
            ImportConfig.Visibility = Visibility.Visible;
            ImportAndReplaceConfig.Visibility = Visibility.Collapsed;

            AddConfig.Content = "�������";
            DelConfig.IsEnabled = false;
            RefConfig.IsEnabled = false;
            WoLConfig.IsEnabled = false;
            RDPConfig.IsEnabled = false;
            ExportConfig.IsEnabled = false;
            HideConfig.IsEnabled = false;

            // ����ַ�����Ϊ��
            if (configInner != null)
            {
                // �޸Ľ���UI�����Ժ�������ʾ
                AddConfig.Content = "�޸�����";
                DelConfig.IsEnabled = true;
                WoLConfig.IsEnabled = true;
                ExportConfig.IsEnabled = true;
                HideConfig.IsEnabled = true;

                // ���ص��� ��ʾ����
                ImportConfig.Visibility = Visibility.Collapsed;
                ImportAndReplaceConfig.Visibility = Visibility.Visible;

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

                // ����������ص�ַ
                string macAddressDisplay = macAddress;
                string ipPortDisplay = ipPort;
                string rdpIpAddressDisplay = rdpIpAddress;
                string rdpPortDisplay = rdpPort;
                if (localSettings.Values["HideConfig"] == null)
                {
                    localSettings.Values["HideConfig"] = "True";
                }
                if (localSettings.Values["HideConfig"].ToString() != "False")
                {
                    HideConfig.Content = "��ʾ��ַ";
                    macAddressDisplay = "**:**:**:**:**:**";
                    ipAddressDisplay = "***.***.***.***";
                    ipPortDisplay = "***";
                    rdpIpAddressDisplay = "***.***.***.***";
                    rdpPortDisplay = "***";
                }

                // �������RDP
                if (rdpIsOpen == "True")
                {
                    ConfigName.Text = "���ñ�����" + configName;
                    MacAddress.Text = "���� Mac��" + macAddressDisplay;
                    IpAddress.Text = "WoL ������ַ��" + ipAddressDisplay;
                    IpPort.Text = "WoL �˿ڣ�" + ipPortDisplay;
                    RDPIpAddress.Text = "RDP ������ַ��" + rdpIpAddressDisplay;
                    RDPIpPort.Text = "RDP �˿ڣ�" + rdpPortDisplay;
                    RDPPing.Text = "RDP �˿��ӳ٣�δ����";

                    RefConfig.IsEnabled = true;
                    RDPConfig.IsEnabled = true;
                }
                // û�п���RDP
                else
                {
                    ConfigName.Text = "���ñ�����" + configName;
                    MacAddress.Text = "���� Mac��" + macAddressDisplay;
                    IpAddress.Text = "WoL ������ַ��" + ipAddressDisplay;
                    IpPort.Text = "WoL �˿ڣ�" + ipPortDisplay;
                    RDPIpAddress.Text = "RDP ������ַ��δ����";
                    RDPIpPort.Text = "RDP �˿ڣ�δ����";
                    RDPPing.Text = "RDP �˿��ӳ٣�δ����";

                    RefConfig.IsEnabled = false;
                    RDPConfig.IsEnabled = false;
                }
            }
        }
        // ��ȡ������Ӧ��IP
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
        // ��UDPЭ�鷢��MagicPacket
        public void sendMagicPacket(string macAddress, IPAddress ipAddress, int port)
        {
            // ��ʱͣ����ذ�ť
            WoLConfig.IsEnabled = false;
            // �����߳���ִ������
            Thread subThread = new Thread(new ThreadStart(() =>
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

                // ��η��ͣ����ⶪ��
                for (int i = 0; i < 10; i++)
                {
                    // ��������
                    socket.SendTo(packet, new IPEndPoint(ipAddress, port));
                }

                // �ر�Socket����
                socket.Close();
                _dispatcherQueue.TryEnqueue(() =>
                {
                    MagicPacketIsSendTips.IsOpen = true;
                    // ������ذ�ť
                    WoLConfig.IsEnabled = true;
                });
            }));
            subThread.Start();
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
                // configName.Text + "," + macAddress.Text + "," + ipAddress.Text + "," + ipPort.Text;
                string macAddress = configInnerSplit[1];
                string ipAddress = configInnerSplit[2];
                string ipPort = configInnerSplit[3];

                // ���Է���Magic Packet���ɹ����ѷ��͵���
                try
                {
                    // ��ȡIP��ַ
                    IPAddress ip = domain2ip(ipAddress);
                    sendMagicPacket(macAddress, ip, int.Parse(ipPort));
                }
                // ʧ�ܴ򿪷���ʧ�ܵ���
                catch
                {
                    MagicPacketNotSendTips.IsOpen = true;
                }
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
        // Ping RDP�����˿�
        private void PingRDPRef(string ConfigIDNum)
        {
            // ��ʱͣ����ذ�ť
            RefConfig.IsEnabled = false;
            configNum.IsEnabled = false;
            // �����߳���ִ������
            Thread subThread = new Thread(new ThreadStart(() =>
            {
                string pingRes = "";
                for (int i = 5; i > 0; i--)
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
                            pingRes = "RDP �˿��ӳ٣�" + PingTest(rdpIpAddress, int.Parse(rdpPort)).ToString();
                        }
                        catch
                        {
                            pingRes = "RDP �˿��ӳ٣��޷���ͨ";
                        }
                    }
                    // Ҫ��UI�߳��ϸ���UI��ʹ��DispatcherQueue
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        int flag = 0;
                        if (pingRes != "RDP �˿��ӳ٣��޷���ͨ")
                        {
                            RDPPing.Text = pingRes;
                            flag++;
                        }
                        if (flag == 3)
                        {
                            RDPPing.Text = "RDP �˿��ӳ٣��޷���ͨ";
                            flag = 0;
                        }
                        RefConfig.Content = "Ping (" + i + ")";
                    });
                    Thread.Sleep(1000);
                }
                // Ҫ��UI�߳��ϸ���UI��ʹ��DispatcherQueue
                _dispatcherQueue.TryEnqueue(() =>
                {
                    RefConfig.Content = "Ping";
                    RefConfig.IsEnabled = true;
                    configNum.IsEnabled = true;
                });
            }));

            subThread.Start();
        }
        // ����mstsc����
        private void RDPPCChildThread()
        {
            // ��ʱͣ����ذ�ť
            RDPConfig.IsEnabled = false;
            // �����߳���ִ������
            Thread subThread = new Thread(new ThreadStart(() =>
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
                // Ҫ��UI�߳��ϸ���UI��ʹ��DispatcherQueue
                _dispatcherQueue.TryEnqueue(() =>
                {
                    RDPTips.IsOpen = true;
                    // ������ذ�ť
                    RDPConfig.IsEnabled = true;
                });
            }));

            subThread.Start();
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
            RDPTips.IsOpen = false;
            SaveConfigTips.IsOpen = false;
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
            // ���ָ��ConfigIDNum��localSettings
            localSettings.Values["ConfigID" + ConfigIDNum] = null;
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
            // ����mstsc
            RDPPCChildThread();
        }
        // �������ð�ť���
        private async void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configNum.SelectedItem.ToString();

            await CommonFunctions.ImportConfig("WinWoL.WoL", "ConfigID", ConfigIDNum);

            // ˢ��UI
            refresh(ConfigIDNum);
        }
        // �������ð�ť���
        private async void ExportConfig_Click(object sender, RoutedEventArgs e)
        {
            // ��localSettings�ж�ȡ�ַ���
            string ConfigIDNum = configNum.SelectedItem.ToString();
            string configInner = localSettings.Values["ConfigID" + ConfigIDNum] as string;
            // ����ַ����ǿ�
            if (configInner != null)
            {
                string result = await CommonFunctions.ExportConfig("WinWoL.WoL", "ConfigID", ConfigIDNum);
                SaveConfigTips.Title = result;
                SaveConfigTips.IsOpen = true;
            }
        }
        private void HideConfig_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configNum.SelectedItem.ToString();
            if (localSettings.Values["HideConfig"].ToString() == "True")
            {
                localSettings.Values["HideConfig"] = "False";
                HideConfig.Content = "���ص�ַ";
            }
            else
            {
                localSettings.Values["HideConfig"] = "True";
                HideConfig.Content = "��ʾ��ַ";
            }
            // ˢ��UI
            refresh(ConfigIDNum);
        }
    }
}
