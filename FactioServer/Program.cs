using System;
using System.Diagnostics;
using System.Threading;

namespace FactioServer
{
    class Program
    {
        public const double TPS = 1;
        public const long SystemTPS = 1000000000;
        public const long TimePerTick = (long)(SystemTPS / TPS);

        static void Main(string[] args)
        {

            Console.WriteLine("[Core] Hello World!");
            Console.WriteLine("[Core] Factio Server v0.1");
            Console.WriteLine("[Core] Starting server...");

            FactioServer factioServer = new FactioServer();

            Console.WriteLine("[Core] Started polling for incoming data");

            var lastTick = new Stopwatch();
            lastTick.Start();
            long timerTicks = 0;

            long nextTickId = 0;

            while (!Console.KeyAvailable)
            {
                factioServer.server.PollEvents();
                lastTick.Stop();
                timerTicks += lastTick.ElapsedTicks;
                lastTick.Restart();
                if (timerTicks >= TimePerTick)
                {
                    timerTicks -= TimePerTick;
                    factioServer.Tick(nextTickId);
                    //Console.WriteLine("Tick: " + nextTickId);
                    nextTickId++;
                }
                Thread.Sleep(1);
            }
        }
    }
}