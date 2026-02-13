using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ONet.FAU.Rx._16_128.Extension.Model
{
    public class LaserChannelModel : BindableBase
    {
        private double _actualPower;
        private double _setPower;
        private bool _isActive;
        private string _channelName;

        public string ChannelName
        {
            get => _channelName;
            set => SetProperty(ref _channelName, value);
        }

        public double ActualPower
        {
            get => _actualPower;
            set => SetProperty(ref _actualPower, value);
        }

        public double SetPower
        {
            get => _setPower;
            set => SetProperty(ref _setPower, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (SetProperty(ref _isActive, value))
                {
                    RaisePropertyChanged(nameof(StatusColor));
                    RaisePropertyChanged(nameof(ButtonText));
                }
            }
        }

        // 逻辑属性：根据开关状态返回颜色（开启为红，警告激光发射）
        public Brush StatusColor => IsActive ? Brushes.Red : Brushes.Gray;
        public string ButtonText => IsActive ? "关闭" : "开启";
    }
}
