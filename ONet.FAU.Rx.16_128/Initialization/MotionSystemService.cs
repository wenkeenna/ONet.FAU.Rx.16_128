using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Motion.Interfaces;
using DM.Foundation.Motion.Models;
using DM.Foundation.Motion.MotionSdk;
using DM.Foundation.Shared.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Initialization
{
    public class MotionSystemService : IMotionSystemService
    {
        #region 基础字段/方法

        /// <summary>
        /// 轴映射
        /// </summary>
        public Dictionary<string, IMotionControl> AxisMap { get; } = new Dictionary<string, IMotionControl>();
        /// <summary>
        /// 输入端口映射
        /// </summary>
        public Dictionary<string, IDigitalInput> InputMap { get; } = new Dictionary<string, IDigitalInput>();
        /// <summary>
        /// 输出端口映射
        /// </summary>
        public Dictionary<string, IDigitalOutput> OutputMap { get; } = new Dictionary<string, IDigitalOutput>();

        /// <summary>
        /// 回零流程列表
        /// </summary>
        public List<HomingItem> HomingItems { get; } = new List<HomingItem>();
        /// <summary>
        /// 复位流程列表
        /// </summary>
        public List<HomingItem> ResetItems { get; } = new List<HomingItem>();

        //public CancellationTokenSource Cts { get; set; }

        public bool IsRunning { get; set; } = false;
        IEventAggregator _eventAggregator;
        ILogger _logger;
        public MotionSystemService(IEventAggregator eventAggregator, ILogger logger)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }


        /// <summary>
        /// 获取轴实例
        /// </summary>
        /// <param name="axisName"></param>
        /// <returns></returns>
        public IMotionControl GetAxis(string axisName)
        {
            AxisMap.TryGetValue(axisName, out var axis);
            return axis;
        }

        /// <summary>
        /// 添加轴实例
        /// </summary>
        /// <param name="axis"></param>
        public void AddAxis(IMotionControl axis)
        {
            if (!AxisMap.ContainsKey(axis.AxisKey))
                AxisMap[axis.AxisKey] = axis;
        }

        /// <summary>
        /// 获取输入端口实例
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDigitalInput GetInput(string name)
        {
            InputMap.TryGetValue(name, out var input);
            return input;
        }
        /// <summary>
        /// 获取输出端口实例
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDigitalOutput GetOutput(string name)
        {
            OutputMap.TryGetValue(name, out var output);
            return output;
        }

        /// <summary>
        /// 添加输入端口实例
        /// </summary>
        /// <param name="input"></param>
        public void AddInput(IDigitalInput input)
        {
            if (!InputMap.ContainsKey(input.Name))
                InputMap[input.Name] = input;
        }
        /// <summary>
        /// 添加输出端口实例
        /// </summary>
        /// <param name="output"></param>
        public void AddOutput(IDigitalOutput output)
        {
            if (!OutputMap.ContainsKey(output.Name))
                OutputMap[output.Name] = output;
        }
        /// <summary>
        /// 获取全部轴名称
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllAxisKeys() => AxisMap.Keys;

        /// <summary>
        /// 获取全部输入端口名称
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllInputKeys() => InputMap.Keys;

        /// <summary>
        /// 获取全部输出端口名称
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllOutputKeys() => OutputMap.Keys;

        #endregion


        /// <summary>
        /// 执行回零
        /// </summary>
        /// <param name="Cts"></param>
        /// <param name="dataBinding"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        Task IMotionSystemService.ExecuteHomingSequenceAsync(CancellationTokenSource Cts, IDataBindingContext dataBinding)
        {
            return null;
        }
        /// <summary>
        /// 执行复位
        /// </summary>
        /// <param name="Cts"></param>
        /// <param name="dataBinding"></param>
        /// <returns></returns>
        Task IMotionSystemService.ExecuteResetSequenceAsync(CancellationTokenSource Cts, IDataBindingContext dataBinding)
        {
            return null;
        }

        void IMotionSystemService.Cancel()
        {

        }

        /// <summary>
        /// 初始化控制卡
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        bool IMotionSystemService.Initialize()
        {


            short ret = LTDMC.dmc_board_init();
            if (ret == 0 || ret < 0)
                throw new Exception($"Error: 初始化控制卡异常，错误码: {ret}");

            _logger.Info($"控制卡初始化成功，控制卡数量:{ret}");
            return true;
        }
        /// <summary>
        /// 下载配置文件
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        bool IMotionSystemService.DownloadConfigFile()
        {
            try
            {

                string card_1_Path = "";

                string card_2_Path = "";

                short ret = LTDMC.dmc_download_configfile((ushort)0, card_1_Path);

                if (ret != 0)
                    throw new Exception($"Error: 下载轴配置文件异常，错误码: {ret}");


                ret = LTDMC.dmc_download_configfile((ushort)1, card_2_Path);

                if (ret != 0)
                    throw new Exception($"Error: 下载轴配置文件异常，错误码: {ret}");

                return true;
            }
            catch (Exception ex)
            {

                throw new Exception($"Error: 下载轴配置文件异常，错误码: {ex}");
            }
        }

        /// <summary>
        /// 读取输入端口电平
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        bool IMotionSystemService.ReadInput(DigitalInput input)
        {
            try
            {
                short res = LTDMC.dmc_read_inbit((ushort)input.CardIndex, (ushort)input.Address);
                if (res == 0)
                    return true;
                else if (res == 1)
                    return false;
                else
                    throw new Exception($"Error: 读取通用InPut状态失败，错误码：{res}");
            }
            catch (Exception ex)
            {

                throw new Exception($"Error: 读取通用InPut状态失败，错误码：{ex.Message}");
            }
        }
        /// <summary>
        /// 写入输出端口电平
        /// </summary>
        /// <param name="output"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        bool IMotionSystemService.WriteOutput(DigitalOutput output, bool status)
        {
            try
            {
                short res = LTDMC.dmc_write_outbit((ushort)output.CardIndex, (ushort)output.Address, (ushort)(status ? 0 : 1));
                //short res = LTDMC.dmc_write_outbit((ushort)outputinfo.CardIndex, (ushort)outputinfo.Address, (ushort)(Status ? 1 : 0));
                if (res == 1)
                    return true;
                else if (res == 0)
                    return false;
                else
                    throw new Exception($"Error: 写入通用OutPut状态失败，错误码：{res}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: 写入通用OutPut状态失败，错误码：{ex.Message}");

            }
        }
        /// <summary>
        /// 读取输出端口电平
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        bool IMotionSystemService.ReadOutput(DigitalOutput output)
        {
            try
            {
                short res = LTDMC.dmc_read_outbit((ushort)output.CardIndex, (ushort)output.Address);
                if (res == 1)
                    return false;
                else if (res == 0)
                    return true;
                else
                    throw new Exception($"Error: 读取通用OutPut状态失败，错误码：{res}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: 读取通用OutPut状态失败，错误码：{ex.Message}");

            }
        }

        /// <summary>
        /// 紧急停止
        /// </summary>
        /// <param name="cardIndex"></param>
        /// <returns></returns>
        async Task<bool> IMotionSystemService.EmergencyStopAsync(int cardIndex)
        {
            await Task.Delay(1000);
            return false;
        }
        /// <summary>
        /// 设置速度曲线
        /// </summary>
        /// <param name="card"></param>
        /// <param name="axis"></param>
        /// <param name="minVel"></param>
        /// <param name="maxVel"></param>
        /// <param name="acc"></param>
        /// <param name="dec"></param>
        /// <param name="stopVel"></param>
        /// <returns></returns>
        bool IMotionSystemService.SetProfile(ushort card, ushort axis, int minVel, int maxVel, double acc, double dec, int stopVel)
        {
            return false;
        }
    }
}
