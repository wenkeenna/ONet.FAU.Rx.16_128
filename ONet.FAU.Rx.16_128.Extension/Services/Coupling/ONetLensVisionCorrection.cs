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
    public class ONetLensVisionCorrection : BindableBase, IToolBase, IToolMigratable
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

        #region 绑定参数

        private string str_pic_a_x;
        private string str_pic_a_y;
        private string str_pic_a_angle;

        private string str_pic_b_x;
        private string str_pic_b_y;
        private string str_pic_b_angle;


        public string Str_PIC_A_X
        {
            get { return str_pic_a_x; }
            set
            {
                str_pic_a_x = value;
                RaisePropertyChanged();
            }
        }
        public string Str_PIC_A_Y
        {
            get { return str_pic_a_y; }
            set
            {
                str_pic_a_y = value;
                RaisePropertyChanged();
            }
        }
        public string Str_PIC_A_Angle
        {
            get { return str_pic_a_angle; }
            set
            {
                str_pic_a_angle = value;
                RaisePropertyChanged();
            }
        }
        public string Str_PIC_B_X
        {
            get { return str_pic_b_x; }
            set
            {
                str_pic_b_x = value;
                RaisePropertyChanged();
            }
        }
        public string Str_PIC_B_Y
        {
            get { return str_pic_b_y; }
            set
            {
                str_pic_b_y = value;
                RaisePropertyChanged();
            }
        }
        public string Str_PIC_B_Angle
        {
            get { return str_pic_b_angle; }
            set
            {
                str_pic_b_angle = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region 补偿绑定参数

        private string offset_pic_a_x;
        private string offset_pic_a_y;
        private string offset_pic_b_x;
        private string offset_pic_b_y;

        public string OffSet_PIC_A_X
        {
            get { return offset_pic_a_x; }
            set
            {
                offset_pic_a_x = value;
                RaisePropertyChanged();
            }
        }

        public string OffSet_PIC_A_Y
        {
            get { return offset_pic_a_y; }
            set
            {
                offset_pic_a_y = value;
                RaisePropertyChanged();
            }
        }
       
        public string OffSet_PIC_B_X
        {
            get { return offset_pic_b_x; }
            set
            {
                offset_pic_b_x = value;
                RaisePropertyChanged();
            }
        }
       
        public string OffSet_PIC_B_Y
        {
            get { return offset_pic_b_y; }
            set
            {
                offset_pic_b_y = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        public ONetLensVisionCorrection()
        {
            Parameter = new ToolParameter()
            {
                ToolGroupName = "ONetTool",
                ToolName = "ONetFA视觉矫正",
                ViewName = "ONetLensVisionCorrectionView",
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

                //获取反射镜角度补偿
                //double LensAngle_L = databinding.Get("反射镜角度补偿", "LeftAngle").ToDouble();
                //double LensAngle_R = databinding.Get("反射镜角度补偿", "RightAngle").ToDouble();

                //获取FA拍照位置
                var BindingPara = LeftPara.X_Str.Split('.');
                double L_FA_GX = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                BindingPara = LeftPara.Y_Str.Split('.');
                double L_FA_GY = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                BindingPara = RightPara.X_Str.Split('.');
                double R_FA_GX = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                BindingPara = RightPara.Y_Str.Split('.');
                double R_FA_GY = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                //获取PIC-A定位位置
                BindingPara = Str_PIC_A_X.Split('.');
                double B_PICA_X = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                BindingPara = Str_PIC_A_Y.Split('.');
                double B_PICA_Y = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                BindingPara = Str_PIC_A_Angle.Split('.');
                double B_PICA_Angle = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();



                //获取PIC-B定位位置
                BindingPara = Str_PIC_B_X.Split('.');
                double B_PICB_X = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                BindingPara = Str_PIC_B_Y.Split('.');
                double B_PICB_Y = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                BindingPara = Str_PIC_B_Angle.Split('.');
                double B_PICB_Angle = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();


                //获取PIC-A补偿数据
                BindingPara = OffSet_PIC_A_X.Split('.');
                double B_OffSet_PICA_X = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                BindingPara = OffSet_PIC_A_Y.Split('.');
                double B_OffSet_PICA_Y = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                //获取PIC-B补偿数据
                BindingPara = OffSet_PIC_B_X.Split('.');
                double B_OffSet_PICB_X = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();

                BindingPara = OffSet_PIC_B_Y.Split('.');
                double B_OffSet_PICB_Y = databinding.Get(BindingPara[0], BindingPara[1]).ToDouble();



                //Speed = 20;

                double AccDec = 0.3;


                //检查龙门轴组是否有机构相互干涉，防止矫正过程中撞机
                /*矫正流程:
                 * 1.检测相机轴、点胶轴位置是否处于安全高度，比如当前位置是否大于1mm(根据实际情况设定)。
                 * 2.检测点胶气缸、UV气缸是否处于收回位置（获取气缸限位开关状态）
                 * 若检测不通过，返回结果False
                 * 
                 * 3.移动龙门XY到参数设定位置
                 * 4.相机轴移动到参数设定高度(芯片焦点高度)
                 * 5.图像检测获取芯片角度
                 * 6.相机轴移动到安全高度
                 * 7.检测左Lesn是否启用矫正
                 * 8.龙门xy移动到左lens矫正位置（参数由工具页编辑）
                 * 9.相机轴下降到左Lens焦点高度
                 * 10.检测左lens角度。
                 * 11.对比芯片与左lens角度，调整左RZ的角度
                 * 12.检测调整后的左lens角度，若与芯片相对角度小于设定值（根据实际情况设定），继续后续执行，否则返回False
                 * 13.相机轴移动到安全高度
                 * 14.检测右Lens是否启用
                 * 15.龙门xy移动到右lens矫正位置（参数由工具页编辑）
                 * 16.相机轴下降到右Lens焦点高度
                 * 17.检测右lens角度。
                 * 18.对比芯片与右lens角度，调整右RZ的角度
                 * 19.检测调整后的右lens角度，若与芯片相对角度小于设定值（根据实际情况设定），继续后续执行，否则返回False
                 * 20.相机轴移动到安全高度
                  */
                //List<Task> tasks = new List<Task>();

                //tasks.Add(Cam.MoveAbsAsync(1,10,token));
                //tasks.Add(Glue.MoveAbsAsync(1,10,token));

                //await Task.WhenAll(tasks.ToArray());
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


                //相机升到安全高度
                await Cam.MoveAbsAsyncAccDec(CamSafePos, Speed, token, AccDec);



                var axistasks = new List<Task>();


                var para = new ParameterModel
                {
                    Name = Parameter.UserDefined,
                    IsRoot = true,
                    IsAddDataBind = true,
                    Children = new ObservableCollection<ParameterModel>()

                };



                if (LeftPara.Enable)
                {

                    axistasks.Add(Gx.MoveAbsAsyncAccDec(L_FA_GX, Speed, token, AccDec));
                    axistasks.Add(Gy.MoveAbsAsyncAccDec(L_FA_GY, Speed, token, AccDec));

                    await Task.WhenAll(axistasks); // 等待GxGy执行完成

                    axistasks.Clear();

                    //设置光源
                    await dc65.SetBrightnessAsync(1, LeftPara.LightValue);
                    //相机下降
                    await Cam.MoveAbsAsyncAccDec(LeftPara.Z, Speed, token, AccDec);
                    await Task.Delay(100);

                    //拍照获取左Lens角度
                    var res = vision.ProcessExecute(LeftPara.VisioName, out float L_FA_X, out float L_FA_Y, out float L_FA_Angle, Parameter.UserDefined);
                    if (!res)
                    {
                        eventAggregator.GetEvent<Event_Message>().Publish("获取左FA角度失败，请检查。");
                        return false;
                    }


                   

                    //计算与芯片角度，补偿RZ
                    var offsetAngle = B_PICA_Angle - L_FA_Angle;

                    eventAggregator.GetEvent<Event_Message>().Publish($"获取左FA角度:{offsetAngle}");
                    //double resAngle = 0;

                    //if (offsetAngle < 0)
                    //{
                    //    resAngle = leftRZ.GetPulsePosition() + Math.Abs(offsetAngle);
                    //}
                    //else
                    //{
                    //    resAngle = leftRZ.GetPulsePosition() + offsetAngle;
                    //}



                    var resAngle = leftRZ.GetPulsePosition() + offsetAngle;

                    eventAggregator.GetEvent<Event_Message>().Publish($"左FA矫正角度:{resAngle.ToString("F4")}，PIC角度:{B_PICA_Angle},识别角度:{L_FA_Angle}");

                    logger.Info($"左FA矫正角度:{resAngle.ToString("F4")}，芯片角度:{B_PICA_Angle},FA角度:{L_FA_Angle},夹角:{offsetAngle}");

                    if (Math.Abs(offsetAngle) > 10)
                    {
                        eventAggregator.GetEvent<Event_Message>().Publish($"左FA角度纠偏过大，请检查。计算纠偏角度：{offsetAngle}");
                        return false;
                    }

                    if (offsetAngle < 0)
                    {
                        await leftRZ.MoveRelAsync(Math.Abs(offsetAngle), true, 2, token);
                    }
                    else
                    {
                        await leftRZ.MoveRelAsync(Math.Abs(offsetAngle), false, 2, token);
                    }

                    res = vision.ProcessExecute(LeftPara.VisioName, out L_FA_X, out L_FA_Y, out L_FA_Angle, Parameter.UserDefined);
                    if (!res)
                    {
                        eventAggregator.GetEvent<Event_Message>().Publish("二次获取左Lens角度失败，请检查。");
                        return false;
                    }

                    eventAggregator.GetEvent<Event_Message>().Publish($"二次获取左FA角度:{L_FA_X.ToString("F4")},{L_FA_Y.ToString("F4")},{L_FA_Angle.ToString("F4")}");


                    var res_X = left_X.GetPulsePosition() + B_PICA_X - L_FA_X + B_OffSet_PICA_X;
                    var res_Y = left_Y.GetPulsePosition() + B_PICA_Y - L_FA_Y + B_OffSet_PICA_Y;

                    para.Children.Add(ParameterModel.Create(Parameter.UserDefined, "PICA_X", res_X.ToString("F4"), ParameterType.Double));
                    para.Children.Add(ParameterModel.Create(Parameter.UserDefined, "PICA_Y", res_Y.ToString("F4"), ParameterType.Double));

                }


                if (RightPara.Enable)
                {

                    axistasks.Add(Gx.MoveAbsAsyncAccDec(R_FA_GX, Speed, token, AccDec));
                    axistasks.Add(Gy.MoveAbsAsyncAccDec(R_FA_GY, Speed, token, AccDec));

                    await Task.WhenAll(axistasks); // 等待GxGy执行完成

                    axistasks.Clear();

                    //设置光源
                    await dc65.SetBrightnessAsync(1, RightPara.LightValue);

                    //相机下降
                    await Cam.MoveAbsAsyncAccDec(RightPara.Z, Speed, token, AccDec);


                    await Task.Delay(100);


                    //拍照获取左Lens角度
                    var res = vision.ProcessExecute(RightPara.VisioName, out float R_FA_X, out float R_FA_Y, out float R_FA_Angle, Parameter.UserDefined);
                    if (!res)
                    {
                        eventAggregator.GetEvent<Event_Message>().Publish("获取右FA角度失败，请检查。");
                        return false;
                    }

                    eventAggregator.GetEvent<Event_Message>().Publish($"右Lens角度:{R_FA_Angle.ToString("F4")}");


                    //计算与芯片角度，补偿RZ
                    var offsetAngle = B_PICB_Angle - R_FA_Angle;
                    double resAngle = 0;

                    if (offsetAngle < 0)
                    {
                        resAngle = rightRZ.GetPulsePosition() + Math.Abs(offsetAngle);
                    }
                    else
                    {
                        resAngle = rightRZ.GetPulsePosition() - offsetAngle;
                    }
                    eventAggregator.GetEvent<Event_Message>().Publish($"获取左FA角度:{offsetAngle}");

                    eventAggregator.GetEvent<Event_Message>().Publish($"右FA矫正角度:{resAngle.ToString("F4")}，PIC角度:{B_PICB_Angle},FA角度:{R_FA_Angle}");
                    logger.Info($"右FA矫正角度:{resAngle.ToString("F4")}，芯片角度:{B_PICB_Angle},FA角度:{R_FA_Angle},夹角:{offsetAngle}");


                    if (Math.Abs(offsetAngle) > 10)
                    {
                        eventAggregator.GetEvent<Event_Message>().Publish($"右Lnes角度纠偏过大，请检查。计算纠偏角度：{offsetAngle}");
                        return false;
                    }

                    if (offsetAngle < 0)
                    {
                        await rightRZ.MoveRelAsync(Math.Abs(offsetAngle), false, 2, token);
                    }
                    else
                    {
                        await rightRZ.MoveRelAsync(offsetAngle, true, 2, token);
                    }


                    res = vision.ProcessExecute(RightPara.VisioName, out R_FA_X, out R_FA_Y, out R_FA_Angle, Parameter.UserDefined);
                    if (!res)
                    {
                        eventAggregator.GetEvent<Event_Message>().Publish("二次获取右FA角度失败，请检查。");
                        return false;
                    }

                    eventAggregator.GetEvent<Event_Message>().Publish($"二次获取右FA角度:{R_FA_X.ToString("F4")},{R_FA_Y.ToString("F4")},{R_FA_Angle.ToString("F4")}");


                    var res_X = right_X.GetPulsePosition() + B_PICB_X - R_FA_X + B_OffSet_PICB_X;
                    var res_Y = right_Y.GetPulsePosition() + B_PICB_Y - R_FA_Y + B_OffSet_PICB_Y;

                    para.Children.Add(ParameterModel.Create(Parameter.UserDefined, "PIC_X", res_X.ToString("F4"), ParameterType.Double));
                    para.Children.Add(ParameterModel.Create(Parameter.UserDefined, "PIC_Y", res_Y.ToString("F4"), ParameterType.Double));

                }

                //相机升到安全高度
                await Cam.MoveAbsAsyncAccDec(CamSafePos, Speed, token, AccDec);


                if (token.IsCancellationRequested)
                {
                    return false;
                }

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
