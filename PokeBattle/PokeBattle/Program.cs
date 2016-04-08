using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeBattle
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Waiting for 2 players.");
            Player[] players = new Player[2] { new Player(), new Player() };
            players[0].Connect();
#if !DEBUG
            players[1].Connect();
#endif
            players[0].WritePokeTeam();
#if !DEBUG
            players[1].WritePokeTeam();
#endif
            players[0].WriteOpponent(players[1].SelectedPokemon);
#if !DEBUG
            players[1].WriteOpponent(players[0].SelectedPokemon);
#endif
            Battle battle = new Battle(players[0].SelectedPokemon, players[1].SelectedPokemon);

            while (players.All(pl => pl.PokeTeam.Any(p => p.InBattle.Stats.Hp > 0)))
            {
                players[0].WriteBeginTurn();
#if !DEBUG
                players[1].WriteBeginTurn();
#endif

                byte[][] m = new byte[2][];
                m[0] = players[0].ReadMove();
#if !DEBUG
                m[1] = players[1].ReadMove();
#else
                m[1] = new byte[] { 0, 1 };
#endif
                for (int i = 0; i < m.Length; i++)
                {
                    if (m[i][0] == 1)
                    {
                        players[i].SelectedPokemonIdx = m[i][1];
                        battle.ChangePokemon(i, players[i].SelectedPokemon);
                    }
                    else
                    {
                        battle.StoreMove(i, m[i][1]);
                    }
                }
                foreach (var t in battle.ExecuteMoves())
                    players[t.Item1].WriteText(players[t.Item1].SelectedPokemon.Name + " used " + players[t.Item1].SelectedPokemon.Moves[t.Item2].Name);

                players[0].WriteInBattle();
#if !DEBUG
                players[1].WriteInBattle();
#endif
                players[0].WriteInBattleOpponent(players[1].SelectedPokemon.InBattle);
#if !DEBUG
                players[1].WriteInBattleOpponent(players[0].SelectedPokemon.InBattle);
#endif
            }

            players[0].Close();
#if !DEBUG
            player[1].Close();
#endif
            Console.WriteLine("Game over. Player " + (players[0].PokeTeam.Any(p => p.InBattle.Stats.Hp > 0) ? 1 : 2) + " won.");
            Console.ReadKey();
        }
    }
}
