using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Motion.Interfaces;
using DM.Foundation.Motion.Services;
using DM.Foundation.Shared.Constants;
using DM.Foundation.Shared.Enums;
using DM.Foundation.Shared.Events;
using DM.Foundation.Shared.Interfaces;
using DM.Foundation.Shared.Models;
using DM.InstrumentKit.Services;
using DM.Vision.Interfaces;
using Newtonsoft.Json.Linq;
using ONet.FAU.Rx._16_128.Extension.Model;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Services.Coupling
{
    public class ONetPICPosition : BindableBase, IToolBase, IToolMigratable
    {
        private ToolParameter _parameter;
        public ToolParameter Parameter { get { return _parameter; } set { _parameter = value; RaisePropertyChanged(); } }


        public CorrectionPara LeftPara { get; set; }
        public CorrectionPara RightPara { get; set; }

        private double _speed;
        public double Speed
        {
            get { return _speed; }
            set
            {
                if (value > 20)
                {
                    _speed = 20;
                }
                else
                {
                    _speed = value;
                }


            }
        }

        private double _camsafepos;
        public double CamSafePos
        {
            get { return _camsafepos; }
            set
            {
                _camsafepos = value;
                RaisePropertyChanged();
            }
        }

    
        public ONetPICPosition()
        {
            Parameter = new ToolParameter()
            {
                ToolGroupName = "ONetTool",
                ToolName = "ONetPIC定位",
                ViewName = "ONetPICPositionView",
                CompletionFlag = DMColor.Gray,
                ExecutionFlag = DMColor.Gray
            };

            LeftPara = new CorrectionPara();
            RightPara = new CorrectionPara();

        }

        private readonly string MsgTitle = "";

        public async Task<bool> ExecuteAsync(CancellationToken token, IEventAggregator eventAggregator, ToolExecutionContext context)
        {
            try
            {
                var motionsystem = context.Get<IMotionSystemService>("IMotionSystemService");//电机控制相关服务
                var databinding = context.Get<IDataBindingContext>("DataBindingContext");//数据绑定容器
                var runtiem = context.Get<IRuntimeContext>("IRuntimeContext");//软件运行过程中更新全局数据
                var logger = context.Get<ILogger>("ILogger");
                var dc65 = context.Get<DC65LightSourceHelper>("DC65LightSourceHelper");
                var vision = context.Get<IVisionProcess>("IVisionProcess");

                var leftRZ = motionsystem.GetAxis(MotionAxisNames.LeftRZ);
                var rightRZ = motionsystem.GetAxis(MotionAxisNames.RightRZ);


                var left_X = motionsystem.GetAxis(MotionAxisNames.LeftX);
                var left_Y = motionsystem.GetAxis(MotionAxisNames.LeftY);

                var right_X = motionsystem.GetAxis(MotionAxisNames.RightX);
                var right_Y = motionsystem.GetAxis(MotionAxisNames.RightY);

                var Gx = motionsystem.GetAxis(MotionAxisNames.Gx);
                var Gy = motionsystem.GetAxis(MotionAxisNames.Gy);
                var Cam = motionsystem.GetAxis(MotionAxisNames.Cam);
                var Glue = motionsystem.GetAxis(MotionAxisNames.Glue);

                //double safePos_Cam = 1;
                double safePos_Glue = 1;

                var BindingPara = LeftPara.X_Str.Split('.');
                double PICA_GX = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                BindingPara = LeftPara.Y_Str.Split('.');
                double PICA_GY = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                BindingPara = RightPara.X_Str.Split('.');
                double PICB_GX = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                BindingPara = RightPara.Y_Str.Split('.');
                double PICB_GY = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();


                eventAggregator.GetEvent<Event_Message>().Publish($"绑定信息:{LeftPara.X_Str},{LeftPara.Y_Str},{RightPara.X_Str},{RightPara.Y_Str}");

                var tasks = new List<Task>
                {
                    Cam.MoveAbsAsync(CamSafePos, 10, token),
                    Glue.MoveAbsAsync(1, 10, token)
                };

                await Task.WhenAll(tasks);


                //检查UV,点胶气缸状态
                var glueInput = motionsystem.GetInput("点胶上限");
                var uvLigntInput = motionsystem.GetInput("UV上限");

                if (!LS_DMC3000.GetInPut((DM.Foundation.Motion.Models.DigitalInput)glueInput) || !LS_DMC3000.GetInPut((DM.Foundation.Motion.Models.DigitalInput)uvLigntInput))
                {
                    eventAggregator.GetEvent<Event_Message>().Publish("点胶气缸或UV气缸处于伸出状态，请检查。");
                    return false;
                }


                //检查相机、点胶轴位置
                if (Cam.GetPulsePosition() > CamSafePos || Glue.GetPulsePosition() > safePos_Glue)
                {
                    eventAggregator.GetEvent<Event_Message>().Publish("相机轴或点胶轴未处于安全高度，请检查。");
                    return false;
                }

                var axistasks = new List<Task>();

                double AccDec = 0.3;

                //PIC-A

                //移动到芯片位置XY
                axistasks.Add(Gx.MoveAbsAsyncAccDec(PICA_GX, Speed, token, AccDec));
                axistasks.Add(Gy.MoveAbsAsyncAccDec(PICA_GY, Speed, token, AccDec));

                await Task.WhenAll(axistasks); // 等待GxGy执行完成

                axistasks.Clear();

                //相机下降
                await Cam.MoveAbsAsyncAccDec(LeftPara.Z, Speed * 2, token, AccDec);
                //设置光源
                await dc65.SetBrightnessAsync(1, LeftPara.LightValue);


                //拍照获取PICA位置
                var res = vision.ProcessExecute(LeftPara.VisioName, out float outX, out float outY, out float outPICA_Angle, Parameter.UserDefined);
                if (!res)
                {
                    eventAggregator.GetEvent<Event_Message>().Publish("获取PICA位置失败，请检查。");
                    return false;
                }
                eventAggregator.GetEvent<Event_Message>().Publish($"PICA:X:{outX},Y:{outY}");

                var Res_PICA_X = outX;
                var Res_PICA_Y = outY;

                await Task.Delay(100);

                if (token.IsCancellationRequested)
                {
                    return false;
                }


                //PIC-B
                //移动到芯片位置XY
                axistasks.Add(Gx.MoveAbsAsyncAccDec(PICB_GX, Speed, token, AccDec));
                axistasks.Add(Gy.MoveAbsAsyncAccDec(PICB_GY, Speed, token, AccDec));

                await Task.WhenAll(axistasks); // 等待GxGy执行完成

                axistasks.Clear();

                //相机下降
                await Cam.MoveAbsAsyncAccDec(RightPara.Z, Speed * 2, token, AccDec);
                //设置光源
                await dc65.SetBrightnessAsync(1, RightPara.LightValue);

                //拍照获取PIC12位置
                res = vision.ProcessExecute(RightPara.VisioName, out outX, out outY, out float outPICB_Angle, Parameter.UserDefined);
                if (!res)
                {
                    eventAggregator.GetEvent<Event_Message>().Publish("获取PICB位置失败，请检查。");
                    return false;
                }

                eventAggregator.GetEvent<Event_Message>().Publish($"PICB:X:{outX},Y:{outY}");

                var Res_PICB_X = outX;
                var Res_PICB_Y = outY;

                await Task.Delay(100);

                if (token.IsCancellationRequested)
                {
                    return false;
                }

                var para = new ParameterModel
                {
                    Name = Parameter.UserDefined,
                    IsRoot = true,
                    IsAddDataBind = true,
                    Children = new ObservableCollection<ParameterModel>
                    {
                             ParameterModel.Create(Parameter.UserDefined,"PICA_X",Res_PICA_X.ToString("F4"), ParameterType.Double),
                             ParameterModel.Create(Parameter.UserDefined,"PICA_Y",Res_PICA_Y.ToString("F4"), ParameterType.Double),
                             ParameterModel.Create(Parameter.UserDefined,"PICA_Angle", outPICA_Angle.ToString("F4"), ParameterType.Double),

                             ParameterModel.Create(Parameter.UserDefined,"PICB_X", Res_PICB_X.ToString("F4"), ParameterType.Double),
                             ParameterModel.Create(Parameter.UserDefined,"PICB_Y", Res_PICB_Y.ToString("F4"), ParameterType.Double),
                             ParameterModel.Create(Parameter.UserDefined,"PICB_Angle", outPICB_Angle.ToString("F4"), ParameterType.Double),     
                    }
                };

                databinding.SetModel(para);
                return true;
            }
            catch (Exception ex)
            {
                eventAggregator.GetEvent<Event_Message>().Publish(ex.Message);
                return false;
            }
        }

        public JObject Migrate(JObject sourceData, string fromVersion, string toVersion)
        {
            return null;
        }
    }
}
