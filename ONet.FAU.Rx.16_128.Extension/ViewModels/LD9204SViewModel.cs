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
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ONet.FAU.Rx._16_128.Extension.ViewModels
{
    public class LD9204SViewModel : BindableBase
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
        private LD9208Controller _ld9208Controller;
        private readonly IContainerProvider _containerProvider;
        private CancellationTokenSource _loopCts;
        private ILogger _logger;

        public LD9204SViewModel(IEventAggregator eventAggregator, IContainerProvider containerProvider, IDataBindingContext dataBinding,ILogger logger)
        {

            // 初始化四个通道，分配初始波长
            Channels = new ObservableCollection<PowerMeterChannelModel>
            {
                new PowerMeterChannelModel { ChannelName = "CH1", Wavelength = "1550" },
                new PowerMeterChannelModel { ChannelName = "CH2", Wavelength = "1550" },
                new PowerMeterChannelModel { ChannelName = "CH3", Wavelength = "1550" },
                new PowerMeterChannelModel { ChannelName = "CH4", Wavelength = "1550" }
            };

            UnitList = new List<string> { "dBm", "mW", "uW" };



            // 初始化命令
            AllZeroCmd = new DelegateCommand(ExecuteAllZeroCmd);
            SingleSampleCmd = new DelegateCommand(ExecuteSingleSampleCmd);

            _containerProvider = containerProvider;

            _eventAggregator = eventAggregator;
          

            _ld9208Controller = _containerProvider.Resolve<LD9208Controller>("OpticalPowerMeterA");

            _dataBinding = dataBinding;
            _logger= logger;

            _eventAggregator.GetEvent<AppStartUpEvent>().Subscribe(OnAppStartUpEvent);

            // 模拟实时数据刷新 (实际开发中由硬件监听器触发)
            //  StartSimulation();
        }

        private void OnAppStartUpEvent(object obj)
        {
            try
            {
                var portNum = _dataBinding.Get("仪表端口号", "光功率计_A").Value;

                if (!_ld9208Controller.Open(portNum))
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish("光功率计：打开串口失败。");

                    return;
                }
                var strin = _ld9208Controller.GetWavelength(1);

                Channels[0].Wavelength=  _ld9208Controller.GetWavelength(1);
                Channels[1].Wavelength = _ld9208Controller.GetWavelength(2);
                Channels[2].Wavelength = _ld9208Controller.GetWavelength(3);
                Channels[3].Wavelength = _ld9208Controller.GetWavelength(4);





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

                _eventAggregator.GetEvent<Event_Message>().Publish($"光功率计:启动实时采集...");

              
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (_ld9208Controller.IsOpen)
                        {
                            // 调用之前优化过的元组方法
                            var data =  _ld9208Controller.GetAllPower().Split(',');

                            for(int i = 0; i < data.Length; i++)
                            {
                                Channels[i].RawPowerValue =Convert.ToDouble( data[i]);
                            }

                            // 回到 UI 线程更新属性 (Prism 的 SetProperty 会自动处理，但如果是复杂逻辑建议加 Dispatcher)
                            //MeasuredVoltage = data.Voltage;
                            //MeasuredCurrent = data.Current;
                        }
                    }
                    catch (Exception ex)
                    {
                        // 记录错误，防止循环因为单个读取失败而崩溃
                        System.Diagnostics.Debug.WriteLine("光功率计 Error: " + ex.Message);
                    }

                    // 设置读取间隔，例如 500ms 读取一次
                    await Task.Delay(500, token);
                }
             
            }, token);
        }



    }
}
