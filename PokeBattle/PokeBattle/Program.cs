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
            Console.ReadKey();
        }
    }
}
