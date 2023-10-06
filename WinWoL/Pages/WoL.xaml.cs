using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using WinWoL.Datas;
using WinWoL.Models;
using System.Net;
using Newtonsoft.Json;
using WinWoL.Methods;
using System.Threading;
using Microsoft.UI.Dispatching;
using WinWoL.Pages.Dialogs;
using System.Security.Principal;
using Validation;
using Renci.SshNet;

namespace WinWoL.Pages
{
    public sealed partial class WoL : Page
    {
        WoLModel selectedWoLModel;
        private DispatcherQueue _dispatcherQueue;
        public WoL()
        {
            this.InitializeComponent();
            // ��ȡUI�̵߳�DispatcherQueue
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            // ��������
            LoadData();

            PingRefConfig.IsEnabled = false;
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
        // ���/�޸����ð�ť���
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


        private void ChangeConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataListView.SelectedItem != null)
            {
                // ��ȡWoLModel����
                WoLModel selectedModel = (WoLModel)dataListView.SelectedItem;
                EditThisConfig(selectedModel);
            }
            else
            {
                NeedSelectedTips.IsOpen = true;
            }
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
        private async void ExportConfigFunction()
        {
            // ��ȡWoLModel����
            WoLModel selectedModel = (WoLModel)dataListView.SelectedItem;
            string result = await WoLMethod.ExportConfig(selectedModel);
        }
        private void CopyThisConfig(WoLModel wolModel)
        {
            SQLiteHelper dbHelper = new SQLiteHelper();
            dbHelper.InsertData(wolModel);
            // ���¼�������
            LoadData();
        }
        private void DelThisConfig(WoLModel wolModel)
        {
            // ʵ����SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();
            // ɾ������
            dbHelper.DeleteData(wolModel);
            // ���¼�������
            LoadData();
        }
        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            // �رն���ȷ��Flyout
            confirmationFlyout.Hide();
            DelThisConfig(selectedWoLModel);
        }
        private async void ConfirmReplace_Click(object sender, RoutedEventArgs e)
        {
            // �رն���ȷ��Flyout
            confirmationReplaceFlyout.Hide();
            // ��ȡNSModel����
            WoLModel selectedModel = (WoLModel)dataListView.SelectedItem;
            ImportConfig.IsEnabled = false;
            // ʵ����SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();

            // ��ȡ���������
            WoLModel newModel = await WoLMethod.ImportConfig();

            if (newModel != null)
            {
                // ��ȡ��ǰ���õ�ID
                int id = selectedModel.Id;
                // �������������
                newModel.Id = id;
                // ����������
                dbHelper.UpdateData(newModel);
                // ���¼�������
                LoadData();
            }
            ImportConfig.IsEnabled = true;
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
        private void PingRefConfig_Click(object sender, RoutedEventArgs e)
        {
            WoLModel selectedItem = (WoLModel)dataListView.SelectedItem;
            PingRef.Text = WoLMethod.PingTest(selectedItem.IPAddress);
        }
        private void OnListViewRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // ��ȡ�Ҽ������ListViewItem
            FrameworkElement listViewItem = (sender as FrameworkElement);

            // ��ȡ�Ҽ���������ݶ���WoLModel��
            WoLModel selectedItem = listViewItem?.DataContext as WoLModel;

            if (selectedItem != null && InProgressing.IsActive == false)
            {
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
                        WoLMethod.SendSSHCommand(selectedItem.SSHCommand, selectedItem.IPAddress, selectedItem.SSHPort, selectedItem.SSHUser, selectedItem.SSHPasswd, selectedItem.SSHKeyPath, selectedItem.SSHKeyIsOpen);
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
                    ExportConfigFunction();
                };
                menuFlyout.Items.Add(exportMenuItem);

                MenuFlyoutItem replaceMenuItem = new MenuFlyoutItem
                {
                    Text = "��������"
                };
                replaceMenuItem.Click += (sender, e) =>
                {
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
        private void dataListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataListView.SelectedItem != null)
            {
                PingRefConfig.IsEnabled = true;

                // ��ȡ��ǰѡ�����
                WoLModel selectedItem = (WoLModel)dataListView.SelectedItem;

                // ʵ����SQLiteHelper
                SQLiteHelper dbHelper = new SQLiteHelper();
                // ��ѯ����
                List<WoLModel> dataList = dbHelper.GetDataListById(selectedItem.Id);

                // �������б�󶨵�ListView
                dataListView2.ItemsSource = dataList;
            }
        }
    }
}
