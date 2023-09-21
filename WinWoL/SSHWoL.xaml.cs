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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using Validation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Renci.SshNet;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.Mail;
using Microsoft.UI.Dispatching;

namespace WinWoL
{
    public sealed partial class SSHWoL : Page
    {
        // ����localSettings
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private DispatcherQueue _dispatcherQueue;

        // Selection��Ҫ��List
        public List<string> ConfigSelector { get; set; } = new List<string>()
        {
            "0","1","2","3","4","5","6","7","8","9","10"
        };
        public SSHWoL()
        {
            this.InitializeComponent();

            // ��ȡUI�̵߳�DispatcherQueue
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            if (localSettings.Values["SSHConfigNum"] == null)
            {
                configNum.SelectedItem = ConfigSelector[0];
                localSettings.Values["SSHConfigNum"] = ConfigSelector[0];
                refresh("0");
            }
            else
            {
                configNum.SelectedItem = localSettings.Values["SSHConfigNum"];
                refresh(localSettings.Values["SSHConfigNum"].ToString());
            }
        }
        // ����ʵ��
        // ��ˢ�º���
        private void refresh(string SSHConfigIDNum)
        {
            // ��ȡlocalSettings�д洢���ַ���
            string configInner = localSettings.Values["SSHConfigID" + SSHConfigIDNum] as string;

            // ��ʼ������Ԫ��
            SSHConfigName.Text = "���ñ�����";
            SSHCommand.Text = "SSH ���";
            SSHHost.Text = "SSH ������";
            SSHPort.Text = "SSH �˿ڣ�";
            SSHUser.Text = "SSH �û���";
            SSHPing.Text = "SSH �˿��ӳ٣�δ����";

            // ���ظ��� ��ʾ����
            ImportConfig.Visibility = Visibility.Visible;
            ImportAndReplaceConfig.Visibility = Visibility.Collapsed;

            AddConfig.Content = "�������";
            DelConfig.IsEnabled = false;
            RefConfig.IsEnabled = false;
            ExportConfig.IsEnabled = false;

            // ����ַ�����Ϊ��
            if (configInner != null)
            {
                // �޸Ľ���UI�����Ժ�������ʾ
                AddConfig.Content = "�޸�����";
                DelConfig.IsEnabled = true;
                //WoLConfig.IsEnabled = true;
                ExportConfig.IsEnabled = true;

                // ���ص��� ��ʾ����
                ImportConfig.Visibility = Visibility.Collapsed;
                ImportAndReplaceConfig.Visibility = Visibility.Visible;

                // �ָ��ַ���
                string[] configInnerSplit = configInner.Split(',');
                // ������ַ����ṹ��
                //  SSHConfigName.Text + "," + SSHCommand.Text + ","
                //+ SSHHost.Text + "," + SSHPort.Text + ","
                //+ SSHUser.Text + "," + SSHPasswd.Text + ","
                //+ PrivateKeyIsOpen.IsOn + "," + SSHKey.Text;
                string sshConfigName = configInnerSplit[0];
                string sshCommand = configInnerSplit[1];
                string sshHost = configInnerSplit[2];
                string sshPort = configInnerSplit[3];
                string sshUser = configInnerSplit[4];

                SSHConfigName.Text = "���ñ�����" + sshConfigName;
                SSHCommand.Text = "SSH ���" + sshCommand;
                SSHHost.Text = "SSH ������" + sshHost;
                SSHPort.Text = "SSH �˿ڣ�" + sshPort;
                SSHUser.Text = "SSH �û���" + sshUser;
                SSHPing.Text = "SSH �˿��ӳ٣�δ����";

                RefConfig.IsEnabled = true;
            }
        }
        // SSHִ������
        private void SendSSHCommand(string sshCommand, string sshHost, string sshPort, string sshUser, string sshPasswdAndKey, string privateKeyIsOpen)
        {
            SshClient sshClient;

            if (privateKeyIsOpen == "True")
            {
                PrivateKeyFile privateKeyFile = new PrivateKeyFile(sshPasswdAndKey);
                ConnectionInfo connectionInfo = new ConnectionInfo(sshHost, sshUser, new PrivateKeyAuthenticationMethod(sshUser, new PrivateKeyFile[] { privateKeyFile }));
                sshClient = new SshClient(connectionInfo);
            }
            else
            {
                sshClient = new SshClient(sshHost, int.Parse(sshPort), sshUser, sshPasswdAndKey);
            }

            sshClient.Connect();

            if (sshClient.IsConnected)
            {
                SshCommand SSHCommand = sshClient.RunCommand(sshCommand);

                if (!string.IsNullOrEmpty(SSHCommand.Error))
                {
                    SSHResponse.Subtitle = "Error: " + SSHCommand.Error;
                    SSHResponse.IsOpen = true;
                }
                else
                {
                    SSHResponse.Subtitle = SSHCommand.Result;
                    SSHResponse.IsOpen = true;
                }
            }
            sshClient.Disconnect();
        }
        private void runSSH(string SSHConfigIDNum)
        {
            // ��ȡlocalSettings�е��ַ���
            string configInner = localSettings.Values["SSHConfigID" + SSHConfigIDNum] as string;

            if (configInner != null)
            {
                // �ָ��ַ���
                string[] configInnerSplit = configInner.Split(',');
                // ������ַ����ṹ��
                //  SSHConfigName.Text + "," + SSHCommand.Text + ","
                //+ SSHHost.Text + "," + SSHPort.Text + ","
                //+ SSHUser.Text + "," + SSHPasswd.Text + ","
                //+ PrivateKeyIsOpen.IsOn + "," + SSHKey.Text;
                string sshCommand = configInnerSplit[1];
                string sshHost = configInnerSplit[2];
                string sshPort = configInnerSplit[3];
                string sshUser = configInnerSplit[4];
                string sshPasswdAndKey;
                string privateKeyIsOpen = configInnerSplit[6];

                if (privateKeyIsOpen == "True")
                {
                    sshPasswdAndKey = configInnerSplit[7];
                }
                else
                {
                    sshPasswdAndKey = configInnerSplit[5];
                }

                // ��ȡIP��ַ
                sshHost = domain2ip(sshHost).ToString();

                try
                {
                    SendSSHCommand(sshCommand, sshHost, sshPort, sshUser, sshPasswdAndKey, privateKeyIsOpen);
                }
                catch
                {
                    SSHCommandNotSendTips.Subtitle = "��������д�������Լ�����״����\nע��˽Կ·���͸�ʽ����֧�� OpenSSH �� ssh.com ��ʽ�� RSA �� DSA ˽Կ��";
                    SSHCommandNotSendTips.IsOpen = true;
                }
            }
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
        // Ping SSH�����˿�
        private void PingSSHRef(string SSHConfigIDNum)
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
                    string configInner = localSettings.Values["SSHConfigID" + SSHConfigIDNum] as string;
                    // ����ַ����ǿ�
                    if (configInner != null)
                    {
                        // �ָ��ַ���
                        string[] configInnerSplit = configInner.Split(',');
                        // ������ַ����ṹ��
                        // SSHConfigName.Text + "," + SSHCommand.Text + ","
                        // + SSHHost.Text + "," + SSHPort.Text + ","
                        // + SSHUser.Text + "," + SSHPasswd.Text;
                        string sshHost = configInnerSplit[2];
                        string sshPort = configInnerSplit[3];

                        // ���RDP�����˿��Ƿ����Pingͨ
                        try
                        {
                            pingRes = "SSH �˿��ӳ٣�" + PingTest(sshHost, int.Parse(sshPort)).ToString();
                        }
                        catch
                        {
                            pingRes = "SSH �˿��ӳ٣��޷���ͨ";
                        }
                    }
                    // Ҫ��UI�߳��ϸ���UI��ʹ��DispatcherQueue
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        int flag = 0;
                        if (pingRes != "SSH �˿��ӳ٣��޷���ͨ")
                        {
                            SSHPing.Text = pingRes;
                            flag++;
                        }
                        if (flag == 3)
                        {
                            SSHPing.Text = "SSH �˿��ӳ٣��޷���ͨ";
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

        // �¼�
        // Selection�ı�
        private void configNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            refresh(configNum.SelectedItem.ToString());
            localSettings.Values["SSHConfigNum"] = configNum.SelectedItem;
        }
        // ���/�޸����ð�ť���
        private async void AddConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string SSHConfigIDNum = configNum.SelectedItem.ToString();
            // ��ConfigIDTemp���洢���ַ�������Ϊ��ǰ�������洢���ַ���
            // ��������ʵ�֡��޸ġ��Ĳ���
            localSettings.Values["SSHConfigIDTemp"] = localSettings.Values["SSHConfigID" + SSHConfigIDNum];

            // ����һ���µ�dialog����
            AddSSHConfigDialog configDialog = new AddSSHConfigDialog();

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
                localSettings.Values["SSHConfigID" + SSHConfigIDNum] = localSettings.Values["SSHConfigIDTemp"];
                // ˢ��UI
                refresh(SSHConfigIDNum);
            }
        }
        // Ping���԰�ť���
        private void RefConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string SSHConfigIDNum = configNum.SelectedItem.ToString();
            // Ping����
            PingSSHRef(SSHConfigIDNum);
        }
        // ɾ�����ð�ť���
        private void DelConfigButton_Click(object sender, RoutedEventArgs e)
        {
            string SSHConfigIDNum = configNum.SelectedItem.ToString();
            // ɾ������
            // ���ָ��ConfigIDNum��localSettings
            localSettings.Values["SSHConfigID" + SSHConfigIDNum] = null;
            // ˢ��UI
            refresh(SSHConfigIDNum);
            // ������ʾFlyout
            if (this.DelConfig.Flyout is Flyout f)
            {
                f.Hide();
            }
        }
        // �������ð�ť���
        private async void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            string SSHConfigIDNum = configNum.SelectedItem.ToString();

            // ����һ��FileOpenPicker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            // ��ȡ��ǰ���ھ�� (HWND) 
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
            // ʹ�ô��ھ�� (HWND) ��ʼ��FileOpenPicker
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // ΪFilePicker����ѡ��
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            // �����λ�� ����
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            // �ļ����͹�����
            openPicker.FileTypeFilter.Add(".sshcmdconfig");

            // ��ѡ�������û�ѡ���ļ�
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var path = file.Path;
                localSettings.Values["SSHConfigID" + SSHConfigIDNum] = File.ReadAllText(path, Encoding.UTF8);
                // ˢ��UI
                refresh(SSHConfigIDNum);
            }
        }
        // �������ð�ť���
        private async void ExportConfig_Click(object sender, RoutedEventArgs e)
        {
            // ��localSettings�ж�ȡ�ַ���
            string SSHConfigIDNum = configNum.SelectedItem.ToString();
            string configInner = localSettings.Values["SSHConfigID" + SSHConfigIDNum] as string;
            // ����ַ����ǿ�
            if (configInner != null)
            {
                // �ָ��ַ���
                string[] configInnerSplit = configInner.Split(',');
                // ������ַ����ṹ��
                // SSHConfigName.Text + "," + SSHCommand.Text + ","
                // + SSHHost.Text + "," + SSHPort.Text + ","
                // + SSHUser.Text + "," + SSHPasswd.Text;
                string sshConfigName = configInnerSplit[0];

                string configContent = localSettings.Values["SSHConfigID" + SSHConfigIDNum].ToString();

                // ����һ��FileSavePicker
                FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();
                // ��ȡ��ǰ���ھ�� (HWND) 
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.m_window);
                // ʹ�ô��ھ�� (HWND) ��ʼ��FileSavePicker
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

                // ΪFilePicker����ѡ��
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                // �û����Խ��ļ����Ϊ���ļ����������б�
                savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".sshcmdconfig" });
                // Ĭ���ļ���
                savePicker.SuggestedFileName = sshConfigName + "_BackUp_" + DateTime.Now.ToString();

                // ��Picker���û�ѡ���ļ�
                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // ��ֹ�����ļ���Զ�̰汾��ֱ��������ɸ��Ĳ����� CompleteUpdatesAsync��
                    CachedFileManager.DeferUpdates(file);

                    // д���ļ�
                    using (var stream = await file.OpenStreamForWriteAsync())
                    {
                        using (var tw = new StreamWriter(stream))
                        {
                            tw.WriteLine(configContent);
                        }
                    }

                    // ��Windows֪������������ļ����ģ��Ա�����Ӧ�ó�����Ը����ļ���Զ�̰汾��
                    // ��ɸ��¿�����ҪWindows�����û����롣
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == FileUpdateStatus.Complete)
                    {
                        // �ļ�����ɹ�
                        SaveConfigTips.Title = "����ɹ���";
                        SaveConfigTips.IsOpen = true;
                    }
                    else if (status == FileUpdateStatus.CompleteAndRenamed)
                    {
                        // �ļ�������������ɹ�
                        SaveConfigTips.Title = "������������ɹ���";
                        SaveConfigTips.IsOpen = true;
                    }
                    else
                    {
                        // �ļ��޷����棡
                        SaveConfigTips.Title = "�޷����棡";
                        SaveConfigTips.IsOpen = true;
                    }
                }
            }
        }
        // ִ�нű���ť���
        private void RunSSHButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configNum.SelectedItem.ToString();
            runSSH(ConfigIDNum);
        }
    }
}
