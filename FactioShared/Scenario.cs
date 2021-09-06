using System;
using System.Collections.Generic;
using System.Text;

namespace FactioShared
{
    public class Scenario
    {
        public string text;
        public short playerAIndex;
        public short playerBIndex;

        public Scenario(string text, short playerAIndex, short playerBIndex)
        {
            this.text = text;
            this.playerAIndex = playerAIndex;
            this.playerBIndex = playerBIndex;
        }

        public string Compile(string playerA, string playerB)
        {
            int insertOffset = playerA.Length;
            string compiledScenario = text.Insert(playerAIndex, playerA);
            compiledScenario = compiledScenario.Insert(playerBIndex + insertOffset, playerB);
            return compiledScenario;
        }

        public static Scenario LoadScenario(string unloadedScenario)
        {
            int textLength = unloadedScenario.LastIndexOf('\"');
            string text = unloadedScenario.Substring(1, textLength - 1);
            string[] indicies = unloadedScenario.Substring(textLength + 2).Split(' ');
            short playerAIndex = short.Parse(indicies[0]);
            short playerBIndex = short.Parse(indicies[1]);
            return new Scenario(text, playerAIndex, playerBIndex);
        }

        public void Print()
        {
            Console.WriteLine($"[Debug] Scenario text: {text}");
            Console.WriteLine($"[Debug] Player A index: {playerAIndex}, Player B index: {playerBIndex}");
        }
    }
}
