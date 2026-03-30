using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Motion.Interfaces;
using DM.Foundation.Shared.Events;
using DM.Foundation.Shared.Models;
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

                await Task.Delay(50);

                couplingData.Clear();//清除位置和功率记录列表

                couplingData.Add( AddCurrentData_1D(motion, LD9208A, LD9208B, para));

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

                    couplingData.Add(AddCurrentData_1D(motion, LD9208A, LD9208B, para));

                    ShowData(couplingData, para, eventAggregator, chartID);
                }


                CouplingData maxAdcItem = couplingData.OrderByDescending(x => x.ADC).First();

                await motion.MoveAbsAsync(maxAdcItem.Pos, 2, token);

                eventAggregator.GetEvent<Event_Message>().Publish($"{toolParameter.UserDefined}:耦合完成，当前位置:{maxAdcItem.Pos},ADC:{maxAdcItem.ADC}");

                result.Success = true;

                result.CouplingData = couplingData;

                if (para.IsSaveData)
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


        private List<double> TempData = new List<double>();
        private CouplingData AddCurrentData_1D(IMotionControl motion,LD9208Controller LD9208A, LD9208Controller LD9208B, Parameter1D para)
        {
            try
            {
                TempData.Clear();

                var dataA = LD9208A.GetAllPower().Split(',');
                for (int i = 0; i < dataA.Length; i++)
                {
                    //TempData[i] = Convert.ToDouble(dataA[i]);
                    TempData.Add(Convert.ToDouble(dataA[i]));
                }

                var dataB = LD9208B.GetAllPower().Split(',');
                for (int i = 0; i < dataB.Length; i++)
                {
                   // TempData[i + 4] = Convert.ToDouble(dataB[i]);
                    TempData.Add(Convert.ToDouble(dataB[i]));
                }

                var result = new CouplingData()
                {
                    Pos = motion.GetPulsePositionEx(),
                    Value = TempData
                };

                return result;
            }
            catch (Exception)
            {

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


            plotDataMessage.MultiChannel.AddRange(couplingDatas.Select(P=>P.Value));

            eventAggregator.GetEvent<MultiChannelPlotDataEvetn>().Publish(plotDataMessage);


        }
    }
}
