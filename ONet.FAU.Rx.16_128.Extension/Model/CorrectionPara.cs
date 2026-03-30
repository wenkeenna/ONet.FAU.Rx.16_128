using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Model
{
    public class CorrectionPara : BindableBase
    {
        private double x;
        public double X { get { return x; } set { x = value; RaisePropertyChanged(); } }

        private double y;
        public double Y { get { return y; } set { y = value; RaisePropertyChanged(); } }

        private double z;
        public double Z { get { return z; } set { z = value; RaisePropertyChanged(); } }


        private string xstr;
        public string X_Str { get { return xstr; } set { xstr = value; RaisePropertyChanged(); } }

        private string ystr;
        public string Y_Str { get { return ystr; } set { ystr = value; RaisePropertyChanged(); } }





        private bool enable;
        public bool Enable { get { return enable; } set { enable = value; RaisePropertyChanged(); } }

        private int _lightvalue;
        public int LightValue
        {
            get { return _lightvalue; }
            set { _lightvalue = value; RaisePropertyChanged(); }
        }

        private string visionName;
        public string VisioName { get { return visionName; } set { visionName = value; RaisePropertyChanged(); } }
    }
}
