using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using WinWoL.Datas;
using WinWoL.Methods;
using WinWoL.Models;
using WinWoL.Pages.Dialogs;

namespace WinWoL.Pages
{
    public sealed partial class WoL : Page
    {
        // ���ñ�����������
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        ResourceLoader resourceLoader = new ResourceLoader();
        WoLModel selectedWoLModel;
        private DispatcherQueue _dispatcherQueue;
        public WoL()
        {
            this.InitializeComponent();
            // ��ȡUI�̵߳�DispatcherQueue
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            // ��������
            LoadData();
            LoadString();

            PingRefConfig.IsEnabled = false;
        }
        private void LoadString()
        {
            Header.Text = resourceLoader.GetString("WoLHeader");

            if (localSettings.Values["HideConfigFlag"] == null || localSettings.Values["HideConfigFlag"] as string == "False")
            {
                HideConfig.Content = resourceLoader.GetString("HideConfig");
            }
            else
            {
                HideConfig.Content = resourceLoader.GetString("ShowConfig");
            }
        }
        private void LoadData()
        {
            // ʵ����SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();
            // ��ѯ����
            List<WoLModel> dataList = dbHelper.QueryData();

            // �������б�󶨵�ListView
            dataListView.ItemsSource = dataList;
        }
        // ������ð�ť���
        private async void AddConfigButton_Click(object sender, RoutedEventArgs e)
        {
            // ����һ����ʼ��WoLModel����
            WoLModel initialWoLData = new WoLModel();

            // ����һ���µ�dialog����
            AddWoL dialog = new AddWoL(initialWoLData);
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
                dbHelper.InsertData(initialWoLData);
                // ���¼�������
                LoadData();
            }
        }
        // ���ص�ַ��ť���
        private void HideConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (localSettings.Values["HideConfigFlag"] == null || localSettings.Values["HideConfigFlag"] as string == "False")
            {
                localSettings.Values["HideConfigFlag"] = "True";
                HideConfig.Content = resourceLoader.GetString("ShowConfig");
            }
            else
            {
                localSettings.Values["HideConfigFlag"] = "False";
                HideConfig.Content = resourceLoader.GetString("HideConfig");
            }

            LoadConfigData();
        }
        // �������ð�ť���
        private async void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            ImportConfig.IsEnabled = false;
            // ʵ����SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();
            // ��ȡ���������
            WoLModel woLModel = await WoLMethod.ImportConfig();
            if (woLModel != null)
            {
                // ����������
                dbHelper.InsertData(woLModel);
                // ���¼�������
                LoadData();
            }
            ImportConfig.IsEnabled = true;
        }
        private void WoLPC(WoLModel woLModel)
        {
            InProgressing.IsActive = true;
            // �����߳���ִ������
            Thread subThread = new Thread(new ThreadStart(() =>
            {
                string SuccessFlag = "0";
                // ���Է���Magic Packet���ɹ����ѷ��͵���
                try
                {
                    IPAddress wolAddress = WoLMethod.DomainToIp(woLModel.WoLAddress, "IPv4");
                    WoLMethod.sendMagicPacket(woLModel.MacAddress, wolAddress, int.Parse(woLModel.WoLPort));
                    SuccessFlag = "1";
                }
                // ʧ�ܴ򿪷���ʧ�ܵ���
                catch
                {
                    SuccessFlag = "0";
                }
                if (SuccessFlag == "1")
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        WoLResultTips.IsOpen = true;
                        WoLResultTips.Title = "Magic Packet ���ͳɹ���";
                        WoLResultTips.Subtitle = "Magic Packet �Ѿ�ͨ�� UDP �ɹ�����";
                    });
                }
                else
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        WoLResultTips.IsOpen = true;
                        WoLResultTips.Title = "Magic Packet ����ʧ�ܣ�";
                        WoLResultTips.Subtitle = "��������д����������";
                    });
                }
                _dispatcherQueue.TryEnqueue(() =>
                {
                    InProgressing.IsActive = false;
                });
            }));
            subThread.Start();
        }
        private async void EditThisConfig(WoLModel woLModel)
        {
            // ����һ���µ�dialog����
            AddWoL dialog = new AddWoL(woLModel);
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
                dbHelper.UpdateData(woLModel);
                // ���¼�������
                LoadData();
            }
        }
        private async void ExportConfigFunction(WoLModel wolModel)
        {
            string result = await WoLMethod.ExportConfig(wolModel);
            SaveFileTips.Title = result;
            SaveFileTips.IsOpen = true;
        }
        private void CopyThisConfig(WoLModel wolModel)
        {
            SQLiteHelper dbHelper = new SQLiteHelper();
            dbHelper.InsertData(wolModel);
            // ���¼�������
            LoadData();
        }
        private async void SSHShutdownConfig(WoLModel wolModel)
        {
            string sshPasswd = null;
            // ʹ�������¼
            if (wolModel.SSHKeyIsOpen == "False")
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
                }
            }
            else
            {
                sshPasswd = null;
            }
            InProgressing.IsActive = true;
            // �����߳���ִ������
            Thread subThread = new Thread(new ThreadStart(() =>
            {
                string res = GeneralMethod.SendSSHCommand(wolModel.SSHCommand, wolModel.IPAddress, wolModel.SSHPort, wolModel.SSHUser, sshPasswd, wolModel.SSHKeyPath, wolModel.SSHKeyIsOpen);
                _dispatcherQueue.TryEnqueue(() =>
                {
                    SSHResponse.Subtitle = res;
                    SSHResponse.IsOpen = true;
                    InProgressing.IsActive = false;
                });
            }));
            subThread.Start();
        }
        private async void ConfirmReplace_Click(object sender, RoutedEventArgs e)
        {
            // �رն���ȷ��Flyout
            confirmationReplaceFlyout.Hide();
            ImportConfig.IsEnabled = false;
            // ʵ����SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();

            // ��ȡ���������
            WoLModel newModel = await WoLMethod.ImportConfig();

            if (newModel != null)
            {
                // ��ȡ��ǰ���õ�ID
                int id = selectedWoLModel.Id;
                // �������������
                newModel.Id = id;
                // ����������
                dbHelper.UpdateData(newModel);
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
            dbHelper.DeleteData(selectedWoLModel);
            // ���¼�������
            LoadData();
        }
        private void ConfirmShutdown_Click(object sender, RoutedEventArgs e)
        {
            // �رն���ȷ��Flyout
            confirmationShutdownFlyout.Hide();
            SSHShutdownConfig(selectedWoLModel);

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
        private void CancelShutdown_Click(object sender, RoutedEventArgs e)
        {
            // �رն���ȷ��Flyout
            confirmationShutdownFlyout.Hide();
        }
        private void AboutAliPay_Click(object sender, RoutedEventArgs e)
        {
            AboutAliPayTips.IsOpen = true;
        }
        private void AboutWePay_Click(object sender, RoutedEventArgs e)
        {
            AboutWePayTips.IsOpen = true;
        }
        private async void PingRefConfig_Click(object sender, RoutedEventArgs e)
        {
            // ��ȡ��ǰѡ�����
            WoLModel wolModel = (WoLModel)dataListView.SelectedItem;
            if (wolModel != null)
            {
                // ����һ���µ�dialog����
                PingTools dialog = new PingTools(wolModel);
                // �Դ�dialog�����������
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.CloseButtonText = "�ر�";
                // Ĭ�ϰ�ťΪPrimaryButton
                dialog.DefaultButton = ContentDialogButton.Primary;

                // ��ʾDialog���ȴ���ر�
                ContentDialogResult result = await dialog.ShowAsync();

                // ���������Primary
                if (result == ContentDialogResult.Primary)
                {

                }
            }
        }
        private void LoadConfigData()
        {
            if (dataListView.SelectedItem != null)
            {
                // ��ȡ��ǰѡ�����
                WoLModel selectedItem = (WoLModel)dataListView.SelectedItem;

                // ʵ����SQLiteHelper
                SQLiteHelper dbHelper = new SQLiteHelper();
                // ��ѯ����
                List<WoLModel> dataList = null;
                if (localSettings.Values["HideConfigFlag"] == null || localSettings.Values["HideConfigFlag"] as string == "False")
                {
                    dataList = dbHelper.GetDataListById(selectedItem.Id);
                }
                else
                {
                    dataList = dbHelper.GetDataListByIdHideAddress(selectedItem.Id);
                }

                // �������б�󶨵�ListView
                dataListView2.ItemsSource = dataList;

                if (selectedItem.IPAddress == null || selectedItem.IPAddress == "")
                {
                    PingRefConfig.IsEnabled = false;
                }
                else
                {
                    PingRefConfig.IsEnabled = true;
                }
            }
        }
        private void dataListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadConfigData();
        }
        private void OnListViewRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // ��ȡ�Ҽ������ListViewItem
            FrameworkElement listViewItem = (sender as FrameworkElement);

            if (listViewItem != null && InProgressing.IsActive == false)
            {
                // ��ȡ�Ҽ���������ݶ���WoLModel��
                WoLModel selectedItem = listViewItem?.DataContext as WoLModel;

                // ���Ҽ������������Ϊѡ����
                dataListView.SelectedItem = selectedItem;

                // ����ContextMenu
                MenuFlyout menuFlyout = new MenuFlyout();

                if (selectedItem.WoLIsOpen == "True")
                {
                    MenuFlyoutItem wolPCMenuItem = new MenuFlyoutItem
                    {
                        Text = "���绽��"
                    };
                    wolPCMenuItem.Click += (sender, e) =>
                    {
                        WoLPC(selectedItem);
                    };
                    menuFlyout.Items.Add(wolPCMenuItem);
                }

                if (selectedItem.RDPIsOpen == "True")
                {
                    MenuFlyoutItem rdpPCMenuItem = new MenuFlyoutItem
                    {
                        Text = "Զ������"
                    };
                    rdpPCMenuItem.Click += (sender, e) =>
                    {
                        string mstscCMD = $"mstsc /v:{selectedItem.IPAddress}:{selectedItem.RDPPort};";
                        WoLMethod.RDPConnect(mstscCMD);
                    };
                    menuFlyout.Items.Add(rdpPCMenuItem);
                }

                if (selectedItem.SSHIsOpen == "True")
                {
                    MenuFlyoutItem sshShutdownPCMenuItem = new MenuFlyoutItem
                    {
                        Text = "Զ�̹ػ�"
                    };
                    sshShutdownPCMenuItem.Click += (sender, e) =>
                    {
                        // ����ѡ�е����ݶ����Ա�ȷ�Ϻ�ִ��
                        selectedWoLModel = selectedItem;
                        // ��������ȷ��Flyout
                        confirmationShutdownFlyout.ShowAt(listViewItem);
                    };
                    menuFlyout.Items.Add(sshShutdownPCMenuItem);
                }

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
                    selectedWoLModel = selectedItem;
                    // ��������ȷ��Flyout
                    confirmationReplaceFlyout.ShowAt(listViewItem);
                };
                menuFlyout.Items.Add(replaceMenuItem);

                // ��ӷָ���
                MenuFlyoutSeparator separator2 = new MenuFlyoutSeparator();
                menuFlyout.Items.Add(separator2);

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
                    selectedWoLModel = selectedItem;
                    // ��������ȷ��Flyout
                    confirmationFlyout.ShowAt(listViewItem);
                };
                menuFlyout.Items.Add(deleteMenuItem);

                Thread.Sleep(10);

                // ��ָ��λ����ʾContextMenu
                menuFlyout.ShowAt(listViewItem, e.GetPosition(listViewItem));
            }
        }
        private void OnListViewDoubleTapped(object sender, RoutedEventArgs e)
        {
            // �������˫���¼��Ĵ���
            // ��ȡ�Ҽ������ListViewItem
            FrameworkElement listViewItem = (sender as FrameworkElement);
            if (listViewItem != null && InProgressing.IsActive == false)
            {
                // ��ȡ��������ݶ���
                WoLModel selectedItem = listViewItem?.DataContext as WoLModel;
                WoLPC(selectedItem);
            }
        }
    }
}
