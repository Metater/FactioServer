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

        public void LoadScenarios()
        {
            if (File.Exists(scenarioRegistryPath))
            {
                Console.WriteLine("[Core (Scenario Registry)] Loading the scenario registry");
                string[] unloadedScenarios = File.ReadAllLines(scenarioRegistryPath);
                foreach (string unloadedScenario in unloadedScenarios)
                    scenarios.Add(Scenario.LoadScenario(unloadedScenario));

                for (int i = 0; i < scenarios.Count; i++)
                {
                    if (factioServer.configRegistry.GetBoolConfig("debugging"))
                        Console.WriteLine($"[Debug] Scenario {i}: {scenarios[i].Compile("Player A", "Player B")}");
                }
            }
            else
            {
                Console.WriteLine("[Core (Scenario Registry)] No scenarios found, creating empty file");
                File.WriteAllText(scenarioRegistryPath, "");
            }
        }
    }
}
