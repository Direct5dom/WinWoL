// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

// Copyright (C) 2023  SI Xiaolong (https://github.com/Direct5dom) (SIXiaolong_GitHub@outlook.com)

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace WinWoL
{
    public sealed partial class SettingsPage : Page
    {
        // ���ñ�����������
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        // ����ComboBox�б�List
        public List<string> material { get; } = new List<string>()
        {
            "Mica",
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

            //backgroundMaterial.PlaceholderText = localSettings.Values["materialStatus"] as string;
            // ��ȡ�����������ݣ�����ComboBox״̬
            if (localSettings.Values["materialStatus"] as string == "Mica")
            {
                backgroundMaterial.SelectedItem = material[0];
            }
            else if (localSettings.Values["materialStatus"] as string == "Acrylic")
            {
                backgroundMaterial.SelectedItem = material[1];
            }
            else
            {
                // �Ƿ����룬����Ĭ�ϲ���ΪMica
                localSettings.Values["materialStatus"] = "Mica";
                backgroundMaterial.SelectedItem = material[0];
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

        private void CMDDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string CMDDisplay = e.AddedItems[0].ToString();
            switch (CMDDisplay)
            {
                case "��":
                    if (localSettings.Values["CMDDisplay"] as string != "��")
                    {
                        localSettings.Values["CMDDisplay"] = "��";
                    }
                    else
                    {
                        localSettings.Values["CMDDisplay"] = "��";
                    }
                    break;
                case "��":
                    if (localSettings.Values["CMDDisplay"] as string != "��")
                    {
                        localSettings.Values["CMDDisplay"] = "��";
                    }
                    else
                    {
                        localSettings.Values["CMDDisplay"] = "��";
                    }
                    break;
            }
        }
    }
}
