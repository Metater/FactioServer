using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace FactioServer
{
    class Program
    {
        public const double TPS = 20;

        static void Main(string[] args)
        {
            Console.WriteLine("[Factio Server] Hello World!");
            Console.WriteLine("[Factio Server] Factio Server v0.2");
            Console.WriteLine("[Factio Server] Starting server...");

            long SystemTPS;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SystemTPS = 10000000;
                Console.WriteLine("[Factio Server] Oooooo, Windows!");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                SystemTPS = 1000000000;
                Console.WriteLine("[Factio Server] Oooooo, Linux!");
            }
            else
            {
                Console.WriteLine("[Factio Server] Unsupported OS detected! Stopping!");
                return;
            }
            long TimePerTick = (long)(SystemTPS / TPS);

            FactioServer factioServer = new FactioServer();

            Console.WriteLine("[Factio Server] Started polling for incoming data");

            var lastTick = new Stopwatch();
            lastTick.Start();
            long timerTicks = 0;

            long nextTickId = 0;

            string input = "";
            int lastChar = -1;
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
                    if (factioServer.isDebuggingTicks) Console.WriteLine("[Factio Server] [Debug] Tick: " + nextTickId);
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
                            if (lastChar != 8 && lastChar != 127) Console.WriteLine();
                            Console.WriteLine(input);
                        }
                    }
                    else
                        if (char.IsLetterOrDigit(c) || c == ' ') input += c;
                    lastChar = c;
                }
                Thread.Sleep(factioServer.PollPeriod);
            }
            factioServer.commandHandler.OutputLine("[Factio Server] Exiting");
        }
    }
}