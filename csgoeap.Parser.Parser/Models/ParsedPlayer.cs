using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace csgoeap.Parser.Parser.Models
{
    public class ParsedPlayer
    {
        public ParsedPlayer()
        {
            Flashes = new ParsedFlashes();
        }
        public long SteamId { get; set; }
        public string Name { get; set; }
        public int Kills { get; set; }
        public int Aces { get; set; }
        public int FriendlyKills { get; set; }
        public int SoloKill { get; set; }
        public int Deaths { get; set; }
        public int TDeaths { get; set; }
        public int CTDeaths { get; set; }
        public int LastDeath { get; set; }
        public int FirstDeath { get; set; }
        public int Suicides { get; set; }
        public int Rounds { get; set; }
        public int TRounds { get; set; }
        public int CTRounds { get; set; }
        public long StartMoney { get; set; }
        public int BombPlants { get; set; }
        public int BombDefuses { get; set; }
        public int DefuseKitBought { get; set; }
        public int DeathWhileHoldingGrenade { get; set; }
        public int DeathWhileHoldingBomb { get; set; }
        public ParsedFlashes Flashes { get; set; }

        public void Combine(ParsedPlayer newPlayer)
        {
            Kills += newPlayer.Kills;
            Aces += newPlayer.Aces;
            FriendlyKills += newPlayer.FriendlyKills;
            Deaths += newPlayer.Deaths;
            TDeaths += newPlayer.TDeaths;
            CTDeaths += newPlayer.CTDeaths;
            LastDeath += newPlayer.LastDeath;
            FirstDeath += newPlayer.FirstDeath;
            SoloKill += newPlayer.SoloKill;
            Suicides += newPlayer.Suicides;
            Rounds += newPlayer.Rounds;
            StartMoney += newPlayer.StartMoney;
            DefuseKitBought += newPlayer.DefuseKitBought;

            DeathWhileHoldingGrenade += newPlayer.DeathWhileHoldingGrenade;
            DeathWhileHoldingBomb += newPlayer.DeathWhileHoldingBomb;

            TRounds += newPlayer.TRounds;
            CTRounds += newPlayer.CTRounds;

            BombPlants += newPlayer.BombPlants;
            BombDefuses += newPlayer.BombDefuses;

            Flashes.Combine(newPlayer.Flashes);
        }

        public void Print()
        {
            Console.WriteLine($"[{SteamId}] {Name} Rounds: {Rounds} ({CTRounds}/{TRounds}) K:{Kills} Ace:{Aces} FK: {FriendlyKills} SK: {SoloKill}");
            Console.WriteLine($"D:{Deaths} ({CTDeaths}/{TDeaths}) S:{Suicides} FD:{FirstDeath} ({Math.Round((double)FirstDeath / Rounds, 3)}) LD: {LastDeath} ({Math.Round((double)LastDeath / Rounds, 3)})");
            Console.WriteLine($"DGran:{DeathWhileHoldingGrenade} ({Math.Round((double)DeathWhileHoldingGrenade / Deaths, 3)}) DBomb:{DeathWhileHoldingBomb} ({Math.Round((double)DeathWhileHoldingBomb / TDeaths, 3)})");
            Console.WriteLine($"EF:{Flashes.EnemiesFlashed} FF:{Flashes.FriendlyFlashed} SF:{Flashes.SelfFlashed} F:{Flashes.Flashed}");
            Console.WriteLine($"Money:{StartMoney} Avg: {StartMoney / Rounds}");
            Console.WriteLine($"BD:{BombDefuses} ({Math.Round((double)BombDefuses / CTRounds, 3)}) BP: {BombPlants} ({Math.Round((double)BombPlants /TRounds, 3)})");
            Console.WriteLine($"Kits:{DefuseKitBought} ({Math.Round((double)DefuseKitBought / CTRounds, 3)})");
        }
    }
}
