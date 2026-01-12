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
        /// 位置
        /// </summary>
        public double Pos { get; set; }

        public double ADC { get; set; }
        /// <summary>
        /// 耦合数据
        /// </summary>
        public List<double> Value { get; set; }

        public CouplingData()
        {
            Value = new List<double>();
        }

    }
}
