using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactioServer;

public class ConfigRegistry
{
    private readonly FactioServer factioServer;

    private static string ConfigRegistryPath => $"{Directory.GetCurrentDirectory()}/config.reg";

    private readonly Dictionary<string, int> intConfigs = new();
    private readonly Dictionary<string, float> floatConfigs = new();
    private readonly Dictionary<string, bool> boolConfigs = new();
    // TODO String configs

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
    // TODO Set methods?
    #endregion SetMethods

    public void LoadConfig()
    {
        intConfigs.Clear();
        floatConfigs.Clear();
        boolConfigs.Clear();
        if (File.Exists(ConfigRegistryPath))
        {
            factioServer.commandHandler.OutputLine(LoggingTag.ConfigRegistry, "Loading the config registry");
            string[] configs = File.ReadAllLines(ConfigRegistryPath);
            for (int i = 0; i < configs.Length; i++)
            {
                string config = configs[i];
                string[] configPair = config.Split(' ');
                if (configPair.Length == 2)
                    if (ParseConfig(configPair[0], configPair[1])) continue;
                factioServer.commandHandler.OutputLine(LoggingTag.ConfigRegistry, $"Could not parse config line index {i}: {config}");
            }
            EnsureDefaultConfigs();
            SaveConfig();
        }
        else
        {
            factioServer.commandHandler.OutputLine(LoggingTag.ConfigRegistry, "No configs found, creating and loading default file");
            EnsureDefaultConfigs();
            SaveConfig();
        }

        if (boolConfigs["isDebugging"]) ListConfig(true);
    }

    public void SaveConfig()
    {
        List<string> configs = new();
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
        File.WriteAllLines(ConfigRegistryPath, configs);
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

    public static ConfigType GetConfigType(string value)
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

    public void ListConfig(bool debug = false)
    {
        string[] configs = File.ReadAllLines(ConfigRegistryPath);
        factioServer.commandHandler.OutputLine(LoggingTag.ConfigRegistry, "Configs: ", debug);
        for (int i = 0; i < configs.Length; i++)
        {
            string config = configs[i];
            factioServer.commandHandler.OutputLine(LoggingTag.None, $"\t{i}: {config}");
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
        if (!TryGetFloatConfig("resultsTime", out _)) floatConfigs.Add("resultsTime", 10);
        if (!TryGetFloatConfig("roundResultsTime", out _)) floatConfigs.Add("roundResultsTime", 20);
        if (!TryGetIntConfig("roundsPerGame", out _)) intConfigs.Add("roundsPerGame", 3);
        if (!TryGetBoolConfig("isDebugging", out _)) boolConfigs.Add("isDebugging", true);
        if (!TryGetBoolConfig("isDebuggingTicks", out _)) boolConfigs.Add("isDebuggingTicks", false);
    }

    private static string GetBoolString(bool value)
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