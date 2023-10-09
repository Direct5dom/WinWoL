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
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using WinWoL.Datas;

namespace WinWoL.Pages
{
    public sealed partial class SettingsPage : Page
    {
        // ���ñ�����������
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        ResourceLoader resourceLoader = new ResourceLoader();

        // ����ComboBox�б�List
        public List<string> material { get; } = new List<string>()
        {
            "Mica",
            "Mica Alt",
            "Acrylic"
        };

        public List<string> CMDDisplays { get; } = new List<string>()
        {
            "��",
            "��"
        };

        // ҳ���ʼ��
        public SettingsPage()
        {
            // ��ʼ��
            this.InitializeComponent();

            materialStatusSet();
            LoadString();
        }
        private void LoadString()
        {
            ResetDatabaseTips.ActionButtonContent = resourceLoader.GetString("Confirm");
            ResetDatabaseTips.CloseButtonContent = resourceLoader.GetString("Cancel");
        }
        private void materialStatusSet()
        {
            // ��ȡ�����������ݣ�����ComboBox״̬
            if (localSettings.Values["materialStatus"] as string == "Mica")
            {
                backgroundMaterial.SelectedItem = material[0];
            }
            else if (localSettings.Values["materialStatus"] as string == "Mica Alt")
            {
                backgroundMaterial.SelectedItem = material[1];
            }
            else if (localSettings.Values["materialStatus"] as string == "Acrylic")
            {
                backgroundMaterial.SelectedItem = material[2];
            }
            else
            {
                // �Ƿ����룬����Ĭ�ϲ���ΪMica Alt
                localSettings.Values["materialStatus"] = "Mica Alt";
                backgroundMaterial.SelectedItem = material[1];
                // �Ƿ����룬�ӳ�����
                //throw new Exception($"Wrong material type: {localSettings.Values["materialStatus"]}");
            }
        }

        // ������������ComboBox�Ķ��¼�
        private void backgroundMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string materialStatus = e.AddedItems[0].ToString();
            switch (materialStatus)
            {
                case "Mica":
                    if (localSettings.Values["materialStatus"] as string != "Mica")
                    {
                        localSettings.Values["materialStatus"] = "Mica";
                        Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
                    }
                    else
                    {
                        localSettings.Values["materialStatus"] = "Mica";
                    }
                    break;
                case "Mica Alt":
                    if (localSettings.Values["materialStatus"] as string != "Mica Alt")
                    {
                        localSettings.Values["materialStatus"] = "Mica Alt";
                        Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
                    }
                    else
                    {
                        localSettings.Values["materialStatus"] = "Mica Alt";
                    }
                    break;
                case "Acrylic":
                    if (localSettings.Values["materialStatus"] as string != "Acrylic")
                    {
                        localSettings.Values["materialStatus"] = "Acrylic";
                        Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
                    }
                    else
                    {
                        localSettings.Values["materialStatus"] = "Acrylic";
                    }
                    break;
                default:
                    throw new Exception($"Invalid argument: {materialStatus}");
            }
        }
        private void ResetDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            ResetDatabaseTips.IsOpen = true;
        }
        private void ResetDatabaseTips_ActionButtonClick(TeachingTip sender, object args)
        {
            // ʵ����SQLiteHelper
            SQLiteHelper dbHelper = new SQLiteHelper();
            dbHelper.DropTable();
            ResetDatabaseTips.IsOpen = false;
        }
    }
}
