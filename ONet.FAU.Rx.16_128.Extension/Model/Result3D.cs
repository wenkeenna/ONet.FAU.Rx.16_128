using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Model
{
    public class Result3D
    {
        public bool Success { get; set; }

        public Result2D XYResult { get; set; }
        public Result1D ZResult { get; set; }
    }
}
