using DM.Foundation.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONet.FAU.Rx._16_128.Initialization
{
    public class GlobalConfigService : IGlobalConfigService
    {
        private const string ConfigFileName = "GlobalConfigPaths.json";
        private Dictionary<string, string> _configPaths;

        public GlobalConfigService()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (File.Exists(ConfigFileName))
            {
                try
                {
                    var json = File.ReadAllText(ConfigFileName);
                    _configPaths = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                }
                catch
                {
                    _configPaths = new Dictionary<string, string>();
                }
            }
            else
            {
                _configPaths = new Dictionary<string, string>();
                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_configPaths, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(ConfigFileName, json);
        }

        public string GetConfigPath(string moduleName)
        {
            if (_configPaths.TryGetValue(moduleName, out var path))
                return path;
            throw new KeyNotFoundException($"未找到 {moduleName} 的配置路径");
        }

        public void SetConfigPath(string moduleName, string path)
        {
            _configPaths[moduleName] = path;
            SaveConfig();
        }

        public IReadOnlyDictionary<string, string> GetAllConfigPaths() => _configPaths;
    }
}
