using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace FactioServer
{
    public class Program
    {
        public const double TPS = 20;

        public static void Main(string[] args)
        {
            LogLine(LoggingTag.FactioServer, "Hello, World!");
            LogLine(LoggingTag.FactioServer, "Factio Server v0.5");
            LogLine(LoggingTag.FactioServer, "Starting server...");

            long SystemTPS;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SystemTPS = 10000000;
                LogLine(LoggingTag.FactioServer, "Oooooo, Windows!");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                SystemTPS = 1000000000;
                LogLine(LoggingTag.FactioServer, "Oooooo, Linux!");
            }
            else
            {
                LogLine(LoggingTag.FactioServer, "Unsupported OS detected! Stopping!");
                return;
            }
            long TimePerTick = (long)(SystemTPS / TPS);

            FactioServer factioServer = new();

            LogLine(LoggingTag.FactioServer, "Started polling for incoming data");

            factioServer.commandHandler.Handle("help");

            var lastTick = new Stopwatch();
            lastTick.Start();
            long timerTicks = 0;

            long nextTickId = 0;

            string input = "";
            int commandIndex = 0;
            string[] pastCommands = new string[64];
            while (!factioServer.isExitRequested)
            {
                factioServer.server.PollEvents();
                lastTick.Stop();
                timerTicks += lastTick.ElapsedTicks;
                lastTick.Restart();
                if (timerTicks >= TimePerTick)
                {
                    timerTicks -= TimePerTick;
                    factioServer.Tick(nextTickId);
                    if (factioServer.isDebuggingTicks) LogLine(LoggingTag.FactioServer, "Tick: " + nextTickId, true);
                    nextTickId++;
                    string quitReqPath = $"{Directory.GetCurrentDirectory()}/quit.req";
                    if (File.Exists(quitReqPath))
                    {
                        factioServer.commandHandler.Handle("exit");
                        File.Delete(quitReqPath);
                    }
                }
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    char c = key.KeyChar;
                    if (c == 13) // Is newline
                    {
                        if (input != "")
                        {
                            factioServer.commandHandler.Handle(input);
                            PushCommand(input, pastCommands);
                        }
                        else Console.WriteLine();
                        input = "";
                        commandIndex = 0;
                    }
                    // TODO When entering lookback mode, it goes back one too many, dont go back any when its opened
                    else if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.LeftArrow)
                    {
                        if (!string.IsNullOrEmpty(pastCommands[commandIndex + 1]))
                        {
                            commandIndex++;
                            ClearCurrentConsoleLine(input.Length);
                            string command = pastCommands[commandIndex];
                            Console.Write(command);
                            input = command;
                        }
                    }
                    else if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.RightArrow)
                    {
                        if (commandIndex != 0)
                        {
                            commandIndex--;
                            ClearCurrentConsoleLine(input.Length);
                            string command = pastCommands[commandIndex];
                            Console.Write(command);
                            input = command;
                        }
                    }
                    else if (c == 8 || c == 127) // Is backspace
                    {
                        if (input.Length > 0)
                        {
                            ClearCurrentConsoleLine(input.Length);
                            input = input.Remove(input.Length - 1);
                            Console.Write(input);
                        }
                    }
                    else if (char.IsLetterOrDigit(c) || c == ' ' || char.IsPunctuation(c))
                        input += c;
                    else
                        ClearCurrentCharacter();
                }
                Thread.Sleep(factioServer.PollPeriod);
            }
            factioServer.ServerShutdown();
            factioServer.commandHandler.OutputLine(LoggingTag.FactioServer, "Exiting");
        }

        public static void ClearCurrentConsoleLine(int width)
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            for (int i = 0; i < width; i++)
                Console.Write(" ");
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void ClearCurrentCharacter()
        {
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            Console.Write(" ");
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }

        private static void PushCommand(string command, string[] pastCommands)
        {
            for (int i = pastCommands.Length - 2; i >= 0; i--)
            {
                pastCommands[i + 1] = pastCommands[i];
            }
            pastCommands[0] = command;
        }

        public static string GetLoggingTag(LoggingTag loggingTag, bool debug = false)
        {
            string text = loggingTag switch
            {
                LoggingTag.None => "",
                LoggingTag.FactioServer => "[Factio Server] ",
                LoggingTag.CommandHandler => "[Command Handler] ",
                LoggingTag.ConfigRegistry => "[Config Registry] ",
                LoggingTag.FactioServerListener => "[Factio Server Listener] ",
                LoggingTag.ScenarioRegistry => "[Scenario Registry] ",
                LoggingTag.FactioGame => "[Factio Game] ",
                _ => "[Unimplemented Logging Tag] ",
            };
            if (debug) text += "[Debug] ";
            return text;
        }

        public static void Log(LoggingTag loggingTag, string text, bool debug = false)
        {
            Console.Write(GetLoggingTag(loggingTag, debug) + text);
        }
        public static void LogLine(LoggingTag loggingTag, string text, bool debug = false)
        {
            Console.WriteLine(GetLoggingTag(loggingTag, debug) + text);
        }
    }

    public enum LoggingTag
    {
        None,
        FactioServer,
        CommandHandler,
        ConfigRegistry,
        FactioServerListener,
        ScenarioRegistry,
        FactioGame
    }
}