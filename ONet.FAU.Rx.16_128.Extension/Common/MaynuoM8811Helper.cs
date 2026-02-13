using DM.Foundation.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Extension.Common
{
    public class MaynuoM8811Helper : IDisposable
    {
        private SerialPort _serialPort;
        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);
        private bool _isDisposed;
        private readonly ILogger _logger;

        public bool IsOpen => _serialPort != null && _serialPort.IsOpen;

        public MaynuoM8811Helper(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool Open(string portName, int baudRate = 9600)
        {
            try
            {
                if (IsOpen) return true;

                _serialPort = new SerialPort(portName, baudRate)
                {
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 1500,
                    WriteTimeout = 1500,
                    Encoding = Encoding.ASCII
                };

                _serialPort.Open();
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();

                _logger?.Info($"M8811 串口打开成功: {portName}@{baudRate}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error($"M8811 打开串口失败 {portName}@{baudRate}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 发送命令并读取响应（异步，带超时控制）
        /// </summary>
        /// <param name="command">命令字符串（会自动加 \n）</param>
        /// <param name="timeoutMs">超时毫秒，默认 500ms，查询命令建议 800ms</param>
        /// <returns>响应字符串（已清理），超时或无响应返回 null</returns>
        public async Task<string> Query(string command, int timeoutMs = 500)
        {
            await _syncLock.WaitAsync();
            try
            {
                if (!IsOpen) return null;

                string formattedCmd = command.TrimEnd('\r', '\n') + "\n";
                byte[] bytes = Encoding.ASCII.GetBytes(formattedCmd);

                var buffer = new StringBuilder();
                var cts = new CancellationTokenSource(timeoutMs);
                var token = cts.Token;

                // 发送
                _serialPort.Write(bytes, 0, bytes.Length);
                await Task.Delay(50, token);  // 关键：发送后稍等，确保数据上链 + 设备开始响应

                // 等待响应
                var sw = Stopwatch.StartNew();
                while (!token.IsCancellationRequested)
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        string data = _serialPort.ReadExisting();
                        buffer.Append(data);

                        // 检查是否收到完整行（以 \n 结尾）
                        string current = buffer.ToString();
                        int nlIndex = current.IndexOf('\n');
                        if (nlIndex >= 0)
                        {
                            string response = current.Substring(0, nlIndex);
                            // 剩余部分放回（如果有）
                            buffer.Remove(0, nlIndex + 1);
                            return CleanResponse(response);
                        }
                    }

                    if (sw.ElapsedMilliseconds > timeoutMs + 100)  // 额外缓冲
                        break;

                    await Task.Delay(20, token);
                }

                _logger?.Warn($"Query 超时: {command}");
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Query 异常 - {command}: {ex.Message}");
                return null;
            }
            finally
            {
                _syncLock.Release();
            }
        }

        /// <summary>
        /// 统一清理响应字符串（针对 M8811 实际格式）
        /// </summary>
        private string CleanResponse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "0";

            string s = raw.Replace("\r", "").Replace("\n", "").Trim().TrimStart();

            // 处理前导 '+' 
            if (s.StartsWith("+")) s = s.Substring(1);

            // 处理 "00.0011" → "0.0011"
            if (s.Contains('.') && s.StartsWith("0") && s.Length > 2 && s[1] != '.')
            {
                int dotIndex = s.IndexOf('.');
                string beforeDot = s.Substring(0, dotIndex).TrimStart('0');
                if (string.IsNullOrEmpty(beforeDot)) beforeDot = "0";
                s = beforeDot + s.Substring(dotIndex);
            }

            _logger?.Debug($"Query 返回清理后: '{s}'");
            return s;
        }

        // ──────────────────────────────────────────────
        // 业务方法（全部改用 Query）
        // ──────────────────────────────────────────────

        public async Task SetVoltageAsync(double volt)
        {
            await Query($"VOLT {volt:F4}", 300);
        }

        public async Task SetCurrentLimitAsync(double curr)
        {
            await Query($"CURR {curr:F4}", 300);
        }

        public async Task SetOutputStateAsync(bool isOn)
        {
            await Query(isOn ? "OUTP ON" : "OUTP OFF", 300);
        }

        public async Task<(double Voltage, double Current)> GetMeasureDataAsync()
        {
            try
            {
                string voltResponse = await Query("MEAS:VOLT?", 800);
                await Task.Delay(150);  // 两条测量命令之间延时，防仪器忙不过来

                string currResponse = await Query("MEAS:CURR?", 800);

                double v = 0, c = 0;

                if (!string.IsNullOrEmpty(voltResponse) &&
                    double.TryParse(voltResponse, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                {
                    // 成功
                }
                else
                {
                    _logger?.Warn($"电压解析失败: '{voltResponse ?? "null"}'");
                }

                if (!string.IsNullOrEmpty(currResponse) &&
                    double.TryParse(currResponse, NumberStyles.Any, CultureInfo.InvariantCulture, out c))
                {
                    // 成功
                }
                else
                {
                    _logger?.Warn($"电流解析失败: '{currResponse ?? "null"}'");
                }

                return (v, c);
            }
            catch (Exception ex)
            {
                _logger?.Error($"GetMeasureDataAsync 异常: {ex.Message}");
                return (0, 0);
            }
        }

        public async Task<string> GetIdentityAsync()
        {
            return await Query("*IDN?", 1000);  // *IDN? 响应较长，超时调大
        }

        public void Close() => Dispose();

        public void Dispose()
        {
            if (_isDisposed) return;

            try
            {
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }
            catch { /* 静默 */ }

            _syncLock?.Dispose();
            _isDisposed = true;
        }


    }
}
