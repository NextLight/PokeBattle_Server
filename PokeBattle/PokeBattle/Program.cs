using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PokeBattle
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Waiting for 2 players.");
            var players = new PlayersCollection(new Player(), new Player());
            players.Connect();
            players.SendPokeTeam();
            players.SendOpponent(players[1]);
            players.SendOpponent(players[0]);
            Battle battle = new Battle(players[0], players[1]);

            bool exit = false;
            while (!exit)
            {
                players.SendBeginTurn();
                int i = 0;
                foreach (var r in players.Read())
                {
                    if (r.Type == ReadType.Switch)
                    {
                        players[i].SelectedPokemonIdx = r.Value;
                        players.SendOpponent(players[i]);
                    }
                    else
                    {
                        battle.StoreMove(i, r.Value);
                    }
                    ++i;
                }
                foreach (var t in battle.ExecuteMoves())
                    players.SendText($"{players[t.Item1].SelectedPokemon.Name} used {players[t.Item1].SelectedPokemon.Moves[t.Item2].Name}\n");

                players.SendInBattle();
                players.SendInBattleOpponent(players[1]);
                players.SendInBattleOpponent(players[0]);

                exit = players.Any(p => p.Lost);
                if (!exit)
                {
                    foreach (Player p in players)
                        if (p.SelectedPokemon.Fainted)
                            players.PlayerFainted(p);
                }
            }

            players.Close();
            Console.WriteLine("Game over. Player " + (players[0].Lost ? 2 : 1) + " won.");
            Console.ReadKey();
        }
    }

    class PlayersCollection : IEnumerable<Player>
    {
        Player[] _p;

        public PlayersCollection(params Player[] players)
        {
            _p = players;
        }

        public Player this[int i] => _p[i];

        public int Length => _p.Length;

        public void Connect() => Execute();

        public void Close() => Execute();

        public void SendBeginTurn() => Execute();

        public void SendText(string text) => Execute(new[] { text });

        public void SendPokeTeam() => Execute();

        public void SendOpponent(Player opp) => Execute(new[] { opp.SelectedPokemon }, opp);

        public void SendInBattle() => Execute();

        public void SendInBattleOpponent(Player opp) => Execute(new[] { opp.SelectedPokemon.InBattle }, opp);

        public void PlayerFainted(Player p)
        {
            p.SendUserFainted();
            var s = p.ReadSwitch();
            if (s.Type != ReadType.Switch)
                throw new Exception();
            p.SelectedPokemonIdx = s.Value;
            Execute(null, p, "SendOpponentFainted");
            SendOpponent(p);
            foreach (var r in ReadSwitch(p))
                r.Key.SelectedPokemonIdx = r.Value;
        }

        public IEnumerable<ReadReturn> Read() => Execute().Cast<ReadReturn>();

        public IEnumerable<KeyValuePair<Player, byte>> ReadSwitch(Player f)
        {
            foreach (Player p in Filter(f))
            {
                var r = p.ReadSwitch();
                if (r.Type == ReadType.Switch)
                    yield return new KeyValuePair<Player, byte>(p, r.Value);
            }
        }


        private IEnumerable<object> Execute(object[] args = null, Player f = null, [CallerMemberName]string name = "")
        {
            MethodInfo method = typeof(Player).GetMethod(name);
            foreach (Player p in Filter(f))
                yield return method.Invoke(p, args);
        }


        public IEnumerable<Player> Filter(Player p) =>
            p == null ? _p : _p.Where(pl => pl != p);

        public IEnumerator<Player> GetEnumerator() => ((IEnumerable<Player>)_p).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
