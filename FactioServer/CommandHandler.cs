using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactioServer
{
    public class CommandHandler
    {
        private FactioServer factioServer;

        public CommandHandler(FactioServer factioServer)
        {
            this.factioServer = factioServer;
        }

        public void Handle(string command)
        {
            string[] commandSplit = command.Split(' ');
            switch (commandSplit[0])
            {
                case "help":
                    Console.WriteLine("[Core] Commands: ");
                    Console.WriteLine("\texit");
                    Console.WriteLine("\tdebug ticks");
                    Console.WriteLine("\tclear");
                    Console.WriteLine("\treload scenarios");
                    Console.WriteLine("\tconfig get configName");
                    Console.WriteLine("\tconfig set configName configValue");
                    Console.WriteLine("\tconfig list");
                    break;
                case "exit":
                    factioServer.requestedExit = true;
                    Console.WriteLine("[Core] Exiting");
                    break;
                case "debug":
                    switch (commandSplit[1])
                    {
                        case "ticks":
                            factioServer.debugTicks = !factioServer.debugTicks;
                            if (factioServer.debugTicks)
                                Console.WriteLine("[Core] Tick debugging enabled");
                            else
                                Console.WriteLine("[Core] Tick debugging disabled");
                            break;
                        default:
                            SubcommandError(command);
                            break;
                    }
                    break;
                case "clear":
                    Console.Clear();
                    break;
                case "reload":
                    switch (commandSplit[1])
                    {
                        case "scenarios":
                            factioServer.scenarioRegistry.LoadScenarios();
                            break;
                        default:
                            SubcommandError(command);
                            break;
                    }
                    break;
                default:
                    Console.WriteLine($"[Core] Unknown command: {command}");
                    break;
                case "config":
                    switch (commandSplit[1])
                    {
                        case "get":
                            if (commandSplit.Length != 3)
                                TermCountError(command, 3);
                            if (factioServer.configRegistry.TryGetConfigValueString(commandSplit[2], out string value))
                                Console.WriteLine($"[Core] Config \"{commandSplit[2]}\" value: {value}");
                            else
                                Console.WriteLine($"[Core] Unknown config \"{commandSplit[2]}\"");
                            break;
                        case "set":
                            if (commandSplit.Length != 4)
                                TermCountError(command, 4);
                            if (factioServer.configRegistry.ParseConfig(commandSplit[2], commandSplit[3]))
                                Console.WriteLine($"[Core] Config \"{commandSplit[2]}\" updated with value: {commandSplit[3]}");
                            else
                                Console.WriteLine($"[Core] Could not parse config and value");
                            break;
                        case "list":
                            factioServer.configRegistry.ListConfigs();
                            break;
                        default:
                            SubcommandError(command);
                            break;
                    }
                    break;
            }
        }

        private void SubcommandError(string command)
        {
            string[] commandSplit = command.Split(' ');
            Console.WriteLine($"[Core] Unknown subcommand of {commandSplit[1]}: {command}");
            Handle("help");
        }

        private void TermCountError(string command, int expectedTermCount)
        {
            Console.WriteLine($"[Core] Expected {expectedTermCount} terms in command: {command}");
        }
    }
}
