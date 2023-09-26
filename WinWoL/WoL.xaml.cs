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
using Renci.SshNet;

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
            IpAddress.Text = "������ַ��";
            IpPort.Text = "WoL �˿ڣ�";
            RDPIpPort.Text = "RDP �˿ڣ�";
            RDPPing.Text = "RDP �˿��ӳ٣�";

            // ���ظ��� ��ʾ����
            ImportConfig.Visibility = Visibility.Visible;
            ImportAndReplaceConfig.Visibility = Visibility.Collapsed;

            // ��ʼ�����а�ť״̬
            AddConfig.Content = "�������";
            DelConfig.IsEnabled = false;
            RefConfig.IsEnabled = false;
            WoLConfig.IsEnabled = false;
            RDPConfig.IsEnabled = false;
            ExportConfig.IsEnabled = false;
            HideConfig.IsEnabled = false;
            ShutdownConfig.IsEnabled = false;

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
                string[] configInnerSplit = new string[17];
                string[] configInnerSplitOld = configInner.Split(',');
                for (int i = 0; i < 17; i++)
                {
                    try
                    {
                        configInnerSplit[i] = configInnerSplitOld[i];
                    }
                    catch
                    {
                        if (i == 4 || i == 7 || i == 8 || i == 12 || i == 15)
                        {
                            configInnerSplit[i] = "False";
                        }
                        else
                        {
                            configInnerSplit[i] = "";
                        }
                    }
                }
                // ������ַ����ṹ��
                //configName.Text = configInnerSplit[0];
                //macAddress.Text = configInnerSplit[1];
                //ipAddress.Text = configInnerSplit[2];
                //ipPort.Text = configInnerSplit[3];
                //rdpIsOpen.IsOn = configInnerSplit[4] == "True";
                //rdpIpAddress.Text = configInnerSplit[5];
                //rdpIpPort.Text = configInnerSplit[6];
                //Broadcast.IsChecked = configInnerSplit[7] == "True";
                //SSHCommand.Text = configInnerSplit[9];
                //SSHPort.Text = configInnerSplit[10];
                //SSHUser.Text = configInnerSplit[11];
                //PrivateKeyIsOpen.IsOn = configInnerSplit[12] == "True";
                //SSHPasswd.Password = configInnerSplit[13];
                //SSHKeyPath.Text = configInnerSplit[14];
                //shutdownIsOpen.IsOn = configInnerSplit[15] == "True";
                //SSHHost.Text = configInnerSplit[16];
                string configName = configInnerSplit[0];
                string macAddress = configInnerSplit[1];
                string ipAddress = configInnerSplit[5];
                string ipPort = configInnerSplit[3];
                string rdpIsOpen = configInnerSplit[4];
                string rdpIpAddress = configInnerSplit[5];
                string rdpPort = configInnerSplit[6];
                string shutdownIsOpen = configInnerSplit[15];
                string sshPort = configInnerSplit[10];

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
                string rdpPortDisplay = rdpPort;
                string sshPortDisplay = sshPort;
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
                    rdpPortDisplay = "***";
                    sshPortDisplay = "***";
                }

                ConfigName.Text = "���ñ�����" + configName;
                MacAddress.Text = "���� Mac��" + macAddressDisplay;
                IpAddress.Text = "������ַ��" + ipAddressDisplay;
                IpPort.Text = "WoL �˿ڣ�" + ipPortDisplay;

                // �������RDP
                if (rdpIsOpen == "True")
                {
                    RDPIpPort.Text = "RDP �˿ڣ�" + rdpPortDisplay;
                    RDPPing.Text = "RDP �˿��ӳ٣�δ����";

                    RefConfig.IsEnabled = true;
                    RDPConfig.IsEnabled = true;
                }
                // û�п���RDP
                else
                {
                    RDPIpPort.Text = "RDP �˿ڣ�δ����";
                    RDPPing.Text = "RDP �˿��ӳ٣�δ����";

                    RefConfig.IsEnabled = false;
                    RDPConfig.IsEnabled = false;
                }

                // ����������رյ��ԡ�
                if (shutdownIsOpen == "True")
                {
                    SSHIpPort.Text = "SSH �˿ڣ�" + sshPortDisplay;
                    ShutdownConfig.IsEnabled = true;
                }
                // û�п������رյ��ԡ�
                else
                {
                    SSHIpPort.Text = "SSH �˿ڣ�δ����";
                    ShutdownConfig.IsEnabled = false;
                }
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
                // configName.Text + "," + macAddress.Text + "," + ipAddress.Text + "," + ipPort.Text;
                string macAddress = configInnerSplit[1];
                string ipAddress = configInnerSplit[2];
                string ipPort = configInnerSplit[3];

                // ���Է���Magic Packet���ɹ����ѷ��͵���
                try
                {
                    // ��ȡIP��ַ
                    IPAddress ip = CommonFunctions.domain2ip(ipAddress);
                    // ��ʱͣ����ذ�ť
                    WoLConfig.IsEnabled = false;
                    // �����߳���ִ������
                    Thread subThread = new Thread(new ThreadStart(() =>
                    {
                        CommonFunctions.sendMagicPacket(macAddress, ip, int.Parse(ipPort));

                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            MagicPacketIsSendTips.IsOpen = true;
                            // ������ذ�ť
                            WoLConfig.IsEnabled = true;
                        });
                    }));
                    subThread.Start();

                }
                // ʧ�ܴ򿪷���ʧ�ܵ���
                catch
                {
                    MagicPacketNotSendTips.IsOpen = true;
                }
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
                            pingRes = "RDP �˿��ӳ٣�" + CommonFunctions.PingTest(rdpIpAddress, int.Parse(rdpPort)).ToString();
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
                string arguments = localSettings.Values["mstscCMD"] as string;

                CommonFunctions.RDPConnect(arguments);

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
            string[] configInnerSplit = configInner.Split(',');
            string configName = configInnerSplit[0];
            // ����ַ����ǿ�
            if (configInner != null)
            {
                string result = await CommonFunctions.ExportConfig("WinWoL.WoL", "ConfigID", ConfigIDNum, configName);
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
        private void runSSH(string ConfigIDNum)
        {
            // ��ȡlocalSettings�е��ַ���
            string configInner = localSettings.Values["ConfigID" + ConfigIDNum] as string;

            if (configInner != null)
            {
                // �ָ��ַ���
                string[] configInnerSplit = configInner.Split(',');
                // ������ַ����ṹ��
                //configName.Text = configInnerSplit[0];
                //macAddress.Text = configInnerSplit[1];
                //ipAddress.Text = configInnerSplit[2];
                //ipPort.Text = configInnerSplit[3];
                //rdpIsOpen.IsOn = configInnerSplit[4] == "True";
                //rdpIpAddress.Text = configInnerSplit[5];
                //rdpIpPort.Text = configInnerSplit[6];
                //Broadcast.IsChecked = configInnerSplit[7] == "True";
                //SSHCommand.Text = configInnerSplit[9];
                //SSHPort.Text = configInnerSplit[10];
                //SSHUser.Text = configInnerSplit[11];
                //PrivateKeyIsOpen.IsOn = configInnerSplit[12] == "True";
                //SSHPasswd.Password = configInnerSplit[13];
                //SSHKeyPath.Text = configInnerSplit[14];
                //shutdownIsOpen.IsOn = configInnerSplit[15] == "True";
                //SSHHost.Text = configInnerSplit[16];
                string sshCommand = configInnerSplit[9];
                string sshHost = configInnerSplit[16];
                string sshPort = configInnerSplit[10];
                string sshUser = configInnerSplit[11];
                string sshPasswdAndKey;
                string privateKeyIsOpen = configInnerSplit[12];

                if (privateKeyIsOpen == "True")
                {
                    sshPasswdAndKey = configInnerSplit[14];
                }
                else
                {
                    sshPasswdAndKey = configInnerSplit[13];
                }

                // ��ȡIP��ַ
                sshHost = CommonFunctions.domain2ip(sshHost).ToString();

                try
                {
                    SSHResponse.Subtitle = CommonFunctions.SendSSHCommand(sshCommand, sshHost, sshPort, sshUser, sshPasswdAndKey, privateKeyIsOpen);
                    SSHResponse.IsOpen = true;
                }
                catch
                {
                    SSHCommandNotSendTips.Subtitle = "��������д�������Լ�����״����\nע��˽Կ·���͸�ʽ����֧�� OpenSSH �� ssh.com ��ʽ�� RSA �� DSA ˽Կ��";
                    SSHCommandNotSendTips.IsOpen = true;
                }
            }
        }
        private void ShutdownConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configNum.SelectedItem.ToString();
            runSSH(ConfigIDNum);
        }
    }
}
