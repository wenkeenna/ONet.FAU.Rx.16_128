using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Shared.Events;
using ONet.FAU.Rx._16_128.Extension.Common;
using ONet.FAU.Rx._16_128.Extension.Converters;
using ONet.FAU.Rx._16_128.Extension.Model;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ONet.FAU.Rx._16_128.Extension.ViewModels
{
    public class OpticalModuleViewModel : BindableBase, IDestructible
    {

        private Brush _displayColor = Brushes.Gray;

        private IEventAggregator _eventAggregator = null;

        private IDataBindingContext _dataBinding;

        private CancellationTokenSource _loopCts;

        private ChannelGroup _selectedGroup = ChannelGroup.None;

        public ChannelGroup SelectedGroup
        {
            get => _selectedGroup;
            set => SetProperty(ref _selectedGroup, value, onChanged: OnGroupChangedAsync);
        }


        private ChannelGroup CurrentGroup = ChannelGroup.None;

        private bool SelectedGroupIsDone = false;



        private string _adcunit;
        public string AdcUnit
        {
            get => _adcunit;
            set => SetProperty(ref _adcunit, value);
        }

        private string _mpdiunit;
        public string MPDiUnit
        {
            get => _mpdiunit;
            set => SetProperty(ref _mpdiunit, value);
        }


        private string _mpdounit;
        public string MPDoUnit
        {
            get => _mpdounit;
            set => SetProperty(ref _mpdounit, value);
        }



        private double _voltage;
        public double Voltage
        {
            get => _voltage;
            set => SetProperty(ref _voltage, value);
        }


        private double _temp;
        public double Temp
        {
            get => _temp;
            set => SetProperty(ref _temp, value);
        }


        // 三組各自的 8 個通道
        public ChannelData[] AdcChannels { get; } = new ChannelData[8];
        public ChannelData[] MpDiChannels { get; } = new ChannelData[8];
        public ChannelData[] MpDoChannels { get; } = new ChannelData[8];

        private readonly IContainerProvider _containerProvider;
        private OpticalModuleService _opticalModuleService;
        private ILogger _logger;

        //private System.Timers.Timer _refreshTimer;

        private Task _pollingTask;
        private CancellationTokenSource _pollingCts;
        public DelegateCommand<object> CheckedLaserCmd { get; private set; }


        private const string DEV_NAME = "M8811";
        private const string DEV_NAME_COUPLING = "Coupling";
        private const string PRODUCT_TYPE = "1_6T";

        private const string LOG_Title = "1.6T光模块:";

        private bool _isPollingActive = false;


        public OpticalModuleViewModel(IEventAggregator eventAggregator, IContainerProvider containerProvider, IDataBindingContext dataBinding)
        {
            try
            {
                _eventAggregator = eventAggregator;
                _containerProvider = containerProvider;

                CheckedLaserCmd = new DelegateCommand<object>(OnCheckedLaserCmdAsync);

                // 初始化所有通道
                for (int i = 0; i < 8; i++)
                {
                    AdcChannels[i] = new ChannelData();
                    MpDiChannels[i] = new ChannelData();
                    MpDoChannels[i] = new ChannelData();
                }

                AdcUnit = "uA";
                MPDiUnit = "uA";
                MPDoUnit = "uA";


                _opticalModuleService = _containerProvider.Resolve<OpticalModuleService>();
                _logger = _containerProvider.Resolve<ILogger>();



                _dataBinding = dataBinding;

                _eventAggregator.GetEvent<AppStartUpEvent>().Subscribe(OnAppStartUpEvent);
                _eventAggregator.GetEvent<InstrmentKitCommandEvent>().Subscribe(OnInstrmentKitCommandEvent);
            }
            catch (Exception ex)
            {

                _logger?.Error(ex.ToString());
            }



        }

        private async void OnCheckedLaserCmdAsync(object obj)
        {
            if (obj.ToString() == "Group1")
            {
                await _opticalModuleService.SetLaserStateAsync(1);
            }
            else if (obj.ToString() == "Group2")
            {
                await _opticalModuleService.SetLaserStateAsync(2);
            }
            else if (obj.ToString() == "Group3")
            {
                await _opticalModuleService.SetLaserStateAsync(3);
            }
            else if (obj.ToString() == "Group4")
            {
                await _opticalModuleService.SetLaserStateAsync(4);
            }
        }

        private void OnAppStartUpEvent(object obj)
        {
            try
            {
                var portNum = _dataBinding.Get("仪表端口号", "光模块").Value;


                string ProductType = _dataBinding.Get("产品类型", "产品名称").Value;

                if (ProductType == PRODUCT_TYPE)
                {
                    if (!_opticalModuleService.Open(portNum))
                    {
                        _eventAggregator.GetEvent<Event_Message>().Publish("1.6T光模块：打开串口失败。");

                        return;
                    }
                }

            }
            catch (Exception ex)
            {

                _eventAggregator.GetEvent<Event_Message>().Publish($"{ex.Message}");
                _logger?.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 仪表命令事件处理
        /// </summary>
        /// <param name="obj"></param>
        private async void OnInstrmentKitCommandEvent(string obj)
        {
            try
            {
                string[] CMD_Parse = obj.Split(':');
                if (CMD_Parse[0] == DEV_NAME)
                {
                    if (CMD_Parse.Length < 3)
                    {
                        _eventAggregator.GetEvent<Event_Message>().Publish("1.6T光模块: 命令格式错误。");
                        return;
                    }
                }
                else
                {
                    return;
                }
              

                // 处理 ON 命令
                if (CMD_Parse[0] == DEV_NAME && CMD_Parse[1] == PRODUCT_TYPE)
                {
                    if (CMD_Parse[2] == "ON")
                    {
                        _logger.Error("1.6T光模块：接收源表启动命令，启动模块数据采集...");
                        await Task.Delay(1000);
                        var res = await _opticalModuleService.SetPawssword();
                        if (!res)
                        {
                            _logger.Error("1.6T光模块:写入密码失败");
                            _eventAggregator.GetEvent<Event_Message>().Publish("1.6T光模块:写入密码失败");
                            return;
                        }
                        else
                        {
                            _logger.Error("1.6T光模块:写入密码成功");
                            _eventAggregator.GetEvent<Event_Message>().Publish("1.6T光模块:写入密码成功");
                        }


                        _isPollingActive = true;

                        StartPolling();
                        _logger?.Info($"{LOG_Title}收到 ON 命令，启动轮询");

                    }
                    else if (CMD_Parse[2] == "OFF")
                    {
                        _isPollingActive = false;
                        StopPollingAsync();
                        _logger?.Info($"{LOG_Title}收到 OFF 命令，停止轮询");
                    }
                }


                if (CMD_Parse[0] == DEV_NAME_COUPLING && CMD_Parse[1] == PRODUCT_TYPE)
                {
                    if (CMD_Parse[2] == "ON")
                    {
                        _isPollingActive = true;

                        _logger.Error("1.6T光模块：接收耦合启动命令，启动模块数据采集...");

                        SelectedGroup = (ChannelGroup)Enum.Parse(typeof(ChannelGroup), CMD_Parse[3]);

                        StartPolling();
                        _logger?.Info($"{LOG_Title}:{DEV_NAME_COUPLING}收到 ON 命令，启动轮询");
                        _eventAggregator.GetEvent<Event_Message>().Publish($"{LOG_Title}:{DEV_NAME_COUPLING}收到 ON 命令，启动轮询");


                    }
                    else if (CMD_Parse[2] == "OFF")
                    {
                        _isPollingActive = false;
                        _logger.Error("1.6T光模块：接收耦合停止命令，停止模块数据采集...");
                        StopPollingAsync();
                        _logger?.Info($"{LOG_Title}:{DEV_NAME_COUPLING}收到 OFF 命令，停止轮询");
                        _eventAggregator.GetEvent<Event_Message>().Publish($"{LOG_Title}:{DEV_NAME_COUPLING}收到 OFF 命令，停止轮询");
                    }
                }

            }
            catch (Exception ex)
            {
                _logger?.Error(ex.ToString());
            }

        }


        // 启动轮询
        private void StartPolling()
        {
            // 先确保旧的完全停止
            StopPollingAsync();

            _pollingCts = new CancellationTokenSource();
            _isPollingActive = true;

            _logger?.Info($"{LOG_Title}启动实时采集刷新...");

            _pollingTask = Task.Run(async () =>
            {
                try
                {
                    while (!_pollingCts.IsCancellationRequested)
                    {
                        try
                        {
                            await RefreshCurrentGroupAsync(_pollingCts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            // 正常取消，不记录错误
                        }
                        catch (Exception ex)
                        {
                            _logger?.Error($"{LOG_Title}轮询刷新单次执行异常: {ex.Message}");
                            // 可选：短暂等待再继续，避免高频错误刷屏
                            await Task.Delay(100, _pollingCts.Token);
                        }

                        try
                        {
                            await Task.Delay(200, _pollingCts.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            break; // 提前退出循环
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // 最外层取消
                }
                catch (Exception ex)
                {
                    _logger?.Error($"{LOG_Title}轮询循环异常退出: {ex.Message}");
                }
                finally
                {
                    _logger?.Info($"{LOG_Title}轮询循环已结束");
                }
            }, _pollingCts.Token);
        }

        // 停止轮询（必须配套实现）
        private async Task StopPollingAsync()
        {


            _isPollingActive = false;

            try
            {
                _pollingCts?.Cancel();
            }
            catch { }

            // 可选：等待任务结束（非必须，但有助于清理）
            // 如果不等待，任务会在后台继续跑完（通常几秒内结束）
            // 如果你希望确保停止后再做其他事，可以加上：
            try { await _pollingTask.ConfigureAwait(false); } catch { }

            // 清理资源
            if (_pollingCts != null)
            {
                try { _pollingCts.Dispose(); } catch { }
                _pollingCts = null;
            }

            _pollingTask = null;  // 可选，帮助 GC

            _logger?.Info($"{LOG_Title}已停止轮询");
        }

        // 刷新方法（需支持 token）
        private async Task RefreshCurrentGroupAsync(CancellationToken ct)
        {
            if (!_isPollingActive || SelectedGroup == ChannelGroup.None)
                return;

            ct.ThrowIfCancellationRequested();  // 提前检查

            try
            {
                await UpdateChannelsForCurrentGroupAsync(ct);
            }
            catch (OperationCanceledException)
            {
                return; // 让上层捕获
            }
            catch (Exception ex)
            {
                _logger?.Error($"{LOG_Title}刷新当前组失败: {ex.Message}");
                _eventAggregator.GetEvent<Event_Message>().Publish("数据刷新异常，请检查设备");
            }
        }


        private async Task UpdateChannelsForCurrentGroupAsync(CancellationToken ct = default)
        {
            if (SelectedGroup == ChannelGroup.None) return;

            if (SelectedGroup != CurrentGroup)
            {
                int startCh = (int)SelectedGroup;
                var res = await _opticalModuleService.SetLaserStateAsync(startCh);

                CurrentGroup = SelectedGroup;
            }



            const int count = 8;

            try
            {

                var adcValues = await _opticalModuleService.ReadIMPDAsync(8);


                var tempvcc = await _opticalModuleService.ReadVccTemp((int)SelectedGroup);

                Voltage = tempvcc.vcc / 10000;
                Temp = tempvcc.temp / 256;

                if (adcValues == null)
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish("读取通道数据失败");
                    return;
                }

                for (int i = 0; i < count; i++)
                {
                    // ADC (RSSI)
                    AdcChannels[i].Current = adcValues[i];
                    AdcChannels[i].StatusColor = GetStatusColor(adcValues[i]);

                    //// MPDi
                    //MpDiChannels[i].Current = mpdiValues[i];
                    //MpDiChannels[i].StatusColor = GetStatusColor(mpdiValues[i]);

                    //// MPDo
                    //MpDoChannels[i].Current = mpdoValues[i];
                    //MpDoChannels[i].StatusColor = GetStatusColor(mpdoValues[i]);
                }

                // 通知 UI 更新（Prism MVVM 常用方式）
                RaisePropertyChanged(nameof(AdcChannels));
                RaisePropertyChanged(nameof(MpDiChannels));
                RaisePropertyChanged(nameof(MpDoChannels));
            }
            catch (OperationCanceledException)
            {
                // 被取消时安静退出，不报错
            }
            catch (Exception ex)
            {
                _logger?.Error($"{LOG_Title}更新通道数据异常: {ex.Message}");
                _eventAggregator.GetEvent<Event_Message>().Publish($"通道更新失败: {ex.Message}");
            }
        }


        public void Destroy()
        {
            StopPollingAsync();

            _pollingCts = null;
            _pollingTask = null;

            _logger?.Info($"{LOG_Title} ViewModel 已销毁，轮询资源已释放");

        }


        private async void OnGroupChangedAsync()
        {
            //if (SelectedGroup == ChannelGroup.None) return;

            //SelectedGroupIsDone = false;

            //int startCh = (int)SelectedGroup;
            //var res = await _opticalModuleService.SetLaserStateAsync(startCh);

            //if (!res)
            //{
            //    _logger.Error("1.6T光模块:设置激光器状态失败");
            //    _eventAggregator.GetEvent<Event_Message>().Publish("1.6T光模块:设置激光器状态失败");
            //    return;
            //}
            //else
            //{
            //    _logger.Error("1.6T光模块:设置激光器状态成功");
            //    _eventAggregator.GetEvent<Event_Message>().Publish("1.6T光模块:设置激光器状态成功");
            //}
            //_eventAggregator.GetEvent<Event_Message>().Publish($"激光器选择:{SelectedGroup}");

            //await Task.Delay(300);

            //SelectedGroupIsDone = true;


        }



        // 简单示例：根据值设置颜色
        private Brush GetStatusColor(ushort rawValue)
        {
            double value = rawValue; // 可加转换

            if (value > 3000) return Brushes.Green;     // 正常高值
            if (value > 1000) return Brushes.Orange;    // 中间
            if (value > 100) return Brushes.Red;       // 异常低
            return Brushes.Gray;                        // 无信号或极低
        }



        private int GetStartChannelForGroup()
        {
            // 假设组1 = CH1~8, 组2 = CH9~16, 组3 = CH17~24, 组4 = CH25~32
            return ((int)SelectedGroup - 1) * 8;  // ChannelGroup.Group1 = 1, Group2 = 2 ...
        }

        public Task ReleaseAsync(CancellationToken cancellationToken = default)
        {
            StopPollingAsync();
            return Task.CompletedTask;
        }


    }
}
