using DemoInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using csgoeap.Parser.Parser.Models;
using Microsoft.Extensions.Logging;

namespace csgoeap.Parser.Parser
{
    public class DemParser
    {
        private bool logDebug = false;
        private DemoParser _parser;
        private List<ParsedPlayer> _players;
        private readonly ILogger<DemParser> _logger;
        private int _round;
        private bool warmup = true;
        private List<WarmupInfo> _warmupPlayers;
        private List<RoundInfo> _roundPlayers;
        public DemParser(ILogger<DemParser> logger)
        {
            _logger = logger;
            _players = new List<ParsedPlayer>();
            _warmupPlayers = new List<WarmupInfo>();
            _roundPlayers = new List<RoundInfo>();
        }

        public List<ParsedPlayer> Parse(string filePath)
        {
            Stopwatch timer = Stopwatch.StartNew();
            using (var input = File.OpenRead(filePath))
            {
                _parser = new DemoParser(input);
                _parser.Blind += ParserOnBlind;
                _parser.RoundEnd += Parser_RoundEnd;
                _parser.RoundOfficiallyEnd += Parser_RoundOfficiallyEnd;
                _parser.PlayerKilled += Parser_PlayerKilled;
                _parser.RankUpdate += Parser_RankUpdate;
                _parser.MatchStarted += Parser_MatchStarted;
                _parser.FreezetimeEnded += _parser_FreezetimeEnded;
                _parser.WeaponFired += Parser_WeaponFired;
                _parser.PlayerHurt += Parser_PlayerHurt;
                _parser.BombPlanted += Parser_BombPlanted;
                _parser.BombDefused += Parser_BombDefused;
                _parser.WinPanelMatch += Parser_WinPanelMatch;
                _parser.RoundStart += _parser_RoundStart;
                _parser.RoundEnd += _parser_RoundEnd;
                _parser.WinPanelMatch += _parser_WinPanelMatch;
                _parser.ParseHeader();
                _parser.ParseToEnd();
            }
            timer.Stop();

            _logger.LogInformation($"Parsing done. Elapsed time {timer.Elapsed.Minutes}m:{timer.Elapsed.Seconds}s");

            return _players;
        }

        private void Log(string message)
        {
            if (logDebug)
            {
                _logger.LogDebug(message);
            }
        }
        private void _parser_RoundEnd(object sender, RoundEndedEventArgs e)
        {
            
        }

        private RoundInfo GetRoundPlayer(long steamId)
        {
            var player = _roundPlayers.SingleOrDefault(p => p.SteamId.Equals(steamId));
            if (player == null)
            {
                var demoPlayer = _parser.PlayingParticipants.Single(p => p.SteamID == steamId);

                player = new RoundInfo()
                {
                    SteamId = demoPlayer.SteamID,
                };

                _roundPlayers.Add(player);
            }

            return player;
        }

        private WarmupInfo GetWarmupPlayer(long steamId)
        {
            var player = _warmupPlayers.SingleOrDefault(p => p.SteamId.Equals(steamId));
            if (player == null)
            {
                var demoPlayer = _parser.PlayingParticipants.Single(p => p.SteamID == steamId);

                player = new WarmupInfo()
                {
                    SteamId = demoPlayer.SteamID,
                };

                _warmupPlayers.Add(player);
            }

            return player;
        }

        private ParsedPlayer GetPlayer(long steamId)
        {
            var player = _players.SingleOrDefault(p => p.SteamId.Equals(steamId));
            if (player == null)
            {
                var demoPlayer = _parser.PlayingParticipants.Single(p => p.SteamID == steamId);

                player = new ParsedPlayer()
                {
                    SteamId = demoPlayer.SteamID,
                    Name = demoPlayer.Name
                };

                _players.Add(player);
                Log($"Player {demoPlayer.SteamID} {demoPlayer.Name} added");
            }

            return player;
        }

        private void _parser_FreezetimeEnded(object sender, FreezetimeEndedEventArgs e)
        {
            foreach (var participant in _parser.PlayingParticipants)
            {
                var warmup = GetWarmupPlayer(participant.SteamID);
                if (participant.HasDefuseKit && !warmup.StartedWithDefuseKit)
                {
                    var player = GetPlayer(participant.SteamID);
                    player.DefuseKitBought++;
                }
            }

            //ctEquipValue = parser.Participants.Where(a => a.Team == Team.CounterTerrorist).Sum(a => a.CurrentEquipmentValue);
        }

        private void _parser_WinPanelMatch(object sender, WinPanelMatchEventArgs e)
        {
            Log($"Match ended on round: {_round}");
        }

        private void _parser_RoundStart(object sender, RoundStartedEventArgs e)
        {
            if (warmup)
            {
                return;
            }

            _warmupPlayers.Clear();
            _roundPlayers.Clear();

            foreach (var participant in _parser.Participants.Where(p => p.Team != Team.Spectate))
            {
                var warmup = GetWarmupPlayer(participant.SteamID);
                if (participant.HasDefuseKit)
                {
                    warmup.StartedWithDefuseKit = true;
                }
                

                var player = GetPlayer(participant.SteamID);
                player.StartMoney += participant.Money;
                Log($"{participant.Name}: {participant.Money}");
            }
        }

        private void ParserOnBlind(object? sender, BlindEventArgs e)
        {
            if (warmup)
            {
                return;
            }

            var victim = GetPlayer(e.Player.SteamID);
            var attacker = GetPlayer(e.Attacker.SteamID);

            victim.Flashes.Flashed++;

            if (e.Attacker.SteamID == e.Player.SteamID)
            {
                attacker.Flashes.SelfFlashed++;
                Log($"{attacker.Name} flashed himself");
            }
            else if (e.Attacker.Team == e.Player.Team)
            {
                attacker.Flashes.FriendlyFlashed++;
                Log($"{attacker.Name} flashed {victim.Name} (Friendly flash)");
            }
            else
            {
                attacker.Flashes.EnemiesFlashed++;
                Log($"{attacker.Name} flashed {victim.Name}");
            }

        }

        private void Parser_WinPanelMatch(object sender, WinPanelMatchEventArgs e)
        {
        }

        private void Parser_BombDefused(object sender, BombEventArgs e)
        {
            var player = GetPlayer(e.Player.SteamID);
            player.BombDefuses++;
        }

        private void Parser_BombPlanted(object sender, BombEventArgs e)
        {
            var player = GetPlayer(e.Player.SteamID);
            player.BombPlants++;
        }

        private void Parser_PlayerHurt(object sender, PlayerHurtEventArgs e)
        {
        }

        private void Parser_WeaponFired(object sender, WeaponFiredEventArgs e)
        {
        }

        private void Parser_MatchStarted(object sender, MatchStartedEventArgs e)
        {
            Log("Match starting");
            warmup = false;
            _round = 1;
            _players.Clear();
            _warmupPlayers.Clear();
            _roundPlayers.Clear();
            Log($"New round: {_round}");
        }

        private void Parser_RankUpdate(object sender, RankUpdateEventArgs e)
        {
        }

        private void Parser_PlayerKilled(object sender, PlayerKilledEventArgs e)
        {
            if (warmup)
            {
                return;
            }
            if (e.Victim == null)
            {
                return;
            }

            var victim = GetPlayer(e.Victim.SteamID);
            victim.Deaths++;
            if (e.Victim.Team == Team.CounterTerrorist)
            {
                victim.CTDeaths++;
            }else
            {
                victim.TDeaths++;
            }
                

            if (e.Killer == null)
            {
                victim.Suicides++;
                Log($"{victim.Name} killed himself");
                return;
            }

            if (e.Victim.ActiveWeapon.Class == EquipmentClass.Grenade)
            {
                victim.DeathWhileHoldingGrenade++;
            }
            if (e.Victim.ActiveWeapon.Weapon == EquipmentElement.Bomb)
            {
                victim.DeathWhileHoldingBomb++;
            }

            var killer = GetPlayer(e.Killer.SteamID);
            if (e.Killer.Team == e.Victim.Team)
            {
                killer.FriendlyKills++;
            }
            else
            {
                var rp = GetRoundPlayer(e.Killer.SteamID);
                rp.Kills++;
                killer.Kills++;                
            }
                        

            if (_parser.PlayingParticipants.Where(p => p.Team == e.Killer.Team && p.IsAlive).Count() == 1)
            {
                killer.SoloKill++;
            }

            if (_parser.PlayingParticipants.Where(p => p.Team == e.Victim.Team && p.IsAlive).Count() == 1)
            {
                victim.LastDeath++;
            }
            if (_parser.PlayingParticipants.Where(p => p.IsAlive).Count() == _parser.PlayingParticipants.Count())
            {
                victim.FirstDeath++;
            }
            Log($"{killer.Name} killed {victim.Name}");
        }

        private void Parser_RoundOfficiallyEnd(object sender, RoundOfficiallyEndedEventArgs e)
        {                       
            foreach (var participant in _parser.Participants.Where(p => p.Team != Team.Spectate))
            {
                var player = GetPlayer(participant.SteamID);
                player.Rounds++;
                if (participant.Team == Team.Terrorist)
                {
                    player.TRounds++;
                }
                else
                {
                    player.CTRounds++;
                }

                var round = GetRoundPlayer(participant.SteamID);
                if (round.Kills == 5)
                {
                    player.Aces++;
                }
            }
            _round++;
            Log($"New round: {_round}");        
        }

        private void Parser_RoundEnd(object sender, RoundEndedEventArgs e)
        {
        } 
    }
}
