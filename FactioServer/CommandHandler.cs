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

        private bool currentlyExecuting = false;
        private string output = "";
        private bool redirectingOutput = false;

        public CommandHandler(FactioServer factioServer)
        {
            this.factioServer = factioServer;
        }

        public string Handle(string command, bool redirectOutput = false)
        {
            currentlyExecuting = true;
            output = "";
            redirectingOutput = redirectOutput;

            string[] commandSplit = command.Split(' ');
            if (commandSplit.Length < 1)
            {
                OutputLine(LoggingTag.CommandHandler, "Can't parse whitespace");
                return CommandReturn();
            }
            switch (commandSplit[0])
            {
                case "help":
                    OutputLine(LoggingTag.CommandHandler, "Commands: ");

                    OutputLine(LoggingTag.None, "\texit");
                    OutputLine(LoggingTag.None, "\tclear");

                    OutputLine(LoggingTag.None, "\treload");
                    OutputLine(LoggingTag.None, "\t\tscenarios");
                    OutputLine(LoggingTag.None, "\t\tconfig");

                    OutputLine(LoggingTag.None, "\tconfig");
                    OutputLine(LoggingTag.None, "\t\tget configName");
                    OutputLine(LoggingTag.None, "\t\tset configName configValue");
                    OutputLine(LoggingTag.None, "\t\tremove configName");
                    OutputLine(LoggingTag.None, "\t\tlist");

                    OutputLine(LoggingTag.None, "\tscenarios");
                    OutputLine(LoggingTag.None, "\t\tadd scenarioText");
                    OutputLine(LoggingTag.None, "\t\tremove scenarioId");
                    OutputLine(LoggingTag.None, "\t\treplace scenarioId scenarioText");
                    OutputLine(LoggingTag.None, "\t\tlist");
                    break;
                case "exit":
                    factioServer.isExitRequested = true;
                    break;
                case "clear":
                    if (!redirectingOutput) Console.Clear();
                    break;
                case "reload":
                    if (TermCountError(command, 2)) return CommandReturn();
                    switch (commandSplit[1])
                    {
                        case "scenarios":
                            factioServer.scenarioRegistry.LoadScenarios();
                            break;
                        case "config":
                            factioServer.configRegistry.LoadConfig();
                            factioServer.LoadServerConfig();
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
                            if (TermCountError(command, 3)) return CommandReturn();
                            if (factioServer.configRegistry.TryGetConfigValueString(commandSplit[2], out string value))
                                OutputLine(LoggingTag.ConfigRegistry, $"Config \"{commandSplit[2]}\" value: {value}");
                            else
                                OutputLine(LoggingTag.ConfigRegistry, $"Unknown config \"{commandSplit[2]}\"");
                            break;
                        case "set":
                            if (TermCountError(command, 4)) return CommandReturn();
                            if (factioServer.configRegistry.ParseConfig(commandSplit[2], commandSplit[3]))
                                OutputLine(LoggingTag.ConfigRegistry, $"Config \"{commandSplit[2]}\" updated with value: {commandSplit[3]}");
                            else
                                OutputLine(LoggingTag.ConfigRegistry, $"Could not parse config and value");
                            break;
                        case "remove":
                            if (TermCountError(command, 3)) return CommandReturn();
                            OutputLine(LoggingTag.ConfigRegistry, $"Removing config \"{commandSplit[2]}\"");
                            factioServer.configRegistry.RemoveConfig(commandSplit[2], true);
                            break;
                        case "list":
                            factioServer.configRegistry.ListConfig();
                            break;
                        default:
                            SubcommandError(command);
                            break;
                    }
                    break;
                case "scenarios":
                    switch (commandSplit[1])
                    {
                        case "add":
                            if (TermCountError(command, 3)) return CommandReturn();
                            string scenarioText = command.Substring(command.IndexOf(commandSplit[1]) + commandSplit[1].Length);
                            factioServer.scenarioRegistry.AddScenario(scenarioText);
                            break;
                        case "remove":
                            if (TermCountError(command, 3)) return CommandReturn();
                            break;
                        case "replace":
                            if (TermCountError(command, 4)) return CommandReturn();
                            break;
                        case "list":
                            factioServer.scenarioRegistry.ListScenarios();
                            break;
                        default:
                            SubcommandError(command);
                            break;
                    }
                    break;
                default:
                    OutputLine(LoggingTag.CommandHandler, $"Unknown command: {command}");
                    break;
            }
            return CommandReturn();
        }

        public void Output(LoggingTag loggingTag, string text, bool debug = false)
        {
            text = Program.GetLoggingTag(loggingTag, debug) + text;
            if (!redirectingOutput)
                Console.Write(text);
            if (currentlyExecuting)
                output += text;
        }
        public void OutputLine(LoggingTag loggingTag, string text, bool debug = false)
        {
            text = Program.GetLoggingTag(loggingTag, debug) + text;
            if (!redirectingOutput)
                Console.WriteLine(text);
            if (currentlyExecuting)
                output += text + "\n";
        }

        private string CommandReturn()
        {
            currentlyExecuting = false;
            redirectingOutput = false;
            return output;
        }

        private void SubcommandError(string command)
        {
            string[] commandSplit = command.Split(' ');
            OutputLine(LoggingTag.CommandHandler, $"Unknown subcommand of {commandSplit[1]}: {command}");
        }

        private bool TermCountError(string command, int expectedTermCount)
        {
            string[] commandSplit = command.Split(' ');
            if (commandSplit.Length != expectedTermCount)
            {
                OutputLine(LoggingTag.CommandHandler, $"Expected {expectedTermCount} terms in command: {command}");
                return true;
            }
            return false;
        }
    }
}
