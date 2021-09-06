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

        // scenario registry
        // and way of refreshing it with a command
        private string scenarioRegistryPath => Directory.GetCurrentDirectory() + "/" + "scenarios.reg";

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
                Console.WriteLine("[Core] Loading the scenario registry");
                string[] unloadedScenarios = File.ReadAllLines(scenarioRegistryPath);
                foreach (string unloadedScenario in unloadedScenarios)
                    scenarios.Add(Scenario.LoadScenario(unloadedScenario));

                scenarios.ForEach((scenario) => { scenario.Print(); Console.WriteLine($"[Debug] {scenario.Compile("Billy the 5th", "Octavius the 0th")}"); });
            }
            else
            {
                Console.WriteLine("[Core] No scenarios found, creating empty file");
                File.WriteAllText(scenarioRegistryPath, "");
            }
        }
    }
}
