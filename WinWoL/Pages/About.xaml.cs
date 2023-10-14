using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Net.Http;
using System.Threading;
using Windows.ApplicationModel;

namespace WinWoL.Pages
{
    public sealed partial class About : Page
    {
        private DispatcherQueue _dispatcherQueue;
        public About()
        {
            this.InitializeComponent();

            // �ڹ��캯���������ʵ�λ�����ð汾��
            var package = Package.Current;
            var version = package.Id.Version;
            // ��ȡUI�̵߳�DispatcherQueue
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            APPVersion.Content = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            APPVersion.NavigateUri = new System.Uri($"https://github.com/Direct5dom/WinWoL/releases/tag/{version.Major}.{version.Minor}.{version.Build}.{version.Revision}");

            GetSponsorList();
        }

        private void AboutAliPay_Click(object sender, RoutedEventArgs e)
        {
            AboutAliPayTips.IsOpen = true;
        }
        private void AboutWePay_Click(object sender, RoutedEventArgs e)
        {
            AboutWePayTips.IsOpen = true;
        }
        private void GetSponsorList()
        {
            // �����߳���ִ������
            Thread subThread = new Thread(new ThreadStart(async () =>
            {
                string nameList = null;
                using (HttpClient client = new HttpClient())
                {
                    // ����GET�����Ի�ȡ�ļ�����
                    // ���ȳ��Դ�GitHub��ȡ����
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync($"https://raw.githubusercontent.com/Direct5dom/Direct5dom/main/README/Sponsor/List");
                        if (response.IsSuccessStatusCode)
                        {
                            // ��GitHub����Ӧ�ж�ȡ�ļ�����
                            nameList = await response.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            nameList = "Try Gitee";
                        }
                    }
                    catch
                    {
                        nameList = "Try Gitee";
                    }

                    // ���GitHubͨ��ʧ�ܣ����Դ�Gitee��ȡ����
                    if (nameList == "Try Gitee")
                    {
                        try
                        {
                            HttpResponseMessage response = await client.GetAsync($"https://gitee.com/XiaolongSI/Direct5dom/raw/main/README/Sponsor/List");
                            if (response.IsSuccessStatusCode)
                            {
                                // ��Gitee����Ӧ�ж�ȡ�ļ�����
                                nameList = await response.Content.ReadAsStringAsync();
                            }
                            else
                            {
                                nameList = "�޷������� Github �� Gitee ��ȡ������������(0)";
                            }
                        }
                        catch
                        {
                            nameList = "�޷������� Github �� Gitee ��ȡ������������(1)";
                        }
                    }
                }
                _dispatcherQueue.TryEnqueue(() =>
                {
                    NameList.Text = nameList;
                });
            }));
            subThread.Start();
        }
    }
}
