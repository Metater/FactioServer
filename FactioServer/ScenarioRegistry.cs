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

        public bool DeleteScenario(int id)
        {
            if (scenarios.Count <= id) return false;
            scenarios.RemoveAt(id);
            RefreshIds();
            SaveScenarios();
            return true;
        }

        public bool ReplaceScenario(int id, string unloadedScenario)
        {
            if (scenarios.Count <= id) return false;
            scenarios[id].text = unloadedScenario;
            SaveScenarios();
            return true;
        }

        public void LoadScenarios()
        {
            if (File.Exists(scenarioRegistryPath))
            {
                factioServer.commandHandler.OutputLine("[Scenario Registry] Loading the scenario registry");
                string[] unloadedScenarios = File.ReadAllLines(scenarioRegistryPath);

                for (int i = 0; i < unloadedScenarios.Length; i++)
                {
                    scenarios.Add(Scenario.Load(i, unloadedScenarios[i]));
                }

                for (int i = 0; i < scenarios.Count; i++)
                {
                    if (factioServer.IsDebugging)
                        factioServer.commandHandler.OutputLine($"[Scenario Registry] [Debug] Scenario {i}: {scenarios[i].Compile("Player A", "Player B")}");
                }

                factioServer.commandHandler.OutputLine("[Scenario Registry] Finished loading the scenario registry");
            }
            else
            {
                factioServer.commandHandler.OutputLine("[Scenario Registry] No scenarios found, creating empty file");
                File.WriteAllText(scenarioRegistryPath, "");
            }
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
