using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FactioShared
{
    public class Scenario
    {
        public int id = -1;
        public string text;

        public Scenario(int id, string text)
        {
            this.id = id;
            this.text = text;
        }

        public Scenario(string text)
        {
            this.text = text;
        }

        public static Scenario Load(int id, string unloadedScenario)
        {
            int textLength = unloadedScenario.LastIndexOf('\"');
            string text = unloadedScenario.Substring(1, textLength - 1);
            return new Scenario(id, text);
        }

        public string Compile(string playerA, string playerB)
        {
            string compiledScenario = text.Replace("{A}", playerA);
            compiledScenario = compiledScenario.Replace("{B}", playerB);
            return compiledScenario;
        }
    }
}
