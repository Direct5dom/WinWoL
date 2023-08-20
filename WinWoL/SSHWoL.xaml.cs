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

namespace WinWoL
{
    public sealed partial class SSHWoL : Page
    {
        // ����localSettings
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        // Selection��Ҫ��List
        public List<string> ConfigSelector { get; set; } = new List<string>()
        {
            "0","1","2","3","4","5","6","7","8","9","10"
        };
        public SSHWoL()
        {
            this.InitializeComponent();

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
            SSHPasswd.Text = "SSH ���룺";
            SSHPing.Text = "SSH �˿��ӳ٣�δ����";

            // ���ظ��� ��ʾ����
            ImportConfig.Visibility = Visibility.Visible;
            ImportAndReplaceConfig.Visibility = Visibility.Collapsed;

            AddConfig.Content = "��������";
            DelConfig.IsEnabled = false;
            RefConfig.IsEnabled = false;
            //WoLConfig.IsEnabled = false;
            //RDPConfig.IsEnabled = false;
            ExportConfig.IsEnabled = false;
            HidePasswd.IsEnabled = false;

            // ����ַ�����Ϊ��
            if (configInner != null)
            {
                // �޸Ľ���UI�����Ժ�������ʾ
                AddConfig.Content = "�޸�����";
                DelConfig.IsEnabled = true;
                //WoLConfig.IsEnabled = true;
                ExportConfig.IsEnabled = true;
                HidePasswd.IsEnabled = true;

                // ���ص��� ��ʾ����
                ImportConfig.Visibility = Visibility.Collapsed;
                ImportAndReplaceConfig.Visibility = Visibility.Visible;

                // �ָ��ַ���
                string[] configInnerSplit = configInner.Split(',');
                // ������ַ����ṹ��
                // SSHConfigName.Text + "," + SSHCommand.Text + ","
                // + SSHHost.Text + "," + SSHPort.Text + ","
                // + SSHUser.Text + "," + SSHPasswd.Text;
                string sshConfigName = configInnerSplit[0];
                string sshCommand = configInnerSplit[1];
                string sshHost = configInnerSplit[2];
                string sshPort = configInnerSplit[3];
                string sshUser = configInnerSplit[4];
                string sshPasswd = configInnerSplit[5];

                SSHConfigName.Text = "���ñ�����" + sshConfigName;
                SSHCommand.Text = "SSH ���" + sshCommand;
                SSHHost.Text = "SSH ������" + sshHost;
                SSHPort.Text = "SSH �˿ڣ�" + sshPort;
                SSHUser.Text = "SSH �û���" + sshUser;
                SSHPasswd.Text = "SSH ���룺" + sshPasswd;
                SSHPing.Text = "SSH �˿��ӳ٣�δ����";

                RefConfig.IsEnabled = true;
                //RDPConfig.IsEnabled = true;
            }
        }
        // SSHִ������
        private void SendSSHCommand(string sshCommand,string sshHost,string sshPort,string sshUser,string sshPasswd)
        {
            var sshClient = new SshClient(sshHost, int.Parse(sshPort), sshUser, sshPasswd);
            // ����
            sshClient.Connect();
            // ִ������
            var cmd = sshClient.RunCommand(sshCommand);
            if (cmd.ExitStatus == 0)
            {
                //Console.WriteLine(cmd.Result);//ִ�н��
                SSHResponse.Subtitle = cmd.Result;
                SSHResponse.IsOpen = true;
                //Test.Text = cmd.Result;
            }
            else
            {
                //Console.WriteLine(cmd.Error);//������Ϣ
                SSHResponse.Subtitle = cmd.Error;
                SSHResponse.IsOpen = true;
                //Test.Text = cmd.Error;
            }
            //�Ͽ�����
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
                // SSHConfigName.Text + "," + SSHCommand.Text + ","
                // + SSHHost.Text + "," + SSHPort.Text + ","
                // + SSHUser.Text + "," + SSHPasswd.Text;
                string sshCommand = configInnerSplit[1];
                string sshHost = configInnerSplit[2];
                string sshPort = configInnerSplit[3];
                string sshUser = configInnerSplit[4];
                string sshPasswd = configInnerSplit[5];

                // ��ȡIP��ַ
                sshHost = domain2ip(sshHost).ToString();

                try
                {
                    SendSSHCommand(sshCommand,sshHost,sshPort,sshUser, sshPasswd);
                }
                catch
                {
                    SSHCommandNotSendTips.IsOpen = true;
                }
                
            }
        }
        static IPAddress domain2ip(string domain)
        {
            // �˺����������Դ������ַǷ�IP�����磺266.266.266.266��
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
            // �Ƿ�IP�ᱻ���ء�������ַ�޷���ͨ������������pingSender��������Ӧ�ñ���
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
                    SSHPing.Text = "SSH �˿��ӳ٣�" + PingTest(sshHost, int.Parse(sshPort)).ToString();
                }
                catch
                {
                    SSHPing.Text = "SSH �˿��ӳ٣��޷���ͨ";
                }
            }
        }

        // �¼�
        // Selection�ı�
        private void configNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            refresh(configNum.SelectedItem.ToString());
            localSettings.Values["SSHConfigNum"] = configNum.SelectedItem;
        }
        // ����/�޸����ð�ť���
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
                configDialog.PrimaryButtonText = "����";
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
                // �û����Խ��ļ�����Ϊ���ļ����������б�
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
        private void HidePasswd_Click(object sender, RoutedEventArgs e)
        {
            string SSHConfigIDNum = configNum.SelectedItem.ToString();
            if (localSettings.Values["SSHHidePasswd"].ToString() == "True")
            {
                localSettings.Values["SSHHidePasswd"] = "False";
                HidePasswd.Content = "���ص�ַ";
            }
            else
            {
                localSettings.Values["SSHHidePasswd"] = "True";
                HidePasswd.Content = "��ʾ��ַ";
            }
            // ˢ��UI
            refresh(SSHConfigIDNum);
        }
        // ִ�нű���ť���
        private void RunSSHButton_Click(object sender, RoutedEventArgs e)
        {
            string ConfigIDNum = configNum.SelectedItem.ToString();
            runSSH(ConfigIDNum);
        }
    }
}