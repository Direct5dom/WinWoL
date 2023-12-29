using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinWoL.Models;
using System;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace WinWoL.Pages.Dialogs
{
    public sealed partial class AddWoL : ContentDialog
    {
        public WoLModel WoLData { get; private set; }
        public AddWoL(WoLModel wolModel)
        {
            this.InitializeComponent();
            PrimaryButtonClick += MyDialog_PrimaryButtonClick;
            SecondaryButtonClick += MyDialog_SecondaryButtonClick;

            // ��ʼ��Dialog�е��ֶΣ�ʹ�ô����WoLModel���������
            WoLData = wolModel;
            ConfigNameTextBox.Text = wolModel.Name;
            IpAddressTextBox.Text = wolModel.IPAddress;
            IndependentAddressCheckBox.IsChecked = wolModel.BroadcastIsOpen == "True";
            WoLIsOpenToggleSwitch.IsOn = wolModel.WoLIsOpen == "True";
            MacAddressTextBox.Text = wolModel.MacAddress;
            WoLPortTextBox.Text = wolModel.WoLPort;
            RDPIsOpenToggleSwitch.IsOn = wolModel.RDPIsOpen == "True";
            RDPIPPortTextBox.Text = wolModel.RDPPort;
            SSHShutdownIsOpenToggleSwitch.IsOn = wolModel.SSHIsOpen == "True";
            SSHCommandTextBox.Text = wolModel.SSHCommand;
            SSHPortTextBox.Text = wolModel.SSHPort;
            SSHUserTextBox.Text = wolModel.SSHUser;
            PrivateKeyIsOpenToggleSwitch.IsOn = wolModel.SSHKeyIsOpen == "True";
            SSHKeyPathTextBox.Text = wolModel.SSHKeyPath;

            // �����Ƿ���������WoL��ַ
            if (IndependentAddressCheckBox.IsChecked == true)
            {
                // ������д�������WoL��ַ
                IndependentAddressTextBox.Text = wolModel.WoLAddress;
            }

            refresh();
        }
        private void MyDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // ��"ȷ��"��ť����¼��б����û����������
            WoLData.Name = string.IsNullOrEmpty(ConfigNameTextBox.Text) ? "<δ��������>" : ConfigNameTextBox.Text;
            WoLData.IPAddress = IpAddressTextBox.Text;
            WoLData.BroadcastIsOpen = IndependentAddressCheckBox.IsChecked == true ? "True" : "False";
            WoLData.WoLIsOpen = WoLIsOpenToggleSwitch.IsOn ? "True" : "False";
            WoLData.MacAddress = MacAddressTextBox.Text;
            WoLData.WoLPort = WoLPortTextBox.Text;
            WoLData.RDPIsOpen = RDPIsOpenToggleSwitch.IsOn ? "True" : "False";
            WoLData.RDPPort = RDPIPPortTextBox.Text;
            WoLData.SSHIsOpen = SSHShutdownIsOpenToggleSwitch.IsOn ? "True" : "False";
            WoLData.SSHCommand = SSHCommandTextBox.Text;
            WoLData.SSHPort = SSHPortTextBox.Text;
            WoLData.SSHUser = SSHUserTextBox.Text;
            WoLData.SSHKeyPath = SSHKeyPathTextBox.Text;
            WoLData.SSHKeyIsOpen = PrivateKeyIsOpenToggleSwitch.IsOn ? "True" : "False";

            // �����Ƿ���������WoL��ַ
            if (IndependentAddressCheckBox.IsChecked == true)
            {
                // ������д�������WoL��ַ
                WoLData.WoLAddress = IndependentAddressTextBox.Text;
            }
            else
            {
                // �رգ�д��IP��ַ
                WoLData.WoLAddress = IpAddressTextBox.Text;
            }
        }

        private void MyDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // ��"ȡ��"��ť����¼��в����κβ���
        }
        private void refresh()
        {
            // �Ƿ����ù���
            WoLIsOpen();
            RDPIsOpen();
            ShutdownIsOpen();
            PrivateKeyIsOpen();
            IndependentAddressIsChecked();
        }
        private void IndependentAddressIsChecked()
        {
            if (IndependentAddressCheckBox.IsChecked == true)
            {
                IndependentAddressTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                IndependentAddressTextBox.Visibility = Visibility.Collapsed;
            }
        }
        private void WoLIsOpen()
        {
            if (WoLIsOpenToggleSwitch.IsOn == true)
            {
                WoLConfig.Visibility = Visibility.Visible;
            }
            else
            {
                WoLConfig.Visibility = Visibility.Collapsed;
            }
        }
        private void RDPIsOpen()
        {
            if (RDPIsOpenToggleSwitch.IsOn == true)
            {
                RDPConfig.Visibility = Visibility.Visible;
            }
            else
            {
                RDPConfig.Visibility = Visibility.Collapsed;
            }
        }
        private void ShutdownIsOpen()
        {
            if (SSHShutdownIsOpenToggleSwitch.IsOn == true)
            {
                shutdownConfig.Visibility = Visibility.Visible;
            }
            else
            {
                shutdownConfig.Visibility = Visibility.Collapsed;
            }
        }
        private void PrivateKeyIsOpen()
        {
            if (PrivateKeyIsOpenToggleSwitch.IsOn == true)
            {
                SSHKey.Visibility = Visibility.Visible;
                SSHPasswordBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                SSHKey.Visibility = Visibility.Collapsed;
                SSHPasswordBox.Visibility = Visibility.Visible;
            }
        }
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
        }
        private void IndependentAddressTextChanged(object sender, TextChangedEventArgs e)
        {
        }
        private void MacAddressTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            MacAddressTextClean(textBox);
        }
        private void IndependentAddressTextPaste(object sender, TextControlPasteEventArgs e)
        {
        }
        private void MacAddressTextPaste(object sender, TextControlPasteEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            MacAddressTextClean(textBox);
        }
        private void MacAddressTextClean(TextBox textBox)
        {
            string input = textBox.Text;

            // ʹ��������ʽ��ƥ��Ϸ��ĸ�ʽ
            string pattern = @"^([0-9A-Fa-f]{2}[:]){5}([0-9A-Fa-f]{2})$";
            if (Regex.IsMatch(input, pattern))
            {
                // ����Ϸ��������ı�����
                textBox.Text = input;
            }
            else
            {
                // ����Ƿ����Ƴ���ƥ����ַ�
                textBox.Text = Regex.Replace(input, "[^0-9A-Fa-f:]", "");
                // ����ƶ���ĩβ
                textBox.SelectionStart = textBox.Text.Length;
            }
        }
        private void IPAddressTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            IPAddressTextClean(textBox);
        }
        private void IPAddressTextPaste(object sender, TextControlPasteEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            IPAddressTextClean(textBox);
        }
        private void IPAddressTextClean(TextBox textBox)
        {
            string input = textBox.Text;

            // ʹ��������ʽ��ƥ��Ϸ��ĸ�ʽ
            string pattern = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$|^((?!-)[A-Za-z0-9-]{1,63}(?<!-)\.)+[A-Za-z]{2,6}$";
            if (Regex.IsMatch(input, pattern))
            {
                // ����Ϸ��������ı�����
                textBox.Text = input;
            }
            else
            {
                // ����Ƿ����Ƴ���ƥ����ַ�
                textBox.Text = Regex.Replace(input, @"[^A-Za-z0-9:.]", "");
                // ����ƶ���ĩβ
                textBox.SelectionStart = textBox.Text.Length;
            }
        }
        private void PortTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            PortTextClean(textBox);
        }
        private void PortTextPaste(object sender, TextControlPasteEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            PortTextClean(textBox);
        }
        private void PortTextClean(TextBox textBox)
        {
            string input = textBox.Text;

            // ʹ��������ʽ��ƥ��Ϸ��ĸ�ʽ
            string pattern = "^[0-9]*$";
            if (Regex.IsMatch(input, pattern))
            {
                // ����Ϸ��������ı�����
                textBox.Text = input;
            }
            else
            {
                // ����Ƿ����Ƴ���ƥ����ַ�
                textBox.Text = Regex.Replace(input, "[^0-9]", "");
                // ����ƶ���ĩβ
                textBox.SelectionStart = textBox.Text.Length;
            }
        }
        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            refresh();
        }
        // ���ö�����WoL��ַ
        private void IndependentAddress_Checked(object sender, RoutedEventArgs e)
        {
            // WoL��ַ��������
            IndependentAddressTextBox.Visibility = Visibility.Visible;
            refresh();
        }
        // �رն�����WoL��ַ
        private void IndependentAddress_Unchecked(object sender, RoutedEventArgs e)
        {
            // WoL��ַ��IP��ַ��ͬ
            IndependentAddressTextBox.Visibility = Visibility.Collapsed;
            refresh();
        }
        private void rdpIsOpen_Toggled(object sender, RoutedEventArgs e)
        {
            refresh();
        }
        private void wolIsOpen_Toggled(object sender, RoutedEventArgs e)
        {
            refresh();
        }
        private void SSHShutdownIsOpen_Toggled(object sender, RoutedEventArgs e)
        {
            refresh();
        }
        private void privateKeyIsOpen_Toggled(object sender, RoutedEventArgs e)
        {
            refresh();
        }
        private async void SelectSSHKeyPath_Click(object sender, RoutedEventArgs e)
        {
            // ����һ��FileOpenPicker
            var openPicker = new FileOpenPicker();
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
            string filePath = null;
            if (file != null)
            {
                filePath = file.Path;
            }
            else
            {
                filePath = null;
            }
            SSHKeyPathTextBox.Text = filePath;
        }
    }
}
