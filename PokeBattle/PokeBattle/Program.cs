using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PokeBattle
{
    class Program
    {
        static void Main(string[] args)
        {
            var matches = new List<Match>();
            while (true)
            {
                matches.RemoveAll(m => m.Ended);
                if (matches.All(m => m.IsWorking))
                    matches.Add(new Match());
                Thread.Sleep(200);
            }
        }
    }

    class Match
    {
        static int _id = 0;
        PlayerCollection _players;
        Battle _battle;
        public Match()
        {
            Id = _id++;
            _players = new PlayerCollection(new Player(), new Player());
            _battle = new Battle(_players[0], _players[1]);
            Start();
        }

        public int Id { get; }

        public bool Ended { get; private set; }

        public bool IsWorking => _players.All(p => p.IsConnected);

        public async Task Start()
        {
            await Task.Run(async () =>
            {
                try
                {
                    Debug("Waiting for 2 players.");
                    _players.Connect();
                    _players.SendPokeTeam();
                    _players.SendOpponent(_players[1]);
                    _players.SendOpponent(_players[0]);

                    bool exit = false;
                    while (!exit)
                    {
                        _players.SendBeginTurn();
                        int i = 0;
                        foreach (var r in await _players.Read())
                        {
                            if (r.Type == ReadType.Switch)
                            {
                                _players[i].SelectedPokemonIdx = r.Value;
                                _players.SendOpponent(_players[i]);
                            }
                            else
                            {
                                _battle.StoreMove(i, r.Value);
                            }
                            ++i;
                        }
                        foreach (var t in _battle.ExecuteMoves())
                            _players.SendText($"{_players[t.Item1].SelectedPokemon.Name} used {_players[t.Item1].SelectedPokemon.Moves[t.Item2].Name}\n");

                        _players.SendInBattle();
                        _players.SendInBattleOpponent(_players[1]);
                        _players.SendInBattleOpponent(_players[0]);

                        exit = _players.Any(p => p.Lost);
                        if (!exit)
                        {
                            foreach (Player p in _players)
                                if (p.SelectedPokemon.Fainted)
                                    await _players.PlayerFainted(p);
                        }
                    }

                    Debug("Game over. Player " + (_players[0].Lost ? 2 : 1) + " won.");
                }
                finally
                {
                    Debug("Someone disconnected.");
                    _players.Close();
                    Ended = true;
                }
            });
        }

        private void Debug(string s) =>
            Console.WriteLine($"{Id}: {s}");
    }
}
