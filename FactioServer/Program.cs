using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace FactioServer
{
    class Program
    {
        public const double TPS = 1;

        static void Main(string[] args)
        {
            Console.WriteLine("[Core] Hello World!");
            Console.WriteLine("[Core] Factio Server v0.1");
            Console.WriteLine("[Core] Starting server...");

            long SystemTPS;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SystemTPS = 10000000;
                Console.WriteLine("[Core] Oooooo, Windows!");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                SystemTPS = 1000000000;
                Console.WriteLine("[Core] Oooooo, Linux!");
            }
            else
            {
                Console.WriteLine("[Core] Unsupported OS detected! Stopping!");
                return;
            }
            long TimePerTick = (long)(SystemTPS / TPS);

            FactioServer factioServer = new FactioServer();

            Console.WriteLine("[Core] Started polling for incoming data");

            var lastTick = new Stopwatch();
            lastTick.Start();
            long timerTicks = 0;

            long nextTickId = 0;

            string input = "";
            bool running = true;
            while (running)
            {
                factioServer.server.PollEvents();
                lastTick.Stop();
                timerTicks += lastTick.ElapsedTicks;
                lastTick.Restart();
                if (timerTicks >= TimePerTick)
                {
                    timerTicks -= TimePerTick;
                    factioServer.Tick(nextTickId);
                    if (factioServer.debugTicks) Console.WriteLine("Tick: " + nextTickId);
                    nextTickId++;
                }
                Thread.Sleep(1);
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    char c = key.KeyChar;
                    if (c == 13) // Is newline
                    {
                        if (input != "") factioServer.Command(input);
                        input = "";
                    }
                    else if (c == 8) // Is backspace
                        input = input.Remove(input.Length - 1);
                    else
                        input += c;
                }
                running = !factioServer.requestedExit;
            }
        }
    }
}