using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Motion.Interfaces;
using DM.Foundation.Shared.Attributes;
using DM.Foundation.Shared.Constants;
using DM.Foundation.Shared.Enums;
using DM.Foundation.Shared.Events;
using DM.Foundation.Shared.Interfaces;
using DM.Foundation.Shared.Models;
using DM.Vision.Services;
using Newtonsoft.Json.Linq;
using ONet.FAU.Rx._16_128.Extension.Common;
using ONet.FAU.Rx._16_128.Extension.Converters;
using ONet.FAU.Rx._16_128.Extension.Model;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Services.Coupling
{
    [ToolVersion("1.0")]
    public class CompositeTool : BindableBase, IToolBase, IToolMigratable
    {
        private ToolParameter _parameter;
        public ToolParameter Parameter { get { return _parameter; } set { _parameter = value; RaisePropertyChanged(); } }

        private string _taskname;
        public string TaskName
        {
            get { return _taskname; }
            set { _taskname = value; RaisePropertyChanged(); }
        }


        private string _taskpara;
        public string TaskPara
        {
            get { return _taskpara; }
            set { _taskpara = value; RaisePropertyChanged(); }
        }


        public CompositeTool()
        {
            Parameter = new ToolParameter()
            {
                ToolGroupName = "ONetTool",
                ToolName = "CompositeTool",
                ViewName = "CompositeToolView",
                CompletionFlag = DMColor.Gray,
                ExecutionFlag = DMColor.Gray
            };

        }


        public async Task<bool> ExecuteAsync(CancellationToken token, IEventAggregator eventAggregator, ToolExecutionContext context)
        {
            try
            {
                var motionsystem = context.Get<IMotionSystemService>("IMotionSystemService");//电机控制相关服务
                var databinding = context.Get<IDataBindingContext>("DataBindingContext");//数据绑定容器
                var runtiem = context.Get<IRuntimeContext>("IRuntimeContext");//软件运行过程中更新全局数据
                var logger = context.Get<ILogger>("ILogger");
                var calibration = context.Get<CalibrationServices>("CalibrationServices");


                eventAggregator.GetEvent<Event_Message>().Publish($"{Parameter.UserDefined}:Sub:{TaskName}。");

                bool Result = false;


                switch (TaskName)
                {
                    case "点胶位置计算":

                        Result= await CalculationOfDispensingPositionAnsync(motionsystem, databinding,logger, calibration,TaskPara);

                        break;

                    default:
                        eventAggregator.GetEvent<Event_Message>().Publish($"{Parameter.UserDefined}:执行参数异常，请检查。");
                        Result = false;
                        break;
                }

                return Result;
            }
            catch (Exception ex)
            {
                eventAggregator.GetEvent<Event_Message>().Publish(ex.ToString());
                return false;
            }
        }

        public JObject Migrate(JObject sourceData, string fromVersion, string toVersion)
        {
            return null;
        }


        /// <summary>
        /// 点胶位置计算
        /// </summary>
        private async Task<bool> CalculationOfDispensingPositionAnsync(IMotionSystemService motionSystem, IDataBindingContext dataBinding,ILogger logger, CalibrationServices  calibration,string taskPara)
        {
            try
            {
                //var axis_Lx = motionSystem.GetAxis(MotionAxisNames.LeftX);
                //var axis_Ly = motionSystem.GetAxis(MotionAxisNames.LeftY);

                //var axis_Rx = motionSystem.GetAxis(MotionAxisNames.RightX);
                //var axis_Ry = motionSystem.GetAxis(MotionAxisNames.RightY);

                var LX_offset = dataBinding.Get("点胶补偿", "LeftX").ToDouble();
                var LY_offset = dataBinding.Get("点胶补偿", "LeftY").ToDouble();
                var RX_offset = dataBinding.Get("点胶补偿", "RightX").ToDouble();
                var RY_offset = dataBinding.Get("点胶补偿", "RightY").ToDouble();


                var BaseX = dataBinding.Get("点胶校准", "BaseX").ToDouble();
                var BaseY = dataBinding.Get("点胶校准", "BaseY").ToDouble();
                var BaseZ = dataBinding.Get("点胶校准", "BaseZ").ToDouble();

                var NewX = dataBinding.Get("点胶校准", "NewX").ToDouble();
                var NewY = dataBinding.Get("点胶校准", "NewY").ToDouble();
                var NewZ = dataBinding.Get("点胶校准", "NewZ").ToDouble();

                double GxCalib = NewX - BaseX;
                double GyCalib = NewY - BaseY;
                double GlueZCalib = NewZ - BaseZ;

                var LxPos = motionSystem.GetAxis(MotionAxisNames.LeftX).GetPulsePosition();
                var LyPos = motionSystem.GetAxis(MotionAxisNames.LeftY).GetPulsePosition();

                var RxPos = motionSystem.GetAxis(MotionAxisNames.RightX).GetPulsePosition();
                var RyPos = motionSystem.GetAxis(MotionAxisNames.RightY).GetPulsePosition();

                switch (taskPara)
                {
                    case "G1:G2":


                        string pathRight1 = "D:\\MyApp\\CalibrationFile\\Glue_Right_G1.xml";
                        calibration.AffineTransformation(pathRight1, "Glue_Right_G1", (float)RxPos, (float)RyPos, out var G1_X, out var G1_Y);

                        var resRx = G1_X + RX_offset + GxCalib;
                        var resRy = G1_Y + RY_offset + GyCalib;


                        string pathLeft2 = "D:\\MyApp\\CalibrationFile\\Glue_Left_G2.xml";
                        calibration.AffineTransformation(pathLeft2, "Glue_Left_G2", (float)LxPos, (float)LyPos, out var G2_X, out var G2_Y);

                        var resLx = G2_X + LX_offset + GxCalib;
                        var resLy = G2_Y + LY_offset + GyCalib;

                   

                        var res = new ParameterModel()
                        {
                            Name = Parameter.UserDefined,
                            IsRoot = true,
                            IsAddDataBind = true,
                            Children = new ObservableCollection<ParameterModel>
                            {
                                 ParameterModel.Create(Parameter.UserDefined,"LeftX",   resLx.ToString("F4"), ParameterType.Double),
                                 ParameterModel.Create(Parameter.UserDefined,"LeftY",   resLy.ToString("F4"), ParameterType.Double),
                                 ParameterModel.Create(Parameter.UserDefined,"RightX",  resRx.ToString("F4"), ParameterType.Double),
                                 ParameterModel.Create(Parameter.UserDefined,"RightY",  resRy.ToString("F4"), ParameterType.Double),
                            }
                        };

                        dataBinding.SetModel(res);

                        break;





                    case "G3:G4":

                        string pathRight4 = "D:\\MyApp\\CalibrationFile\\Glue_Right_G3.xml";
                        calibration.AffineTransformation(pathRight4, "Glue_Right_G3", (float)RxPos, (float)RyPos, out var G3_X, out var G3_Y);

                        var resRx_G3 = G3_X + RX_offset + GxCalib;
                        var resRy_G3 = G3_Y + RY_offset + GyCalib;



                        string pathLeft3 = "D:\\MyApp\\CalibrationFile\\Glue_Left_G4.xml";
                        calibration.AffineTransformation(pathLeft3, "Glue_Left_G4", (float)LxPos, (float)LyPos, out var G4_X, out var G4_Y);

                        var resLx_G4 = G4_X + LX_offset + GxCalib;
                        var resLy_G4 = G4_Y + LY_offset + GyCalib;

                     

                        var res34 = new ParameterModel()
                        {
                            Name = Parameter.UserDefined,
                            IsRoot = true,
                            IsAddDataBind = true,
                            Children = new ObservableCollection<ParameterModel>
                            {
                                 ParameterModel.Create(Parameter.UserDefined,"LeftX",   resLx_G4.ToString("F4"), ParameterType.Double),
                                 ParameterModel.Create(Parameter.UserDefined,"LeftY",   resLy_G4.ToString("F4"), ParameterType.Double),
                                 ParameterModel.Create(Parameter.UserDefined,"RightX",  resRx_G3.ToString("F4"), ParameterType.Double),
                                 ParameterModel.Create(Parameter.UserDefined,"RightY",  resRy_G3.ToString("F4"), ParameterType.Double),
                            }
                        };

                        dataBinding.SetModel(res34);



                        break;

                }

                await Task.Delay(10);

                return true;
            }
            catch (Exception ex)
            {
                logger?.Error(ex.ToString());
                return false;
            }
          
        }









    }
}
