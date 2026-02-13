using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Model
{
    public class PowerMeterChannelModel : BindableBase
    {
        private double _rawPowerValue; // 原始功率值 (单位: W)
        private string _wavelength;       // 当前波长 (单位: nm)
        private string _currentUnit = "dBm";

        public string ChannelName { get; set; }

        // 当前波长
        public string Wavelength
        {
            get => _wavelength;
            set => SetProperty(ref _wavelength, value);
        }

        // 当前单位
        public string CurrentUnit
        {
            get => _currentUnit;
            set => SetProperty(ref _currentUnit, value, () => RaisePropertyChanged(nameof(DisplayReadout)));
        }

        // 原始功率值（从硬件驱动获取后赋值）
        public double RawPowerValue
        {
            get => _rawPowerValue;
            set => SetProperty(ref _rawPowerValue, value, () => RaisePropertyChanged(nameof(DisplayReadout)));
        }

        // 最终显示的读数字符串
        public string DisplayReadout
        {
            get
            {
                if (_rawPowerValue <= 0 && _currentUnit == "dBm") return "---";

                switch (_currentUnit)
                {
                    case "dBm":
                        // P(dBm) = 10 * log10(P(W) * 1000)
                        return (10 * Math.Log10(_rawPowerValue * 1000)).ToString("F2");
                    case "uW":
                        return (_rawPowerValue * 1000000).ToString("F2");
                    case "mW":
                        return (_rawPowerValue * 1000).ToString("F3");
                    default:
                        return _rawPowerValue.ToString("E2"); // 科学计数法备用
                }
            }
        }


    }
}
