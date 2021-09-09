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
            if (commandSplit.Length < 1) return;
            switch (commandSplit[0])
            {
                case "help":
                    Console.WriteLine("[Core] Commands: ");
                    Console.WriteLine("\texit");
                    Console.WriteLine("\tclear");
                    Console.WriteLine("\treload scenarios");
                    Console.WriteLine("\treload config");
                    Console.WriteLine("\tconfig get configName");
                    Console.WriteLine("\tconfig set configName configValue");
                    Console.WriteLine("\tconfig del configName");
                    Console.WriteLine("\tconfig list");
                    break;
                case "exit":
                    factioServer.isExitRequested = true;
                    Console.WriteLine("[Core] Exiting");
                    break;
                case "clear":
                    Console.Clear();
                    break;
                case "reload":
                    if (TermCountError(command, 2)) return;
                    switch (commandSplit[1])
                    {
                        case "scenarios":
                            factioServer.scenarioRegistry.LoadScenarios();
                            break;
                        case "config":
                            Console.WriteLine($"[Core] Reloading server config");
                            factioServer.ReloadServerConfig();
                            break;
                        default:
                            SubcommandError(command);
                            break;
                    }
                    break;
                case "config":
                    switch (commandSplit[1])
                    {
                        case "get":
                            if (TermCountError(command, 3)) return;
                            if (factioServer.configRegistry.TryGetConfigValueString(commandSplit[2], out string value))
                                Console.WriteLine($"[Core] Config \"{commandSplit[2]}\" value: {value}");
                            else
                                Console.WriteLine($"[Core] Unknown config \"{commandSplit[2]}\"");
                            break;
                        case "set":
                            if (TermCountError(command, 4)) return;
                            if (factioServer.configRegistry.ParseConfig(commandSplit[2], commandSplit[3]))
                                Console.WriteLine($"[Core] Config \"{commandSplit[2]}\" updated with value: {commandSplit[3]}");
                            else
                                Console.WriteLine($"[Core] Could not parse config and value");
                            break;
                        case "del":
                            if (TermCountError(command, 3)) return;
                            Console.WriteLine($"[Core] Deleting config \"{commandSplit[2]}\"");
                            factioServer.configRegistry.RemoveConfig(commandSplit[2], true);
                            break;
                        case "list":
                            factioServer.configRegistry.ListConfigs();
                            break;
                        default:
                            SubcommandError(command);
                            break;
                    }
                    break;
                default:
                    Console.WriteLine($"[Core] Unknown command: {command}");
                    break;
            }
        }

        private void SubcommandError(string command)
        {
            string[] commandSplit = command.Split(' ');
            Console.WriteLine($"[Core] Unknown subcommand of {commandSplit[1]}: {command}");
            Handle("help");
        }

        private bool TermCountError(string command, int expectedTermCount)
        {
            string[] commandSplit = command.Split(' ');
            if (commandSplit.Length != expectedTermCount)
            {
                Console.WriteLine($"[Core] Expected {expectedTermCount} terms in command: {command}");
                return true;
            }
            return false;
        }
    }
}
