using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Motion.Interfaces;
using DM.Foundation.Shared.Constants;
using DM.Foundation.Shared.Events;
using DM.Foundation.Shared.Models;
using ImTools;
using ONet.FAU.Rx._16_128.Extension.Model;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Common
{
    public class CouplingController
    {
        public async Task<Result1D> Run1DFullRangeAsync(IMotionControl motion, Parameter1D para, CancellationToken token, IEventAggregator eventAggregator, ToolParameter toolParameter, ILogger logger,LD9208Controller LD9208A,LD9208Controller LD9208B, int chartID)
        {

            Result1D result = new Result1D();
            List<CouplingData> couplingData = new List<CouplingData>();

            try
            {
                double StartPos = motion.GetPulsePosition();//获取当前位置

                double Max = StartPos + (para.Range / 2);//耦合位置最大值

                double Min = StartPos - (para.Range / 2);//耦合位置最小值

                await motion.MoveAbsAsync(Min, 3, token);

                LD9208A.GetAllPower();
                LD9208B.GetAllPower();  

                await Task.Delay(500);

                couplingData.Clear();//清除位置和功率记录列表

                couplingData.Add( AddCurrentData_1D(motion, LD9208A, LD9208B, para,logger));

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        motion.Stop(0);
                        result.Success = false;
                        await Task.Delay(200);

                        await motion.MoveAbsAsync(StartPos, 3, token);
                        eventAggregator.GetEvent<Event_Message>().Publish($"{toolParameter.UserDefined}:取消耦合。");
                        return result;
                    }

                    //判断当前位置是否超出最小位置限位
                    if (motion.GetPulsePosition() > Max)
                    {
                        eventAggregator.GetEvent<Event_Message>().Publish(
                            $"{toolParameter.UserDefined}:1D耦合,超过最大限位位置，退出耦合。" +
                            $"轴当前位置{motion.GetPulsePosition()}" +
                            $"最大限位:{Max}");
                        break;
                    }

                    #region 轴移动->等待停止->延时
                    await motion.MoveRelAsync(para.StepDist, true, para.AxisVel, token);

                    await Task.Delay(para.DataDelay);
                    #endregion

                    couplingData.Add(AddCurrentData_1D(motion, LD9208A, LD9208B, para,logger));

                    ShowData(couplingData, para, eventAggregator, chartID);
                }



                var flatRes = ExtractFlatPointsByChannel(couplingData);

                var bestpos = FindBestPos(flatRes);

                //CouplingData maxAdcItem = couplingData.OrderByDescending(x => x.ADC).First();

                await motion.MoveAbsAsync(bestpos, 2, token);

                eventAggregator.GetEvent<Event_Message>().Publish($"{toolParameter.UserDefined}:耦合完成，当前位置:{bestpos}");

                result.Success = true;

                result.CouplingData = couplingData;

                if (toolParameter.IsSaveData)
                {
                    if (couplingData != null)
                    {
                        DataSaveHelper.ExportToCsv(couplingData, logger, $"{toolParameter.UserDefined}-{para.AxisName}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                eventAggregator.GetEvent<Event_Message>().Publish($"{toolParameter.UserDefined}:1D耦合:{ex.ToString()}");
                logger.Error(ex.ToString());
                result.Success = false;
                return result;
            }


        }

        public async Task<Result1D> Run1DPartialRangeAsync(IMotionControl motion,Parameter1D para, CancellationToken token,IEventAggregator eventAggregator,ToolParameter toolParameter, ILogger logger,LD9208Controller LD9208A,LD9208Controller LD9208B,int chartID)
        {
            Result1D result = new Result1D();
            List<CouplingData> couplingData = new List<CouplingData>();

            try
            {
                double startPos = motion.GetPulsePosition();

                LD9208A.GetAllPower();
                LD9208B.GetAllPower();
                await Task.Delay(500);

                couplingData.Clear();
                couplingData.Add(AddCurrentData_1D(motion, LD9208A, LD9208B, para, logger));

                //正向扫描
                await SweepOneDirectionAsync(+1, motion, para, token, eventAggregator, toolParameter,logger, LD9208A, LD9208B, chartID, couplingData, startPos);

                //回到起始位置
                await motion.MoveAbsAsync(startPos, 3, token);

                //反向扫描
                await SweepOneDirectionAsync(-1, motion, para, token, eventAggregator, toolParameter,logger, LD9208A, LD9208B, chartID, couplingData, startPos);

                // 找最佳位置
                var flatRes = ExtractFlatPointsByChannel(couplingData);
                var bestpos = FindBestPos(flatRes);

                await motion.MoveAbsAsync(bestpos, 2, token);
                eventAggregator.GetEvent<Event_Message>().Publish($"{toolParameter.UserDefined}:耦合完成，当前位置:{bestpos}");

                result.Success = true;
                result.CouplingData = couplingData;

                if (toolParameter.IsSaveData && couplingData != null)
                    DataSaveHelper.ExportToCsv(couplingData, logger, $"{toolParameter.UserDefined}-{para.AxisName}");

                return result;

               
            }
            catch (Exception ex)
            {
                eventAggregator.GetEvent<Event_Message>().Publish($"{toolParameter.UserDefined}:1D耦合:{ex}");
                logger.Error(ex.ToString());
                result.Success = false;
                return result;
            }
        }


        /// <summary>
        /// FA矫正耦合
        /// </summary>
        /// <param name="motion"></param>
        /// <param name="para"></param>
        /// <param name="token"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="toolParameter"></param>
        /// <param name="logger"></param>
        /// <param name="LD9208A"></param>
        /// <param name="LD9208B"></param>
        /// <param name="chartID"></param>
        /// <returns></returns>
        public async Task<Result1D> Run1DCouplingCorrectionAsync(IMotionControl motion, Parameter1D para, CancellationToken token, IEventAggregator eventAggregator, ToolParameter toolParameter, ILogger logger, LD9208Controller LD9208A, LD9208Controller LD9208B, int chartID, CorrectChannel correctChannel)
        {

            Result1D result = new Result1D();
            List<CouplingData> couplingData = new List<CouplingData>();

            try
            {
                double StartPos = motion.GetPulsePosition();//获取当前位置

                double Max = StartPos + (para.Range / 2);//耦合位置最大值

                double Min = StartPos - (para.Range / 2);//耦合位置最小值

                await motion.MoveAbsAsync(Min, 3, token);

                LD9208A.GetAllPower();
                LD9208B.GetAllPower();

                await Task.Delay(500);

                couplingData.Clear();//清除位置和功率记录列表

                couplingData.Add(AddCurrentData_1D(motion, LD9208A, LD9208B, para, logger,correctChannel));

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        motion.Stop(0);
                        result.Success = false;
                        await Task.Delay(200);

                        await motion.MoveAbsAsync(StartPos, 3, token);
                        eventAggregator.GetEvent<Event_Message>().Publish($"{toolParameter.UserDefined}:取消耦合。");
                        return result;
                    }

                    //判断当前位置是否超出最小位置限位
                    if (motion.GetPulsePosition() > Max)
                    {
                        eventAggregator.GetEvent<Event_Message>().Publish(
                            $"{toolParameter.UserDefined}:1D耦合,超过最大限位位置，退出耦合。" +
                            $"轴当前位置{motion.GetPulsePosition()}" +
                            $"最大限位:{Max}");
                        break;
                    }

                    #region 轴移动->等待停止->延时
                        await motion.MoveRelAsync(para.StepDist, true, para.AxisVel, token);

                        await Task.Delay(para.DataDelay);
                    #endregion

                    couplingData.Add(AddCurrentData_1D(motion, LD9208A, LD9208B, para, logger, correctChannel));

                    ShowDataSingle(couplingData, para, eventAggregator, chartID);
                }



             

                CouplingData bestpos = couplingData.OrderByDescending(x => x.Values[0]).First();

                await motion.MoveAbsAsync(bestpos.Pos, 2, token);

                eventAggregator.GetEvent<Event_Message>().Publish($"{toolParameter.UserDefined}:耦合完成，当前位置:{bestpos.Pos}");

                result.Success = true;
                result.BestPos = bestpos.Pos;

                result.CouplingData = couplingData;

                if (toolParameter.IsSaveData)
                {
                    if (couplingData != null)
                    {
                        DataSaveHelper.ExportToCsv(couplingData, logger, $"{toolParameter.UserDefined}-{para.AxisName}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                eventAggregator.GetEvent<Event_Message>().Publish($"{toolParameter.UserDefined}:1D耦合:{ex.ToString()}");
                logger.Error(ex.ToString());
                result.Success = false;
                return result;
            }


        }



        public async Task<Result2D> Run2DFullRangeAsync(IMotionControl motionX,IMotionControl motionY, Parameter2D para, CancellationToken token, IEventAggregator eventAggregator, ToolParameter toolParameter, ILogger logger, LD9208Controller LD9208A, LD9208Controller LD9208B, int chartID)
        {

            Result2D result = new Result2D();
        

            try
            {

                var resultA =await Run1DFullRangeAsync(motionX,para.XParameter,token,eventAggregator,toolParameter,logger,LD9208A,LD9208B,chartID);

                await Task.Delay(200);

                var resultB =await Run1DFullRangeAsync(motionY, para.YParameter, token, eventAggregator, toolParameter, logger, LD9208A, LD9208B, chartID);


                result.XResult = resultA;
                result.YResult = resultB;


                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                eventAggregator.GetEvent<Event_Message>().Publish($"{toolParameter.UserDefined}:1D耦合:{ex.ToString()}");
                logger.Error(ex.ToString());
                result.Success = false;
                return result;
            }


        }


        /// <summary>
        /// 耦合矫正FA，耦合CH1-CH8获取FA与PIC角度
        /// </summary>
        /// <param name="motionX"></param>
        /// <param name="motionY"></param>
        /// <param name="para"></param>
        /// <param name="token"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="toolParameter"></param>
        /// <param name="logger"></param>
        /// <param name="LD9208A"></param>
        /// <param name="LD9208B"></param>
        /// <param name="chartID"></param>
        /// <returns></returns>
        public async Task<bool> RunFACouplingCorrectionAsync(IMotionSystemService motionSystem, IMotionControl motionX, IMotionControl motionY, Parameter2D para, CancellationToken token, IEventAggregator eventAggregator, ToolParameter toolParameter, ILogger logger, LD9208Controller LD9208A, LD9208Controller LD9208B, int chartID,string Text)
        {
            try
            {
                Result2D result = new Result2D();

                for (int i = 0; i < para.LoopCountt; i++)
                {
                    var CH_1_X = await Run1DCouplingCorrectionAsync(motionX, para.XParameter, token, eventAggregator, toolParameter, logger, LD9208A, LD9208B, chartID, CorrectChannel.Channle_1);

                    await Task.Delay(200);

                    var CH_1_Y = await Run1DCouplingCorrectionAsync(motionY, para.YParameter, token, eventAggregator, toolParameter, logger, LD9208A, LD9208B, chartID, CorrectChannel.Channle_1);

                    await Task.Delay(200);

                    var CH_8_X = await Run1DCouplingCorrectionAsync(motionX, para.XParameter, token, eventAggregator, toolParameter, logger, LD9208A, LD9208B, chartID, CorrectChannel.Channle_8);

                    await Task.Delay(200);

                    var CH_8_Y = await Run1DCouplingCorrectionAsync(motionY, para.YParameter, token, eventAggregator, toolParameter, logger, LD9208A, LD9208B, chartID, CorrectChannel.Channle_8);

                    eventAggregator.GetEvent<Event_Message>().Publish($"CH1_Y:{CH_1_Y.BestPos},CH8_Y:{CH_8_Y.BestPos}");

                    //计算矫正角度
                    double deltaX = CH_8_Y.BestPos - CH_1_Y.BestPos;

                    double angle = Math.Atan2(deltaX, 1.5) * (180 / Math.PI);

                    eventAggregator.GetEvent<Event_Message>().Publish($"{Text}:耦合角度:{angle.ToString("F4")}");

                    if (Math.Abs(angle) <= 0.01) return true;

                    if (Math.Abs(angle) < 1)
                    {
                        if (para.Axisgroup == AxisGroup.Left)
                        {
                            angle = (angle > 0) ? -angle : Math.Abs(angle);
                            eventAggregator.GetEvent<Event_Message>().Publish($"{Text}角度调整:{angle}");
                            var axis = motionSystem.GetAxis(MotionAxisNames.LeftRX);
                            await axis.MoveRelAsync(angle, 1, token);
                        }
                        else
                        {
                            angle = (angle > 0) ? -angle : Math.Abs(angle);
                            eventAggregator.GetEvent<Event_Message>().Publish($"{Text}角度调整:{angle}");
                            var axis = motionSystem.GetAxis(MotionAxisNames.RightRX);
                            await axis.MoveRelAsync(angle, 1, token);
                        }
                    }
                    else
                    {
                        eventAggregator.GetEvent<Event_Message>().Publish($"FA角度大于设定阈值，请检查。");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {

                logger.Error(ex.ToString());

                return false;   
            }
        }
        

        private CouplingData AddCurrentData_1D(IMotionControl motion,LD9208Controller LD9208A, LD9208Controller LD9208B, Parameter1D para,ILogger logger)
        {
            try
            {
                // 获取 A、B 两台设备的所有功率数据
                string rawA = LD9208A.GetAllPower();
                string rawB = LD9208B.GetAllPower();

                if (string.IsNullOrWhiteSpace(rawA) || string.IsNullOrWhiteSpace(rawB))
                {
                    logger?.Warn("AddCurrentData_1D: GetAllPower 返回空数据");
                    return null;
                }

                string[] dataA = rawA.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] dataB = rawB.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                List<double> values = new List<double>(dataA.Length + dataB.Length);

                // 转换 A 的数据
                for (int i = 0; i < dataA.Length; i++)
                {
                    if (double.TryParse(dataA[i].Trim(), out double val))
                        values.Add(val);
                    else
                        values.Add(double.NaN); // 转换失败时放入 NaN
                }

                // 转换 B 的数据
                for (int i = 0; i < dataB.Length; i++)
                {
                    if (double.TryParse(dataB[i].Trim(), out double val))
                        values.Add(val);
                    else
                        values.Add(double.NaN);
                }

                var result =  CouplingData.CreateEight(motion.GetPulsePositionEx(), values);


                return result;
            }
            catch (Exception ex)
            {
                logger?.Error(ex.ToString(), "AddCurrentData_1D: 获取耦合数据失败");
                return null;
            }

        }

        private CouplingData AddCurrentData_1D(IMotionControl motion, LD9208Controller LD9208A, LD9208Controller LD9208B, Parameter1D para, ILogger logger, CorrectChannel correctChannel)
        {
            try
            {
                // 获取 A、B 两台设备的所有功率数据
                string rawA = LD9208A.GetAllPower();
                string rawB = LD9208B.GetAllPower();

                if (string.IsNullOrWhiteSpace(rawA) || string.IsNullOrWhiteSpace(rawB))
                {
                    logger?.Warn("AddCurrentData_1D: GetAllPower 返回空数据");
                    return null;
                }

                string[] dataA = rawA.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] dataB = rawB.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                //List<double> values = new List<double>(dataA.Length + dataB.Length);

                //// 转换 A 的数据
                //for (int i = 0; i < dataA.Length; i++)
                //{
                //    if (double.TryParse(dataA[i].Trim(), out double val))
                //        values.Add(val);
                //    else
                //        values.Add(double.NaN); // 转换失败时放入 NaN
                //}

                //// 转换 B 的数据
                //for (int i = 0; i < dataB.Length; i++)
                //{
                //    if (double.TryParse(dataB[i].Trim(), out double val))
                //        values.Add(val);
                //    else
                //        values.Add(double.NaN);
                //}
                double ResultData = 0;

                if (correctChannel == CorrectChannel.Channle_1)
                {
                    double.TryParse(dataA[0].Trim(), out ResultData);
                }else if (correctChannel == CorrectChannel.Channle_8)
                {
                    double.TryParse(dataB[2].Trim(), out ResultData);
                }


                var result = CouplingData.CreateSingle(motion.GetPulsePositionEx(), ResultData);


                return result;
            }
            catch (Exception ex)
            {
                logger?.Error(ex.ToString(), "AddCurrentData_1D: 获取耦合数据失败");
                return null;
            }

        }

        private void ShowData(List<CouplingData> couplingDatas,Parameter1D para ,IEventAggregator eventAggregator,int chartID)
        {
            PlotDataMessage plotDataMessage = new PlotDataMessage()
            {
                ChartId = chartID,
                Label = para.AxisName,
                Xs = new List<double>(),       // 横轴数据
                Ys = new List<double>(), // 每个 double[8] 对应一个 X 点的 8 通道数据
                MultiChannel = new List<List<double>>()
            };

            plotDataMessage.Xs.AddRange(couplingDatas.Select(P => (double)P.Pos).ToList());


            plotDataMessage.MultiChannel.AddRange(couplingDatas.Select(P=>P.Values));

            eventAggregator.GetEvent<MultiChannelPlotDataEvetn>().Publish(plotDataMessage);


        }

        private void ShowDataSingle(List<CouplingData> couplingDatas, Parameter1D para, IEventAggregator eventAggregator, int chartID)
        {
            PlotDataMessage plotDataMessage = new PlotDataMessage()
            {
                ChartId = chartID,
                Label = para.AxisName,
                Xs = new List<double>(),       // 横轴数据
                Ys = new List<double>(), // 每个 double[8] 对应一个 X 点的 8 通道数据
                MultiChannel = new List<List<double>>()
            };

            plotDataMessage.Xs.AddRange(couplingDatas.Select(P => (double)P.Pos).ToList());


            plotDataMessage.Ys.AddRange(couplingDatas.Select(P => P.Values[0]).ToList());

            //eventAggregator.GetEvent<MultiChannelPlotDataEvetn>().Publish(plotDataMessage);

            eventAggregator.GetEvent<PlotDataEvent>().Publish(plotDataMessage);
        }


        







        /// <summary>
        /// 提取所有通道的平坦区点（阈值 = min + range * ratio）
        /// </summary>
        public static List<FlatAreaData> ExtractFlatPoints(List<CouplingData> data, double ratio = 0.8)
        {
            var result = new List<FlatAreaData>();
            if (data == null || data.Count == 0) return result;

            int maxCh = data.Max(d => d.ChannelCount);

            for (int ch = 0; ch < maxCh; ch++)
            {
                var series = data
                    .Where(d => d.Values.Count > ch)
                    .Select(d => new { d.Pos, Value = d.Values[ch] })
                    .ToList();

                if (series.Count == 0) continue;

                double max = series.Max(p => p.Value);
                double min = series.Min(p => p.Value);
                double range = max - min;

                double threshold = min + range * ratio;

                foreach (var p in series)
                {
                    if (p.Value >= threshold)
                        result.Add(new FlatAreaData(p.Pos, p.Value, ch));
                }
            }

            return result;
        }

        /// <summary>
        /// 按通道分组返回
        /// </summary>
        public static Dictionary<int, List<FlatAreaData>> ExtractFlatPointsByChannel( List<CouplingData> data, double ratio = 0.8)
        {
            var result = new Dictionary<int, List<FlatAreaData>>();
            if (data == null || data.Count == 0) return result;

            int maxCh = data.Max(d => d.ChannelCount);

            for (int ch = 0; ch < maxCh; ch++)
            {
                var series = data
                    .Where(d => d.Values.Count > ch)
                    .Select(d => new { d.Pos, Value = d.Values[ch] })
                    .ToList();

                if (series.Count == 0)
                {
                    result[ch] = new List<FlatAreaData>();
                    continue;
                }

                double max = series.Max(p => p.Value);
                double min = series.Min(p => p.Value);
                double range = max - min;

                double threshold = min + range * ratio;

                var list = new List<FlatAreaData>();
                foreach (var p in series)
                {
                    if (p.Value >= threshold)
                        list.Add(new FlatAreaData(p.Pos, p.Value, ch));
                }

                result[ch] = list;
            }

            return result;
        }

       



        /// <summary>
        /// 查找最佳位置
        /// </summary>
        /// <param name="flatRes"></param>
        /// <param name="roundDigits"></param>
        /// <returns></returns>
        public static double FindBestPos(Dictionary<int, List<FlatAreaData>> flatRes,int roundDigits = 4)
        {
            if (flatRes == null || flatRes.Count == 0) return double.NaN;

            // 合并所有平坦区点
            var all = flatRes.SelectMany(kv => kv.Value).ToList();
            if (all.Count == 0) return double.NaN;

            // 按位置分组（四舍五入）
            var groups = all
                .GroupBy(p => Math.Round(p.Pos, roundDigits))
                .Select(g => new
                {
                    Pos = g.Key,
                    ChannelCount = g.Select(x => x.Channel).Distinct().Count(),
                    AvgValue = g.Average(x => x.Value)
                })
                .ToList();

            // 先看通道数最多，再看平均值最高
            var best = groups
                .OrderByDescending(g => g.ChannelCount)
                .ThenByDescending(g => g.AvgValue)
                .First();

            return best.Pos;
        }



        private async Task SweepOneDirectionAsync(int dir, IMotionControl motion,Parameter1D para,CancellationToken token,IEventAggregator eventAggregator,ToolParameter toolParameter,ILogger logger,LD9208Controller LD9208A,LD9208Controller LD9208B,int chartID,List<CouplingData> couplingData,double startPos,double epsilon = 0.02)
        {
            int downCount = 0;
            double? lastScore = null;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    motion.Stop(0);
                    await Task.Delay(200);
                    await motion.MoveAbsAsync(startPos, 3, token);
                    eventAggregator.GetEvent<Event_Message>().Publish($"{toolParameter.UserDefined}:取消耦合。");
                    return;
                }

                // 走一步
                await motion.MoveRelAsync(dir * para.StepDist, true, para.AxisVel, token);
                await Task.Delay(para.DataDelay);

                // 采集
                var data = AddCurrentData_1D(motion, LD9208A, LD9208B, para, logger);
                couplingData.Add(data);
                ShowData(couplingData, para, eventAggregator, chartID);

                // 评分
                double score = ComputeScore(data);

                // 下降阈值判断
                if (lastScore.HasValue && score < lastScore.Value - epsilon)
                    downCount++;
                else
                    downCount = 0;

                if (downCount >= 3)
                    break;

                lastScore = score;
            }
        }

        /// <summary>
        /// 计算得分
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private double ComputeScore(CouplingData d)
        {
            if (d.Values == null || d.Values.Count == 0) return double.NegativeInfinity;
            return d.Values.Average();
        }
    }
}
