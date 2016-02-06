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
            //players[1].Connect();
            players[0].WritePokeTeam();
            //players[1].WritePokeTeam();
            players[0].WritePokemon(players[1].SelectedPokemon);
            //players[1].WritePokemon(players[0].SelectedPokemon);
            Battle battle = new Battle(players[0].SelectedPokemon, players[1].SelectedPokemon);
            byte[][] m = new byte[2][];
            m[0] = players[0].ReadMove();
            //m[1] = players[1].ReadMove();
            m[1] = new byte[] { 0, 1 }; // tmp debug
            battle.ExecuteMoves(m[0][0] == 0 ? (int?)m[0][1] : null, m[1][0] == 0 ? (int?)m[1][1] : null);
            players[0].WriteInBattleStatus();
            //players[1].WriteInBattleStatus();
            players[0].WriteInBattleStatus(players[1].SelectedPokemon.InBattle);
            //players[1].WriteInBattleStatus(players[0].SelectedPokemon.InBattle);
            Console.ReadKey();
        }
    }
}
