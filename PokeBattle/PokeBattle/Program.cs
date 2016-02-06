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
            players[0].WritePokemon(players[1].PokeTeam[0]);
            //players[1].WritePokemon(players[0].PokeTeam[0]);
            Battle battle = new Battle(players[0].PokeTeam[0], players[1].PokeTeam[0]);
            byte[][] m = new byte[2][2];
            byte[] m0 = players[0].ReadMove();
            byte[] m1 = players[1].ReadMove();
            battle.ExecuteMoves(m0[0] == 0 ? (int?)m0[1] : null, m1[0] == 0 ? (int?)m1[1] : null);
            
            Console.ReadKey();
        }
    }
}
