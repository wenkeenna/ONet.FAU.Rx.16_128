using ONet.FAU.Rx._16_128.Extension.Converters;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Model
{
    public class Parameter2D : BindableBase
    {

        private ChannelGroup _selectedGroup = ChannelGroup.None;

        public ChannelGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set { _selectedGroup = value; RaisePropertyChanged(); }
        }

        private double targetvalue;
        /// <summary>
        /// 目标值
        /// </summary>
        public double TargetValue
        { get { return targetvalue; } set { targetvalue = value; RaisePropertyChanged(); } }

        private AxisGroup axisGroup;
        /// <summary>
        /// 耦合轴组
        /// </summary>
        public AxisGroup Axisgroup
        { get { return axisGroup; } set { axisGroup = value; RaisePropertyChanged(); } }



        private bool isjump;
        /// <summary>
        /// 达到目标值后是否跳出
        /// </summary>
        public bool IsJump
        { get { return isjump; } set { isjump = value; RaisePropertyChanged(); } }



        private bool isall;
        /// <summary>
        /// 是否全行程
        /// </summary>
        public bool IsAll
        { get { return isall; } set { isall = value; RaisePropertyChanged(); } }


        private int _loopcount;
        /// <summary>
        /// 循环次数
        /// </summary>
        public int LoopCountt { get { return _loopcount; } set { _loopcount = value; RaisePropertyChanged(); } }




        private bool enable;
        /// <summary>
        ///启动耦合
        /// </summary>
        public bool Enable
        { get { return enable; } set { enable = value; RaisePropertyChanged(); } }


        private Parameter1D xparameter;
        /// <summary>
        /// X耦合参数
        /// </summary>
        public Parameter1D XParameter
        { get { return xparameter; } set { xparameter = value; RaisePropertyChanged(); } }


        private Parameter1D yparameter;
        /// <summary>
        /// Y耦合参数
        /// </summary>
        public Parameter1D YParameter
        { get { return yparameter; } set { yparameter = value; RaisePropertyChanged(); } }


        public Parameter2D()
        {
            XParameter =new Parameter1D();
            YParameter = new Parameter1D();
        }



    }
}
