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

namespace WinWoL.Pages
{
    public sealed partial class WoL : Page
    {
        // ����һ��ObservableCollection���ڱ�������
        private ObservableCollection<Models.WoLModel> dataList = new ObservableCollection<Models.WoLModel>();
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public WoL()
        {
            this.InitializeComponent();
            LoadData();
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
                LoadData();
            }
        }
        private void LoadData()
        {
            SQLiteHelper dbHelper = new SQLiteHelper();
            List<WoLModel> dataList = dbHelper.QueryData();

            // �������б�󶨵�ListView
            dataListView.ItemsSource = dataList;
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
    }
}
