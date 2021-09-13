using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactioServer
{
    public class ConfigRegistry
    {
        private FactioServer factioServer;

        private string configRegistryPath => $"{Directory.GetCurrentDirectory()}/config.reg";

        private Dictionary<string, int> intConfigs = new Dictionary<string, int>();
        private Dictionary<string, float> floatConfigs = new Dictionary<string, float>();
        private Dictionary<string, bool> boolConfigs = new Dictionary<string, bool>();
        // May want string configs later

        public ConfigRegistry(FactioServer factioServer)
        {
            this.factioServer = factioServer;
            LoadConfig();
        }

        #region GetMethods
        public int GetIntConfig(string configKey)
        {
            return intConfigs[configKey];
        }
        public float GetFloatConfig(string configKey)
        {
            if (intConfigs.ContainsKey(configKey))
                return intConfigs[configKey];
            return floatConfigs[configKey];
        }
        public bool GetBoolConfig(string configKey)
        {
            return boolConfigs[configKey];
        }
        public bool TryGetIntConfig(string configKey, out int value)
        {
            return intConfigs.TryGetValue(configKey, out value);
        }
        public bool TryGetFloatConfig(string configKey, out float value)
        {
            if (intConfigs.ContainsKey(configKey))
            {
                value = intConfigs[configKey];
                return true;
            }
            return floatConfigs.TryGetValue(configKey, out value);
        }
        public bool TryGetBoolConfig(string configKey, out bool value)
        {
            return boolConfigs.TryGetValue(configKey, out value);
        }
        public bool TryGetConfigValueString(string configKey, out string value)
        {
            value = "";
            if (intConfigs.ContainsKey(configKey)) value = intConfigs[configKey].ToString();
            else if (floatConfigs.ContainsKey(configKey)) value = floatConfigs[configKey].ToString();
            else if (boolConfigs.ContainsKey(configKey)) value = GetBoolString(boolConfigs[configKey]);
            else return false;
            return true;
        }
        #endregion GetMethods

        #region SetMethods
        // Make when you need them
        #endregion SetMethods

        public void LoadConfig()
        {
            intConfigs.Clear();
            floatConfigs.Clear();
            boolConfigs.Clear();
            if (File.Exists(configRegistryPath))
            {
                factioServer.commandHandler.OutputLine("[Config Registry] Loading the config registry");
                string[] configs = File.ReadAllLines(configRegistryPath);
                for (int i = 0; i < configs.Length; i++)
                {
                    string config = configs[i];
                    string[] configPair = config.Split(' ');
                    if (configPair.Length == 2)
                        if (ParseConfig(configPair[0], configPair[1])) continue;
                    factioServer.commandHandler.OutputLine($"[Config Registry] Could not parse config line index {i}: {config}");
                }
                EnsureDefaultConfigs();
                SaveConfig();
            }
            else
            {
                factioServer.commandHandler.OutputLine("[Config Registry] No configs found, creating and loading default file");
                EnsureDefaultConfigs();
                SaveConfig();
            }

            if (factioServer.IsDebugging) ListConfig();
        }

        public void SaveConfig()
        {
            List<string> configs = new List<string>();
            foreach (KeyValuePair<string, int> intConfig in intConfigs)
            {
                configs.Add($"{intConfig.Key} {intConfig.Value}");
            }
            foreach (KeyValuePair<string, float> floatConfig in floatConfigs)
            {
                configs.Add($"{floatConfig.Key} {floatConfig.Value}");
            }
            foreach (KeyValuePair<string, bool> boolConfig in boolConfigs)
            {
                configs.Add($"{boolConfig.Key} {GetBoolString(boolConfig.Value)}");
            }
            File.WriteAllLines(configRegistryPath, configs);
        }

        public bool ParseConfig(string key, string value)
        {
            ConfigType configType = GetConfigType(value);
            switch (configType)
            {
                case ConfigType.Unknown:
                    return false;
                case ConfigType.Int:
                    IntAddReplace(key, int.Parse(value));
                    break;
                case ConfigType.Float:
                    FloatAddReplace(key, float.Parse(value));
                    break;
                case ConfigType.Bool:
                    BoolAddReplace(key, value.ToLower() == "true");
                    break;
            }
            SaveConfig();
            return true;
        }

        public ConfigType GetConfigType(string value)
        {
            if (value.Contains('.'))
                if (float.TryParse(value, out _))
                    return ConfigType.Float;
            if (int.TryParse(value, out _))
                return ConfigType.Int;
            if (value.ToLower() == "false" || value.ToLower() == "true")
                return ConfigType.Bool;
            return ConfigType.Unknown;
        }

        public void ListConfig()
        {
            string[] configs = File.ReadAllLines(configRegistryPath);
            factioServer.commandHandler.OutputLine("[Config Registry] Configs: ");
            for (int i = 0; i < configs.Length; i++)
            {
                string config = configs[i];
                factioServer.commandHandler.OutputLine($"\t{i}: {config}");
            }
        }

        public void IntAddReplace(string configKey, int value)
        {
            RemoveConfig(configKey);
            intConfigs.Add(configKey, value);
        }
        public void FloatAddReplace(string configKey, float value)
        {
            RemoveConfig(configKey);
            floatConfigs.Add(configKey, value);
        }
        public void BoolAddReplace(string configKey, bool value)
        {
            RemoveConfig(configKey);
            boolConfigs.Add(configKey, value);
        }

        public void RemoveConfig(string configKey, bool save = false)
        {
            intConfigs.Remove(configKey);
            floatConfigs.Remove(configKey);
            boolConfigs.Remove(configKey);
            if (save) SaveConfig();
        }

        private void EnsureDefaultConfigs()
        {
            if (!TryGetIntConfig("pollPeriod", out _)) intConfigs.Add("pollPeriod", 1);
            if (!TryGetIntConfig("password", out _)) intConfigs.Add("password", factioServer.rand.Next(0, int.MaxValue));
            if (!TryGetFloatConfig("responseTime", out _)) floatConfigs.Add("responseTime", 60);
            if (!TryGetFloatConfig("votingTime", out _)) floatConfigs.Add("votingTime", 30);
            if (!TryGetFloatConfig("resultsTime", out _)) floatConfigs.Add("resultsTime", 15);
            if (!TryGetBoolConfig("isDebugging", out _)) boolConfigs.Add("isDebugging", true);
            if (!TryGetBoolConfig("isDebuggingTicks", out _)) boolConfigs.Add("isDebuggingTicks", false);
        }

        private string GetBoolString(bool value)
        {
            if (!value) return "false";
            return "true";
        }

        public enum ConfigType
        {
            Unknown,
            Int,
            Float,
            Bool
        }
    }
}
