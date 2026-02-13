using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Common
{
    public class LD9208Controller : IDisposable
    {
        private  SerialPort _serialPort;
        private readonly object _lock = new object(); // 用于线程同步的锁对象
        public bool IsOpen => _serialPort != null && _serialPort.IsOpen;

        public bool Open(string portName)
        {
            try
            {
                if (IsOpen) return true;

                // 根据手册设定：波特率 115200，无校验，8位数据，1位停止位 [cite: 22, 24]
                _serialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
                _serialPort.ReadTimeout = 2000;
                _serialPort.WriteTimeout = 2000;
                // 命令以 \CR\LF (0DH, 0AH) 结束 
                _serialPort.NewLine = "\r\n";


                lock (_lock)
                {
                    if (!_serialPort.IsOpen) _serialPort.Open();
                }


                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        
        }

        /// <summary>
        /// 发送指令并获取返回字符串
        /// </summary>
        private string SendCommand(string command)
        {
            lock (_lock) // 确保同一时间只有一个线程操作串口
            {
                if (!_serialPort.IsOpen) throw new Exception("Serial port is not open.");

                _serialPort.DiscardInBuffer();
                _serialPort.WriteLine(command); // 自动附加 \r\n 

                // 仪器返回以 ">" 结束 
                string response = "";
                DateTime start = DateTime.Now;
                while (!response.Contains(">"))
                {
                    if ((DateTime.Now - start).TotalMilliseconds > _serialPort.ReadTimeout)
                        throw new TimeoutException("LD9208 response timeout.");

                    if (_serialPort.BytesToRead > 0)
                    {
                        response += _serialPort.ReadExisting();
                    }
                    Thread.Sleep(10);
                }
                return response.Replace(">", "").Trim();
            }
        }

        // --- 核心功能方法 ---

        /// <summary>
        /// 查询仪器信息 (*IDN?) [cite: 51, 61]
                    /// </summary>
        public string GetIdentity() => SendCommand("*IDN?");

        /// <summary>
        /// 同时读取所有 8 通道功率值 (dBm) 
        /// </summary>
        public string GetAllPower() => SendCommand("POW?");

        /// <summary>
        /// 读取指定通道的功率值 [cite: 93, 98]
                    /// </summary>
                    /// <param name="channel">通道号 (1-8)</param>
        public string ReadChannelPower(int channel) => SendCommand($"Read {channel}: Pow?");

        /// <summary>
        /// 设置指定通道的波长 [cite: 159, 162]
                    /// </summary>
        public void SetWavelength(int channel, int wavelength) => SendCommand($"Sens {channel}: Pow: Wavelength {wavelength}");
        public string GetWavelength(int channel) => SendCommand($"Sens {channel}: Pow: Wavelength?");
        /// <summary>
        /// 执行无光清零 (注意：需等待约5秒) [cite: 124, 128]
        /// </summary>
        public async Task<bool> ZeroCalibration(int channel)
        {
            SendCommand($"Sens {channel}: Correction: Collect: ZERO");
            await Task.Delay(1000);
            string status = SendCommand($"Sens {channel}: Correction: Collect: ZERO?"); 
            return status.Contains("OK"); 
        }

        /// <summary>
        /// 设置功率单位 (dBm, mW, dB) [cite: 115, 180]
                    /// </summary>
        public void SetUnit(int channel, string unit)=> SendCommand($"Sens {channel}: Pow: Unit {unit}");

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
