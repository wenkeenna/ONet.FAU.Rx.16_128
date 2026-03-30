using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Shared.Events;
using ImTools;
using ONet.FAU.Rx._16_128.Extension.Common;
using ONet.FAU.Rx._16_128.Extension.Model;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.ViewModels
{
    public class LD9204S_B_ViewModel : BindableBase
    {
        private string _selectedUnit = "dBm";

        #region 属性
        public ObservableCollection<PowerMeterChannelModel> Channels { get; }
        public List<string> UnitList { get; }

        // 全局单位切换
        public string SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (SetProperty(ref _selectedUnit, value))
                {
                    foreach (var channel in Channels)
                    {
                        channel.CurrentUnit = value;
                    }
                }
            }
        }

        #endregion

        #region 命令

        public DelegateCommand AllZeroCmd { get; }
        public DelegateCommand SingleSampleCmd { get; }

        #endregion


        private IEventAggregator _eventAggregator;
        private IDataBindingContext _dataBinding;
        private LD9208Controller _ld9208ControllerC, _ld9208ControllerD;
        private readonly IContainerProvider _containerProvider;

        private CancellationTokenSource _loopCts;
        private ILogger _logger;

        public LD9204S_B_ViewModel(IEventAggregator eventAggregator, IContainerProvider containerProvider, IDataBindingContext dataBinding, ILogger logger)
        {

            // 初始化四个通道，分配初始波长
            Channels = new ObservableCollection<PowerMeterChannelModel>
            {
                new PowerMeterChannelModel { ChannelName = "CH1", Wavelength = "1550" },
                new PowerMeterChannelModel { ChannelName = "CH2", Wavelength = "1550" },
                new PowerMeterChannelModel { ChannelName = "CH3", Wavelength = "1550" },
                new PowerMeterChannelModel { ChannelName = "CH4", Wavelength = "1550" },

                new PowerMeterChannelModel { ChannelName = "CH5", Wavelength = "1550" },
                new PowerMeterChannelModel { ChannelName = "CH6", Wavelength = "1550" },
                new PowerMeterChannelModel { ChannelName = "CH7", Wavelength = "1550" },
                new PowerMeterChannelModel { ChannelName = "CH8", Wavelength = "1550" }
            };

            UnitList = new List<string> { "dBm", "mW", "uW" };
            _containerProvider = containerProvider;
         

            // 初始化命令
            AllZeroCmd = new DelegateCommand(ExecuteAllZeroCmd);
            SingleSampleCmd = new DelegateCommand(ExecuteSingleSampleCmd);

            _eventAggregator = eventAggregator;


            _ld9208ControllerC = _containerProvider.Resolve<LD9208Controller>("OpticalPowerMeterC");
            _ld9208ControllerD = _containerProvider.Resolve<LD9208Controller>("OpticalPowerMeterD");


            _dataBinding = dataBinding;
            _logger = logger;

            _eventAggregator.GetEvent<AppStartUpEvent>().Subscribe(OnAppStartUpEvent);
            _eventAggregator.GetEvent<InstrmentKitCommandEvent>().Subscribe(OnInstrmentKitCommandEvent);
            // 模拟实时数据刷新 (实际开发中由硬件监听器触发)
            //  StartSimulation();
        }

        private const string DEV_NAME = "LD9204S_B";
        private const string DEV_STATE_ON = "ON";
        private const string DEV_STATE_OFF = "OFF";
      

        private bool _isPollingActive = false;

        private void OnInstrmentKitCommandEvent(string obj)
        {
            //string[] CMD_Parse = obj.Split(':');

            //if (CMD_Parse.Length < 2)
            //{
            //    _eventAggregator.GetEvent<Event_Message>().Publish($"{DEV_NAME}: 命令格式错误:{obj}。");
            //    return;
            //}

            //if (CMD_Parse[0] == DEV_NAME && CMD_Parse[1] == DEV_STATE_ON)
            //{
             
            //        _logger.Error($"{DEV_NAME}：接收启动命令，启动数据采集...");
            //    _isPollingActive = true;

            //    StartPolling();


            //}
            //else if(CMD_Parse[0] == DEV_NAME && CMD_Parse[1] == DEV_STATE_OFF)
            //{
            //    _logger?.Info($"{DEV_NAME}收到 OFF 命令，停止轮询");
            //    _eventAggregator.GetEvent<Event_Message>().Publish($"{DEV_NAME}收到 OFF 命令，停止轮询");
            //    _isPollingActive = false;
            //    // StopPollingAsync();

            //    _loopCts?.Cancel();
            //}


        }

        private void OnAppStartUpEvent(object obj)
        {
            try
            {
                var portNumA = _dataBinding.Get("仪表端口号", "光功率计_C").Value;
                var portNumB = _dataBinding.Get("仪表端口号", "光功率计_D").Value;

                if (!_ld9208ControllerC.Open(portNumA))
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish("光功率计C：打开串口失败。");

                    return;
                }

                if (!_ld9208ControllerD.Open(portNumB))
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish("光功率计D：打开串口失败。");

                    return;
                }




                Channels[0].Wavelength = _ld9208ControllerC.GetWavelength(1);
                Channels[1].Wavelength = _ld9208ControllerC.GetWavelength(2);
                Channels[2].Wavelength = _ld9208ControllerC.GetWavelength(3);
                Channels[3].Wavelength = _ld9208ControllerC.GetWavelength(4);

                Channels[4].Wavelength = _ld9208ControllerD.GetWavelength(1);
                Channels[5].Wavelength = _ld9208ControllerD.GetWavelength(2);
                Channels[6].Wavelength = _ld9208ControllerD.GetWavelength(3);
                Channels[7].Wavelength = _ld9208ControllerD.GetWavelength(4);





                StartPolling();

            }
            catch (Exception ex)
            {

                _eventAggregator.GetEvent<Event_Message>().Publish($"{ex.Message}");
            }
        }

        private void ExecuteSingleSampleCmd()
        {

        }

        private void ExecuteAllZeroCmd()
        {

        }


        private void StartPolling()
        {
            // 确保不会重复启动
            _loopCts?.Cancel();
            _loopCts = new CancellationTokenSource();

            var token = _loopCts.Token;

            // 启动后台长时间运行的任务
            Task.Run(async () =>
            {

                _eventAggregator.GetEvent<Event_Message>().Publish($"光功率计_CD:启动实时采集...");


                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (_ld9208ControllerC.IsOpen)
                        {
                            // 调用之前优化过的元组方法
                            var data  = _ld9208ControllerC.GetAllPower().Split(',');
                            var dataB = _ld9208ControllerD.GetAllPower().Split(',');
                          

                            for (int i = 0; i < data.Length; i++)
                            {
                                Channels[i].RawPowerValue = Convert.ToDouble(data[i]);
                            }

                            for (int i = 0; i < dataB.Length; i++)
                            {
                                Channels[i + 4].RawPowerValue = Convert.ToDouble(dataB[i]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 记录错误，防止循环因为单个读取失败而崩溃
                        System.Diagnostics.Debug.WriteLine("光功率计_CD Error: " + ex.Message);
                    }

                    // 设置读取间隔，例如 500ms 读取一次
                    await Task.Delay(500, token);
                }
                _eventAggregator.GetEvent<Event_Message>().Publish($"{DEV_NAME}: 跳出循环。");



            }, token);
        }




    }
}
