using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text.RegularExpressions;
using Windows.Storage.Pickers;
using WinWoL.Models;

namespace WinWoL.Pages.Dialogs
{
    public sealed partial class AddSSH : ContentDialog
    {
        public SSHModel SSHData { get; private set; }
        public AddSSH(SSHModel sshModel)
        {
            this.InitializeComponent();
            PrimaryButtonClick += MyDialog_PrimaryButtonClick;
            SecondaryButtonClick += MyDialog_SecondaryButtonClick;

            // ��ʼ��Dialog�е��ֶΣ�ʹ�ô����WoLModel���������
            SSHData = sshModel;
            ConfigNameTextBox.Text = sshModel.Name;
            IpAddressTextBox.Text = sshModel.IPAddress;
            SSHCommandTextBox.Text = sshModel.SSHCommand;
            SSHPortTextBox.Text = sshModel.SSHPort;
            SSHUserTextBox.Text = sshModel.SSHUser;
            PrivateKeyIsOpenToggleSwitch.IsOn = sshModel.SSHKeyIsOpen == "True";
            SSHKeyPathTextBox.Text = sshModel.SSHKeyPath;

            refresh();
        }
        private void MyDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // ��"ȷ��"��ť����¼��б����û����������
            SSHData.Name = string.IsNullOrEmpty(ConfigNameTextBox.Text) ? "<δ��������>" : ConfigNameTextBox.Text;
            SSHData.IPAddress = IpAddressTextBox.Text;
            SSHData.SSHCommand = SSHCommandTextBox.Text;
            SSHData.SSHPort = SSHPortTextBox.Text;
            SSHData.SSHUser = SSHUserTextBox.Text;
            SSHData.SSHKeyPath = SSHKeyPathTextBox.Text;
            SSHData.SSHKeyIsOpen = PrivateKeyIsOpenToggleSwitch.IsOn ? "True" : "False";
        }

        private void MyDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // ��"ȡ��"��ť����¼��в����κβ���
        }
        private void refresh()
        {
            // �Ƿ����ù���
            PrivateKeyIsOpen();
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
    }
}
