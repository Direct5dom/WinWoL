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
        public SSHShortcut()
        {
            this.InitializeComponent();

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
            //// �رն���ȷ��Flyout
            //confirmationReplaceFlyout.Hide();
            //// ��ȡNSModel����
            //WoLModel selectedModel = (WoLModel)dataListView.SelectedItem;
            //ImportConfig.IsEnabled = false;
            //// ʵ����SQLiteHelper
            //SQLiteHelper dbHelper = new SQLiteHelper();

            //// ��ȡ���������
            //WoLModel newModel = await WoLMethod.ImportConfig();

            //if (newModel != null)
            //{
            //    // ��ȡ��ǰ���õ�ID
            //    int id = selectedModel.Id;
            //    // �������������
            //    newModel.Id = id;
            //    // ����������
            //    dbHelper.UpdateData(newModel);
            //    // ���¼�������
            //    LoadData();
            //}
            //ImportConfig.IsEnabled = true;
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            // �رն���ȷ��Flyout
            confirmationFlyout.Hide();
            //DelThisConfig(selectedWoLModel);
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
        private void OnGridViewRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // ��ȡ�Ҽ������Item
            FrameworkElement gridViewItem = (sender as FrameworkElement);

            // ��ȡ�Ҽ���������ݶ���WoLModel��
            WoLModel selectedItem = gridViewItem?.DataContext as WoLModel;

            if (selectedItem != null && InProgressing.IsActive == false)
            {
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
                    //ExportConfigFunction();
                };
                menuFlyout.Items.Add(exportMenuItem);

                MenuFlyoutItem replaceMenuItem = new MenuFlyoutItem
                {
                    Text = "��������"
                };
                replaceMenuItem.Click += (sender, e) =>
                {
                    // ��������ȷ��Flyout
                    //confirmationReplaceFlyout.ShowAt(listViewItem);
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
                    //EditThisConfig(selectedItem);
                };
                menuFlyout.Items.Add(editMenuItem);

                MenuFlyoutItem copyMenuItem = new MenuFlyoutItem
                {
                    Text = "��������"
                };
                copyMenuItem.Click += (sender, e) =>
                {
                    //CopyThisConfig(selectedItem);
                };
                menuFlyout.Items.Add(copyMenuItem);

                MenuFlyoutItem deleteMenuItem = new MenuFlyoutItem
                {
                    Text = "ɾ������"
                };
                deleteMenuItem.Click += (sender, e) =>
                {
                    //// ����ѡ�е����ݶ����Ա�ȷ�Ϻ�ִ��
                    //selectedWoLModel = selectedItem;
                    //// ��������ȷ��Flyout
                    //confirmationFlyout.ShowAt(listViewItem);
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
            //FrameworkElement listViewItem = (sender as FrameworkElement);
            //if (listViewItem != null && InProgressing.IsActive == false)
            //{
            //    // ��ȡ��������ݶ���
            //    WoLModel selectedItem = listViewItem?.DataContext as WoLModel;
            //    WoLPC(selectedItem);
            //}
        }
    }
}
