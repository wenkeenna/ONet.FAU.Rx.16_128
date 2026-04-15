using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Model
{
    /// <summary>
    /// 耦合数据
    /// </summary>
    public class CouplingData
    {
        /// <summary>
        /// 位置（Pos）
        /// </summary>
        public double Pos { get; set; }

        /// <summary>
        /// 通道功率值列表
        /// 单通道时：Count = 1
        /// 八通道时：Count = 8
        /// </summary>
        public List<double> Values { get; set; }

        /// <summary>
        /// 通道数量（1 或 8）
        /// </summary>
        public int ChannelCount => Values.Count;

        /// <summary>
        /// 创建单通道数据
        /// </summary>
        public static CouplingData CreateSingle(double pos, double value)
        {
            return new CouplingData
            {
                Pos = pos,
                Values = new List<double> { value }
            };
        }

        /// <summary>
        /// 创建八通道数据
        /// </summary>
        public static CouplingData CreateEight(double pos, List<double> values)
        {
            if (values == null || values.Count != 8)
                throw new ArgumentException("八通道数据必须提供8个值");

            return new CouplingData
            {
                Pos = pos,
                Values = new List<double>(values)
            };
        }

        /// <summary>
        /// 获取指定通道的值（0-based）
        /// </summary>
        public double GetValue(int channel)
        {
            if (channel < 0 || channel >= ChannelCount) return double.NaN;
            return Values[channel];
        }

    }
}
