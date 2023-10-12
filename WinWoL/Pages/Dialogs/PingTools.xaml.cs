using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System.Threading;
using WinWoL.Methods;
using WinWoL.Models;

namespace WinWoL.Pages.Dialogs
{
    public sealed partial class PingTools : ContentDialog
    {
        private DispatcherQueue _dispatcherQueue;
        public WoLModel WoLData { get; private set; }
        public PingTools(WoLModel wolModel)
        {
            this.InitializeComponent();

            // ��ȡUI�̵߳�DispatcherQueue
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            if (wolModel.IPAddress != null)
            {
                // �����߳���ִ������
                Thread subThread = new Thread(new ThreadStart(() =>
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        PingRef.Text = "������";
                    });
                    string PingRes;
                    while (true)
                    {
                        if (WoLMethod.PingTest(wolModel.IPAddress) == "TimedOut")
                        {
                            PingRes = "��ʱ";
                        }
                        else
                        {
                            PingRes = WoLMethod.PingTest(wolModel.IPAddress);
                        }

                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            PingRef.Text = PingRes;
                        });
                        Thread.Sleep(1000);
                    }
                }));
                subThread.Start();
                // �����߳���ִ������
                Thread subThread2 = new Thread(new ThreadStart(() =>
                {
                    string WoLPingRes;
                    // �ṩ��WoL�˿�
                    if (wolModel.WoLPort != null && wolModel.WoLPort != "")
                    {
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            WoLPingRef.Text = "������";
                        });
                        while (true)
                        {
                            if (WoLMethod.TCPingTest(wolModel.IPAddress, wolModel.WoLPort) == -1)
                            {
                                WoLPingRes = "��ʱ";
                            }
                            else
                            {
                                WoLPingRes = $"{WoLMethod.TCPingTest(wolModel.IPAddress, wolModel.WoLPort)} ms";
                            }
                            _dispatcherQueue.TryEnqueue(() =>
                            {
                                WoLPingRef.Text = WoLPingRes;
                            });
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            WoLPingRef.Text = "������";
                        });
                    }

                }));
                subThread2.Start();
                // �����߳���ִ������
                Thread subThread3 = new Thread(new ThreadStart(() =>
                {
                    string RDPPingRes;
                    // �ṩ��RDP�˿�
                    if (wolModel.RDPPort != null && wolModel.RDPPort != "")
                    {
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            RDPPingRef.Text = "������";
                        });
                        while (true)
                        {
                            if (WoLMethod.TCPingTest(wolModel.IPAddress, wolModel.RDPPort) == -1)
                            {
                                RDPPingRes = "��ʱ";
                            }
                            else
                            {
                                RDPPingRes = $"{WoLMethod.TCPingTest(wolModel.IPAddress, wolModel.WoLPort)} ms";
                            }
                            _dispatcherQueue.TryEnqueue(() =>
                            {
                                RDPPingRef.Text = RDPPingRes;
                            });
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            RDPPingRef.Text = "������";
                        });
                    }
                }));
                subThread3.Start();
                // �����߳���ִ������
                Thread subThread4 = new Thread(new ThreadStart(() =>
                {
                    string SSHPingRes;
                    // �ṩ��SSH�˿�
                    if (wolModel.SSHPort != null && wolModel.SSHPort != "")
                    {
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            SSHPingRef.Text = "������";
                        });
                        while (true)
                        {
                            if (WoLMethod.TCPingTest(wolModel.IPAddress, wolModel.SSHPort) == -1)
                            {
                                SSHPingRes = "��ʱ";
                            }
                            else
                            {
                                SSHPingRes = $"{WoLMethod.TCPingTest(wolModel.IPAddress, wolModel.WoLPort)} ms";
                            }
                            _dispatcherQueue.TryEnqueue(() =>
                            {
                                SSHPingRef.Text = SSHPingRes;
                            });
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            SSHPingRef.Text = "������";
                        });
                    }
                }));
                subThread4.Start();
            }
        }
    }
}