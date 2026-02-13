using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.DataBinding.Services;
using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Shared.Events;
using DryIoc;
using Newtonsoft.Json.Linq;
using ONet.FAU.Rx._16_128.Extension.Common;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ONet.FAU.Rx._16_128.Extension.ViewModels
{
    public class MaynuoM8811ViewModel : BindableBase, IDestructible
    {
        // 内部字段
        private double _measuredVoltage;
        private double _measuredCurrent;
        private double _setVoltage = 0.0;
        private double _setCurrentLimit = 0.01;
        private string _currentModeText = "恒压模式 (CV)";
        private bool _isOutputOn;
        private Brush _displayColor = Brushes.Gray;

        private IEventAggregator _eventAggregator = null;
        private MaynuoM8811Helper _maynuoM8811 = null;
        private IDataBindingContext _dataBinding;
        private ILogger _logger;

        private CancellationTokenSource _loopCts;
        public MaynuoM8811ViewModel(IEventAggregator eventAggregator,MaynuoM8811Helper maynuoM8811, IDataBindingContext dataBinding,ILogger logger)
        {
            // 初始化命令
            SwitchToCVCmd = new DelegateCommand(ExecuteSwitchToCV);
            SwitchToCCCmd = new DelegateCommand(ExecuteSwitchToCC);
            OutputOnCmd = new DelegateCommand(ExecuteOutputOn, () => !IsOutputOn).ObservesProperty(() => IsOutputOn);
            OutputOffCmd = new DelegateCommand(ExecuteOutputOff, () => IsOutputOn).ObservesProperty(() => IsOutputOn);


            _eventAggregator= eventAggregator;
            _maynuoM8811= maynuoM8811;
            _dataBinding= dataBinding;
            _logger = logger;



            _eventAggregator.GetEvent<AppStartUpEvent>().Subscribe(OnAppStartUpEvent);

            // 模拟实时数据刷新 (实际开发中由硬件监听器触发)
          //  StartSimulation();
        }

        private void OnAppStartUpEvent(object obj)
        {
            try
            {
                var portNum = _dataBinding.Get("仪表端口号", "M8811").Value;

                if (!_maynuoM8811.Open(portNum)) 
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish("M8811源表：打开串口失败。");

                    return; 
                }


               // StartPolling();

            }
            catch (Exception ex)
            {

                _eventAggregator.GetEvent<Event_Message>().Publish($"{ex.Message}");
            }
        

        }

        #region 绑定属性

        public double MeasuredVoltage
        {
            get => _measuredVoltage;
            set => SetProperty(ref _measuredVoltage, value);
        }

        public double MeasuredCurrent
        {
            get => _measuredCurrent;
            set => SetProperty(ref _measuredCurrent, value);
        }

        public double SetVoltage
        {
            get => _setVoltage;
            set => SetProperty(ref _setVoltage, value);
        }

        public double SetCurrentLimit
        {
            get => _setCurrentLimit;
            set => SetProperty(ref _setCurrentLimit, value);
        }

        public string CurrentModeText
        {
            get => _currentModeText;
            set => SetProperty(ref _currentModeText, value);
        }

        public bool IsOutputOn
        {
            get => _isOutputOn;
            set
            {
                if (SetProperty(ref _isOutputOn, value))
                {
                    RaisePropertyChanged(nameof(IsOutputOff));
                    DisplayColor = value ? Brushes.LightBlue : Brushes.Black;
                }
            }
        }

        public bool IsOutputOff => !IsOutputOn;

        public Brush DisplayColor
        {
            get => _displayColor;
            set => SetProperty(ref _displayColor, value);
        }

        #endregion

        #region 命令 (Commands)

        public DelegateCommand SwitchToCVCmd { get; }
        public DelegateCommand SwitchToCCCmd { get; }
        public DelegateCommand OutputOnCmd { get; }
        public DelegateCommand OutputOffCmd { get; }

        private void ExecuteSwitchToCV()
        {
            //try
            //{
            //    CurrentModeText = "恒压模式 (CV)";
            //    _maynuoM8811.set
            //}
            //catch (Exception ex)
            //{

            //    throw;
            //}
          
        }
        private void ExecuteSwitchToCC() => CurrentModeText = "恒流模式 (CC)";

        private async void ExecuteOutputOn()
        {
            try
            {
                // 这里调用硬件通讯接口：SMUService.SetOutput(true)
                IsOutputOn = true;

                await _maynuoM8811.SetOutputStateAsync(true);

                await Task.Delay(500);

                StartPolling();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<Event_Message>().Publish($"M8811源表:{ex.Message}");

            }
         

        }

        private async void ExecuteOutputOff()
        {
            //try
            //{
            //    // 这里调用硬件通讯接口：SMUService.SetOutput(false)
            //    IsOutputOn = false;
            //    MeasuredVoltage = 0;
            //    MeasuredCurrent = 0;

            //    _loopCts.Cancel();
            //    await Task.Delay(500);

            //    await _maynuoM8811.SetOutputStateAsync(false);

            //    _eventAggregator.GetEvent<Event_Message>().Publish($"M8811源表:关闭输出");
            //}
            //catch (Exception ex)
            //{

            //    _eventAggregator.GetEvent<Event_Message>().Publish($"M8811源表:{ex.Message}");
            //}

            try
            {
                IsOutputOn = false;
                MeasuredVoltage = 0;
                MeasuredCurrent = 0;

                _loopCts?.Cancel();

                // 等待循环退出（最多等 2s）
                await Task.Delay(2000);

                await _maynuoM8811.SetOutputStateAsync(false);
                _eventAggregator.GetEvent<Event_Message>().Publish("M8811源表:关闭输出");
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<Event_Message>().Publish($"M8811源表:{ex.Message}");
            }

        }

        private void StartPolling()
        {

            try
            {
                // 确保不会重复启动
                _loopCts?.Cancel();
                _loopCts = new CancellationTokenSource();

                //var token = _loopCts.Token;

                // 启动后台长时间运行的任务
                Task.Run(async () =>
                {

                    _eventAggregator.GetEvent<Event_Message>().Publish($"M8811源表:启动实时采集...");

                    IsOutputOn = true;
                    //while (!_loopCts.IsCancellationRequested)
                    //{
                    //    try
                    //    {
                    //        if (_maynuoM8811.IsOpen)
                    //        {
                    //            // 调用之前优化过的元组方法
                    //            var data = await _maynuoM8811.GetMeasureDataAsync();

                    //            // 回到 UI 线程更新属性 (Prism 的 SetProperty 会自动处理，但如果是复杂逻辑建议加 Dispatcher)
                    //            MeasuredVoltage = data.Voltage;
                    //            MeasuredCurrent = data.Current;
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {

                    //        _logger.Error($"M8811采集错误:{ex.ToString()}");
                    //    }

                    //    // 设置读取间隔，例如 500ms 读取一次
                    //    await Task.Delay(500, _loopCts.Token);
                    //}


                    while (!_loopCts.IsCancellationRequested)
                    {
                        try
                        {
                            if (_maynuoM8811.IsOpen)
                            {
                                var data = await _maynuoM8811.GetMeasureDataAsync();

                                // 用 Dispatcher 更新 UI
                                if (Application.Current?.Dispatcher != null)
                                {
                                    await Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        MeasuredVoltage = data.Voltage;
                                        MeasuredCurrent = data.Current;
                                    });
                                }
                                else
                                {
                                    // fallback，如果无 Application.Current
                                    MeasuredVoltage = data.Voltage;
                                    MeasuredCurrent = data.Current;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"M8811 采集异常: {ex.Message}");
                            await Task.Delay(1000, _loopCts.Token); // 出错时慢点重试
                        }

                        await Task.Delay(500, _loopCts.Token);
                    }



                    IsOutputOn = false;

                    _eventAggregator.GetEvent<Event_Message>().Publish($"M8811源表:停止采集");
                }, _loopCts.Token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                _eventAggregator.GetEvent<Event_Message>().Publish($"{ex.Message}");    
            }

         
        }

        private void StopPolling()
        {
            //if (_loopCts != null)
            //{
            //    _loopCts.Cancel();
            //    _loopCts.Dispose();
            //    _loopCts = null;

            //    _eventAggregator.GetEvent<Event_Message>().Publish($"M8811源表:停止实时采集！");
            //}

            _loopCts?.Cancel();
            _eventAggregator.GetEvent<Event_Message>().Publish("M8811源表:停止实时采集！");
        }



        public void Destroy()
        {
            StopPolling();
            _maynuoM8811?.Dispose();
        }

        #endregion

    }
}
