using DM.Foundation.DataBinding.Interfaces;
using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Motion.Interfaces;
using DM.Foundation.Motion.Models;
using DM.Foundation.Motion.MotionSdk;
using DM.Foundation.Motion.Services;
using DM.Foundation.Shared.Events;
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
        public async Task ExecuteHomingSequenceAsync(CancellationTokenSource Cts, IDataBindingContext dataBinding)
        {
            IsRunning = true;
            //Cts = new CancellationTokenSource();


            try
            {
                var token = Cts.Token;
                // 1. 按优先级排序分组
                var groups = HomingItems
                    .GroupBy(i => i.Priority)
                    .OrderBy(g => g.Key); // 从低优先级到高

                foreach (var group in groups)
                {
                    var tasks = new List<Task>();
                    foreach (var item in group)
                    {

                        if (item.StepType == DM.Foundation.Shared.Enums.HomeStepType.SetOutput)
                        {
                            tasks.Add(HomeStepOutput(Cts, item, 1500));
                        }
                        else if (item.StepType == DM.Foundation.Shared.Enums.HomeStepType.Home)
                        {

                            if (AxisMap.TryGetValue(item.AxisName, out var axis))
                            {

                                if (item.Speed > axis.AxisPara.MaxSpeed)
                                {
                                    item.Speed = axis.AxisPara.MaxSpeed;
                                }


                                tasks.Add(axis.HomeAsync(item.Speed, 0, token));
                            }
                        }
                        else if (item.StepType == DM.Foundation.Shared.Enums.HomeStepType.AbsMove)
                        {
                            string[] command = item.BindingParamName.Split(':');//解析获取绑定参数键值
                            if (command.Length == 2)
                            {
                                var data = dataBinding.Get(command[0], command[1]);

                                if (data == null)
                                {
                                    //_eventAggregator.GetEvent<Event_Message>().Publish("回零获取数据绑定错误，请检查");
                                    //_logger.Error($"回零获取数据绑定错误，请检查{command[0]},{command[1]}");
                                    PushMessage($"回零获取数据绑定错误，请检查{command[0]},{command[1]}");
                                    throw new Exception($"Error: 获取绑定数据错误{command[0]},{command[1]}");
                                }

                                if (AxisMap.TryGetValue(item.AxisName, out var axis))
                                {
                                    tasks.Add(axis.MoveAbsAsync(Convert.ToDouble(data.Value), item.Speed, token));
                                }
                            }

                        }

                    }

                    await Task.WhenAll(tasks); // 等当前优先级组全部完成

                    foreach (var item in group)
                    {
                        //_eventAggregator.GetEvent<Event_Message>().Publish($"{item.AxisName},优先级:{item.Priority},回零完成。");
                        //_logger.Error($"{item.AxisName},优先级:{item.Priority},回零完成。");
                        PushMessage($"{item.AxisName},优先级:{item.Priority},回零完成。");
                    }
                }

                //MessageBox.Show("所有轴回零完成");

                //_eventAggregator.GetEvent<Event_Message>().Publish("所有轴回零完成");
                //_logger.Error("所有轴回零完成");
                PushMessage("所有轴回零完成");
            }
            catch (OperationCanceledException)
            {
                IsRunning = false;
                //MessageBox.Show("回零被用户取消");
                //_eventAggregator.GetEvent<Event_Message>().Publish("回零被用户取消");
                //_logger.Error("回零被用户取消");
                PushMessage("回零被用户取消");
            }
            catch (Exception ex)
            {
                IsRunning = false;
                _eventAggregator.GetEvent<Event_Message>().Publish(ex.Message);
            }
            finally
            {

            }



            return;
        }

        private async Task HomeStepOutput(CancellationTokenSource tokenSource, HomingItem item, int delaytime)
        {
            try
            {
                OutputMap.TryGetValue(item.OutputName, out IDigitalOutput output);
                InputMap.TryGetValue(item.InputName, out IDigitalInput input);


                LS_DMC3000.SetOutPut((DM.Foundation.Motion.Models.DigitalOutput)output, item.OutPutState);

                DateTime startTime = DateTime.Now;//起始时间

                TimeSpan elapsedTime;


                //_eventAggregator.GetEvent<Event_Message>().Publish($"{output.Name}:执行。");
                //_logger.Error($"{output.Name}:执行。");
                PushMessage($"{output.Name}:执行。");
                do
                {
                    if (tokenSource.IsCancellationRequested)
                    {
                        PushMessage("取消回零");
                        throw new OperationCanceledException($"Error: 取消回零");
                    }

                    if (LS_DMC3000.GetInPut((DM.Foundation.Motion.Models.DigitalInput)input) == item.InputState)
                    {
                        // _eventAggregator.GetEvent<Event_Message>().Publish($"{output.Name},{input.Name}:执行完成。");
                        PushMessage($"{output.Name},{input.Name}:执行完成。");
                        return;
                    }

                    // 计算耗时
                    elapsedTime = DateTime.Now - startTime;

                    await Task.Delay(10);
                } while (elapsedTime < TimeSpan.FromMilliseconds(delaytime));


                PushMessage($"{output.Name}:执行超时，{elapsedTime.ToString()}ms,请检查。");
                throw new Exception($"执行超时");

            }
            catch (Exception ex)
            {

                PushMessage(ex.ToString());
                throw new Exception($"Error: {ex.Message}");
            }
        }

        private void PushMessage(string msg)
        {
            _eventAggregator.GetEvent<Event_Message>().Publish(msg);
            _logger.Info(msg);
        }





        /// <summary>
        /// 执行复位
        /// </summary>
        /// <param name="Cts"></param>
        /// <param name="dataBinding"></param>
        /// <returns></returns>
        public async Task ExecuteResetSequenceAsync(CancellationTokenSource Cts, IDataBindingContext dataBinding)
        {
            IsRunning = true;
            //Cts = new CancellationTokenSource();


            try
            {
                var token = Cts.Token;
                // 1. 按优先级排序分组
                var groups = ResetItems
                    .GroupBy(i => i.Priority)
                    .OrderBy(g => g.Key); // 从低优先级到高

                foreach (var group in groups)
                {
                    var tasks = new List<Task>();
                    foreach (var item in group)
                    {


                        if (item.StepType == DM.Foundation.Shared.Enums.HomeStepType.AbsMove)
                        {
                            string[] command = item.BindingParamName.Split(':');//解析获取绑定参数键值
                            if (command.Length == 2)
                            {
                                var data = dataBinding.Get(command[0], command[1]);

                                if (data == null)
                                {
                                    //_eventAggregator.GetEvent<Event_Message>().Publish("复位获取数据绑定错误，请检查");
                                    //_logger.Error($"复位获取数据绑定错误{command[0]},{command[1]}");
                                    PushMessage($"复位获取数据绑定错误{command[0]},{command[1]}");
                                    throw new Exception($"Error: 获取绑定数据错误{command[0]},{command[1]}");
                                }

                                if (AxisMap.TryGetValue(item.AxisName, out var axis))
                                {

                                    if (item.Speed > axis.AxisPara.MaxSpeed)
                                    {
                                        item.Speed = axis.AxisPara.MaxSpeed;
                                    }



                                    tasks.Add(axis.MoveAbsAsync(Convert.ToDouble(data.Value), item.Speed, token));
                                }
                            }


                        }
                        else if (item.StepType == DM.Foundation.Shared.Enums.HomeStepType.SetOutput)
                        {
                            tasks.Add(HomeStepOutput(Cts, item, 1500));
                        }

                    }

                    await Task.WhenAll(tasks); // 等当前优先级组全部完成

                    foreach (var item in group)
                    {
                        //_eventAggregator.GetEvent<Event_Message>().Publish($"{item.AxisName},优先级:{item.Priority},复位完成。");
                        //_logger.Error($"{item.AxisName},优先级:{item.Priority},复位完成。");
                        PushMessage($"{item.AxisName},优先级:{item.Priority},复位完成。");
                    }
                }
                PushMessage("所有轴复位完成");
            }
            catch (OperationCanceledException)
            {
                IsRunning = false;
                //MessageBox.Show("回零被用户取消");
                //_eventAggregator.GetEvent<Event_Message>().Publish("复位被用户取消");
                //_logger.Error("复位被用户取消");
                PushMessage("复位被用户取消");
            }
            catch (Exception ex)
            {
                IsRunning = false;
                //_eventAggregator.GetEvent<Event_Message>().Publish(ex.Message);
                //_logger.Error(ex.Message);
                PushMessage(ex.ToString());
            }
            return;
        }

       public void Cancel()
        {

        }

        /// <summary>
        /// 初始化控制卡
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Initialize()
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
        public bool DownloadConfigFile()
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
        public bool ReadInput(DigitalInput input)
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
        public bool WriteOutput(DigitalOutput output, bool status)
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
        public bool ReadOutput(DigitalOutput output)
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
        public async Task<bool> EmergencyStopAsync(int cardIndex)
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
        public bool SetProfile(ushort card, ushort axis, int minVel, int maxVel, double acc, double dec, int stopVel)
        {
            return false;
        }
    }
}
