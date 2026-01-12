using DM.Foundation.Shared.Enums;
using DM.Foundation.Shared.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Services
{
    public class RuntimeContext : BindableBase, IRuntimeContext
    {
        public RuntimeContext()
        {

        }


        private string _currentrecipename;
        /// <summary>
        /// 当前加载的方案
        /// </summary>
        public string CurrentRecipeName
        {
            get { return _currentrecipename; }
            set { _currentrecipename = value; RaisePropertyChanged(); }
        }


        private string _currentprodictcode;
        /// <summary>
        /// 当前产品型号
        /// </summary>
        public string CurrentProductCode
        {
            get { return _currentprodictcode; }
            set { _currentprodictcode = value; RaisePropertyChanged(); }
        }

        private string _currentuser;
        /// <summary>
        ///  当前操作用户
        /// </summary>
        public string CurrentUser
        {
            get { return _currentuser; }
            set { _currentuser = value; RaisePropertyChanged(); }
        }


        private string _currentbatchid;
        /// <summary>
        /// 当前生产批次号
        /// </summary>
        public string CurrentBatchId
        {
            get { return _currentbatchid; }
            set { _currentbatchid = value; RaisePropertyChanged(); }
        }


        private int _totalproduct;
        /// <summary>
        /// 当前产量
        /// </summary>
        public int TotalProduced
        {
            get { return _totalproduct; }
            set { _totalproduct = value; RaisePropertyChanged(); }
        }




        private bool _pressurealarm;
        /// <summary>
        /// 压力报警
        /// </summary>
        public bool PressureAlarm
        {
            get { return _pressurealarm; }
            set { _pressurealarm = value; RaisePropertyChanged(); }
        }




        private DeviceStatus _devicestatu;
        /// <summary>
        /// 设备状态
        /// </summary>
        public DeviceStatus DeviceStatu
        {
            get { return _devicestatu; }
            set { _devicestatu = value; RaisePropertyChanged(); }
        }
    }
}
