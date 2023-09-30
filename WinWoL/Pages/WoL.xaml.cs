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

namespace WinWoL.Pages
{
    public sealed partial class WoL : Page
    {
        private DispatcherQueue _dispatcherQueue;
        public WoL()
        {
            this.InitializeComponent();
            // ��ȡUI�̵߳�DispatcherQueue
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            // ��������
            LoadData();
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
            Dialogs.AddWoL dialog = new Dialogs.AddWoL(initialWoLData);
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
        private async void ChangeConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataListView.SelectedItem != null)
            {
                // ��ȡWoLModel����
                WoLModel selectedModel = (WoLModel)dataListView.SelectedItem;

                // ����һ���µ�dialog����
                Dialogs.AddWoL dialog = new Dialogs.AddWoL(selectedModel);
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
                    dbHelper.UpdateData(selectedModel);
                    // ���¼�������
                    LoadData();
                }
            }
            else
            {
                NeedSelectedTips.IsOpen = true;
            }
        }
        private void DelConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataListView.SelectedItem != null)
            {
                // ��ȡWoLModel����
                WoLModel selectedModel = (WoLModel)dataListView.SelectedItem;
                // ʵ����SQLiteHelper
                SQLiteHelper dbHelper = new SQLiteHelper();
                // ɾ������
                dbHelper.DeleteData(selectedModel);
                // ���¼�������
                LoadData();
                // ������ʾFlyout
                if (this.DelConfig.Flyout is Flyout f)
                {
                    f.Hide();
                }
            }
            else
            {
                NeedSelectedTips.IsOpen = true;
            }
        }
        // ���绽�Ѱ�ť���
        private void WoLConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataListView.SelectedItem != null)
            {
                // ��ʱͣ����ذ�ť
                WoLConfig.IsEnabled = false;

                // ��ȡWoLModel����
                WoLModel selectedModel = (WoLModel)dataListView.SelectedItem;
                // ʵ����SQLiteHelper
                SQLiteHelper dbHelper = new SQLiteHelper();
                // ����id�������
                WoLModel woLModel = dbHelper.GetDataById(selectedModel);

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
                        WoLConfig.IsEnabled = true;
                    });
                }));
                subThread.Start();
            }
            else
            {
                NeedSelectedTips.IsOpen = true;
            }
        }
        // Զ�����水ť���
        private void RDPConfigButton_Click(object sender, RoutedEventArgs e)
        {
        }
        // �رյ��԰�ť���
        private void ShutdownConfigButton_Click(object sender, RoutedEventArgs e)
        {
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
        // �������ð�ť���
        private async void ExportConfig_Click(object sender, RoutedEventArgs e)
        {
            if (dataListView.SelectedItem != null)
            {
                // ��ȡWoLModel����
                WoLModel selectedModel = (WoLModel)dataListView.SelectedItem;
                string result = await WoLMethod.ExportConfig(selectedModel);
            }
            else
            {
                NeedSelectedTips.IsOpen = true;
            }
        }
    }
}
