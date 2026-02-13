using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Shared.Events;
using ONet.FAU.Rx._16_128.Extension.Common;
using ONet.FAU.Rx._16_128.Extension.Model;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ONet.FAU.Rx._16_128.Extension.ViewModels
{
    public class GolightOSMWD41310ViewModel : BindableBase
    {
        // 我们定义四个独立的属性，方便与你之前的 XAML 布局一一对应
        private LaserChannelModel _ch1;
        private LaserChannelModel _ch2;
        private LaserChannelModel _ch3;
        private LaserChannelModel _ch4;

        private IEventAggregator _eventAggregator;
        private IDataBindingContext _dataBinding;
        private GolightOSMWD41310Helper _golightOSMWD41310Helper;

        private readonly IContainerProvider _containerProvider;

        private CancellationTokenSource _loopCts;
        private ILogger _logger;
        public GolightOSMWD41310ViewModel(IEventAggregator eventAggregator, IContainerProvider containerProvider, IDataBindingContext dataBinding, ILogger logger) 
        {
            // 初始化通道数据
            Ch1 = new LaserChannelModel { ChannelName = "CH1", SetPower = 10.0 };
            Ch2 = new LaserChannelModel { ChannelName = "CH2", SetPower = 10.0 };
            Ch3 = new LaserChannelModel { ChannelName = "CH3", SetPower = 0.0 };
            Ch4 = new LaserChannelModel { ChannelName = "CH4", SetPower = 0.0 };

            // 命令绑定
            ToggleCh1Cmd = new DelegateCommand(() => Ch1.IsActive = !Ch1.IsActive);
            ToggleCh2Cmd = new DelegateCommand(() => Ch2.IsActive = !Ch2.IsActive);
            ToggleCh3Cmd = new DelegateCommand(() => Ch3.IsActive = !Ch3.IsActive);
            ToggleCh4Cmd = new DelegateCommand(() => Ch4.IsActive = !Ch4.IsActive);

            UpdateAllPowerCmd = new DelegateCommand(ExecuteUpdateAll);
            EmergencyStopCmd = new DelegateCommand(ExecuteEmergencyStop);

            TurnOnAllChannelsCommand = new DelegateCommand(OnTurnOnAllChannelsCommand);
            TurnOffAllChannelsCommand = new DelegateCommand(OnTurnOffAllChannelsCommand);


            _eventAggregator = eventAggregator;


            _containerProvider = containerProvider;
            _golightOSMWD41310Helper = _containerProvider.Resolve<GolightOSMWD41310Helper>("LaserLightSourceA");

       
            _dataBinding = dataBinding;
            _logger = logger;

            _eventAggregator.GetEvent<AppStartUpEvent>().Subscribe(OnAppStartUpEvent);


        }


        private void OnTurnOffAllChannelsCommand()
        {
            try
            {
                // 全局切断逻辑
                Ch1.IsActive = false;
                Ch2.IsActive = false;
                Ch3.IsActive = false;
                Ch4.IsActive = false;
                // 立即发送物理切断指令
                _golightOSMWD41310Helper.SetChannelSwitch(1, false);
                _golightOSMWD41310Helper.SetChannelSwitch(2, false);
                _golightOSMWD41310Helper.SetChannelSwitch(3, false);
                _golightOSMWD41310Helper.SetChannelSwitch(4, false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
        }

        private void OnTurnOnAllChannelsCommand()
        {
            try
            {
                _golightOSMWD41310Helper.SetChannelSwitch(1, true);
                _golightOSMWD41310Helper.SetChannelSwitch(2, true);
                _golightOSMWD41310Helper.SetChannelSwitch(3, true);
                _golightOSMWD41310Helper.SetChannelSwitch(4, true);

                Ch1.IsActive = true;
                Ch2.IsActive = true;
                Ch3.IsActive = true;
                Ch4.IsActive = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
        }




        private void OnAppStartUpEvent(object obj)
        {
            try
            {
                var portNum = _dataBinding.Get("仪表端口号", "激光光源_A").Value;

                if (!_golightOSMWD41310Helper.Open(portNum))
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish("激光光源：打开串口失败。");

                    return;
                }
               //Ch1.ActualPower = _golightOSMWD41310Helper.GetPower(1);

              

                _golightOSMWD41310Helper.SetChannelSwitch(1,true);
                _golightOSMWD41310Helper.SetChannelSwitch(2, true);
                _golightOSMWD41310Helper.SetChannelSwitch(3, true);
                _golightOSMWD41310Helper.SetChannelSwitch(4, true);
                var brushConverter = new BrushConverter();
                Ch1.IsActive = true;
                Ch2.IsActive = true;
                Ch3.IsActive = true;
                Ch4.IsActive = true;

                StartPolling();

            }
            catch (Exception ex)
            {

                _eventAggregator.GetEvent<Event_Message>().Publish($"{ex.Message}");
            }
        }

        #region 属性定义

        public LaserChannelModel Ch1 { get => _ch1; set => SetProperty(ref _ch1, value); }
        public LaserChannelModel Ch2 { get => _ch2; set => SetProperty(ref _ch2, value); }
        public LaserChannelModel Ch3 { get => _ch3; set => SetProperty(ref _ch3, value); }
        public LaserChannelModel Ch4 { get => _ch4; set => SetProperty(ref _ch4, value); }

        #endregion

        #region 命令定义

        public DelegateCommand ToggleCh1Cmd { get; }
        public DelegateCommand ToggleCh2Cmd { get; }
        public DelegateCommand ToggleCh3Cmd { get; }
        public DelegateCommand ToggleCh4Cmd { get; }
        public DelegateCommand UpdateAllPowerCmd { get; }
        public DelegateCommand EmergencyStopCmd { get; }

        public DelegateCommand TurnOnAllChannelsCommand { get; }
        public DelegateCommand TurnOffAllChannelsCommand { get; }

        private void ExecuteUpdateAll()
        {
            // 模拟下发指令给底层硬件驱动
            // LaserService.SetPower(1, Ch1.SetPower);
            // ...以此类推
        }

        private void ExecuteEmergencyStop()
        {
            // 全局切断逻辑
            Ch1.IsActive = false;
            Ch2.IsActive = false;
            Ch3.IsActive = false;
            Ch4.IsActive = false;
            // 立即发送物理切断指令
        }

        #endregion


        private void StartPolling()
        {
            // 确保不会重复启动
            _loopCts?.Cancel();
            _loopCts = new CancellationTokenSource();

            var token = _loopCts.Token;

            // 启动后台长时间运行的任务
            Task.Run(async () =>
            {

                _eventAggregator.GetEvent<Event_Message>().Publish($"激光光源:启动实时采集...");


                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (_golightOSMWD41310Helper.IsOpen)
                        {
                            // 调用之前优化过的元组方法
                            Ch1.ActualPower = _golightOSMWD41310Helper.GetPower(1);
                            Ch2.ActualPower = _golightOSMWD41310Helper.GetPower(2);
                            Ch3.ActualPower = _golightOSMWD41310Helper.GetPower(3);
                            Ch4.ActualPower = _golightOSMWD41310Helper.GetPower(4);


                        }
                    }
                    catch (Exception ex)
                    {
                        // 记录错误，防止循环因为单个读取失败而崩溃
                        System.Diagnostics.Debug.WriteLine("激光光源 Error: " + ex.Message);
                    }

                    // 设置读取间隔，例如 500ms 读取一次
                    await Task.Delay(500, token);
                }

            }, token);
        }

    }
}
