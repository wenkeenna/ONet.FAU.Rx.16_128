using DM.Foundation.Shared.Enums;
using DM.Foundation.Shared.Interfaces;
using DM.Foundation.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Initialization
{
    public class ParameterInitializer : IParameterInitializer
    {
        ObservableCollection<ParameterModel> IParameterInitializer.InitDeviceParameters()
        {
            return new ObservableCollection<ParameterModel>
                {
                    new ParameterModel
                    {
                        Name = "[龙门]复位位置",
                        IsRoot = true,
                        Children = new ObservableCollection<ParameterModel>
                        {
                            ParameterModel.Create("[龙门]复位位置","Gx", "0.00", ParameterType.Double),
                            ParameterModel.Create("[龙门]复位位置","Gy", "0.00", ParameterType.Double),
                            ParameterModel.Create("[龙门]复位位置","Cam", "0.00", ParameterType.Double),
                            ParameterModel.Create("[龙门]复位位置","Glue", "0.00", ParameterType.Double),
                        }
                    },



                    new ParameterModel
                    {
                        Name = "[左轴组]复位位置",
                        IsRoot = true,
                        Children = new ObservableCollection<ParameterModel>
                        {
                            ParameterModel.Create("[左轴组]复位位置","X", "0.00", ParameterType.Double),
                            ParameterModel.Create("[左轴组]复位位置","Y", "0.00", ParameterType.Double),
                            ParameterModel.Create("[左轴组]复位位置","Z", "0.00", ParameterType.Double),
                            ParameterModel.Create("[左轴组]复位位置","RX", "0.00", ParameterType.Double),
                            ParameterModel.Create("[左轴组]复位位置","RY", "0.00", ParameterType.Double),
                            ParameterModel.Create("[左轴组]复位位置","RZ", "0.00", ParameterType.Double),
                        }
                    },



                   new ParameterModel
                    {
                        Name = "[右轴组]复位位置",
                        IsRoot = true,
                        Children = new ObservableCollection<ParameterModel>
                        {
                            ParameterModel.Create("[右轴组]复位位置","X", "0.00", ParameterType.Double),
                            ParameterModel.Create("[右轴组]复位位置","Y", "0.00", ParameterType.Double),
                            ParameterModel.Create("[右轴组]复位位置","Z", "0.00", ParameterType.Double),
                            ParameterModel.Create("[右轴组]复位位置","RX", "0.00", ParameterType.Double),
                            ParameterModel.Create("[右轴组]复位位置","RY", "0.00", ParameterType.Double),
                            ParameterModel.Create("[右轴组]复位位置","RZ", "0.00", ParameterType.Double),
                        }
                    },


                    new ParameterModel
                    {
                        Name = "三轴压力传感器",
                        IsRoot = true,
                        IsAddDataBind=false,
                        Children = new ObservableCollection<ParameterModel>
                        {
                            ParameterModel.Create("三轴压力传感器","报警阈值", "0.00", ParameterType.Double),
                        }
                    },


               



                };
        }

        ObservableCollection<ParameterModel> IParameterInitializer.InitProductParameters()
        {
            return new ObservableCollection<ParameterModel>
                {
                    new ParameterModel
                    {
                        Name = "[左]托盘取料补偿",
                        IsRoot = true,
                        Children = new ObservableCollection<ParameterModel>
                        {
                            ParameterModel.Create("[左]托盘取料补偿","X", "0", ParameterType.Double),
                            ParameterModel.Create("[左]托盘取料补偿","Y", "0", ParameterType.Double),

                        }
                    },
                    new ParameterModel
                    {
                        Name = "[右]托盘取料补偿",
                        IsRoot = true,
                        Children = new ObservableCollection<ParameterModel>
                        {
                            ParameterModel.Create("[右]托盘取料补偿","X", "0", ParameterType.Double),
                            ParameterModel.Create("[右]托盘取料补偿","Y", "0", ParameterType.Double),
                        }
                    },
                 
                       new ParameterModel
                    {
                        Name = "Lens拍照位置",
                        IsRoot = true,
                        Children = new ObservableCollection<ParameterModel>
                        {
                            ParameterModel.Create("Lens拍照位置","Left_X", "0", ParameterType.Double),
                            ParameterModel.Create("Lens拍照位置","Left_Y", "0", ParameterType.Double),

                            ParameterModel.Create("Lens拍照位置","Right_X", "0", ParameterType.Double),
                            ParameterModel.Create("Lens拍照位置","Right_Y", "0", ParameterType.Double),


                        }
                    },

                    new ParameterModel
                    {
                        Name = "Lens定位补偿",
                        IsRoot = true,
                        Children = new ObservableCollection<ParameterModel>
                        {
                            ParameterModel.Create("Lens定位补偿","Left_X", "0", ParameterType.Double),
                            ParameterModel.Create("Lens定位补偿","Left_Y", "0", ParameterType.Double),

                            ParameterModel.Create("Lens定位补偿","Right_X", "0", ParameterType.Double),
                            ParameterModel.Create("Lens定位补偿","Right_Y", "0", ParameterType.Double),
                        }
                    },

                            new ParameterModel
                    {
                        Name = "点胶校准",
                        IsRoot = true,
                        IsAddDataBind=false,
                        Children = new ObservableCollection<ParameterModel>
                        {
                             ParameterModel.Create("点胶校准","BaseX", "0.00", ParameterType.Double),
                             ParameterModel.Create("点胶校准","BaseY", "0.00", ParameterType.Double),
                             ParameterModel.Create("点胶校准","BaseZ", "0.00", ParameterType.Double),

                             ParameterModel.Create("点胶校准","NewX", "0.00", ParameterType.Double),
                             ParameterModel.Create("点胶校准","NewY", "0.00", ParameterType.Double),
                             ParameterModel.Create("点胶校准","NewZ", "0.00", ParameterType.Double),
                        }
                    },


                       new ParameterModel
                    {
                        Name = "仪表端口号",
                        IsRoot = true,
                            IsAddDataBind=true,
                        Children = new ObservableCollection<ParameterModel>
                        {
                             ParameterModel.Create("仪表端口号","UVLight", "COM1", ParameterType.String),

                             ParameterModel.Create("仪表端口号","DC65", "COM3", ParameterType.String),

                             ParameterModel.Create("仪表端口号","M8811", "COM4", ParameterType.String),

                             ParameterModel.Create("仪表端口号","光功率计", "COM2", ParameterType.String),
                             ParameterModel.Create("仪表端口号","光功率计_B", "COM6", ParameterType.String),

                             ParameterModel.Create("仪表端口号","激光光源", "COM2", ParameterType.String),

                        }
                    },

                            new ParameterModel
                    {
                        Name = "料盘补偿",
                        IsRoot = true,
                        IsAddDataBind=true,
                        Children = new ObservableCollection<ParameterModel>
                        {
                             ParameterModel.Create("料盘补偿","LeftX", "0.0", ParameterType.Double),
                             ParameterModel.Create("料盘补偿","LeftY", "0.0", ParameterType.Double),
                             ParameterModel.Create("料盘补偿","RightX", "0.0", ParameterType.Double),
                             ParameterModel.Create("料盘补偿","RightY", "0.0", ParameterType.Double),
                        }
                    },

                      new ParameterModel
                    {
                        Name = "吸嘴耦合位置补偿",
                        IsRoot = true,
                        IsAddDataBind=true,
                        Children = new ObservableCollection<ParameterModel>
                        {
                             ParameterModel.Create("吸嘴耦合位置补偿","LeftX", "0.0", ParameterType.Double),
                             ParameterModel.Create("吸嘴耦合位置补偿","LeftY", "0.0", ParameterType.Double),
                             ParameterModel.Create("吸嘴耦合位置补偿","RightX", "0.0", ParameterType.Double),
                             ParameterModel.Create("吸嘴耦合位置补偿","RightY", "0.0", ParameterType.Double),
                        }
                    },
                    new ParameterModel
                    {
                        Name = "点胶补偿",
                        IsRoot = true,
                        IsAddDataBind=true,
                        Children = new ObservableCollection<ParameterModel>
                        {
                             ParameterModel.Create("点胶补偿","LeftX", "0.0", ParameterType.Double),
                             ParameterModel.Create("点胶补偿","LeftY", "0.0", ParameterType.Double),
                             ParameterModel.Create("点胶补偿","RightX", "0.0", ParameterType.Double),
                             ParameterModel.Create("点胶补偿","RightY", "0.0", ParameterType.Double),
                        }
                    },


                       new ParameterModel
                    {
                        Name = "仪表扩展参数",
                        IsRoot = true,
                            IsAddDataBind=true,
                        Children = new ObservableCollection<ParameterModel>
                        {
                             ParameterModel.Create("仪表扩展参数","UVLightCount","1", ParameterType.Int),

                        }
                    },





                };
        }
    }
}
