using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ONet.FAU.Rx._16_128.Extension.Model
{
    public class ChannelData : BindableBase
    {
        private double _current;
        public double Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        private Brush _statusColor = Brushes.Gray;  // 或你的自訂 Brush/Color
        public Brush StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

      
    }
}
