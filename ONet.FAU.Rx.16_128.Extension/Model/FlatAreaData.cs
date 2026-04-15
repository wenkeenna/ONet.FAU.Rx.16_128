using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Model
{
    /// <summary>
    /// 平坦区数据结构
    /// </summary>
    public class FlatAreaData
    {
        /// <summary>
        /// 位置
        /// </summary>
        public double Pos { get; set; }

        /// <summary>
        /// 该通道在平坦区的功率值
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 通道号（Value0~Value7）
        /// </summary>
        public int Channel { get; set; }

        public FlatAreaData() { }

        public FlatAreaData(double pos, double value, int channel)
        {
            Pos = pos;
            Value = value;
            Channel = channel;
        }

        public override string ToString()
        {
            return $"Pos={Pos:F4}, Value={Value:F3}, Channel={Channel}";
        }
    }
}
