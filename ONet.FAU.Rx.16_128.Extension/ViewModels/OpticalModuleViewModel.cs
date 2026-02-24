using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.Shared.Events;
using ONet.FAU.Rx._16_128.Extension.Common;
using ONet.FAU.Rx._16_128.Extension.Converters;
using ONet.FAU.Rx._16_128.Extension.Model;
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



        // 三組各自的 8 個通道
        public ChannelData[] AdcChannels { get; } = new ChannelData[8];
        public ChannelData[] MpDiChannels { get; } = new ChannelData[8];
        public ChannelData[] MpDoChannels { get; } = new ChannelData[8];

        private readonly IContainerProvider _containerProvider;
        private OpticalModuleService  _opticalModuleService;

        private System.Timers.Timer _refreshTimer;

        public OpticalModuleViewModel(IEventAggregator eventAggregator, IContainerProvider containerProvider, IDataBindingContext dataBinding) 
        {
            _eventAggregator= eventAggregator;
            _containerProvider= containerProvider;

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

            _refreshTimer = new System.Timers.Timer(3000);          // 每 3 秒触发一次
            _refreshTimer.Elapsed += async (s, e) => await RefreshCurrentGroupAsync();
            _refreshTimer.AutoReset = true;                         // 自动重复
            _refreshTimer.SynchronizingObject = null;               // 不需要 UI 线程同步（异步处理）
        }


        // 开始轮询（在 OnGroupChanged 中调用）
        private void StartPolling()
        {
            if (_refreshTimer.Enabled) return;
            _refreshTimer.Start();
        }

        // 停止轮询（在切换组或销毁时调用）
        private void StopPolling()
        {
            _refreshTimer?.Stop();
        }

        // 刷新方法（之前提到的 UpdateChannelsForCurrentGroupAsync 改个名）
        private async Task RefreshCurrentGroupAsync()
        {
            if (SelectedGroup == ChannelGroup.None) return;

            // 你的读取和更新逻辑...
            await UpdateChannelsForCurrentGroupAsync();
        }



        public void Destroy()
        {
            StopPolling();
            _refreshTimer?.Dispose();
            _refreshTimer = null;
        }


        private void  OnGroupChangedAsync()
        {
            if (SelectedGroup == ChannelGroup.None) return;

            _eventAggregator.GetEvent<Event_Message>().Publish(SelectedGroup.ToString());


         
            StartPolling();
        }

        private async Task UpdateChannelsForCurrentGroupAsync()
        {
            if (SelectedGroup == ChannelGroup.None) return;

            int startCh = GetStartChannelForGroup();  // 0,8,16,24
            const int count = 8;

            try
            {
                // 读取三组数据（MPDo, MPDi, RSSI 或 ADC，根据你的实际需求）
                // 这里假设 ADC 对应 RSSI，MPDi 对应 MPDi，MPDo 对应 MPDo
                var mpdoValues = await _opticalModuleService.ReadMPDoAsync(startCh, count);
                var mpdiValues = await _opticalModuleService.ReadMPDiAsync(startCh, count);
                var adcValues = await _opticalModuleService.ReadRSSIAsync(startCh, count);  // 假设 ADC 用 RSSI 寄存器

                if (mpdoValues == null || mpdiValues == null || adcValues == null)
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish("读取通道数据失败");
                    return;
                }

                // 更新三组通道
                for (int i = 0; i < count; i++)
                {
                    // ADC
                    AdcChannels[i].Current = adcValues[i];   // 或加单位转换：adcValues[i] * factor
                    AdcChannels[i].StatusColor = GetStatusColor(adcValues[i]);

                    // MPDi
                    MpDiChannels[i].Current = mpdiValues[i];
                    MpDiChannels[i].StatusColor = GetStatusColor(mpdiValues[i]);

                    // MPDo
                    MpDoChannels[i].Current = mpdoValues[i];
                    MpDoChannels[i].StatusColor = GetStatusColor(mpdoValues[i]);
                }

                RaisePropertyChanged(nameof(AdcChannels));    // 通知 UI 更新
                RaisePropertyChanged(nameof(MpDiChannels));
                RaisePropertyChanged(nameof(MpDoChannels));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<Event_Message>().Publish($"更新通道异常: {ex.Message}");
            }
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



    }
}
