using DM.Foundation.Logging.Interfaces;
using DM.Foundation.Shared.Events;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Common
{

    public class OpticalModuleService
    {
        private readonly object _lock = new object();
        private SerialPort _serialPort;
        private bool _disposed;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        //private byte? _currentPage = null;

        private const byte CMD_TITLE = 0x55;
        private const byte CMD_WRITE = 0xA0;
        private const byte CMD_READ = 0xA1;
        private const byte REG_PAGE_SELECT = 0x7F;

        public OpticalModuleService(IEventAggregator eventAggregator, ILogger logger)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public bool Open(string portName, int baudRate = 115200)
        {
            lock (_lock)
            {
                try
                {
                    if (_serialPort != null && _serialPort.IsOpen) return true;

                    _serialPort = new SerialPort(portName, baudRate)
                    {
                        Parity = Parity.None,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        Handshake = Handshake.None,
                        ReadTimeout = 600,
                        WriteTimeout = 600
                    };

                    _serialPort.Open();
                    //_currentPage = null;
                    return true;
                }
                catch (Exception ex)
                {
                    PublishMessage($"打开串口 {portName} 失败: {ex.Message}");
                    return false;
                }
            }
        }

        public bool Close()
        {
            lock (_lock)
            {
                try
                {
                    _serialPort?.Close();
                    _serialPort?.Dispose();
                    _serialPort = null;
                    //_currentPage = null;
                    return true;
                }
                catch (Exception ex)
                {
                    PublishMessage($"关闭串口失败: {ex.Message}");
                    return false;
                }
            }
        }

        private async Task<byte[]> SendAndReceiveAsync(byte[] command, int timeoutMs = 800)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (_serialPort == null || !_serialPort.IsOpen)
                        return null;

                    try
                    {
                        _serialPort.DiscardInBuffer();
                        _serialPort.DiscardOutBuffer();
                        _serialPort.Write(command, 0, command.Length);


                        //PublishMessage("发送命令: " + BitConverter.ToString(command));

                        // 给设备反应时间（根据实际测试调整这个值）
                        const int responseDelayMs = 10;
                        Thread.Sleep(responseDelayMs);

                        var startTime = DateTime.UtcNow;
                        var timeout = TimeSpan.FromMilliseconds(timeoutMs);

                        while (DateTime.UtcNow - startTime < timeout)
                        {
                            if (_serialPort.BytesToRead > 0)
                            {
                                byte[] buffer = new byte[_serialPort.BytesToRead];
                                _serialPort.Read(buffer, 0, buffer.Length);

                                //PublishMessage("返回数据: " + BitConverter.ToString(buffer));
                                return buffer;
                            }

                            Thread.Sleep(5);  // 轮询间隔稍大一点，降低 CPU 占用
                        }

                        PublishMessage("超时，命令: " + BitConverter.ToString(command));
                        return null;
                    }
                    catch (Exception ex)
                    {
                        PublishMessage("通信异常: " + ex.Message);
                        return null;
                    }
                }
            });


        }



        private async Task<bool> SwitchPageAsync(byte page)
        {
            var cmd = new byte[] { CMD_TITLE, CMD_WRITE, REG_PAGE_SELECT, 0x01, page };

            var response = await SendAndReceiveAsync(cmd, 800);

            if (response == null || response.Length != 1 || response[0] != 1)
            {
                string respStr = response != null ? BitConverter.ToString(response) : "无响应";

                _logger?.Warn($"切换页面到 0x{page:X2} 失败，响应: {respStr}");

                return false;
            }

            await Task.Delay(10);   // 简单延时，不再支持取消

            return true;
        }

        public async Task<bool> WriteRegisterAsync(byte regAddr, ushort value)
        {

            byte msb = (byte)(value >> 8);
            byte lsb = (byte)(value & 0xFF);
            var cmd = new byte[] { CMD_TITLE, CMD_WRITE, regAddr, 0x02, msb, lsb };

            //_logger?.Info($"WriteRegister → Reg:0x{regAddr:X2}, Value:0x{value:X4}");

            var response = await SendAndReceiveAsync(cmd, 800);
            return response != null;
        }

        public async Task<bool> WriteBytesAsync(byte startReg, byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                _logger?.Error("WriteBytesAsync: data 不能为空");
                return false;
            }
            if (data.Length > 250)
            {
                _logger?.Error($"WriteBytesAsync: 数据过长 {data.Length} 字节");
                return false;
            }

            //if (!await EnsurePageAsync(page))
            //    return false;

            var cmd = new byte[4 + data.Length];
            cmd[0] = CMD_TITLE;
            cmd[1] = CMD_WRITE;
            cmd[2] = startReg;
            cmd[3] = (byte)data.Length;
            Buffer.BlockCopy(data, 0, cmd, 4, data.Length);

            //_logger?.Info($"WriteBytes → Reg:0x{startReg:X2}, Len:{data.Length}, Data:{BitConverter.ToString(data)}");

            var response = await SendAndReceiveAsync(cmd, 800);
            return response != null;
        }



        public async Task<byte[]> ReadRegistersAsync(byte startReg, byte length)
        {


            var cmd = new byte[] { CMD_TITLE, CMD_READ, startReg, length };
            //_logger?.Info($"Read →  Reg:0x{startReg:X2}, Len:{length}");

            return await SendAndReceiveAsync(cmd, 800);
        }

        public async Task<bool> SetLaserStateAsync(int channel)
        {
            try
            {
                byte groupValue;

                switch (channel)
                {
                    case 1:
                        groupValue = 0x11;
                        break;
                    case 2:
                        groupValue = 0x22;
                        break;
                    case 3:
                        groupValue = 0x33;
                        break;
                    case 4:
                        groupValue = 0x44;
                        break;
                    case 5:
                        groupValue = 0x45;
                        break;
                    default:
                        groupValue = 0;
                        break;
                }

                if (groupValue == 0)
                {
                    _logger?.Error($"无效通道: {channel}");
                    PublishMessage($"SetLaserStateAsync 无效通道: {channel}");
                    return false;
                }

                //await SendAndReceiveAsync(new byte[] { CMD_TITLE, CMD_WRITE, 0x7F, 0x01, 0x80 }, 800);
                //await Task.Delay(50);

                await SwitchPageAsync(0x00);

                await SendAndReceiveAsync(new byte[] { CMD_TITLE, CMD_WRITE, 0x7F, 0x01, 0x80 }, 800);
                await Task.Delay(100);

                await SendAndReceiveAsync(new byte[] { CMD_TITLE, CMD_WRITE, 0xE6, 0x01, groupValue }, 800);
                await Task.Delay(50);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error($"SetLaserStateAsync 异常: {ex}");
                PublishMessage($"SetLaserStateAsync {ex}");
                return false;
            }
        }

        private int LASER_GROUP_1=1, LASER_GROUP_2=2, LASER_GROUP_3=3, LASER_GROUP_4=4;
        public async Task<bool> SetLaserStateAsync(int channelA,int channelB)
        {
            try
            {
                byte groupValue;

                //switch (channelA)
                //{
                //    case 1:
                //        groupValue = 0x11;
                //        break;
                //    case 2:
                //        groupValue = 0x22;
                //        break;
                //    case 3:
                //        groupValue = 0x33;
                //        break;
                //    case 4:
                //        groupValue = 0x44;
                //        break;
                //    case 5:
                //        groupValue = 0x45;
                //        break;
                //    default:
                //        groupValue = 0;
                //        break;
                //}

                if (channelA == LASER_GROUP_1 && channelB == LASER_GROUP_2)
                {
                    groupValue = 0X12;
                }
                else if (channelA == LASER_GROUP_1 && channelB == LASER_GROUP_3)
                {
                    groupValue = 0X13;
                }
                else if (channelA == LASER_GROUP_1 && channelB == LASER_GROUP_4)
                {
                    groupValue = 0X14;
                }
                else if (channelA == LASER_GROUP_2 && channelB == LASER_GROUP_3)
                {
                    groupValue = 0X23;
                }
                else if (channelA == LASER_GROUP_2 && channelB == LASER_GROUP_4)
                {
                    groupValue = 0X24;
                }
                else if (channelA == LASER_GROUP_3 && channelB == LASER_GROUP_4)
                {
                    groupValue = 0X34;
                }
                else
                {
                    groupValue = 0;
                }


                if (groupValue == 0)
                {
                    _logger?.Error($"无效通道: {channelA}");
                    PublishMessage($"SetLaserStateAsync 无效通道: {channelA}");
                    return false;
                }


                await SwitchPageAsync(0x00);

                await SendAndReceiveAsync(new byte[] { CMD_TITLE, CMD_WRITE, 0x7F, 0x01, 0x80 }, 800);
                await Task.Delay(100);

                await SendAndReceiveAsync(new byte[] { CMD_TITLE, CMD_WRITE, 0xE6, 0x01, groupValue }, 800);
                await Task.Delay(50);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error($"SetLaserStateAsync 异常: {ex}");
                PublishMessage($"SetLaserStateAsync {ex}");
                return false;
            }
        }
        public async Task<bool> SetPawssword()
        {
            //var success = await WriteBytesAsync(0x00, 0x7A, new byte[] { 0x84, 0x85, 0x86, 0x87 });
            await SwitchPageAsync(0x00);

            var success = await WriteBytesAsync(0x7A, new byte[] { 0x84, 0x85, 0x86, 0x87 });
            if (!success)
            {
                PublishMessage("SetPawssword 失败");
            }
            return success;
        }

        public async Task<(ushort vcc, ushort temp)> ReadVccTemp(int group)
        {
            try
            {
                await SwitchPageAsync(0x00);
                //var rawBytes = await ReadRegistersAsync(0x00, 0x0E, 0x04);
                byte send;

                if (group == 1)
                {
                    send = 0x0E;
                }
                else if (group == 2)
                {
                    send = 0x12;
                }
                else if (group == 3)
                {
                    send = 0x14;
                }
                else
                {
                    send = 0x16;
                }


                var rawBytes = await ReadRegistersAsync(send, 0x04);
                if (rawBytes == null || rawBytes.Length != 4)
                    return (0, 0);


                //_logger.Error( $"电压温度数据:{BitConverter.ToString(rawBytes)}");

                ushort temp = (ushort)((rawBytes[0] << 8) | rawBytes[1]);
                ushort vcc = (ushort)((rawBytes[2] << 8) | rawBytes[3]);

                return (vcc, temp);
            }
            catch
            {
                return (0, 0);
            }
        }

        public async Task<bool> IsOnline()
        {
            try
            {
                byte[] command = new byte[] { 0x58, 0xA0 };
                byte[] data = await SendAndReceiveAsync(command, 800);
                return data != null && data.Length > 0;
            }
            catch (Exception ex)
            {
                PublishMessage($"IsOnline 失败: {ex.Message}");
                return false;
            }
        }

        // ──────────────────────────────────────────────
        // 以下方法已去除 CancellationToken
        // ──────────────────────────────────────────────

        public async Task<ushort[]> ReadMPDoAsync(int startChannel, int count, int timeoutMs = 800)
        {
            if (startChannel < 0 || count <= 0 || startChannel + count > 32)
            {
                PublishMessage($"ReadMPDoAsync 参数无效: start={startChannel}, count={count}");
                return null;
            }

            byte page;
            byte baseReg = 0xB0;
            int localChannel = startChannel;

            if (startChannel <= 7) page = 0x8A;
            else if (startChannel <= 15) { page = 0xC1; localChannel -= 8; }
            else if (startChannel <= 23) { page = 0xC2; localChannel -= 16; }
            else { page = 0xC3; localChannel -= 24; }

            byte startReg = (byte)(baseReg + localChannel * 2);
            byte length = (byte)(count * 2);

            await SwitchPageAsync(page);
            //var raw = await ReadRegistersAsync(page, startReg, length);
            var raw = await ReadRegistersAsync(startReg, length);
            if (raw == null || raw.Length < length)
            {
                PublishMessage($"ReadMPDoAsync 读取不足: 预期 {length} 字节，实际 {raw?.Length ?? 0}");
                return null;
            }

            var values = new ushort[count];
            for (int i = 0; i < count; i++)
                values[i] = (ushort)((raw[i * 2] << 8) | raw[i * 2 + 1]);

            return values;
        }

        public async Task<ushort[]> ReadMPDiAsync(int startChannel, int count, int timeoutMs = 800)
        {
            if (startChannel < 0 || count <= 0 || startChannel + count > 32)
            {
                PublishMessage($"ReadMPDiAsync 参数无效: start={startChannel}, count={count}");
                return null;
            }

            byte page;
            byte baseReg = 0xC0;
            int localChannel = startChannel;

            if (startChannel <= 7) page = 0x8A;
            else if (startChannel <= 15) { page = 0xC1; localChannel -= 8; }
            else if (startChannel <= 23) { page = 0xC2; localChannel -= 16; }
            else { page = 0xC3; localChannel -= 24; }

            byte startReg = (byte)(baseReg + localChannel * 2);
            byte length = (byte)(count * 2);

            await SwitchPageAsync(page);
            //var raw = await ReadRegistersAsync(page, startReg, length);
            var raw = await ReadRegistersAsync(startReg, length);
            if (raw == null || raw.Length < length)
            {
                PublishMessage($"ReadMPDiAsync 读取不足: 预期 {length} 字节，实际 {raw?.Length ?? 0}");
                return null;
            }

            var values = new ushort[count];
            for (int i = 0; i < count; i++)
                values[i] = (ushort)((raw[i * 2] << 8) | raw[i * 2 + 1]);

            return values;
        }

        public async Task<ushort[]> ReadRSSIAsync(int startChannel, int count, int timeoutMs = 800)
        {
            if (startChannel < 0 || count <= 0 || startChannel + count > 32)
            {
                PublishMessage($"ReadRSSIAsync 参数无效: start={startChannel}, count={count}");
                return null;
            }

            byte page;
            byte baseReg = 0x8C;
            int localChannel = startChannel;

            if (startChannel <= 7) page = 0x8A;
            else if (startChannel <= 15) { page = 0xC1; localChannel -= 8; }
            else if (startChannel <= 23) { page = 0xC2; localChannel -= 16; }
            else { page = 0xC3; localChannel -= 24; }

            byte startReg = (byte)(baseReg + localChannel * 2);
            byte length = (byte)(count * 2);

            await SwitchPageAsync(page);
            // var raw = await ReadRegistersAsync(page, startReg, length);
            var raw = await ReadRegistersAsync(startReg, length);
            if (raw == null || raw.Length < length)
            {
                PublishMessage($"ReadRSSIAsync 读取不足: 预期 {length} 字节，实际 {raw?.Length ?? 0}");
                return null;
            }

            var values = new ushort[count];
            for (int i = 0; i < count; i++)
                values[i] = (ushort)((raw[i * 2] << 8) | raw[i * 2 + 1]);

            return values;
        }

        public async Task<ushort[]> ReadIMPDAsync(int count, int timeoutMs = 800)
        {
            if (count <= 0 || count > 16)
            {
                PublishMessage($"ReadIMPDAsync 参数无效: count={count}");
                return null;
            }

            byte page = 0x8A;
            byte startReg = 0xE8;
            byte length = (byte)(count * 2);

            // var raw = await ReadRegistersAsync(page, startReg, length);

            await SwitchPageAsync(page);

            var raw = await ReadRegistersAsync(startReg, length);
            if (raw == null || raw.Length < length)
            {
                PublishMessage($"ReadIMPDAsync 读取不足: 预期 {length} 字节，实际 {raw?.Length ?? 0}");
                return null;
            }

            var values = new ushort[count];
            for (int i = 0; i < count; i++)
                values[i] = (ushort)((raw[i * 2] << 8) | raw[i * 2 + 1]);

            return values;
        }

        public async Task<bool> SetHeaterRamAsync(int heaterIndex, ushort value, int timeoutMs = 800)
        {
            if (heaterIndex < 0 || heaterIndex > 31)
            {
                _logger?.Error($"SetHeaterRamAsync 无效索引: {heaterIndex}");
                return false;
            }

            byte page = (heaterIndex <= 15) ? (byte)0x8C : (byte)0x8D;
            int baseReg = (heaterIndex % 16 < 8) ? 0x90 : 0xB8;
            int offset = (heaterIndex % 8) * 2;
            byte regAddr = (byte)(baseReg + offset);

            await SwitchPageAsync(page);

            await Task.Delay(100);

            return await WriteRegisterAsync(regAddr, value);
        }

        private void PublishMessage(string msg)
        {
            _eventAggregator?.GetEvent<Event_Message>()?.Publish(msg);
            _logger?.Info(msg);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Close();
            _logger?.Info("OpticalModuleService 已释放");
        }
    }


   

}
