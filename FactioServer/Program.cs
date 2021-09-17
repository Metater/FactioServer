using System;
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
            LogLine(LoggingTag.FactioServer, "Hello World!");
            LogLine(LoggingTag.FactioServer, "Factio Server v0.3");
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

            FactioServer factioServer = new FactioServer();

            LogLine(LoggingTag.FactioServer, "Started polling for incoming data");

            factioServer.commandHandler.Handle("help");

            var lastTick = new Stopwatch();
            lastTick.Start();
            long timerTicks = 0;

            long nextTickId = 0;

            string input = "";
            //int lastChar = -1;
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
                    if (File.Exists($"{Directory.GetCurrentDirectory()}/quit.req"))
                    {
                        factioServer.commandHandler.Handle("exit");
                        File.Delete($"{Directory.GetCurrentDirectory()}/quit.req");
                    }
                }
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    char c = key.KeyChar;
                    if (c == 13) // Is newline
                    {
                        if (input != "") factioServer.commandHandler.Handle(input);
                        else Console.WriteLine();
                        input = "";
                    }
                    else if (c == 8 || c == 127) // Is backspace
                    {
                        if (input.Length > 0)
                        {
                            input = input.Remove(input.Length - 1);
                            //if (lastChar != 8 && lastChar != 127) Console.WriteLine();
                            Console.Write("\n" + input);
                        }
                    }
                    else
                        if (char.IsLetterOrDigit(c) || c == ' ') input += c;
                    //lastChar = c;
                }
                Thread.Sleep(factioServer.PollPeriod);
            }
            factioServer.gameManager.ServerShutdown();
            factioServer.commandHandler.OutputLine(LoggingTag.FactioServer, "Exiting");
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