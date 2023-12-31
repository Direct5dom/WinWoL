using Microsoft.UI.Dispatching;
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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using WinWoL.Datas;
using WinWoL.Methods;
using WinWoL.Models;
using WinWoL.Pages.Dialogs;

namespace WinWoL.Pages
{
    public sealed partial class SSHShortcut : Page
    {
        // ���ñ�����������
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        ResourceLoader resourceLoader = new ResourceLoader();
        private DispatcherQueue _dispatcherQueue;
        private SSHModel selectedSSHModel;
        public SSHShortcut()
        {
            this.InitializeComponent();

            // ��ȡUI�̵߳�DispatcherQueue
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            LoadData();
            LoadString();
        }
        private void LoadData()
        {
            // ʵ����SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();
            // ��ѯ����
            List<SSHModel> dataList = dbHelper.QuerySSHData();

            // �������б�󶨵�ListView
            dataGridView.ItemsSource = dataList;
        }
        private void LoadString()
        {
            Header.Text = resourceLoader.GetString("SSHHeader");
        }
        // ���/�޸����ð�ť���
        private async void AddConfigButton_Click(object sender, RoutedEventArgs e)
        {
            // ����һ����ʼ��WoLModel����
            SSHModel initialWoLData = new SSHModel();

            // ����һ���µ�dialog����
            AddSSH dialog = new AddSSH(initialWoLData);
            // �Դ�dialog�����������
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.PrimaryButtonText = "���";
            dialog.CloseButtonText = "�ر�";
            // Ĭ�ϰ�ťΪPrimaryButton
            dialog.DefaultButton = ContentDialogButton.Primary;

            // ��ʾDialog���ȴ���ر�
            ContentDialogResult result = await dialog.ShowAsync();

            // ���������Primary
            if (result == ContentDialogResult.Primary)
            {
                // ʵ����SQLiteHelper
                SQLiteHelper dbHelper = new SQLiteHelper();
                // ����������
                dbHelper.InsertSSHData(initialWoLData);
                // ���¼�������
                LoadData();
            }
        }
        private async void ConfirmReplace_Click(object sender, RoutedEventArgs e)
        {
            // �رն���ȷ��Flyout
            confirmationReplaceFlyout.Hide();
            ImportConfig.IsEnabled = false;
            // ʵ����SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();

            // ��ȡ���������
            SSHModel newModel = await SSHMethod.ImportConfig();

            if (newModel != null)
            {
                // ��ȡ��ǰ���õ�ID
                int id = selectedSSHModel.Id;
                // �������������
                newModel.Id = id;
                // ����������
                dbHelper.UpdateSSHData(newModel);
                // ���¼�������
                LoadData();
            }
            ImportConfig.IsEnabled = true;
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            // �رն���ȷ��Flyout
            confirmationFlyout.Hide();
            // ʵ����SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();
            // ɾ������
            dbHelper.DeleteSSHData(selectedSSHModel);
            // ���¼�������
            LoadData();
        }
        private void CancelReplace_Click(object sender, RoutedEventArgs e)
        {
            // �رն���ȷ��Flyout
            confirmationReplaceFlyout.Hide();
        }
        private void CancelDelete_Click(object sender, RoutedEventArgs e)
        {
            // �رն���ȷ��Flyout
            confirmationFlyout.Hide();
        }
        private async void SSHSend(SSHModel sshModel)
        {
            string sshPasswd = null;
            // ʹ�������¼
            if (sshModel.SSHKeyIsOpen == "False")
            {
                SSHPasswdModel sshPasswdModel = new SSHPasswdModel();
                // ����һ���µ�dialog����
                EnterSSHPasswd dialog = new EnterSSHPasswd(sshPasswdModel);
                // �Դ�dialog�����������
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.PrimaryButtonText = "ȷ��";
                dialog.CloseButtonText = "�ر�";
                // Ĭ�ϰ�ťΪPrimaryButton
                dialog.DefaultButton = ContentDialogButton.Primary;

                // ��ʾDialog���ȴ���ر�
                ContentDialogResult result = await dialog.ShowAsync();

                // ���������Primary
                if (result == ContentDialogResult.Primary)
                {
                    sshPasswd = sshPasswdModel.SSHPasswd;
                    SSHSendThread(sshModel, sshPasswd);
                }
            }
            else
            {
                sshPasswd = null;
                SSHSendThread(sshModel, sshPasswd);
            }
        }
        private void SSHSendThread(SSHModel sshModel, string sshPasswd)
        {
            InProgressing.IsActive = true;
            // �����߳���ִ������
            Thread subThread = new Thread(new ThreadStart(() =>
            {
                string res = GeneralMethod.SendSSHCommand(sshModel.SSHCommand, sshModel.IPAddress, sshModel.SSHPort, sshModel.SSHUser, sshPasswd, sshModel.SSHKeyPath, sshModel.SSHKeyIsOpen);
                _dispatcherQueue.TryEnqueue(() =>
                {
                    SSHResponse.Subtitle = res;
                    SSHResponse.IsOpen = true;
                    InProgressing.IsActive = false;
                });
            }));
            subThread.Start();
        }
        private async void EditThisConfig(SSHModel sshModel)
        {
            // ����һ���µ�dialog����
            AddSSH dialog = new AddSSH(sshModel);
            // �Դ�dialog�����������
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.PrimaryButtonText = "�޸�";
            dialog.CloseButtonText = "�ر�";
            // Ĭ�ϰ�ťΪPrimaryButton
            dialog.DefaultButton = ContentDialogButton.Primary;

            // ��ʾDialog���ȴ���ر�
            ContentDialogResult result = await dialog.ShowAsync();

            // ���������Primary
            if (result == ContentDialogResult.Primary)
            {
                // ʵ����SQLiteHelper
                SQLiteHelper dbHelper = new SQLiteHelper();
                // ��������
                dbHelper.UpdateSSHData(sshModel);
                // ���¼�������
                LoadData();
            }
        }
        private void CopyThisConfig(SSHModel sshModel)
        {
            SQLiteHelper dbHelper = new SQLiteHelper();
            dbHelper.InsertSSHData(sshModel);
            // ���¼�������
            LoadData();
        }
        private async void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            ImportConfig.IsEnabled = false;
            // ʵ����SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();
            // ��ȡ���������
            SSHModel sshModel = await SSHMethod.ImportConfig();
            if (sshModel != null)
            {
                // ����������
                dbHelper.InsertSSHData(sshModel);
                // ���¼�������
                LoadData();
            }
            ImportConfig.IsEnabled = true;
        }
        private async void ExportConfigFunction(SSHModel sshModel)
        {
            string result = await SSHMethod.ExportConfig(sshModel);
            SaveFileTips.Title = result;
            SaveFileTips.IsOpen = true;
        }
        private void AboutAliPay_Click(object sender, RoutedEventArgs e)
        {
            AboutAliPayTips.IsOpen = true;
        }
        private void AboutWePay_Click(object sender, RoutedEventArgs e)
        {
            AboutWePayTips.IsOpen = true;
        }
        private void OnGridViewRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // ��ȡ�Ҽ������Item
            FrameworkElement gridViewItem = (sender as FrameworkElement);

            if (gridViewItem != null && InProgressing.IsActive == false)
            {
                // ��ȡ�Ҽ���������ݶ���WoLModel��
                SSHModel selectedItem = gridViewItem?.DataContext as SSHModel;

                // ʵ����SQLiteHelper
                SQLiteHelper dbHelper = new SQLiteHelper();

                // ���Ҽ������������Ϊѡ����
                dataGridView.SelectedItem = selectedItem;

                // ����ContextMenu
                MenuFlyout menuFlyout = new MenuFlyout();

                MenuFlyoutItem sshShutdownPCMenuItem = new MenuFlyoutItem
                {
                    Text = "ִ�� SSH �ű�"
                };
                sshShutdownPCMenuItem.Click += (sender, e) =>
                {
                    SSHSend(selectedItem);
                };
                menuFlyout.Items.Add(sshShutdownPCMenuItem);

                // ��ӷָ���
                MenuFlyoutSeparator separator = new MenuFlyoutSeparator();
                menuFlyout.Items.Add(separator);

                MenuFlyoutItem exportMenuItem = new MenuFlyoutItem
                {
                    Text = "��������"
                };
                exportMenuItem.Click += (sender, e) =>
                {
                    ExportConfigFunction(selectedItem);
                };
                menuFlyout.Items.Add(exportMenuItem);

                MenuFlyoutItem replaceMenuItem = new MenuFlyoutItem
                {
                    Text = "��������"
                };
                replaceMenuItem.Click += (sender, e) =>
                {
                    // ����ѡ�е����ݶ����Ա�ȷ�Ϻ�ִ��
                    selectedSSHModel = selectedItem;
                    // ��������ȷ��Flyout
                    confirmationReplaceFlyout.ShowAt(gridViewItem);
                };
                menuFlyout.Items.Add(replaceMenuItem);

                // ��ӷָ���
                MenuFlyoutSeparator separator2 = new MenuFlyoutSeparator();
                menuFlyout.Items.Add(separator2);

                if (dbHelper.GetSSHPreRowsId(selectedItem) != -1)
                {
                    MenuFlyoutItem upSwapMenuItem = new MenuFlyoutItem
                    {
                        Text = "��ǰ�ƶ�"
                    };
                    upSwapMenuItem.Click += (sender, e) =>
                    {
                        // �����ƶ�
                        if (dbHelper.UpSwapSSHRows(selectedItem))
                        {
                            // ���¼�������
                            LoadData();
                        }
                    };
                    menuFlyout.Items.Add(upSwapMenuItem);
                }

                if (dbHelper.GetSSHPosRowsId(selectedItem) != -1)
                {
                    MenuFlyoutItem downSwapMenuItem = new MenuFlyoutItem
                    {
                        Text = "����ƶ�"
                    };
                    downSwapMenuItem.Click += (sender, e) =>
                    {
                        // �����ƶ�
                        if (dbHelper.DownSwapSSHRows(selectedItem))
                        {
                            // ���¼�������
                            LoadData();
                        }
                    };
                    menuFlyout.Items.Add(downSwapMenuItem);
                }

                if (dbHelper.GetSSHPreRowsId(selectedItem) != -1 || dbHelper.GetSSHPosRowsId(selectedItem) != -1)
                {
                    // ��ӷָ���
                    MenuFlyoutSeparator separator3 = new MenuFlyoutSeparator();
                    menuFlyout.Items.Add(separator3);
                }

                MenuFlyoutItem editMenuItem = new MenuFlyoutItem
                {
                    Text = "�༭����"
                };
                editMenuItem.Click += (sender, e) =>
                {
                    EditThisConfig(selectedItem);
                };
                menuFlyout.Items.Add(editMenuItem);

                MenuFlyoutItem copyMenuItem = new MenuFlyoutItem
                {
                    Text = "��������"
                };
                copyMenuItem.Click += (sender, e) =>
                {
                    CopyThisConfig(selectedItem);
                };
                menuFlyout.Items.Add(copyMenuItem);

                MenuFlyoutItem deleteMenuItem = new MenuFlyoutItem
                {
                    Text = "ɾ������"
                };
                deleteMenuItem.Click += (sender, e) =>
                {
                    // ����ѡ�е����ݶ����Ա�ȷ�Ϻ�ִ��
                    selectedSSHModel = selectedItem;
                    // ��������ȷ��Flyout
                    confirmationFlyout.ShowAt(gridViewItem);
                };
                menuFlyout.Items.Add(deleteMenuItem);

                Thread.Sleep(10);

                // ��ָ��λ����ʾContextMenu
                menuFlyout.ShowAt(gridViewItem, e.GetPosition(gridViewItem));
            }

        }
        private void OnGridViewDoubleTapped(object sender, RoutedEventArgs e)
        {
            // �������˫���¼��Ĵ���
            // ��ȡ�Ҽ������Item
            FrameworkElement listViewItem = (sender as FrameworkElement);
            if (listViewItem != null && InProgressing.IsActive == false)
            {
                // ��ȡ��������ݶ���
                SSHModel selectedItem = listViewItem?.DataContext as SSHModel;
                SSHSend(selectedItem);
            }
        }
    }
}
