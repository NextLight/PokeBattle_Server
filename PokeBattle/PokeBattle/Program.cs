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
            players[0].WritePokemon(players[1].SelectedPokemon);
#if !DEBUG
            players[1].WritePokemon(players[0].SelectedPokemon);
#endif
            Battle battle = new Battle(players[0].SelectedPokemon, players[1].SelectedPokemon);
            while (players.All(pl => pl.PokeTeam.Any(p => p.InBattle.Hp > 0)))
            {
                byte[][] m = new byte[2][];
                m[0] = players[0].ReadMove();
#if !DEBUG
                m[1] = players[1].ReadMove();
#else
                m[1] = new byte[] { 0, 1 };
#endif
                // check for active pokemon changes
                for (int i = 0; i < m.Length; i++)
                    if (m[i][0] == 1)
                    {
                        players[i].SelectedPokemonIdx = m[i][1];
                        battle.ChangePokemon(i, players[i].SelectedPokemon);
                    }

                battle.ExecuteMoves(m[0][0] == 0 ? (int?)m[0][1] : null, m[1][0] == 0 ? (int?)m[1][1] : null);
                players[0].WriteInBattleStatus();
#if !DEBUG
                players[1].WriteInBattleStatus();
#endif
                players[0].WriteInBattleStatus(players[1].SelectedPokemon.InBattle);
#if !DEBUG
                players[1].WriteInBattleStatus(players[0].SelectedPokemon.InBattle);
#endif
            }
            players[0].Close();
#if !DEBUG
            player[1].Close();
#endif
            Console.WriteLine("Game over. Player " + (players[0].PokeTeam.Any(p => p.InBattle.Hp > 0) ? 1 : 2) + " won.");
            Console.ReadKey();
        }
    }
}
