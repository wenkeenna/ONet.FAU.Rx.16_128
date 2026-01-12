using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Model
{
    public class Parameter1D : BindableBase
    {
        public List<CouplingData> Data = new List<CouplingData>();

        private double stepdist;
        /// <summary>
        /// 单步位移距离单位：mm
        /// </summary>
        public double StepDist
        { get { return stepdist; } set { stepdist = value; RaisePropertyChanged(); } }



        private double range;
        /// <summary>
        /// 耦合范围单位：mm
        /// </summary>
        public double Range
        { get { return range; } set { range = value; RaisePropertyChanged(); } }




        private bool isall;
        /// <summary>
        /// 是否全行程
        /// </summary>
        public bool IsAll
        { get { return isall; } set { isall = value; RaisePropertyChanged(); } }





        private AxisGroup axisGroup;
        /// <summary>
        /// 耦合轴组
        /// </summary>
        public AxisGroup Axisgroup
        { get { return axisGroup; } set { axisGroup = value; RaisePropertyChanged(); } }


        //private CouplingDataSource dataSource;
        ///// <summary>
        ///// 耦合数据来源
        ///// </summary>
        //public CouplingDataSource DataSource { get { return dataSource; } set { dataSource = value; RaisePropertyChanged(); } }





        private ProductType type;
        /// <summary>
        /// 产品类型
        /// </summary>
        public ProductType Type
        { get { return type; } set { type = value; RaisePropertyChanged(); } }





        private double targetvalue;
        /// <summary>
        /// 目标值
        /// </summary>
        public double TargetValue
        { get { return targetvalue; } set { targetvalue = value; RaisePropertyChanged(); } }





        private bool isjump;
        /// <summary>
        /// 达到目标值后是否跳出
        /// </summary>
        public bool IsJump
        { get { return isjump; } set { isjump = value; RaisePropertyChanged(); } }




        private bool isprint;
        /// <summary>
        /// 是否打印耦合数据
        /// </summary>
        public bool IsPrint
        { get { return isprint; } set { isprint = value; RaisePropertyChanged(); } }



        private bool iscorrection;
        /// <summary>
        /// 是否为矫正耦合
        /// </summary>
        public bool IsCorrection
        { get { return iscorrection; } set { iscorrection = value; RaisePropertyChanged(); } }



        /// <summary>
        /// 矫正耦合通道
        /// </summary>
        public CorrectChannel CorrectChannel { get; set; }




        private string axisname;
        /// <summary>
        /// 耦合轴名称
        /// </summary>
        public string AxisName
        { get { return axisname; } set { axisname = value; RaisePropertyChanged(); } }




        private bool issavedata;
        /// <summary>
        /// 是否保存数据
        /// </summary>
        public bool IsSaveData
        { get { return issavedata; } set { issavedata = value; RaisePropertyChanged(); } }



        private string dataremark;
        /// <summary>
        /// 数据备注
        /// </summary>
        public string DataRemark
        { get { return dataremark; } set { dataremark = value; RaisePropertyChanged(); } }



        private bool enable;
        /// <summary>
        ///启动耦合
        /// </summary>
        public bool Enable
        { get { return enable; } set { enable = value; RaisePropertyChanged(); } }


        private int _channel;
        public int Channel { get { return _channel; } set { _channel = value; RaisePropertyChanged(); } }



        private int _axisvelocity;
        public int AxisVel
        {
            get { return _axisvelocity; }
            set
            {
                if (value > 5)
                {
                    _axisvelocity = 5;
                }
                else
                {
                    _axisvelocity = value;
                }

                RaisePropertyChanged();
            }
        }



        private int _datadealy;
        public int DataDelay { get { return _datadealy; } set { _datadealy = value; RaisePropertyChanged(); } }
    }



    public enum CorrectChannel
    {
        Channle_1, Channle_8
    }

    public enum AxisGroup
    {
        Left,Right
    }


    public enum ProductType
    {
        Type1, Type2, Type3, Type4, Type5, Type6
    }
}
