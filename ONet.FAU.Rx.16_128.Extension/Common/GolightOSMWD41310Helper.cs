using DM.Foundation.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Common
{
    public  class GolightOSMWD41310Helper : IDisposable
    {
        private  SerialPort _serialPort;
        private readonly object _lock = new object();
        private ILogger _logger;
        public bool IsOpen => _serialPort != null && _serialPort.IsOpen;
        private uint GetCommand(string cmdStr) => BitConverter.ToUInt32(Encoding.ASCII.GetBytes(cmdStr), 0);
        public GolightOSMWD41310Helper(ILogger logger)
        {
            _logger= logger;
        }

        public bool Open(string portName)
        {
            try
            {
                if (IsOpen) return true;
                // 建议：在新建实例前先检查旧实例，防止重复创建导致句柄泄露
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen) _serialPort.Close();
                    _serialPort.Dispose();
                }

                _serialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
                _serialPort.ReadTimeout = 2000;
                _serialPort.WriteTimeout = 2000;


                lock (_lock) { if (!_serialPort.IsOpen) _serialPort.Open(); }


                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return false;
              
            }
           
        }

        /// <summary>
        /// 通用发送与接收逻辑 (处理二进制协议封装)
        /// </summary>
        private byte[] SendAndReceive(string cmdStr, byte[] extraData = null)
        {
            //lock (_lock)
            //{
            //    if (_serialPort == null || !_serialPort.IsOpen) throw new Exception("Port not open.");

            //    // 1. 发送 (直接利用 BuildPacket 以保持逻辑统一)
            //    byte[] sendBuf = BuildPacket(commandWord, extraData);
            //    _serialPort.Write(sendBuf, 0, sendBuf.Length);

            //    // 2. 接收头
            //    int head = _serialPort.ReadByte();
            //    if (head != 0xAA)
            //    {
            //        _serialPort.DiscardInBuffer(); // 清理残留数据
            //        throw new Exception("Invalid response header.");
            //    }

            //    // 3. 接收长度
            //    byte[] lenBytes = new byte[2];
            //    _serialPort.Read(lenBytes, 0, 2);
            //    ushort length = BitConverter.ToUInt16(lenBytes, 0);

            //    // 4. 接收内容 (命令字+数据+校验)
            //    byte[] rest = new byte[length];
            //    int offset = 0;
            //    while (offset < length)
            //    {
            //        int read = _serialPort.Read(rest, offset, length - offset);
            //        offset += read;
            //    }

            //    // 5. 校验 (使用统一的 CalculateChecksum)
            //    byte receivedCS = rest[length - 1];
            //    byte calculatedCS = CalculateChecksum((byte)0xAA, lenBytes, rest.Take(length - 1).ToArray());

            //    if (receivedCS != calculatedCS) throw new Exception("Checksum error.");

            //    return rest; // rest[0-3]是命令字, rest[4...]是数据
            //}


            uint commandWord = GetCommand(cmdStr); // 自动处理 "RDCC" -> 0x43434452 的转换
            lock (_lock)
            {
                // 1. 构建包 (包头 + 长度 + 命令 + 数据)
                List<byte> fullPacket = new List<byte>();
                fullPacket.Add((byte)0xAA);

                byte[] data = extraData ?? new byte[0];
                ushort lenField = (ushort)(4 + data.Length + 1); // 4字节命令 + N字节数据 + 1字节校验 [cite: 10]
                fullPacket.AddRange(BitConverter.GetBytes(lenField));
                fullPacket.AddRange(BitConverter.GetBytes(commandWord));
                fullPacket.AddRange(data);

                // 2. 计算校验和并发送
                byte cs = (byte)(fullPacket.Sum(b => (int)b) & 0xFF);
                fullPacket.Add(cs);

                _serialPort.DiscardInBuffer();
                _serialPort.Write(fullPacket.ToArray(), 0, fullPacket.Count);

                // 3. 接收响应
                if (_serialPort.ReadByte() != 0xAA) throw new Exception("头校验错误");

                byte[] lenBytes = new byte[2];
                _serialPort.Read(lenBytes, 0, 2);
                ushort length = BitConverter.ToUInt16(lenBytes, 0);

                byte[] rest = new byte[length];
                int readCount = 0;
                while (readCount < length)
                {
                    readCount += _serialPort.Read(rest, readCount, length - readCount);
                }

                return rest; // 返回结果：[0-3]是命令回显，[4...]是真正的数据
            }
        }

        /// <summary> 读取通道个数 </summary>
        public int GetChannelCount()
        {
            var res = SendAndReceive("RDCC"); // 对应手册 10. 读取通道个数
            return res[4];
        }

        /// <summary> 设置通道开关 </summary>
        public void SetChannelSwitch(byte channel, bool on)
        {
            byte status = (byte)(on ? 1 : 0); 
            SendAndReceive("STSW", new byte[] { channel, status }); // 对应手册 12. 设置通道开关状态
        }

        /// <summary> 读取当前功率 (单位: mW) </summary>
        public float GetPower(byte channel)
        {
            // 上位机发送: AA 06 00 52 44 50 52 [通道序号] [校验和] 
            var res = SendAndReceive("RDPR", new byte[] { channel });

            // 下位机返回: [RDPR命令字4字节] [通道1字节] [功率4字节] [校验1字节]
            // 我们只需要提取功率部分 (索引 5 开始的 4 个字节) 
            return BitConverter.ToSingle(res, 5);
        }

        /// <summary> 
        /// 读取通道当前的波长值 (对应手册：10. 读取通道波长) 
        /// </summary>
        /// <param name="channel">通道序号 (1~通道个数)</param>
        /// <returns>波长值 (单位: nm)</returns>
        public float GetWavelength(byte channel)
        {
            // 发送 "RDWL" 指令 (十六进制对应 52 44 57 4C)
            // 根据手册 10.1：上位机发送 AA 06 00 52 44 57 4C [通道序号] [校验和]
            var res = SendAndReceive("RDWL", new byte[] { channel });

            // 下位机返回：[RDWL 4字节] [通道 1字节] [波长 4字节] [校验 1字节]
            // 波长值同样是从索引 5 开始的 4 字节浮点数 (Intel Little-Endian)
            return BitConverter.ToSingle(res, 5);
        }

        public void Dispose()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen) _serialPort.Close();
                _serialPort.Dispose();
            }
        }


    }
}
