using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using System.Text.RegularExpressions;
using Windows.Storage.Pickers;

namespace WinWoL
{
    public sealed partial class AddConfigDialog : ContentDialog
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public AddConfigDialog()
        {
            this.InitializeComponent();

            string configInner = localSettings.Values["ConfigIDTemp"] as string;
            if (configInner != null)
            {
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
                configName.Text = configInnerSplit[0];
                macAddress.Text = configInnerSplit[1];
                ipAddress.Text = configInnerSplit[2];
                ipPort.Text = configInnerSplit[3];
                rdpIsOpen.IsOn = configInnerSplit[4] == "True";
                rdpIpAddress.Text = configInnerSplit[5];
                rdpIpPort.Text = configInnerSplit[6];
                Broadcast.IsChecked = configInnerSplit[7] == "True";
                //configInnerSplit[8];
                SSHCommand.Text = configInnerSplit[9];
                SSHPort.Text = configInnerSplit[10];
                SSHUser.Text = configInnerSplit[11];
                PrivateKeyIsOpen.IsOn = configInnerSplit[12] == "True";
                SSHPasswd.Password = configInnerSplit[13];
                SSHKeyPath.Text = configInnerSplit[14];
                shutdownIsOpen.IsOn = configInnerSplit[15] == "True";
                SSHHost.Text = configInnerSplit[16];
            }

            Test.Visibility = Visibility.Collapsed;
            redIsOpenCheck();
            ShutdownIsOpen();
            PrivateKeyIsOpenCheck();
        }
        private void InnerChanged()
        {
            SSHHost.Text = rdpIpAddress.Text;

            localSettings.Values["ConfigIDTemp"] = configName.Text + "," + macAddress.Text + ","
            + ipAddress.Text + "," + ipPort.Text + ","
            + rdpIsOpen.IsOn + "," + rdpIpAddress.Text + "," + rdpIpPort.Text + ","
            + Broadcast.IsChecked + "," + "SameIPAddr.IsChecked" + "," + SSHCommand.Text + "," + SSHPort.Text + "," + SSHUser.Text + "," + PrivateKeyIsOpen.IsOn + "," + SSHPasswd.Password + "," + SSHKeyPath.Text + "," + shutdownIsOpen.IsOn + "," + SSHHost.Text;

            if (localSettings.Values["DeveloperImpartIsOpen"] as string == "True")
            {
                Test.Text = localSettings.Values["ConfigIDTemp"] as string;
                Test.Visibility = Visibility.Visible;
            }
            else
            {
                Test.Visibility = Visibility.Collapsed;
            }
        }
        public void TextChanged(object sender, TextChangedEventArgs e)
        {
            // ��ȡTextBox�ĵ�ǰ�ı�
            var text = ((TextBox)sender).Text;
            // ʹ��������ʽ�ų�Ӣ�Ķ���","
            // ��Ϊ�÷����ڴ����߼��������ָ�
            var regex = new Regex(",");
            // ����ı���������ʽƥ�䣬������Ӣ�Ķ��ţ��������ġ�������ʾ��
            if (regex.IsMatch(text))
            {
                ((TextBox)sender).Undo();
                RegularErrorTips.IsOpen = true;
            }

            // ���ݱ��
            InnerChanged();
        }
        private void Broadcast_Checked(object sender, RoutedEventArgs e)
        {
            ipAddress.Text = "255.255.255.255";
            InnerChanged();
        }
        private void Broadcast_Unchecked(object sender, RoutedEventArgs e)
        {
            ipAddress.Text = rdpIpAddress.Text;
            InnerChanged();
        }
        private void rdpIsOpen_Toggled(object sender, RoutedEventArgs e)
        {
            redIsOpenCheck();
            InnerChanged();
        }
        private void redIsOpenCheck()
        {
            if (rdpIsOpen.IsOn == true)
            {
                RDPSettings.Visibility = Visibility.Visible;
            }
            else
            {
                RDPSettings.Visibility = Visibility.Collapsed;
            }
        }
        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            // ���ݱ��
            InnerChanged();
        }
        private void shutdownIsOpen_Toggled(object sender, RoutedEventArgs e)
        {
            ShutdownIsOpen();
            InnerChanged();
        }
        private void ShutdownIsOpen()
        {
            if (shutdownIsOpen.IsOn == true)
            {
                shutdownSettings.Visibility = Visibility.Visible;
            }
            else
            {
                shutdownSettings.Visibility = Visibility.Collapsed;
            }
        }
        private void PrivateKeyIsOpen_Toggled(object sender, RoutedEventArgs e)
        {
            PrivateKeyIsOpenCheck();
        }
        private void PrivateKeyIsOpenCheck()
        {
            if (PrivateKeyIsOpen.IsOn == true)
            {
                SSHKey.Visibility = Visibility.Visible;
                SSHPasswd.Visibility = Visibility.Collapsed;
                SSHPasswd.Password = "";
            }
            else
            {
                SSHKey.Visibility = Visibility.Collapsed;
                SSHKeyPath.Text = "";
                SSHPasswd.Visibility = Visibility.Visible;
            }
        }
        private async void SelectSSHKeyPath_Click(object sender, RoutedEventArgs e)
        {
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
            openPicker.FileTypeFilter.Add("*");

            // ��ѡ�������û�ѡ���ļ�
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                SSHKeyPath.Text = file.Path;
            }
        }
    }
}