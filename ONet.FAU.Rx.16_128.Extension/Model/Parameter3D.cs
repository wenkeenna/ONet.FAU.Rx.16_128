using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Model
{
    public class Parameter3D : BindableBase
    {
        private Parameter2D xyparameter;
        /// <summary>
        /// XY耦合参数
        /// </summary>
        public Parameter2D XYParameter
        { get { return xyparameter; } set { xyparameter = value; RaisePropertyChanged(); } }


        private Parameter1D zparameter;
        /// <summary>
        /// Z耦合参数
        /// </summary>
        public Parameter1D ZParameter
        { get { return zparameter; } set { zparameter = value; RaisePropertyChanged(); } }
    }
}
