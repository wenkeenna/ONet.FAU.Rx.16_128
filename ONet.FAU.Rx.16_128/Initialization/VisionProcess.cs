using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Shared.Events;
using DM.Vision.Interfaces;
using DM.Vision.Services;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VM.Core;
using VM.PlatformSDKCS;
using VMControls.Interface;

namespace ONet.FAU.Rx._16_128.Initialization
{
    public class VisionProcess : IVisionProcess
    {
        public VmProcedure procedure;

        public IVmModule VmModule { get; set; }

        IEventAggregator _eventAggregator;



        /// <summary>
        /// 方案加载标志
        /// </summary>
        bool mSolutionIsLoad { get; set; }
        /// <summary>
        /// 日志实例
        /// </summary>
        ILogger _logger;

        IDataBindingContext _dataBindingContext;
        CalibrationServices _calibration;

        public string Solutionpath { get; set; }
        public string SolutionName { get; set; }
        public bool SolutionIsLoad { get; set; }

        public VisionProcess(ILogger logger, IEventAggregator eventAggregator, string path, string name, IDataBindingContext dataBindingContext, CalibrationServices calibration)
        {
            Solutionpath = path;
            SolutionName = name;

            _eventAggregator = eventAggregator;
            _logger = logger;
            _dataBindingContext = dataBindingContext;
            _calibration = calibration;
        }

        public List<string> GetAllProcedureList()
        {
            try
            {
                //    if (!(bool)VmSolution.Instance.IsReady) { return null; }

                ProcessInfoList infoList = VmSolution.Instance.GetAllProcedureList();
                List<string> processName = new List<string>();
                foreach (ProcessInfo info in infoList.astProcessInfo)
                {
                    if (info.strProcessName == null) { break; }
                    processName.Add(info.strProcessName);
                }

                return processName;
            }
            catch (Exception ex)
            {

                _eventAggregator.GetEvent<Event_Message>().Publish($"[Vision]:{ex.Message}");

                return null;
            }
        }

        public void Load()
        {
            try
            {
                var laodpath = Path.Combine(Solutionpath, SolutionName);
                VmSolution.Load(laodpath, "");  // 加载方案      

                _eventAggregator.GetEvent<Event_Message>().Publish($"[Vision]方案加载:{laodpath}");

                mSolutionIsLoad = true;

            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<Event_Message>().Publish($"方案加载报错: {ex.Message}");
                mSolutionIsLoad = false;
            }
        }

        public bool ProcessExecute(string ProcessName, out float X, out float Y)
        {
            X = (float)0.0123;
            Y = (float)0.456;
            return false;
        }

        public bool ProcessExecute(string ProcessName, out float X, out float Y, out float Angle, string UserDefined)
        {

            X = 0;
            Y = 0;
            Angle = 0;

            try
            {

                procedure = (VmProcedure)VmSolution.Instance[ProcessName];

                procedure.ContinuousRunEnable = false;

                VmModule = procedure;

                if (!(bool)VmSolution.Instance.IsReady)
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish("[Vision-实时图像]模块订阅未完成");
                    return false;
                }

                _eventAggregator.GetEvent<VisionExecuteEvent>().Publish(VmModule);

                if (procedure != null)
                {
                    procedure.Run();
                }
                else
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish($"{ProcessName}:实例为Null");
                    _logger.Error($"{ProcessName}:实例为Null");

                    return false;
                }

                IntDataArray result_state = procedure.ModuResult.GetOutputInt("State");




                if (result_state.pIntVal[0] != 1)
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish($"{ProcessName}:执行失败");
                    _logger.Error($"{ProcessName}:执行失败");
                    return false;
                }

                FloatDataArray result_x = procedure.ModuResult.GetOutputFloat("X");
                FloatDataArray result_y = procedure.ModuResult.GetOutputFloat("Y");
                FloatDataArray result_angle = procedure.ModuResult.GetOutputFloat("Angle");
                var pix_X = result_x.pFloatVal[0];
                var pix_Y = result_y.pFloatVal[0];
                Angle = result_angle.pFloatVal[0];

                if (ProcessName == "右Lens标定" || ProcessName == "左Lens标定")
                {
                    X = pix_X;
                    Y = pix_Y;
                }





                if (ProcessName == "左轴耦合位置标定" || ProcessName == "右轴组耦合位置标定")
                {
                    X = (float)pix_X;
                    Y = (float)pix_Y;
                }


                return true;
            }
            catch (Exception ex)
            {

                _eventAggregator.GetEvent<Event_Message>().Publish($"{ProcessName}:异常，{ex.Message}");
                _logger.Error($"{ProcessName}:异常，{ex.Message}");

                return false;
            }

        }

        public bool ProcessExecute(string ProcessName, out float[] X, out float[] Y, out float[] Angle, string UserDefined)
        {

            X = new float[2];
            Y = new float[2];
            Angle = new float[2];

            try
            {

                procedure = (VmProcedure)VmSolution.Instance[ProcessName];

                procedure.ContinuousRunEnable = false;

                VmModule = procedure;

                if (!(bool)VmSolution.Instance.IsReady)
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish("[Vision-实时图像]模块订阅未完成");
                    return false;
                }

                _eventAggregator.GetEvent<VisionExecuteEvent>().Publish(VmModule);

                if (procedure != null)
                {
                    procedure.Run();
                }
                else
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish($"{ProcessName}:实例为Null");
                    _logger.Error($"{ProcessName}:实例为Null");

                    return false;
                }




                return true;
            }
            catch (Exception ex)
            {

                _eventAggregator.GetEvent<Event_Message>().Publish($"{ProcessName}:异常，{ex.Message}");
                _logger.Error($"{ProcessName}:异常，{ex.Message}");

                return false;
            }

        }

        public async Task<bool> RealTimeImageExecute(bool RunEnable)
        {
            try
            {
                procedure = (VmProcedure)VmSolution.Instance["RealTime"];


                procedure.ContinuousRunEnable = RunEnable;

                await Task.Delay(500);

                if (!RunEnable)
                {
                    return true;
                }



                VmModule = procedure;
                if (!(bool)VmSolution.Instance.IsReady)
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish("[Vision-实时图像]模块订阅未完成");
                    return false;
                }

                _eventAggregator.GetEvent<VisionExecuteEvent>().Publish(VmModule);

                if (procedure != null) { procedure.Run(); }

                procedure.ContinuousRunEnable = RunEnable;
                //_eventAggregator.GetEvent<VisionExecuteEvent>().Publish(VmModule);

                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<Event_Message>().Publish($"[Vision]实时图像:{ex.Message}");

                return false;
            }
        }

        public void Save()
        {
            if (mSolutionIsLoad == true)
            {
                try
                {
                    VmSolution.Save();
                    MessageBox.Show($"{Solutionpath}{SolutionName}-保存成功");
                }
                catch (VmException ex)
                {
                    _eventAggregator.GetEvent<Event_Message>().Publish($"[Vision]方案保存失败:{ex.Message}");
                    return;
                }
            }
        }

        public void Close()
        {
            if (mSolutionIsLoad == true)
            {
                try
                {
                    VmSolution.Instance.CloseSolution();
                    _eventAggregator.GetEvent<Event_Message>().Publish($"[Vision]方案关闭！");
                }
                catch (Exception ex)
                {

                    _eventAggregator.GetEvent<Event_Message>().Publish($"[Vision]方案关闭失败：{ex.Message}");
                }
            }
        }
    }
}
