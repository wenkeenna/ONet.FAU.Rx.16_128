using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Motion.Interfaces;
using DM.Foundation.Motion.Models;
using DM.Foundation.Motion.Services;
using DM.Foundation.Shared.Constants;
using DM.Foundation.Shared.Enums;
using DM.Foundation.Shared.Models;
using DM.ManualMotionControl.Models;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Initialization
{
    public static class MotionSystemInitializer
    {
        /// <summary>
        /// 创建轴实例
        /// </summary>
        /// <returns></returns>
        public static IMotionSystemService Initialize(IEventAggregator eventAggregator, ILogger logger)
        {
            var system = new MotionSystemService(eventAggregator, logger);

            #region 龙门轴实例

            var axisPara_GX = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 0,
                AxisName = MotionAxisNames.Gx,
                MicroStep = 50,
                LeadScrew = 2,
                ReductionRatio = 1,
                ModuleType = ModuleType.LinearMotion,
                MotorType = MotorType.Stepper,
                StepAngle = 0.72,
                MaxSpeed = 20
            };
            system.AddAxis(new LS_DMC3000(axisPara_GX));

            var axisPara_GY = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 1,
                AxisName = MotionAxisNames.Gy,
                MicroStep = 10000,
                LeadScrew = 10,
                ReductionRatio = 1,
                ModuleType = ModuleType.LinearMotion,
                MotorType = MotorType.Servo,
                StepAngle = 0.72,
                MaxSpeed = 50
            };
            system.AddAxis(new LS_DMC3000(axisPara_GY));

            var axisPara_Cam = new AxisPara
            {
                CardIndex = 1,
                AxisIndex = 2,
                AxisName = MotionAxisNames.Cam,
                MicroStep = 50,
                LeadScrew = 1,
                ReductionRatio = 1,
                ModuleType = ModuleType.LinearMotion,
                MotorType = MotorType.Stepper,
                StepAngle = 0.72,
                MaxSpeed = 16
            };
            system.AddAxis(new LS_DMC3000(axisPara_Cam));

            //var axisPara_Nozzle = new AxisPara
            //{
            //    CardIndex = 0,
            //    AxisIndex = 0,
            //    AxisName = MotionAxisNames.Nozzle,
            //    MicroStep = 20,
            //    LeadScrew = 1,
            //    ReductionRatio = 1,
            //    ModuleType = ModuleType.LinearMotion,
            //    MotorType = MotorType.Stepper,
            //    StepAngle = 0.72,
            //    MaxSpeed = 18
            //};
            //system.AddAxis(new LS_DMC3000(axisPara_Nozzle));

            var axisPara_Glue = new AxisPara
            {
                CardIndex = 1,
                AxisIndex = 3,
                AxisName = MotionAxisNames.Glue,
                MicroStep = 50,
                LeadScrew = 2,
                ReductionRatio = 1,
                ModuleType = ModuleType.LinearMotion,
                MotorType = MotorType.Stepper,
                StepAngle = 0.72,
                MaxSpeed = 16
            };
            system.AddAxis(new LS_DMC3000(axisPara_Glue));
            #endregion

            #region 左轴实例


            var axisPara_LX = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 2,
                AxisName = MotionAxisNames.LeftX,
                MicroStep = 600000,
                LeadScrew = 60,
                ReductionRatio = 1,
                ModuleType = ModuleType.LinearMotion,
                MotorType = MotorType.Servo,
                StepAngle = 0.72,
                MaxSpeed = 50
            };
            system.AddAxis(new LS_DMC3000(axisPara_LX));

            var axisPara_LY = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 3,
                AxisName = MotionAxisNames.LeftY,
                MicroStep = 600000,
                LeadScrew = 30,
                ReductionRatio = 1,
                ModuleType = ModuleType.LinearMotion,
                MotorType = MotorType.Servo,
                StepAngle = 0.72,
                MaxSpeed = 50
            };
            system.AddAxis(new LS_DMC3000(axisPara_LY));

            var axisPara_LZ = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 4,
                AxisName = MotionAxisNames.LeftZ,
                MicroStep = 300000,
                LeadScrew = 30,
                ReductionRatio = 1,
                ModuleType = ModuleType.LinearMotion,
                MotorType = MotorType.Servo,
                StepAngle = 0.72,
                MaxSpeed = 50
            };
            system.AddAxis(new LS_DMC3000(axisPara_LZ));

            var axisPara_L_RX = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 8,
                AxisName = MotionAxisNames.LeftRX,
                MicroStep = 50,
                LeadScrew = 1,
                ReductionRatio = 160,
                ModuleType = ModuleType.RotaryMotion,
                MotorType = MotorType.Stepper,
                StepAngle = 0.72,
                MaxSpeed = 15
            };
            system.AddAxis(new LS_DMC3000(axisPara_L_RX));

            var axisPara_L_RY = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 9,
                AxisName = MotionAxisNames.LeftRY,
                MicroStep = 50,
                LeadScrew = 1,
                ReductionRatio = 360,
                ModuleType = ModuleType.RotaryMotion,
                MotorType = MotorType.Stepper,
                StepAngle = 0.72,
                MaxSpeed = 15
            };
            system.AddAxis(new LS_DMC3000(axisPara_L_RY));

            var axisPara_L_RZ = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 10,
                AxisName = MotionAxisNames.LeftRZ,
                MicroStep = 50,
                LeadScrew = 1,
                ReductionRatio = 225,
                ModuleType = ModuleType.RotaryMotion,
                MotorType = MotorType.Stepper,
                StepAngle = 0.72,
                MaxSpeed = 15
            };
            system.AddAxis(new LS_DMC3000(axisPara_L_RZ));

            var axisPara_Default = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 0,
                AxisName = MotionAxisNames.Default,
                MicroStep = 20,
                LeadScrew = 1,
                ReductionRatio = 1,
                ModuleType = ModuleType.LinearMotion,
                MotorType = MotorType.Stepper,
                StepAngle = 0.72,
                MaxSpeed = 15
            };
            system.AddAxis(new LS_DMC3000(axisPara_Default));
            #endregion

            #region 右轴组实例

            var axisPara_RX = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 5,
                AxisName = MotionAxisNames.RightX,
                MicroStep = 600000,
                LeadScrew = 60,
                ReductionRatio = 1,
                ModuleType = ModuleType.LinearMotion,
                MotorType = MotorType.Servo,
                StepAngle = 0.72,
                MaxSpeed = 50
            };
            system.AddAxis(new LS_DMC3000(axisPara_RX));

            var axisPara_RY = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 6,
                AxisName = MotionAxisNames.RightY,
                MicroStep = 600000,
                LeadScrew = 30,
                ReductionRatio = 1,
                ModuleType = ModuleType.LinearMotion,
                MotorType = MotorType.Servo,
                StepAngle = 0.72,
                MaxSpeed = 50
            };
            system.AddAxis(new LS_DMC3000(axisPara_RY));

            var axisPara_RZ = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 7,
                AxisName = MotionAxisNames.RightZ,
                MicroStep = 300000,
                LeadScrew = 30,
                ReductionRatio = 1,
                ModuleType = ModuleType.LinearMotion,
                MotorType = MotorType.Servo,
                StepAngle = 0.72,
                MaxSpeed = 50
            };
            system.AddAxis(new LS_DMC3000(axisPara_RZ));


            var axisPara_R_RX = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 11,
                AxisName = MotionAxisNames.RightRX,
                MicroStep = 50,
                LeadScrew = 1,
                ReductionRatio = 160,
                ModuleType = ModuleType.RotaryMotion,
                MotorType = MotorType.Stepper,
                StepAngle = 0.72,
                MaxSpeed = 15
            };
            system.AddAxis(new LS_DMC3000(axisPara_R_RX));

            var axisPara_R_RY = new AxisPara
            {
                CardIndex = 1,
                AxisIndex = 0,
                AxisName = MotionAxisNames.RightRY,
                MicroStep = 50,
                LeadScrew = 1,
                ReductionRatio = 360,
                ModuleType = ModuleType.RotaryMotion,
                MotorType = MotorType.Stepper,
                StepAngle = 0.72,
                MaxSpeed = 15
            };
            system.AddAxis(new LS_DMC3000(axisPara_R_RY));

            var axisPara_R_RZ = new AxisPara
            {
                CardIndex = 1,
                AxisIndex = 1,
                AxisName = MotionAxisNames.RightRZ,
                MicroStep = 50,
                LeadScrew = 1,
                ReductionRatio = 225,
                ModuleType = ModuleType.RotaryMotion,
                MotorType = MotorType.Stepper,
                StepAngle = 0.72,
                MaxSpeed = 15
            };
            system.AddAxis(new LS_DMC3000(axisPara_R_RZ));

            var axisPara_Default_R = new AxisPara
            {
                CardIndex = 0,
                AxisIndex = 11,
                AxisName = MotionAxisNames.Default,
                MicroStep = 20,
                LeadScrew = 1,
                ReductionRatio = 1,
                ModuleType = ModuleType.LinearMotion,
                MotorType = MotorType.Stepper,
                StepAngle = 0.72,
                MaxSpeed = 15
            };
            system.AddAxis(new LS_DMC3000(axisPara_Default_R));
            #endregion




            #region 输入点信息
            var input = new DigitalInput
            {
                CardIndex = 1,
                Name = "点胶上限",
                Address = 0,
                Description = "检测点胶气缸上限"
            };
            system.AddInput(input);


            input = new DigitalInput
            {
                CardIndex = 1,
                Name = "点胶下限",
                Address = 1,
                Description = "检测点胶气缸下限"
            };
            system.AddInput(input);

            input = new DigitalInput
            {
                CardIndex = 1,
                Name = "UV上限",
                Address = 2,
                Description = "检测UV气缸上限"
            };
            system.AddInput(input);


            input = new DigitalInput
            {
                CardIndex = 1,
                Name = "UV下限",
                Address = 3,
                Description = "检测UV气缸下限"
            };
            system.AddInput(input);

            input = new DigitalInput
            {
                CardIndex = 1,
                Name = "真空吸-1",
                Address = 4,
                Description = "真空吸-1"
            };
            system.AddInput(input);

            input = new DigitalInput
            {
                CardIndex = 1,
                Name = "真空吸-2",
                Address = 5,
                Description = "真空吸-2"
            };
            system.AddInput(input);



            input = new DigitalInput
            {
                CardIndex = 0,
                Name = "急停按钮",
                Address = 15,
                Description = "急停是否按下"
            };
            system.AddInput(input);

            #endregion


            #region 输出点信息

            var output = new DigitalOutput()
            {
                CardIndex = 1,
                Name = "点胶气缸",
                Address = 0,
                Description = "",
            };
            system.AddOutput(output);

            output = new DigitalOutput()
            {
                CardIndex = 1,
                Name = "UV气缸",
                Address = 1,
                Description = "",
            };
            system.AddOutput(output);

            output = new DigitalOutput()
            {
                CardIndex = 1,
                Name = "吹气散热",
                Address = 2,
                Description = "",
            };
            system.AddOutput(output);

            output = new DigitalOutput()
            {
                CardIndex = 1,
                Name = "报警灯/红",
                Address = 4,
                Description = "",
            };
            system.AddOutput(output);


            output = new DigitalOutput()
            {
                CardIndex = 1,
                Name = "报警灯/黄",
                Address = 5,
                Description = "",
            };
            system.AddOutput(output);

            output = new DigitalOutput()
            {
                CardIndex = 1,
                Name = "报警灯/绿",
                Address = 6,
                Description = "",
            };
            system.AddOutput(output);


            output = new DigitalOutput()
            {
                CardIndex = 1,
                Name = "报警灯/蜂鸣器",
                Address = 13,
                Description = "",
            };
            system.AddOutput(output);


            output = new DigitalOutput()
            {
                CardIndex = 1,
                Name = "真空吸-1",
                Address = 9,
                Description = "",
            };
            system.AddOutput(output);

            output = new DigitalOutput()
            {
                CardIndex = 1,
                Name = "真空吸-2",
                Address = 10,
                Description = "",
            };
            system.AddOutput(output);




            output = new DigitalOutput()
            {
                CardIndex = 1,
                Name = "点胶机",
                Address = 15,
                Description = "",
            };
            system.AddOutput(output);

            output = new DigitalOutput()
            {
                CardIndex = 1,
                Name = "左夹爪",
                Address = 2,
                Description = "",
            };
            system.AddOutput(output);


            output = new DigitalOutput()
            {
                CardIndex = 1,
                Name = "右夹爪",
                Address = 3,
                Description = "",
            };
            system.AddOutput(output);

            #endregion


            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.SetOutput, OutputName = "点胶气缸", OutPutState = false, InputName = "点胶上限", InputState = true, Priority = 0, Description = "收回点胶气缸" });
            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.SetOutput, OutputName = "UV气缸", OutPutState = false, InputName = "UV上限", InputState = true, Priority = 0, Description = "收回点胶气缸" });

            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.Cam, Priority = 1, Speed = 10, Description = "相机轴回零" });
            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.Glue, Priority = 1, Speed = 10, Description = "点胶轴回零" });

            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.Gy, Priority = 10, Speed = 25, Description = "Gy轴回零" });
            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.Gx, Priority = 20, Speed = 18, Description = "Gx轴回零" });

            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.AbsMove, AxisName = MotionAxisNames.Gx, Priority = 30, Speed = 18, BindingParamName = "[龙门]复位位置:Gx", Description = "Gx轴复位" });

            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.LeftZ, Priority = 40, Speed = 15, Description = "LZ轴回零" });
            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.RightZ, Priority = 40, Speed = 15, Description = "RZ轴回零" });

            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.LeftX, Priority = 50, Speed = 15, Description = "LX轴回零" });
            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.RightX, Priority = 50, Speed = 15, Description = "RX轴回零" });

            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.LeftY, Priority = 60, Speed = 15, Description = "LY轴回零" });
            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.RightY, Priority = 60, Speed = 15, Description = "RY轴回零" });


            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.LeftRX, Priority = 70, Speed = 8, Description = "L_RX轴回零" });
            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.RightRX, Priority = 70, Speed = 8, Description = "R_RX轴回零" });

            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.LeftRY, Priority = 80, Speed = 8, Description = "L_RY轴回零" });
            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.RightRY, Priority = 80, Speed = 8, Description = "R_RY轴回零" });

            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.LeftRZ, Priority = 90, Speed = 8, Description = "L_RZ轴回零" });
            system.HomingItems.Add(new HomingItem() { StepType = HomeStepType.Home, AxisName = MotionAxisNames.RightRZ, Priority = 90, Speed = 8, Description = "R_RZ轴回零" });




            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.SetOutput,
                Priority = 1,
                OutputName = "点胶气缸",
                OutPutState = false,
                InputName = "点胶上限",
                InputState = true,
                Description = "收回点胶气缸",

            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.SetOutput,
                Priority = 1,
                OutputName = "UV气缸",
                OutPutState = false,
                InputName = "UV上限",
                InputState = true,
                Description = "收回UV气缸",
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.Cam,
                Priority = 10,
                Speed = 10,
                ResetPosition = 1,
                Description = "Cam轴复位",
                BindingParamName = "[龙门]复位位置:Cam"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.Glue,
                Priority = 10,
                Speed = 10,
                ResetPosition = 1,
                Description = "Glue轴复位",
                BindingParamName = "[龙门]复位位置:Glue"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.Gy,
                Priority = 11,
                Speed = 10,
                ResetPosition = 1,
                Description = "Gy轴复位",
                BindingParamName = "[龙门]复位位置:Gy"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.Gx,
                Priority = 12,
                Speed = 10,
                ResetPosition = 100,
                Description = "Gx轴复位",
                BindingParamName = "[龙门]复位位置:Gx"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.LeftZ,
                Priority = 20,
                Speed = 10,
                ResetPosition = 5,
                Description = "LZ轴复位",
                BindingParamName = "[左轴组]复位位置:Z"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.RightZ,
                Priority = 20,
                Speed = 10,
                ResetPosition = 5,
                Description = "RZ轴复位",
                BindingParamName = "[右轴组]复位位置:Z"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.LeftY,
                Priority = 30,
                Speed = 10,
                ResetPosition = 5,
                Description = "LY轴复位",
                BindingParamName = "[左轴组]复位位置:Y"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.RightY,
                Priority = 30,
                Speed = 10,
                ResetPosition = 5,
                Description = "RY轴复位",
                BindingParamName = "[右轴组]复位位置:Y"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.LeftX,
                Priority = 40,
                Speed = 10,
                ResetPosition = 5,
                Description = "LX轴复位",
                BindingParamName = "[左轴组]复位位置:X"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.RightX,
                Priority = 40,
                Speed = 10,
                ResetPosition = 5,
                Description = "RX轴复位",
                BindingParamName = "[右轴组]复位位置:X"
            });

            //角度轴复位配置
            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.LeftRX,
                Priority = 50,
                Speed = 10,
                ResetPosition = 5,
                Description = "LRx轴复位",
                BindingParamName = "[左轴组]复位位置:RX"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.RightRX,
                Priority = 50,
                Speed = 10,
                ResetPosition = 5,
                Description = "RRx轴复位",
                BindingParamName = "[右轴组]复位位置:RX"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.LeftRY,
                Priority = 60,
                Speed = 10,
                ResetPosition = 5,
                Description = "LRy轴复位",
                BindingParamName = "[左轴组]复位位置:RY"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.RightRY,
                Priority = 60,
                Speed = 10,
                ResetPosition = 5,
                Description = "RRy轴复位",
                BindingParamName = "[右轴组]复位位置:RY"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.LeftRZ,
                Priority = 70,
                Speed = 10,
                ResetPosition = 5,
                Description = "LRz轴复位",
                BindingParamName = "[左轴组]复位位置:RZ"
            });

            system.ResetItems.Add(new HomingItem()
            {
                StepType = HomeStepType.AbsMove,
                AxisName = MotionAxisNames.RightRZ,
                Priority = 70,
                Speed = 10,
                ResetPosition = 5,
                Description = "RRz轴复位",
                BindingParamName = "[右轴组]复位位置:RZ"
            });

            return system;
        }

        /// <summary>
        /// 创建手动控制View布局
        /// </summary>
        /// <param name="motionSystem"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="OnActiveCommand"></param>
        /// <param name="OnSyncModeSetCommand"></param>
        /// <returns></returns>
        public static ObservableCollection<AxisGroupViewModel> InitialAxisGroupView(IMotionSystemService motionSystem, IEventAggregator eventAggregator, Action<object> OnActiveCommand, Action<object> OnSyncModeSetCommand)
        {
            var AxisGroups = new ObservableCollection<AxisGroupViewModel>
            {
                new AxisGroupViewModel
                {
                    GroupName = "龙门轴组",
                    IsJogModeTest="点动",
                    ActiveCommand= new DelegateCommand<object>(OnActiveCommand),
                    SyncModeIsHidden=false,
                    SyncModeSetCommand= new DelegateCommand<object>(OnSyncModeSetCommand),
                    Axes = new List<AxisViewModel>
                    {
                        new AxisViewModel(motionSystem,MotionAxisNames.Gx ,eventAggregator,false)
                        {
                            NegativeContent="GX-",
                            PositiveContent="GX+",
                            StepLab="Gx",
                            PositionText="Gx",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                            StepSize=0.1,
                            IsButtonVisible=true
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.Gy,eventAggregator,false)
                        {
                            NegativeContent="GY-",
                            PositiveContent="GY+",
                            StepLab="Gy",
                              PositionText="Gy",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                             StepSize=0.1,
                            IsButtonVisible=true
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.Default,eventAggregator,false)
                        {
                            NegativeContent="default-",
                            PositiveContent="default+",
                            StepLab="default",

                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                            IsButtonVisible=false
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.Cam,eventAggregator,false)
                        {
                            NegativeContent="Cam-",
                            PositiveContent="Cam+",
                            StepLab="Cam",
                               PositionText="Cam",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                             StepSize=0.1,
                            IsButtonVisible=true
                        },

                        //new AxisViewModel (motionSystem,MotionAxisNames.Nozzle,eventAggregator,false)
                        //{
                        //    NegativeContent="Nozzle-",
                        //    PositiveContent="Nozzle+",
                        //    StepLab="Nozzle",
                        //    PositionText="Nozzle",
                        //    Position=0.0000,
                        //    StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                        //    IsButtonVisible=true
                        //},

                            new AxisViewModel (motionSystem,MotionAxisNames.Glue,eventAggregator,false)
                        {
                            NegativeContent="Glue-",
                            PositiveContent="Glue+",
                            StepLab="Glue",
                            PositionText="Glue",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                             StepSize=0.1,
                            IsButtonVisible=true
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.Default,eventAggregator,false)
                        {
                            NegativeContent="default-",
                            PositiveContent="default+",
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                            IsButtonVisible=false
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.Default,eventAggregator,false)
                        {
                            NegativeContent="default-",
                            PositiveContent="default+",
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                            IsButtonVisible=false
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.Default,eventAggregator,false)
                        {
                            NegativeContent="default-",
                            PositiveContent="default+",
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                            IsButtonVisible=false
                        },
                           new AxisViewModel (motionSystem,MotionAxisNames.Default,eventAggregator,false)
                        {
                            NegativeContent="default-",
                            PositiveContent="default+",
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                            IsButtonVisible=false
                        },
                    }
                },
                new AxisGroupViewModel
                {
                    GroupName = "左轴组",
                    IsJogModeTest="点动",
                    ActiveCommand= new DelegateCommand<object>(OnActiveCommand),
                    SyncModeIsHidden=true,
                    SyncModeSetCommand= new DelegateCommand<object>(OnSyncModeSetCommand),
                    Axes = new List<AxisViewModel>
                    {

                        new AxisViewModel(motionSystem,MotionAxisNames.LeftX,eventAggregator,false)
                        {
                            NegativeContent="X-",
                            PositiveContent="X+",
                            StepLab="X",
                            PositionText="X",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                              StepSize=0.1,
                            IsButtonVisible=true
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.LeftY,eventAggregator,false)
                        {
                            NegativeContent="Y-",
                            PositiveContent="Y+",
                            StepLab="Y",
                                  PositionText="Y",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                              StepSize=0.1,
                            IsButtonVisible=true
                        },

                             new AxisViewModel (motionSystem,MotionAxisNames.LeftZ,eventAggregator,false)
                        {

                            NegativeContent="Z-",
                            PositiveContent="Z+",
                            StepLab="Z",
                            PositionText="Z",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                              StepSize=0.1,
                            IsButtonVisible=true
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.Default,eventAggregator, false)
                        {

                            NegativeContent="default-",
                            PositiveContent="default+",
                            StepLab="defaelt",
                            IsButtonVisible=false
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.Default,eventAggregator, false)
                        {

                            NegativeContent="default-",
                            PositiveContent="default+",
                            StepLab="defaelt",
                            IsButtonVisible=false
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.LeftRX,eventAggregator,false)
                        {

                            NegativeContent="Rx-",
                            PositiveContent="Rx+",
                            StepLab="RX",
                                  PositionText="RX",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                              StepSize=0.1,
                            IsButtonVisible=true
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.LeftRY,eventAggregator, false)
                        {

                            NegativeContent="LRy-",
                            PositiveContent="LRy+",
                            StepLab="RY",
                                  PositionText="RY",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                              StepSize=0.1,
                            IsButtonVisible=true
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.LeftRZ,eventAggregator,false)
                        {
                            NegativeContent="Rz-",
                            PositiveContent="Rz+",
                            StepLab      ="RZ",
                            PositionText="RZ",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                              StepSize=0.1,
                            IsButtonVisible=true
                        },

                          new AxisViewModel (motionSystem,MotionAxisNames.Default,eventAggregator, false)
                        {

                            NegativeContent="default-",
                            PositiveContent="default+",
                            StepLab="defaelt",
                            IsButtonVisible=false
                        },


                    }
                },

                  new AxisGroupViewModel
                {
                    GroupName = "右轴组",
                    IsJogModeTest="点动",
                    ActiveCommand= new DelegateCommand<object>(OnActiveCommand),
                    SyncModeIsHidden=false,
                    SyncModeSetCommand= new DelegateCommand<object>(OnSyncModeSetCommand),
                    Axes = new List<AxisViewModel>
                    {

                        new AxisViewModel(motionSystem,MotionAxisNames.RightX,eventAggregator, true)
                        {
                            NegativeContent="X+",
                            PositiveContent="X-",
                            StepLab="X",
                            PositionText="X",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                              StepSize=0.1,
                            IsButtonVisible=true
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.RightY,eventAggregator,false)
                        {
                            NegativeContent="Y-",
                            PositiveContent="Y+",
                            StepLab="Y",
                            PositionText="Y",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                              StepSize=0.1,
                            IsButtonVisible=true
                        },

                          new AxisViewModel (motionSystem,MotionAxisNames.RightZ,eventAggregator,false)
                        {

                            NegativeContent="Z-",
                            PositiveContent="Z+",
                            StepLab="Z",
                                  PositionText="Z",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                              StepSize=0.1,
                            IsButtonVisible=true
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.Default,eventAggregator, false)
                        {

                            NegativeContent="default-",
                            PositiveContent="default+",
                            StepLab="defaelt",
                            IsButtonVisible=false
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.Default,eventAggregator,false)
                        {

                            NegativeContent="default-",
                            PositiveContent="default+",
                            StepLab="defaelt",
                            IsButtonVisible=false
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.RightRX,eventAggregator, false)
                        {

                            NegativeContent="Rx-",
                            PositiveContent="Rx+",
                            StepLab="RX",
                                  PositionText="RX",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                              StepSize=0.1,
                            IsButtonVisible=true
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.RightRY,eventAggregator,false)
                        {

                            NegativeContent="LRy-",
                            PositiveContent="LRy+",
                            StepLab="RY",
                                  PositionText="RY",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                              StepSize=0.1,
                            IsButtonVisible=true
                        },

                        new AxisViewModel (motionSystem,MotionAxisNames.RightRZ,eventAggregator, false)
                        {
                            NegativeContent="Rz-",
                            PositiveContent="Rz+",
                            StepLab      ="RZ",
                            PositionText="RZ",
                            Position=0.0000,
                            StepList=new List<double>(){0.0001,0.0002,0.0005,0.001,0.002,0.005,0.01,0.02,0.05,0.1,0.5,1,2,5,10},
                              StepSize=0.1,
                            IsButtonVisible=true
                        },

                          new AxisViewModel (motionSystem,MotionAxisNames.Default,eventAggregator, false)
                        {

                            NegativeContent="default-",
                            PositiveContent="default+",
                            StepLab="defaelt",
                            IsButtonVisible=false
                        },
                    }
                }
            };

            return AxisGroups;
        }
    }
}
