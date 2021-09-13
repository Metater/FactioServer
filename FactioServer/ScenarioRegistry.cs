using FactioShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FactioServer
{
    public class ScenarioRegistry
    {
        private FactioServer factioServer;

        private string scenarioRegistryPath => $"{Directory.GetCurrentDirectory()}/scenarios.reg";

        private List<Scenario> scenarios = new List<Scenario>();

        public ScenarioRegistry(FactioServer factioServer)
        {
            this.factioServer = factioServer;
            LoadScenarios();
        }

        public Scenario GetRandomScenario()
        {
            return scenarios[factioServer.rand.Next(0, scenarios.Count)];
        }

        public void AddScenario(string scenarioText)
        {
            int id = scenarios.Count;
            scenarios.Add(new Scenario(id, scenarioText));
            SaveScenarios();
        }

        public bool RemoveScenario(int id)
        {
            if (scenarios.Count <= id) return false;
            scenarios.RemoveAt(id);
            RefreshIds();
            SaveScenarios();
            return true;
        }

        public bool ReplaceScenario(int id, string scenarioText)
        {
            if (scenarios.Count <= id) return false;
            scenarios[id].text = scenarioText;
            SaveScenarios();
            return true;
        }

        public void LoadScenarios()
        {
            if (File.Exists(scenarioRegistryPath))
            {
                factioServer.commandHandler.OutputLine(LoggingTag.ScenarioRegistry, "Loading the scenario registry");
                string[] unloadedScenarios = File.ReadAllLines(scenarioRegistryPath);

                for (int i = 0; i < unloadedScenarios.Length; i++)
                    scenarios.Add(Scenario.Load(i, unloadedScenarios[i]));

                factioServer.commandHandler.OutputLine(LoggingTag.ScenarioRegistry, "Finished loading the scenario registry");

                if (factioServer.IsDebugging) ListScenarios();
            }
            else
            {
                factioServer.commandHandler.OutputLine(LoggingTag.ScenarioRegistry, "No scenarios found, creating empty file");
                File.WriteAllText(scenarioRegistryPath, "");
            }
        }

        public void ListScenarios()
        {
            factioServer.commandHandler.OutputLine(LoggingTag.ScenarioRegistry, $"Listing scenarios: ");
            for (int i = 0; i < scenarios.Count; i++)
                factioServer.commandHandler.OutputLine(LoggingTag.None, $"\tScenario {i}: {scenarios[i].Compile("Player A", "Player B")}");
        }

        private void SaveScenarios()
        {
            List<string> unloadedScenarios = new List<string>();
            foreach (Scenario scenario in scenarios) unloadedScenarios.Add($"\"{scenario.text}\"");
            File.WriteAllLines(scenarioRegistryPath, unloadedScenarios.ToArray());
        }

        private void RefreshIds()
        {
            for (int i = 0; i < scenarios.Count; i++)
                scenarios[i].id = i;
        }
    }
}
