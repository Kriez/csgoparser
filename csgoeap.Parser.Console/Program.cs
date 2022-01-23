using csgoeap.Parser.Parser;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using csgoeap.Parser.Parser.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace csgoeap.Parser.Console
{
    class Program
    {
        static void Main(string[] args)     
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", true)
                .Build();
            
            List<long> playerSteamId = config.GetValue<string>("Players").Split(";").Select(p => long.Parse(p)).ToList();

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DemParser>();

            DemParser parser = new DemParser(logger);
            Dictionary<long, ParsedPlayer> players = new Dictionary<long, ParsedPlayer>();
            Stopwatch timer = Stopwatch.StartNew();

            var files = Directory.GetFiles(config.GetValue<string>("DemoPath"));

            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = config.GetValue<int>("Threads")
            };

            object locker = new object();

            Parallel.ForEach(files, parallelOptions, (filePath, cancellationToken) =>
            {
                logger.LogInformation($"Parsing file {filePath}");
                DemParser parser = new DemParser(logger);
                var participants = parser.Parse(filePath);

                lock (locker)
                {
                    foreach (var participant in participants)
                    {
                        if (!playerSteamId.Contains(participant.SteamId))
                        {
                            continue;
                        }

                        if (players.TryGetValue(participant.SteamId, out var player))
                        {
                            player.Combine(participant);
                        }
                        else
                        {
                            players.Add(participant.SteamId, participant);
                        }
                    }
                }
            });

            foreach (var player in players.Values)
            {
                player.Print();
            }
            System.Console.WriteLine($"Parsing done. Elapsed time {timer.Elapsed.Minutes}m:{timer.Elapsed.Seconds}s");
        }
    }
}
