using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PokeBattle
{
    class Program
    {
        static void Main(string[] args)
        {
            var match = new Match();
            match.Start();
            while (true)
                System.Threading.Thread.Sleep(200);
        }
    }

    class Match
    {
        PlayerCollection _players;
        Battle _battle;

        public Match()
        {
            _players = new PlayerCollection(new Player(), new Player());
            _battle = new Battle(_players[0], _players[1]);
            Console.WriteLine("Waiting for 2 players.");
        }

        public bool Ended { get; private set; }

        public async Task Start()
        {
            await _players.Connect();
            Loop();
        }

        int _loopId = -1;
        private async void Loop()
        {
            int lId = ++_loopId;
#if DEBUG
            Console.WriteLine(lId);
#endif
            try
            {
                while (!Ended && lId == _loopId)
                {
                    _players.SendBeginTurn();
                    int i = 0;
                    foreach (var a in _players.Read())
                    {
                        var r = await a;
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

                    Ended = _players.Any(p => p.Lost);
                    if (!Ended)
                    {
                        foreach (Player p in _players)
                            if (p.SelectedPokemon.Fainted)
                                await _players.PlayerFainted(p);
                    }
                }
                if (lId == _loopId)
                {
                    _players.Close();
                    Console.WriteLine("Game over. Player " + (_players[0].Lost ? 2 : 1) + " won.");
                }
            }
            catch (IOException)
            {
                Console.WriteLine("lala");
                //_players.CancelRead();
                Loop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
