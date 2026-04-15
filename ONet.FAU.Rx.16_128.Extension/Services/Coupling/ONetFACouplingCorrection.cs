using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Motion.Interfaces;
using DM.Foundation.Shared.Attributes;
using DM.Foundation.Shared.Events;
using DM.Foundation.Shared.Interfaces;
using DM.Foundation.Shared.Models;
using Newtonsoft.Json.Linq;
using ONet.FAU.Rx._16_128.Extension.Common;
using ONet.FAU.Rx._16_128.Extension.Converters;
using ONet.FAU.Rx._16_128.Extension.Model;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Services.Coupling
{
    [ToolVersion("1.0")]
    public class ONetFACouplingCorrection : BindableBase, IToolBase, IToolMigratable
    {
        private ToolParameter _parameter;
        public ToolParameter Parameter { get { return _parameter; } set { _parameter = value; RaisePropertyChanged(); } }


        private Parameter2D _leftpara;
        public Parameter2D LeftPara
        {
            get { return _leftpara; }
            set { _leftpara = value; RaisePropertyChanged(); }
        }


        private Parameter2D _rightpara;
        public Parameter2D RightPara
        {
            get { return _rightpara; }
            set { _rightpara = value; RaisePropertyChanged(); }
        }

        private const string LD9204S_B = "LD9204S_B";
        private const string LD9204S_A = "LD9204S_A";

        private const string STATE_ON = "ON";
        private const string STATE_OFF = "OFF";

        private IContainerProvider _containerProvider;
        private LD9208Controller _ld9208A;
        private LD9208Controller _ld9208B;
        private LD9208Controller _ld9208C;
        private LD9208Controller _ld9208D;

        private OpticalModuleService _opticalModuleService;

        private int _axisvelocity;
        public int AxisVel
        {
            get { return _axisvelocity; }
            set
            {
                if (value > 5)
                {
                    _axisvelocity = 5;
                }
                else
                {
                    _axisvelocity = value;
                }

                RaisePropertyChanged();
            }
        }



        private int _datadealy;
        public int DataDelay { get { return _datadealy; } set { _datadealy = value; RaisePropertyChanged(); } }

        private int _loopcount;
        /// <summary>
        /// 循环次数
        /// </summary>
        public int LoopCountt { get { return _loopcount; } set { _loopcount = value; RaisePropertyChanged(); } }



        public ONetFACouplingCorrection()
        {
            Parameter = new ToolParameter()
            {
                ToolGroupName = "ONetTool",
                ToolName = "ONet2D耦合矫正",
                ViewName = "ONetFACouplingCorrectionView",
                CompletionFlag = DMColor.Gray,
                ExecutionFlag = DMColor.Gray
            };

            LeftPara = new Parameter2D();
            RightPara = new Parameter2D();
        }

        public async Task<bool> ExecuteAsync(CancellationToken token, IEventAggregator eventAggregator, ToolExecutionContext context)
        {
            try
            {
                var motionsystem = context.Get<IMotionSystemService>("IMotionSystemService");//电机控制相关服务
                var databinding = context.Get<IDataBindingContext>("DataBindingContext");//数据绑定容器
                var runtiem = context.Get<IRuntimeContext>("IRuntimeContext");//软件运行过程中更新全局数据
                var logger = context.Get<ILogger>("ILogger");

                _containerProvider = context.ContainerProvider;
                _ld9208A = _containerProvider.Resolve<LD9208Controller>("OpticalPowerMeterA");
                _ld9208B = _containerProvider.Resolve<LD9208Controller>("OpticalPowerMeterB");
                _ld9208C = _containerProvider.Resolve<LD9208Controller>("OpticalPowerMeterC");
                _ld9208D = _containerProvider.Resolve<LD9208Controller>("OpticalPowerMeterD");

                _opticalModuleService = _containerProvider.Resolve<OpticalModuleService>();

                //关闭实时读取
                eventAggregator.GetEvent<InstrmentKitCommandEvent>().Publish($"{LD9204S_A}:{STATE_OFF}");
                eventAggregator.GetEvent<InstrmentKitCommandEvent>().Publish($"{LD9204S_B}:{STATE_OFF}");

                await Task.Delay(1000);

                List<Task<bool>> tasks = new List<Task<bool>>();

                CouplingController controller = new CouplingController();

                //轴速度、数据延时
                LeftPara.XParameter.AxisVel = AxisVel;
                LeftPara.XParameter.DataDelay = DataDelay;

                LeftPara.YParameter.AxisVel = AxisVel;
                LeftPara.YParameter.DataDelay = DataDelay;

                //轴组、循环次数
                LeftPara.Axisgroup =AxisGroup.Left;
                LeftPara.LoopCountt = LoopCountt;

                //轴速度、数据延时
                RightPara.XParameter.AxisVel = AxisVel;
                RightPara.XParameter.DataDelay = DataDelay;

                RightPara.YParameter.AxisVel = AxisVel;
                RightPara.YParameter.DataDelay = DataDelay;

                //轴组、循环次数
                RightPara.Axisgroup = AxisGroup.Right;
                RightPara.LoopCountt = LoopCountt;

                //设置激光器
                if (LeftPara.SelectedGroup == ChannelGroup.None || RightPara.SelectedGroup == ChannelGroup.None) return false;
                int startChA = (int)LeftPara.SelectedGroup;
                int startChB = (int)RightPara.SelectedGroup;

                await _opticalModuleService.SetLaserStateAsync(startChA, startChB);

                //添加左轴异步执行
                if (LeftPara.Enable)
                {
                    var motionX = motionsystem.GetAxis(LeftPara.XParameter.AxisName);
                    var motionY = motionsystem.GetAxis(LeftPara.YParameter.AxisName);

                    tasks.Add(controller.RunFACouplingCorrectionAsync(motionsystem,motionX, motionY, LeftPara, token, eventAggregator, Parameter, logger, _ld9208A, _ld9208B, 0,"左FA"));
                }

                //添加右轴异步执行
                if (RightPara.Enable)
                {
                    var motionX = motionsystem.GetAxis(RightPara.XParameter.AxisName);
                    var motionY = motionsystem.GetAxis(RightPara.YParameter.AxisName);

                    logger.Info($"Axis_X:{RightPara.XParameter.AxisName},Axis_Y:{RightPara.YParameter.AxisName}");

                    tasks.Add(controller.RunFACouplingCorrectionAsync(motionsystem,motionX, motionY, RightPara, token, eventAggregator, Parameter, logger, _ld9208C, _ld9208D, 1,"右FA"));
                }
                await Task.Delay(500);

                //等待异步执行完成
                bool[] allTasks = await Task.WhenAll(tasks.ToArray());

                //开启实时读取
                eventAggregator.GetEvent<InstrmentKitCommandEvent>().Publish($"{LD9204S_A}:{STATE_ON}");
                eventAggregator.GetEvent<InstrmentKitCommandEvent>().Publish($"{LD9204S_B}:{STATE_ON}");

                //检查执行结果
                foreach (var task in allTasks)
                {
                    if (!task)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                eventAggregator.GetEvent<Event_Message>().Publish(ex.ToString());
                eventAggregator.GetEvent<InstrmentKitCommandEvent>().Publish($"{LD9204S_A}:{STATE_ON}");
                eventAggregator.GetEvent<InstrmentKitCommandEvent>().Publish($"{LD9204S_B}:{STATE_ON}");

                return false;
            }
        }

        public JObject Migrate(JObject sourceData, string fromVersion, string toVersion)
        {
            return null;
        }





    }
}
